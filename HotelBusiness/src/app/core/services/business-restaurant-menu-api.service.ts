import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  CreateRestaurantMenuCategoryRequest,
  CreateRestaurantMenuItemRequest,
  RestaurantMenuCategoriesApiResponse,
  RestaurantMenuCategoryApiResponse,
  RestaurantMenuCategoryDto,
  RestaurantMenuItemApiResponse,
  RestaurantMenuItemDto,
  RestaurantMenuItemsApiResponse,
  RestaurantMenuSettingsApiResponse,
  RestaurantMenuSettingsDto,
  UpdateRestaurantMenuCategoryRequest,
  UpdateRestaurantMenuItemRequest,
  UpdateRestaurantMenuSettingsRequest,
} from '../models/restaurant-menu.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessRestaurantMenuApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly root = `${this.baseUrl}/api/business/restaurant-menu`;

  private query(locationId: string, extra?: Record<string, string>): string {
    const params = new URLSearchParams({ locationId });
    if (extra) {
      for (const [key, value] of Object.entries(extra)) {
        params.set(key, value);
      }
    }
    return `?${params.toString()}`;
  }

  getSettings(locationId: string): Observable<RestaurantMenuSettingsApiResponse> {
    return this.http.get<unknown>(`${this.root}/settings${this.query(locationId)}`).pipe(
      map((body) => normalizeApiResult<RestaurantMenuSettingsDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuSettingsDto>(err)),
      ),
    );
  }

  updateSettings(
    locationId: string,
    body: UpdateRestaurantMenuSettingsRequest,
  ): Observable<RestaurantMenuSettingsApiResponse> {
    return this.http.put<unknown>(`${this.root}/settings${this.query(locationId)}`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuSettingsDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuSettingsDto>(err)),
      ),
    );
  }

  uploadHeroImage(locationId: string, file: File): Observable<RestaurantMenuSettingsApiResponse> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<unknown>(`${this.root}/settings/hero-image${this.query(locationId)}`, form).pipe(
      map((res) => normalizeApiResult<RestaurantMenuSettingsDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuSettingsDto>(err)),
      ),
    );
  }

  deleteHeroImage(locationId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.root}/settings/hero-image${this.query(locationId)}`).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  listCategories(
    locationId: string,
    section?: 'food' | 'drink',
    includeArchived = false,
  ): Observable<RestaurantMenuCategoriesApiResponse> {
    const extra: Record<string, string> = {};
    if (section) {
      extra['section'] = section;
    }
    if (includeArchived) {
      extra['includeArchived'] = 'true';
    }
    return this.http.get<unknown>(`${this.root}/categories${this.query(locationId, extra)}`).pipe(
      map((body) => normalizeApiResult<RestaurantMenuCategoryDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuCategoryDto[]>(err)),
      ),
    );
  }

  createCategory(
    locationId: string,
    body: CreateRestaurantMenuCategoryRequest,
  ): Observable<RestaurantMenuCategoryApiResponse> {
    return this.http.post<unknown>(`${this.root}/categories${this.query(locationId)}`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuCategoryDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuCategoryDto>(err)),
      ),
    );
  }

  updateCategory(
    locationId: string,
    id: string,
    body: UpdateRestaurantMenuCategoryRequest,
  ): Observable<RestaurantMenuCategoryApiResponse> {
    return this.http.put<unknown>(`${this.root}/categories/${id}${this.query(locationId)}`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuCategoryDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuCategoryDto>(err)),
      ),
    );
  }

  archiveCategory(locationId: string, id: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/categories/${id}/archive${this.query(locationId)}`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  restoreCategory(locationId: string, id: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/categories/${id}/restore${this.query(locationId)}`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  listItems(
    locationId: string,
    categoryId: string,
    includeArchived = false,
  ): Observable<RestaurantMenuItemsApiResponse> {
    const extra = includeArchived ? { includeArchived: 'true' } : undefined;
    return this.http
      .get<unknown>(`${this.root}/categories/${categoryId}/items${this.query(locationId, extra)}`)
      .pipe(
        map((body) => normalizeApiResult<RestaurantMenuItemDto[]>(body)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<RestaurantMenuItemDto[]>(err)),
        ),
      );
  }

  createItem(
    locationId: string,
    categoryId: string,
    body: CreateRestaurantMenuItemRequest,
  ): Observable<RestaurantMenuItemApiResponse> {
    return this.http
      .post<unknown>(`${this.root}/categories/${categoryId}/items${this.query(locationId)}`, body)
      .pipe(
        map((res) => normalizeApiResult<RestaurantMenuItemDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<RestaurantMenuItemDto>(err)),
        ),
      );
  }

  updateItem(
    locationId: string,
    itemId: string,
    body: UpdateRestaurantMenuItemRequest,
  ): Observable<RestaurantMenuItemApiResponse> {
    return this.http.put<unknown>(`${this.root}/items/${itemId}${this.query(locationId)}`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuItemDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuItemDto>(err)),
      ),
    );
  }

  archiveItem(locationId: string, itemId: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/items/${itemId}/archive${this.query(locationId)}`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  restoreItem(locationId: string, itemId: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/items/${itemId}/restore${this.query(locationId)}`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  uploadItemImage(locationId: string, itemId: string, file: File): Observable<RestaurantMenuItemApiResponse> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<unknown>(`${this.root}/items/${itemId}/image${this.query(locationId)}`, form).pipe(
      map((res) => normalizeApiResult<RestaurantMenuItemDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuItemDto>(err)),
      ),
    );
  }

  deleteItemImage(locationId: string, itemId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.root}/items/${itemId}/image${this.query(locationId)}`).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  setItemAvailability(
    locationId: string,
    itemId: string,
    isAvailable: boolean,
  ): Observable<RestaurantMenuItemApiResponse> {
    return this.http
      .put<unknown>(`${this.root}/items/${itemId}/availability${this.query(locationId)}`, { isAvailable })
      .pipe(
        map((res) => normalizeApiResult<RestaurantMenuItemDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<RestaurantMenuItemDto>(err)),
        ),
      );
  }

  resolveImageUrl(path: string | null | undefined): string {
    if (!path) {
      return '';
    }
    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }
    return `${this.baseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }
}
