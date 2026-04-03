using System.Security.Claims;
using KeycloakOAuthApi.Contracts.Orders;
using KeycloakOAuthApi.Infrastructure;
using KeycloakOAuthApi.Mappings;
using KeycloakOAuthApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakOAuthApi.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(ICommerceStore commerceStore) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<OrderResponse> Create([FromBody] CreateOrderRequest request)
    {
        if (request.Items.Any(item => item.ProductId == Guid.Empty || item.Quantity <= 0))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["items"] = ["Each item must contain a valid product id and quantity greater than zero."]
            }));
        }

        var customerId = User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Unauthorized();
        }

        var customerUsername = User.Identity?.Name
            ?? User.FindFirstValue("preferred_username")
            ?? customerId;

        var result = commerceStore.PlaceOrder(
            customerId,
            customerUsername,
            request.Items.Select(item => new OrderLineInput(item.ProductId, item.Quantity)).ToArray());

        if (!result.Succeeded || result.Order is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Order creation failed",
                Detail = result.Error
            });
        }

        return CreatedAtAction(nameof(GetMineById), new { id = result.Order.Id }, result.Order.ToResponse());
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrderResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<OrderResponse>> GetMine()
    {
        var customerId = User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Unauthorized();
        }

        var orders = commerceStore.GetOrdersForCustomer(customerId)
            .Select(order => order.ToResponse())
            .ToArray();

        return Ok(orders);
    }

    [HttpGet("me/{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OrderResponse> GetMineById(Guid id)
    {
        var customerId = User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Unauthorized();
        }

        var order = commerceStore.GetOrderForCustomer(id, customerId);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order.ToResponse());
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrderResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<OrderResponse>> GetAll()
    {
        var orders = commerceStore.GetOrders()
            .Select(order => order.ToResponse())
            .ToArray();

        return Ok(orders);
    }
}
