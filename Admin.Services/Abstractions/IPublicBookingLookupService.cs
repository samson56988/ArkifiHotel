using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IPublicBookingLookupService
{
    Task<GuestBookingLookupDto?> GetByConfirmationCodeAsync(string confirmationCode, CancellationToken cancellationToken = default);
}
