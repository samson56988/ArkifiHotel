import type { ApiResult } from './api-result.model';
import type { AmenityDto } from './amenities.models';

export type { AmenityDto } from './amenities.models';

export interface RoomImageDto {
  id: string;
  url: string;
  originalFileName: string | null;
  sortOrder: number;
}

export interface BusinessRoomSummaryDto {
  id: string;
  name: string;
  tagline?: string | null;
  maxOccupancy: number;
  bedroomCount?: number | null;
  bathroomCount?: number | null;
  isGuestFavorite: boolean;
  basePricePerNight: number;
  quantity: number;
  locationId: string | null;
  locationName: string | null;
  primaryImageUrl: string | null;
  amenityCount: number;
  isArchived: boolean;
}

export interface BusinessRoomDetailDto {
  id: string;
  name: string;
  tagline?: string | null;
  description: string | null;
  maxOccupancy: number;
  bedroomCount?: number | null;
  bathroomCount?: number | null;
  isGuestFavorite: boolean;
  basePricePerNight: number;
  quantity: number;
  locationId: string | null;
  locationName: string | null;
  images: RoomImageDto[];
  amenities: AmenityDto[];
  isArchived: boolean;
}

export interface CreateBusinessRoomRequest {
  name: string;
  tagline?: string | null;
  description?: string | null;
  maxOccupancy: number;
  bedroomCount?: number | null;
  bathroomCount?: number | null;
  isGuestFavorite?: boolean;
  basePricePerNight: number;
  quantity: number;
  locationId: string;
  amenityIds?: string[] | null;
}

export type UpdateBusinessRoomRequest = CreateBusinessRoomRequest;

export type RoomsListApiResponse = ApiResult<BusinessRoomSummaryDto[]>;
export type RoomDetailApiResponse = ApiResult<BusinessRoomDetailDto>;
export type RoomImagesUploadResponse = ApiResult<RoomImageDto[]>;
