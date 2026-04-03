using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KeycloakOAuthApi.Contracts.Products;

public sealed class UpdateProductStockRequest
{
    [JsonConstructor]
    public UpdateProductStockRequest(int stockQuantity)
    {
        StockQuantity = stockQuantity;
    }

    [Range(0, 1000000)]
    public int StockQuantity { get; }
}
