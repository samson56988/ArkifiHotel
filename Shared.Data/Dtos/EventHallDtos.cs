namespace Shared.Data.Dtos;

public sealed class EventHallSummaryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal RentalPrice { get; set; }

    public int? MaxCapacity { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public int ImageCount { get; set; }

    public Guid LocationId { get; set; }

    public string? LocationName { get; set; }

    public bool IsArchived { get; set; }
}

public sealed class EventHallDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal RentalPrice { get; set; }

    public int? MaxCapacity { get; set; }

    public Guid LocationId { get; set; }

    public string? LocationName { get; set; }

    public bool IsArchived { get; set; }

    public IReadOnlyList<EventHallImageDto> Images { get; set; } = Array.Empty<EventHallImageDto>();
}

public sealed class EventHallImageDto
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public int SortOrder { get; set; }
}

public sealed class CreateEventHallRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal RentalPrice { get; set; }

    public int? MaxCapacity { get; set; }

    public Guid LocationId { get; set; }
}

public sealed class UpdateEventHallRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal RentalPrice { get; set; }

    public int? MaxCapacity { get; set; }

    public Guid LocationId { get; set; }
}

public sealed class EventHallRequestListItemDto
{
    public Guid Id { get; set; }

    public string EventHallName { get; set; } = string.Empty;

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string GuestPhone { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public DateOnly? EventEndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? LocationName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class EventHallRequestDetailDto
{
    public Guid Id { get; set; }

    public Guid EventHallId { get; set; }

    public string EventHallName { get; set; } = string.Empty;

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string GuestPhone { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public DateOnly? EventEndDate { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? LocationName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class UpdateEventHallRequestStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class GuestCreateEventHallRequest
{
    public Guid LocationId { get; set; }

    public Guid EventHallId { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string GuestPhone { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public DateOnly? EventEndDate { get; set; }

    public string? Notes { get; set; }
}

public sealed class GuestEventHallRequestResultDto
{
    public Guid RequestId { get; set; }

    public string Status { get; set; } = "Pending";

    public string Message { get; set; } = string.Empty;
}

public sealed class PublicStorefrontEventHallDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal RentalPrice { get; set; }

    public int? MaxCapacity { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public IReadOnlyList<string> ImageUrls { get; set; } = Array.Empty<string>();
}
