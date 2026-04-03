namespace KeycloakOAuthApi.Domain;

public sealed class Product
{
    public Product(
        Guid id,
        string name,
        string description,
        decimal price,
        int stockQuantity,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than zero.");
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock quantity cannot be negative.");
        }

        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public decimal Price { get; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void SetStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Stock quantity cannot be negative.");
        }

        StockQuantity = quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool TryReserve(int quantity)
    {
        if (quantity <= 0 || StockQuantity < quantity)
        {
            return false;
        }

        StockQuantity -= quantity;
        UpdatedAtUtc = DateTime.UtcNow;
        return true;
    }
}
