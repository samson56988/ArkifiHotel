import type { ApiResult } from './api-result.model';

export type BookingStayPhase = 'All' | 'Active' | 'Closed';

export interface PagedResultDto<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ListBookingsParams {
  page?: number;
  pageSize?: number;
  checkInFrom?: string | null;
  checkInTo?: string | null;
  checkOutFrom?: string | null;
  checkOutTo?: string | null;
  stayPhase?: BookingStayPhase | null;
  status?: string | null;
}

export interface BookingSummaryDto {
  id: string;
  roomId: string;
  roomName: string;
  locationId: string | null;
  locationName: string | null;
  guestName: string;
  guestEmail: string;
  checkInDate: string;
  checkOutDate: string;
  nights: number;
  status: string;
  totalAmount: number;
  currency: string;
  confirmationCode: string;
  createdAt: string;
  isStayClosed: boolean;
}

export interface BookingDetailDto {
  id: string;
  roomId: string;
  roomName: string;
  locationId: string | null;
  locationName: string | null;
  guestName: string;
  guestEmail: string;
  guestPhone: string | null;
  checkInDate: string;
  checkOutDate: string;
  nights: number;
  status: string;
  totalAmount: number;
  currency: string;
  confirmationCode: string;
  internalNotes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateBookingRequest {
  locationId: string;
  roomId: string;
  guestName: string;
  guestEmail: string;
  guestPhone: string;
  checkInDate: string;
  checkOutDate: string;
  internalNotes?: string | null;
  paymentMethod: string;
  paymentConfirmed: boolean;
}

export interface RoomAvailabilityDto {
  roomId: string;
  roomName: string;
  totalQuantity: number;
  peakBooked: number;
  availableUnits: number;
  isAvailable: boolean;
  basePricePerNight: number;
  basePricePerWeek?: number | null;
  maxOccupancy: number;
  locationId: string | null;
  locationName: string | null;
}

export interface UpdateBookingStatusRequest {
  status: string;
}

export type BookingsListApiResponse = ApiResult<PagedResultDto<BookingSummaryDto>>;
export type BookingDetailApiResponse = ApiResult<BookingDetailDto>;
export type RoomAvailabilityApiResponse = ApiResult<RoomAvailabilityDto[]>;
