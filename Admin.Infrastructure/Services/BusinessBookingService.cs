using Admin.Data;
using Admin.Data.Constants;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;
using Shared.Data.Helpers;

namespace Admin.Infrastructure.Services;

public sealed class BusinessBookingService : IBusinessBookingService
{
    private const string DefaultCurrency = "NGN";

    private readonly AdminDbContext _db;
    private readonly IOrganizationUserContext _actor;
    private readonly ICustomerConfirmationEmailService _confirmationEmails;

    public BusinessBookingService(
        AdminDbContext db,
        IOrganizationUserContext actor,
        ICustomerConfirmationEmailService confirmationEmails)
    {
        _db = db;
        _actor = actor;
        _confirmationEmails = confirmationEmails;
    }

    public async Task<PagedResultDto<BookingSummaryDto>> ListAsync(
        Guid businessId,
        ListBookingsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => query.PageSize,
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var bookings = _db.Bookings
            .AsNoTracking()
            .Where(b => b.BusinessRegistrationId == businessId);

        bookings = OrganizationQueryScope.ApplyBookingScope(bookings, _actor);

        if (TryParseStayPhase(query.StayPhase, out var stayPhase))
        {
            bookings = stayPhase switch
            {
                BookingStayPhaseFilter.Active => bookings.Where(b => b.CheckOutDate > today),
                BookingStayPhaseFilter.Closed => bookings.Where(b => b.CheckOutDate <= today),
                _ => bookings,
            };
        }

        if (query.CheckInFrom.HasValue)
        {
            bookings = bookings.Where(b => b.CheckInDate >= query.CheckInFrom.Value);
        }

        if (query.CheckInTo.HasValue)
        {
            bookings = bookings.Where(b => b.CheckInDate <= query.CheckInTo.Value);
        }

        if (query.CheckOutFrom.HasValue)
        {
            bookings = bookings.Where(b => b.CheckOutDate >= query.CheckOutFrom.Value);
        }

        if (query.CheckOutTo.HasValue)
        {
            bookings = bookings.Where(b => b.CheckOutDate <= query.CheckOutTo.Value);
        }

        if (TryParseStatus(query.Status, out var statusFilter))
        {
            bookings = bookings.Where(b => b.Status == statusFilter);
        }

        var totalCount = await bookings.CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var rows = await bookings
            .Include(b => b.Room)
            .Include(b => b.Location)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResultDto<BookingSummaryDto>
        {
            Items = rows.Select(b => MapSummary(b, today)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }

    public async Task<BookingDetailDto?> GetAsync(Guid businessId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var b = await _db.Bookings
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return b is null ? null : MapDetail(b);
    }

    public async Task<IReadOnlyList<RoomAvailabilityDto>> GetAvailabilityAsync(
        Guid businessId,
        DateOnly checkIn,
        DateOnly checkOut,
        Guid? roomId = null,
        Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        if (checkOut <= checkIn)
        {
            return Array.Empty<RoomAvailabilityDto>();
        }

        var roomQuery = _db.Rooms
            .AsNoTracking()
            .Include(r => r.Location)
            .Where(r => r.BusinessRegistrationId == businessId && !r.IsArchived);

        if (roomId.HasValue)
        {
            roomQuery = roomQuery.Where(r => r.Id == roomId.Value);
        }

        if (locationId.HasValue)
        {
            roomQuery = roomQuery.Where(r => r.LocationId == locationId.Value);
        }

        var rooms = await roomQuery
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rooms.Count == 0)
        {
            return Array.Empty<RoomAvailabilityDto>();
        }

        var roomIds = rooms.Select(r => r.Id).ToList();
        var bookingRows = await _db.Bookings
            .AsNoTracking()
            .Where(b => roomIds.Contains(b.RoomId) && b.Status != BookingStatus.Cancelled)
            .Select(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var byRoom = bookingRows
            .GroupBy(b => b.RoomId)
            .ToDictionary(g => g.Key, g => g.Select(x => (x.CheckInDate, x.CheckOutDate)).ToList());

        return rooms
            .Select(room =>
            {
                byRoom.TryGetValue(room.Id, out var stays);
                stays ??= [];
                var peak = RoomBookingAvailability.GetPeakOccupancyInRange(stays, checkIn, checkOut);
                var available = Math.Max(0, room.Quantity - peak);

                return new RoomAvailabilityDto
                {
                    RoomId = room.Id,
                    RoomName = room.Name,
                    TotalQuantity = room.Quantity,
                    PeakBooked = peak,
                    AvailableUnits = available,
                    IsAvailable = available > 0,
                    BasePricePerNight = room.BasePricePerNight,
                    BasePricePerWeek = room.BasePricePerWeek,
                    MaxOccupancy = room.MaxOccupancy,
                    LocationId = room.LocationId,
                    LocationName = room.Location?.Name,
                };
            })
            .ToList();
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

        if (request.LocationId == Guid.Empty
            || !room.LocationId.HasValue
            || room.LocationId.Value != request.LocationId)
        {
            return null;
        }

        if (!await _db.BusinessLocations
                .AsNoTracking()
                .AnyAsync(l => l.Id == request.LocationId && l.BusinessRegistrationId == businessId, cancellationToken)
                .ConfigureAwait(false))
        {
            return null;
        }

        if (await IsFullyBookedAsync(request.RoomId, room.Quantity, checkIn, checkOut, excludeBookingId: null, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        if (!request.PaymentConfirmed)
        {
            return null;
        }

        if (!TryParseReceptionPaymentMethod(request.PaymentMethod, out var paymentMethod))
        {
            return null;
        }

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

        var code = await GenerateUniqueConfirmationCodeAsync(businessId, businessName, cancellationToken)
            .ConfigureAwait(false);
        var total = RoomPricingHelper.CalculateStayTotal(room.BasePricePerNight, room.BasePricePerWeek, nights);
        var now = DateTimeOffset.UtcNow;

        var entity = new Booking
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            RoomId = request.RoomId,
            LocationId = room.LocationId,
            GuestName = guestName,
            GuestEmail = guestEmail,
            GuestPhone = guestPhone,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalAmount = total,
            Currency = DefaultCurrency,
            Status = BookingStatus.Confirmed,
            ConfirmationCode = code,
            InternalNotes = string.IsNullOrWhiteSpace(request.InternalNotes) ? null : request.InternalNotes.Trim(),
            CreatedAt = now,
        };

        _db.Bookings.Add(entity);

        _db.BookingPayments.Add(
            new BookingPayment
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = businessId,
                BookingId = entity.Id,
                Amount = total,
                Currency = DefaultCurrency,
                Status = BookingPaymentStatus.Completed,
                Method = paymentMethod,
                Gateway = PaymentGatewayProvider.None,
                ExternalReference = code,
                CreatedAt = now,
            });

        await UpsertCustomerFromGuestAsync(businessId, guestName, guestEmail, guestPhone!, now, cancellationToken)
            .ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _confirmationEmails.SendBookingConfirmationAsync(entity.Id, cancellationToken).ConfigureAwait(false);

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

        return Enum.TryParse(status.Trim(), ignoreCase: true, out value)
               && Enum.IsDefined(typeof(BookingStatus), value);
    }

    private static bool TryParseReceptionPaymentMethod(string? value, out BookingPaymentMethod method)
    {
        method = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Enum.TryParse(value.Trim(), ignoreCase: true, out method)
            || !Enum.IsDefined(typeof(BookingPaymentMethod), method))
        {
            return false;
        }

        return method is BookingPaymentMethod.Cash or BookingPaymentMethod.BankTransfer;
    }

    private enum BookingStayPhaseFilter
    {
        All,
        Active,
        Closed,
    }

    private static bool TryParseStayPhase(string? value, out BookingStayPhaseFilter phase)
    {
        phase = BookingStayPhaseFilter.All;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out phase)
               && Enum.IsDefined(typeof(BookingStayPhaseFilter), phase);
    }

    private static BookingSummaryDto MapSummary(Booking b, DateOnly today)
    {
        var nights = ComputeNights(b.CheckInDate, b.CheckOutDate);
        return new BookingSummaryDto
        {
            Id = b.Id,
            RoomId = b.RoomId,
            RoomName = b.Room.Name,
            LocationId = b.LocationId,
            LocationName = b.Location?.Name,
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
            IsStayClosed = b.CheckOutDate <= today,
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
            LocationId = b.LocationId,
            LocationName = b.Location?.Name,
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
