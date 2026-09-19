using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Models;
using InventoryManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Api.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetAll(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchTerm = null)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(
                "pageNumber must be at least 1 and pageSize must be between 1 and 100.");
        }

        var productsPage = await _productService.GetAllAsync(pageNumber, pageSize,searchTerm);

        return Ok(new PagedResult<ProductResponse>
        {
            Items = productsPage.Items.Select(ToResponse).ToList(),
            PageNumber = productsPage.PageNumber,
            PageSize = productsPage.PageSize,
            TotalCount = productsPage.TotalCount,
            TotalPages = productsPage.TotalPages
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(product));
    }

    [Authorize(Roles = Roles.InventoryManager)]
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        var response = ToResponse(product);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [Authorize(Roles = Roles.InventoryManager)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var updated = await _productService.UpdateAsync(id, request);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [Authorize(Roles = Roles.InventoryManager)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [Authorize(Roles = Roles.InventoryManager)]
    [HttpPost("{id}/stock-adjustments")]
    public async Task<ActionResult<StockAdjustmentResponse>> CreateStockAdjustment(
    int id,
    CreateStockAdjustmentRequest request)
    {
        var result = await _productService.CreateStockAdjustmentAsync(id, request);

        if (!result.Succeeded)
        {
            return result.Failure switch
            {
                StockAdjustmentFailure.ProductNotFound => NotFound(),

                StockAdjustmentFailure.InvalidQuantity => BadRequest(new
                {
                    message = "The quantity change is not valid for this adjustment type."
                }),

                StockAdjustmentFailure.InsufficientStock => BadRequest(new
                {
                    message = "Stock cannot be reduced below zero."
                }),

                StockAdjustmentFailure.DuplicateExternalReference => Conflict(new
                {
                    message = "An adjustment with this external reference has already been processed."
                }),

                _ => BadRequest()
            };
        }

        var response = new StockAdjustmentResponse
        {
            Id = result.Adjustment!.Id,
            ProductId = result.Adjustment.ProductId,
            Type = result.Adjustment.Type,
            QuantityChange = result.Adjustment.QuantityChange,
            Reason = result.Adjustment.Reason,
            CreatedAtUtc = result.Adjustment.CreatedAtUtc
        };

        return Created(
            $"/api/products/{id}/stock-adjustments/{response.Id}",
            response);
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            QuantityInStock = product.QuantityInStock
        };
    }
}