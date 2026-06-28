import { environment } from '../../../environments/environment';

/** Base URL for guest storefront links in the business admin UI. */
export const CUSTOMER_STOREFRONT_BASE_URL = environment.customerAppUrl;

export function buildStorefrontUrl(slug: string): string {
  const normalized = slug.trim().replace(/^\/+|\/+$/g, '');
  return normalized ? `${CUSTOMER_STOREFRONT_BASE_URL}/${normalized}` : CUSTOMER_STOREFRONT_BASE_URL;
}
