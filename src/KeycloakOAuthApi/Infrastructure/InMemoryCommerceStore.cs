using KeycloakOAuthApi.Domain;

namespace KeycloakOAuthApi.Infrastructure;

public sealed class InMemoryCommerceStore : ICommerceStore
{
    private readonly Dictionary<Guid, Product> _products = new();
    private readonly Dictionary<Guid, Order> _orders = new();
    private readonly Lock _sync = new();

    public InMemoryCommerceStore()
    {
        SeedProducts();
    }

    public IReadOnlyCollection<Product> GetProducts()
    {
        lock (_sync)
        {
            return _products.Values
                .OrderBy(product => product.Name)
                .ToArray();
        }
    }

    public Product? GetProduct(Guid id)
    {
        lock (_sync)
        {
            return _products.GetValueOrDefault(id);
        }
    }

    public Product CreateProduct(string name, string description, decimal price, int stockQuantity)
    {
        var now = DateTime.UtcNow;
        var product = new Product(
            Guid.NewGuid(),
            name.Trim(),
            description.Trim(),
            price,
            stockQuantity,
            now);

        lock (_sync)
        {
            _products[product.Id] = product;
        }

        return product;
    }

    public bool TrySetStock(Guid productId, int stockQuantity, out Product? product)
    {
        lock (_sync)
        {
            if (!_products.TryGetValue(productId, out product))
            {
                return false;
            }

            product.SetStock(stockQuantity);
            return true;
        }
    }

    public OrderPlacementResult PlaceOrder(
        string customerId,
        string customerUsername,
        IReadOnlyCollection<OrderLineInput> requestedItems)
    {
        if (requestedItems.Count == 0)
        {
            return OrderPlacementResult.Failed("Order must include at least one item.");
        }

        var normalizedItems = requestedItems
            .GroupBy(item => item.ProductId)
            .Select(group => new OrderLineInput(group.Key, group.Sum(item => item.Quantity)))
            .ToArray();

        if (normalizedItems.Any(item => item.ProductId == Guid.Empty || item.Quantity <= 0))
        {
            return OrderPlacementResult.Failed("Each order item must have a valid product id and quantity.");
        }

        lock (_sync)
        {
            var productsToReserve = new List<(Product Product, int Quantity)>(normalizedItems.Length);

            foreach (var item in normalizedItems)
            {
                if (!_products.TryGetValue(item.ProductId, out var product))
                {
                    return OrderPlacementResult.Failed($"Product '{item.ProductId}' was not found.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    return OrderPlacementResult.Failed(
                        $"Insufficient stock for '{product.Name}'. Requested {item.Quantity}, available {product.StockQuantity}.");
                }

                productsToReserve.Add((product, item.Quantity));
            }

            foreach (var item in productsToReserve)
            {
                item.Product.TryReserve(item.Quantity);
            }

            var lines = productsToReserve
                .Select(item => new OrderLine(
                    item.Product.Id,
                    item.Product.Name,
                    item.Quantity,
                    item.Product.Price))
                .ToArray();

            var order = new Order(Guid.NewGuid(), customerId, customerUsername, lines, DateTime.UtcNow);
            _orders[order.Id] = order;

            return OrderPlacementResult.Success(order);
        }
    }

    public IReadOnlyCollection<Order> GetOrders()
    {
        lock (_sync)
        {
            return _orders.Values
                .OrderByDescending(order => order.CreatedAtUtc)
                .ToArray();
        }
    }

    public IReadOnlyCollection<Order> GetOrdersForCustomer(string customerId)
    {
        lock (_sync)
        {
            return _orders.Values
                .Where(order => order.CustomerId == customerId)
                .OrderByDescending(order => order.CreatedAtUtc)
                .ToArray();
        }
    }

    public Order? GetOrder(Guid orderId)
    {
        lock (_sync)
        {
            return _orders.GetValueOrDefault(orderId);
        }
    }

    public Order? GetOrderForCustomer(Guid orderId, string customerId)
    {
        lock (_sync)
        {
            if (!_orders.TryGetValue(orderId, out var order))
            {
                return null;
            }

            return order.CustomerId == customerId ? order : null;
        }
    }

    private void SeedProducts()
    {
        var seededProducts = new[]
        {
            new Product(Guid.NewGuid(), "Mechanical Keyboard", "Compact 75% wireless keyboard.", 149.90m, 40, DateTime.UtcNow),
            new Product(Guid.NewGuid(), "Noise-Cancelling Headphones", "Over-ear Bluetooth headphones.", 219.00m, 28, DateTime.UtcNow),
            new Product(Guid.NewGuid(), "4K Monitor", "27-inch IPS display for development.", 399.00m, 15, DateTime.UtcNow)
        };

        foreach (var product in seededProducts)
        {
            _products[product.Id] = product;
        }
    }
}
