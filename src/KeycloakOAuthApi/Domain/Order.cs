namespace KeycloakOAuthApi.Domain;

public sealed class Order
{
    public Order(
        Guid id,
        string customerId,
        string customerUsername,
        IReadOnlyCollection<OrderLine> lines,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(customerUsername))
        {
            throw new ArgumentException("Customer username is required.", nameof(customerUsername));
        }

        if (lines.Count == 0)
        {
            throw new ArgumentException("Order must include at least one line.", nameof(lines));
        }

        Id = id;
        CustomerId = customerId;
        CustomerUsername = customerUsername;
        Lines = lines;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }
    public string CustomerId { get; }
    public string CustomerUsername { get; }
    public IReadOnlyCollection<OrderLine> Lines { get; }
    public DateTime CreatedAtUtc { get; }
    public decimal TotalAmount => Lines.Sum(line => line.LineTotal);
}
