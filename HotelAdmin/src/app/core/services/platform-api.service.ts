import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import type { ApiResult } from '../models/api-result.model';
import type {
  PagedResult,
  PlatformActivityLog,
  PlatformBusinessDetail,
  PlatformBusinessSummary,
  PlatformDashboardStats,
  PlatformSubscriptionPayment,
  SubscriptionPlan,
  UpdatePlatformBusinessRequest,
} from '../models/platform.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class PlatformApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getDashboard(): Observable<ApiResult<PlatformDashboardStats>> {
    return this.http
      .get<unknown>(`${this.baseUrl}/api/platform/businesses/dashboard`)
      .pipe(map((body) => normalizeApiResult<PlatformDashboardStats>(body)));
  }

  listBusinesses(): Observable<ApiResult<PlatformBusinessSummary[]>> {
    return this.http
      .get<unknown>(`${this.baseUrl}/api/platform/businesses`)
      .pipe(map((body) => normalizeApiResult<PlatformBusinessSummary[]>(body)));
  }

  getBusiness(id: string): Observable<ApiResult<PlatformBusinessDetail>> {
    return this.http
      .get<unknown>(`${this.baseUrl}/api/platform/businesses/${id}`)
      .pipe(map((body) => normalizeApiResult<PlatformBusinessDetail>(body)));
  }

  updateBusiness(
    id: string,
    request: UpdatePlatformBusinessRequest,
  ): Observable<ApiResult<PlatformBusinessDetail>> {
    return this.http
      .patch<unknown>(`${this.baseUrl}/api/platform/businesses/${id}`, request)
      .pipe(map((body) => normalizeApiResult<PlatformBusinessDetail>(body)));
  }

  listActivity(params: {
    page?: number;
    pageSize?: number;
    businessId?: string;
  }): Observable<ApiResult<PagedResult<PlatformActivityLog>>> {
    const query = new URLSearchParams();
    if (params.page) {
      query.set('page', String(params.page));
    }
    if (params.pageSize) {
      query.set('pageSize', String(params.pageSize));
    }
    if (params.businessId) {
      query.set('businessId', params.businessId);
    }

    const suffix = query.toString() ? `?${query.toString()}` : '';
    return this.http
      .get<unknown>(`${this.baseUrl}/api/platform/activity${suffix}`)
      .pipe(map((body) => normalizeApiResult<PagedResult<PlatformActivityLog>>(body)));
  }

  listPlans(): Observable<ApiResult<SubscriptionPlan[]>> {
    return this.http
      .get<unknown>(`${this.baseUrl}/api/platform/subscriptions/plans`)
      .pipe(map((body) => normalizeApiResult<SubscriptionPlan[]>(body)));
  }

  listPayments(): Observable<ApiResult<PlatformSubscriptionPayment[]>> {
    return this.http
      .get<unknown>(`${this.baseUrl}/api/platform/subscriptions/payments`)
      .pipe(map((body) => normalizeApiResult<PlatformSubscriptionPayment[]>(body)));
  }
}
