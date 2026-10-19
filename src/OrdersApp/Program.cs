using OrdersApp.Data;
using OrdersApp.Models;
using OrdersApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.MapGet("/orders", async (IOrderRepository repository) =>
    Results.Ok(await repository.ListAsync()));

app.MapGet("/orders/{id:int}", async (int id, IOrderRepository repository) =>
    await repository.GetByIdAsync(id) is { } order
        ? Results.Ok(order)
        : Results.NotFound());

app.MapPost("/orders", async (CreateOrderRequest request, OrderService service) =>
{
    try
    {
        var order = await service.CreateOrderAsync(request);
        return Results.Created($"/orders/{order.Id}", order);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

// Exposes the entry point to WebApplicationFactory so the integration
// tests can boot the real app in-process.
public partial class Program
{
}
