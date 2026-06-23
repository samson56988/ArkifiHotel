import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  BusinessDashboardApiResponse,
  BusinessDashboardDto,
  DashboardDateRange,
} from '../models/dashboard.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getDashboard(range?: DashboardDateRange): Observable<BusinessDashboardApiResponse> {
    let params = new HttpParams();
    if (range) {
      params = params.set('from', range.from).set('to', range.to);
    }

    return this.http.get<unknown>(`${this.baseUrl}/api/business/dashboard`, { params }).pipe(
      map((body) => normalizeApiResult<BusinessDashboardDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessDashboardDto>(err)),
      ),
    );
  }
}
