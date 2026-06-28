import { createShortletDefaultTheme } from '../models/storefront-theme.models';
import type { ShortletShowcase } from '../models/shortlet-showcase.models';

const IMG = {
  hero: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=1600&q=80',
  living1: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=900&q=80',
  living2: 'https://images.unsplash.com/photo-1493809842364-78817add7ffb?w=900&q=80',
  bedroom: 'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=900&q=80',
  kitchen: 'https://images.unsplash.com/photo-1556912173-46c336c7fd55?w=900&q=80',
  studio: 'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=900&q=80',
  loft: 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=900&q=80',
  penthouse: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=900&q=80',
  balcony: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=900&q=80',
  host: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400&q=80',
  host2: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400&q=80',
  abujaHero: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=1600&q=80',
  abujaLiving: 'https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?w=900&q=80',
  abujaBed: 'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=900&q=80',
};

function nomadTheme() {
  const theme = createShortletDefaultTheme('Nomad Stays');
  theme.contact.location = 'Victoria Island, Lagos';
  theme.banner.subheadline = 'Curated apartments for week-long stays in Victoria Island';
  theme.footer.tagline = 'Curated apartments for modern travelers.';
  return theme;
}

function skylineTheme() {
  const theme = createShortletDefaultTheme('Skyline Apartments');
  theme.colors = {
    preset: 'sage-luxe',
    primary: '#2d4a3e',
    accent: '#c9a227',
    background: '#f7f5f0',
    text: '#1c2b24',
  };
  theme.contact.location = 'Maitama, Abuja';
  theme.contact.checkIn = '2:00 PM';
  theme.contact.checkOut = '12:00 PM';
  theme.footer.tagline = 'Elevated short stays in the capital.';
  return theme;
}

const NOMAD_STAYS: ShortletShowcase = {
  businessId: '00000000-0000-0000-0000-000000020001',
  businessName: 'Nomad Stays',
  slug: 'nomad-stays',
  logoUrl: null,
  tagline: 'Designer apartments for week-long Lagos living',
  neighborhood: 'Victoria Island · Lagos',
  heroImage: IMG.hero,
  galleryImages: [IMG.living1, IMG.living2, IMG.kitchen, IMG.balcony, IMG.bedroom],
  checkIn: '3:00 PM',
  checkOut: '11:00 AM',
  minNights: 2,
  host: {
    name: 'Amara Okafor',
    photoUrl: IMG.host,
    role: 'Superhost',
    bio: 'I’ve hosted over 400 stays across Victoria Island. Every apartment is personally styled — think hotel polish with the warmth of a real home. I live 10 minutes away and love recommending the best local spots.',
    responseTime: 'Under 1 hour',
    languages: ['English', 'Igbo', 'French'],
    verified: true,
    rating: 4.97,
    reviewCount: 186,
    yearsHosting: 6,
  },
  listings: [
    {
      id: 'ns-loft-1',
      title: 'The Oniru Loft',
      tagline: 'Floor-to-ceiling windows & city skyline',
      beds: 2,
      baths: 2,
      guests: 4,
      nightlyPrice: 95000,
      weeklyPrice: 595000,
      images: [IMG.loft, IMG.living1, IMG.kitchen, IMG.bedroom],
      highlightAmenities: ['Fast Wi‑Fi', 'Dedicated workspace', 'Smart TV', 'Full kitchen'],
      description:
        'A bright corner loft with marble kitchen, rainfall shower, and a balcony perfect for morning coffee. Ideal for couples or two remote workers.',
      featured: true,
      locationId: 'loc-nomad-vi',
    },
    {
      id: 'ns-studio-1',
      title: 'Admiralty Studio',
      tagline: 'Compact luxury for solo travelers',
      beds: 1,
      baths: 1,
      guests: 2,
      nightlyPrice: 55000,
      weeklyPrice: 340000,
      images: [IMG.studio, IMG.living2, IMG.kitchen],
      highlightAmenities: ['Self check-in', 'Washer', 'Air conditioning', 'Netflix'],
      description: 'Thoughtfully designed studio with queen bed, kitchenette, and walk-in wardrobe. Steps from restaurants and co-working cafes.',
      featured: true,
      locationId: 'loc-nomad-vi',
    },
    {
      id: 'ns-pent-1',
      title: 'Oceanview Penthouse',
      tagline: 'Private terrace & panoramic Atlantic views',
      beds: 3,
      baths: 3,
      guests: 6,
      nightlyPrice: 185000,
      weeklyPrice: 1150000,
      images: [IMG.penthouse, IMG.balcony, IMG.living1, IMG.bedroom],
      highlightAmenities: ['Private terrace', 'Jacuzzi', 'Chef kitchen', '24/7 security'],
      description: 'Our flagship unit — wraparound terrace, three ensuite bedrooms, and sunset views over the lagoon.',
      featured: true,
      locationId: 'loc-nomad-vi',
    },
    {
      id: 'ns-1bed-1',
      title: 'Coral One-Bed',
      tagline: 'Quiet street, walkable to VI nightlife',
      beds: 1,
      baths: 1,
      guests: 2,
      nightlyPrice: 72000,
      weeklyPrice: 450000,
      images: [IMG.bedroom, IMG.living2, IMG.kitchen],
      highlightAmenities: ['Parking', 'Generator backup', 'Housekeeping', 'Coffee machine'],
      description: 'Cozy one-bedroom with separate living area. Popular with business travelers who want a residential feel.',
      locationId: 'loc-nomad-vi',
    },
    {
      id: 'ns-lekki-2bed',
      title: 'Lekki Garden Two-Bed',
      tagline: 'Quiet estate living near the mall',
      beds: 2,
      baths: 2,
      guests: 4,
      nightlyPrice: 68000,
      weeklyPrice: 420000,
      images: [IMG.living2, IMG.bedroom, IMG.kitchen],
      highlightAmenities: ['Parking', 'Pool access', 'Wi‑Fi', 'Backup power'],
      description: 'Spacious two-bedroom in a gated Lekki estate — pool, gym, and 24-hour security included.',
      featured: true,
      locationId: 'loc-nomad-lekki',
    },
    {
      id: 'ns-lekki-studio',
      title: 'Admiralty Walk Studio',
      tagline: 'Affordable week-long Lekki stays',
      beds: 1,
      baths: 1,
      guests: 2,
      nightlyPrice: 48000,
      weeklyPrice: 295000,
      images: [IMG.studio, IMG.kitchen],
      highlightAmenities: ['Self check-in', 'Kitchenette', 'AC', 'Netflix'],
      description: 'Bright studio on Admiralty Way — walkable to cafes, supermarkets, and the cinema.',
      featured: true,
      locationId: 'loc-nomad-lekki',
    },
  ],
  amenities: [
    { id: 'a1', label: 'High-speed Wi‑Fi', icon: '📶', description: 'Fiber internet — 100 Mbps+ in every unit.', category: 'Essentials' },
    { id: 'a2', label: 'Self check-in', icon: '🔑', description: 'Smart lock access from 3 PM on arrival day.', category: 'Essentials' },
    { id: 'a3', label: 'Full kitchen', icon: '🍳', description: 'Cookware, utensils, and basic pantry staples.', category: 'Essentials' },
    { id: 'a4', label: 'Air conditioning', icon: '❄️', description: 'Split units in every room.', category: 'Comfort' },
    { id: 'a5', label: 'Washer & dryer', icon: '🧺', description: 'In-unit or shared laundry depending on apartment.', category: 'Comfort' },
    { id: 'a6', label: 'Dedicated workspace', icon: '💻', description: 'Desk, ergonomic chair, and good lighting.', category: 'Work' },
    { id: 'a7', label: 'Smart TV', icon: '📺', description: 'Streaming apps pre-installed.', category: 'Comfort' },
    { id: 'a8', label: '24/7 security', icon: '🛡️', description: 'Gated estate with CCTV and on-site guards.', category: 'Safety' },
    { id: 'a9', label: 'Generator backup', icon: '⚡', description: 'Uninterrupted power during outages.', category: 'Safety' },
    { id: 'a10', label: 'Housekeeping', icon: '✨', description: 'Mid-stay cleaning available on request.', category: 'Comfort' },
    { id: 'a11', label: 'Parking', icon: '🅿️', description: 'One dedicated spot per apartment.', category: 'Essentials' },
    { id: 'a12', label: 'Coffee & tea', icon: '☕', description: 'Starter kit for your first morning.', category: 'Essentials' },
  ],
  houseRules: [
    'No parties or events',
    'Quiet hours after 10 PM',
    'No smoking indoors',
    'Pets allowed in select units — ask before booking',
    'Valid ID required at check-in',
    'Minimum stay: 2 nights',
  ],
  theme: nomadTheme(),
  locations: [
    { id: 'loc-nomad-vi', name: 'Victoria Island', address: 'Oniru & Admiralty Way, Victoria Island, Lagos' },
    { id: 'loc-nomad-lekki', name: 'Lekki Phase 1', address: '14 Admiralty Way, Lekki Phase 1, Lagos' },
  ],
  requiresBranchSelection: false,
  activeLocationId: null,
  branchName: null,
};

const SKYLINE_APARTMENTS: ShortletShowcase = {
  businessId: '00000000-0000-0000-0000-000000020002',
  businessName: 'Skyline Apartments',
  slug: 'skyline-apartments',
  logoUrl: null,
  tagline: 'Residential calm in the heart of Abuja',
  neighborhood: 'Maitama · Abuja',
  heroImage: IMG.abujaHero,
  galleryImages: [IMG.abujaLiving, IMG.abujaBed, IMG.kitchen, IMG.living1],
  checkIn: '2:00 PM',
  checkOut: '12:00 PM',
  minNights: 1,
  host: {
    name: 'Ibrahim Musa',
    photoUrl: IMG.host2,
    role: 'Experienced host',
    bio: 'Former diplomat turned hospitality entrepreneur. Skyline is my answer to Abuja’s need for furnished apartments that feel like home — not a hotel lobby.',
    responseTime: 'Within 2 hours',
    languages: ['English', 'Hausa', 'Arabic'],
    verified: true,
    rating: 4.92,
    reviewCount: 94,
    yearsHosting: 4,
  },
  listings: [
    {
      id: 'sk-2bed-1',
      title: 'Maitama Two-Bed',
      tagline: 'Garden view & open-plan living',
      beds: 2,
      baths: 2,
      guests: 4,
      nightlyPrice: 78000,
      weeklyPrice: 485000,
      images: [IMG.abujaLiving, IMG.abujaBed, IMG.kitchen],
      highlightAmenities: ['Garden access', 'Backup power', 'Workspace', 'Netflix'],
      description: 'Spacious two-bedroom with private garden access. Popular with families and extended business stays.',
      featured: true,
    },
    {
      id: 'sk-1bed-1',
      title: 'Diplomatic Suite',
      tagline: 'Polished one-bed for executives',
      beds: 1,
      baths: 1,
      guests: 2,
      nightlyPrice: 62000,
      weeklyPrice: 385000,
      images: [IMG.abujaBed, IMG.abujaLiving],
      highlightAmenities: ['Concierge', 'Daily housekeeping', 'Gym access', 'Parking'],
      description: 'Executive one-bedroom in a secure compound. Walking distance to embassies and restaurants.',
      featured: true,
    },
    {
      id: 'sk-studio-1',
      title: 'Capital Studio',
      tagline: 'Affordable week-long stays',
      beds: 1,
      baths: 1,
      guests: 2,
      nightlyPrice: 42000,
      weeklyPrice: 260000,
      images: [IMG.studio, IMG.kitchen],
      highlightAmenities: ['Self check-in', 'Wi‑Fi', 'Kitchenette', 'AC'],
      description: 'Efficient studio layout with everything you need for a short Abuja assignment.',
    },
  ],
  amenities: [
    { id: 's1', label: 'Fiber Wi‑Fi', icon: '📶', description: 'Reliable internet for video calls.', category: 'Essentials' },
    { id: 's2', label: 'Backup power', icon: '⚡', description: 'Inverter + generator on compound.', category: 'Safety' },
    { id: 's3', label: 'Secure compound', icon: '🛡️', description: '24-hour gate and patrol.', category: 'Safety' },
    { id: 's4', label: 'Gym access', icon: '🏋️', description: 'Shared fitness room on premises.', category: 'Comfort' },
    { id: 's5', label: 'Parking', icon: '🅿️', description: 'Covered parking for one vehicle.', category: 'Essentials' },
    { id: 's6', label: 'Workspace', icon: '💻', description: 'Desk setup in every unit.', category: 'Work' },
    { id: 's7', label: 'Kitchen essentials', icon: '🍳', description: 'Full cookware and dining set.', category: 'Essentials' },
    { id: 's8', label: 'Housekeeping', icon: '✨', description: 'Weekly cleaning included.', category: 'Comfort' },
  ],
  houseRules: [
    'No loud music after 9 PM',
    'No smoking',
    'Visitors must sign in at gate',
    'Check-out by 12 PM',
  ],
  theme: skylineTheme(),
  locations: [
    { id: 'loc-skyline-maitama', name: 'Maitama', address: 'Aguiyi Ironsi Street, Maitama, Abuja' },
  ],
  requiresBranchSelection: false,
  activeLocationId: null,
  branchName: null,
};

export const MOCK_SHORTLETS: Record<string, ShortletShowcase> = {
  'nomad-stays': NOMAD_STAYS,
  'skyline-apartments': SKYLINE_APARTMENTS,
};

export function getMockShortlet(slug: string, locationRouteId?: string | null): ShortletShowcase | null {
  const base = MOCK_SHORTLETS[slug];
  if (!base) {
    return null;
  }

  const locations = base.locations ?? [];

  if (!locationRouteId && locations.length > 1) {
    return {
      ...base,
      requiresBranchSelection: true,
      activeLocationId: null,
      branchName: null,
      listings: [],
      galleryImages: [],
      heroImage: base.heroImage,
    };
  }

  const effectiveId =
    locationRouteId && locationRouteId !== 'default'
      ? locationRouteId
      : locations.length === 1
        ? locations[0].id
        : null;

  const branch = effectiveId ? locations.find((l) => l.id === effectiveId) : null;
  const filterByBranch = !!effectiveId && locations.length > 1;

  const listings = filterByBranch
    ? base.listings.filter((l) => !l.locationId || l.locationId === effectiveId)
    : base.listings;

  return {
    ...base,
    listings,
    activeLocationId: effectiveId,
    branchName: branch?.name ?? null,
    requiresBranchSelection: !effectiveId && locations.length > 1,
    neighborhood: branch?.name ? `${branch.name} · Lagos` : base.neighborhood,
  };
}
