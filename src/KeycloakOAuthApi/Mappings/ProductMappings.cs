using KeycloakOAuthApi.Contracts.Products;
using KeycloakOAuthApi.Domain;

namespace KeycloakOAuthApi.Mappings;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc
        };
}
