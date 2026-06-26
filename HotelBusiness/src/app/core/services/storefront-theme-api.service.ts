import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  PublicStorefront,
  PublicStorefrontApiResponse,
  StorefrontTheme,
  StorefrontThemeApiResponse,
} from '../models/storefront-theme.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class StorefrontThemeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getTheme(): Observable<StorefrontThemeApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/storefront-theme`).pipe(
      map((body) => normalizeApiResult<StorefrontTheme>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<StorefrontTheme>(err))),
    );
  }

  updateTheme(theme: StorefrontTheme): Observable<StorefrontThemeApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/storefront-theme`, theme).pipe(
      map((body) => normalizeApiResult<StorefrontTheme>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<StorefrontTheme>(err))),
    );
  }

  getPublicStorefront(slug: string, locationId?: string | null): Observable<PublicStorefrontApiResponse> {
    const encoded = encodeURIComponent(slug);
    const query =
      locationId && locationId !== 'default'
        ? `?locationId=${encodeURIComponent(locationId)}`
        : '';
    return this.http.get<unknown>(`${this.baseUrl}/api/public/stores/${encoded}${query}`).pipe(
      map((body) => normalizeApiResult<PublicStorefront>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<PublicStorefront>(err))),
    );
  }
}
