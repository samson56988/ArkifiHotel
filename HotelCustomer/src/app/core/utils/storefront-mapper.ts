import type { PublicStorefront, PublicStorefrontFacility, PublicStorefrontRoom } from '../models/storefront-theme.models';
import type { HotelShowcase, ShowcaseFacility, ShowcaseRoom, ShowcaseLocation, ShowcaseSocialLink } from '../models/hotel-showcase.models';
import { facilityEmoji } from './hotel-theme';

export function mapPublicToShowcase(dto: PublicStorefront): HotelShowcase {
  const heroImages = dto.heroImages ?? [];
  const roomImages = dto.rooms
    .map((r) => r.primaryImageUrl)
    .filter((u): u is string => !!u);
  const contact = dto.theme.contact ?? {
    location: '',
    checkIn: '',
    checkOut: '',
    introText:
      'Questions about your stay? Send us a message and our team will respond within a few hours.',
  };

  return {
    businessId: dto.businessId,
    businessName: dto.businessName,
    slug: dto.slug,
    logoUrl: dto.logoUrl,
    theme: {
      ...dto.theme,
      banner: {
        ...dto.theme.banner,
        badgeText: dto.theme.banner.badgeText || 'Your stay awaits',
      },
      contact,
      about: {
        ...dto.theme.about,
        eyebrow: dto.theme.about.eyebrow || 'About us',
        quote: dto.theme.about.quote ?? '',
        quoteBy: dto.theme.about.quoteBy ?? '',
        showStats: dto.theme.about.showStats ?? false,
        stats: dto.theme.about.stats ?? [],
      },
      rooms: {
        ...dto.theme.rooms,
        eyebrow: dto.theme.rooms.eyebrow || 'Accommodations',
        showFeaturedSection: dto.theme.rooms.showFeaturedSection ?? true,
        featuredEyebrow: dto.theme.rooms.featuredEyebrow || 'Signature Stay',
        featuredTitle: dto.theme.rooms.featuredTitle || 'Our most sought-after room',
        showPageStats: dto.theme.rooms.showPageStats ?? true,
        showPolicies: dto.theme.rooms.showPolicies ?? true,
        policyBreakfast: dto.theme.rooms.policyBreakfast ?? '',
        policyPets: dto.theme.rooms.policyPets ?? '',
        policyCancellation: dto.theme.rooms.policyCancellation ?? '',
        ctaTitle: dto.theme.rooms.ctaTitle ?? '',
        ctaSubtitle: dto.theme.rooms.ctaSubtitle ?? '',
        ctaButtonText: dto.theme.rooms.ctaButtonText || 'Check availability',
      },
      facilities: {
        ...dto.theme.facilities,
        eyebrow: dto.theme.facilities.eyebrow || 'On Property',
        showPageStats: dto.theme.facilities.showPageStats ?? true,
        supportStatValue: dto.theme.facilities.supportStatValue || '24/7',
        supportStatLabel: dto.theme.facilities.supportStatLabel || 'Guest support',
        showGuestPerks: dto.theme.facilities.showGuestPerks ?? true,
        perksEyebrow: dto.theme.facilities.perksEyebrow || 'Guest Perks',
        perksTitle: dto.theme.facilities.perksTitle || 'Everything included in your stay',
        perksSubtitle:
          dto.theme.facilities.perksSubtitle ||
          'Complimentary access to most on-property amenities for all registered guests.',
        perksItems: dto.theme.facilities.perksItems ?? [],
      },
    },
    location: contact.location,
    category: dto.theme.banner.badgeText || 'Hotel',
    stars: 0,
    checkIn: contact.checkIn,
    checkOut: contact.checkOut,
    heroImages,
    galleryImages: [...heroImages, ...roomImages].filter((v, i, a) => a.indexOf(v) === i),
    aboutImageUrl: dto.aboutImageUrl,
    aboutQuote: dto.theme.about.quote ?? '',
    aboutQuoteBy: dto.theme.about.quoteBy ?? '',
    aboutStory: splitAboutParagraphs(dto.theme.about.description),
    aboutStats: dto.theme.about.stats ?? [],
    social: dto.social,
    socialLinks: buildSocialLinks(dto),
    locations: mapLocations(dto),
    requiresBranchSelection: dto.requiresBranchSelection ?? false,
    activeLocationId: dto.activeLocationId ?? null,
    branchName: activeBranchName(dto),
    rooms: dto.rooms.map(mapRoom),
    facilities: dto.facilities.map(mapFacility),
  };
}

function mapRoom(room: PublicStorefrontRoom): ShowcaseRoom {
  const imageUrls = room.imageUrls?.length
    ? room.imageUrls
    : room.primaryImageUrl
      ? [room.primaryImageUrl]
      : [];
  return {
    id: room.id,
    name: room.name,
    roomType: 'Room',
    beds: `${room.maxOccupancy} guest${room.maxOccupancy === 1 ? '' : 's'}`,
    size: '',
    basePricePerNight: room.basePricePerNight,
    maxOccupancy: room.maxOccupancy,
    available: true,
    description: '',
    amenities: [],
    primaryImageUrl: room.primaryImageUrl,
    imageUrls,
    locationId: room.locationId,
    locationName: room.locationName,
  };
}

function mapFacility(facility: PublicStorefrontFacility): ShowcaseFacility {
  const imageUrls = facility.imageUrls?.length
    ? facility.imageUrls
    : facility.primaryImageUrl
      ? [facility.primaryImageUrl]
      : [];
  return {
    id: facility.id,
    name: facility.name,
    description: '',
    emoji: facilityEmoji(facility.name),
    category: 'Services',
    hours: null,
    primaryImageUrl: facility.primaryImageUrl,
    imageUrls,
    locationId: facility.locationId,
    locationName: facility.locationName,
  };
}

function mapLocations(dto: PublicStorefront): ShowcaseLocation[] {
  return (dto.locations ?? []).map((l) => ({
    id: l.id,
    name: l.name,
    address: l.address,
  }));
}

function activeBranchName(dto: PublicStorefront): string | null {
  if (!dto.activeLocationId) {
    return null;
  }
  return (dto.locations ?? []).find((l) => l.id === dto.activeLocationId)?.name ?? null;
}

function buildSocialLinks(dto: PublicStorefront): ShowcaseSocialLink[] {
  const links: ShowcaseSocialLink[] = [];
  const s = dto.social;

  if (s.instagramUrl) {
    links.push({
      platform: 'Instagram',
      handle: s.instagramHandle?.trim() || 'Instagram',
      url: s.instagramUrl,
      emoji: '📸',
      color: '#E1306C',
      followers: s.instagramFollowers?.trim() || null,
    });
  }
  if (s.facebookUrl) {
    links.push({
      platform: 'Facebook',
      handle: s.facebookHandle?.trim() || 'Facebook',
      url: s.facebookUrl,
      emoji: '👤',
      color: '#1877F2',
      followers: s.facebookFollowers?.trim() || null,
    });
  }
  if (s.tikTokUrl) {
    links.push({
      platform: 'TikTok',
      handle: s.tikTokHandle?.trim() || 'TikTok',
      url: s.tikTokUrl,
      emoji: '🎵',
      color: '#010101',
      followers: s.tikTokFollowers?.trim() || null,
    });
  }
  if (s.xUrl) {
    links.push({
      platform: 'X',
      handle: s.xHandle?.trim() || 'X',
      url: s.xUrl,
      emoji: '𝕏',
      color: '#14171A',
      followers: s.xFollowers?.trim() || null,
    });
  }
  if (s.contactPhone) {
    const digits = s.contactPhone.replace(/\D/g, '');
    links.push({
      platform: 'WhatsApp',
      handle: s.contactPhone,
      url: digits ? `https://wa.me/${digits}` : '#',
      emoji: '💬',
      color: '#25D366',
      followers: null,
    });
  }

  return links;
}

function splitAboutParagraphs(description: string | undefined): string[] {
  if (!description?.trim()) {
    return [];
  }
  return description
    .split(/\n\s*\n/)
    .map((p) => p.trim())
    .filter(Boolean);
}
