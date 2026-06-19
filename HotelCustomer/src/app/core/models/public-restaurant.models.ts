export interface PublicStorefrontMenuItem {
  id: string;
  name: string;
  description: string;
  price: number;
  tags: string[];
  imageUrl: string | null;
}

export interface PublicStorefrontMenuCategory {
  id: string;
  name: string;
  items: PublicStorefrontMenuItem[];
}

export interface PublicStorefrontRestaurant {
  enabled: boolean;
  navLabel: string;
  heroEyebrow: string;
  heroTitle: string;
  heroSubtitle: string;
  mealsSectionTitle: string;
  drinksSectionTitle: string;
  heroImageUrl: string | null;
  foodCategories: PublicStorefrontMenuCategory[];
  drinkCategories: PublicStorefrontMenuCategory[];
}
