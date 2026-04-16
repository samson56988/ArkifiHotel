import type { ApiResult } from './api-result.model';

export interface CustomerSummaryDto {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  createdAt: string;
}

export interface CustomerDetailDto {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCustomerRequest {
  fullName: string;
  email: string;
  phone?: string | null;
  notes?: string | null;
}

export interface UpdateCustomerRequest {
  fullName: string;
  email: string;
  phone?: string | null;
  notes?: string | null;
}

export type CustomersListApiResponse = ApiResult<CustomerSummaryDto[]>;
export type CustomerDetailApiResponse = ApiResult<CustomerDetailDto>;
