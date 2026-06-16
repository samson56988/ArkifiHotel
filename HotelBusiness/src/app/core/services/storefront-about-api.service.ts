import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type { StorefrontAboutImageDto } from '../models/storefront-about.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class StorefrontAboutApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getImage(): Observable<ApiResult<StorefrontAboutImageDto | null>> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/storefront-about/image`).pipe(
      map((body) => normalizeApiResult<StorefrontAboutImageDto | null>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<StorefrontAboutImageDto | null>(err)),
      ),
    );
  }

  uploadImage(file: File): Observable<ApiResult<StorefrontAboutImageDto>> {
    const form = new FormData();
    form.append('file', file, file.name);

    return this.http.post<unknown>(`${this.baseUrl}/api/business/storefront-about/image`, form).pipe(
      map((body) => normalizeApiResult<StorefrontAboutImageDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<StorefrontAboutImageDto>(err)),
      ),
    );
  }

  deleteImage(): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/storefront-about/image`).pipe(
      map((body) => normalizeApiResult<unknown>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }
}
