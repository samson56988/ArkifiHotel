namespace Shared.Data.Dtos;

public sealed class ChangeDefaultPasswordResult
{
    public bool Success { get; init; }

    public string? ErrorCode { get; init; }

    public string? Message { get; init; }

    public static ChangeDefaultPasswordResult Ok() => new() { Success = true };

    public static ChangeDefaultPasswordResult Fail(string code, string message) =>
        new() { Success = false, ErrorCode = code, Message = message };
}
