namespace Admin.Infrastructure.Helpers;

/// <summary>Peak concurrent occupancy for room-type inventory (quantity &gt; 1).</summary>
public static class RoomBookingAvailability
{
    public static int GetPeakOccupancyInRange(
        IEnumerable<(DateOnly CheckIn, DateOnly CheckOut)> bookings,
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        if (rangeEnd <= rangeStart)
        {
            return 0;
        }

        var list = bookings as IList<(DateOnly CheckIn, DateOnly CheckOut)> ?? bookings.ToList();
        var max = 0;

        for (var day = rangeStart; day < rangeEnd; day = day.AddDays(1))
        {
            var count = 0;
            foreach (var (checkIn, checkOut) in list)
            {
                if (checkIn <= day && day < checkOut)
                {
                    count++;
                }
            }

            if (count > max)
            {
                max = count;
            }
        }

        return max;
    }

    public static bool WouldExceedCapacity(
        IEnumerable<(DateOnly CheckIn, DateOnly CheckOut)> existingBookings,
        DateOnly proposedCheckIn,
        DateOnly proposedCheckOut,
        int roomQuantity)
    {
        if (proposedCheckOut <= proposedCheckIn || roomQuantity < 1)
        {
            return true;
        }

        var all = existingBookings.ToList();
        all.Add((proposedCheckIn, proposedCheckOut));
        var peak = GetPeakOccupancyInRange(all, proposedCheckIn, proposedCheckOut);
        return peak > roomQuantity;
    }
}
