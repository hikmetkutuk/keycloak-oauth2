using KeycloakOAuthApi.Contracts.Products;
using KeycloakOAuthApi.Infrastructure;
using KeycloakOAuthApi.Mappings;
using KeycloakOAuthApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakOAuthApi.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController(ICommerceStore commerceStore) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<ProductResponse>> GetAll()
    {
        var products = commerceStore.GetProducts()
            .Select(product => product.ToResponse())
            .ToArray();

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductResponse> GetById(Guid id)
    {
        var product = commerceStore.GetProduct(id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product.ToResponse());
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public ActionResult<ProductResponse> Create([FromBody] CreateProductRequest request)
    {
        var product = commerceStore.CreateProduct(
            request.Name,
            request.Description ?? string.Empty,
            request.Price,
            request.StockQuantity);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product.ToResponse());
    }

    [HttpPatch("{id:guid}/stock")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductResponse> SetStock(Guid id, [FromBody] UpdateProductStockRequest request)
    {
        if (!commerceStore.TrySetStock(id, request.StockQuantity, out var product))
        {
            return NotFound();
        }

        return Ok(product!.ToResponse());
    }
}
