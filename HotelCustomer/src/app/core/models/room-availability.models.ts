export interface GuestRoomAvailabilityDto {
  roomId: string;
  roomName: string;
  totalQuantity: number;
  peakBooked: number;
  availableUnits: number;
  isAvailable: boolean;
  basePricePerNight: number;
  basePricePerWeek?: number | null;
  maxOccupancy: number;
  locationId?: string | null;
  locationName?: string | null;
}
