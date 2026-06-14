using System.Text.Json;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Helpers;

public static class StorefrontThemeDefaults
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public static StorefrontThemeDto Create(string businessName) =>
        new()
        {
            GlobalFont = "modern-sans",
            Banner = new StorefrontBannerSectionDto
            {
                Style = "grand-hero",
                HeadlineFont = "display",
                SubheadlineFont = "body",
                Headline = $"Welcome to {businessName}",
                Subheadline = "Book your stay with us — comfort, style, and warm hospitality.",
                TextAlign = "center",
                OverlayOpacity = 55,
            },
            About = new StorefrontAboutSectionDto
            {
                Enabled = true,
                Title = "Who we are",
                Description =
                    "We are a hospitality team dedicated to memorable stays. From check-in to checkout, every detail is crafted for comfort and ease.",
                TitleFont = "display",
                BodyFont = "body",
                Layout = "side-by-side",
            },
            Rooms = new StorefrontRoomsSectionDto
            {
                Enabled = true,
                Title = "Our rooms",
                Subtitle = "Thoughtfully designed spaces for every traveler.",
                TitleFont = "display",
                CardStyle = "elevated",
                ShowPrice = true,
            },
            Facilities = new StorefrontFacilitiesSectionDto
            {
                Enabled = true,
                Title = "Facilities & amenities",
                Subtitle = "Relax, recharge, and enjoy our property.",
                TitleFont = "display",
                DisplayStyle = "grid",
            },
            Footer = new StorefrontFooterSectionDto
            {
                Style = "columns",
                Tagline = "Your home away from home.",
                CopyrightText = $"© {DateTime.UtcNow.Year} {businessName}. All rights reserved.",
                ShowContact = true,
                BackgroundStyle = "dark-band",
            },
            Colors = new StorefrontColorPaletteDto
            {
                Preset = "sage-luxe",
                Primary = "#5c7a5c",
                Accent = "#c8dcc8",
                Background = "#faf9f6",
                Text = "#1f2a1f",
            },
        };

    public static StorefrontThemeDto Deserialize(string? json, string businessName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Create(businessName);
        }

        try
        {
            var theme = JsonSerializer.Deserialize<StorefrontThemeDto>(json, JsonOptions);
            return theme ?? Create(businessName);
        }
        catch (JsonException)
        {
            return Create(businessName);
        }
    }

    public static string Serialize(StorefrontThemeDto theme) =>
        JsonSerializer.Serialize(theme, JsonOptions);
}
