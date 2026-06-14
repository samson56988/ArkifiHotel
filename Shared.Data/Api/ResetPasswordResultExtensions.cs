using Shared.Data.Dtos;

namespace Shared.Data.Api;

public static class ResetPasswordResultExtensions
{
    public static ApiResult ToApiResult(this ResetPasswordResult result, string? successMessage = null)
    {
        if (result.Succeeded)
        {
            return ApiResult.Ok(successMessage ?? "Password updated. You can sign in with your new password.");
        }

        return ApiResult.Fail(
            result.ErrorCode ?? "Error",
            result.ErrorMessage ?? "Reset failed.");
    }
}
