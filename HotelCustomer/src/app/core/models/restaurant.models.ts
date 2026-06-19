/** Guest-facing menu item (mock / future API). */
export interface ShowcaseMenuItem {
  id: string;
  name: string;
  description: string;
  price: number;
  /** e.g. Vegan, Spicy, Chef's pick */
  tags?: string[];
  imageUrl?: string | null;
}

/** Admin-defined category grouping items under meals or drinks. */
export interface ShowcaseMenuCategory {
  id: string;
  name: string;
  items: ShowcaseMenuItem[];
}

/** Optional restaurant & bar section on the guest storefront. */
export interface ShowcaseRestaurant {
  enabled: boolean;
  /** Nav link label, e.g. "Restaurant & menu" */
  navLabel: string;
  heroEyebrow: string;
  heroTitle: string;
  heroSubtitle: string;
  heroImageUrl: string | null;
  /** Section heading above the meals list */
  mealsSectionTitle: string;
  /** Section heading above the drinks list */
  drinksSectionTitle: string;
  foodCategories: ShowcaseMenuCategory[];
  drinkCategories: ShowcaseMenuCategory[];
}
