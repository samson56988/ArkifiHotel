import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  BusinessLocationDto,
  CreateBusinessLocationRequest,
  LocationDetailApiResponse,
  LocationsListApiResponse,
  UpdateBusinessLocationRequest,
} from '../models/locations.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessLocationsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listLocations(): Observable<LocationsListApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/locations`).pipe(
      map((body) => normalizeApiResult<BusinessLocationDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessLocationDto[]>(err)),
      ),
    );
  }

  createLocation(body: CreateBusinessLocationRequest): Observable<LocationDetailApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/locations`, body).pipe(
      map((body) => normalizeApiResult<BusinessLocationDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessLocationDto>(err)),
      ),
    );
  }

  updateLocation(locationId: string, body: UpdateBusinessLocationRequest): Observable<LocationDetailApiResponse> {
    return this.http
      .put<unknown>(`${this.baseUrl}/api/business/locations/${locationId}`, body)
      .pipe(
        map((body) => normalizeApiResult<BusinessLocationDto>(body)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<BusinessLocationDto>(err)),
        ),
      );
  }

  deleteLocation(locationId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/locations/${locationId}`).pipe(
      map((body) => normalizeApiResult<unknown>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }
}
