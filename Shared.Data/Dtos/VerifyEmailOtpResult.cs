namespace Shared.Data.Dtos;

public sealed class VerifyEmailOtpResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static VerifyEmailOtpResult Ok() => new() { Succeeded = true };

    public static VerifyEmailOtpResult Fail(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}
