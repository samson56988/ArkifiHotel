import type { ShowcaseEventHall } from '../models/event-hall.models';

const HALL_IMG = {
  ballroom: 'https://images.unsplash.com/photo-1519167758481-83f550bb49b8?w=1200&q=80',
  conference: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=1200&q=80',
  garden: 'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=1200&q=80',
  rooftop: 'https://images.unsplash.com/photo-1511795409834-ef04bbd61622?w=1200&q=80',
};

/** Event halls for Lekki Suites — branch-scoped via locationId. */
export const LEKKI_EVENT_HALLS: ShowcaseEventHall[] = [
  {
    id: 'eh-lekki-grand',
    name: 'Admiralty Grand Ballroom',
    description:
      'Our flagship 420-capacity ballroom with crystal chandeliers, built-in AV, and lagoon views through floor-to-ceiling windows. Ideal for weddings, galas, and corporate awards nights.',
    rentalPrice: 850000,
    maxCapacity: 420,
    primaryImageUrl: HALL_IMG.ballroom,
    imageUrls: [HALL_IMG.ballroom, HALL_IMG.conference],
    locationId: 'loc-lekki-main',
    locationName: 'Main Tower',
  },
  {
    id: 'eh-lekki-boardroom',
    name: 'Executive Boardroom',
    description:
      'Intimate 24-seat boardroom with video conferencing, whiteboard walls, and dedicated concierge. Half-day and full-day packages include coffee service.',
    rentalPrice: 180000,
    maxCapacity: 24,
    primaryImageUrl: HALL_IMG.conference,
    imageUrls: [HALL_IMG.conference],
    locationId: 'loc-lekki-main',
    locationName: 'Main Tower',
  },
  {
    id: 'eh-lekki-garden',
    name: 'Garden Pavilion',
    description:
      'Open-air pavilion surrounded by tropical landscaping — perfect for cocktail receptions, product launches, and sunset ceremonies for up to 150 guests.',
    rentalPrice: 420000,
    maxCapacity: 150,
    primaryImageUrl: HALL_IMG.garden,
    imageUrls: [HALL_IMG.garden, HALL_IMG.rooftop],
    locationId: 'loc-lekki-garden',
    locationName: 'Garden Wing',
  },
  {
    id: 'eh-lekki-sky',
    name: 'Sky Lounge Events',
    description:
      'Twelfth-floor lounge available for private buyouts — panoramic Lagos skyline, modular seating, and in-house bar team for evening events.',
    rentalPrice: 650000,
    maxCapacity: 80,
    primaryImageUrl: HALL_IMG.rooftop,
    imageUrls: [HALL_IMG.rooftop, HALL_IMG.ballroom],
    locationId: 'loc-lekki-garden',
    locationName: 'Garden Wing',
  },
];

export const LEKKI_EVENT_HALLS_PAGE = {
  navLabel: 'Event halls',
  heroEyebrow: 'Meetings & celebrations',
  heroTitle: 'Event spaces at Lekki Suites',
  heroSubtitle:
    'From intimate boardrooms to grand ballrooms — submit a request and our events team will confirm availability. No online payment required.',
  heroImageUrl: HALL_IMG.ballroom,
};
