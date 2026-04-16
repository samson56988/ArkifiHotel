import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  BookingDetailApiResponse,
  BookingDetailDto,
  BookingsListApiResponse,
  BookingSummaryDto,
  CreateBookingRequest,
} from '../models/bookings.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class BusinessBookingsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listBookings(): Observable<BookingsListApiResponse> {
    return this.http.get<BookingsListApiResponse>(`${this.baseUrl}/api/business/bookings`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BookingSummaryDto[]>(err)),
      ),
    );
  }

  getBooking(bookingId: string): Observable<BookingDetailApiResponse> {
    return this.http.get<BookingDetailApiResponse>(`${this.baseUrl}/api/business/bookings/${bookingId}`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BookingDetailDto>(err)),
      ),
    );
  }

  createBooking(body: CreateBookingRequest): Observable<BookingDetailApiResponse> {
    return this.http.post<BookingDetailApiResponse>(`${this.baseUrl}/api/business/bookings`, body).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BookingDetailDto>(err)),
      ),
    );
  }

  updateStatus(bookingId: string, status: string): Observable<BookingDetailApiResponse> {
    return this.http
      .patch<BookingDetailApiResponse>(`${this.baseUrl}/api/business/bookings/${bookingId}/status`, {
        status,
      })
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<BookingDetailDto>(err)),
        ),
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
