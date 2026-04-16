import type { ApiResult } from './api-result.model';

export interface BookingSummaryDto {
  id: string;
  roomId: string;
  roomName: string;
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
}

export interface BookingDetailDto {
  id: string;
  roomId: string;
  roomName: string;
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
  roomId: string;
  guestName: string;
  guestEmail: string;
  guestPhone?: string | null;
  checkInDate: string;
  checkOutDate: string;
  internalNotes?: string | null;
}

export interface UpdateBookingStatusRequest {
  status: string;
}

export type BookingsListApiResponse = ApiResult<BookingSummaryDto[]>;
export type BookingDetailApiResponse = ApiResult<BookingDetailDto>;
