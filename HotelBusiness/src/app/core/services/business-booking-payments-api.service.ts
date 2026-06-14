import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  BookingPaymentDetailApiResponse,
  BookingPaymentSummaryDto,
  BookingPaymentsListApiResponse,
  CreateBookingPaymentRequest,
} from '../models/booking-payments.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessBookingPaymentsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listPayments(): Observable<BookingPaymentsListApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/booking-payments`).pipe(
      map((body) => normalizeApiResult<BookingPaymentSummaryDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BookingPaymentSummaryDto[]>(err)),
      ),
    );
  }

  createPayment(body: CreateBookingPaymentRequest): Observable<BookingPaymentDetailApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/booking-payments`, body).pipe(
      map((body) => normalizeApiResult<BookingPaymentSummaryDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BookingPaymentSummaryDto>(err)),
      ),
    );
  }
}
