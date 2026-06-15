import type { ApiResult } from './api-result.model';

export interface BusinessSocialProfileDto {
  facebookUrl: string | null;
  instagramUrl: string | null;
  tikTokUrl: string | null;
  xUrl: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
}

export interface UpdateBusinessSocialProfileRequest {
  facebookUrl?: string | null;
  instagramUrl?: string | null;
  tikTokUrl?: string | null;
  xUrl?: string | null;
  contactEmail?: string | null;
  contactPhone?: string | null;
}

export type BusinessSocialProfileApiResponse = ApiResult<BusinessSocialProfileDto>;

export const EMPTY_BUSINESS_SOCIAL_PROFILE: BusinessSocialProfileDto = {
  facebookUrl: null,
  instagramUrl: null,
  tikTokUrl: null,
  xUrl: null,
  contactEmail: null,
  contactPhone: null,
};
