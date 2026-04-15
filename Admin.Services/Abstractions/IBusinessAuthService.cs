using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessAuthService
{
    Task<LoginBusinessResult> LoginAsync(
        LoginBusinessRequest request,
        CancellationToken cancellationToken = default);
}
