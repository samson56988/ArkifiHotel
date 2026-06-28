import type { HotelShowcase } from '../models/hotel-showcase.models';
import type { ShortletListing, ShortletShowcase } from '../models/shortlet-showcase.models';
import type { ShowcaseRoom } from '../models/hotel-showcase.models';

export function listingToShowcaseRoom(listing: ShortletListing): ShowcaseRoom {
  return {
    id: listing.id,
    name: listing.title,
    roomType: 'Apartment',
    beds: `${listing.beds} bed${listing.beds === 1 ? '' : 's'}`,
    size: `${listing.guests} guests · ${listing.baths} bath`,
    basePricePerNight: listing.nightlyPrice,
    basePricePerWeek: listing.weeklyPrice ?? null,
    maxOccupancy: listing.guests,
    available: true,
    description: listing.description,
    amenities: listing.highlightAmenities,
    primaryImageUrl: listing.images[0] ?? null,
    imageUrls: listing.images,
    locationId: undefined,
    locationName: null,
    featured: listing.featured,
  };
}

export function shortletAsHotelShowcase(sl: ShortletShowcase): HotelShowcase {
  return {
    businessId: sl.businessId,
    businessName: sl.businessName,
    slug: sl.slug,
    logoUrl: sl.logoUrl,
    theme: sl.theme,
    location: sl.neighborhood,
    category: 'Shortlet',
    stars: 0,
    checkIn: sl.checkIn,
    checkOut: sl.checkOut,
    heroImages: [sl.heroImage],
    aboutImageUrl: sl.galleryImages[0] ?? null,
    galleryImages: sl.galleryImages,
    aboutQuote: '',
    aboutQuoteBy: sl.host.name,
    aboutStory: [],
    aboutStats: [],
    social: {
      facebookUrl: null,
      facebookHandle: null,
      facebookFollowers: null,
      instagramUrl: null,
      instagramHandle: null,
      instagramFollowers: null,
      tikTokUrl: null,
      tikTokHandle: null,
      tikTokFollowers: null,
      xUrl: null,
      xHandle: null,
      xFollowers: null,
      contactEmail: null,
      contactPhone: null,
    },
    socialLinks: [],
    locations: sl.locations,
    requiresBranchSelection: sl.requiresBranchSelection,
    activeLocationId: sl.activeLocationId,
    branchName: sl.branchName,
    rooms: sl.listings.map(listingToShowcaseRoom),
    facilities: [],
    restaurant: null,
    eventHalls: [],
    eventHallsPage: null,
  };
}
