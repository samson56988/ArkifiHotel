export interface GuestRoomAvailabilityDto {
  roomId: string;
  roomName: string;
  totalQuantity: number;
  peakBooked: number;
  availableUnits: number;
  isAvailable: boolean;
  basePricePerNight: number;
  maxOccupancy: number;
  locationId?: string | null;
  locationName?: string | null;
}
