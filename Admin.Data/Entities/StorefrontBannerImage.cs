namespace Admin.Data.Entities;

public class StorefrontBannerImage
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    /// <summary>Path relative to wwwroot, e.g. uploads/{businessId}/banner/file.jpg</summary>
    public string RelativePath { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
