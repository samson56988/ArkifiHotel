using Admin.Data;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class StorefrontThemeService : IStorefrontThemeService
{
    private readonly AdminDbContext _db;

    public StorefrontThemeService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<StorefrontThemeDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        var theme = StorefrontThemeDefaults.Deserialize(entity.StorefrontThemeJson, entity.BusinessName);
        NormalizeTheme(theme);
        return theme;
    }

    public async Task<(StorefrontThemeDto? Theme, string? ErrorMessage)> UpdateAsync(
        Guid businessId,
        StorefrontThemeDto theme,
        CancellationToken cancellationToken = default)
    {
        if (theme is null)
        {
            return (null, "Theme payload is required.");
        }

        NormalizeTheme(theme);

        var entity = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return (null, "Business not found.");
        }

        entity.StorefrontThemeJson = StorefrontThemeDefaults.Serialize(theme);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (theme, null);
    }

    public async Task<PublicStorefrontDto?> GetPublicBySlugAsync(
        string slug,
        Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = BusinessSlugHelper.Normalize(slug);
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        var business = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug == normalized, cancellationToken)
            .ConfigureAwait(false);

        if (business is null)
        {
            return null;
        }

        var theme = StorefrontThemeDefaults.Deserialize(business.StorefrontThemeJson, business.BusinessName);
        NormalizeTheme(theme);

        var locations = await _db.BusinessLocations
            .AsNoTracking()
            .Where(l => l.BusinessRegistrationId == business.Id)
            .OrderBy(l => l.Name)
            .Select(l => new PublicStorefrontLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Guid? effectiveLocationId = null;
        var requiresBranchSelection = false;

        if (locationId.HasValue)
        {
            if (!locations.Any(l => l.Id == locationId.Value))
            {
                return null;
            }

            effectiveLocationId = locationId;
        }
        else if (locations.Count == 1)
        {
            effectiveLocationId = locations[0].Id;
        }
        else if (locations.Count > 1)
        {
            requiresBranchSelection = true;
        }

        var socialEntity = await _db.BusinessSocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BusinessRegistrationId == business.Id, cancellationToken)
            .ConfigureAwait(false);

        var social = socialEntity is null
            ? new BusinessSocialProfileDto()
            : BusinessSocialProfileService.Map(socialEntity);

        if (requiresBranchSelection)
        {
            return new PublicStorefrontDto
            {
                BusinessId = business.Id,
                BusinessName = business.BusinessName,
                Slug = business.Slug!,
                LogoUrl = business.LogoPath,
                Theme = theme,
                Locations = locations,
                RequiresBranchSelection = true,
                Social = social,
            };
        }

        var roomQuery = _db.Rooms
            .AsNoTracking()
            .Include(r => r.Images)
            .Include(r => r.Location)
            .Where(r => r.BusinessRegistrationId == business.Id && !r.IsArchived);

        if (effectiveLocationId.HasValue)
        {
            roomQuery = roomQuery.Where(r => r.LocationId == effectiveLocationId);
        }

        var roomRows = await roomQuery
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rooms = roomRows
            .Select(r =>
            {
                var imageUrls = r.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
                    .ToList();
                return new PublicStorefrontRoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    BasePricePerNight = r.BasePricePerNight,
                    MaxOccupancy = r.MaxOccupancy,
                    PrimaryImageUrl = imageUrls.FirstOrDefault(),
                    ImageUrls = imageUrls,
                    LocationId = r.LocationId,
                    LocationName = r.Location?.Name,
                };
            })
            .ToList();

        var facilityQuery = _db.PropertyFacilities
            .AsNoTracking()
            .Include(f => f.Images)
            .Include(f => f.Location)
            .Where(f => f.BusinessRegistrationId == business.Id && !f.IsArchived);

        if (effectiveLocationId.HasValue)
        {
            facilityQuery = facilityQuery.Where(f => f.LocationId == effectiveLocationId);
        }

        var facilityRows = await facilityQuery
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var facilities = facilityRows
            .Select(f =>
            {
                var imageUrls = f.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
                    .ToList();
                return new PublicStorefrontFacilityDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    PrimaryImageUrl = imageUrls.FirstOrDefault(),
                    ImageUrls = imageUrls,
                    LocationId = f.LocationId,
                    LocationName = f.Location?.Name,
                };
            })
            .ToList();

        var bannerQuery = _db.StorefrontBannerImages
            .AsNoTracking()
            .Where(i => i.BusinessRegistrationId == business.Id);

        if (effectiveLocationId.HasValue)
        {
            bannerQuery = bannerQuery.Where(i => i.LocationId == effectiveLocationId);
        }

        var bannerImages = await bannerQuery
            .OrderBy(i => i.SortOrder)
            .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var aboutImagePath = await _db.StorefrontAboutImages
            .AsNoTracking()
            .Where(i => i.BusinessRegistrationId == business.Id)
            .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PublicStorefrontDto
        {
            BusinessId = business.Id,
            BusinessName = business.BusinessName,
            Slug = business.Slug!,
            LogoUrl = business.LogoPath,
            Theme = theme,
            Locations = locations,
            RequiresBranchSelection = false,
            ActiveLocationId = effectiveLocationId,
            Rooms = rooms,
            Facilities = facilities,
            Social = social,
            HeroImages = bannerImages,
            AboutImageUrl = aboutImagePath,
        };
    }

    private static void NormalizeTheme(StorefrontThemeDto theme)
    {
        theme.GlobalFont = TrimOrDefault(theme.GlobalFont, "modern-sans");

        theme.Banner.Style = TrimOrDefault(theme.Banner.Style, "grand-hero");
        theme.Banner.HeadlineFont = TrimOrDefault(theme.Banner.HeadlineFont, "display");
        theme.Banner.SubheadlineFont = TrimOrDefault(theme.Banner.SubheadlineFont, "body");
        theme.Banner.Headline = TrimOrDefault(theme.Banner.Headline, "Welcome");
        theme.Banner.Subheadline = TrimOrDefault(theme.Banner.Subheadline, string.Empty);
        theme.Banner.TextAlign = TrimOrDefault(theme.Banner.TextAlign, "center");
        theme.Banner.OverlayOpacity = Math.Clamp(theme.Banner.OverlayOpacity, 0, 90);
        theme.Banner.BadgeText = theme.Banner.BadgeText?.Trim() ?? "Your stay awaits";

        theme.Contact.Location = TrimOrDefault(theme.Contact.Location, string.Empty);
        theme.Contact.CheckIn = TrimOrDefault(theme.Contact.CheckIn, string.Empty);
        theme.Contact.CheckOut = TrimOrDefault(theme.Contact.CheckOut, string.Empty);
        theme.Contact.IntroText = TrimOrDefault(
            theme.Contact.IntroText,
            "Questions about your stay? Send us a message and our team will respond within a few hours.");

        theme.About.Eyebrow = TrimOrDefault(theme.About.Eyebrow, "About us");
        theme.About.Title = TrimOrDefault(theme.About.Title, "Our story");
        theme.About.Description = TrimOrDefault(theme.About.Description, string.Empty);
        theme.About.TitleFont = TrimOrDefault(theme.About.TitleFont, "display");
        theme.About.BodyFont = TrimOrDefault(theme.About.BodyFont, "body");
        theme.About.Layout = TrimOrDefault(theme.About.Layout, "side-by-side");
        theme.About.Quote = TrimOrDefault(theme.About.Quote, string.Empty);
        theme.About.QuoteBy = TrimOrDefault(theme.About.QuoteBy, string.Empty);
        theme.About.Stats = theme.About.Stats
            .Where(s => !string.IsNullOrWhiteSpace(s.Num) || !string.IsNullOrWhiteSpace(s.Label))
            .Take(4)
            .Select(s => new StorefrontAboutStatDto
            {
                Num = s.Num?.Trim() ?? string.Empty,
                Label = s.Label?.Trim() ?? string.Empty,
            })
            .ToList();

        theme.Rooms.Eyebrow = TrimOrDefault(theme.Rooms.Eyebrow, "Accommodations");
        theme.Rooms.Title = TrimOrDefault(theme.Rooms.Title, "Our rooms");
        theme.Rooms.Subtitle = TrimOrDefault(theme.Rooms.Subtitle, string.Empty);
        theme.Rooms.TitleFont = TrimOrDefault(theme.Rooms.TitleFont, "display");
        theme.Rooms.CardStyle = TrimOrDefault(theme.Rooms.CardStyle, "elevated");
        theme.Rooms.FeaturedEyebrow = TrimOrDefault(theme.Rooms.FeaturedEyebrow, "Signature Stay");
        theme.Rooms.FeaturedTitle = TrimOrDefault(theme.Rooms.FeaturedTitle, "Our most sought-after room");
        theme.Rooms.PolicyBreakfast = TrimOrDefault(theme.Rooms.PolicyBreakfast, string.Empty);
        theme.Rooms.PolicyPets = TrimOrDefault(theme.Rooms.PolicyPets, string.Empty);
        theme.Rooms.PolicyCancellation = TrimOrDefault(theme.Rooms.PolicyCancellation, string.Empty);
        theme.Rooms.CtaTitle = TrimOrDefault(theme.Rooms.CtaTitle, string.Empty);
        theme.Rooms.CtaSubtitle = TrimOrDefault(theme.Rooms.CtaSubtitle, string.Empty);
        theme.Rooms.CtaButtonText = TrimOrDefault(theme.Rooms.CtaButtonText, "Check availability");

        theme.Facilities.Eyebrow = TrimOrDefault(theme.Facilities.Eyebrow, "On Property");
        theme.Facilities.Title = TrimOrDefault(theme.Facilities.Title, "Facilities");
        theme.Facilities.Subtitle = TrimOrDefault(theme.Facilities.Subtitle, string.Empty);
        theme.Facilities.TitleFont = TrimOrDefault(theme.Facilities.TitleFont, "display");
        theme.Facilities.DisplayStyle = TrimOrDefault(theme.Facilities.DisplayStyle, "grid");
        theme.Facilities.SupportStatValue = TrimOrDefault(theme.Facilities.SupportStatValue, "24/7");
        theme.Facilities.SupportStatLabel = TrimOrDefault(theme.Facilities.SupportStatLabel, "Guest support");
        theme.Facilities.PerksEyebrow = TrimOrDefault(theme.Facilities.PerksEyebrow, "Guest Perks");
        theme.Facilities.PerksTitle = TrimOrDefault(theme.Facilities.PerksTitle, "Everything included in your stay");
        theme.Facilities.PerksSubtitle = TrimOrDefault(
            theme.Facilities.PerksSubtitle,
            "Complimentary access to most on-property amenities for all registered guests.");
        theme.Facilities.PerksItems = theme.Facilities.PerksItems
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Take(6)
            .ToList();

        theme.Footer.Style = TrimOrDefault(theme.Footer.Style, "columns");
        theme.Footer.Tagline = TrimOrDefault(theme.Footer.Tagline, string.Empty);
        theme.Footer.CopyrightText = TrimOrDefault(theme.Footer.CopyrightText, string.Empty);
        theme.Footer.BackgroundStyle = TrimOrDefault(theme.Footer.BackgroundStyle, "dark-band");

        theme.Colors.Preset = TrimOrDefault(theme.Colors.Preset, "sage-luxe");
        theme.Colors.Primary = TrimOrDefault(theme.Colors.Primary, "#5c7a5c");
        theme.Colors.Accent = TrimOrDefault(theme.Colors.Accent, "#c8dcc8");
        theme.Colors.Background = TrimOrDefault(theme.Colors.Background, "#faf9f6");
        theme.Colors.Text = TrimOrDefault(theme.Colors.Text, "#1f2a1f");
    }

    private static string TrimOrDefault(string? value, string fallback)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? fallback : trimmed;
    }
}
