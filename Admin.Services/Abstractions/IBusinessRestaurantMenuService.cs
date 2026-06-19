using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessRestaurantMenuService
{
    Task<RestaurantMenuSettingsDto?> GetSettingsAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<RestaurantMenuSettingsDto?> UpdateSettingsAsync(
        Guid businessId,
        UpdateRestaurantMenuSettingsRequest request,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuSettingsDto?> UpsertHeroImageAsync(
        Guid businessId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveHeroImageAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RestaurantMenuCategoryDto>> ListCategoriesAsync(
        Guid businessId,
        string? section,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuCategoryDto?> CreateCategoryAsync(
        Guid businessId,
        CreateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuCategoryDto?> UpdateCategoryAsync(
        Guid businessId,
        Guid categoryId,
        UpdateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveCategoryAsync(Guid businessId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<bool> RestoreCategoryAsync(Guid businessId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RestaurantMenuItemDto>> ListItemsAsync(
        Guid businessId,
        Guid categoryId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> CreateItemAsync(
        Guid businessId,
        Guid categoryId,
        CreateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> UpdateItemAsync(
        Guid businessId,
        Guid itemId,
        UpdateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveItemAsync(Guid businessId, Guid itemId, CancellationToken cancellationToken = default);

    Task<bool> RestoreItemAsync(Guid businessId, Guid itemId, CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> UpsertItemImageAsync(
        Guid businessId,
        Guid itemId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemImageAsync(Guid businessId, Guid itemId, CancellationToken cancellationToken = default);

    Task<RestaurantMenuItemDto?> SetItemAvailabilityAsync(
        Guid businessId,
        Guid itemId,
        bool isAvailable,
        CancellationToken cancellationToken = default);

    Task<PublicStorefrontRestaurantDto?> GetPublicMenuAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);
}
