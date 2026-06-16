import type { PublicStorefront, StorefrontTheme } from '../models/storefront-theme.models';
import { resolveSectionFont, themeCssVariables } from '../data/storefront-theme-presets';

export function hotelThemeStyle(theme: StorefrontTheme): Record<string, string> {
  const base = themeCssVariables(theme);
  const primary = theme.colors.primary;
  const accent = theme.colors.accent;

  return {
    ...base,
    '--primary': primary,
    '--accent': accent,
    '--accent-light': accent,
    '--bg': theme.colors.background,
    '--bg-alt': '#ffffff',
    '--text': theme.colors.text,
    '--text-mid': theme.colors.text,
    '--text-low': theme.colors.text,
    '--border': '#e2ddd6',
    '--hero-overlay': `rgba(0,0,0,${Math.min(theme.banner.overlayOpacity / 100, 0.65)})`,
    '--radius': '14px',
    '--font-heading': resolveSectionFont(theme.globalFont, 'display'),
    '--font-body': resolveSectionFont(theme.globalFont, 'body'),
  };
}

export function formatNaira(amount: number): string {
  return `₦${amount.toLocaleString('en-NG', { maximumFractionDigits: 0 })}`;
}

export function facilityEmoji(name: string): string {
  const n = name.toLowerCase();
  if (n.includes('pool') || n.includes('swim')) return '🏊';
  if (n.includes('wifi') || n.includes('internet')) return '📶';
  if (n.includes('restaurant') || n.includes('dining') || n.includes('dine')) return '🍽️';
  if (n.includes('gym') || n.includes('fitness')) return '🏋️';
  if (n.includes('park')) return '🚗';
  if (n.includes('spa') || n.includes('wellness')) return '🧖';
  if (n.includes('conference') || n.includes('meeting')) return '🎰';
  if (n.includes('service') || n.includes('concierge')) return '🛎️';
  if (n.includes('coffee') || n.includes('café') || n.includes('cafe')) return '☕';
  if (n.includes('laundry')) return '🧺';
  return '✦';
}
