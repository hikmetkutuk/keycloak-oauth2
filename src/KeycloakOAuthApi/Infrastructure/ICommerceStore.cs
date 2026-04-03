using KeycloakOAuthApi.Domain;

namespace KeycloakOAuthApi.Infrastructure;

public interface ICommerceStore
{
    public IReadOnlyCollection<Product> GetProducts();
    public Product? GetProduct(Guid id);
    public Product CreateProduct(string name, string description, decimal price, int stockQuantity);
    public bool TrySetStock(Guid productId, int stockQuantity, out Product? product);

    public OrderPlacementResult PlaceOrder(
        string customerId,
        string customerUsername,
        IReadOnlyCollection<OrderLineInput> requestedItems);

    public IReadOnlyCollection<Order> GetOrders();
    public IReadOnlyCollection<Order> GetOrdersForCustomer(string customerId);
    public Order? GetOrder(Guid orderId);
    public Order? GetOrderForCustomer(Guid orderId, string customerId);
}

public sealed record OrderLineInput(Guid ProductId, int Quantity);

public sealed class OrderPlacementResult
{
    private OrderPlacementResult(bool succeeded, Order? order, string? error)
    {
        Succeeded = succeeded;
        Order = order;
        Error = error;
    }

    public bool Succeeded { get; }
    public Order? Order { get; }
    public string? Error { get; }

    public static OrderPlacementResult Success(Order order) => new(true, order, null);
    public static OrderPlacementResult Failed(string error) => new(false, null, error);
}
