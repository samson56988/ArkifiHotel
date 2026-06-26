/** Demo shortlet storefront slugs — use dedicated shortlet UI, not hotel layout. */
export const SHORTLET_SLUGS = ['nomad-stays', 'skyline-apartments'] as const;

export type ShortletSlug = (typeof SHORTLET_SLUGS)[number];

export function isShortletSlug(slug: string): slug is ShortletSlug {
  return (SHORTLET_SLUGS as readonly string[]).includes(slug);
}
