using System.Diagnostics.CodeAnalysis;

namespace KeycloakOAuthApi.Contracts.Orders;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Serialized response contract.")]
public sealed class OrderResponse
{
    public Guid Id { get; init; }
    public string CustomerUsername { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public IReadOnlyCollection<OrderLineResponse> Lines { get; init; } = [];
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Serialized response contract.")]
public sealed class OrderLineResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}
