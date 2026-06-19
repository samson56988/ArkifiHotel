import { createDefaultTheme } from '../models/storefront-theme.models';
import type { HotelShowcase } from '../models/hotel-showcase.models';
import { LEKKI_RESTAURANT_MENU } from './mock-restaurant-menu.data';

const IMG = {
  hero1: 'https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=1600&q=80',
  hero2: 'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=1600&q=80',
  hero3: 'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=1600&q=80',
  roomSuite: 'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=800&q=80',
  roomDeluxe: 'https://images.unsplash.com/photo-1618773928121-c32242e63f39?w=800&q=80',
  roomStudio: 'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800&q=80',
  pool: 'https://images.unsplash.com/photo-1575429198097-0414ec085588?w=800&q=80',
  restaurant: 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=800&q=80',
  gym: 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=800&q=80',
  lobby: 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800&q=80',
  abujaHero: 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=1600&q=80',
  abujaRoom: 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&q=80',
};

function lekkiTheme() {
  const theme = createDefaultTheme('Lekki Suites & Shortlets');
  theme.colors = {
    preset: 'midnight-gold',
    primary: '#1B3A4B',
    accent: '#C9954C',
    background: '#F8F5F0',
    text: '#1B3A4B',
  };
  theme.banner.headline = 'Where Lagos slows down';
  theme.banner.subheadline =
    'Boutique comfort on Admiralty Way — rooms, shortlets, and warm hospitality minutes from the Atlantic.';
  theme.banner.overlayOpacity = 52;
  theme.banner.badgeText = 'Luxury Boutique Hotel';
  theme.contact.location = '14 Admiralty Way, Lekki Phase 1, Lagos';
  theme.contact.checkIn = '2:00 PM';
  theme.contact.checkOut = '12:00 PM';
  theme.about.title = 'A decade of Lagos hospitality';
  theme.about.description =
    'Lekki Suites was founded with one conviction: the best stay feels like you never needed to leave.';
  theme.about.showStats = true;
  theme.about.stats = [
    { num: '2012', label: 'Founded' },
    { num: '28', label: 'Rooms' },
    { num: '4.9★', label: 'Guest rating' },
    { num: '10k+', label: 'Guests hosted' },
  ];
  theme.rooms.title = 'Our rooms';
  theme.rooms.subtitle = 'Each space is individually designed for comfort, work, and rest.';
  theme.facilities.title = 'Every amenity you need';
  theme.facilities.subtitle = 'Relax, recharge, and enjoy our property.';
  theme.footer.tagline = 'Your home away from home in Lekki Phase 1.';
  return theme;
}

function heritageTheme() {
  const theme = createDefaultTheme('Heritage Boutique Hotel');
  theme.colors = {
    preset: 'terracotta-warm',
    primary: '#4A3728',
    accent: '#B8860B',
    background: '#FAF6F1',
    text: '#3D2318',
  };
  theme.banner.headline = 'Abuja’s quiet luxury';
  theme.banner.subheadline =
    'Twenty rooms of calm elegance in the heart of the capital — crafted for business travelers and weekend escapes.';
  theme.banner.overlayOpacity = 48;
  theme.banner.badgeText = 'Boutique Hotel';
  theme.contact.location = '12 Aminu Kano Crescent, Wuse II, Abuja';
  theme.contact.checkIn = '3:00 PM';
  theme.contact.checkOut = '11:00 AM';
  theme.about.title = 'Rooted in Nigerian warmth';
  theme.about.description = 'Heritage Boutique blends contemporary design with the soul of Abuja hospitality.';
  theme.about.showStats = true;
  theme.about.stats = [
    { num: '2018', label: 'Opened' },
    { num: '20', label: 'Rooms' },
    { num: '4.8★', label: 'Guest rating' },
    { num: '5k+', label: 'Guests hosted' },
  ];
  return theme;
}

export const MOCK_HOTELS: Record<string, HotelShowcase> = {
  'lekki-suites': {
    businessId: '00000000-0000-0000-0000-000000010001',
    businessName: 'Lekki Suites & Shortlets',
    slug: 'lekki-suites',
    logoUrl: null,
    theme: lekkiTheme(),
    location: '14 Admiralty Way, Lekki Phase 1, Lagos',
    category: 'Luxury Boutique Hotel',
    stars: 4,
    checkIn: '2:00 PM',
    checkOut: '12:00 PM',
    heroImages: [IMG.hero1, IMG.hero2, IMG.hero3],
    aboutImageUrl: IMG.lobby,
    galleryImages: [IMG.hero1, IMG.pool, IMG.restaurant, IMG.lobby, IMG.roomSuite, IMG.roomDeluxe],
    aboutQuote: 'We don’t run a hotel. We run a home that happens to have more rooms than most.',
    aboutQuoteBy: '— Adaeze Okonkwo, Founder',
    aboutStory: [
      'Lekki Suites was founded in 2012 with one conviction: that the best hotel experience you can give someone is the feeling that they never needed to leave.',
      'We built this property from a single floor of six rooms. Today, we have 28 rooms and apartments across two buildings — each one designed with materials sourced from Nigerian artisans.',
    ],
    aboutStats: [
      { num: '2012', label: 'Founded' },
      { num: '28', label: 'Rooms' },
      { num: '4.9★', label: 'Guest rating' },
      { num: '10k+', label: 'Guests hosted' },
    ],
    social: {
      facebookUrl: 'https://facebook.com',
      facebookHandle: 'Lekki Suites Official',
      facebookFollowers: '8.1K',
      instagramUrl: 'https://instagram.com',
      instagramHandle: '@lekkisuites',
      instagramFollowers: '12.4K',
      tikTokUrl: 'https://tiktok.com',
      tikTokHandle: '@lekkisuites.ng',
      tikTokFollowers: '4.3K',
      xUrl: null,
      xHandle: null,
      xFollowers: null,
      contactEmail: 'hello@lekkisuites.com',
      contactPhone: '+234 803 456 7890',
    },
    socialLinks: [
      {
        platform: 'Instagram',
        handle: '@lekkisuites',
        url: 'https://instagram.com',
        color: '#E1306C',
        emoji: '📸',
        followers: '12.4K',
      },
      {
        platform: 'Facebook',
        handle: 'Lekki Suites Official',
        url: 'https://facebook.com',
        color: '#1877F2',
        emoji: '👤',
        followers: '8.1K',
      },
      {
        platform: 'TikTok',
        handle: '@lekkisuites.ng',
        url: 'https://tiktok.com',
        color: '#010101',
        emoji: '🎵',
        followers: '4.3K',
      },
      {
        platform: 'WhatsApp',
        handle: '+234 803 456 7890',
        url: 'https://wa.me/2348034567890',
        color: '#25D366',
        emoji: '💬',
        followers: null,
      },
    ],
    locations: [
      {
        id: 'loc-lekki-main',
        name: 'Main Tower',
        address: '14 Admiralty Way, Lekki Phase 1, Lagos',
      },
      {
        id: 'loc-lekki-garden',
        name: 'Garden Wing',
        address: '18 Bishop Oluwole St, Lekki Phase 1, Lagos',
      },
    ],
    requiresBranchSelection: false,
    activeLocationId: null,
    branchName: null,
    rooms: [
      {
        id: 'r1',
        name: 'Presidential Suite',
        roomType: 'Suite',
        beds: 'King Bed',
        size: '85 sqm',
        basePricePerNight: 185000,
        maxOccupancy: 2,
        available: true,
        description: 'Our crown jewel — panoramic Lagos views, private terrace, and dedicated in-room butler.',
        amenities: ['Private Balcony', 'Jacuzzi', 'Butler Service', 'Ocean View'],
        primaryImageUrl: IMG.roomSuite,
        imageUrls: [IMG.roomSuite, IMG.hero1, IMG.roomDeluxe],
        locationId: 'loc-lekki-main',
        locationName: 'Main Tower',
        featured: true,
      },
      {
        id: 'r2',
        name: 'Executive Suite',
        roomType: 'Suite',
        beds: 'King Bed',
        size: '60 sqm',
        basePricePerNight: 95000,
        maxOccupancy: 2,
        available: true,
        description: 'Floor-to-ceiling windows and a plush sitting area perfect for business or leisure.',
        amenities: ['City View', 'Mini Bar', 'Bathtub', 'Work Desk'],
        primaryImageUrl: IMG.roomDeluxe,
        imageUrls: [IMG.roomDeluxe, IMG.hero2, IMG.lobby],
        locationId: 'loc-lekki-main',
        locationName: 'Main Tower',
      },
      {
        id: 'r3',
        name: 'Deluxe Double Room',
        roomType: 'Double',
        beds: 'Queen Bed',
        size: '42 sqm',
        basePricePerNight: 58000,
        maxOccupancy: 2,
        available: true,
        description: 'Thoughtfully appointed with garden views and everything you need for a restful stay.',
        amenities: ['Garden View', 'Smart TV', 'Air Conditioning', 'Mini Fridge'],
        primaryImageUrl: IMG.roomDeluxe,
        locationId: 'loc-lekki-garden',
        locationName: 'Garden Wing',
      },
      {
        id: 'r4',
        name: 'Classic Twin Room',
        roomType: 'Twin',
        beds: '2 Single Beds',
        size: '38 sqm',
        basePricePerNight: 48000,
        maxOccupancy: 2,
        available: false,
        description: 'Ideal for colleagues or friends travelling together with a shared workspace.',
        amenities: ['City View', 'Smart TV', 'Work Desk'],
        primaryImageUrl: IMG.roomStudio,
        locationId: 'loc-lekki-garden',
        locationName: 'Garden Wing',
      },
      {
        id: 'r5',
        name: 'Studio Shortlet',
        roomType: 'Shortlet',
        beds: 'Queen Bed',
        size: '50 sqm',
        basePricePerNight: 65000,
        maxOccupancy: 2,
        available: true,
        description: 'Fully equipped for longer stays — kitchen, laundry, and a true sense of home.',
        amenities: ['Full Kitchen', 'Washing Machine', 'Balcony', 'Smart TV'],
        primaryImageUrl: IMG.roomStudio,
        locationId: 'loc-lekki-garden',
        locationName: 'Annex',
      },
      {
        id: 'r6',
        name: '2-Bedroom Apartment',
        roomType: 'Shortlet',
        beds: '1 King + 2 Singles',
        size: '90 sqm',
        basePricePerNight: 130000,
        maxOccupancy: 4,
        available: true,
        description: 'Perfect for families or small groups with full apartment layout and dining area.',
        amenities: ['Full Kitchen', 'Living Room', '2 Bathrooms', 'Dining Area'],
        primaryImageUrl: IMG.roomSuite,
        locationId: 'loc-lekki-garden',
        locationName: 'Annex',
      },
    ],
    facilities: [
      {
        id: 'f1',
        name: 'Rooftop Pool',
        description: 'Open 6am–10pm daily with panoramic Lagos skyline views and poolside loungers.',
        emoji: '🏊',
        category: 'Wellness',
        hours: '6:00 AM – 10:00 PM',
        featured: true,
        primaryImageUrl: IMG.pool,
        locationName: null,
      },
      {
        id: 'f2',
        name: 'The Lekki Table',
        description: 'In-house restaurant serving Nigerian and continental cuisine with weekend brunch.',
        emoji: '🍽️',
        category: 'Dining',
        hours: '7:00 AM – 11:00 PM',
        primaryImageUrl: IMG.restaurant,
        locationName: null,
      },
      {
        id: 'f3',
        name: 'Fitness Centre',
        description: 'Fully equipped gym with cardio, free weights, and personal training on request.',
        emoji: '🏋️',
        category: 'Wellness',
        hours: '24 hours',
        primaryImageUrl: IMG.gym,
        locationName: null,
      },
      {
        id: 'f5',
        name: 'Valet Parking',
        description: 'Secure underground parking with valet service for all guests.',
        emoji: '🚗',
        category: 'Services',
        hours: '24 hours',
        primaryImageUrl: null,
        locationName: null,
      },
      {
        id: 'f6',
        name: 'Spa & Wellness',
        description: 'Full-service spa with massages, facials, steam room, and couples packages.',
        emoji: '🧖',
        category: 'Wellness',
        hours: '9:00 AM – 9:00 PM',
        primaryImageUrl: IMG.lobby,
        locationName: null,
      },
      {
        id: 'f7',
        name: 'Conference Rooms',
        description: 'Three meeting rooms with AV setup, catering, and capacity for up to 120 guests.',
        emoji: '🎰',
        category: 'Business',
        hours: 'By appointment',
        primaryImageUrl: IMG.lobby,
        locationName: null,
      },
      {
        id: 'f8',
        name: '24hr Concierge',
        description: 'Round-the-clock front desk, airport transfers, and local recommendations.',
        emoji: '🛎️',
        category: 'Services',
        hours: '24 hours',
        primaryImageUrl: null,
        locationName: null,
      },
      {
        id: 'f9',
        name: 'Sky Lounge Bar',
        description: 'Cocktails and small plates on the 12th floor with sunset views over the lagoon.',
        emoji: '🍸',
        category: 'Dining',
        hours: '5:00 PM – 1:00 AM',
        primaryImageUrl: IMG.hero2,
        locationName: null,
      },
    ],
    restaurant: LEKKI_RESTAURANT_MENU,
  },

  'heritage-abuja': {
    businessId: '00000000-0000-0000-0000-000000010002',
    businessName: 'Heritage Boutique Hotel',
    slug: 'heritage-abuja',
    logoUrl: null,
    theme: heritageTheme(),
    location: '12 Aguiyi Ironsi Street, Maitama, Abuja',
    category: 'Boutique Hotel',
    stars: 5,
    checkIn: '3:00 PM',
    checkOut: '11:00 AM',
    heroImages: [IMG.abujaHero, IMG.hero2, IMG.lobby],
    aboutImageUrl: IMG.restaurant,
    galleryImages: [IMG.abujaHero, IMG.abujaRoom, IMG.restaurant, IMG.pool],
    aboutQuote: 'Every guest should leave feeling the capital was kind to them.',
    aboutQuoteBy: '— Emmanuel Adeyemi, General Manager',
    aboutStory: [
      'Heritage opened in 2018 to offer Abuja a hotel that feels personal, not corporate.',
      'Our team of twenty manages every detail — from pillow preference to airport pickup.',
    ],
    aboutStats: [
      { num: '2018', label: 'Opened' },
      { num: '20', label: 'Rooms' },
      { num: '5★', label: 'Rating' },
      { num: '3', label: 'Award wins' },
    ],
    social: {
      facebookUrl: 'https://facebook.com',
      facebookHandle: 'Heritage Abuja',
      facebookFollowers: '4.5K',
      instagramUrl: 'https://instagram.com',
      instagramHandle: '@heritageabuja',
      instagramFollowers: '6.2K',
      tikTokUrl: null,
      tikTokHandle: null,
      tikTokFollowers: null,
      xUrl: 'https://x.com',
      xHandle: '@heritageabuja',
      xFollowers: '2.1K',
      contactEmail: 'stay@heritageabuja.com',
      contactPhone: '+234 809 112 3344',
    },
    socialLinks: [
      {
        platform: 'Instagram',
        handle: '@heritageabuja',
        url: 'https://instagram.com',
        color: '#E1306C',
        emoji: '📸',
        followers: '6.2K',
      },
      {
        platform: 'X',
        handle: '@heritageabuja',
        url: 'https://x.com',
        color: '#14171A',
        emoji: '𝕏',
        followers: '2.1K',
      },
    ],
    locations: [
      {
        id: 'loc-abuja-main',
        name: 'Main Building',
        address: '12 Aguiyi Ironsi Street, Maitama, Abuja',
      },
    ],
    requiresBranchSelection: false,
    activeLocationId: null,
    branchName: null,
    rooms: [
      {
        id: 'h1',
        name: 'Maitama King Room',
        roomType: 'Deluxe',
        beds: 'King Bed',
        size: '45 sqm',
        basePricePerNight: 72000,
        maxOccupancy: 2,
        available: true,
        description: 'Soft terracotta tones, city views, and a rain shower built for unwinding.',
        amenities: ['Rain Shower', 'Smart TV', 'Work Desk', 'Mini Bar'],
        primaryImageUrl: IMG.abujaRoom,
        locationName: 'Main Building',
      },
      {
        id: 'h2',
        name: 'Garden Terrace',
        roomType: 'Premium',
        beds: 'Queen Bed',
        size: '52 sqm',
        basePricePerNight: 88000,
        maxOccupancy: 2,
        available: true,
        description: 'Private terrace overlooking our courtyard garden.',
        amenities: ['Terrace', 'Bathtub', 'Nespresso', 'Robes'],
        primaryImageUrl: IMG.roomDeluxe,
        locationName: 'Garden Level',
      },
      {
        id: 'h3',
        name: 'Executive Work Suite',
        roomType: 'Suite',
        beds: 'King Bed',
        size: '68 sqm',
        basePricePerNight: 125000,
        maxOccupancy: 2,
        available: true,
        description: 'Separate living area and dedicated workspace for extended business stays.',
        amenities: ['Living Room', 'Printer', 'Fast WiFi', 'Lounge Access'],
        primaryImageUrl: IMG.roomSuite,
        locationName: 'Main Building',
        featured: true,
      },
    ],
    facilities: [
      {
        id: 'hf1',
        name: 'Courtyard Restaurant',
        description: 'Farm-to-table dining with a rotating Nigerian tasting menu and al fresco seating.',
        emoji: '🍽️',
        category: 'Dining',
        hours: '7:00 AM – 10:00 PM',
        featured: true,
        primaryImageUrl: IMG.restaurant,
        locationName: null,
      },
      {
        id: 'hf2',
        name: 'Infinity Pool',
        description: 'Heated pool with sunset views over Maitama and complimentary towels.',
        emoji: '🏊',
        category: 'Wellness',
        hours: '6:00 AM – 9:00 PM',
        primaryImageUrl: IMG.pool,
        locationName: null,
      },
      {
        id: 'hf3',
        name: 'Business Lounge',
        description: 'Quiet co-working space with meeting pods, printing, and complimentary coffee.',
        emoji: '💼',
        category: 'Business',
        hours: '24 hours',
        primaryImageUrl: IMG.lobby,
        locationName: null,
      },
      {
        id: 'hf4',
        name: 'Heritage Spa',
        description: 'Treatments inspired by West African botanicals — massages, body wraps, and facials.',
        emoji: '🧖',
        category: 'Wellness',
        hours: '10:00 AM – 8:00 PM',
        primaryImageUrl: IMG.abujaRoom,
        locationName: null,
      },
      {
        id: 'hf5',
        name: 'Garden Terrace Bar',
        description: 'Craft cocktails and light bites in our courtyard garden every evening.',
        emoji: '🍸',
        category: 'Dining',
        hours: '4:00 PM – 12:00 AM',
        primaryImageUrl: IMG.hero2,
        locationName: null,
      },
      {
        id: 'hf7',
        name: 'Airport Shuttle',
        description: 'Scheduled pickups from Nnamdi Azikiwe International Airport on request.',
        emoji: '🚐',
        category: 'Services',
        hours: 'By appointment',
        primaryImageUrl: null,
        locationName: null,
      },
      {
        id: 'hf8',
        name: 'Private Dining',
        description: 'Intimate dinners for two to twelve in our wine cellar or garden pavilion.',
        emoji: '🥂',
        category: 'Dining',
        hours: 'By reservation',
        primaryImageUrl: IMG.restaurant,
        locationName: null,
      },
    ],
    restaurant: null,
  },
};

export const DEMO_HOTEL_SLUGS = Object.keys(MOCK_HOTELS);

export function getMockHotel(slug: string, locationRouteId?: string | null): HotelShowcase | null {
  const base = MOCK_HOTELS[slug];
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
      rooms: [],
      facilities: [],
      heroImages: [],
      galleryImages: [],
      restaurant: null,
    };
  }

  const effectiveId =
    locationRouteId && locationRouteId !== 'default'
      ? locationRouteId
      : locations.length === 1
        ? locations[0].id
        : null;

  const branchName = effectiveId
    ? (locations.find((l) => l.id === effectiveId)?.name ?? null)
    : null;

  const filterByBranch = !!effectiveId && locations.length > 0;

  const rooms = filterByBranch
    ? base.rooms.filter((r) => r.locationId === effectiveId)
    : base.rooms;
  const facilities = filterByBranch
    ? base.facilities.filter((f) => !f.locationId || f.locationId === effectiveId)
    : base.facilities;

  const heroImages = filterByBranch
    ? rooms.map((r) => r.primaryImageUrl).filter((u): u is string => !!u)
    : base.heroImages;

  return {
    ...base,
    requiresBranchSelection: false,
    activeLocationId: effectiveId,
    branchName,
    rooms,
    facilities,
    heroImages: heroImages.length > 0 ? heroImages : base.heroImages,
    galleryImages: base.galleryImages,
  };
}
