using System.Globalization;
using FluentAssertions;
using OrdersApp.Models;
using OrdersApp.Services;
using Xunit;

namespace OrdersApp.Tests;

/// <summary>
/// Pure unit tests: no mocks, no I/O. The fastest tests you can write —
/// input in, value out, asserted with FluentAssertions.
/// </summary>
public class OrderPricingTests
{
    // C# attributes cannot take decimal literals (CS0182), so the data is
    // passed as strings and parsed — a common xUnit pattern.
    [Theory]
    [InlineData("50.00", "0.00")]
    [InlineData("99.99", "0.00")]
    [InlineData("100.00", "10.00")]
    [InlineData("250.00", "25.00")]
    public void CalculateDiscount_Applies_Ten_Percent_At_Or_Above_Threshold(
        string subtotalText,
        string expectedDiscountText)
    {
        var subtotal = decimal.Parse(subtotalText, CultureInfo.InvariantCulture);
        var expectedDiscount = decimal.Parse(expectedDiscountText, CultureInfo.InvariantCulture);

        OrderPricing.CalculateDiscount(subtotal).Should().Be(expectedDiscount);
    }

    [Fact]
    public void CalculateSubtotal_Sums_Price_Times_Quantity()
    {
        var items = new List<OrderItem>
        {
            new() { ProductName = "Widget", UnitPrice = 25m, Quantity = 2 },
            new() { ProductName = "Gadget", UnitPrice = 10m, Quantity = 3 }
        };

        OrderPricing.CalculateSubtotal(items).Should().Be(80m);
    }

    [Fact]
    public void CalculateTotal_Applies_Discount_Then_Tax()
    {
        // 4 x 30 = 120 subtotal -> 12 discount -> 108 -> +8% tax = 116.64
        var items = new List<OrderItem>
        {
            new() { ProductName = "Gadget", UnitPrice = 30m, Quantity = 4 }
        };

        OrderPricing.CalculateTotal(items).Should().Be(116.64m);
    }

    [Fact]
    public void CalculateTotal_Without_Discount_Adds_Tax_Only()
    {
        // 2 x 25 = 50 subtotal -> no discount -> +8% tax = 54
        var items = new List<OrderItem>
        {
            new() { ProductName = "Widget", UnitPrice = 25m, Quantity = 2 }
        };

        OrderPricing.CalculateTotal(items).Should().Be(54m);
    }
}
