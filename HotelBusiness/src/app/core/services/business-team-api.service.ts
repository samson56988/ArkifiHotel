import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  BusinessTeamInviteDto,
  BusinessTeamMemberDto,
  CreateBusinessTeamMemberRequest,
  OrganizationModuleDefinitionDto,
  SetBusinessTeamMemberStatusRequest,
  TeamInviteApiResponse,
  TeamInvitesApiResponse,
  TeamMemberApiResponse,
  TeamMembersApiResponse,
  TeamModulesApiResponse,
  UpdateBusinessTeamMemberRequest,
} from '../models/team.models';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { normalizeApiResult, parseHttpApiResult } from '../utils/http-api-result';

@Injectable({ providedIn: 'root' })
export class BusinessTeamApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  listModules(): Observable<TeamModulesApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/team/modules`).pipe(
      map((body) => normalizeApiResult<OrganizationModuleDefinitionDto[]>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  listMembers(): Observable<TeamMembersApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/team`).pipe(
      map((body) => normalizeApiResult<BusinessTeamMemberDto[]>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  createMember(body: CreateBusinessTeamMemberRequest): Observable<TeamMemberApiResponse> {
    return this.http.post<unknown>(`${this.baseUrl}/api/business/team`, body).pipe(
      map((res) => normalizeApiResult<BusinessTeamMemberDto>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  updateMember(id: string, body: UpdateBusinessTeamMemberRequest): Observable<TeamMemberApiResponse> {
    return this.http.put<unknown>(`${this.baseUrl}/api/business/team/${encodeURIComponent(id)}`, body).pipe(
      map((res) => normalizeApiResult<BusinessTeamMemberDto>(res)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  setMemberStatus(id: string, isActive: boolean): Observable<TeamMemberApiResponse> {
    const body: SetBusinessTeamMemberStatusRequest = { isActive };
    return this.http
      .patch<unknown>(`${this.baseUrl}/api/business/team/${encodeURIComponent(id)}/status`, body)
      .pipe(
        map((res) => normalizeApiResult<BusinessTeamMemberDto>(res)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
      );
  }

  listInvites(): Observable<TeamInvitesApiResponse> {
    return this.http.get<unknown>(`${this.baseUrl}/api/business/team/invites`).pipe(
      map((body) => normalizeApiResult<BusinessTeamInviteDto[]>(body)),
      catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
    );
  }

  resendInvite(id: string): Observable<TeamInviteApiResponse> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/business/team/${encodeURIComponent(id)}/resend-invite`, {})
      .pipe(
        map((res) => normalizeApiResult<BusinessTeamInviteDto>(res)),
        catchError((err: HttpErrorResponse) => throwError(() => parseHttpApiResult(err))),
      );
  }
}
