using System.Net;
using System.Net.Http.Json;
using KeycloakOAuthApi.Contracts.Products;
using KeycloakOAuthApi.Tests.Integration.Infrastructure;

namespace KeycloakOAuthApi.Tests.Integration;

public sealed class ProductsEndpointsTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task GetProducts_WhenUnauthenticated_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WhenAuthenticated_ReturnsSeededProducts()
    {
        using var client = factory.CreateAuthenticatedClient("user-products", "demo", "user");

        var response = await client.GetAsync("/api/products");
        var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task CreateProduct_WhenUserRole_ReturnsForbidden()
    {
        using var client = factory.CreateAuthenticatedClient("user-products-create", "demo", "user");

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            name = "User Product",
            description = "Should not be created",
            price = 49.90m,
            stockQuantity = 3
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WhenAdminRole_ReturnsCreated()
    {
        using var client = factory.CreateAuthenticatedClient("admin-products-create", "admin", "admin", "user");

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            name = $"Admin Product {Guid.NewGuid():N}",
            description = "Created by integration test",
            price = 79.90m,
            stockQuantity = 6
        });
        var responseBody = await response.Content.ReadAsStringAsync();
        var createdProduct = response.StatusCode == HttpStatusCode.Created
            ? await response.Content.ReadFromJsonAsync<ProductResponse>()
            : null;

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 but got {(int)response.StatusCode}. Body: {responseBody}");
        Assert.NotNull(createdProduct);
        Assert.True(createdProduct.Id != Guid.Empty);
    }
}
