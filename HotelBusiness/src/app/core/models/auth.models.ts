import type { ApiResult } from './api-result.model';

export interface RegisterBusinessRequest {
  businessName: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  acceptTerms: boolean;
  phoneNumber: string;
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
}

export type RegisterApiResponse = ApiResult<BusinessRegistrationDto>;

export interface LoginBusinessRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface BusinessAccountDto {
  id: string;
  businessName: string;
  contactEmail: string;
  isEmailVerified: boolean;
  status: string;
}

export interface LoginBusinessData {
  accessToken: string | null;
  expiresAtUtc: string | null;
  account: BusinessAccountDto | null;
  requiresTwoFactor: boolean;
  challengeId: string | null;
  challengeExpiresAtUtc: string | null;
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
