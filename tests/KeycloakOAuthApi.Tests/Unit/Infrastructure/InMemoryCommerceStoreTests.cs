using KeycloakOAuthApi.Infrastructure;

namespace KeycloakOAuthApi.Tests.Unit.Infrastructure;

public sealed class InMemoryCommerceStoreTests
{
    [Fact]
    public void GetProducts_ReturnsSeededProducts()
    {
        var store = new InMemoryCommerceStore();

        var products = store.GetProducts();

        Assert.NotEmpty(products);
    }

    [Fact]
    public void PlaceOrder_WhenProductDoesNotExist_ReturnsFailedResult()
    {
        var store = new InMemoryCommerceStore();

        var result = store.PlaceOrder(
            "user-1",
            "demo",
            [new OrderLineInput(Guid.NewGuid(), 1)]);

        Assert.False(result.Succeeded);
        Assert.Null(result.Order);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void PlaceOrder_WhenInputIsValid_ReturnsOrderAndReservesStock()
    {
        var store = new InMemoryCommerceStore();
        var product = store.GetProducts().First();
        var initialStock = product.StockQuantity;

        var result = store.PlaceOrder(
            "user-1",
            "demo",
            [new OrderLineInput(product.Id, 2)]);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Order);
        Assert.Equal(initialStock - 2, store.GetProduct(product.Id)!.StockQuantity);
    }
}
