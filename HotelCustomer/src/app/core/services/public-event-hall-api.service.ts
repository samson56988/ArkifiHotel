import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

export interface GuestCreateEventHallRequest {
  locationId: string;
  eventHallId: string;
  guestName: string;
  guestEmail: string;
  guestPhone: string;
  eventDate: string;
  eventEndDate?: string | null;
  eventPurpose: string;
  notes?: string | null;
}

export interface GuestEventHallRequestResultDto {
  requestId: string;
  status: string;
  message: string;
}

export type GuestEventHallRequestResponse = ApiResult<GuestEventHallRequestResultDto>;

@Injectable({ providedIn: 'root' })
export class PublicEventHallApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  createRequest(slug: string, body: GuestCreateEventHallRequest): Observable<GuestEventHallRequestResponse> {
    const encoded = encodeURIComponent(slug);
    return this.http.post<unknown>(`${this.baseUrl}/api/public/stores/${encoded}/event-hall-requests`, body).pipe(
      map((res) => normalizeApiResult<GuestEventHallRequestResultDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<GuestEventHallRequestResultDto>(err)),
      ),
    );
  }
}
