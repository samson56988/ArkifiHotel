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

public sealed class PublicGuestBookingService : IPublicGuestBookingService
{
    private const string DefaultCurrency = "NGN";

    private readonly AdminDbContext _db;
    private readonly IBusinessBookingService _bookings;
    private readonly IBusinessPaymentConfigurationService _paymentConfig;
    private readonly PaymentGatewayRouter _gatewayRouter;
    private readonly CustomerAppOptions _customerApp;

    public PublicGuestBookingService(
        AdminDbContext db,
        IBusinessBookingService bookings,
        IBusinessPaymentConfigurationService paymentConfig,
        PaymentGatewayRouter gatewayRouter,
        IOptions<CustomerAppOptions> customerApp)
    {
        _db = db;
        _bookings = bookings;
        _paymentConfig = paymentConfig;
        _gatewayRouter = gatewayRouter;
        _customerApp = customerApp.Value;
    }

    public async Task<(GuestBookingCheckoutDto? Data, PublicGuestBookingError? Error, string? Message)> CreateCheckoutAsync(
        string slug,
        GuestCreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var business = await ResolveBusinessAsync(slug, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return (null, PublicGuestBookingError.NotFound, "Storefront not found.");
        }

        if (!ValidateGuestAndDates(request, out var guestName, out var guestEmail, out var guestPhone, out var checkIn, out var checkOut, out var nights))
        {
            return (null, PublicGuestBookingError.InvalidRequest, "Check guest details, phone (+234…), and stay dates.");
        }

        var config = await _paymentConfig.GetAsync(business.Id, cancellationToken).ConfigureAwait(false);
        if (config is null || !config.IsConfigured || config.Provider is "None")
        {
            return (null, PublicGuestBookingError.PaymentNotConfigured, "Online payment is not available for this hotel.");
        }

        var credentials = await _paymentConfig.GetDecryptedCredentialsAsync(business.Id, cancellationToken).ConfigureAwait(false);
        if (credentials is null || !Enum.TryParse<PaymentGatewayProvider>(credentials.Provider, true, out var provider))
        {
            return (null, PublicGuestBookingError.PaymentNotConfigured, "Online payment is not available for this hotel.");
        }

        var room = await _db.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.BusinessRegistrationId == business.Id, cancellationToken)
            .ConfigureAwait(false);

        if (room is null || room.IsArchived)
        {
            return (null, PublicGuestBookingError.InvalidRequest, "Selected room is not available.");
        }

        if (request.LocationId == Guid.Empty
            || !room.LocationId.HasValue
            || room.LocationId.Value != request.LocationId)
        {
            return (null, PublicGuestBookingError.InvalidRequest, "Select a valid branch for this room.");
        }

        if (!await _db.BusinessLocations
                .AsNoTracking()
                .AnyAsync(l => l.Id == request.LocationId && l.BusinessRegistrationId == business.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            return (null, PublicGuestBookingError.InvalidRequest, "Branch not found.");
        }

        if (await HasConfirmedDuplicateAsync(business.Id, request.RoomId, guestEmail, checkIn, checkOut, cancellationToken)
                .ConfigureAwait(false))
        {
            return (null, PublicGuestBookingError.InvalidRequest, "You already have a confirmed booking for this room and stay.");
        }

        var total = decimal.Round(room.BasePricePerNight * nights, 2, MidpointRounding.AwayFromZero);

        var reusable = await TryGetReusablePendingCheckoutAsync(
                business.Id,
                request.RoomId,
                guestEmail,
                checkIn,
                checkOut,
                cancellationToken)
            .ConfigureAwait(false);

        if (reusable is not null)
        {
            return await ResumePendingCheckoutAsync(
                    business,
                    reusable.Value.Booking,
                    reusable.Value.Payment,
                    provider,
                    credentials,
                    request.LocationId,
                    guestName,
                    guestEmail,
                    guestPhone!,
                    total,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (await IsFullyBookedAsync(
                request.RoomId,
                room.Quantity,
                checkIn,
                checkOut,
                excludeBookingId: null,
                cancellationToken)
            .ConfigureAwait(false))
        {
            return (null, PublicGuestBookingError.RoomUnavailable, "This room is not available for the selected dates.");
        }

        await CancelSupersededPendingBookingsAsync(
                business.Id,
                request.RoomId,
                guestEmail,
                checkIn,
                checkOut,
                excludeBookingId: null,
                cancellationToken)
            .ConfigureAwait(false);

        var confirmationCode = await GenerateUniqueConfirmationCodeAsync(business.Id, business.BusinessName, cancellationToken)
            .ConfigureAwait(false);
        var paymentReference = await GenerateUniquePaymentReferenceAsync(business.Id, business.BusinessName, cancellationToken)
            .ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var bookingId = Guid.NewGuid();

        var redirectUrl = BuildRedirectUrl(business.Slug!, request.LocationId, paymentReference);

        var booking = new Booking
        {
            Id = bookingId,
            BusinessRegistrationId = business.Id,
            RoomId = request.RoomId,
            LocationId = room.LocationId,
            GuestName = guestName,
            GuestEmail = guestEmail,
            GuestPhone = guestPhone,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalAmount = total,
            Currency = DefaultCurrency,
            Status = BookingStatus.Pending,
            ConfirmationCode = confirmationCode,
            CreatedAt = now,
        };

        _db.Bookings.Add(booking);
        _db.BookingPayments.Add(
            new BookingPayment
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = business.Id,
                BookingId = bookingId,
                Amount = total,
                Currency = DefaultCurrency,
                Status = BookingPaymentStatus.Pending,
                Method = BookingPaymentMethod.Gateway,
                Gateway = provider,
                ExternalReference = paymentReference,
                CreatedAt = now,
            });

        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            var raced = await TryGetReusablePendingCheckoutAsync(
                    business.Id,
                    request.RoomId,
                    guestEmail,
                    checkIn,
                    checkOut,
                    cancellationToken)
                .ConfigureAwait(false);

            if (raced is not null)
            {
                return await ResumePendingCheckoutAsync(
                        business,
                        raced.Value.Booking,
                        raced.Value.Payment,
                        provider,
                        credentials,
                        request.LocationId,
                        guestName,
                        guestEmail,
                        guestPhone!,
                        total,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            throw;
        }

        return await InitializeAndReturnCheckoutAsync(
                business,
                bookingId,
                paymentReference,
                provider,
                credentials,
                request.LocationId,
                guestName,
                guestEmail,
                guestPhone!,
                total,
                redirectUrl,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<(GuestBookingCheckoutDto? Data, PublicGuestBookingError? Error, string? Message)> ResumePendingCheckoutAsync(
        BusinessRegistration business,
        Booking booking,
        BookingPayment payment,
        PaymentGatewayProvider provider,
        PaymentGatewayCredentialsDto credentials,
        Guid locationId,
        string guestName,
        string guestEmail,
        string guestPhone,
        decimal total,
        CancellationToken cancellationToken)
    {
        if (await IsFullyBookedAsync(
                booking.RoomId,
                await GetRoomQuantityAsync(booking.RoomId, cancellationToken).ConfigureAwait(false),
                booking.CheckInDate,
                booking.CheckOutDate,
                excludeBookingId: booking.Id,
                cancellationToken)
            .ConfigureAwait(false))
        {
            return (null, PublicGuestBookingError.RoomUnavailable, "This room is not available for the selected dates.");
        }

        await CancelSupersededPendingBookingsAsync(
                business.Id,
                booking.RoomId,
                guestEmail,
                booking.CheckInDate,
                booking.CheckOutDate,
                excludeBookingId: booking.Id,
                cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        booking.GuestName = guestName;
        booking.GuestEmail = guestEmail;
        booking.GuestPhone = guestPhone;
        booking.TotalAmount = total;
        booking.UpdatedAt = now;
        payment.Amount = total;
        payment.Gateway = provider;
        payment.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var reference = payment.ExternalReference!;
        var redirectUrl = BuildRedirectUrl(business.Slug!, locationId, reference);

        return await InitializeAndReturnCheckoutAsync(
                business,
                booking.Id,
                reference,
                provider,
                credentials,
                locationId,
                guestName,
                guestEmail,
                guestPhone,
                total,
                redirectUrl,
                cancellationToken,
                booking)
            .ConfigureAwait(false);
    }

    private async Task<(GuestBookingCheckoutDto? Data, PublicGuestBookingError? Error, string? Message)> InitializeAndReturnCheckoutAsync(
        BusinessRegistration business,
        Guid bookingId,
        string paymentReference,
        PaymentGatewayProvider provider,
        PaymentGatewayCredentialsDto credentials,
        Guid locationId,
        string guestName,
        string guestEmail,
        string guestPhone,
        decimal total,
        string redirectUrl,
        CancellationToken cancellationToken,
        Booking? trackedBooking = null)
    {
        var initContext = new PaymentInitializeContext
        {
            Provider = provider,
            SecretKey = credentials.SecretKey!,
            ApiKey = credentials.ApiKey,
            ContractCode = credentials.ContractCode,
            Reference = paymentReference,
            Amount = total,
            Currency = DefaultCurrency,
            CustomerEmail = guestEmail,
            CustomerName = guestName,
            CustomerPhone = guestPhone,
            RedirectUrl = redirectUrl,
            Description = $"Booking at {business.BusinessName}",
        };

        var handler = _gatewayRouter.Get(provider);
        var initResult = await handler.InitializeAsync(initContext, cancellationToken).ConfigureAwait(false);
        if (!initResult.Success || string.IsNullOrWhiteSpace(initResult.PaymentUrl))
        {
            var booking = trackedBooking ?? await _db.Bookings
                .FirstAsync(b => b.Id == bookingId, cancellationToken)
                .ConfigureAwait(false);
            var failedPayment = await _db.BookingPayments
                .FirstAsync(p => p.BookingId == booking.Id, cancellationToken)
                .ConfigureAwait(false);

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTimeOffset.UtcNow;
            failedPayment.Status = BookingPaymentStatus.Failed;
            failedPayment.UpdatedAt = DateTimeOffset.UtcNow;
            failedPayment.Notes = initResult.Message;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return (null, PublicGuestBookingError.PaymentInitFailed, initResult.Message ?? "Could not start payment.");
        }

        return (
            new GuestBookingCheckoutDto
            {
                BookingId = bookingId,
                PaymentReference = paymentReference,
                PaymentUrl = initResult.PaymentUrl,
                Provider = provider.ToString(),
                Amount = total,
                Currency = DefaultCurrency,
            },
            null,
            null);
    }

    private async Task<(Booking Booking, BookingPayment Payment)?> TryGetReusablePendingCheckoutAsync(
        Guid businessId,
        Guid roomId,
        string guestEmail,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken)
    {
        var emailLower = guestEmail.ToLowerInvariant();

        var match = await (
                from b in _db.Bookings
                join p in _db.BookingPayments on b.Id equals p.BookingId
                where b.BusinessRegistrationId == businessId
                      && b.RoomId == roomId
                      && b.GuestEmail.ToLower() == emailLower
                      && b.CheckInDate == checkIn
                      && b.CheckOutDate == checkOut
                      && b.Status == BookingStatus.Pending
                      && p.Status == BookingPaymentStatus.Pending
                      && p.Method == BookingPaymentMethod.Gateway
                      && p.ExternalReference != null
                orderby b.CreatedAt descending
                select new { Booking = b, Payment = p })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (match is null)
        {
            return null;
        }

        return (match.Booking, match.Payment);
    }

    private async Task<bool> HasConfirmedDuplicateAsync(
        Guid businessId,
        Guid roomId,
        string guestEmail,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken)
    {
        var emailLower = guestEmail.ToLowerInvariant();

        return await _db.Bookings
            .AsNoTracking()
            .AnyAsync(
                b => b.BusinessRegistrationId == businessId
                     && b.RoomId == roomId
                     && b.GuestEmail.ToLower() == emailLower
                     && b.CheckInDate == checkIn
                     && b.CheckOutDate == checkOut
                     && b.Status == BookingStatus.Confirmed,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task CancelSupersededPendingBookingsAsync(
        Guid businessId,
        Guid roomId,
        string guestEmail,
        DateOnly checkIn,
        DateOnly checkOut,
        Guid? excludeBookingId,
        CancellationToken cancellationToken)
    {
        var emailLower = guestEmail.ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;

        var duplicates = await _db.Bookings
            .Where(b => b.BusinessRegistrationId == businessId
                        && b.RoomId == roomId
                        && b.GuestEmail.ToLower() == emailLower
                        && b.CheckInDate == checkIn
                        && b.CheckOutDate == checkOut
                        && b.Status == BookingStatus.Pending
                        && (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (duplicates.Count == 0)
        {
            return;
        }

        var duplicateIds = duplicates.Select(b => b.Id).ToList();
        var payments = await _db.BookingPayments
            .Where(p => duplicateIds.Contains(p.BookingId) && p.Status == BookingPaymentStatus.Pending)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var booking in duplicates)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = now;
        }

        foreach (var payment in payments)
        {
            payment.Status = BookingPaymentStatus.Cancelled;
            payment.UpdatedAt = now;
            payment.Notes = "Superseded by a newer checkout attempt.";
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<int> GetRoomQuantityAsync(Guid roomId, CancellationToken cancellationToken)
    {
        return await _db.Rooms
            .AsNoTracking()
            .Where(r => r.Id == roomId)
            .Select(r => r.Quantity)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<(IReadOnlyList<RoomAvailabilityDto>? Data, PublicGuestBookingError? Error, string? Message)>
        GetRoomAvailabilityAsync(
            string slug,
            Guid locationId,
            DateOnly checkIn,
            DateOnly checkOut,
            CancellationToken cancellationToken = default)
    {
        var business = await ResolveBusinessAsync(slug, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return (null, PublicGuestBookingError.NotFound, "Storefront not found.");
        }

        if (locationId == Guid.Empty)
        {
            return (null, PublicGuestBookingError.InvalidRequest, "Select a branch.");
        }

        if (checkOut <= checkIn)
        {
            return (null, PublicGuestBookingError.InvalidRequest, "Check-out must be after check-in.");
        }

        if (!await _db.BusinessLocations
                .AsNoTracking()
                .AnyAsync(l => l.Id == locationId && l.BusinessRegistrationId == business.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            return (null, PublicGuestBookingError.InvalidRequest, "Branch not found.");
        }

        var availability = await _bookings
            .GetAvailabilityAsync(business.Id, checkIn, checkOut, roomId: null, locationId, cancellationToken)
            .ConfigureAwait(false);

        return (availability, null, null);
    }

    public async Task<GuestPaymentVerifyResultDto?> VerifyPaymentAsync(
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
            return new GuestPaymentVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = "Invalid",
                Message = "Payment reference is missing.",
            };
        }

        var payment = await _db.BookingPayments
            .Include(p => p.Booking)
            .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(
                p => p.BusinessRegistrationId == business.Id && p.ExternalReference == reference,
                cancellationToken)
            .ConfigureAwait(false);

        if (payment is null)
        {
            return new GuestPaymentVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = "NotFound",
                Message = "No booking matches this payment reference.",
            };
        }

        var booking = payment.Booking;

        if (payment.Status == BookingPaymentStatus.Completed && booking.Status == BookingStatus.Confirmed)
        {
            return BuildVerifySuccess(booking, business.BusinessName);
        }

        if (payment.Status is BookingPaymentStatus.Failed or BookingPaymentStatus.Cancelled)
        {
            return new GuestPaymentVerifyResultDto
            {
                PaymentSuccessful = false,
                Status = payment.Status.ToString(),
                Message = "This payment was not completed.",
            };
        }

        var credentials = await _paymentConfig.GetDecryptedCredentialsAsync(business.Id, cancellationToken).ConfigureAwait(false);
        if (credentials is null || payment.Gateway == PaymentGatewayProvider.None)
        {
            return new GuestPaymentVerifyResultDto
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
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = now;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new GuestPaymentVerifyResultDto
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

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = now;

        await UpsertCustomerFromGuestAsync(
                business.Id,
                booking.GuestName,
                booking.GuestEmail,
                booking.GuestPhone ?? string.Empty,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BuildVerifySuccess(booking, business.BusinessName);
    }

    private string BuildRedirectUrl(string slug, Guid locationId, string paymentReference)
    {
        var baseUrl = (_customerApp.BaseUrl ?? "http://localhost:4201").TrimEnd('/');
        var loc = locationId == Guid.Empty ? "default" : locationId.ToString();
        var query = Uri.EscapeDataString(paymentReference);
        return $"{baseUrl}/{slug}/l/{loc}/booking/payment/verify?reference={query}";
    }

    private static GuestPaymentVerifyResultDto BuildVerifySuccess(Booking booking, string propertyName)
    {
        var nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber;
        return new GuestPaymentVerifyResultDto
        {
            PaymentSuccessful = true,
            Status = "Confirmed",
            Message = "Payment verified. Your booking is confirmed.",
            Booking = new GuestBookingLookupDto
            {
                PropertyName = propertyName,
                RoomName = booking.Room.Name,
                GuestName = booking.GuestName,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Nights = nights,
                Status = booking.Status.ToString(),
                TotalAmount = booking.TotalAmount,
                Currency = booking.Currency,
                ConfirmationCode = booking.ConfirmationCode,
            },
        };
    }

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
            .ConfigureAwait(false) is { } business
            && SubscriptionAccessHelper.IsStorefrontAccessible(business)
            ? business
            : null;
    }

    private async Task<bool> IsFullyBookedAsync(
        Guid roomId,
        int roomQuantity,
        DateOnly checkIn,
        DateOnly checkOut,
        Guid? excludeBookingId,
        CancellationToken cancellationToken)
    {
        var stays = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.RoomId == roomId
                        && b.Status != BookingStatus.Cancelled
                        && (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value))
            .Select(b => new { b.CheckInDate, b.CheckOutDate })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existing = stays.Select(b => (b.CheckInDate, b.CheckOutDate));
        return RoomBookingAvailability.WouldExceedCapacity(existing, checkIn, checkOut, roomQuantity);
    }

    private Task<string> GenerateUniqueConfirmationCodeAsync(
        Guid businessId,
        string businessName,
        CancellationToken cancellationToken) =>
        BusinessReferenceCodeGenerator.GenerateUniqueAsync(
            businessName,
            async (code, ct) => await _db.Bookings
                .AnyAsync(b => b.BusinessRegistrationId == businessId && b.ConfirmationCode == code, ct)
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
            var reference = $"{prefix}-{Random.Shared.Next(10000000, 99999999)}-PAY";
            if (!await _db.BookingPayments
                    .AnyAsync(p => p.BusinessRegistrationId == businessId && p.ExternalReference == reference, cancellationToken)
                    .ConfigureAwait(false))
            {
                return reference;
            }
        }

        return $"{prefix}-{Guid.NewGuid():N}"[..24];
    }

    private async Task UpsertCustomerFromGuestAsync(
        Guid businessId,
        string guestName,
        string guestEmail,
        string guestPhone,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var email = guestEmail.Trim();
        var emailLower = email.ToLowerInvariant();

        var existing = await _db.Customers
            .FirstOrDefaultAsync(
                c => c.BusinessRegistrationId == businessId && c.Email.ToLower() == emailLower,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.FullName = guestName;
            existing.Phone = guestPhone;
            existing.UpdatedAt = now;
            return;
        }

        _db.Customers.Add(
            new Customer
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = businessId,
                FullName = guestName,
                Email = email,
                Phone = guestPhone,
                CreatedAt = now,
            });
    }

    private static bool ValidateGuestAndDates(
        GuestCreateBookingRequest request,
        out string guestName,
        out string guestEmail,
        out string? guestPhone,
        out DateOnly checkIn,
        out DateOnly checkOut,
        out int nights)
    {
        guestName = string.Empty;
        guestEmail = string.Empty;
        guestPhone = null;
        checkIn = default;
        checkOut = default;
        nights = 0;

        var gn = (request.GuestName ?? string.Empty).Trim();
        if (gn.Length < 2 || gn.Length > 200)
        {
            return false;
        }

        var ge = (request.GuestEmail ?? string.Empty).Trim();
        if (ge.Length < 3 || ge.Length > 320 || ge.IndexOf('@', StringComparison.Ordinal) < 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.GuestPhone))
        {
            return false;
        }

        var p = request.GuestPhone.Trim().Replace(" ", string.Empty);
        if (p.Length < 8 || p.Length > 40 || !p.StartsWith("+", StringComparison.Ordinal))
        {
            return false;
        }

        for (var i = 1; i < p.Length; i++)
        {
            if (!char.IsDigit(p[i]))
            {
                return false;
            }
        }

        guestPhone = p;
        checkIn = request.CheckInDate;
        checkOut = request.CheckOutDate;
        if (checkOut <= checkIn)
        {
            return false;
        }

        nights = checkOut.DayNumber - checkIn.DayNumber;
        if (nights < 1 || nights > 366)
        {
            return false;
        }

        guestName = gn;
        guestEmail = ge.ToLowerInvariant();
        return true;
    }
}
