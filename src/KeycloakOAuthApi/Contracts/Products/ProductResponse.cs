using System.Diagnostics.CodeAnalysis;

namespace KeycloakOAuthApi.Contracts.Products;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Serialized response contract.")]
public sealed class ProductResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
