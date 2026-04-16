import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  AmenityDto,
  AmenitiesApiResponse,
  BusinessRoomDetailDto,
  BusinessRoomSummaryDto,
  CreateBusinessRoomRequest,
  CreateCustomAmenityRequest,
  RoomDetailApiResponse,
  RoomImageDto,
  RoomImagesUploadResponse,
  RoomsListApiResponse,
  UpdateBusinessRoomRequest,
} from '../models/rooms.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class BusinessRoomsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listAmenities(): Observable<AmenitiesApiResponse> {
    return this.http.get<AmenitiesApiResponse>(`${this.baseUrl}/api/business/amenities`).pipe(
      catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<AmenityDto[]>(err))),
    );
  }

  createCustomAmenity(body: CreateCustomAmenityRequest): Observable<ApiResult<AmenityDto>> {
    return this.http.post<ApiResult<AmenityDto>>(`${this.baseUrl}/api/business/amenities`, body).pipe(
      catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<AmenityDto>(err))),
    );
  }

  listRooms(includeArchived = false): Observable<RoomsListApiResponse> {
    const q = includeArchived ? '?includeArchived=true' : '';
    return this.http.get<RoomsListApiResponse>(`${this.baseUrl}/api/business/rooms${q}`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BusinessRoomSummaryDto[]>(err)),
      ),
    );
  }

  getRoom(roomId: string): Observable<RoomDetailApiResponse> {
    return this.http.get<RoomDetailApiResponse>(`${this.baseUrl}/api/business/rooms/${roomId}`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BusinessRoomDetailDto>(err)),
      ),
    );
  }

  createRoom(body: CreateBusinessRoomRequest): Observable<RoomDetailApiResponse> {
    return this.http.post<RoomDetailApiResponse>(`${this.baseUrl}/api/business/rooms`, body).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BusinessRoomDetailDto>(err)),
      ),
    );
  }

  updateRoom(roomId: string, body: UpdateBusinessRoomRequest): Observable<RoomDetailApiResponse> {
    return this.http.put<RoomDetailApiResponse>(`${this.baseUrl}/api/business/rooms/${roomId}`, body).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<BusinessRoomDetailDto>(err)),
      ),
    );
  }

  deleteRoom(roomId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<ApiResult<unknown>>(`${this.baseUrl}/api/business/rooms/${roomId}`).pipe(
      catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<unknown>(err))),
    );
  }

  archiveRoom(roomId: string): Observable<RoomDetailApiResponse> {
    return this.http
      .post<RoomDetailApiResponse>(`${this.baseUrl}/api/business/rooms/${roomId}/archive`, {})
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<BusinessRoomDetailDto>(err)),
        ),
      );
  }

  restoreRoom(roomId: string): Observable<RoomDetailApiResponse> {
    return this.http
      .post<RoomDetailApiResponse>(`${this.baseUrl}/api/business/rooms/${roomId}/restore`, {})
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<BusinessRoomDetailDto>(err)),
        ),
      );
  }

  uploadRoomImages(roomId: string, files: File[]): Observable<RoomImagesUploadResponse> {
    const fd = new FormData();
    for (const f of files) {
      fd.append('files', f, f.name);
    }

    return this.http
      .post<RoomImagesUploadResponse>(`${this.baseUrl}/api/business/rooms/${roomId}/images`, fd)
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<RoomImageDto[]>(err)),
        ),
      );
  }

  deleteRoomImage(roomId: string, imageId: string): Observable<ApiResult<unknown>> {
    return this.http
      .delete<ApiResult<unknown>>(`${this.baseUrl}/api/business/rooms/${roomId}/images/${imageId}`)
      .pipe(catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<unknown>(err))));
  }

  resolveImageUrl(path: string): string {
    if (!path) {
      return '';
    }

    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }

    return `${this.baseUrl}${path.startsWith('/') ? '' : '/'}${path}`;
  }

  private normalizeHttpError<T>(err: HttpErrorResponse): ApiResult<T> {
    const body = err.error as Partial<ApiResult<T>> | null;
    if (body && typeof body === 'object' && 'success' in body) {
      return body as ApiResult<T>;
    }

    return {
      success: false,
      data: null,
      message: err.message || 'Network error. Is the API running?',
      code: 'HttpError',
      validationErrors: null,
    };
  }
}
