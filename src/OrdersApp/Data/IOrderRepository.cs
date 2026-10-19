using OrdersApp.Models;

namespace OrdersApp.Data;

/// <summary>
/// Persistence contract for orders. Keeping this behind an interface is what
/// makes <see cref="Services.OrderService"/> unit-testable with a mock.
/// </summary>
public interface IOrderRepository
{
    Task<Order> SaveAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default);
}
