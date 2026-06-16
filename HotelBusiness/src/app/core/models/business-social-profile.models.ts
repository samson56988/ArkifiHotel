import type { ApiResult } from './api-result.model';

export interface BusinessSocialProfileDto {
  facebookUrl: string | null;
  facebookHandle: string | null;
  facebookFollowers: string | null;
  instagramUrl: string | null;
  instagramHandle: string | null;
  instagramFollowers: string | null;
  tikTokUrl: string | null;
  tikTokHandle: string | null;
  tikTokFollowers: string | null;
  xUrl: string | null;
  xHandle: string | null;
  xFollowers: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
}

export interface UpdateBusinessSocialProfileRequest {
  facebookUrl?: string | null;
  facebookHandle?: string | null;
  facebookFollowers?: string | null;
  instagramUrl?: string | null;
  instagramHandle?: string | null;
  instagramFollowers?: string | null;
  tikTokUrl?: string | null;
  tikTokHandle?: string | null;
  tikTokFollowers?: string | null;
  xUrl?: string | null;
  xHandle?: string | null;
  xFollowers?: string | null;
  contactEmail?: string | null;
  contactPhone?: string | null;
}

export type BusinessSocialProfileApiResponse = ApiResult<BusinessSocialProfileDto>;

export const EMPTY_BUSINESS_SOCIAL_PROFILE: BusinessSocialProfileDto = {
  facebookUrl: null,
  facebookHandle: null,
  facebookFollowers: null,
  instagramUrl: null,
  instagramHandle: null,
  instagramFollowers: null,
  tikTokUrl: null,
  tikTokHandle: null,
  tikTokFollowers: null,
  xUrl: null,
  xHandle: null,
  xFollowers: null,
  contactEmail: null,
  contactPhone: null,
};
