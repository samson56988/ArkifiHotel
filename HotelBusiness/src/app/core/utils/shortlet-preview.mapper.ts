import type { PublicStorefront, PublicStorefrontAmenity } from '../models/storefront-theme.models';

export type ShortletPreviewPage = 'home' | 'listings' | 'amenities' | 'host';

export interface ShortletPreviewListing {
  id: string;
  title: string;
  tagline: string;
  guests: number;
  beds: number;
  baths: number;
  nightlyPrice: number;
  image: string | null;
  amenities: string[];
  featured?: boolean;
}

export interface ShortletPreviewAmenity {
  id: string;
  label: string;
  icon: string;
  description: string;
}

export interface ShortletPreviewData {
  businessName: string;
  logoUrl: string | null;
  branchName: string | null;
  tagline: string;
  headline: string;
  neighborhood: string;
  heroImage: string | null;
  checkIn: string;
  checkOut: string;
  hostName: string;
  hostBio: string;
  hostPhoto: string | null;
  listingsTitle: string;
  listingsSubtitle: string;
  featuredTitle: string;
  amenitiesTitle: string;
  amenitiesSubtitle: string;
  listings: ShortletPreviewListing[];
  amenities: ShortletPreviewAmenity[];
  houseRules: string[];
  theme: PublicStorefront['theme'];
}

const ICONS: Record<string, string> = {
  wifi: '📶',
  kitchen: '🍳',
  parking: '🅿️',
  security: '🛡️',
  power: '⚡',
  gym: '🏋️',
};

export function isShortletBusinessType(businessType: string | undefined | null): boolean {
  return (businessType ?? '').toLowerCase() === 'shortlet';
}

export function mapPublicToShortletPreview(dto: PublicStorefront): ShortletPreviewData {
  const contact = dto.theme.contact;
  const locations = dto.locations ?? [];
  const effectiveId = dto.activeLocationId ?? null;
  const branch = effectiveId ? locations.find((l) => l.id === effectiveId) : null;
  const rooms =
    effectiveId && locations.length > 1
      ? dto.rooms.filter((r) => !r.locationId || r.locationId === effectiveId)
      : dto.rooms;

  const about = dto.theme.about;
  const hostName =
    about.quoteBy?.replace(/^[—–-]\s*/, '').trim() ||
    dto.social.contactEmail?.split('@')[0] ||
    'Your host';

  return {
    businessName: dto.businessName,
    logoUrl: dto.logoUrl ?? null,
    branchName: branch?.name ?? null,
    tagline: dto.theme.banner.subheadline?.trim() || dto.theme.banner.headline,
    headline: dto.theme.banner.headline,
    neighborhood: branch?.name || contact.location || dto.businessName,
    heroImage: dto.heroImages[0] ?? rooms[0]?.primaryImageUrl ?? null,
    checkIn: contact.checkIn || '3:00 PM',
    checkOut: contact.checkOut || '11:00 AM',
    hostName,
    hostBio:
      about.description?.trim() ||
      `Welcome to ${dto.businessName}. Furnished apartments for comfortable stays.`,
    hostPhoto: dto.aboutImageUrl ?? dto.heroImages[0] ?? null,
    listingsTitle: dto.theme.rooms.title || 'All listings',
    listingsSubtitle: dto.theme.rooms.subtitle || `${rooms.length} furnished apartments`,
    featuredTitle: dto.theme.rooms.featuredTitle || 'Featured apartments',
    amenitiesTitle: dto.theme.facilities.title || 'Amenities',
    amenitiesSubtitle:
      dto.theme.facilities.subtitle || 'Everything included in your stay',
    listings: rooms.map((room) => ({
      id: room.id,
      title: room.name,
      tagline:
        room.tagline?.trim() ||
        room.description?.split(/[.!?]/)[0]?.trim() ||
        `${room.maxOccupancy} guests · furnished apartment`,
      guests: room.maxOccupancy,
      beds: room.bedroomCount ?? Math.max(1, Math.ceil(room.maxOccupancy / 2)),
      baths: room.bathroomCount ?? ((room.bedroomCount ?? 1) >= 2 ? 2 : 1),
      nightlyPrice: room.basePricePerNight,
      image: room.primaryImageUrl,
      amenities: (room.amenityNames ?? []).slice(0, 3),
      featured: room.isGuestFavorite,
    })),
    amenities: (dto.amenities ?? []).map((a) => mapAmenity(a)),
    houseRules: buildRules(dto),
    theme: dto.theme,
  };
}

function mapAmenity(a: PublicStorefrontAmenity): ShortletPreviewAmenity {
  const lower = a.name.toLowerCase();
  let icon = '✓';
  for (const [k, v] of Object.entries(ICONS)) {
    if (lower.includes(k)) {
      icon = v;
      break;
    }
  }
  return {
    id: a.id,
    label: a.name,
    icon,
    description: a.category ? `${a.name} — ${a.category}` : a.name,
  };
}

function buildRules(dto: PublicStorefront): string[] {
  const rules = [
    ...(dto.theme.facilities.perksItems ?? []),
    dto.theme.rooms.policyPets?.trim(),
    dto.theme.rooms.policyCancellation?.trim(),
  ].filter((r): r is string => !!r);
  if (rules.length) return rules.slice(0, 6);
  return [
    'No parties or events',
    'Quiet hours after 10 PM',
    `Check-in ${dto.theme.contact.checkIn || '3:00 PM'}`,
    `Check-out ${dto.theme.contact.checkOut || '11:00 AM'}`,
  ];
}

export function formatNaira(amount: number): string {
  return new Intl.NumberFormat('en-NG', {
    style: 'currency',
    currency: 'NGN',
    maximumFractionDigits: 0,
  }).format(amount);
}

export function shortletPreviewThemeStyle(theme: PublicStorefront['theme']): Record<string, string> {
  const c = theme.colors;
  return {
    '--sl-bg': c.background || '#faf8f5',
    '--sl-surface': '#ffffff',
    '--sl-ink': c.text || '#1a1a1a',
    '--sl-muted': '#6b6560',
    '--sl-accent': c.accent || '#e07a5f',
    '--sl-primary': c.primary || '#3d405b',
    '--sl-radius': '12px',
    '--font-display': "'Fraunces', Georgia, serif",
    '--font-body': "'DM Sans', system-ui, sans-serif",
  };
}
