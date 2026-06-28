import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';
import type { ApiResult } from '../models/api-result.model';
import type { PlatformLoginData, PlatformStaffAccount } from '../models/platform.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

const TOKEN_KEY = 'arkifistay_platform_token';
const TOKEN_EXPIRES_KEY = 'arkifistay_platform_token_expires';
const ACCOUNT_KEY = 'arkifistay_platform_account';

export interface PlatformLoginRequest {
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  login(request: PlatformLoginRequest, rememberMe = false): Observable<ApiResult<PlatformLoginData>> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/platform/auth/login`, request)
      .pipe(
        map((body) => {
          const result = normalizeApiResult<PlatformLoginData>(body);
          if (result.success && result.data?.accessToken) {
            this.persistSession(rememberMe, result.data);
          }
          return result;
        }),
        catchError((err: HttpErrorResponse) =>
          throwError(() => parseHttpApiResult<PlatformLoginData>(err)),
        ),
      );
  }

  logout(): Observable<ApiResult<unknown>> {
    return this.http.post<unknown>(`${this.baseUrl}/api/platform/auth/logout`, {}).pipe(
      map((body) => normalizeApiResult<unknown>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult<unknown>(err))),
      finalize(() => this.clearSession()),
    );
  }

  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY) ?? sessionStorage.getItem(TOKEN_KEY);
  }

  getAccount(): PlatformStaffAccount | null {
    const raw = localStorage.getItem(ACCOUNT_KEY) ?? sessionStorage.getItem(ACCOUNT_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as PlatformStaffAccount;
    } catch {
      return null;
    }
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    if (!token) {
      return false;
    }

    const expiresRaw = localStorage.getItem(TOKEN_EXPIRES_KEY) ?? sessionStorage.getItem(TOKEN_EXPIRES_KEY);
    if (!expiresRaw) {
      return true;
    }

    const expires = Date.parse(expiresRaw);
    return Number.isNaN(expires) || expires > Date.now();
  }

  clearSession(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_EXPIRES_KEY);
    sessionStorage.removeItem(ACCOUNT_KEY);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(TOKEN_EXPIRES_KEY);
    localStorage.removeItem(ACCOUNT_KEY);
  }

  private persistSession(rememberMe: boolean, data: PlatformLoginData): void {
    const store = rememberMe ? localStorage : sessionStorage;
    this.clearSession();
    store.setItem(TOKEN_KEY, data.accessToken);
    store.setItem(TOKEN_EXPIRES_KEY, data.expiresAtUtc);
    store.setItem(ACCOUNT_KEY, JSON.stringify(data.account));
  }
}
