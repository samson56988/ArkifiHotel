using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessEventHallService
{
    Task<IReadOnlyList<EventHallSummaryDto>> ListAsync(
        Guid businessId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<EventHallDetailDto?> GetAsync(
        Guid businessId,
        Guid eventHallId,
        CancellationToken cancellationToken = default);

    Task<EventHallDetailDto?> CreateAsync(
        Guid businessId,
        CreateEventHallRequest request,
        CancellationToken cancellationToken = default);

    Task<EventHallDetailDto?> UpdateAsync(
        Guid businessId,
        Guid eventHallId,
        UpdateEventHallRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid businessId, Guid eventHallId, CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(Guid businessId, Guid eventHallId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid businessId, Guid eventHallId, CancellationToken cancellationToken = default);

    Task<EventHallDetailDto?> AddImageAsync(
        Guid businessId,
        Guid eventHallId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveImageAsync(
        Guid businessId,
        Guid eventHallId,
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventHallRequestListItemDto>> ListRequestsAsync(
        Guid businessId,
        string? status,
        CancellationToken cancellationToken = default);

    Task<EventHallRequestDetailDto?> GetRequestAsync(
        Guid businessId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<EventHallRequestDetailDto?> UpdateRequestStatusAsync(
        Guid businessId,
        Guid requestId,
        UpdateEventHallRequestStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicStorefrontEventHallDto>> GetPublicForLocationAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default);
}

public interface IPublicEventHallRequestService
{
    Task<(GuestEventHallRequestResultDto? Data, string? Error)> CreateRequestAsync(
        string slug,
        GuestCreateEventHallRequest request,
        CancellationToken cancellationToken = default);
}
