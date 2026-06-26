import type { ApiResult } from './api-result.model';

export interface BusinessProfileDto {
  id: string;
  businessName: string;
  slug: string | null;
  logoUrl: string | null;
  contactEmail: string;
  phoneNumber: string;
  firstName: string;
  lastName: string;
  isEmailVerified: boolean;
  status: string;
  createdAt: string;
  updatedAt: string | null;
  businessType?: 'Hotel' | 'Shortlet';
}

export interface UpdateBusinessProfileRequest {
  businessName: string;
  slug: string;
}

export interface SlugAvailabilityDto {
  slug: string;
  available: boolean;
}

export type BusinessProfileApiResponse = ApiResult<BusinessProfileDto>;
export type SlugAvailabilityApiResponse = ApiResult<SlugAvailabilityDto>;
