import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  CreatePropertyFacilityRequest,
  FacilitiesListApiResponse,
  FacilityDetailApiResponse,
  FacilityImageDto,
  FacilityImagesUploadResponse,
  PropertyFacilityDetailDto,
  PropertyFacilitySummaryDto,
  UpdatePropertyFacilityRequest,
} from '../models/facilities.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessFacilitiesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listFacilities(includeArchived = false): Observable<FacilitiesListApiResponse> {
    const q = includeArchived ? '?includeArchived=true' : '';
    return this.http.get<unknown>(`${this.baseUrl}/api/business/facilities${q}`).pipe(
      map((body) => normalizeApiResult<PropertyFacilitySummaryDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<PropertyFacilitySummaryDto[]>(err)),
      ),
    );
  }

  getFacility(id: string): Observable<FacilityDetailApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/facilities/${id}`).pipe(
      map((body) => normalizeApiResult<PropertyFacilityDetailDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<PropertyFacilityDetailDto>(err)),
      ),
    );
  }

  createFacility(body: CreatePropertyFacilityRequest): Observable<FacilityDetailApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/facilities`, body).pipe(
      map((res) => normalizeApiResult<PropertyFacilityDetailDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<PropertyFacilityDetailDto>(err)),
      ),
    );
  }

  updateFacility(id: string, body: UpdatePropertyFacilityRequest): Observable<FacilityDetailApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/facilities/${id}`, body).pipe(
      map((res) => normalizeApiResult<PropertyFacilityDetailDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<PropertyFacilityDetailDto>(err)),
      ),
    );
  }

  deleteFacility(id: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/facilities/${id}`).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  archiveFacility(facilityId: string): Observable<FacilityDetailApiResponse> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/facilities/${facilityId}/archive`, {})
      .pipe(
        map((res) => normalizeApiResult<PropertyFacilityDetailDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<PropertyFacilityDetailDto>(err)),
        ),
      );
  }

  restoreFacility(facilityId: string): Observable<FacilityDetailApiResponse> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/facilities/${facilityId}/restore`, {})
      .pipe(
        map((res) => normalizeApiResult<PropertyFacilityDetailDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<PropertyFacilityDetailDto>(err)),
        ),
      );
  }

  uploadFacilityImages(facilityId: string, files: File[]): Observable<FacilityImagesUploadResponse> {
    const fd = new FormData();
    for (const f of files) {
      fd.append('files', f, f.name);
    }

    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/facilities/${facilityId}/images`, fd)
      .pipe(
        map((res) => normalizeApiResult<FacilityImageDto[]>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<FacilityImageDto[]>(err)),
        ),
      );
  }

  deleteFacilityImage(facilityId: string, imageId: string): Observable<ApiResult<unknown>> {
    return this.http
      .delete<unknown>(`${this.baseUrl}/api/business/facilities/${facilityId}/images/${imageId}`)
      .pipe(
        map((res) => normalizeApiResult<unknown>(res)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
      );
  }

  resolveImageUrl(path: string): string {
    if (!path) {
      return '';
    }

    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }

    return `${this.baseUrl}${path.startsWith('/') ? '' : '/'}${path}`;
  }
}
