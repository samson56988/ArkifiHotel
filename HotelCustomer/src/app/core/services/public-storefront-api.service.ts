import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { PublicStorefront, PublicStorefrontApiResponse } from '../models/storefront-theme.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class PublicStorefrontApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getBySlug(slug: string): Observable<PublicStorefrontApiResponse> {
    const encoded = encodeURIComponent(slug);
    return this.http.get<unknown>(`${this.baseUrl}/api/public/stores/${encoded}`).pipe(
      map((body) => normalizeApiResult<PublicStorefront>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<PublicStorefront>(err))),
    );
  }
}
