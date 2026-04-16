import type { ApiResult } from './api-result.model';

export interface AmenityDto {
  id: string;
  name: string;
  category: string | null;
  isCustom: boolean;
}

export interface CreateCustomAmenityRequest {
  name: string;
  category?: string | null;
}

export interface RoomImageDto {
  id: string;
  url: string;
  originalFileName: string | null;
  sortOrder: number;
}

export interface BusinessRoomSummaryDto {
  id: string;
  name: string;
  maxOccupancy: number;
  basePricePerNight: number;
  primaryImageUrl: string | null;
  amenityCount: number;
  isArchived: boolean;
}

export interface BusinessRoomDetailDto {
  id: string;
  name: string;
  description: string | null;
  maxOccupancy: number;
  basePricePerNight: number;
  images: RoomImageDto[];
  amenities: AmenityDto[];
  isArchived: boolean;
}

export interface CreateBusinessRoomRequest {
  name: string;
  description?: string | null;
  maxOccupancy: number;
  basePricePerNight: number;
  amenityIds?: string[] | null;
}

export type UpdateBusinessRoomRequest = CreateBusinessRoomRequest;

export type AmenitiesApiResponse = ApiResult<AmenityDto[]>;
export type RoomsListApiResponse = ApiResult<BusinessRoomSummaryDto[]>;
export type RoomDetailApiResponse = ApiResult<BusinessRoomDetailDto>;
export type RoomImagesUploadResponse = ApiResult<RoomImageDto[]>;
