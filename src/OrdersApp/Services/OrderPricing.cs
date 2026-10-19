using OrdersApp.Models;

namespace OrdersApp.Services;

/// <summary>
/// Pure pricing rules for orders: no I/O, no dependencies.
/// This is the easiest kind of code to unit test — input in, value out.
/// </summary>
public static class OrderPricing
{
    public const decimal DiscountThreshold = 100m;
    public const decimal DiscountRate = 0.10m;
    public const decimal TaxRate = 0.08m;

    public static decimal CalculateSubtotal(IEnumerable<OrderItem> items) =>
        items.Sum(item => item.UnitPrice * item.Quantity);

    public static decimal CalculateDiscount(decimal subtotal) =>
        subtotal >= DiscountThreshold
            ? Math.Round(subtotal * DiscountRate, 2)
            : 0m;

    public static decimal CalculateTotal(IEnumerable<OrderItem> items)
    {
        var subtotal = CalculateSubtotal(items);
        var discounted = subtotal - CalculateDiscount(subtotal);
        return Math.Round(discounted * (1 + TaxRate), 2);
    }
}
