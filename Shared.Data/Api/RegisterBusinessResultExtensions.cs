using Shared.Data.Dtos;

namespace Shared.Data.Api;

public static class RegisterBusinessResultExtensions
{
    public static ApiResult<BusinessRegistrationDto> ToApiResult(this RegisterBusinessResult result)
    {
        if (result.Succeeded && result.Registration is not null)
        {
            return ApiResult<BusinessRegistrationDto>.Ok(result.Registration);
        }

        return ApiResult<BusinessRegistrationDto>.Fail(
            result.ErrorCode ?? "Error",
            result.ErrorMessage ?? "An error occurred.");
    }
}
