using Admin.Data.Entities;

namespace Admin.Infrastructure.Seeding;

internal static class RestaurantMenuSeedData
{
    internal const string HeroImageUrl =
        "https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=1200&q=80";

    internal static IReadOnlyList<SeedCategory> Categories { get; } =
    [
        new SeedCategory(
            "Appetizers",
            RestaurantMenuSection.Food,
            1,
            [
                new SeedItem("Pepper Soup Shooters", "Goat meat pepper soup served in tasting cups with fresh scent leaves.", 4500, ["Spicy", "Chef's pick"], "https://images.unsplash.com/photo-1541014742521-4031758946-0-228c4ac12b?w=600&q=80"),
                new SeedItem("Puff Puff Trio", "Classic, cinnamon-sugar, and pepper-honey dips.", 3200, ["Vegetarian"], null),
                new SeedItem("Grilled Plantain & Suya Skewers", "Sweet plantain with beef suya and yaji dust.", 5800, ["Spicy"], "https://images.unsplash.com/photo-1541014742521-4031758946-0-228c4ac12b?w=600&q=80"),
            ]),
        new SeedCategory(
            "Soups & Salads",
            RestaurantMenuSection.Food,
            2,
            [
                new SeedItem("Edikaikong Bowl", "Waterleaf and ugu stew with assorted meats.", 8500, ["Chef's pick"], "https://images.unsplash.com/photo-1547592166-23ac45744acd?w=600&q=80"),
                new SeedItem("Lekki Garden Salad", "Mixed greens, mango, cashews, and citrus vinaigrette.", 6200, ["Vegetarian"], null),
            ]),
        new SeedCategory(
            "Snacks & Small Plates",
            RestaurantMenuSection.Food,
            3,
            [
                new SeedItem("Akara Sliders", "Bean fritter sliders with pepper sauce.", 4800, ["Vegetarian", "Spicy"], null),
                new SeedItem("Loaded Yam Fries", "Crispy yam chips with suya spice and scotch bonnet aioli.", 5500, [], null),
            ]),
        new SeedCategory(
            "Mains — Nigerian",
            RestaurantMenuSection.Food,
            4,
            [
                new SeedItem("Smoked Jollof & Grilled Chicken", "Party-style jollof, half chicken, and fried plantain.", 12500, ["Chef's pick"], "https://images.unsplash.com/photo-1604329760661-e71dc83f26f5?w=600&q=80"),
                new SeedItem("Seafood Okro Pot", "Catfish, prawns, and periwinkle in rich okro.", 14800, [], null),
                new SeedItem("Ofada Plate", "Local rice, ayamase sauce, and fried plantain.", 11200, ["Spicy"], null),
            ]),
        new SeedCategory(
            "Mains — Continental",
            RestaurantMenuSection.Food,
            5,
            [
                new SeedItem("Atlantic Catch", "Pan-seared barracuda with herb potatoes.", 16500, [], "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=600&q=80"),
                new SeedItem("Herb-Crusted Lamb Rack", "Mint jus and roasted root vegetables.", 22000, ["Chef's pick"], "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=600&q=80"),
            ]),
        new SeedCategory(
            "Desserts",
            RestaurantMenuSection.Food,
            6,
            [
                new SeedItem("Chin Chin Cheesecake", "Caramelized chin chin crumble and mango coulis.", 6500, [], "https://images.unsplash.com/photo-1551024506-0bccd8281f66?w=600&q=80"),
                new SeedItem("Palm Wine Sorbet", "Light sorbet with toasted coconut.", 4200, ["Vegetarian"], null),
            ]),
        new SeedCategory(
            "Cocktails",
            RestaurantMenuSection.Drink,
            1,
            [
                new SeedItem("Lagoon Sunset", "Bourbon, hibiscus syrup, and smoked orange peel.", 7500, ["Signature"], "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d88?w=600&q=80"),
                new SeedItem("Admiralty Old Fashioned", "Whiskey, palm sugar, and charred rosemary.", 8200, [], "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d88?w=600&q=80"),
                new SeedItem("Chapman Royale", "Elevated Chapman with prosecco and cucumber.", 6800, ["Signature"], null),
            ]),
        new SeedCategory(
            "Mocktails",
            RestaurantMenuSection.Drink,
            2,
            [
                new SeedItem("Virgin Zobo Fizz", "Hibiscus, ginger, lime, and soda.", 4500, ["Non-alcoholic"], "https://images.unsplash.com/photo-1546173159-315724a31696?w=600&q=80"),
                new SeedItem("Tropical Breeze", "Pineapple, coconut cream, and mint.", 4200, ["Non-alcoholic"], null),
            ]),
        new SeedCategory(
            "Wines",
            RestaurantMenuSection.Drink,
            3,
            [
                new SeedItem("Chenin Blanc — Glass", "South African white, crisp stone fruit.", 6500, [], "https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=600&q=80"),
                new SeedItem("Shiraz — Bottle", "Full-bodied red with pepper finish.", 28000, [], null),
            ]),
        new SeedCategory(
            "Beers & Ciders",
            RestaurantMenuSection.Drink,
            4,
            [
                new SeedItem("Star Lager", "330ml, ice-cold.", 2500, [], null),
                new SeedItem("Craft Cider", "Imported apple cider, 330ml.", 4800, [], null),
            ]),
        new SeedCategory(
            "Hot & Cold Beverages",
            RestaurantMenuSection.Drink,
            5,
            [
                new SeedItem("Barista Coffee", "Espresso, americano, or cappuccino.", 2800, [], null),
                new SeedItem("Fresh Pressed Juice", "Orange, pineapple, or watermelon.", 3500, ["Non-alcoholic"], null),
            ]),
    ];

    internal sealed record SeedCategory(
        string Name,
        RestaurantMenuSection Section,
        int SortOrder,
        IReadOnlyList<SeedItem> Items);

    internal sealed record SeedItem(
        string Name,
        string Description,
        decimal Price,
        string[] Tags,
        string? ImageUrl);
}
