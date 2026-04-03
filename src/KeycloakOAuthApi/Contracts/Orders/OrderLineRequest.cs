using System.Text.Json.Serialization;

namespace KeycloakOAuthApi.Contracts.Orders;

public readonly struct OrderLineRequest
{
    [JsonConstructor]
    public OrderLineRequest(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid ProductId { get; }
    public int Quantity { get; }
}
