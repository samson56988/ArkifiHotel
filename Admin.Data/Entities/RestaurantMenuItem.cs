namespace Admin.Data.Entities;

public class RestaurantMenuItem
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public RestaurantMenuCategory Category { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    /// <summary>JSON array of tag strings, e.g. ["Spicy","Vegetarian"].</summary>
    public string TagsJson { get; set; } = "[]";

    public string? ImageRelativePath { get; set; }

    public string? ImageOriginalFileName { get; set; }

    public int SortOrder { get; set; }

    public bool IsArchived { get; set; }

    /// <summary>When false, item is hidden from guest ordering but kept in admin.</summary>
    public bool IsAvailable { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
