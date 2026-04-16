using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessBookingService : IBusinessBookingService
{
    private const string DefaultCurrency = "NGN";
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly AdminDbContext _db;

    public BusinessBookingService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BookingSummaryDto>> ListAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
            .Where(b => b.BusinessRegistrationId == businessId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(MapSummary).ToList();
    }

    public async Task<BookingDetailDto?> GetAsync(Guid businessId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var b = await _db.Bookings
            .AsNoTracking()
            .Include(x => x.Room)
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return b is null ? null : MapDetail(b);
    }

    public async Task<BookingDetailDto?> CreateAsync(Guid businessId, CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateGuestAndDates(request, out var guestName, out var guestEmail, out var guestPhone, out var checkIn, out var checkOut, out var nights))
        {
            return null;
        }

        var room = await _db.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (room is null || room.IsArchived)
        {
            return null;
        }

        if (await HasOverlappingBookingAsync(request.RoomId, checkIn, checkOut, excludeBookingId: null, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var code = await GenerateUniqueConfirmationCodeAsync(cancellationToken).ConfigureAwait(false);
        var total = decimal.Round(room.BasePricePerNight * nights, 2, MidpointRounding.AwayFromZero);
        var now = DateTimeOffset.UtcNow;

        var entity = new Booking
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            RoomId = request.RoomId,
            GuestName = guestName,
            GuestEmail = guestEmail,
            GuestPhone = guestPhone,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalAmount = total,
            Currency = DefaultCurrency,
            Status = BookingStatus.Pending,
            ConfirmationCode = code,
            InternalNotes = string.IsNullOrWhiteSpace(request.InternalNotes) ? null : request.InternalNotes.Trim(),
            CreatedAt = now,
        };

        _db.Bookings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, entity.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BookingDetailDto?> UpdateStatusAsync(
        Guid businessId,
        Guid bookingId,
        string status,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseStatus(status, out var newStatus))
        {
            return null;
        }

        var entity = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        entity.Status = newStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, bookingId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        Guid? excludeBookingId,
        CancellationToken cancellationToken)
    {
        return await _db.Bookings.AnyAsync(
                b => b.RoomId == roomId
                    && b.Status != BookingStatus.Cancelled
                    && (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value)
                    && b.CheckInDate < checkOut
                    && b.CheckOutDate > checkIn,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<string> GenerateUniqueConfirmationCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var code = new string(Enumerable.Range(0, 10).Select(_ => CodeChars[Random.Shared.Next(CodeChars.Length)]).ToArray());
            var exists = await _db.Bookings.AnyAsync(b => b.ConfirmationCode == code, cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                return code;
            }
        }

        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }

    private static bool ValidateGuestAndDates(
        CreateBookingRequest request,
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

        if (!string.IsNullOrWhiteSpace(request.GuestPhone))
        {
            var p = request.GuestPhone.Trim();
            if (p.Length > 40)
            {
                return false;
            }

            guestPhone = p;
        }

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
        guestEmail = ge;
        return true;
    }

    private static bool TryParseStatus(string? status, out BookingStatus value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        return Enum.TryParse(status.Trim(), ignoreCase: true, out value);
    }

    private static BookingSummaryDto MapSummary(Booking b)
    {
        var nights = ComputeNights(b.CheckInDate, b.CheckOutDate);
        return new BookingSummaryDto
        {
            Id = b.Id,
            RoomId = b.RoomId,
            RoomName = b.Room.Name,
            GuestName = b.GuestName,
            GuestEmail = b.GuestEmail,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            Nights = nights,
            Status = b.Status.ToString(),
            TotalAmount = b.TotalAmount,
            Currency = b.Currency,
            ConfirmationCode = b.ConfirmationCode,
            CreatedAt = b.CreatedAt,
        };
    }

    private static BookingDetailDto MapDetail(Booking b)
    {
        var nights = ComputeNights(b.CheckInDate, b.CheckOutDate);
        return new BookingDetailDto
        {
            Id = b.Id,
            RoomId = b.RoomId,
            RoomName = b.Room.Name,
            GuestName = b.GuestName,
            GuestEmail = b.GuestEmail,
            GuestPhone = b.GuestPhone,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            Nights = nights,
            Status = b.Status.ToString(),
            TotalAmount = b.TotalAmount,
            Currency = b.Currency,
            ConfirmationCode = b.ConfirmationCode,
            InternalNotes = b.InternalNotes,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt,
        };
    }

    private static int ComputeNights(DateOnly checkIn, DateOnly checkOut) => checkOut.DayNumber - checkIn.DayNumber;
}
