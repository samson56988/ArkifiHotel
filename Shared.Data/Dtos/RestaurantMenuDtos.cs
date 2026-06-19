namespace Shared.Data.Dtos;

public sealed class RestaurantMenuSettingsDto
{
    public bool Enabled { get; set; }

    public string NavLabel { get; set; } = "Restaurant & menu";

    public string HeroEyebrow { get; set; } = "Dining";

    public string HeroTitle { get; set; } = "Restaurant & bar";

    public string HeroSubtitle { get; set; } = string.Empty;

    public string MealsSectionTitle { get; set; } = "Meals";

    public string DrinksSectionTitle { get; set; } = "Drinks";

    public string? HeroImageUrl { get; set; }
}

public sealed class UpdateRestaurantMenuSettingsRequest
{
    public bool Enabled { get; set; }

    public string NavLabel { get; set; } = "Restaurant & menu";

    public string HeroEyebrow { get; set; } = "Dining";

    public string HeroTitle { get; set; } = "Restaurant & bar";

    public string HeroSubtitle { get; set; } = string.Empty;

    public string MealsSectionTitle { get; set; } = "Meals";

    public string DrinksSectionTitle { get; set; } = "Drinks";
}

public sealed class RestaurantMenuCategoryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>food or drink</summary>
    public string Section { get; set; } = "food";

    public int SortOrder { get; set; }

    public bool IsArchived { get; set; }

    public int ItemCount { get; set; }
}

public sealed class CreateRestaurantMenuCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public string Section { get; set; } = "food";

    public int SortOrder { get; set; }
}

public sealed class UpdateRestaurantMenuCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

public sealed class RestaurantMenuItemDto
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }

    public bool IsArchived { get; set; }

    public bool IsAvailable { get; set; } = true;
}

public sealed class CreateRestaurantMenuItemRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public IReadOnlyList<string>? Tags { get; set; }

    public int SortOrder { get; set; }
}

public sealed class UpdateRestaurantMenuItemRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public IReadOnlyList<string>? Tags { get; set; }

    public int SortOrder { get; set; }
}

public sealed class PublicStorefrontMenuItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;
}

public sealed class PublicStorefrontMenuCategoryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<PublicStorefrontMenuItemDto> Items { get; set; } = Array.Empty<PublicStorefrontMenuItemDto>();
}

public sealed class PublicStorefrontRestaurantDto
{
    public bool Enabled { get; set; }

    public string NavLabel { get; set; } = "Restaurant & menu";

    public string HeroEyebrow { get; set; } = string.Empty;

    public string HeroTitle { get; set; } = string.Empty;

    public string HeroSubtitle { get; set; } = string.Empty;

    public string MealsSectionTitle { get; set; } = "Meals";

    public string DrinksSectionTitle { get; set; } = "Drinks";

    public string? HeroImageUrl { get; set; }

    public IReadOnlyList<PublicStorefrontMenuCategoryDto> FoodCategories { get; set; } =
        Array.Empty<PublicStorefrontMenuCategoryDto>();

    public IReadOnlyList<PublicStorefrontMenuCategoryDto> DrinkCategories { get; set; } =
        Array.Empty<PublicStorefrontMenuCategoryDto>();
}
