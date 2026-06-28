import type { StorefrontTheme } from '../models/storefront-theme.models';
import type { ShortletShowcase } from '../models/shortlet-showcase.models';
import { themeCssVariables } from '../data/storefront-theme-presets';

/** Residential shortlet palette — warm, airy, distinct from hotel storefront. */
export function shortletThemeStyle(theme: StorefrontTheme): Record<string, string> {
  const base = themeCssVariables(theme);
  return {
    ...base,
    '--sl-bg': '#faf8f5',
    '--sl-surface': '#ffffff',
    '--sl-ink': theme.colors.text || '#1a1a1a',
    '--sl-muted': '#6b6560',
    '--sl-accent': theme.colors.accent || '#e07a5f',
    '--sl-accent-soft': 'color-mix(in srgb, var(--sl-accent) 14%, #fff)',
    '--sl-primary': theme.colors.primary || '#3d405b',
    '--sl-radius': '16px',
    '--sl-shadow': '0 12px 40px rgba(26, 26, 26, 0.08)',
    '--font-display': "'Fraunces', Georgia, serif",
    '--font-body': "'DM Sans', system-ui, sans-serif",
  };
}

export function formatNaira(amount: number): string {
  return new Intl.NumberFormat('en-NG', {
    style: 'currency',
    currency: 'NGN',
    maximumFractionDigits: 0,
  }).format(amount);
}

export function shortletHeroSubtitle(shortlet: ShortletShowcase): string {
  const intro = shortlet.theme.contact.introText?.trim();
  if (intro) {
    return intro.replace('{count}', String(shortlet.listings.length));
  }
  const subtitle = shortlet.theme.rooms.subtitle?.trim();
  if (subtitle) {
    return subtitle.replace('{count}', String(shortlet.listings.length));
  }
  return `Fully furnished apartments · Self check-in · ${shortlet.listings.length} units available`;
}

export function shortletListingsSubtitle(shortlet: ShortletShowcase): string {
  const sub = shortlet.theme.rooms.subtitle?.trim();
  if (sub) {
    return sub.replace('{count}', String(shortlet.listings.length));
  }
  return `${shortlet.listings.length} furnished apartments — each with full kitchen, Wi‑Fi, and self check-in.`;
}

export function shortletAmenitiesPageSubtitle(shortlet: ShortletShowcase): string {
  const sub = shortlet.theme.facilities.subtitle?.trim();
  if (sub) {
    return sub.replace('{name}', shortlet.businessName);
  }
  return `Every ${shortlet.businessName} apartment includes these essentials — no extra fees, no surprises.`;
}
