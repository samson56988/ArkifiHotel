using System.Text.Json;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessRestaurantMenuService : IBusinessRestaurantMenuService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AdminDbContext _db;

    public BusinessRestaurantMenuService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<RestaurantMenuSettingsDto?> GetSettingsAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.RestaurantMenuSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return row is null ? null : MapSettings(row);
    }

    public async Task<RestaurantMenuSettingsDto?> UpdateSettingsAsync(
        Guid businessId,
        UpdateRestaurantMenuSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await BusinessExistsAsync(businessId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var row = await _db.RestaurantMenuSettings
            .FirstOrDefaultAsync(s => s.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
        {
            row = new RestaurantMenuSettings
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = businessId,
                CreatedAt = now,
            };
            _db.RestaurantMenuSettings.Add(row);
        }

        row.Enabled = request.Enabled;
        row.NavLabel = TrimRequired(request.NavLabel, "Restaurant & menu", 120);
        row.HeroEyebrow = TrimRequired(request.HeroEyebrow, "Dining", 120);
        row.HeroTitle = TrimRequired(request.HeroTitle, "Restaurant & bar", 200);
        row.HeroSubtitle = TrimOptional(request.HeroSubtitle, 1000);
        row.MealsSectionTitle = TrimRequired(request.MealsSectionTitle, "Meals", 120);
        row.DrinksSectionTitle = TrimRequired(request.DrinksSectionTitle, "Drinks", 120);
        row.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapSettings(row);
    }

    public async Task<RestaurantMenuSettingsDto?> UpsertHeroImageAsync(
        Guid businessId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        var row = await EnsureSettingsAsync(businessId, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            return null;
        }

        row.HeroImageRelativePath = relativePath;
        row.HeroImageOriginalFileName = originalFileName;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapSettings(row);
    }

    public async Task<bool> RemoveHeroImageAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var row = await _db.RestaurantMenuSettings
            .FirstOrDefaultAsync(s => s.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (row is null || string.IsNullOrWhiteSpace(row.HeroImageRelativePath))
        {
            return false;
        }

        row.HeroImageRelativePath = null;
        row.HeroImageOriginalFileName = null;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<RestaurantMenuCategoryDto>> ListCategoriesAsync(
        Guid businessId,
        string? section,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.RestaurantMenuCategories
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => c.BusinessRegistrationId == businessId);

        if (!includeArchived)
        {
            query = query.Where(c => !c.IsArchived);
        }

        if (TryParseSection(section, out var parsed))
        {
            query = query.Where(c => c.Section == parsed);
        }

        var rows = await query
            .OrderBy(c => c.Section)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(c => MapCategory(c, c.Items.Count(i => !i.IsArchived)))
            .ToList();
    }

    public async Task<RestaurantMenuCategoryDto?> CreateCategoryAsync(
        Guid businessId,
        CreateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateCategoryName(request.Name, out var name) ||
            !TryParseSectionRequired(request.Section, out var section))
        {
            return null;
        }

        if (!await BusinessExistsAsync(businessId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var entity = new RestaurantMenuCategory
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            Name = name,
            Section = section,
            SortOrder = request.SortOrder,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.RestaurantMenuCategories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapCategory(entity, 0);
    }

    public async Task<RestaurantMenuCategoryDto?> UpdateCategoryAsync(
        Guid businessId,
        Guid categoryId,
        UpdateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateCategoryName(request.Name, out var name))
        {
            return null;
        }

        var entity = await _db.RestaurantMenuCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        entity.Name = name;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var count = entity.Items.Count(i => !i.IsArchived);
        return MapCategory(entity, count);
    }

    public async Task<bool> ArchiveCategoryAsync(
        Guid businessId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindCategoryAsync(businessId, categoryId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return false;
        }

        entity.IsArchived = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> RestoreCategoryAsync(
        Guid businessId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindCategoryAsync(businessId, categoryId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return false;
        }

        entity.IsArchived = false;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<RestaurantMenuItemDto>> ListItemsAsync(
        Guid businessId,
        Guid categoryId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        if (!await CategoryOwnedAsync(businessId, categoryId, cancellationToken).ConfigureAwait(false))
        {
            return Array.Empty<RestaurantMenuItemDto>();
        }

        var query = _db.RestaurantMenuItems
            .AsNoTracking()
            .Where(i => i.CategoryId == categoryId);

        if (!includeArchived)
        {
            query = query.Where(i => !i.IsArchived);
        }

        var rows = await query
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(MapItem).ToList();
    }

    public async Task<RestaurantMenuItemDto?> CreateItemAsync(
        Guid businessId,
        Guid categoryId,
        CreateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateItemName(request.Name, out var name) || !ValidatePrice(request.Price))
        {
            return null;
        }

        if (!await CategoryOwnedAsync(businessId, categoryId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var entity = new RestaurantMenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Name = name,
            Description = TrimOptional(request.Description, 2000),
            Price = request.Price,
            TagsJson = SerializeTags(request.Tags),
            SortOrder = request.SortOrder,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.RestaurantMenuItems.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapItem(entity);
    }

    public async Task<RestaurantMenuItemDto?> UpdateItemAsync(
        Guid businessId,
        Guid itemId,
        UpdateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateItemName(request.Name, out var name) || !ValidatePrice(request.Price))
        {
            return null;
        }

        var entity = await FindItemAsync(businessId, itemId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return null;
        }

        entity.Name = name;
        entity.Description = TrimOptional(request.Description, 2000);
        entity.Price = request.Price;
        entity.TagsJson = SerializeTags(request.Tags);
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapItem(entity);
    }

    public async Task<bool> ArchiveItemAsync(
        Guid businessId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindItemAsync(businessId, itemId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return false;
        }

        entity.IsArchived = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> RestoreItemAsync(
        Guid businessId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindItemAsync(businessId, itemId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return false;
        }

        entity.IsArchived = false;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<RestaurantMenuItemDto?> UpsertItemImageAsync(
        Guid businessId,
        Guid itemId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindItemAsync(businessId, itemId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return null;
        }

        entity.ImageRelativePath = relativePath;
        entity.ImageOriginalFileName = originalFileName;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapItem(entity);
    }

    public async Task<bool> RemoveItemImageAsync(
        Guid businessId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindItemAsync(businessId, itemId, cancellationToken).ConfigureAwait(false);
        if (entity is null || string.IsNullOrWhiteSpace(entity.ImageRelativePath))
        {
            return false;
        }

        entity.ImageRelativePath = null;
        entity.ImageOriginalFileName = null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<RestaurantMenuItemDto?> SetItemAvailabilityAsync(
        Guid businessId,
        Guid itemId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindItemAsync(businessId, itemId, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.IsArchived)
        {
            return null;
        }

        entity.IsAvailable = isAvailable;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapItem(entity);
    }

    public async Task<PublicStorefrontRestaurantDto?> GetPublicMenuAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _db.RestaurantMenuSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (settings is null || !settings.Enabled)
        {
            return null;
        }

        var categoryRows = await _db.RestaurantMenuCategories
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => c.BusinessRegistrationId == businessId && !c.IsArchived)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PublicStorefrontRestaurantDto
        {
            Enabled = true,
            NavLabel = settings.NavLabel,
            HeroEyebrow = settings.HeroEyebrow,
            HeroTitle = settings.HeroTitle,
            HeroSubtitle = settings.HeroSubtitle,
            MealsSectionTitle = settings.MealsSectionTitle,
            DrinksSectionTitle = settings.DrinksSectionTitle,
            HeroImageUrl = ToPublicUrl(settings.HeroImageRelativePath),
            FoodCategories = categoryRows
                .Where(c => c.Section == RestaurantMenuSection.Food)
                .Select(c => MapPublicCategory(c.Id, c.Name, c.Items.Where(i => !i.IsArchived && i.IsAvailable).ToList()))
                .Where(c => c.Items.Count > 0)
                .ToList(),
            DrinkCategories = categoryRows
                .Where(c => c.Section == RestaurantMenuSection.Drink)
                .Select(c => MapPublicCategory(c.Id, c.Name, c.Items.Where(i => !i.IsArchived && i.IsAvailable).ToList()))
                .Where(c => c.Items.Count > 0)
                .ToList(),
        };
    }

    private async Task<RestaurantMenuSettings?> EnsureSettingsAsync(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        if (!await BusinessExistsAsync(businessId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var row = await _db.RestaurantMenuSettings
            .FirstOrDefaultAsync(s => s.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (row is not null)
        {
            return row;
        }

        row = new RestaurantMenuSettings
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.RestaurantMenuSettings.Add(row);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return row;
    }

    private async Task<RestaurantMenuCategory?> FindCategoryAsync(
        Guid businessId,
        Guid categoryId,
        CancellationToken cancellationToken) =>
        await _db.RestaurantMenuCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

    private async Task<RestaurantMenuItem?> FindItemAsync(
        Guid businessId,
        Guid itemId,
        CancellationToken cancellationToken) =>
        await _db.RestaurantMenuItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(
                i => i.Id == itemId && i.Category.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

    private async Task<bool> CategoryOwnedAsync(
        Guid businessId,
        Guid categoryId,
        CancellationToken cancellationToken) =>
        await _db.RestaurantMenuCategories
            .AnyAsync(c => c.Id == categoryId && c.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

    private async Task<bool> BusinessExistsAsync(Guid businessId, CancellationToken cancellationToken) =>
        await _db.BusinessRegistrations.AnyAsync(b => b.Id == businessId, cancellationToken).ConfigureAwait(false);

    private static RestaurantMenuSettingsDto MapSettings(RestaurantMenuSettings row) =>
        new()
        {
            Enabled = row.Enabled,
            NavLabel = row.NavLabel,
            HeroEyebrow = row.HeroEyebrow,
            HeroTitle = row.HeroTitle,
            HeroSubtitle = row.HeroSubtitle,
            MealsSectionTitle = row.MealsSectionTitle,
            DrinksSectionTitle = row.DrinksSectionTitle,
            HeroImageUrl = ToPublicUrl(row.HeroImageRelativePath),
        };

    private static RestaurantMenuCategoryDto MapCategory(RestaurantMenuCategory row, int itemCount) =>
        new()
        {
            Id = row.Id,
            Name = row.Name,
            Section = row.Section == RestaurantMenuSection.Drink ? "drink" : "food",
            SortOrder = row.SortOrder,
            IsArchived = row.IsArchived,
            ItemCount = itemCount,
        };

    private static RestaurantMenuItemDto MapItem(RestaurantMenuItem row) =>
        new()
        {
            Id = row.Id,
            CategoryId = row.CategoryId,
            Name = row.Name,
            Description = row.Description,
            Price = row.Price,
            Tags = DeserializeTags(row.TagsJson),
            ImageUrl = ToPublicUrl(row.ImageRelativePath),
            SortOrder = row.SortOrder,
            IsArchived = row.IsArchived,
            IsAvailable = row.IsAvailable,
        };

    private static PublicStorefrontMenuCategoryDto MapPublicCategory(
        Guid id,
        string name,
        IReadOnlyList<RestaurantMenuItem> items) =>
        new()
        {
            Id = id,
            Name = name,
            Items = items
                .Select(i => new PublicStorefrontMenuItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description ?? string.Empty,
                    Price = i.Price,
                    Tags = DeserializeTags(i.TagsJson),
                    ImageUrl = ToPublicUrl(i.ImageRelativePath),
                })
                .ToList(),
        };

    private static string? ToPublicUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        return "/" + relativePath.Replace("\\", "/", StringComparison.Ordinal);
    }

    private static string SerializeTags(IReadOnlyList<string>? tags)
    {
        var cleaned = (tags ?? Array.Empty<string>())
            .Select(t => t?.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Take(8)
            .ToList();

        return JsonSerializer.Serialize(cleaned, JsonOptions);
    }

    private static IReadOnlyList<string> DeserializeTags(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static bool TryParseSection(string? section, out RestaurantMenuSection parsed)
    {
        parsed = RestaurantMenuSection.Food;
        if (string.IsNullOrWhiteSpace(section))
        {
            return false;
        }

        return TryParseSectionRequired(section, out parsed);
    }

    private static bool TryParseSectionRequired(string section, out RestaurantMenuSection parsed)
    {
        parsed = RestaurantMenuSection.Food;
        if (string.Equals(section, "drink", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(section, "drinks", StringComparison.OrdinalIgnoreCase))
        {
            parsed = RestaurantMenuSection.Drink;
            return true;
        }

        if (string.Equals(section, "food", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(section, "meals", StringComparison.OrdinalIgnoreCase))
        {
            parsed = RestaurantMenuSection.Food;
            return true;
        }

        return false;
    }

    private static bool ValidateCategoryName(string? name, out string trimmed)
    {
        trimmed = name?.Trim() ?? string.Empty;
        return trimmed.Length >= 2 && trimmed.Length <= 200;
    }

    private static bool ValidateItemName(string? name, out string trimmed)
    {
        trimmed = name?.Trim() ?? string.Empty;
        return trimmed.Length >= 2 && trimmed.Length <= 200;
    }

    private static bool ValidatePrice(decimal price) => price >= 0 && price <= 99_999_999;

    private static string TrimRequired(string value, string fallback, int maxLen)
    {
        var t = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return t.Length <= maxLen ? t : t[..maxLen];
    }

    private static string TrimOptional(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var t = value.Trim();
        return t.Length <= maxLen ? t : t[..maxLen];
    }
}
