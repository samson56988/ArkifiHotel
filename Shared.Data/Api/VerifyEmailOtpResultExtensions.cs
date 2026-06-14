using Shared.Data.Dtos;

namespace Shared.Data.Api;

public static class VerifyEmailOtpResultExtensions
{
    public static ApiResult ToApiResult(this VerifyEmailOtpResult result, string? successMessage = null)
    {
        if (result.Succeeded)
        {
            return ApiResult.Ok(successMessage ?? "Email verified successfully.");
        }

        return ApiResult.Fail(
            result.ErrorCode ?? "Error",
            result.ErrorMessage ?? "Verification failed.");
    }
}
