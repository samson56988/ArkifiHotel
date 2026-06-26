import type { ApiResult } from './api-result.model';

export interface OrganizationModuleDefinitionDto {
  code: string;
  label: string;
}

export interface BusinessTeamMemberDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  username: string | null;
  isSuperAdmin: boolean;
  isDefaultPassword: boolean;
  hasAllModuleAccess: boolean;
  hasAllLocationAccess: boolean;
  defaultLocationId: string | null;
  isActive: boolean;
  moduleCodes: string[];
  locationIds: string[];
  createdAt: string;
}

export interface CreateBusinessTeamMemberRequest {
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  hasAllModuleAccess: boolean;
  hasAllLocationAccess: boolean;
  defaultLocationId: string | null;
  moduleCodes: string[];
  locationIds: string[];
}

export interface UpdateBusinessTeamMemberRequest {
  firstName: string;
  lastName: string;
  email: string;
  hasAllModuleAccess: boolean;
  hasAllLocationAccess: boolean;
  defaultLocationId: string | null;
  isActive: boolean;
  moduleCodes: string[];
  locationIds: string[];
}

export interface SetBusinessTeamMemberStatusRequest {
  isActive: boolean;
}

export interface BusinessTeamInviteDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  username: string | null;
  staffLoginId: string;
  isActive: boolean;
  isPending: boolean;
  invitedAt: string;
  lastInviteSentAt: string;
}

export type TeamMembersApiResponse = ApiResult<BusinessTeamMemberDto[]>;
export type TeamMemberApiResponse = ApiResult<BusinessTeamMemberDto>;
export type TeamModulesApiResponse = ApiResult<OrganizationModuleDefinitionDto[]>;
export type TeamInvitesApiResponse = ApiResult<BusinessTeamInviteDto[]>;
export type TeamInviteApiResponse = ApiResult<BusinessTeamInviteDto>;

export interface ChangeDefaultPasswordRequest {
  login: string;
  currentPassword: string;
  newPassword: string;
}

export type ChangeDefaultPasswordResponse = ApiResult<null>;
