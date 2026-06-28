/** Stay total: nightly for under 7 nights; weekly packages plus nightly remainder when weekly rate is set. */
export function calculateStayTotal(
  nightlyRate: number,
  weeklyRate: number | null | undefined,
  nights: number,
): number {
  if (nights <= 0) {
    return 0;
  }
  if (!weeklyRate || weeklyRate <= 0 || nights < 7) {
    return nightlyRate * nights;
  }
  const fullWeeks = Math.floor(nights / 7);
  const extraNights = nights % 7;
  return fullWeeks * weeklyRate + extraNights * nightlyRate;
}

export function hasWeeklyRate(weeklyRate: number | null | undefined): weeklyRate is number {
  return weeklyRate != null && weeklyRate > 0;
}
