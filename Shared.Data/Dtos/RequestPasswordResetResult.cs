namespace Shared.Data.Dtos;

public sealed class RequestPasswordResetResult
{
    public bool Succeeded { get; init; }

    public RequestPasswordResetData? Data { get; init; }

    public string? Message { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static RequestPasswordResetResult Ok(RequestPasswordResetData? data, string? message = null) =>
        new() { Succeeded = true, Data = data, Message = message };

    public static RequestPasswordResetResult Fail(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}
