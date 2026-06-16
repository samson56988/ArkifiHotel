namespace Admin.Data.Entities;

/// <summary>Single “Who we are” section photo for a business storefront (not shared with hero banner).</summary>
public class StorefrontAboutImage
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    /// <summary>Path relative to wwwroot, e.g. uploads/{businessId}/about/file.jpg</summary>
    public string RelativePath { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
