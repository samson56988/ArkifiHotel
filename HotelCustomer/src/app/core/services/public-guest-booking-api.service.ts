import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type { GuestBookingLookupDto } from './public-booking-api.service';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

export interface GuestCreateBookingRequest {
  locationId: string;
  roomId: string;
  guestName: string;
  guestEmail: string;
  guestPhone: string;
  checkInDate: string;
  checkOutDate: string;
}

export interface GuestBookingCheckoutDto {
  bookingId: string;
  paymentReference: string;
  paymentUrl: string;
  provider: string;
  amount: number;
  currency: string;
}

export interface GuestPaymentVerifyResultDto {
  paymentSuccessful: boolean;
  status: string;
  message?: string | null;
  booking?: GuestBookingLookupDto | null;
}

export type GuestBookingCheckoutResponse = ApiResult<GuestBookingCheckoutDto>;
export type GuestPaymentVerifyResponse = ApiResult<GuestPaymentVerifyResultDto>;

@Injectable({ providedIn: 'root' })
export class PublicGuestBookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  createCheckout(slug: string, body: GuestCreateBookingRequest): Observable<GuestBookingCheckoutResponse> {
    const encoded = encodeURIComponent(slug);
    return this.http.post<unknown>(`${this.baseUrl}/api/public/stores/${encoded}/bookings`, body).pipe(
      map((res) => normalizeApiResult<GuestBookingCheckoutDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<GuestBookingCheckoutDto>(err)),
      ),
    );
  }

  verifyPayment(slug: string, reference: string): Observable<GuestPaymentVerifyResponse> {
    const encoded = encodeURIComponent(slug);
    const ref = encodeURIComponent(reference);
    return this.http
      .get<unknown>(`${this.baseUrl}/api/public/stores/${encoded}/bookings/payment/verify?reference=${ref}`)
      .pipe(
        map((res) => normalizeApiResult<GuestPaymentVerifyResultDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<GuestPaymentVerifyResultDto>(err)),
        ),
      );
  }
}
