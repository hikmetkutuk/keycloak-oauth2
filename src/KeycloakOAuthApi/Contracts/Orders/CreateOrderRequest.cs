using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KeycloakOAuthApi.Contracts.Orders;

public sealed class CreateOrderRequest
{
    [JsonConstructor]
    public CreateOrderRequest(IReadOnlyList<OrderLineRequest>? items)
    {
        Items = items ?? [];
    }

    [Required]
    [MinLength(1)]
    public IReadOnlyList<OrderLineRequest> Items { get; }
}
