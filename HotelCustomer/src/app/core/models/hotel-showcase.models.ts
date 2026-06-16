import type { PublicStorefront, PublicStorefrontFacility, PublicStorefrontRoom } from './storefront-theme.models';

export interface ShowcaseRoom extends PublicStorefrontRoom {
  roomType: string;
  beds: string;
  size: string;
  available: boolean;
  description: string;
  amenities: string[];
  featured?: boolean;
}

export interface ShowcaseFacility extends PublicStorefrontFacility {
  description: string;
  emoji: string;
  category: string;
  hours: string | null;
  featured?: boolean;
}

export interface ShowcaseSocialLink {
  platform: string;
  handle: string;
  url: string;
  emoji: string;
  color: string;
  followers: string | null;
}

export interface HotelShowcase extends Omit<PublicStorefront, 'rooms' | 'facilities'> {
  location: string;
  category: string;
  stars: number;
  checkIn: string;
  checkOut: string;
  heroImages: string[];
  galleryImages: string[];
  aboutQuote: string;
  aboutQuoteBy: string;
  aboutStory: string[];
  aboutStats: { num: string; label: string }[];
  socialLinks: ShowcaseSocialLink[];
  rooms: ShowcaseRoom[];
  facilities: ShowcaseFacility[];
}
