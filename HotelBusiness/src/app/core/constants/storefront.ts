import { isDevMode } from '@angular/core';

/** Production guest storefront host. */
export const CUSTOMER_STOREFRONT_PRODUCTION_URL = 'https://arkifiStay.com';

/** Local HotelCustomer dev server (see HotelCustomer/angular.json serve port). */
export const CUSTOMER_STOREFRONT_DEV_URL = 'http://localhost:4201';

/** Base URL for guest storefront links in UI. */
export const CUSTOMER_STOREFRONT_BASE_URL = isDevMode()
  ? CUSTOMER_STOREFRONT_DEV_URL
  : CUSTOMER_STOREFRONT_PRODUCTION_URL;

export function buildStorefrontUrl(slug: string): string {
  const normalized = slug.trim().replace(/^\/+|\/+$/g, '');
  return normalized ? `${CUSTOMER_STOREFRONT_BASE_URL}/${normalized}` : CUSTOMER_STOREFRONT_BASE_URL;
}
