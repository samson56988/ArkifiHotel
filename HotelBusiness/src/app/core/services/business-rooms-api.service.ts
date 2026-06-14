import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  BusinessRoomDetailDto,
  BusinessRoomSummaryDto,
  CreateBusinessRoomRequest,
  RoomDetailApiResponse,
  RoomImageDto,
  RoomImagesUploadResponse,
  RoomsListApiResponse,
  UpdateBusinessRoomRequest,
} from '../models/rooms.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { parseHttpApiResult, normalizeApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessRoomsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listRooms(includeArchived = false): Observable<RoomsListApiResponse> {
    const q = includeArchived ? '?includeArchived=true' : '';
    return this.http.get<unknown>(`${this.baseUrl}/api/business/rooms${q}`).pipe(
      map((body) => normalizeApiResult<BusinessRoomSummaryDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessRoomSummaryDto[]>(err)),
      ),
    );
  }

  getRoom(roomId: string): Observable<RoomDetailApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/rooms/${roomId}`).pipe(
      map((body) => normalizeApiResult<BusinessRoomDetailDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessRoomDetailDto>(err)),
      ),
    );
  }

  createRoom(body: CreateBusinessRoomRequest): Observable<RoomDetailApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/rooms`, body).pipe(
      map((res) => normalizeApiResult<BusinessRoomDetailDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessRoomDetailDto>(err)),
      ),
    );
  }

  updateRoom(roomId: string, body: UpdateBusinessRoomRequest): Observable<RoomDetailApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/rooms/${roomId}`, body).pipe(
      map((res) => normalizeApiResult<BusinessRoomDetailDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessRoomDetailDto>(err)),
      ),
    );
  }

  deleteRoom(roomId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/rooms/${roomId}`).pipe(
      map((res) => normalizeApiResult<unknown>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }

  archiveRoom(roomId: string): Observable<RoomDetailApiResponse> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/rooms/${roomId}/archive`, {})
      .pipe(
        map((res) => normalizeApiResult<BusinessRoomDetailDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<BusinessRoomDetailDto>(err)),
        ),
      );
  }

  restoreRoom(roomId: string): Observable<RoomDetailApiResponse> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/rooms/${roomId}/restore`, {})
      .pipe(
        map((res) => normalizeApiResult<BusinessRoomDetailDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<BusinessRoomDetailDto>(err)),
        ),
      );
  }

  uploadRoomImages(roomId: string, files: File[]): Observable<RoomImagesUploadResponse> {
    const fd = new FormData();
    for (const f of files) {
      fd.append('files', f, f.name);
    }

    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/rooms/${roomId}/images`, fd)
      .pipe(
        map((res) => normalizeApiResult<RoomImageDto[]>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<RoomImageDto[]>(err)),
        ),
      );
  }

  deleteRoomImage(roomId: string, imageId: string): Observable<ApiResult<unknown>> {
    return this.http
      .delete<unknown>(`${this.baseUrl}/api/business/rooms/${roomId}/images/${imageId}`)
      .pipe(
        map((res) => normalizeApiResult<unknown>(res)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
      );
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
}
