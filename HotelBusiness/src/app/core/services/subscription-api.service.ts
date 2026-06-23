import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  BusinessSubscriptionApiResponse,
  BusinessSubscriptionDto,
  BusinessSubscriptionPaymentHistoryApiResponse,
  BusinessSubscriptionPaymentHistoryDto,
  InitSubscriptionPaymentApiResponse,
  InitSubscriptionPaymentResultDto,
  SubscriptionPlanDto,
  SubscriptionPlanOptionDto,
  SubscriptionPlanOptionsApiResponse,
  SubscriptionPlansApiResponse,
} from '../models/subscription.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class SubscriptionApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listPublicPlans(): Observable<SubscriptionPlansApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/public/subscription-plans`).pipe(
      map((body) => normalizeApiResult<SubscriptionPlanDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<SubscriptionPlanDto[]>(err)),
      ),
    );
  }

  getCurrent(): Observable<BusinessSubscriptionApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/subscription`).pipe(
      map((body) => normalizeApiResult<BusinessSubscriptionDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessSubscriptionDto>(err)),
      ),
    );
  }

  listPlanOptions(): Observable<SubscriptionPlanOptionsApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/subscription/plans`).pipe(
      map((body) => normalizeApiResult<SubscriptionPlanOptionDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<SubscriptionPlanOptionDto[]>(err)),
      ),
    );
  }

  initializePayment(planCode: string): Observable<InitSubscriptionPaymentApiResponse> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/subscription/initialize-payment`, { planCode })
      .pipe(
        map((body) => normalizeApiResult<InitSubscriptionPaymentResultDto>(body)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
      );
  }

  verifyPayment(reference: string): Observable<BusinessSubscriptionApiResponse> {
    const encoded = encodeURIComponent(reference);
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/subscription/verify-payment?reference=${encoded}`, {})
      .pipe(
        map((body) => normalizeApiResult<BusinessSubscriptionDto>(body)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<BusinessSubscriptionDto>(err)),
        ),
      );
  }

  changePlan(planCode: string): Observable<BusinessSubscriptionApiResponse> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/subscription/change-plan`, { planCode })
      .pipe(
        map((body) => normalizeApiResult<BusinessSubscriptionDto>(body)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<BusinessSubscriptionDto>(err)),
        ),
      );
  }

  listPaymentHistory(): Observable<BusinessSubscriptionPaymentHistoryApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/subscription/payments`).pipe(
      map((body) => normalizeApiResult<BusinessSubscriptionPaymentHistoryDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessSubscriptionPaymentHistoryDto[]>(err)),
      ),
    );
  }
}
