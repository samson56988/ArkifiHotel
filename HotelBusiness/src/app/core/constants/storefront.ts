/** Public customer storefront base URL shown in slug hints. */
export const CUSTOMER_STOREFRONT_BASE_URL = 'https://arkifiStay.com';

export function buildStorefrontUrl(slug: string): string {
  const normalized = slug.trim().replace(/^\/+|\/+$/g, '');
  return normalized ? `${CUSTOMER_STOREFRONT_BASE_URL}/${normalized}` : CUSTOMER_STOREFRONT_BASE_URL;
}
