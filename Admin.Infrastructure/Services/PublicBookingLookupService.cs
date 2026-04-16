using Admin.Data;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class PublicBookingLookupService : IPublicBookingLookupService
{
    private readonly AdminDbContext _db;

    public PublicBookingLookupService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<GuestBookingLookupDto?> GetByConfirmationCodeAsync(string confirmationCode, CancellationToken cancellationToken = default)
    {
        var code = NormalizeCode(confirmationCode);
        if (code.Length < 4)
        {
            return null;
        }

        var b = await _db.Bookings
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.BusinessRegistration)
            .FirstOrDefaultAsync(x => x.ConfirmationCode == code, cancellationToken)
            .ConfigureAwait(false);

        if (b is null)
        {
            return null;
        }

        var nights = b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber;
        return new GuestBookingLookupDto
        {
            PropertyName = b.BusinessRegistration.BusinessName,
            RoomName = b.Room.Name,
            GuestName = b.GuestName,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            Nights = nights,
            Status = b.Status.ToString(),
            TotalAmount = b.TotalAmount,
            Currency = b.Currency,
            ConfirmationCode = b.ConfirmationCode,
        };
    }

    private static string NormalizeCode(string? confirmationCode)
    {
        if (string.IsNullOrWhiteSpace(confirmationCode))
        {
            return string.Empty;
        }

        return confirmationCode.Trim().ToUpperInvariant();
    }
}
