import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type {
  BusinessRegistrationDto,
  ChangeDefaultPasswordRequest,
  ChangeDefaultPasswordResponse,
  LoginApiResponse,
  LoginBusinessData,
  LoginBusinessRequest,
  RegisterApiResponse,
  RegisterBusinessRequest,
  VerifyEmailOtpRequest,
  VerifyEmailOtpResponse,
  VerifyLoginOtpRequest,
  VerifyLoginOtpResponse,
  RequestPasswordResetData,
  RequestPasswordResetRequest,
  RequestPasswordResetResponse,
  ResetPasswordRequest,
  ResetPasswordResponse,
} from '../models/auth.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { OrganizationAccessService } from './organization-access.service';
import { OrganizationLocationService } from './organization-location.service';
import { parseHttpApiResult } from '../utils/http-api-result';

const TOKEN_KEY = 'arkifihub_business_token';
const TOKEN_EXPIRES_KEY = 'arkifihub_business_token_expires';
const ACCOUNT_KEY = 'arkifihub_business_account';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly orgAccess = inject(OrganizationAccessService);
  private readonly orgLocation = inject(OrganizationLocationService);

  register(request: RegisterBusinessRequest, logo?: File | null): Observable<RegisterApiResponse> {
    const url = `${this.baseUrl}/api/Registration`;
    const form = new FormData();
    form.append('businessName', request.businessName);
    form.append('slug', request.slug);
    form.append('firstName', request.firstName);
    form.append('lastName', request.lastName);
    form.append('email', request.email);
    form.append('phoneNumber', request.phoneNumber);
    form.append('password', request.password);
    form.append('acceptTerms', String(request.acceptTerms));
    form.append('businessType', request.businessType);
    form.append('planCode', request.planCode);
    if (logo) {
      form.append('logo', logo, logo.name);
    }

    return this.http.post<RegisterApiResponse>(url, form).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessRegistrationDto>(err)),
      ),
    );
  }

  login(request: LoginBusinessRequest): Observable<LoginApiResponse> {
    const url = `${this.baseUrl}/api/Auth/login`;
    const body = {
      login: request.login,
      password: request.password,
      rememberMe: request.rememberMe,
    };
    return this.http.post<LoginApiResponse>(url, body).pipe(
      map((body) => {
        if (body.success && body.data?.accessToken) {
          this.persistSession(request.rememberMe === true, body.data);
        }
        return body;
      }),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<LoginBusinessData>(err)),
      ),
    );
  }

  verifyEmailOtp(request: VerifyEmailOtpRequest): Observable<VerifyEmailOtpResponse> {
    const url = `${this.baseUrl}/api/Auth/verify-email-otp`;
    return this.http.post<VerifyEmailOtpResponse>(url, request).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<null>(err)),
      ),
    );
  }

  verifyLoginOtp(request: VerifyLoginOtpRequest): Observable<VerifyLoginOtpResponse> {
    const url = `${this.baseUrl}/api/Auth/verify-login-otp`;
    return this.http.post<VerifyLoginOtpResponse>(url, request).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<LoginBusinessData>(err)),
      ),
    );
  }

  requestPasswordReset(request: RequestPasswordResetRequest): Observable<RequestPasswordResetResponse> {
    const url = `${this.baseUrl}/api/Auth/forgot-password`;
    return this.http.post<RequestPasswordResetResponse>(url, request).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<RequestPasswordResetData | null>(err)),
      ),
    );
  }

  resetPassword(request: ResetPasswordRequest): Observable<ResetPasswordResponse> {
    const url = `${this.baseUrl}/api/Auth/reset-password`;
    return this.http.post<ResetPasswordResponse>(url, request).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<null>(err)),
      ),
    );
  }

  changeDefaultPassword(request: ChangeDefaultPasswordRequest): Observable<ChangeDefaultPasswordResponse> {
    const url = `${this.baseUrl}/api/Auth/change-default-password`;
    return this.http.post<ChangeDefaultPasswordResponse>(url, request).pipe(
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<null>(err)),
      ),
    );
  }

  /**
   * Call after a successful login or verify-login-otp response so the JWT is stored
   * before any authenticated API calls or router navigation.
   */
  saveSessionFromLogin(rememberMe: boolean, data: LoginBusinessData): void {
    this.persistSession(rememberMe, data);
  }

  clearSession(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_EXPIRES_KEY);
    sessionStorage.removeItem(ACCOUNT_KEY);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(TOKEN_EXPIRES_KEY);
    localStorage.removeItem(ACCOUNT_KEY);
    this.orgAccess.setAccount(null);
    this.orgLocation.setAccount(null);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY) ?? sessionStorage.getItem(TOKEN_KEY);
  }

  private persistSession(rememberMe: boolean, data: LoginBusinessData): void {
    if (!data.accessToken || !data.expiresAtUtc || !data.account) {
      return;
    }

    const useLocalStorage = rememberMe === true;
    this.clearSession();
    const store = useLocalStorage ? localStorage : sessionStorage;
    store.setItem(TOKEN_KEY, data.accessToken);
    store.setItem(TOKEN_EXPIRES_KEY, data.expiresAtUtc);
    store.setItem(ACCOUNT_KEY, JSON.stringify(data.account));
    this.orgAccess.setAccount(data.account);
    this.orgLocation.setAccount(data.account);
  }

}
