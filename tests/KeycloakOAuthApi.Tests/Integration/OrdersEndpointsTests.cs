using System.Net;
using System.Net.Http.Json;
using KeycloakOAuthApi.Contracts.Orders;
using KeycloakOAuthApi.Contracts.Products;
using KeycloakOAuthApi.Tests.Integration.Infrastructure;

namespace KeycloakOAuthApi.Tests.Integration;

public sealed class OrdersEndpointsTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task CreateOrderAndGetMine_WhenUserRole_ReturnsSuccess()
    {
        using var userClient = factory.CreateAuthenticatedClient("user-orders", "demo", "user");
        var productsResponse = await userClient.GetFromJsonAsync<IReadOnlyList<ProductResponse>>("/api/products");
        var productId = productsResponse![0].Id;

        var createResponse = await userClient.PostAsJsonAsync("/api/orders", new
        {
            items = new[]
            {
                new
                {
                    productId,
                    quantity = 1
                }
            }
        });

        var myOrdersResponse = await userClient.GetAsync("/api/orders/me");
        var myOrders = await myOrdersResponse.Content.ReadFromJsonAsync<IReadOnlyList<OrderResponse>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, myOrdersResponse.StatusCode);
        Assert.NotNull(myOrders);
        Assert.NotEmpty(myOrders);
    }

    [Fact]
    public async Task GetAllOrders_WhenUserRole_ReturnsForbidden()
    {
        using var userClient = factory.CreateAuthenticatedClient("user-orders-forbidden", "demo", "user");

        var response = await userClient.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllOrders_WhenAdminRole_ReturnsSuccess()
    {
        using var adminClient = factory.CreateAuthenticatedClient("admin-orders", "admin", "admin", "user");

        var response = await adminClient.GetAsync("/api/orders");
        var orders = await response.Content.ReadFromJsonAsync<IReadOnlyList<OrderResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(orders);
    }
}
