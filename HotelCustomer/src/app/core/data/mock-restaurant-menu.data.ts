import type { ShowcaseRestaurant } from '../models/restaurant.models';

const FOOD_IMG = {
  appetizer: 'https://images.unsplash.com/photo-1541014742521-4031758946-0-228c4ac12b?w=600&q=80',
  soup: 'https://images.unsplash.com/photo-1547592166-23ac45744acd?w=600&q=80',
  jollof: 'https://images.unsplash.com/photo-1604329760661-e71dc83f26f5?w=600&q=80',
  steak: 'https://images.unsplash.com/photo-1600891964092-4316c288032e?w=600&q=80',
  dessert: 'https://images.unsplash.com/photo-1551024506-0bccd8281f66?w=600&q=80',
};

const DRINK_IMG = {
  cocktail: 'https://images.unsplash.com/photo-1514362545857-3bc16c4c7d88?w=600&q=80',
  mocktail: 'https://images.unsplash.com/photo-1546173159-315724a31696?w=600&q=80',
  wine: 'https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=600&q=80',
};

/** Full restaurant menu for Lekki Suites demo storefront. */
export const LEKKI_RESTAURANT_MENU: ShowcaseRestaurant = {
  enabled: true,
  navLabel: 'Restaurant & menu',
  heroEyebrow: 'Dining at Lekki',
  heroTitle: 'The Lekki Table',
  heroSubtitle:
    'Nigerian soul food, continental classics, and a rooftop bar — served daily from breakfast through late night.',
  heroImageUrl: 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=1600&q=80',
  mealsSectionTitle: 'Meals',
  drinksSectionTitle: 'Drinks',
  foodCategories: [
    {
      id: 'cat-appetizers',
      name: 'Appetizers',
      items: [
        {
          id: 'food-1',
          name: 'Pepper Soup Shooters',
          description: 'Goat meat pepper soup served in tasting cups with fresh scent leaves.',
          price: 4500,
          tags: ['Spicy', 'Chef\'s pick'],
          imageUrl: FOOD_IMG.appetizer,
        },
        {
          id: 'food-2',
          name: 'Puff Puff Trio',
          description: 'Classic, cinnamon-sugar, and pepper-honey dips with palm wine reduction.',
          price: 3200,
          tags: ['Vegetarian'],
        },
        {
          id: 'food-3',
          name: 'Grilled Plantain & Suya Skewers',
          description: 'Sweet plantain coins with beef suya, onion relish, and yaji dust.',
          price: 5800,
          tags: ['Spicy'],
          imageUrl: FOOD_IMG.appetizer,
        },
      ],
    },
    {
      id: 'cat-soups',
      name: 'Soups & Salads',
      items: [
        {
          id: 'food-4',
          name: 'Edikaikong Bowl',
          description: 'Waterleaf and ugu stew with assorted meats, served with semolina dumplings.',
          price: 8500,
          tags: ['Chef\'s pick'],
          imageUrl: FOOD_IMG.soup,
        },
        {
          id: 'food-5',
          name: 'Lekki Garden Salad',
          description: 'Mixed greens, mango, cashews, and citrus vinaigrette with optional grilled prawns.',
          price: 6200,
          tags: ['Vegetarian'],
        },
      ],
    },
    {
      id: 'cat-snacks',
      name: 'Snacks & Small Plates',
      items: [
        {
          id: 'food-6',
          name: 'Akara Sliders',
          description: 'Three bean fritter sliders with pepper sauce and pickled onions.',
          price: 4800,
          tags: ['Vegetarian', 'Spicy'],
        },
        {
          id: 'food-7',
          name: 'Loaded Yam Fries',
          description: 'Crispy yam chips with suya spice, cheese melt, and scotch bonnet aioli.',
          price: 5500,
        },
      ],
    },
    {
      id: 'cat-mains-ng',
      name: 'Mains — Nigerian',
      items: [
        {
          id: 'food-8',
          name: 'Smoked Jollof & Grilled Chicken',
          description: 'Party-style jollof over charcoal, half chicken, fried plantain, and coleslaw.',
          price: 12500,
          tags: ['Chef\'s pick'],
          imageUrl: FOOD_IMG.jollof,
        },
        {
          id: 'food-9',
          name: 'Seafood Okro Pot',
          description: 'Catfish, prawns, and periwinkle in rich okro, with eba or pounded yam.',
          price: 14800,
        },
        {
          id: 'food-10',
          name: 'Ofada Plate',
          description: 'Local rice, ayamase sauce, assorted offal, and fried plantain.',
          price: 11200,
          tags: ['Spicy'],
        },
      ],
    },
    {
      id: 'cat-mains-cont',
      name: 'Mains — Continental',
      items: [
        {
          id: 'food-11',
          name: 'Atlantic Catch',
          description: 'Pan-seared barracuda, lemon butter, herb potatoes, and seasonal greens.',
          price: 16500,
          imageUrl: FOOD_IMG.steak,
        },
        {
          id: 'food-12',
          name: 'Herb-Crusted Lamb Rack',
          description: 'Mint jus, roasted root vegetables, and red wine reduction.',
          price: 22000,
          tags: ['Chef\'s pick'],
          imageUrl: FOOD_IMG.steak,
        },
      ],
    },
    {
      id: 'cat-desserts',
      name: 'Desserts',
      items: [
        {
          id: 'food-13',
          name: 'Chin Chin Cheesecake',
          description: 'Baked cheesecake with caramelized chin chin crumble and mango coulis.',
          price: 6500,
          imageUrl: FOOD_IMG.dessert,
        },
        {
          id: 'food-14',
          name: 'Palm Wine Sorbet',
          description: 'Light sorbet with toasted coconut and lime zest.',
          price: 4200,
          tags: ['Vegetarian'],
        },
      ],
    },
  ],
  drinkCategories: [
    {
      id: 'cat-cocktails',
      name: 'Cocktails',
      items: [
        {
          id: 'drink-1',
          name: 'Lagoon Sunset',
          description: 'Bourbon, hibiscus syrup, bitters, and smoked orange peel.',
          price: 7500,
          tags: ['Signature'],
          imageUrl: DRINK_IMG.cocktail,
        },
        {
          id: 'drink-2',
          name: 'Admiralty Old Fashioned',
          description: 'Whiskey, palm sugar cube, angostura, and charred rosemary.',
          price: 8200,
          imageUrl: DRINK_IMG.cocktail,
        },
        {
          id: 'drink-3',
          name: 'Chapman Royale',
          description: 'Elevated Chapman with prosecco, cucumber, and grenadine.',
          price: 6800,
          tags: ['Signature'],
        },
      ],
    },
    {
      id: 'cat-mocktails',
      name: 'Mocktails',
      items: [
        {
          id: 'drink-4',
          name: 'Virgin Zobo Fizz',
          description: 'Hibiscus, ginger, lime, and soda — no alcohol.',
          price: 4500,
          tags: ['Non-alcoholic'],
          imageUrl: DRINK_IMG.mocktail,
        },
        {
          id: 'drink-5',
          name: 'Tropical Breeze',
          description: 'Pineapple, coconut cream, and mint over crushed ice.',
          price: 4200,
          tags: ['Non-alcoholic'],
        },
      ],
    },
    {
      id: 'cat-wines',
      name: 'Wines',
      items: [
        {
          id: 'drink-6',
          name: 'Chenin Blanc — Glass',
          description: 'South African white, crisp stone fruit notes.',
          price: 6500,
          imageUrl: DRINK_IMG.wine,
        },
        {
          id: 'drink-7',
          name: 'Shiraz — Bottle',
          description: 'Full-bodied red, blackberry and pepper finish.',
          price: 28000,
        },
      ],
    },
    {
      id: 'cat-beers',
      name: 'Beers & Ciders',
      items: [
        {
          id: 'drink-8',
          name: 'Star Lager',
          description: '330ml, ice-cold.',
          price: 2500,
        },
        {
          id: 'drink-9',
          name: 'Craft Cider',
          description: 'Imported apple cider, 330ml.',
          price: 4800,
        },
      ],
    },
    {
      id: 'cat-hot-cold',
      name: 'Hot & Cold Beverages',
      items: [
        {
          id: 'drink-10',
          name: 'Barista Coffee',
          description: 'Espresso, americano, or cappuccino.',
          price: 2800,
        },
        {
          id: 'drink-11',
          name: 'Fresh Pressed Juice',
          description: 'Orange, pineapple, or watermelon — daily selection.',
          price: 3500,
          tags: ['Non-alcoholic'],
        },
      ],
    },
  ],
};
