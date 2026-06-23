import type { ApiResult } from './api-result.model';

export interface DashboardDateRange {
  from: string;
  to: string;
}

export type DashboardRangePreset = 'last7' | 'last30' | 'thisMonth' | 'custom';

export interface DashboardRecentBookingDto {
  id: string;
  guestName: string;
  roomName: string;
  checkInDate: string;
  nights: number;
  status: string;
  totalAmount: number;
  currency: string;
}

export interface DashboardGatewaySplitDto {
  label: string;
  amount: number;
  percent: number;
}

export interface DashboardTopRoomDto {
  roomId: string;
  roomName: string;
  revenue: number;
  occupancyPercent: number;
  currency: string;
}

export interface DashboardRevenueTrendPointDto {
  date: string;
  bookingRevenue: number;
  restaurantRevenue: number;
  totalRevenue: number;
}

export interface BusinessDashboardDto {
  businessName: string;
  currency: string;
  periodStart: string;
  periodEnd: string;
  totalRevenue: number;
  bookingRevenue: number;
  restaurantRevenue: number;
  totalRevenueChangePercent: number | null;
  occupancyRatePercent: number;
  occupancyChangePercent: number | null;
  totalRoomUnits: number;
  roomTypesCount: number;
  activeStaysToday: number;
  pendingRestaurantOrders: number;
  pendingEventHallRequests: number;
  revenueTrend: DashboardRevenueTrendPointDto[];
  recentBookings: DashboardRecentBookingDto[];
  gatewaySplit: DashboardGatewaySplitDto[];
  topRooms: DashboardTopRoomDto[];
}

export type BusinessDashboardApiResponse = ApiResult<BusinessDashboardDto>;
