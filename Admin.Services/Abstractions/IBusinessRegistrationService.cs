using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessRegistrationService
{
    Task<RegisterBusinessResult> RegisterAsync(
        RegisterBusinessRequest request,
        CancellationToken cancellationToken = default);
}
