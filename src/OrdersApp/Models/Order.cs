namespace OrdersApp.Models;

/// <summary>
/// An order stored by the application.
/// </summary>
public sealed class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// A single line item on an order.
/// </summary>
public sealed class OrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
