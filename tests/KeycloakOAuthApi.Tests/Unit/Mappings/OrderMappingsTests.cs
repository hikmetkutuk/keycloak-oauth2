using KeycloakOAuthApi.Domain;
using KeycloakOAuthApi.Mappings;

namespace KeycloakOAuthApi.Tests.Unit.Mappings;

public sealed class OrderMappingsTests
{
    [Fact]
    public void ToResponse_MapsOrderAndLineValues()
    {
        var line = new OrderLine(Guid.NewGuid(), "4K Monitor", 2, 399m);
        var order = new Order(Guid.NewGuid(), "sub-1", "demo", [line], DateTime.UtcNow);

        var response = order.ToResponse();

        Assert.Equal(order.Id, response.Id);
        Assert.Equal(order.CustomerUsername, response.CustomerUsername);
        Assert.Equal(order.TotalAmount, response.TotalAmount);
        Assert.Single(response.Lines);
        Assert.Equal(line.LineTotal, response.Lines.First().LineTotal);
    }
}
