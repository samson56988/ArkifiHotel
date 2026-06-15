import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  BusinessSocialProfileApiResponse,
  BusinessSocialProfileDto,
  UpdateBusinessSocialProfileRequest,
} from '../models/business-social-profile.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessSocialProfileApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getProfile(): Observable<BusinessSocialProfileApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/social-profile`).pipe(
      map((body) => normalizeApiResult<BusinessSocialProfileDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessSocialProfileDto>(err)),
      ),
    );
  }

  updateProfile(body: UpdateBusinessSocialProfileRequest): Observable<BusinessSocialProfileApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/social-profile`, body).pipe(
      map((body) => normalizeApiResult<BusinessSocialProfileDto>(body)),
      catchError((err: HttpErrorResponse) =>
        throwError(() => parseHttpApiResult<BusinessSocialProfileDto>(err)),
      ),
    );
  }
}
