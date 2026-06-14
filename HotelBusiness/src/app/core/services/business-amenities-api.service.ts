import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  AmenityApiResponse,
  AmenityDto,
  AmenitiesApiResponse,
  CreateAmenityRequest,
  UpdateAmenityRequest,
} from '../models/amenities.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessAmenitiesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listAmenities(): Observable<AmenitiesApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/amenities`).pipe(
      map((body) => normalizeApiResult<AmenityDto[]>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<AmenityDto[]>(err))),
    );
  }

  createAmenity(body: CreateAmenityRequest): Observable<AmenityApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/amenities`, body).pipe(
      map((body) => normalizeApiResult<AmenityDto>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<AmenityDto>(err))),
    );
  }

  updateAmenity(amenityId: string, body: UpdateAmenityRequest): Observable<AmenityApiResponse> {
    return this.http
      .put<unknown>(`${this.baseUrl}/api/business/amenities/${amenityId}`, body)
      .pipe(
        map((body) => normalizeApiResult<AmenityDto>(body)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<AmenityDto>(err))),
      );
  }

  deleteAmenity(amenityId: string): Observable<ApiResult<unknown>> {
    return this.http
      .delete<unknown>(`${this.baseUrl}/api/business/amenities/${amenityId}`)
      .pipe(
        map((body) => normalizeApiResult<unknown>(body)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
      );
  }
}
