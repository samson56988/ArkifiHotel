import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  CreateEventHallRequest,
  EventHallDetailApiResponse,
  EventHallDetailDto,
  EventHallRequestDetailApiResponse,
  EventHallRequestDetailDto,
  EventHallRequestsListApiResponse,
  EventHallRequestListItemDto,
  EventHallSummaryDto,
  EventHallsListApiResponse,
  UpdateEventHallRequest,
} from '../models/event-hall.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessEventHallsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  list(includeArchived = false): Observable<EventHallsListApiResponse> {
    const params = includeArchived ? new HttpParams().set('includeArchived', 'true') : undefined;
    return this.http.get<unknown>(`${this.baseUrl}/api/business/event-halls`, { params }).pipe(
      map((body) => normalizeApiResult<EventHallSummaryDto[]>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  get(id: string): Observable<EventHallDetailApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/event-halls/${id}`).pipe(
      map((body) => normalizeApiResult<EventHallDetailDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<EventHallDetailDto>(err)),
      ),
    );
  }

  create(body: CreateEventHallRequest): Observable<EventHallDetailApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/event-halls`, body).pipe(
      map((res) => normalizeApiResult<EventHallDetailDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<EventHallDetailDto>(err)),
      ),
    );
  }

  update(id: string, body: UpdateEventHallRequest): Observable<EventHallDetailApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/event-halls/${id}`, body).pipe(
      map((res) => normalizeApiResult<EventHallDetailDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<EventHallDetailDto>(err)),
      ),
    );
  }

  archive(id: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/event-halls/${id}/archive`, {}).pipe(
      map((res) => normalizeApiResult(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  restore(id: string): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/event-halls/${id}/restore`, {}).pipe(
      map((res) => normalizeApiResult(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  delete(id: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/event-halls/${id}`).pipe(
      map((res) => normalizeApiResult(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  uploadImage(eventHallId: string, file: File): Observable<EventHallDetailApiResponse> {
    const fd = new FormData();
    fd.append('file', file, file.name);
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/event-halls/${eventHallId}/images`, fd)
      .pipe(
        map((res) => normalizeApiResult<EventHallDetailDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<EventHallDetailDto>(err)),
        ),
      );
  }

  deleteImage(eventHallId: string, imageId: string): Observable<ApiResult<unknown>> {
    return this.http
      .delete<unknown>(`${this.baseUrl}/api/business/event-halls/${eventHallId}/images/${imageId}`)
      .pipe(
        map((res) => normalizeApiResult(res)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
      );
  }

  listRequests(status?: string): Observable<EventHallRequestsListApiResponse> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<unknown>(`${this.baseUrl}/api/business/event-halls/requests`, { params }).pipe(
      map((body) => normalizeApiResult<EventHallRequestListItemDto[]>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  getRequest(id: string): Observable<EventHallRequestDetailApiResponse> {
    return this.http
      .get<unknown>(`${this.baseUrl}/api/business/event-halls/requests/${id}`)
      .pipe(
        map((body) => normalizeApiResult<EventHallRequestDetailDto>(body)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<EventHallRequestDetailDto>(err)),
        ),
      );
  }

  updateRequestStatus(id: string, status: string): Observable<EventHallRequestDetailApiResponse> {
    return this.http
      .put<unknown>(`${this.baseUrl}/api/business/event-halls/requests/${id}/status`, { status })
      .pipe(
        map((body) => normalizeApiResult<EventHallRequestDetailDto>(body)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<EventHallRequestDetailDto>(err)),
        ),
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
