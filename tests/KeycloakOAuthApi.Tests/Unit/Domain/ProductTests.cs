using KeycloakOAuthApi.Domain;

namespace KeycloakOAuthApi.Tests.Unit.Domain;

public sealed class ProductTests
{
    [Fact]
    public void TryReserve_WhenStockIsEnough_DecreasesStock()
    {
        var product = new Product(Guid.NewGuid(), "Keyboard", "Test", 10m, 5, DateTime.UtcNow);

        var result = product.TryReserve(3);

        Assert.True(result);
        Assert.Equal(2, product.StockQuantity);
    }

    [Fact]
    public void TryReserve_WhenStockIsInsufficient_ReturnsFalseWithoutChangingStock()
    {
        var product = new Product(Guid.NewGuid(), "Keyboard", "Test", 10m, 2, DateTime.UtcNow);

        var result = product.TryReserve(3);

        Assert.False(result);
        Assert.Equal(2, product.StockQuantity);
    }

    [Fact]
    public void SetStock_WhenQuantityIsNegative_Throws()
    {
        var product = new Product(Guid.NewGuid(), "Keyboard", "Test", 10m, 2, DateTime.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.SetStock(-1));
    }
}
