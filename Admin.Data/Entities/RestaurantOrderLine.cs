namespace Admin.Data.Entities;

public class RestaurantOrderLine
{
    public Guid Id { get; set; }

    public Guid RestaurantOrderId { get; set; }

    public RestaurantOrder RestaurantOrder { get; set; } = null!;

    public Guid MenuItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
