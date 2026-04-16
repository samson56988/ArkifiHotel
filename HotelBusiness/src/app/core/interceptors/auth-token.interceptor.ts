import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthApiService } from '../services/auth-api.service';
import { API_BASE_URL } from '../tokens/api-base-url.token';

/** Adds Bearer token to ArkifiHotel API requests after business login. */
export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authApi = inject(AuthApiService);
  const baseUrl = inject(API_BASE_URL);

  if (!req.url.startsWith(baseUrl)) {
    return next(req);
  }

  const token = authApi.getAccessToken();
  if (!token) {
    return next(req);
  }

  // Avoid sending stale token to login/register endpoints.
  if (
    req.url.endsWith('/api/Auth/login') ||
    req.url.endsWith('/api/Auth/verify-login-otp') ||
    req.url.endsWith('/api/Registration') ||
    req.url.endsWith('/api/Auth/verify-email-otp')
  ) {
    return next(req);
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authReq);
};
