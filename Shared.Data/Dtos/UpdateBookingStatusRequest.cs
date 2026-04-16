namespace Shared.Data.Dtos;

public sealed class UpdateBookingStatusRequest
{
    /// <summary>Pending, Confirmed, Cancelled, or Completed.</summary>
    public string Status { get; set; } = string.Empty;
}
