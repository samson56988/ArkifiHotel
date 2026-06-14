import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  CreateCustomerRequest,
  CustomerDetailApiResponse,
  CustomerDetailDto,
  CustomerSummaryDto,
  CustomersListApiResponse,
  UpdateCustomerRequest,
} from '../models/customers.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessCustomersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listCustomers(): Observable<CustomersListApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/customers`).pipe(
      map((body) => normalizeApiResult<CustomerSummaryDto[]>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<CustomerSummaryDto[]>(err)),
      ),
    );
  }

  getCustomer(customerId: string): Observable<CustomerDetailApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/customers/${customerId}`).pipe(
      map((body) => normalizeApiResult<CustomerDetailDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<CustomerDetailDto>(err)),
      ),
    );
  }

  createCustomer(body: CreateCustomerRequest): Observable<CustomerDetailApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/customers`, body).pipe(
      map((body) => normalizeApiResult<CustomerDetailDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<CustomerDetailDto>(err)),
      ),
    );
  }

  updateCustomer(customerId: string, body: UpdateCustomerRequest): Observable<CustomerDetailApiResponse> {
    return this.http
      .put<unknown>(`${this.baseUrl}/api/business/customers/${customerId}`, body)
      .pipe(
        map((body) => normalizeApiResult<CustomerDetailDto>(body)),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<CustomerDetailDto>(err)),
        ),
      );
  }

  deleteCustomer(customerId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<unknown>(`${this.baseUrl}/api/business/customers/${customerId}`).pipe(
      map((body) => normalizeApiResult<unknown>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
    );
  }
}
