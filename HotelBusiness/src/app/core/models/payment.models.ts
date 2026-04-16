import type { ApiResult } from './api-result.model';

export interface PaymentConfigurationDto {
  provider: string;
  hasSecretKey: boolean;
}

export interface UpdatePaymentConfigurationRequest {
  provider: string;
  secretKey?: string | null;
}

export type PaymentConfigurationApiResponse = ApiResult<PaymentConfigurationDto>;
