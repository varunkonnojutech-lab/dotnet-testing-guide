namespace OrdersApp.Models;

/// <summary>
/// Payload accepted by POST /orders.
/// </summary>
public sealed class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public List<CreateOrderItem> Items { get; set; } = new();
}

/// <summary>
/// A single line item in a create-order request.
/// </summary>
public sealed class CreateOrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
