using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessRestaurantMenuService
{
    Task<RestaurantMenuSettingsDto?> GetSettingsAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuSettingsDto?> UpdateSettingsAsync(
        Guid businessId,
        Guid locationId,
        UpdateRestaurantMenuSettingsRequest request,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuSettingsDto?> UpsertHeroImageAsync(
        Guid businessId,
        Guid locationId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveHeroImageAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RestaurantMenuCategoryDto>> ListCategoriesAsync(
        Guid businessId,
        Guid locationId,
        string? section,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuCategoryDto?> CreateCategoryAsync(
        Guid businessId,
        Guid locationId,
        CreateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuCategoryDto?> UpdateCategoryAsync(
        Guid businessId,
        Guid locationId,
        Guid categoryId,
        UpdateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveCategoryAsync(
        Guid businessId,
        Guid locationId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> RestoreCategoryAsync(
        Guid businessId,
        Guid locationId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RestaurantMenuItemDto>> ListItemsAsync(
        Guid businessId,
        Guid locationId,
        Guid categoryId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> CreateItemAsync(
        Guid businessId,
        Guid locationId,
        Guid categoryId,
        CreateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> UpdateItemAsync(
        Guid businessId,
        Guid locationId,
        Guid itemId,
        UpdateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveItemAsync(
        Guid businessId,
        Guid locationId,
        Guid itemId,
        CancellationToken cancellationToken = default);

    Task<bool> RestoreItemAsync(
        Guid businessId,
        Guid locationId,
        Guid itemId,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> UpsertItemImageAsync(
        Guid businessId,
        Guid locationId,
        Guid itemId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemImageAsync(
        Guid businessId,
        Guid locationId,
        Guid itemId,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> SetItemAvailabilityAsync(
        Guid businessId,
        Guid locationId,
        Guid itemId,
        bool isAvailable,
        CancellationToken cancellationToken = default);

    Task<PublicStorefrontRestaurantDto?> GetPublicMenuAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default);
}
