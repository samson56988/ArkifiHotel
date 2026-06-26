namespace Shared.Data.Dtos;

/// <summary>Multipart registration form (hotel / shortlet) with optional logo file.</summary>
public sealed class RegisterBusinessFormRequest
{
    public string BusinessName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool AcceptTerms { get; set; }

    public string BusinessType { get; set; } = "Hotel";

    public string PlanCode { get; set; } = "free";

    public RegisterBusinessRequest ToRegisterBusinessRequest() =>
        new()
        {
            BusinessName = BusinessName,
            Slug = Slug,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            Password = Password,
            AcceptTerms = AcceptTerms,
            BusinessType = BusinessType,
            PlanCode = PlanCode,
        };
}
