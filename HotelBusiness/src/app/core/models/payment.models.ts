import type { ApiResult } from './api-result.model';

export type PaymentProvider = 'None' | 'Paystack' | 'Flutterwave' | 'Monify';

export interface PaymentConfigurationDto {
  provider: string;
  isConfigured: boolean;
  hasSecretKey: boolean;
  hasApiKey: boolean;
  hasContractCode: boolean;
}

export interface UpdatePaymentConfigurationRequest {
  provider: string;
  secretKey?: string | null;
  apiKey?: string | null;
  contractCode?: string | null;
}

export type PaymentConfigurationApiResponse = ApiResult<PaymentConfigurationDto>;
