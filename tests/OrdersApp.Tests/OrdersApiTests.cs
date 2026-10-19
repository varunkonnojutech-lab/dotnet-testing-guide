using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OrdersApp.Models;
using Xunit;

namespace OrdersApp.Tests;

/// <summary>
/// Integration tests: <see cref="WebApplicationFactory{Program}"/> boots the
/// real API in-process and exercises it over HTTP — routing, model binding,
/// validation, and status codes included.
/// </summary>
public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Order_Returns_Created_With_Location_And_Total()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Ada Lovelace",
            Items = new List<CreateOrderItem>
            {
                new() { ProductName = "Widget", UnitPrice = 25m, Quantity = 2 }
            }
        };

        var response = await _client.PostAsJsonAsync("/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var order = await response.Content.ReadFromJsonAsync<Order>();
        order.Should().NotBeNull();
        order!.CustomerName.Should().Be("Ada Lovelace");
        order.Total.Should().Be(54m);
        order.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_Order_By_Id_Returns_The_Created_Order()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Katherine Johnson",
            Items = new List<CreateOrderItem>
            {
                new() { ProductName = "Gadget", UnitPrice = 10m, Quantity = 1 }
            }
        };
        var created = await _client.PostAsJsonAsync("/orders", request);
        var createdOrder = await created.Content.ReadFromJsonAsync<Order>();

        var response = await _client.GetAsync($"/orders/{createdOrder!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<Order>();
        order!.CustomerName.Should().Be("Katherine Johnson");
    }

    [Fact]
    public async Task Get_Order_By_Id_Returns_NotFound_For_Unknown_Id()
    {
        var response = await _client.GetAsync("/orders/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_Orders_Includes_Previously_Created_Order()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Alan Turing",
            Items = new List<CreateOrderItem>
            {
                new() { ProductName = "Gadget", UnitPrice = 10m, Quantity = 1 }
            }
        };
        var created = await _client.PostAsJsonAsync("/orders", request);
        created.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
        orders.Should().NotBeNull();
        orders!.Should().Contain(o => o.CustomerName == "Alan Turing");
    }

    [Fact]
    public async Task Post_Order_Without_Items_Returns_BadRequest()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Nobody",
            Items = new List<CreateOrderItem>()
        };

        var response = await _client.PostAsJsonAsync("/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
