import type { ApiResult } from './api-result.model';

export interface BookingPaymentSummaryDto {
  id: string;
  bookingId: string;
  bookingGuestName: string;
  bookingConfirmationCode: string;
  roomName: string;
  amount: number;
  currency: string;
  status: string;
  method: string;
  gateway: string;
  externalReference: string | null;
  createdAt: string;
}

export interface CreateBookingPaymentRequest {
  bookingId: string;
  amount: number;
  currency: string;
  status: string;
  method: string;
  gateway: string;
  externalReference?: string | null;
  notes?: string | null;
}

export type BookingPaymentsListApiResponse = ApiResult<BookingPaymentSummaryDto[]>;
export type BookingPaymentDetailApiResponse = ApiResult<BookingPaymentSummaryDto>;
