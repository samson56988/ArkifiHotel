import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  BookingDetailApiResponse,
  BookingDetailDto,
  BookingsListApiResponse,
  BookingSummaryDto,
  CreateBookingRequest,
  ListBookingsParams,
  PagedResultDto,
  RoomAvailabilityApiResponse,
  RoomAvailabilityDto,
} from '../models/bookings.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessBookingsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listBookings(params: ListBookingsParams = {}): Observable<BookingsListApiResponse> {
    const q = new URLSearchParams();
    if (params.page != null) {
      q.set('page', String(params.page));
    }
    if (params.pageSize != null) {
      q.set('pageSize', String(params.pageSize));
    }
    if (params.checkInFrom) {
      q.set('checkInFrom', params.checkInFrom);
    }
    if (params.checkInTo) {
      q.set('checkInTo', params.checkInTo);
    }
    if (params.checkOutFrom) {
      q.set('checkOutFrom', params.checkOutFrom);
    }
    if (params.checkOutTo) {
      q.set('checkOutTo', params.checkOutTo);
    }
    if (params.stayPhase && params.stayPhase !== 'All') {
      q.set('stayPhase', params.stayPhase);
    }
    if (params.status) {
      q.set('status', params.status);
    }

    const query = q.toString();
    const url = `${this.baseUrl}/api/business/bookings${query ? `?${query}` : ''}`;

    return this.http.get<unknown>(url).pipe(
      map((body) => normalizeApiResult<PagedResultDto<BookingSummaryDto>>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<PagedResultDto<BookingSummaryDto>>(err)),
      ),
    );
  }

  getBooking(bookingId: string): Observable<BookingDetailApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/bookings/${bookingId}`).pipe(
      map((body) => normalizeApiResult<BookingDetailDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BookingDetailDto>(err)),
      ),
    );
  }

  createBooking(body: CreateBookingRequest): Observable<BookingDetailApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/bookings`, body).pipe(
      map((res) => normalizeApiResult<BookingDetailDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BookingDetailDto>(err)),
      ),
    );
  }

  updateStatus(bookingId: string, status: string): Observable<BookingDetailApiResponse> {
    return this.http
      .patch<unknown>(`${this.baseUrl}/api/business/bookings/${bookingId}/status`, {
        status,
      })
      .pipe(
        map((res) => normalizeApiResult<BookingDetailDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<BookingDetailDto>(err)),
        ),
      );
  }

  getAvailability(
    checkInDate: string,
    checkOutDate: string,
    roomId?: string | null,
    locationId?: string | null,
  ): Observable<RoomAvailabilityApiResponse> {
    let url = `${this.baseUrl}/api/business/bookings/availability?checkInDate=${encodeURIComponent(checkInDate)}&checkOutDate=${encodeURIComponent(checkOutDate)}`;
    if (roomId) {
      url += `&roomId=${encodeURIComponent(roomId)}`;
    }
    if (locationId) {
      url += `&locationId=${encodeURIComponent(locationId)}`;
    }

    return this.http.get<unknown>(url).pipe(
      map((body) => normalizeApiResult<RoomAvailabilityDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RoomAvailabilityDto[]>(err)),
      ),
    );
  }
}
