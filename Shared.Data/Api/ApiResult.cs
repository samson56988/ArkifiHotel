namespace Shared.Data.Api;

/// <summary>
/// Standard API response envelope for endpoints that return a payload.
/// Use with appropriate HTTP status codes (e.g. 200/201 on success, 400/409 on failure).
/// </summary>
public sealed class ApiResult<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    /// <summary>Human-readable message (optional on success; error description on failure).</summary>
    public string? Message { get; init; }

    /// <summary>Machine-readable code on failure (e.g. Validation, DuplicateEmail, NotFound).</summary>
    public string? Code { get; init; }

    /// <summary>Optional validation or field-level errors.</summary>
    public IReadOnlyList<string>? ValidationErrors { get; init; }

    public static ApiResult<T> Ok(T data, string? message = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
        };

    public static ApiResult<T> Fail(
        string code,
        string message,
        IReadOnlyList<string>? validationErrors = null) =>
        new()
        {
            Success = false,
            Code = code,
            Message = message,
            ValidationErrors = validationErrors,
        };
}

/// <summary>Standard API response envelope when there is no payload (e.g. delete, command).</summary>
public sealed class ApiResult
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public string? Code { get; init; }

    public IReadOnlyList<string>? ValidationErrors { get; init; }

    public static ApiResult Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ApiResult Fail(
        string code,
        string message,
        IReadOnlyList<string>? validationErrors = null) =>
        new()
        {
            Success = false,
            Code = code,
            Message = message,
            ValidationErrors = validationErrors,
        };
}
