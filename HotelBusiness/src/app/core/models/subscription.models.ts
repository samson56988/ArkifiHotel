import type { ApiResult } from './api-result.model';

export interface SubscriptionPlanDto {
  id: string;
  code: string;
  name: string;
  description: string | null;
  tier: 'Free' | 'Pro';
  billingInterval: 'None' | 'Monthly' | 'Yearly';
  priceAmount: number;
  currency: string;
  yearlyDiscountPercent: number | null;
  sortOrder: number;
}

export interface SubscriptionPlanOptionDto {
  id: string;
  code: string;
  name: string;
  description: string | null;
  tier: 'Free' | 'Pro';
  billingInterval: 'None' | 'Monthly' | 'Yearly';
  priceAmount: number;
  currency: string;
  yearlyDiscountPercent: number | null;
  sortOrder: number;
  canSelect: boolean;
  requiresPayment: boolean;
  changeType: 'Current' | 'Upgrade' | 'Downgrade' | 'Renew';
  disabledReason: string | null;
}

export interface InitSubscriptionPaymentResultDto {
  paymentReference: string;
  paymentUrl: string;
  amount: number;
  currency: string;
  planCode: string;
  planName: string;
}

export interface BusinessSubscriptionDto {
  businessId: string;
  businessType: 'Hotel' | 'Shortlet';
  plan: SubscriptionPlanDto;
  expiresAt: string | null;
  status: 'Active' | 'GracePeriod' | 'Expired';
  gracePeriodDays: number;
  isStorefrontAccessible: boolean;
  daysRemaining: number | null;
}

export type SubscriptionPlansApiResponse = ApiResult<SubscriptionPlanDto[]>;
export type SubscriptionPlanOptionsApiResponse = ApiResult<SubscriptionPlanOptionDto[]>;
export interface BusinessSubscriptionPaymentHistoryDto {
  id: string;
  paymentReference: string;
  planName: string;
  planCode: string;
  amount: number;
  currency: string;
  status: 'Pending' | 'Completed' | 'Failed';
  createdAt: string;
  completedAt: string | null;
}

export type BusinessSubscriptionApiResponse = ApiResult<BusinessSubscriptionDto>;
export type InitSubscriptionPaymentApiResponse = ApiResult<InitSubscriptionPaymentResultDto>;
export type BusinessSubscriptionPaymentHistoryApiResponse = ApiResult<BusinessSubscriptionPaymentHistoryDto[]>;

export type BusinessTypeOption = 'Hotel' | 'Shortlet';
export type PlanTierOption = 'free' | 'pro';
