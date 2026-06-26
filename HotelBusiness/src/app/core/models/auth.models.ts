import type { ApiResult } from './api-result.model';

export interface RegisterBusinessRequest {
  businessName: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  acceptTerms: boolean;
  phoneNumber: string;
  slug: string;
  businessType: 'Hotel' | 'Shortlet';
  planCode: string;
}

export interface BusinessRegistrationDto {
  id: string;
  businessName: string;
  firstName: string;
  lastName: string;
  contactEmail: string;
  isEmailVerified: boolean;
  status: string;
  createdAt: string;
  termsAcceptedAt: string;
  phoneNumber: string;
  slug: string | null;
  logoUrl: string | null;
}

export type RegisterApiResponse = ApiResult<BusinessRegistrationDto>;

export interface LoginBusinessRequest {
  login: string;
  password: string;
  rememberMe: boolean;
}

export interface BusinessAccountDto {
  id: string;
  businessName: string;
  contactEmail: string;
  isEmailVerified: boolean;
  status: string;
  userId?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  isSuperAdmin?: boolean;
  username?: string | null;
  hasAllModuleAccess?: boolean;
  requiresPasswordChange?: boolean;
  twoFactorEmail?: string | null;
  moduleCodes?: string[];
  hasAllLocationAccess?: boolean;
  defaultLocationId?: string | null;
  locationIds?: string[];
}

export interface LoginBusinessData {
  accessToken: string | null;
  expiresAtUtc: string | null;
  account: BusinessAccountDto | null;
  requiresTwoFactor: boolean;
  challengeId: string | null;
  challengeExpiresAtUtc: string | null;
  requiresPasswordChange?: boolean;
}

export type LoginApiResponse = ApiResult<LoginBusinessData>;

export interface VerifyEmailOtpRequest {
  email: string;
  otp: string;
}

export type VerifyEmailOtpResponse = ApiResult<null>;

export interface VerifyLoginOtpRequest {
  email: string;
  otp: string;
  challengeId: string;
  rememberMe: boolean;
}

export type VerifyLoginOtpResponse = ApiResult<LoginBusinessData>;

export interface RequestPasswordResetRequest {
  email: string;
}

export interface RequestPasswordResetData {
  challengeId: string;
  challengeExpiresAtUtc: string;
}

export type RequestPasswordResetResponse = ApiResult<RequestPasswordResetData | null>;

export interface ResetPasswordRequest {
  email: string;
  challengeId: string;
  otp: string;
  newPassword: string;
}

export type ResetPasswordResponse = ApiResult<null>;

export type { ChangeDefaultPasswordRequest, ChangeDefaultPasswordResponse } from './team.models';
