using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessEmailVerificationService
{
    Task SendOtpAsync(
        Guid businessId,
        string businessName,
        string contactEmail,
        CancellationToken cancellationToken = default);

    Task<VerifyEmailOtpResult> VerifyOtpAsync(
        VerifyEmailOtpRequest request,
        CancellationToken cancellationToken = default);
}
