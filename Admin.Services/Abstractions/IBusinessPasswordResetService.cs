using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessPasswordResetService
{
    Task<RequestPasswordResetResult> RequestResetAsync(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken = default);

    Task<ResetPasswordResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}
