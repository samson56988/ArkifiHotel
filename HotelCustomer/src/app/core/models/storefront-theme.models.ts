import type { ApiResult } from './api-result.model';

export type GlobalFontId =
  | 'modern-sans'
  | 'classic-serif'
  | 'elegant-display'
  | 'luxury-contrast'
  | 'warm-hospitality';

export type SectionFontRole = 'display' | 'body' | 'accent';

export type BannerStyleId = 'grand-hero' | 'split-showcase' | 'minimal-serif' | 'glass-panel' | 'sunset-gradient';

export type BannerTextAlign = 'left' | 'center' | 'right';

export type AboutLayoutId = 'side-by-side' | 'stacked' | 'quote';

export type RoomCardStyleId = 'elevated' | 'bordered' | 'minimal' | 'glass';

export type FacilityDisplayStyleId = 'grid' | 'carousel' | 'list';

export type FooterStyleId = 'simple' | 'columns' | 'centered' | 'dark-band';

export type FooterBackgroundStyleId = 'dark-band' | 'match-primary' | 'light';

export type ColorPresetId =
  | 'sage-luxe'
  | 'midnight-gold'
  | 'ocean-calm'
  | 'terracotta-warm'
  | 'slate-minimal';

export interface StorefrontBannerSection {
  style: BannerStyleId;
  headlineFont: SectionFontRole;
  subheadlineFont: SectionFontRole;
  headline: string;
  subheadline: string;
  textAlign: BannerTextAlign;
  overlayOpacity: number;
  badgeText: string;
}

export interface StorefrontAboutStat {
  num: string;
  label: string;
}

export interface StorefrontAboutSection {
  enabled: boolean;
  eyebrow: string;
  title: string;
  description: string;
  titleFont: SectionFontRole;
  bodyFont: SectionFontRole;
  layout: AboutLayoutId;
  quote: string;
  quoteBy: string;
  showStats: boolean;
  stats: StorefrontAboutStat[];
}

export const MAX_ABOUT_STATS = 4;
export const MAX_FACILITY_PERKS = 6;

export interface StorefrontRoomsSection {
  enabled: boolean;
  eyebrow: string;
  title: string;
  subtitle: string;
  titleFont: SectionFontRole;
  cardStyle: RoomCardStyleId;
  showPrice: boolean;
  showFeaturedSection: boolean;
  featuredEyebrow: string;
  featuredTitle: string;
  showPageStats: boolean;
  showPolicies: boolean;
  policyBreakfast: string;
  policyPets: string;
  policyCancellation: string;
  ctaTitle: string;
  ctaSubtitle: string;
  ctaButtonText: string;
}

export interface StorefrontFacilitiesSection {
  enabled: boolean;
  eyebrow: string;
  title: string;
  subtitle: string;
  titleFont: SectionFontRole;
  displayStyle: FacilityDisplayStyleId;
  showPageStats: boolean;
  supportStatValue: string;
  supportStatLabel: string;
  showGuestPerks: boolean;
  perksEyebrow: string;
  perksTitle: string;
  perksSubtitle: string;
  perksItems: string[];
}

export interface StorefrontFooterSection {
  style: FooterStyleId;
  tagline: string;
  copyrightText: string;
  showContact: boolean;
  backgroundStyle: FooterBackgroundStyleId;
}

export interface StorefrontContactSection {
  location: string;
  checkIn: string;
  checkOut: string;
  introText: string;
}

export interface StorefrontColorPalette {
  preset: ColorPresetId;
  primary: string;
  accent: string;
  background: string;
  text: string;
}

export interface StorefrontTheme {
  globalFont: GlobalFontId;
  banner: StorefrontBannerSection;
  about: StorefrontAboutSection;
  rooms: StorefrontRoomsSection;
  facilities: StorefrontFacilitiesSection;
  footer: StorefrontFooterSection;
  contact: StorefrontContactSection;
  colors: StorefrontColorPalette;
}

export interface BusinessSocialProfileDto {
  facebookUrl: string | null;
  facebookHandle: string | null;
  facebookFollowers: string | null;
  instagramUrl: string | null;
  instagramHandle: string | null;
  instagramFollowers: string | null;
  tikTokUrl: string | null;
  tikTokHandle: string | null;
  tikTokFollowers: string | null;
  xUrl: string | null;
  xHandle: string | null;
  xFollowers: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
}

export interface PublicStorefrontRoom {
  id: string;
  name: string;
  basePricePerNight: number;
  maxOccupancy: number;
  primaryImageUrl: string | null;
  locationName: string | null;
}

export interface PublicStorefrontFacility {
  id: string;
  name: string;
  primaryImageUrl: string | null;
  locationName: string | null;
}

export interface PublicStorefront {
  businessId: string;
  businessName: string;
  slug: string;
  logoUrl: string | null;
  theme: StorefrontTheme;
  rooms: PublicStorefrontRoom[];
  facilities: PublicStorefrontFacility[];
  social: BusinessSocialProfileDto;
  heroImages: string[];
  aboutImageUrl: string | null;
}

export type StorefrontThemeApiResponse = ApiResult<StorefrontTheme>;
export type PublicStorefrontApiResponse = ApiResult<PublicStorefront>;

export function createDefaultTheme(businessName: string): StorefrontTheme {
  const year = new Date().getFullYear();
  return {
    globalFont: 'modern-sans',
    banner: {
      style: 'grand-hero',
      headlineFont: 'display',
      subheadlineFont: 'body',
      headline: `Welcome to ${businessName}`,
      subheadline: 'Book your stay with us — comfort, style, and warm hospitality.',
      textAlign: 'center',
      overlayOpacity: 55,
      badgeText: 'Your stay awaits',
    },
    about: {
      enabled: true,
      eyebrow: 'Who We Are',
      title: 'Who we are',
      description:
        'We are a hospitality team dedicated to memorable stays. From check-in to checkout, every detail is crafted for comfort and ease.',
      titleFont: 'display',
      bodyFont: 'body',
      layout: 'side-by-side',
      quote: '',
      quoteBy: '',
      showStats: false,
      stats: [],
    },
    rooms: {
      enabled: true,
      eyebrow: 'Accommodations',
      title: 'Our rooms',
      subtitle: 'Thoughtfully designed spaces for every traveler.',
      titleFont: 'display',
      cardStyle: 'elevated',
      showPrice: true,
      showFeaturedSection: true,
      featuredEyebrow: 'Signature Stay',
      featuredTitle: 'Our most sought-after room',
      showPageStats: true,
      showPolicies: true,
      policyBreakfast: 'Complimentary for suite guests',
      policyPets: 'Small pets welcome on request',
      policyCancellation: 'Free up to 48 hours before arrival',
      ctaTitle: 'Ready to book your stay?',
      ctaSubtitle: 'Reserve directly — no payment required until confirmation.',
      ctaButtonText: 'Check availability',
    },
    facilities: {
      enabled: true,
      eyebrow: 'On Property',
      title: 'Facilities & amenities',
      subtitle: 'Relax, recharge, and enjoy our property.',
      titleFont: 'display',
      displayStyle: 'grid',
      showPageStats: true,
      supportStatValue: '24/7',
      supportStatLabel: 'Guest support',
      showGuestPerks: true,
      perksEyebrow: 'Guest Perks',
      perksTitle: 'Everything included in your stay',
      perksSubtitle:
        'Complimentary access to most on-property amenities for all registered guests.',
      perksItems: [
        'Daily housekeeping',
        'High-speed WiFi',
        'Pool & fitness access',
        'Concierge assistance',
        'Secure parking',
        'Late checkout on request',
      ],
    },
    footer: {
      style: 'columns',
      tagline: 'Your home away from home.',
      copyrightText: `© ${year} ${businessName}. All rights reserved.`,
      showContact: true,
      backgroundStyle: 'dark-band',
    },
    contact: {
      location: '',
      checkIn: '',
      checkOut: '',
      introText:
        'Questions about your stay? Send us a message and our team will respond within a few hours.',
    },
    colors: {
      preset: 'sage-luxe',
      primary: '#5c7a5c',
      accent: '#c8dcc8',
      background: '#faf9f6',
      text: '#1f2a1f',
    },
  };
}
