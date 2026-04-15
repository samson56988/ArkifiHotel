namespace Shared.Data.Dtos;

public sealed class LoginBusinessResult
{
    public bool Succeeded { get; init; }

    public LoginBusinessData? Data { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static LoginBusinessResult Ok(LoginBusinessData data) =>
        new() { Succeeded = true, Data = data };

    public static LoginBusinessResult Fail(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}
