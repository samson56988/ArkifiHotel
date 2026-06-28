export interface PropertyIconInput {
  logoUrl?: string | null;
  businessName: string;
  primaryColor?: string | null;
  accentColor?: string | null;
}

export function resolvePropertyIconUrl(input: PropertyIconInput): string {
  const logo = input.logoUrl?.trim();
  if (logo) {
    return logo;
  }

  return buildPropertyMarkDataUrl(
    input.businessName,
    input.primaryColor ?? '#1d2632',
    input.accentColor ?? '#c9a84c',
  );
}

function buildPropertyMarkDataUrl(
  businessName: string,
  primaryColor: string,
  accentColor: string,
): string {
  const letter = (businessName.trim().charAt(0) || 'H').toUpperCase();
  const primary = sanitizeColor(primaryColor, '#1d2632');
  const accent = sanitizeColor(accentColor, '#c9a84c');
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32" role="img" aria-label="${escapeXml(
    businessName,
  )}"><rect width="32" height="32" rx="7" fill="${primary}"/><rect x="5" y="5" width="22" height="22" rx="5" fill="${accent}"/><text x="16" y="22" text-anchor="middle" font-family="Georgia,'Times New Roman',serif" font-size="17" font-weight="700" fill="${primary}">${letter}</text></svg>`;

  return `data:image/svg+xml,${encodeURIComponent(svg)}`;
}

function sanitizeColor(value: string, fallback: string): string {
  const trimmed = value.trim();
  return /^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/.test(trimmed) ? trimmed : fallback;
}

function escapeXml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}
