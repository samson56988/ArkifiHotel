import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  BusinessProfileApiResponse,
  BusinessProfileDto,
  SlugAvailabilityApiResponse,
  UpdateBusinessProfileRequest,
} from '../models/business-profile.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessProfileApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getProfile(): Observable<BusinessProfileApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/profile`).pipe(
      map((body) => normalizeApiResult<BusinessProfileDto>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<BusinessProfileDto>(err))),
    );
  }

  updateProfile(body: UpdateBusinessProfileRequest): Observable<BusinessProfileApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/profile`, body).pipe(
      map((body) => normalizeApiResult<BusinessProfileDto>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<BusinessProfileDto>(err))),
    );
  }

  checkSlug(slug: string): Observable<SlugAvailabilityApiResponse> {
    const encoded = encodeURIComponent(slug);
    return this.http.get<unknown>(`${this.baseUrl}/api/business/profile/check-slug?slug=${encoded}`).pipe(
      map((body) => normalizeApiResult<{ slug: string; available: boolean }>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<{ slug: string; available: boolean }>(err)),
      ),
    );
  }

  uploadLogo(file: File): Observable<BusinessProfileApiResponse> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<unknown>(`${this.baseUrl}/api/business/profile/logo`, form).pipe(
      map((body) => normalizeApiResult<BusinessProfileDto>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<BusinessProfileDto>(err))),
    );
  }

  removeLogo(): Observable<BusinessProfileApiResponse> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/profile/logo`).pipe(
      map((body) => normalizeApiResult<BusinessProfileDto>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<BusinessProfileDto>(err))),
    );
  }
}
