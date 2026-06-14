import type { ApiResult } from './api-result.model';

export interface AmenityDto {
  id: string;
  name: string;
  category: string | null;
  isCustom: boolean;
}

export interface CreateAmenityRequest {
  name: string;
  category?: string | null;
}

export type UpdateAmenityRequest = CreateAmenityRequest;

export type AmenitiesApiResponse = ApiResult<AmenityDto[]>;
export type AmenityApiResponse = ApiResult<AmenityDto>;
