using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessAuthService
{
    Task<LoginBusinessResult> LoginAsync(
        LoginBusinessRequest request,
        CancellationToken cancellationToken = default);

    Task<VerifyEmailOtpResult> VerifyEmailOtpAsync(
        VerifyEmailOtpRequest request,
        CancellationToken cancellationToken = default);

    Task<LoginBusinessResult> VerifyLoginOtpAsync(
        VerifyLoginOtpRequest request,
        CancellationToken cancellationToken = default);
}
