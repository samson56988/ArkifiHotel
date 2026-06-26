using Admin.Data;
using Admin.Data.Constants;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessBookingPaymentService : IBusinessBookingPaymentService
{
    private readonly AdminDbContext _db;
    private readonly IOrganizationUserContext _actor;
    private readonly IOrganizationAuditService _audit;
    private readonly ICustomerConfirmationEmailService _confirmationEmails;

    public BusinessBookingPaymentService(
        AdminDbContext db,
        IOrganizationUserContext actor,
        IOrganizationAuditService audit,
        ICustomerConfirmationEmailService confirmationEmails)
    {
        _db = db;
        _actor = actor;
        _audit = audit;
        _confirmationEmails = confirmationEmails;
    }

    public async Task<IReadOnlyList<BookingPaymentSummaryDto>> ListAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var query = _db.BookingPayments
            .AsNoTracking()
            .Include(p => p.Booking)
            .ThenInclude(b => b.Room)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Location)
            .Where(p => p.BusinessRegistrationId == businessId);

        query = OrganizationQueryScope.ApplyBookingPaymentScope(query, _actor);

        var rows = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(Map).ToList();
    }

    public async Task<BookingPaymentSummaryDto?> CreateAsync(
        Guid businessId,
        CreateBookingPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0 || request.Amount > 1_000_000_000)
        {
            return null;
        }

        var cur = (request.Currency ?? "NGN").Trim().ToUpperInvariant();
        if (cur.Length != 3)
        {
            return null;
        }

        if (!TryParseStatus(request.Status, out var status))
        {
            return null;
        }

        if (!TryParseMethod(request.Method, out var method))
        {
            return null;
        }

        if (!TryParseGateway(request.Gateway, out var gateway))
        {
            return null;
        }

        if (method != BookingPaymentMethod.Gateway)
        {
            gateway = PaymentGatewayProvider.None;
        }
        else if (gateway == PaymentGatewayProvider.None)
        {
            return null;
        }

        var booking = await _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (booking is null)
        {
            return null;
        }

        if (!OrganizationLocationHelper.CanAccessLocation(_actor, booking.LocationId))
        {
            return null;
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes is { Length: > 4000 })
        {
            notes = notes[..4000];
        }

        var ext = string.IsNullOrWhiteSpace(request.ExternalReference) ? null : request.ExternalReference.Trim();
        if (ext is { Length: > 256 })
        {
            ext = ext[..256];
        }

        if (ext is null)
        {
            var businessName = await _db.BusinessRegistrations
                .AsNoTracking()
                .Where(b => b.Id == businessId)
                .Select(b => b.BusinessName)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (businessName is null)
            {
                return null;
            }

            ext = await GenerateUniquePaymentReferenceAsync(businessId, businessName, cancellationToken)
                .ConfigureAwait(false);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new BookingPayment
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            BookingId = booking.Id,
            Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Currency = cur,
            Status = status,
            Method = method,
            Gateway = gateway,
            ExternalReference = ext,
            Notes = notes,
            CreatedAt = now,
        };

        _db.BookingPayments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (status == BookingPaymentStatus.Completed)
        {
            var bookingEntity = await _db.Bookings
                .FirstOrDefaultAsync(
                    b => b.Id == booking.Id && b.BusinessRegistrationId == businessId,
                    cancellationToken)
                .ConfigureAwait(false);

            if (bookingEntity is not null && bookingEntity.Status == BookingStatus.Pending)
            {
                bookingEntity.Status = BookingStatus.Confirmed;
                bookingEntity.UpdatedAt = now;
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var completedPaymentCount = await _db.BookingPayments
                .CountAsync(
                    p => p.BookingId == booking.Id && p.Status == BookingPaymentStatus.Completed,
                    cancellationToken)
                .ConfigureAwait(false);

            if (completedPaymentCount == 1)
            {
                await _confirmationEmails.SendBookingConfirmationAsync(booking.Id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        var locationName = booking.LocationId.HasValue
            ? await _db.BusinessLocations.AsNoTracking()
                .Where(l => l.Id == booking.LocationId.Value)
                .Select(l => l.Name)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        await _audit.LogForCurrentUserAsync(
            businessId,
            new OrganizationAuditEntry(
                OrganizationAuditActions.StatusChange,
                OrganizationAuditEntityTypes.BookingPayment,
                entity.Id,
                booking.LocationId,
                locationName,
                $"Recorded booking payment {status} for {booking.ConfirmationCode} ({entity.Amount:N2} {cur})."),
            cancellationToken).ConfigureAwait(false);

        return await GetSingleSummaryAsync(businessId, entity.Id, cancellationToken).ConfigureAwait(false);
    }

    private async Task<BookingPaymentSummaryDto?> GetSingleSummaryAsync(
        Guid businessId,
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var p = await _db.BookingPayments
            .AsNoTracking()
            .Include(x => x.Booking)
            .ThenInclude(b => b.Room)
            .Include(x => x.Booking)
            .ThenInclude(b => b.Location)
            .FirstOrDefaultAsync(x => x.Id == paymentId && x.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return p is null ? null : Map(p);
    }

    private Task<string> GenerateUniquePaymentReferenceAsync(
        Guid businessId,
        string businessName,
        CancellationToken cancellationToken) =>
        BusinessReferenceCodeGenerator.GenerateUniqueAsync(
            businessName,
            async (code, ct) => await _db.BookingPayments
                .AnyAsync(
                    p => p.BusinessRegistrationId == businessId
                        && p.ExternalReference != null
                        && p.ExternalReference == code,
                    ct)
                .ConfigureAwait(false),
            cancellationToken);

    private static BookingPaymentSummaryDto Map(BookingPayment p) =>
        new()
        {
            Id = p.Id,
            BookingId = p.BookingId,
            BookingGuestName = p.Booking.GuestName,
            BookingConfirmationCode = p.Booking.ConfirmationCode,
            RoomName = p.Booking.Room.Name,
            LocationId = p.Booking.LocationId,
            LocationName = p.Booking.Location?.Name,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status.ToString(),
            Method = p.Method.ToString(),
            Gateway = p.Gateway.ToString(),
            ExternalReference = p.ExternalReference,
            CreatedAt = p.CreatedAt,
        };

    private static bool TryParseStatus(string? value, out BookingPaymentStatus status)
    {
        status = BookingPaymentStatus.Pending;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out status)
               && Enum.IsDefined(typeof(BookingPaymentStatus), status);
    }

    private static bool TryParseGateway(string? value, out PaymentGatewayProvider gateway)
    {
        gateway = PaymentGatewayProvider.None;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out gateway)
               && Enum.IsDefined(typeof(PaymentGatewayProvider), gateway);
    }

    private static bool TryParseMethod(string? value, out BookingPaymentMethod method)
    {
        method = BookingPaymentMethod.Cash;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out method)
               && Enum.IsDefined(typeof(BookingPaymentMethod), method);
    }
}
