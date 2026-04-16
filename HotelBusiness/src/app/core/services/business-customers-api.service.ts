import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
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

@Injectable({ providedIn: 'root' })
export class BusinessCustomersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listCustomers(): Observable<CustomersListApiResponse> {
    return this.http.get<CustomersListApiResponse>(`${this.baseUrl}/api/business/customers`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<CustomerSummaryDto[]>(err)),
      ),
    );
  }

  getCustomer(customerId: string): Observable<CustomerDetailApiResponse> {
    return this.http.get<CustomerDetailApiResponse>(`${this.baseUrl}/api/business/customers/${customerId}`).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<CustomerDetailDto>(err)),
      ),
    );
  }

  createCustomer(body: CreateCustomerRequest): Observable<CustomerDetailApiResponse> {
    return this.http.post<CustomerDetailApiResponse>(`${this.baseUrl}/api/business/customers`, body).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => this.normalizeHttpError<CustomerDetailDto>(err)),
      ),
    );
  }

  updateCustomer(customerId: string, body: UpdateCustomerRequest): Observable<CustomerDetailApiResponse> {
    return this.http
      .put<CustomerDetailApiResponse>(`${this.baseUrl}/api/business/customers/${customerId}`, body)
      .pipe(
        catchError((err: HttpErrorResponse) =>
          throwError(() => this.normalizeHttpError<CustomerDetailDto>(err)),
        ),
      );
  }

  deleteCustomer(customerId: string): Observable<ApiResult<unknown>> {
    return this.http.delete<ApiResult<unknown>>(`${this.baseUrl}/api/business/customers/${customerId}`).pipe(
      catchError((err: HttpErrorResponse) => throwError(() => this.normalizeHttpError<unknown>(err))),
    );
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
