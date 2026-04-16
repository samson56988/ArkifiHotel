import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
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

@Injectable({ providedIn: 'root' })
export class BusinessFacilitiesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listFacilities(includeArchived = false): Observable<FacilitiesListApiResponse> {
    const q = includeArchived ? '?includeArchived=true' : '';
    return this.http.get<FacilitiesListApiResponse>(`${this.baseUrl}/api/business/facilities${q}`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<PropertyFacilitySummaryDto[]>(err)),
      ),
    );
  }

  getFacility(id: string): Observable<FacilityDetailApiResponse> {
    return this.http.get<FacilityDetailApiResponse>(`${this.baseUrl}/api/business/facilities/${id}`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<PropertyFacilityDetailDto>(err)),
      ),
    );
  }

  createFacility(body: CreatePropertyFacilityRequest): Observable<FacilityDetailApiResponse> {
    return this.http.post<FacilityDetailApiResponse>(`${this.baseUrl}/api/business/facilities`, body).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<PropertyFacilityDetailDto>(err)),
      ),
    );
  }

  updateFacility(id: string, body: UpdatePropertyFacilityRequest): Observable<FacilityDetailApiResponse> {
    return this.http.put<FacilityDetailApiResponse>(`${this.baseUrl}/api/business/facilities/${id}`, body).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<PropertyFacilityDetailDto>(err)),
      ),
    );
  }

  deleteFacility(id: string): Observable<ApiResult<unknown>> {
    return this.http.delete<ApiResult<unknown>>(`${this.baseUrl}/api/business/facilities/${id}`).pipe(
      catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<unknown>(err))),
    );
  }

  archiveFacility(facilityId: string): Observable<FacilityDetailApiResponse> {
    return this.http
      .post<FacilityDetailApiResponse>(`${this.baseUrl}/api/business/facilities/${facilityId}/archive`, {})
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<PropertyFacilityDetailDto>(err)),
        ),
      );
  }

  restoreFacility(facilityId: string): Observable<FacilityDetailApiResponse> {
    return this.http
      .post<FacilityDetailApiResponse>(`${this.baseUrl}/api/business/facilities/${facilityId}/restore`, {})
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<PropertyFacilityDetailDto>(err)),
        ),
      );
  }

  uploadFacilityImages(facilityId: string, files: File[]): Observable<FacilityImagesUploadResponse> {
    const fd = new FormData();
    for (const f of files) {
      fd.append('files', f, f.name);
    }

    return this.http
      .post<FacilityImagesUploadResponse>(
        `${this.baseUrl}/api/business/facilities/${facilityId}/images`,
        fd,
      )
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<FacilityImageDto[]>(err)),
        ),
      );
  }

  deleteFacilityImage(facilityId: string, imageId: string): Observable<ApiResult<unknown>> {
    return this.http
      .delete<ApiResult<unknown>>(`${this.baseUrl}/api/business/facilities/${facilityId}/images/${imageId}`)
      .pipe(catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<unknown>(err))));
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
