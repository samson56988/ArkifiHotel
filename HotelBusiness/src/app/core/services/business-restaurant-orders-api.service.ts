import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  ListRestaurantOrdersParams,
  RestaurantOrderDetailApiResponse,
  RestaurantOrderDetailDto,
  RestaurantOrderListApiResponse,
  RestaurantOrderListResultDto,
} from '../models/restaurant-order.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessRestaurantOrdersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly root = `${this.baseUrl}/api/business/restaurant-orders`;

  list(params: ListRestaurantOrdersParams = {}): Observable<RestaurantOrderListApiResponse> {
    let httpParams = new HttpParams();
    if (params.page) {
      httpParams = httpParams.set('page', String(params.page));
    }
    if (params.pageSize) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }
    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }

    return this.http.get<unknown>(this.root, { params: httpParams }).pipe(
      map((body) => normalizeApiResult<RestaurantOrderListResultDto>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  get(orderId: string): Observable<RestaurantOrderDetailApiResponse> {
    return this.http.get<unknown>(`${this.root}/${encodeURIComponent(orderId)}`).pipe(
      map((body) => normalizeApiResult<RestaurantOrderDetailDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantOrderDetailDto>(err)),
      ),
    );
  }
}
