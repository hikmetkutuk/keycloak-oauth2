using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KeycloakOAuthApi.Contracts.Products;

public sealed class CreateProductRequest
{
    [JsonConstructor]
    public CreateProductRequest(string? name, string? description, decimal price, int stockQuantity)
    {
        Name = name ?? string.Empty;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
    }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(150)]
    public string Name { get; }

    [MaxLength(500)]
    public string? Description { get; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; }

    [Range(0, 1000000)]
    public int StockQuantity { get; }
}
