using OrdersApp.Data;
using OrdersApp.Models;

namespace OrdersApp.Services;

/// <summary>
/// Application logic for creating orders. Depends on
/// <see cref="IOrderRepository"/> so unit tests can substitute a mock
/// instead of a real store.
/// </summary>
public sealed class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Order> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ArgumentException("Customer name is required.", nameof(request));

        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException("An order must contain at least one item.", nameof(request));

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ProductName))
                throw new ArgumentException("Each item must have a product name.", nameof(request));
            if (item.Quantity <= 0)
                throw new ArgumentException("Each item quantity must be greater than zero.", nameof(request));
            if (item.UnitPrice < 0)
                throw new ArgumentException("Each item price cannot be negative.", nameof(request));
        }

        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            Items = request.Items
                .Select(item => new OrderItem
                {
                    ProductName = item.ProductName.Trim(),
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                })
                .ToList(),
            Status = "Created"
        };
        order.Total = OrderPricing.CalculateTotal(order.Items);

        return await _repository.SaveAsync(order, cancellationToken);
    }
}
