import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { SlugAvailabilityApiResponse } from '../models/business-profile.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class RegistrationApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  checkSlug(slug: string): Observable<SlugAvailabilityApiResponse> {
    const encoded = encodeURIComponent(slug);
    return this.http.get<unknown>(`${this.baseUrl}/api/Registration/check-slug?slug=${encoded}`).pipe(
      map((body) => normalizeApiResult<{ slug: string; available: boolean }>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<{ slug: string; available: boolean }>(err)),
      ),
    );
  }
}
