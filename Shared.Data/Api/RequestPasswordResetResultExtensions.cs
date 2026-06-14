using Shared.Data.Dtos;

namespace Shared.Data.Api;

public static class RequestPasswordResetResultExtensions
{
    public static ApiResult<RequestPasswordResetData> ToApiResult(this RequestPasswordResetResult result)
    {
        if (result.Succeeded)
        {
            return ApiResult<RequestPasswordResetData>.Ok(result.Data, result.Message);
        }

        return ApiResult<RequestPasswordResetData>.Fail(
            result.ErrorCode ?? "Error",
            result.ErrorMessage ?? "An error occurred.");
    }
}
