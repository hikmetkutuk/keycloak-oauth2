using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace KeycloakOAuthApi.Contracts.Products;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Model binding contract.")]
public sealed class CreateProductRequest
{
    [Required(AllowEmptyStrings = false)]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Price { get; set; }

    [Range(0, 1000000)]
    public int StockQuantity { get; set; }
}
