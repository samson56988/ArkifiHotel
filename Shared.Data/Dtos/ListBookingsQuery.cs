namespace Shared.Data.Dtos;

public sealed class ListBookingsQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public DateOnly? CheckInFrom { get; set; }

    public DateOnly? CheckInTo { get; set; }

    public DateOnly? CheckOutFrom { get; set; }

    public DateOnly? CheckOutTo { get; set; }

    /// <summary>All, Active, or Closed (checkout date has passed or is today).</summary>
    public string? StayPhase { get; set; }

    public string? Status { get; set; }
}
