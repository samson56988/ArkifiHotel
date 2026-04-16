import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  BookingPaymentDetailApiResponse,
  BookingPaymentSummaryDto,
  BookingPaymentsListApiResponse,
  CreateBookingPaymentRequest,
} from '../models/booking-payments.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class BusinessBookingPaymentsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listPayments(): Observable<BookingPaymentsListApiResponse> {
    return this.http.get<BookingPaymentsListApiResponse>(`${this.baseUrl}/api/business/booking-payments`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BookingPaymentSummaryDto[]>(err)),
      ),
    );
  }

  createPayment(body: CreateBookingPaymentRequest): Observable<BookingPaymentDetailApiResponse> {
    return this.http.post<BookingPaymentDetailApiResponse>(`${this.baseUrl}/api/business/booking-payments`, body).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BookingPaymentSummaryDto>(err)),
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
