using System.Collections.Concurrent;
using OrdersApp.Models;

namespace OrdersApp.Data;

/// <summary>
/// Thread-safe in-memory order store. The API and the integration tests run
/// against this, so no database is required.
/// </summary>
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<int, Order> _orders = new();
    private int _nextId;

    public Task<Order> SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref _nextId);
        order.Id = id;
        _orders[id] = order;
        return Task.FromResult(order);
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_orders.TryGetValue(id, out var order) ? order : null);

    public Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Order>>(_orders.Values.OrderBy(o => o.Id).ToList());
}
