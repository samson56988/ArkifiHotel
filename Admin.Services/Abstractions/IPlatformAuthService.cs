using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IPlatformAuthService
{
    Task<PlatformLoginResult> LoginAsync(PlatformLoginRequest request, CancellationToken cancellationToken = default);
}

public sealed class PlatformLoginResult
{
    public bool Success { get; init; }

    public string? ErrorCode { get; init; }

    public string? Message { get; init; }

    public PlatformLoginData? Data { get; init; }

    public static PlatformLoginResult Ok(PlatformLoginData data) =>
        new() { Success = true, Data = data };

    public static PlatformLoginResult Fail(string code, string message) =>
        new() { Success = false, ErrorCode = code, Message = message };
}
