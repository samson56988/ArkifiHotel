import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type { StorefrontBannerImageDto } from '../models/storefront-banner.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class StorefrontBannerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listImages(locationId?: string | null): Observable<ApiResult<StorefrontBannerImageDto[]>> {
    const query = locationId ? `?locationId=${encodeURIComponent(locationId)}` : '';
    return this.http.get<unknown>(`${this.baseUrl}/api/business/storefront-banner/images${query}`).pipe(
      map((body) => normalizeApiResult<StorefrontBannerImageDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<StorefrontBannerImageDto[]>(err)),
      ),
    );
  }

  uploadImages(files: File[], locationId: string): Observable<ApiResult<StorefrontBannerImageDto[]>> {
    const form = new FormData();
    form.append('locationId', locationId);
    for (const file of files) {
      form.append('files', file, file.name);
    }

    return this.http.post<unknown>(`${this.baseUrl}/api/business/storefront-banner/images`, form).pipe(
      map((body) => normalizeApiResult<StorefrontBannerImageDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<StorefrontBannerImageDto[]>(err)),
      ),
    );
  }

  deleteImage(imageId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/storefront-banner/images/${imageId}`).pipe(
      map((body) => normalizeApiResult<unknown>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }
}
