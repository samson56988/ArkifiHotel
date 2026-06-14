namespace Shared.Data.Dtos;

public sealed class ResetPasswordResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static ResetPasswordResult Ok() => new() { Succeeded = true };

    public static ResetPasswordResult Fail(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}
