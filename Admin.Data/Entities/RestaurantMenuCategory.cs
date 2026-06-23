namespace Admin.Data.Entities;

public class RestaurantMenuCategory
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid LocationId { get; set; }

    public BusinessLocation Location { get; set; } = null!;

    public string Name { get; set; } = null!;

    public RestaurantMenuSection Section { get; set; }

    public int SortOrder { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<RestaurantMenuItem> Items { get; set; } = new List<RestaurantMenuItem>();
}
