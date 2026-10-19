using FluentAssertions;
using Moq;
using OrdersApp.Data;
using OrdersApp.Models;
using OrdersApp.Services;
using Xunit;

namespace OrdersApp.Tests;

/// <summary>
/// Unit tests with Moq: the repository is a mock, so we verify behavior
/// (was SaveAsync called? with what?) without touching a real store.
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repository = new();

    private OrderService CreateService() => new(_repository.Object);

    private static CreateOrderRequest ValidRequest() => new()
    {
        CustomerName = "Grace Hopper",
        Items = new List<CreateOrderItem>
        {
            new() { ProductName = "Gadget", UnitPrice = 30m, Quantity = 4 }
        }
    };

    [Fact]
    public async Task CreateOrderAsync_Saves_Order_With_Calculated_Total()
    {
        _repository
            .Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        var order = await CreateService().CreateOrderAsync(ValidRequest());

        order.Total.Should().Be(116.64m);
        order.Status.Should().Be("Created");
        _repository.Verify(
            r => r.SaveAsync(
                It.Is<Order>(o => o.CustomerName == "Grace Hopper"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_With_Empty_Items_Throws_And_Saves_Nothing()
    {
        var request = ValidRequest();
        request.Items.Clear();

        var act = () => CreateService().CreateOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*at least one item*");
        _repository.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrderAsync_With_Blank_CustomerName_Throws(string customerName)
    {
        var request = ValidRequest();
        request.CustomerName = customerName;

        var act = () => CreateService().CreateOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Customer name*");
    }

    [Fact]
    public async Task CreateOrderAsync_With_Negative_Price_Throws()
    {
        var request = ValidRequest();
        request.Items[0].UnitPrice = -5m;

        var act = () => CreateService().CreateOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
        _repository.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_Trims_Whitespace_From_Names()
    {
        _repository
            .Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        var request = ValidRequest();
        request.CustomerName = "  Grace Hopper  ";

        var order = await CreateService().CreateOrderAsync(request);

        order.CustomerName.Should().Be("Grace Hopper");
    }
}
