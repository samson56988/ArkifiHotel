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

  getSettings(): Observable<RestaurantMenuSettingsApiResponse> {
    return this.http.get<unknown>(`${this.root}/settings`).pipe(
      map((body) => normalizeApiResult<RestaurantMenuSettingsDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuSettingsDto>(err)),
      ),
    );
  }

  updateSettings(body: UpdateRestaurantMenuSettingsRequest): Observable<RestaurantMenuSettingsApiResponse> {
    return this.http.put<unknown>(`${this.root}/settings`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuSettingsDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuSettingsDto>(err)),
      ),
    );
  }

  uploadHeroImage(file: File): Observable<RestaurantMenuSettingsApiResponse> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<unknown>(`${this.root}/settings/hero-image`, form).pipe(
      map((res) => normalizeApiResult<RestaurantMenuSettingsDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuSettingsDto>(err)),
      ),
    );
  }

  deleteHeroImage(): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.root}/settings/hero-image`).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  listCategories(section?: 'food' | 'drink', includeArchived = false): Observable<RestaurantMenuCategoriesApiResponse> {
    const params = new URLSearchParams();
    if (section) {
      params.set('section', section);
    }
    if (includeArchived) {
      params.set('includeArchived', 'true');
    }
    const q = params.toString();
    return this.http.get<unknown>(`${this.root}/categories${q ? `?${q}` : ''}`).pipe(
      map((body) => normalizeApiResult<RestaurantMenuCategoryDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuCategoryDto[]>(err)),
      ),
    );
  }

  createCategory(body: CreateRestaurantMenuCategoryRequest): Observable<RestaurantMenuCategoryApiResponse> {
    return this.http.post<unknown>(`${this.root}/categories`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuCategoryDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuCategoryDto>(err)),
      ),
    );
  }

  updateCategory(id: string, body: UpdateRestaurantMenuCategoryRequest): Observable<RestaurantMenuCategoryApiResponse> {
    return this.http.put<unknown>(`${this.root}/categories/${id}`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuCategoryDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuCategoryDto>(err)),
      ),
    );
  }

  archiveCategory(id: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/categories/${id}/archive`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  restoreCategory(id: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/categories/${id}/restore`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  listItems(categoryId: string, includeArchived = false): Observable<RestaurantMenuItemsApiResponse> {
    const q = includeArchived ? '?includeArchived=true' : '';
    return this.http.get<unknown>(`${this.root}/categories/${categoryId}/items${q}`).pipe(
      map((body) => normalizeApiResult<RestaurantMenuItemDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuItemDto[]>(err)),
      ),
    );
  }

  createItem(categoryId: string, body: CreateRestaurantMenuItemRequest): Observable<RestaurantMenuItemApiResponse> {
    return this.http.post<unknown>(`${this.root}/categories/${categoryId}/items`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuItemDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuItemDto>(err)),
      ),
    );
  }

  updateItem(itemId: string, body: UpdateRestaurantMenuItemRequest): Observable<RestaurantMenuItemApiResponse> {
    return this.http.put<unknown>(`${this.root}/items/${itemId}`, body).pipe(
      map((res) => normalizeApiResult<RestaurantMenuItemDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuItemDto>(err)),
      ),
    );
  }

  archiveItem(itemId: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/items/${itemId}/archive`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  restoreItem(itemId: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.root}/items/${itemId}/restore`, {}).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  uploadItemImage(itemId: string, file: File): Observable<RestaurantMenuItemApiResponse> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<unknown>(`${this.root}/items/${itemId}/image`, form).pipe(
      map((res) => normalizeApiResult<RestaurantMenuItemDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RestaurantMenuItemDto>(err)),
      ),
    );
  }

  deleteItemImage(itemId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.root}/items/${itemId}/image`).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  setItemAvailability(itemId: string, isAvailable: boolean): Observable<RestaurantMenuItemApiResponse> {
    return this.http.put<unknown>(`${this.root}/items/${itemId}/availability`, { isAvailable }).pipe(
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
