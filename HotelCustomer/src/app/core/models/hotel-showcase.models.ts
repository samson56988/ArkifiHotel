import type { PublicStorefront, PublicStorefrontFacility, PublicStorefrontRoom } from './storefront-theme.models';
import type { ShowcaseRestaurant } from './restaurant.models';

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

export interface ShowcaseLocation {
  id: string;
  name: string;
  address: string | null;
}

export interface HotelShowcase extends Omit<PublicStorefront, 'rooms' | 'facilities' | 'restaurant'> {
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
  locations: ShowcaseLocation[];
  requiresBranchSelection: boolean;
  activeLocationId: string | null;
  branchName: string | null;
  rooms: ShowcaseRoom[];
  facilities: ShowcaseFacility[];
  /** Optional in-house restaurant & menu (mock until API ships). */
  restaurant: ShowcaseRestaurant | null;
}
