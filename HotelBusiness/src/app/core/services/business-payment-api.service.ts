import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  PaymentConfigurationApiResponse,
  PaymentConfigurationDto,
  UpdatePaymentConfigurationRequest,
} from '../models/payment.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessPaymentApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getConfiguration(): Observable<PaymentConfigurationApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/payment-configuration`).pipe(
      map((body) => normalizeApiResult<PaymentConfigurationDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<PaymentConfigurationDto>(err)),
      ),
    );
  }

  updateConfiguration(body: UpdatePaymentConfigurationRequest): Observable<PaymentConfigurationApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/payment-configuration`, body).pipe(
      map((body) => normalizeApiResult<PaymentConfigurationDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<PaymentConfigurationDto>(err)),
      ),
    );
  }
}
