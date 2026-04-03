using KeycloakOAuthApi.Domain;
using KeycloakOAuthApi.Mappings;

namespace KeycloakOAuthApi.Tests.Unit.Mappings;

public sealed class ProductMappingsTests
{
    [Fact]
    public void ToResponse_MapsAllFields()
    {
        var now = DateTime.UtcNow;
        var product = new Product(Guid.NewGuid(), "Keyboard", "Wireless", 149.9m, 5, now);

        var response = product.ToResponse();

        Assert.Equal(product.Id, response.Id);
        Assert.Equal(product.Name, response.Name);
        Assert.Equal(product.Description, response.Description);
        Assert.Equal(product.Price, response.Price);
        Assert.Equal(product.StockQuantity, response.StockQuantity);
        Assert.Equal(product.IsActive, response.IsActive);
        Assert.Equal(product.CreatedAtUtc, response.CreatedAtUtc);
    }
}
