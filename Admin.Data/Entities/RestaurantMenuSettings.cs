namespace Admin.Data.Entities;

/// <summary>Per-business restaurant / menu page configuration for the guest storefront.</summary>
public class RestaurantMenuSettings
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public bool Enabled { get; set; }

    public string NavLabel { get; set; } = "Restaurant & menu";

    public string HeroEyebrow { get; set; } = "Dining";

    public string HeroTitle { get; set; } = "Restaurant & bar";

    public string HeroSubtitle { get; set; } = string.Empty;

    public string MealsSectionTitle { get; set; } = "Meals";

    public string DrinksSectionTitle { get; set; } = "Drinks";

    public string? HeroImageRelativePath { get; set; }

    public string? HeroImageOriginalFileName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
