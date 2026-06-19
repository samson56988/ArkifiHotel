using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Infrastructure.Options;
using Admin.Infrastructure.Payments;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class PublicRestaurantOrderService : IPublicRestaurantOrderService
{
    private const string DefaultCurrency = "NGN";

    private readonly AdminDbContext _db;
    private readonly IBusinessPaymentConfigurationService _paymentConfig;
    private readonly PaymentGatewayRouter _gatewayRouter;
    private readonly CustomerAppOptions _customerApp;

    public PublicRestaurantOrderService(
        AdminDbContext db,
        IBusinessPaymentConfigurationService paymentConfig,
        PaymentGatewayRouter gatewayRouter,
        IOptions<CustomerAppOptions> customerApp)
    {
        _db = db;
        _paymentConfig = paymentConfig;
        _gatewayRouter = gatewayRouter;
        _customerApp = customerApp.Value;
    }

    public async Task<(GuestRestaurantOrderCheckoutDto? Data, PublicRestaurantOrderError? Error, string? Message)> CreateCheckoutAsync(
        string slug,
        GuestCreateRestaurantOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var business = await ResolveBusinessAsync(slug, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return (null, PublicRestaurantOrderError.NotFound, "Storefront not found.");
        }

        if (!ValidateRequest(request, out var guestType, out var roomNumber, out var guestPhone, out var message))
        {
            return (null, PublicRestaurantOrderError.InvalidRequest, message);
        }

        var menuEnabled = await _db.RestaurantMenuSettings
            .AsNoTracking()
            .AnyAsync(s => s.BusinessRegistrationId == business.Id && s.Enabled, cancellationToken)
            .ConfigureAwait(false);

        if (!menuEnabled)
        {
            return (null, PublicRestaurantOrderError.InvalidRequest, "Restaurant ordering is not available.");
        }

        if (request.LocationId == Guid.Empty
            || !await _db.BusinessLocations
                .AsNoTracking()
                .AnyAsync(l => l.Id == request.LocationId && l.BusinessRegistrationId == business.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            return (null, PublicRestaurantOrderError.InvalidRequest, "Select a valid branch.");
        }

        var mergedLines = MergeLines(request.Items);
        if (mergedLines.Count == 0)
        {
            return (null, PublicRestaurantOrderError.InvalidRequest, "Add at least one menu item.");
        }

        var menuItemIds = mergedLines.Keys.ToList();
        var menuItems = await _db.RestaurantMenuItems
            .AsNoTracking()
            .Include(i => i.Category)
            .Where(i => menuItemIds.Contains(i.Id)
                        && i.Category.BusinessRegistrationId == business.Id
                        && !i.IsArchived
                        && i.IsAvailable
                        && !i.Category.IsArchived)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (menuItems.Count != menuItemIds.Count)
        {
            return (null, PublicRestaurantOrderError.ItemUnavailable, "One or more items are no longer available.");
        }

        var orderLines = new List<RestaurantOrderLine>();
        decimal total = 0;

        foreach (var (menuItemId, quantity) in mergedLines)
        {
            var item = menuItems.First(i => i.Id == menuItemId);
            var lineTotal = decimal.Round(item.Price * quantity, 2, MidpointRounding.AwayFromZero);
            total += lineTotal;
            orderLines.Add(
                new RestaurantOrderLine
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = item.Id,
                    ItemName = item.Name,
                    UnitPrice = item.Price,
                    Quantity = quantity,
                    LineTotal = lineTotal,
                });
        }

        total = decimal.Round(total, 2, MidpointRounding.AwayFromZero);
        if (total <= 0)
        {
            return (null, PublicRestaurantOrderError.InvalidRequest, "Order total must be greater than zero.");
        }

        var config = await _paymentConfig.GetAsync(business.Id, cancellationToken).ConfigureAwait(false);
        if (config is null || !config.IsConfigured || config.Provider is "None")
        {
            return (null, PublicRestaurantOrderError.PaymentNotConfigured, "Online payment is not available for this hotel.");
        }

        var credentials = await _paymentConfig.GetDecryptedCredentialsAsync(business.Id, cancellationToken).ConfigureAwait(false);
        if (credentials is null || !Enum.TryParse<PaymentGatewayProvider>(credentials.Provider, true, out var provider))
        {
            return (null, PublicRestaurantOrderError.PaymentNotConfigured, "Online payment is not available for this hotel.");
        }

        var orderNumber = await GenerateUniqueOrderNumberAsync(business.Id, business.BusinessName, cancellationToken)
            .ConfigureAwait(false);
        var paymentReference = await GenerateUniquePaymentReferenceAsync(business.Id, business.BusinessName, cancellationToken)
            .ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var orderId = Guid.NewGuid();

        var order = new RestaurantOrder
        {
            Id = orderId,
            BusinessRegistrationId = business.Id,
            LocationId = request.LocationId,
            GuestType = guestType,
            RoomNumber = roomNumber,
            GuestPhone = guestPhone,
            OrderNumber = orderNumber,
            Status = RestaurantOrderStatus.Pending,
            TotalAmount = total,
            Currency = DefaultCurrency,
            CreatedAt = now,
        };

        foreach (var line in orderLines)
        {
            line.RestaurantOrderId = orderId;
        }

        _db.RestaurantOrders.Add(order);
        _db.RestaurantOrderLines.AddRange(orderLines);
        _db.RestaurantOrderPayments.Add(
            new RestaurantOrderPayment
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = business.Id,
                RestaurantOrderId = orderId,
                Amount = total,
                Currency = DefaultCurrency,
                Status = BookingPaymentStatus.Pending,
                Method = BookingPaymentMethod.Gateway,
                Gateway = provider,
                ExternalReference = paymentReference,
                CreatedAt = now,
            });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var redirectUrl = BuildRedirectUrl(business.Slug!, request.LocationId, paymentReference);
        var customerEmail = BuildGuestEmail(guestPhone);

        var initContext = new PaymentInitializeContext
        {
            Provider = provider,
            SecretKey = credentials.SecretKey!,
            ApiKey = credentials.ApiKey,
            ContractCode = credentials.ContractCode,
            Reference = paymentReference,
            Amount = total,
            Currency = DefaultCurrency,
            CustomerEmail = customerEmail,
            CustomerName = guestType == RestaurantGuestType.RoomGuest ? $"Room {roomNumber}" : "Restaurant guest",
            CustomerPhone = guestPhone,
            RedirectUrl = redirectUrl,
            Description = $"Restaurant order at {business.BusinessName}",
        };

        var handler = _gatewayRouter.Get(provider);
        var initResult = await handler.InitializeAsync(initContext, cancellationToken).ConfigureAwait(false);
        if (!initResult.Success || string.IsNullOrWhiteSpace(initResult.PaymentUrl))
        {
            order.Status = RestaurantOrderStatus.Cancelled;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            var payment = await _db.RestaurantOrderPayments
                .FirstAsync(p => p.RestaurantOrderId == orderId, cancellationToken)
                .ConfigureAwait(false);
            payment.Status = BookingPaymentStatus.Failed;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            payment.Notes = initResult.Message;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return (null, PublicRestaurantOrderError.PaymentInitFailed, initResult.Message ?? "Could not start payment.");
        }

        return (
            new GuestRestaurantOrderCheckoutDto
            {
                OrderId = orderId,
                OrderNumber = orderNumber,
                PaymentReference = paymentReference,
                PaymentUrl = initResult.PaymentUrl,
                Provider = provider.ToString(),
                Amount = total,
                Currency = DefaultCurrency,
            },
            null,
            null);
    }

    public async Task<GuestRestaurantOrderVerifyResultDto?> VerifyPaymentAsync(
        string slug,
        string paymentReference,
        CancellationToken cancellationToken = default)
    {
        var business = await ResolveBusinessAsync(slug, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return null;
        }

        var reference = (paymentReference ?? string.Empty).Trim();
        if (reference.Length < 4)
        {
            return new GuestRestaurantOrderVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = "Invalid",
                Message = "Payment reference is missing.",
            };
        }

        var payment = await _db.RestaurantOrderPayments
            .Include(p => p.RestaurantOrder)
            .ThenInclude(o => o.Lines)
            .FirstOrDefaultAsync(
                p => p.BusinessRegistrationId == business.Id && p.ExternalReference == reference,
                cancellationToken)
            .ConfigureAwait(false);

        if (payment is null)
        {
            return new GuestRestaurantOrderVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = "NotFound",
                Message = "No order matches this payment reference.",
            };
        }

        var order = payment.RestaurantOrder;

        if (payment.Status == BookingPaymentStatus.Completed && order.Status == RestaurantOrderStatus.Paid)
        {
            return BuildVerifySuccess(order, business.BusinessName);
        }

        if (payment.Status is BookingPaymentStatus.Failed or BookingPaymentStatus.Cancelled)
        {
            return new GuestRestaurantOrderVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = payment.Status.ToString(),
                Message = "This payment was not completed.",
            };
        }

        var credentials = await _paymentConfig.GetDecryptedCredentialsAsync(business.Id, cancellationToken).ConfigureAwait(false);
        if (credentials is null || payment.Gateway == PaymentGatewayProvider.None)
        {
            return new GuestRestaurantOrderVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = "Error",
                Message = "Payment gateway is not configured.",
            };
        }

        var handler = _gatewayRouter.Get(payment.Gateway);
        var verifyResult = await handler.VerifyAsync(
            new PaymentVerifyContext
            {
                Provider = payment.Gateway,
                SecretKey = credentials.SecretKey!,
                ApiKey = credentials.ApiKey,
                ContractCode = credentials.ContractCode,
                Reference = reference,
                ExpectedAmount = payment.Amount,
                Currency = payment.Currency,
            },
            cancellationToken).ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;

        if (!verifyResult.Success)
        {
            payment.Status = BookingPaymentStatus.Failed;
            payment.UpdatedAt = now;
            order.Status = RestaurantOrderStatus.Cancelled;
            order.UpdatedAt = now;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new GuestRestaurantOrderVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = "Failed",
                Message = verifyResult.Message ?? "Payment could not be verified.",
            };
        }

        payment.Status = BookingPaymentStatus.Completed;
        payment.UpdatedAt = now;
        if (!string.IsNullOrWhiteSpace(verifyResult.ProviderReference))
        {
            payment.Notes = $"Provider ref: {verifyResult.ProviderReference}";
        }

        order.Status = RestaurantOrderStatus.Paid;
        order.UpdatedAt = now;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BuildVerifySuccess(order, business.BusinessName);
    }

    private static Dictionary<Guid, int> MergeLines(IReadOnlyList<GuestCreateRestaurantOrderLineRequest> items)
    {
        var merged = new Dictionary<Guid, int>();
        foreach (var item in items ?? Array.Empty<GuestCreateRestaurantOrderLineRequest>())
        {
            if (item.MenuItemId == Guid.Empty || item.Quantity < 1 || item.Quantity > 99)
            {
                continue;
            }

            merged[item.MenuItemId] = merged.TryGetValue(item.MenuItemId, out var existing)
                ? existing + item.Quantity
                : item.Quantity;
        }

        return merged;
    }

    private static bool ValidateRequest(
        GuestCreateRestaurantOrderRequest request,
        out RestaurantGuestType guestType,
        out string? roomNumber,
        out string guestPhone,
        out string message)
    {
        guestType = RestaurantGuestType.InRestaurant;
        roomNumber = null;
        guestPhone = string.Empty;
        message = string.Empty;

        var typeRaw = (request.GuestType ?? string.Empty).Trim().ToLowerInvariant();
        guestType = typeRaw switch
        {
            "roomguest" or "room" or "room-guest" => RestaurantGuestType.RoomGuest,
            _ => RestaurantGuestType.InRestaurant,
        };

        if (guestType == RestaurantGuestType.RoomGuest)
        {
            var rn = (request.RoomNumber ?? string.Empty).Trim();
            if (rn.Length < 1 || rn.Length > 40)
            {
                message = "Enter your room number.";
                return false;
            }

            roomNumber = rn;
        }

        if (string.IsNullOrWhiteSpace(request.GuestPhone))
        {
            message = "Enter a phone number in international format (+234…).";
            return false;
        }

        if (!TryNormalizeGuestPhone(request.GuestPhone, out var p, out message))
        {
            return false;
        }

        guestPhone = p;
        return true;
    }

    private static bool TryNormalizeGuestPhone(string raw, out string normalized, out string message)
    {
        normalized = string.Empty;
        message = string.Empty;

        var p = raw.Trim().Replace(" ", string.Empty);
        if (!p.StartsWith("+", StringComparison.Ordinal))
        {
            if (p.StartsWith("234", StringComparison.Ordinal) && p.Length >= 12)
            {
                p = "+" + p;
            }
            else if (p.StartsWith('0') && p.Length >= 10)
            {
                p = "+234" + p[1..];
            }
        }

        if (p.Length < 8 || p.Length > 40 || !p.StartsWith("+", StringComparison.Ordinal))
        {
            message = "Enter a phone number in international format (+234…).";
            return false;
        }

        for (var i = 1; i < p.Length; i++)
        {
            if (!char.IsDigit(p[i]))
            {
                message = "Enter a phone number in international format (+234…).";
                return false;
            }
        }

        normalized = p;
        return true;
    }

    private static string BuildGuestEmail(string guestPhone)
    {
        var digits = new string(guestPhone.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            return "guest@orders.arkifihotel.com";
        }

        // Paystack and other gateways reject reserved TLDs like .local — use a valid public format.
        return $"order.{digits}@orders.arkifihotel.com";
    }

    private string BuildRedirectUrl(string slug, Guid locationId, string paymentReference)
    {
        var baseUrl = (_customerApp.BaseUrl ?? "http://localhost:4201").TrimEnd('/');
        var loc = locationId == Guid.Empty ? "default" : locationId.ToString();
        var query = Uri.EscapeDataString(paymentReference);
        return $"{baseUrl}/{slug}/l/{loc}/restaurant/payment/verify?reference={query}";
    }

    private static GuestRestaurantOrderVerifyResultDto BuildVerifySuccess(RestaurantOrder order, string propertyName) =>
        new()
        {
            PaymentSuccessful = true,
            Status = "Paid",
            Message = "Payment verified. Your order has been placed.",
            Order = MapOrderLookup(order, propertyName),
        };

    private static GuestRestaurantOrderLookupDto MapOrderLookup(RestaurantOrder order, string propertyName) =>
        new()
        {
            OrderNumber = order.OrderNumber,
            PropertyName = propertyName,
            GuestType = order.GuestType == RestaurantGuestType.RoomGuest ? "roomGuest" : "inRestaurant",
            RoomNumber = order.RoomNumber,
            GuestPhone = order.GuestPhone,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Lines = order.Lines
                .OrderBy(l => l.ItemName)
                .Select(l => new GuestRestaurantOrderLineDto
                {
                    ItemName = l.ItemName,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal,
                })
                .ToList(),
        };

    private async Task<BusinessRegistration?> ResolveBusinessAsync(string slug, CancellationToken cancellationToken)
    {
        var normalized = BusinessSlugHelper.Normalize(slug);
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        return await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug == normalized, cancellationToken)
            .ConfigureAwait(false);
    }

    private Task<string> GenerateUniqueOrderNumberAsync(
        Guid businessId,
        string businessName,
        CancellationToken cancellationToken) =>
        BusinessReferenceCodeGenerator.GenerateUniqueAsync(
            businessName,
            async (code, ct) => await _db.RestaurantOrders
                .AnyAsync(o => o.BusinessRegistrationId == businessId && o.OrderNumber == code, ct)
                .ConfigureAwait(false),
            cancellationToken);

    private async Task<string> GenerateUniquePaymentReferenceAsync(
        Guid businessId,
        string businessName,
        CancellationToken cancellationToken)
    {
        var prefix = BusinessReferenceCodeGenerator.GetPrefixFromBusinessName(businessName);

        for (var attempt = 0; attempt < 32; attempt++)
        {
            var reference = $"{prefix}-{Random.Shared.Next(10000000, 99999999)}-FOOD";
            var existsInBookings = await _db.BookingPayments
                .AnyAsync(p => p.BusinessRegistrationId == businessId && p.ExternalReference == reference, cancellationToken)
                .ConfigureAwait(false);
            var existsInOrders = await _db.RestaurantOrderPayments
                .AnyAsync(p => p.BusinessRegistrationId == businessId && p.ExternalReference == reference, cancellationToken)
                .ConfigureAwait(false);

            if (!existsInBookings && !existsInOrders)
            {
                return reference;
            }
        }

        return $"{prefix}-{Guid.NewGuid():N}"[..24];
    }
}
