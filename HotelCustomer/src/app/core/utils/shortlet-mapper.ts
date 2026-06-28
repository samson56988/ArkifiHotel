import type { PublicStorefront, PublicStorefrontAmenity } from '../models/storefront-theme.models';
import type { ShortletAmenity, ShortletHost, ShortletListing, ShortletShowcase } from '../models/shortlet-showcase.models';
import type { ShowcaseLocation } from '../models/hotel-showcase.models';

const AMENITY_ICONS: Record<string, string> = {
  wifi: '📶',
  internet: '📶',
  kitchen: '🍳',
  parking: '🅿️',
  pool: '🏊',
  gym: '🏋️',
  security: '🛡️',
  power: '⚡',
  generator: '⚡',
  washer: '🧺',
  laundry: '🧺',
  tv: '📺',
  workspace: '💻',
  desk: '💻',
  coffee: '☕',
  air: '❄️',
  ac: '❄️',
};

const DEFAULT_HOST_PHOTO =
  'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400&q=80';

export function isShortletBusinessType(businessType: string | undefined | null): boolean {
  return (businessType ?? '').toLowerCase() === 'shortlet';
}

export function mapPublicToShortletShowcase(dto: PublicStorefront): ShortletShowcase {
  const contact = dto.theme.contact ?? { location: '', checkIn: '', checkOut: '', introText: '' };
  const locations = mapLocations(dto);
  const requiresBranchSelection = dto.requiresBranchSelection ?? false;
  const effectiveId = dto.activeLocationId ?? null;
  const branch = effectiveId ? locations.find((l) => l.id === effectiveId) : null;

  if (requiresBranchSelection) {
    return {
      businessId: dto.businessId,
      businessName: dto.businessName,
      slug: dto.slug,
      logoUrl: dto.logoUrl,
      tagline: dto.theme.banner.subheadline?.trim() || dto.theme.banner.headline,
      neighborhood: contact.location || dto.businessName,
      heroImage: DEFAULT_HOST_PHOTO,
      galleryImages: [],
      checkIn: contact.checkIn || '3:00 PM',
      checkOut: contact.checkOut || '11:00 AM',
      minNights: 1,
      host: buildHost(dto),
      listings: [],
      amenities: [],
      houseRules: [],
      theme: dto.theme,
      locations,
      requiresBranchSelection: true,
      activeLocationId: null,
      branchName: null,
    };
  }

  const heroImages = dto.heroImages ?? [];
  const roomRows =
    effectiveId && locations.length > 1
      ? dto.rooms.filter((r) => !r.locationId || r.locationId === effectiveId)
      : dto.rooms;
  const listings = roomRows.map((room, index) => mapListing(room, index));

  return {
    businessId: dto.businessId,
    businessName: dto.businessName,
    slug: dto.slug,
    logoUrl: dto.logoUrl,
    tagline: dto.theme.banner.subheadline?.trim() || dto.theme.banner.headline,
    neighborhood: branch?.name || contact.location || dto.businessName,
    heroImage: heroImages[0] ?? listings[0]?.images[0] ?? DEFAULT_HOST_PHOTO,
    galleryImages: buildGallery(heroImages, listings),
    checkIn: contact.checkIn || '3:00 PM',
    checkOut: contact.checkOut || '11:00 AM',
    minNights: 1,
    host: buildHost(dto),
    listings,
    amenities: mapAmenities(dto.amenities ?? []),
    houseRules: buildHouseRules(dto),
    theme: dto.theme,
    locations,
    requiresBranchSelection: false,
    activeLocationId: effectiveId,
    branchName: branch?.name ?? null,
  };
}

function mapListing(
  room: PublicStorefront['rooms'][number],
  index: number,
): ShortletListing {
  const images = room.imageUrls?.length
    ? room.imageUrls
    : room.primaryImageUrl
      ? [room.primaryImageUrl]
      : [];
  const desc = room.description?.trim() ?? '';
  const beds = room.bedroomCount ?? Math.max(1, Math.ceil(room.maxOccupancy / 2));
  const baths = room.bathroomCount ?? (beds >= 2 ? 2 : 1);
  const tagline =
    room.tagline?.trim() ||
    desc.split(/[.!?]/)[0]?.trim() ||
    `${room.maxOccupancy} guests · furnished apartment`;

  return {
    id: room.id,
    title: room.name,
    tagline,
    beds,
    baths,
    guests: room.maxOccupancy,
    nightlyPrice: room.basePricePerNight,
    weeklyPrice: room.basePricePerWeek ?? null,
    images,
    highlightAmenities: room.amenityNames ?? [],
    description: desc || `${room.name} — fully furnished with self check-in.`,
    featured: room.isGuestFavorite === true,
    locationId: room.locationId ?? undefined,
  };
}

function mapAmenities(items: PublicStorefrontAmenity[]): ShortletAmenity[] {
  return items.map((a) => ({
    id: a.id,
    label: a.name,
    icon: amenityIcon(a.name),
    description: a.category ? `${a.name} — ${a.category}` : a.name,
    category: mapAmenityCategory(a.category),
  }));
}

function mapAmenityCategory(raw: string | null | undefined): ShortletAmenity['category'] {
  const c = (raw ?? '').toLowerCase();
  if (c.includes('safe') || c.includes('security')) return 'Safety';
  if (c.includes('work') || c.includes('business')) return 'Work';
  if (c.includes('comfort') || c.includes('leisure')) return 'Comfort';
  return 'Essentials';
}

function amenityIcon(name: string): string {
  const lower = name.toLowerCase();
  for (const [key, icon] of Object.entries(AMENITY_ICONS)) {
    if (lower.includes(key)) {
      return icon;
    }
  }
  return '✓';
}

function buildHost(dto: PublicStorefront): ShortletHost {
  const about = dto.theme.about;
  const quoteBy = about.quoteBy?.replace(/^[—–-]\s*/, '').trim();
  const ownerName =
    quoteBy ||
    [dto.social.contactEmail?.split('@')[0], dto.businessName].find((v) => v && v.length > 2) ||
    'Your host';

  return {
    name: ownerName,
    photoUrl: dto.aboutImageUrl ?? dto.heroImages?.[0] ?? DEFAULT_HOST_PHOTO,
    role: 'Host',
    bio:
      about.description?.trim() ||
      `Welcome to ${dto.businessName}. We host furnished apartments designed for comfortable week-long stays.`,
    responseTime: 'Within a few hours',
    languages: ['English'],
    verified: true,
    rating: 4.9,
    reviewCount: Math.max(12, dto.rooms.length * 8),
    yearsHosting: 3,
  };
}

function buildHouseRules(dto: PublicStorefront): string[] {
  const perks = dto.theme.facilities.perksItems ?? [];
  const policies = [
    dto.theme.rooms.policyPets?.trim(),
    dto.theme.rooms.policyCancellation?.trim(),
  ].filter((p): p is string => !!p);

  const rules = [...perks, ...policies];
  if (rules.length === 0) {
    return [
      'No parties or events',
      'Quiet hours after 10 PM',
      'No smoking indoors',
      `Check-in ${dto.theme.contact.checkIn || '3:00 PM'}`,
      `Check-out ${dto.theme.contact.checkOut || '11:00 AM'}`,
    ];
  }
  return rules.slice(0, 8);
}

function buildGallery(heroImages: string[], listings: ShortletListing[]): string[] {
  const fromListings = listings.flatMap((l) => l.images);
  return [...heroImages, ...fromListings].filter((v, i, a) => v && a.indexOf(v) === i).slice(0, 8);
}

function mapLocations(dto: PublicStorefront): ShowcaseLocation[] {
  return (dto.locations ?? []).map((l) => ({
    id: l.id,
    name: l.name,
    address: l.address,
  }));
}
