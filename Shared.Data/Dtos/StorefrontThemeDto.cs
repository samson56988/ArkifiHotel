namespace Shared.Data.Dtos;

/// <summary>Full storefront theme configuration for a business guest site.</summary>
public sealed class StorefrontThemeDto
{
    public string GlobalFont { get; set; } = "modern-sans";

    public StorefrontBannerSectionDto Banner { get; set; } = new();

    public StorefrontAboutSectionDto About { get; set; } = new();

    public StorefrontRoomsSectionDto Rooms { get; set; } = new();

    public StorefrontFacilitiesSectionDto Facilities { get; set; } = new();

    public StorefrontFooterSectionDto Footer { get; set; } = new();

    public StorefrontColorPaletteDto Colors { get; set; } = new();
}

public sealed class StorefrontBannerSectionDto
{
    public string Style { get; set; } = "grand-hero";

    public string HeadlineFont { get; set; } = "display";

    public string SubheadlineFont { get; set; } = "body";

    public string Headline { get; set; } = string.Empty;

    public string Subheadline { get; set; } = string.Empty;

    public string TextAlign { get; set; } = "center";

    public int OverlayOpacity { get; set; } = 55;
}

public sealed class StorefrontAboutSectionDto
{
    public bool Enabled { get; set; } = true;

    public string Title { get; set; } = "Who we are";

    public string Description { get; set; } = string.Empty;

    public string TitleFont { get; set; } = "display";

    public string BodyFont { get; set; } = "body";

    public string Layout { get; set; } = "side-by-side";
}

public sealed class StorefrontRoomsSectionDto
{
    public bool Enabled { get; set; } = true;

    public string Title { get; set; } = "Our rooms";

    public string Subtitle { get; set; } = "Comfortable stays tailored to you.";

    public string TitleFont { get; set; } = "display";

    public string CardStyle { get; set; } = "elevated";

    public bool ShowPrice { get; set; } = true;
}

public sealed class StorefrontFacilitiesSectionDto
{
    public bool Enabled { get; set; } = true;

    public string Title { get; set; } = "Facilities & amenities";

    public string Subtitle { get; set; } = "Everything you need for a memorable stay.";

    public string TitleFont { get; set; } = "display";

    public string DisplayStyle { get; set; } = "grid";
}

public sealed class StorefrontFooterSectionDto
{
    public string Style { get; set; } = "columns";

    public string Tagline { get; set; } = string.Empty;

    public string CopyrightText { get; set; } = string.Empty;

    public bool ShowContact { get; set; } = true;

    public string BackgroundStyle { get; set; } = "dark-band";
}

public sealed class StorefrontColorPaletteDto
{
    public string Preset { get; set; } = "sage-luxe";

    public string Primary { get; set; } = "#5c7a5c";

    public string Accent { get; set; } = "#c8dcc8";

    public string Background { get; set; } = "#faf9f6";

    public string Text { get; set; } = "#1f2a1f";
}

public sealed class PublicStorefrontRoomDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal BasePricePerNight { get; set; }

    public int MaxOccupancy { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public string? LocationName { get; set; }
}

public sealed class PublicStorefrontFacilityDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PrimaryImageUrl { get; set; }

    public string? LocationName { get; set; }
}

public sealed class PublicStorefrontDto
{
    public Guid BusinessId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public StorefrontThemeDto Theme { get; set; } = new();

    public IReadOnlyList<PublicStorefrontRoomDto> Rooms { get; set; } = Array.Empty<PublicStorefrontRoomDto>();

    public IReadOnlyList<PublicStorefrontFacilityDto> Facilities { get; set; } = Array.Empty<PublicStorefrontFacilityDto>();

    public BusinessSocialProfileDto Social { get; set; } = new();
}
