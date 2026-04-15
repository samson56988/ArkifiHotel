namespace Shared.Data.Dtos;

/// <summary>Outcome of a registration attempt.</summary>
public sealed class RegisterBusinessResult
{
    public bool Succeeded { get; init; }

    public BusinessRegistrationDto? Registration { get; init; }

    /// <summary>Machine-friendly code, e.g. Validation, DuplicateEmail, AcceptTermsRequired.</summary>
    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static RegisterBusinessResult Ok(BusinessRegistrationDto dto) =>
        new() { Succeeded = true, Registration = dto };

    public static RegisterBusinessResult Fail(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}
