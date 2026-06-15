import type { ApiResult } from './api-result.model';
import type { BusinessSocialProfileDto } from './business-social-profile.models';

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
}

export interface StorefrontAboutSection {
  enabled: boolean;
  title: string;
  description: string;
  titleFont: SectionFontRole;
  bodyFont: SectionFontRole;
  layout: AboutLayoutId;
}

export interface StorefrontRoomsSection {
  enabled: boolean;
  title: string;
  subtitle: string;
  titleFont: SectionFontRole;
  cardStyle: RoomCardStyleId;
  showPrice: boolean;
}

export interface StorefrontFacilitiesSection {
  enabled: boolean;
  title: string;
  subtitle: string;
  titleFont: SectionFontRole;
  displayStyle: FacilityDisplayStyleId;
}

export interface StorefrontFooterSection {
  style: FooterStyleId;
  tagline: string;
  copyrightText: string;
  showContact: boolean;
  backgroundStyle: FooterBackgroundStyleId;
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
  colors: StorefrontColorPalette;
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
    },
    about: {
      enabled: true,
      title: 'Who we are',
      description:
        'We are a hospitality team dedicated to memorable stays. From check-in to checkout, every detail is crafted for comfort and ease.',
      titleFont: 'display',
      bodyFont: 'body',
      layout: 'side-by-side',
    },
    rooms: {
      enabled: true,
      title: 'Our rooms',
      subtitle: 'Thoughtfully designed spaces for every traveler.',
      titleFont: 'display',
      cardStyle: 'elevated',
      showPrice: true,
    },
    facilities: {
      enabled: true,
      title: 'Facilities & amenities',
      subtitle: 'Relax, recharge, and enjoy our property.',
      titleFont: 'display',
      displayStyle: 'grid',
    },
    footer: {
      style: 'columns',
      tagline: 'Your home away from home.',
      copyrightText: `© ${year} ${businessName}. All rights reserved.`,
      showContact: true,
      backgroundStyle: 'dark-band',
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
