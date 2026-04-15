using Shared.Data.Dtos;

namespace Shared.Data.Api;

public static class LoginBusinessResultExtensions
{
    public static ApiResult<LoginBusinessData> ToApiResult(this LoginBusinessResult result)
    {
        if (result.Succeeded && result.Data is not null)
        {
            return ApiResult<LoginBusinessData>.Ok(result.Data);
        }

        return ApiResult<LoginBusinessData>.Fail(
            result.ErrorCode ?? "Error",
            result.ErrorMessage ?? "An error occurred.");
    }
}
