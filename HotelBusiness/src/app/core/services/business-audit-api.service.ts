import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type { PagedResultDto } from '../models/bookings.models';
import type { OrganizationAuditLogDto, ListOrganizationAuditQuery } from '../models/audit.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

export type AuditLogsApiResponse = ApiResult<PagedResultDto<OrganizationAuditLogDto>>;

@Injectable({ providedIn: 'root' })
export class BusinessAuditApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  list(query: ListOrganizationAuditQuery = {}): Observable<AuditLogsApiResponse> {
    let params = new HttpParams();
    if (query.locationId) params = params.set('locationId', query.locationId);
    if (query.entityType) params = params.set('entityType', query.entityType);
    if (query.action) params = params.set('action', query.action);
    if (query.userOrganizationId) params = params.set('userOrganizationId', query.userOrganizationId);
    if (query.fromUtc) params = params.set('fromUtc', query.fromUtc);
    if (query.toUtc) params = params.set('toUtc', query.toUtc);
    if (query.page) params = params.set('page', String(query.page));
    if (query.pageSize) params = params.set('pageSize', String(query.pageSize));

    return this.http.get<unknown>(`${this.baseUrl}/api/business/audit`, { params }).pipe(
      map((body) => normalizeApiResult<PagedResultDto<OrganizationAuditLogDto>>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }
}
