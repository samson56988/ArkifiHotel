import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  PaymentConfigurationApiResponse,
  PaymentConfigurationDto,
  UpdatePaymentConfigurationRequest,
} from '../models/payment.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class BusinessPaymentApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getConfiguration(): Observable<PaymentConfigurationApiResponse> {
    return this.http.get<PaymentConfigurationApiResponse>(`${this.baseUrl}/api/business/payment-configuration`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<PaymentConfigurationDto>(err)),
      ),
    );
  }

  updateConfiguration(body: UpdatePaymentConfigurationRequest): Observable<PaymentConfigurationApiResponse> {
    return this.http
      .put<PaymentConfigurationApiResponse>(`${this.baseUrl}/api/business/payment-configuration`, body)
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<PaymentConfigurationDto>(err)),
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
