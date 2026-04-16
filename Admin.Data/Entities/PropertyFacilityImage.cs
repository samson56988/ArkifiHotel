namespace Admin.Data.Entities;

public class PropertyFacilityImage
{
    public Guid Id { get; set; }

    public Guid PropertyFacilityId { get; set; }

    public PropertyFacility PropertyFacility { get; set; } = null!;

    /// <summary>Path under wwwroot, e.g. uploads/{businessId}/facilities/{facilityId}/file.jpg</summary>
    public string RelativePath { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
