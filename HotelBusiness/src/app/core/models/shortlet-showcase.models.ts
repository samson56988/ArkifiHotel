import type { StorefrontTheme } from './storefront-theme.models';

export interface ShowcaseLocation {
  id: string;
  name: string;
  address: string;
}

export interface ShortletHost {
  name: string;
  photoUrl: string;
  role: string;
  bio: string;
  responseTime: string;
  languages: string[];
  verified: boolean;
  rating: number;
  reviewCount: number;
  yearsHosting: number;
}

export interface ShortletListing {
  id: string;
  title: string;
  tagline: string;
  beds: number;
  baths: number;
  guests: number;
  nightlyPrice: number;
  weeklyPrice?: number | null;
  images: string[];
  highlightAmenities: string[];
  description: string;
  featured?: boolean;
  locationId?: string;
}

export interface ShortletAmenity {
  id: string;
  label: string;
  icon: string;
  description: string;
  category: 'Essentials' | 'Comfort' | 'Safety' | 'Work';
}

export interface ShortletShowcase {
  businessId: string;
  businessName: string;
  slug: string;
  logoUrl: string | null;
  tagline: string;
  neighborhood: string;
  heroImage: string;
  galleryImages: string[];
  host: ShortletHost;
  listings: ShortletListing[];
  amenities: ShortletAmenity[];
  houseRules: string[];
  theme: StorefrontTheme;
  locations: ShowcaseLocation[];
  requiresBranchSelection: boolean;
  activeLocationId: string | null;
  branchName: string | null;
  checkIn: string;
  checkOut: string;
  minNights: number;
}
