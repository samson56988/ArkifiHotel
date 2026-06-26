import type { ApiResult } from './api-result.model';

export interface EventHallImageDto {
  id: string;
  url: string;
  originalFileName: string | null;
  sortOrder: number;
}

export interface EventHallSummaryDto {
  id: string;
  name: string;
  rentalPrice: number;
  maxCapacity: number | null;
  primaryImageUrl: string | null;
  imageCount: number;
  locationId: string;
  locationName: string | null;
  isArchived: boolean;
}

export interface EventHallDetailDto {
  id: string;
  name: string;
  description: string | null;
  rentalPrice: number;
  maxCapacity: number | null;
  locationId: string;
  locationName: string | null;
  isArchived: boolean;
  images: EventHallImageDto[];
}

export interface CreateEventHallRequest {
  name: string;
  description: string | null;
  rentalPrice: number;
  maxCapacity: number | null;
  locationId: string;
}

export interface UpdateEventHallRequest {
  name: string;
  description: string | null;
  rentalPrice: number;
  maxCapacity: number | null;
  locationId: string;
}

export interface EventHallRequestListItemDto {
  id: string;
  eventHallName: string;
  guestName: string;
  guestEmail: string;
  guestPhone: string;
  eventDate: string;
  eventEndDate: string | null;
  eventPurpose: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';
  locationName: string | null;
  createdAt: string;
}

export interface EventHallRequestDetailDto extends EventHallRequestListItemDto {
  eventHallId: string;
  notes: string | null;
}

export type EventHallsListApiResponse = ApiResult<EventHallSummaryDto[]>;
export type EventHallDetailApiResponse = ApiResult<EventHallDetailDto>;
export type EventHallRequestsListApiResponse = ApiResult<EventHallRequestListItemDto[]>;
export type EventHallRequestDetailApiResponse = ApiResult<EventHallRequestDetailDto>;
