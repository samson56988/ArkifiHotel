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

    public StorefrontContactSectionDto Contact { get; set; } = new();

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

    /// <summary>Short label in the hero badge (e.g. “Luxury Boutique Hotel”).</summary>
    public string BadgeText { get; set; } = "Your stay awaits";
}

public sealed class StorefrontAboutStatDto
{
    public string Num { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public sealed class StorefrontAboutSectionDto
{
    public bool Enabled { get; set; } = true;

    public string Eyebrow { get; set; } = "About us";

    public string Title { get; set; } = "Our story";

    public string Description { get; set; } = string.Empty;

    public string TitleFont { get; set; } = "display";

    public string BodyFont { get; set; } = "body";

    public string Layout { get; set; } = "side-by-side";

    public string Quote { get; set; } = string.Empty;

    public string QuoteBy { get; set; } = string.Empty;

    /// <summary>When true, highlight stats (e.g. founded year, room count) show under the about copy.</summary>
    public bool ShowStats { get; set; }

    public List<StorefrontAboutStatDto> Stats { get; set; } = new();
}

public sealed class StorefrontRoomsSectionDto
{
    public bool Enabled { get; set; } = true;

    public string Eyebrow { get; set; } = "Accommodations";

    public string Title { get; set; } = "Our rooms";

    public string Subtitle { get; set; } = "Comfortable stays tailored to you.";

    public string TitleFont { get; set; } = "display";

    public string CardStyle { get; set; } = "elevated";

    public bool ShowPrice { get; set; } = true;

    public bool ShowFeaturedSection { get; set; } = true;

    public string FeaturedEyebrow { get; set; } = "Signature Stay";

    public string FeaturedTitle { get; set; } = "Our most sought-after room";

    public bool ShowPageStats { get; set; } = true;

    public bool ShowPolicies { get; set; } = true;

    public string PolicyBreakfast { get; set; } = "Complimentary for suite guests";

    public string PolicyPets { get; set; } = "Small pets welcome on request";

    public string PolicyCancellation { get; set; } = "Free up to 48 hours before arrival";

    public string CtaTitle { get; set; } = "Ready to book your stay?";

    public string CtaSubtitle { get; set; } = "Reserve directly — no payment required until confirmation.";

    public string CtaButtonText { get; set; } = "Check availability";
}

public sealed class StorefrontFacilitiesSectionDto
{
    public bool Enabled { get; set; } = true;

    public string Eyebrow { get; set; } = "On Property";

    public string Title { get; set; } = "Facilities & amenities";

    public string Subtitle { get; set; } = "Everything you need for a memorable stay.";

    /// <summary>Eyebrow above the facilities grid (below the page hero).</summary>
    public string GridEyebrow { get; set; } = "Browse amenities";

    /// <summary>Heading above the facilities grid.</summary>
    public string GridTitle { get; set; } = "What's on offer";

    /// <summary>Subtitle above the facilities grid.</summary>
    public string GridSubtitle { get; set; } = "Tap any facility to view photos and details.";

    public string TitleFont { get; set; } = "display";

    public string DisplayStyle { get; set; } = "grid";

    public bool ShowPageStats { get; set; } = true;

    public string SupportStatValue { get; set; } = "24/7";

    public string SupportStatLabel { get; set; } = "Guest support";

    public bool ShowGuestPerks { get; set; } = true;

    public string PerksEyebrow { get; set; } = "Guest Perks";

    public string PerksTitle { get; set; } = "Everything included in your stay";

    public string PerksSubtitle { get; set; } =
        "Complimentary access to most on-property amenities for all registered guests.";

    public List<string> PerksItems { get; set; } = new();
}

public sealed class StorefrontContactSectionDto
{
    /// <summary>Guest-facing address shown in the hero and contact footer.</summary>
    public string Location { get; set; } = string.Empty;

    public string CheckIn { get; set; } = string.Empty;

    public string CheckOut { get; set; } = string.Empty;

    /// <summary>Intro copy above the contact form.</summary>
    public string IntroText { get; set; } =
        "Questions about your stay? Send us a message and our team will respond within a few hours.";
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

    public string? Tagline { get; set; }

    public string? Description { get; set; }

    public decimal BasePricePerNight { get; set; }

    public int MaxOccupancy { get; set; }

    public int? BedroomCount { get; set; }

    public int? BathroomCount { get; set; }

    public bool IsGuestFavorite { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public IReadOnlyList<string> ImageUrls { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> AmenityNames { get; set; } = Array.Empty<string>();

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }
}

public sealed class PublicStorefrontAmenityDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }
}

public sealed class PublicStorefrontFacilityDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PrimaryImageUrl { get; set; }

    public IReadOnlyList<string> ImageUrls { get; set; } = Array.Empty<string>();

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }
}

public sealed class PublicStorefrontDto
{
    public Guid BusinessId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    /// <summary>Hotel or Shortlet — drives guest storefront layout.</summary>
    public string BusinessType { get; set; } = "Hotel";

    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public StorefrontThemeDto Theme { get; set; } = new();

    public IReadOnlyList<PublicStorefrontLocationDto> Locations { get; set; } = Array.Empty<PublicStorefrontLocationDto>();

    /// <summary>When true, the guest must pick a branch before rooms, facilities, and banners are returned.</summary>
    public bool RequiresBranchSelection { get; set; }

    public Guid? ActiveLocationId { get; set; }

    public IReadOnlyList<PublicStorefrontRoomDto> Rooms { get; set; } = Array.Empty<PublicStorefrontRoomDto>();

    public IReadOnlyList<PublicStorefrontFacilityDto> Facilities { get; set; } = Array.Empty<PublicStorefrontFacilityDto>();

    public IReadOnlyList<PublicStorefrontAmenityDto> Amenities { get; set; } = Array.Empty<PublicStorefrontAmenityDto>();

    public BusinessSocialProfileDto Social { get; set; } = new();

    /// <summary>Hero banner images in carousel order (max 10). One image = static hero; two or more = carousel.</summary>
    public IReadOnlyList<string> HeroImages { get; set; } = Array.Empty<string>();

    /// <summary>Dedicated “Who we are” section photo (not used in hero banner).</summary>
    public string? AboutImageUrl { get; set; }

    public PublicStorefrontRestaurantDto? Restaurant { get; set; }

    public IReadOnlyList<PublicStorefrontEventHallDto> EventHalls { get; set; } = Array.Empty<PublicStorefrontEventHallDto>();
}
