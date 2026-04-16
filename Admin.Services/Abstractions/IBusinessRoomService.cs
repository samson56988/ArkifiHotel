using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessRoomService
{
    Task<IReadOnlyList<BusinessRoomSummaryDto>> ListAsync(
        Guid businessId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<BusinessRoomDetailDto?> GetAsync(Guid businessId, Guid roomId, CancellationToken cancellationToken = default);

    Task<BusinessRoomDetailDto?> CreateAsync(
        Guid businessId,
        CreateBusinessRoomRequest request,
        CancellationToken cancellationToken = default);

    Task<BusinessRoomDetailDto?> UpdateAsync(
        Guid businessId,
        Guid roomId,
        UpdateBusinessRoomRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid businessId, Guid roomId, CancellationToken cancellationToken = default);

    Task<bool> SetArchivedAsync(
        Guid businessId,
        Guid roomId,
        bool archived,
        CancellationToken cancellationToken = default);

    Task<RoomImageDto?> AddImageAsync(
        Guid businessId,
        Guid roomId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(
        Guid businessId,
        Guid roomId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}
