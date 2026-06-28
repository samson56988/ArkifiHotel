import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthApiService } from '../services/auth-api.service';
import { API_BASE_URL } from '../tokens/api-base-url.token';

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authApi = inject(AuthApiService);
  const baseUrl = inject(API_BASE_URL);

  if (!req.url.startsWith(baseUrl)) {
    return next(req);
  }

  if (req.url.endsWith('/api/platform/auth/login')) {
    return next(req);
  }

  const token = authApi.getAccessToken();
  if (!token) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    }),
  );
};
