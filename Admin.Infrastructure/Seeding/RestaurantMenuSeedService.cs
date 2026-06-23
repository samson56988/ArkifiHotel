using System.Net.Http.Headers;
using System.Text.Json;
using Admin.Data;
using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Seeding;

/// <summary>Seeds demo restaurant menus (with images) for businesses that have no menu yet.</summary>
public sealed class RestaurantMenuSeedService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AdminDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RestaurantMenuSeedService> _logger;

    public RestaurantMenuSeedService(
        AdminDbContext db,
        IHttpClientFactory httpClientFactory,
        ILogger<RestaurantMenuSeedService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SeedMissingMenusAsync(string webRootPath, CancellationToken cancellationToken = default)
    {
        var locations = await (
            from l in _db.BusinessLocations.AsNoTracking()
            join b in _db.BusinessRegistrations.AsNoTracking() on l.BusinessRegistrationId equals b.Id
            select new { l.Id, l.BusinessRegistrationId, BusinessName = b.BusinessName })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existing = await _db.RestaurantMenuSettings
            .AsNoTracking()
            .Select(s => new { s.BusinessRegistrationId, s.LocationId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingSet = existing.Select(e => (e.BusinessRegistrationId, e.LocationId)).ToHashSet();
        var toSeed = locations.Where(l => !existingSet.Contains((l.BusinessRegistrationId, l.Id))).ToList();

        if (toSeed.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Seeding restaurant menus for {Count} branch(es).", toSeed.Count);

        foreach (var location in toSeed)
        {
            await SeedLocationAsync(location.BusinessRegistrationId, location.Id, location.BusinessName, webRootPath, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task SeedLocationAsync(
        Guid businessId,
        Guid locationId,
        string businessName,
        string webRootPath,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var settings = new RestaurantMenuSettings
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            LocationId = locationId,
            Enabled = true,
            NavLabel = "Restaurant & menu",
            HeroEyebrow = "Dining",
            HeroTitle = string.IsNullOrWhiteSpace(businessName)
                ? "Restaurant & bar"
                : $"{businessName} Restaurant",
            HeroSubtitle =
                "Nigerian soul food, continental classics, and a curated drinks list — served daily.",
            MealsSectionTitle = "Meals",
            DrinksSectionTitle = "Drinks",
            CreatedAt = now,
        };

        settings.HeroImageRelativePath = await DownloadImageAsync(
            businessId,
            "hero",
            RestaurantMenuSeedData.HeroImageUrl,
            webRootPath,
            cancellationToken).ConfigureAwait(false);

        if (settings.HeroImageRelativePath is not null)
        {
            settings.HeroImageOriginalFileName = "hero.jpg";
        }

        _db.RestaurantMenuSettings.Add(settings);

        foreach (var seedCategory in RestaurantMenuSeedData.Categories)
        {
            var category = new RestaurantMenuCategory
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = businessId,
                LocationId = locationId,
                Name = seedCategory.Name,
                Section = seedCategory.Section,
                SortOrder = seedCategory.SortOrder,
                CreatedAt = now,
            };
            _db.RestaurantMenuCategories.Add(category);

            var itemOrder = 0;
            foreach (var seedItem in seedCategory.Items)
            {
                var item = new RestaurantMenuItem
                {
                    Id = Guid.NewGuid(),
                    CategoryId = category.Id,
                    Name = seedItem.Name,
                    Description = seedItem.Description,
                    Price = seedItem.Price,
                    TagsJson = JsonSerializer.Serialize(seedItem.Tags, JsonOptions),
                    SortOrder = itemOrder++,
                    CreatedAt = now,
                };

                if (!string.IsNullOrWhiteSpace(seedItem.ImageUrl))
                {
                    item.ImageRelativePath = await DownloadImageAsync(
                        businessId,
                        item.Id.ToString("N"),
                        seedItem.ImageUrl!,
                        webRootPath,
                        cancellationToken).ConfigureAwait(false);

                    if (item.ImageRelativePath is not null)
                    {
                        item.ImageOriginalFileName = "menu-item.jpg";
                    }
                }

                _db.RestaurantMenuItems.Add(item);
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Seeded restaurant menu for business {BusinessId} location {LocationId}.", businessId, locationId);
    }

    private async Task<string?> DownloadImageAsync(
        Guid businessId,
        string fileKey,
        string url,
        string webRootPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(nameof(RestaurantMenuSeedService));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ArkifiHotel", "1.0"));

            using var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            if (bytes.Length == 0)
            {
                return null;
            }

            var relativeDir = Path.Combine(
                "uploads",
                businessId.ToString("N"),
                "restaurant",
                fileKey);
            var absoluteDir = Path.Combine(webRootPath, relativeDir);
            Directory.CreateDirectory(absoluteDir);

            var fileName = $"{Guid.NewGuid():N}.jpg";
            var absolutePath = Path.Combine(absoluteDir, fileName);
            await File.WriteAllBytesAsync(absolutePath, bytes, cancellationToken).ConfigureAwait(false);

            return Path.Combine(relativeDir, fileName).Replace('\\', '/');
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not download seed image from {Url}", url);
            return null;
        }
    }
}
