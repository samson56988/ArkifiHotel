namespace Shared.Data.Helpers;

public static class RoomPricingHelper
{
    /// <summary>
    /// Nightly rate for stays under 7 nights. For 7+ nights, full weeks use the weekly rate when set; leftover nights use nightly.
    /// </summary>
    public static decimal CalculateStayTotal(decimal nightlyRate, decimal? weeklyRate, int nights)
    {
        if (nights <= 0)
        {
            return 0m;
        }

        if (weeklyRate is null or <= 0 || nights < 7)
        {
            return decimal.Round(nightlyRate * nights, 2, MidpointRounding.AwayFromZero);
        }

        var fullWeeks = nights / 7;
        var extraNights = nights % 7;
        var total = fullWeeks * weeklyRate.Value + extraNights * nightlyRate;
        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
