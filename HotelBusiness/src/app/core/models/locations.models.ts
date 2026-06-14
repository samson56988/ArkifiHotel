import type { ApiResult } from './api-result.model';

export interface BusinessLocationDto {
  id: string;
  name: string;
  address: string | null;
  createdAt: string;
}

export interface CreateBusinessLocationRequest {
  name: string;
  address?: string | null;
}

export type UpdateBusinessLocationRequest = CreateBusinessLocationRequest;

export type LocationsListApiResponse = ApiResult<BusinessLocationDto[]>;
export type LocationDetailApiResponse = ApiResult<BusinessLocationDto>;
