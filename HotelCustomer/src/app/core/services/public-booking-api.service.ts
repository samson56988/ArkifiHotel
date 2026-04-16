import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import { API_BASE_URL } from '../tokens/api-base-url.token';

export interface GuestBookingLookupDto {
  propertyName: string;
  roomName: string;
  guestName: string;
  checkInDate: string;
  checkOutDate: string;
  nights: number;
  status: string;
  totalAmount: number;
  currency: string;
  confirmationCode: string;
}

export type GuestBookingLookupResponse = ApiResult<GuestBookingLookupDto>;

@Injectable({ providedIn: 'root' })
export class PublicBookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  lookupByCode(confirmationCode: string): Observable<GuestBookingLookupResponse> {
    const code = confirmationCode.trim();
    const encoded = encodeURIComponent(code);
    return this.http
      .get<GuestBookingLookupResponse>(`${this.baseUrl}/api/public/bookings/${encoded}`)
      .pipe(
        catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<GuestBookingLookupDto>(err))),
      );
  }

  private normalizeHttpError<T>(err: HttpErrorResponse): ApiResult<T> {
    const body = err.error as Partial<ApiResult<T>> | null;
    if (body && typeof body === 'object' && 'success' in body) {
      return body as ApiResult<T>;
    }

    return {
      success: false,
      data: null,
      message: err.message || 'Network error. Is the API running?',
      code: 'HttpError',
      validationErrors: null,
    };
  }
}
