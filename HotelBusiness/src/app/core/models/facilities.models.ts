import type { ApiResult } from './api-result.model';

export interface FacilityImageDto {
  id: string;
  url: string;
  originalFileName: string | null;
  sortOrder: number;
}

export interface PropertyFacilitySummaryDto {
  id: string;
  name: string;
  primaryImageUrl: string | null;
  imageCount: number;
  isArchived: boolean;
}

export interface PropertyFacilityDetailDto {
  id: string;
  name: string;
  description: string | null;
  images: FacilityImageDto[];
  isArchived: boolean;
}

export interface CreatePropertyFacilityRequest {
  name: string;
  description?: string | null;
}

export type UpdatePropertyFacilityRequest = CreatePropertyFacilityRequest;

export type FacilitiesListApiResponse = ApiResult<PropertyFacilitySummaryDto[]>;
export type FacilityDetailApiResponse = ApiResult<PropertyFacilityDetailDto>;
export type FacilityImagesUploadResponse = ApiResult<FacilityImageDto[]>;
