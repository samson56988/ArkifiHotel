using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/restaurant-menu")]
[Authorize(Roles = "Business")]
public sealed class BusinessRestaurantMenuController : ControllerBase
{
    private const long MaxUploadBytes = 8 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    private readonly IBusinessRestaurantMenuService _menu;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BusinessRestaurantMenuController> _logger;

    public BusinessRestaurantMenuController(
        IBusinessRestaurantMenuService menu,
        IWebHostEnvironment env,
        ILogger<BusinessRestaurantMenuController> logger)
    {
        _menu = menu;
        _env = env;
        _logger = logger;
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings([FromQuery] Guid locationId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuSettingsDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("Validation", "locationId is required."));
        }

        var dto = await _menu.GetSettingsAsync(businessId.Value, locationId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<RestaurantMenuSettingsDto>.Ok(dto ?? new RestaurantMenuSettingsDto { LocationId = locationId }));
    }

    [HttpPut("settings")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuSettingsDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings(
        [FromQuery] Guid locationId,
        [FromBody] UpdateRestaurantMenuSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuSettingsDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("Validation", "locationId is required."));
        }

        var dto = await _menu.UpdateSettingsAsync(businessId.Value, locationId, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("Validation", "Could not save settings."));
        }

        return Ok(ApiResult<RestaurantMenuSettingsDto>.Ok(MapSettingsUrls(dto)));
    }

    [HttpPost("settings/hero-image")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadHeroImage(
        [FromQuery] Guid locationId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuSettingsDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("Validation", "locationId is required."));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("Validation", "No file uploaded."));
        }

        if (!TryValidateImage(file, out var error))
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("Validation", error));
        }

        var relativePath = await SaveUploadAsync(businessId.Value, "hero", file, cancellationToken).ConfigureAwait(false);
        if (relativePath is null)
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("UploadFailed", "Could not save image."));
        }

        var dto = await _menu
            .UpsertHeroImageAsync(businessId.Value, locationId, relativePath, file.FileName, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return BadRequest(ApiResult<RestaurantMenuSettingsDto>.Fail("UploadFailed", "Could not register image."));
        }

        return Ok(ApiResult<RestaurantMenuSettingsDto>.Ok(MapSettingsUrls(dto)));
    }

    [HttpDelete("settings/hero-image")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteHeroImage([FromQuery] Guid locationId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult.Fail("Validation", "locationId is required."));
        }

        await _menu.RemoveHeroImageAsync(businessId.Value, locationId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult.Ok("Hero image removed."));
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RestaurantMenuCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCategories(
        [FromQuery] Guid locationId,
        [FromQuery] string? section,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(
                ApiResult<IReadOnlyList<RestaurantMenuCategoryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(
                ApiResult<IReadOnlyList<RestaurantMenuCategoryDto>>.Fail("Validation", "locationId is required."));
        }

        var list = await _menu
            .ListCategoriesAsync(businessId.Value, locationId, section, includeArchived, cancellationToken)
            .ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<RestaurantMenuCategoryDto>>.Ok(list));
    }

    [HttpPost("categories")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCategory(
        [FromQuery] Guid locationId,
        [FromBody] CreateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuCategoryDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuCategoryDto>.Fail("Validation", "locationId is required."));
        }

        var dto = await _menu.CreateCategoryAsync(businessId.Value, locationId, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return BadRequest(ApiResult<RestaurantMenuCategoryDto>.Fail("Validation", "Could not create category."));
        }

        return Ok(ApiResult<RestaurantMenuCategoryDto>.Ok(dto));
    }

    [HttpPut("categories/{categoryId:guid}")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCategory(
        [FromQuery] Guid locationId,
        Guid categoryId,
        [FromBody] UpdateRestaurantMenuCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuCategoryDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuCategoryDto>.Fail("Validation", "locationId is required."));
        }

        var dto = await _menu
            .UpdateCategoryAsync(businessId.Value, locationId, categoryId, request, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return NotFound(ApiResult<RestaurantMenuCategoryDto>.Fail("NotFound", "Category not found."));
        }

        return Ok(ApiResult<RestaurantMenuCategoryDto>.Ok(dto));
    }

    [HttpPost("categories/{categoryId:guid}/archive")]
    public async Task<IActionResult> ArchiveCategory(
        [FromQuery] Guid locationId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult.Fail("Validation", "locationId is required."));
        }

        var ok = await _menu.ArchiveCategoryAsync(businessId.Value, locationId, categoryId, cancellationToken).ConfigureAwait(false);
        return ok
            ? Ok(ApiResult.Ok("Category archived."))
            : NotFound(ApiResult.Fail("NotFound", "Category not found."));
    }

    [HttpPost("categories/{categoryId:guid}/restore")]
    public async Task<IActionResult> RestoreCategory(
        [FromQuery] Guid locationId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult.Fail("Validation", "locationId is required."));
        }

        var ok = await _menu.RestoreCategoryAsync(businessId.Value, locationId, categoryId, cancellationToken).ConfigureAwait(false);
        return ok
            ? Ok(ApiResult.Ok("Category restored."))
            : NotFound(ApiResult.Fail("NotFound", "Category not found."));
    }

    [HttpGet("categories/{categoryId:guid}/items")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RestaurantMenuItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListItems(
        [FromQuery] Guid locationId,
        Guid categoryId,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(
                ApiResult<IReadOnlyList<RestaurantMenuItemDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(
                ApiResult<IReadOnlyList<RestaurantMenuItemDto>>.Fail("Validation", "locationId is required."));
        }

        var list = await _menu
            .ListItemsAsync(businessId.Value, locationId, categoryId, includeArchived, cancellationToken)
            .ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<RestaurantMenuItemDto>>.Ok(MapItemUrls(list)));
    }

    [HttpPost("categories/{categoryId:guid}/items")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateItem(
        [FromQuery] Guid locationId,
        Guid categoryId,
        [FromBody] CreateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuItemDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("Validation", "locationId is required."));
        }

        var dto = await _menu
            .CreateItemAsync(businessId.Value, locationId, categoryId, request, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("Validation", "Could not create item."));
        }

        return Ok(ApiResult<RestaurantMenuItemDto>.Ok(MapItemUrl(dto)));
    }

    [HttpPut("items/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateItem(
        [FromQuery] Guid locationId,
        Guid itemId,
        [FromBody] UpdateRestaurantMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuItemDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("Validation", "locationId is required."));
        }

        var dto = await _menu.UpdateItemAsync(businessId.Value, locationId, itemId, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<RestaurantMenuItemDto>.Fail("NotFound", "Item not found."));
        }

        return Ok(ApiResult<RestaurantMenuItemDto>.Ok(MapItemUrl(dto)));
    }

    [HttpPost("items/{itemId:guid}/archive")]
    public async Task<IActionResult> ArchiveItem(
        [FromQuery] Guid locationId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult.Fail("Validation", "locationId is required."));
        }

        var ok = await _menu.ArchiveItemAsync(businessId.Value, locationId, itemId, cancellationToken).ConfigureAwait(false);
        return ok
            ? Ok(ApiResult.Ok("Item archived."))
            : NotFound(ApiResult.Fail("NotFound", "Item not found."));
    }

    [HttpPost("items/{itemId:guid}/restore")]
    public async Task<IActionResult> RestoreItem(
        [FromQuery] Guid locationId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult.Fail("Validation", "locationId is required."));
        }

        var ok = await _menu.RestoreItemAsync(businessId.Value, locationId, itemId, cancellationToken).ConfigureAwait(false);
        return ok
            ? Ok(ApiResult.Ok("Item restored."))
            : NotFound(ApiResult.Fail("NotFound", "Item not found."));
    }

    [HttpPost("items/{itemId:guid}/image")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadItemImage(
        [FromQuery] Guid locationId,
        Guid itemId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuItemDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("Validation", "locationId is required."));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("Validation", "No file uploaded."));
        }

        if (!TryValidateImage(file, out var error))
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("Validation", error));
        }

        var relativePath = await SaveUploadAsync(
            businessId.Value,
            Path.Combine("items", itemId.ToString("N")),
            file,
            cancellationToken).ConfigureAwait(false);

        if (relativePath is null)
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("UploadFailed", "Could not save image."));
        }

        var dto = await _menu
            .UpsertItemImageAsync(businessId.Value, locationId, itemId, relativePath, file.FileName, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return NotFound(ApiResult<RestaurantMenuItemDto>.Fail("NotFound", "Item not found."));
        }

        return Ok(ApiResult<RestaurantMenuItemDto>.Ok(MapItemUrl(dto)));
    }

    [HttpPut("items/{itemId:guid}/availability")]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<RestaurantMenuItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetItemAvailability(
        [FromQuery] Guid locationId,
        Guid itemId,
        [FromBody] SetRestaurantMenuItemAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantMenuItemDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<RestaurantMenuItemDto>.Fail("Validation", "locationId is required."));
        }

        var dto = await _menu
            .SetItemAvailabilityAsync(businessId.Value, locationId, itemId, request.IsAvailable, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return NotFound(ApiResult<RestaurantMenuItemDto>.Fail("NotFound", "Item not found."));
        }

        return Ok(ApiResult<RestaurantMenuItemDto>.Ok(MapItemUrl(dto)));
    }

    [HttpDelete("items/{itemId:guid}/image")]
    public async Task<IActionResult> DeleteItemImage(
        [FromQuery] Guid locationId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult.Fail("Validation", "locationId is required."));
        }

        await _menu.RemoveItemImageAsync(businessId.Value, locationId, itemId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult.Ok("Item image removed."));
    }

    private bool TryValidateImage(IFormFile file, out string error)
    {
        error = string.Empty;
        if (file.Length > MaxUploadBytes)
        {
            error = "Image must be 8 MB or smaller.";
            return false;
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            error = "Only JPEG or PNG images are allowed.";
            return false;
        }

        return true;
    }

    private async Task<string?> SaveUploadAsync(
        Guid businessId,
        string subFolder,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = Path.Combine("uploads", businessId.ToString("N"), "restaurant", subFolder)
            .Replace('\\', '/');
        var physicalDir = Path.Combine(webRoot, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(physicalDir);

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || ext.Length > 10)
        {
            ext = file.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => ".bin",
            };
        }

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(physicalDir, fileName);

        try
        {
            await using var stream = System.IO.File.Create(physicalPath);
            await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            return $"{relativeFolder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed saving restaurant image for business {BusinessId}", businessId);
            return null;
        }
    }

    private RestaurantMenuSettingsDto MapSettingsUrls(RestaurantMenuSettingsDto dto)
    {
        dto.HeroImageUrl = ToAbsoluteUrl(dto.HeroImageUrl);
        return dto;
    }

    private RestaurantMenuItemDto MapItemUrl(RestaurantMenuItemDto dto)
    {
        dto.ImageUrl = ToAbsoluteUrl(dto.ImageUrl);
        return dto;
    }

    private IReadOnlyList<RestaurantMenuItemDto> MapItemUrls(IReadOnlyList<RestaurantMenuItemDto> list) =>
        list.Select(MapItemUrl).ToList();

    private string? ToAbsoluteUrl(string? urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath))
        {
            return null;
        }

        if (urlOrPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || urlOrPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return urlOrPath;
        }

        var path = urlOrPath.StartsWith('/') ? urlOrPath : "/" + urlOrPath;
        return $"{Request.Scheme}://{Request.Host}{path}";
    }
}
