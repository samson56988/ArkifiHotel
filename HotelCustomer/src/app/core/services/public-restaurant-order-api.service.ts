import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

export interface GuestCreateRestaurantOrderLineRequest {
  menuItemId: string;
  quantity: number;
}

export interface GuestCreateRestaurantOrderRequest {
  locationId: string;
  guestType: 'inRestaurant' | 'roomGuest';
  roomNumber?: string | null;
  guestPhone: string;
  items: GuestCreateRestaurantOrderLineRequest[];
}

export interface GuestRestaurantOrderCheckoutDto {
  orderId: string;
  orderNumber: string;
  paymentReference: string;
  paymentUrl: string;
  provider: string;
  amount: number;
  currency: string;
}

export interface GuestRestaurantOrderLineDto {
  itemName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface GuestRestaurantOrderLookupDto {
  orderNumber: string;
  propertyName: string;
  guestType: string;
  roomNumber: string | null;
  guestPhone: string;
  status: string;
  totalAmount: number;
  currency: string;
  lines: GuestRestaurantOrderLineDto[];
}

export interface GuestRestaurantOrderVerifyResultDto {
  paymentSuccessful: boolean;
  status: string;
  message?: string | null;
  order?: GuestRestaurantOrderLookupDto | null;
}

export type GuestRestaurantOrderCheckoutResponse = ApiResult<GuestRestaurantOrderCheckoutDto>;
export type GuestRestaurantOrderVerifyResponse = ApiResult<GuestRestaurantOrderVerifyResultDto>;

@Injectable({ providedIn: 'root' })
export class PublicRestaurantOrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  createCheckout(slug: string, body: GuestCreateRestaurantOrderRequest): Observable<GuestRestaurantOrderCheckoutResponse> {
    const encoded = encodeURIComponent(slug);
    return this.http.post<unknown>(`${this.baseUrl}/api/public/stores/${encoded}/restaurant-orders`, body).pipe(
      map((res) => normalizeApiResult<GuestRestaurantOrderCheckoutDto>(res)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<GuestRestaurantOrderCheckoutDto>(err)),
      ),
    );
  }

  verifyPayment(slug: string, reference: string): Observable<GuestRestaurantOrderVerifyResponse> {
    const encoded = encodeURIComponent(slug);
    const ref = encodeURIComponent(reference);
    return this.http
      .get<unknown>(`${this.baseUrl}/api/public/stores/${encoded}/restaurant-orders/payment/verify?reference=${ref}`)
      .pipe(
        map((res) => normalizeApiResult<GuestRestaurantOrderVerifyResultDto>(res)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<GuestRestaurantOrderVerifyResultDto>(err)),
        ),
      );
  }
}
