using KeycloakOAuthApi.Contracts.Orders;
using KeycloakOAuthApi.Domain;

namespace KeycloakOAuthApi.Mappings;

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) =>
        new()
        {
            Id = order.Id,
            CustomerUsername = order.CustomerUsername,
            TotalAmount = order.TotalAmount,
            CreatedAtUtc = order.CreatedAtUtc,
            Lines = order.Lines.Select(ToResponse).ToArray()
        };

    private static OrderLineResponse ToResponse(this OrderLine line) =>
        new()
        {
            ProductId = line.ProductId,
            ProductName = line.ProductName,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            LineTotal = line.LineTotal
        };
}
