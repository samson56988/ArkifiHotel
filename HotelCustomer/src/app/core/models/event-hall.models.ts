export interface ShowcaseEventHall {
  id: string;
  name: string;
  description: string;
  rentalPrice: number;
  maxCapacity: number | null;
  primaryImageUrl: string | null;
  imageUrls: string[];
  locationId?: string;
  locationName?: string | null;
}

export interface ShowcaseEventHallsPage {
  navLabel: string;
  heroEyebrow: string;
  heroTitle: string;
  heroSubtitle: string;
  heroImageUrl: string | null;
}

export interface PublicStorefrontEventHall {
  id: string;
  name: string;
  description: string;
  rentalPrice: number;
  maxCapacity: number | null;
  primaryImageUrl: string | null;
  imageUrls: string[];
}
