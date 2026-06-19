import type { ApiResult } from './api-result.model';

export interface RestaurantOrderLineDto {
  itemName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface RestaurantOrderListItemDto {
  id: string;
  orderNumber: string;
  guestType: 'inRestaurant' | 'roomGuest';
  roomNumber: string | null;
  guestPhone: string;
  status: string;
  totalAmount: number;
  currency: string;
  itemCount: number;
  createdAt: string;
}

export interface RestaurantOrderDetailDto {
  id: string;
  orderNumber: string;
  guestType: 'inRestaurant' | 'roomGuest';
  roomNumber: string | null;
  guestPhone: string;
  status: string;
  totalAmount: number;
  currency: string;
  locationName: string | null;
  createdAt: string;
  lines: RestaurantOrderLineDto[];
}

export interface RestaurantOrderListResultDto {
  items: RestaurantOrderListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ListRestaurantOrdersParams {
  page?: number;
  pageSize?: number;
  status?: string;
}

export type RestaurantOrderListApiResponse = ApiResult<RestaurantOrderListResultDto>;
export type RestaurantOrderDetailApiResponse = ApiResult<RestaurantOrderDetailDto>;
