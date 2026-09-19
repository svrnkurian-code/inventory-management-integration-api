using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Models;

namespace InventoryManagement.Api.Services;

public interface IProductService
{
    Task<PagedResult<Product>> GetAllAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm);

    Task<Product?> GetByIdAsync(int id);

    Task<Product> CreateAsync(CreateProductRequest request);

    Task<bool> UpdateAsync(int id, UpdateProductRequest request);

    Task<bool> DeleteAsync(int id);

    Task<StockAdjustmentResult> CreateStockAdjustmentAsync(
    int productId,
    CreateStockAdjustmentRequest request);

    Task<InboundReceiptResult> ProcessInboundReceiptAsync(
    InboundStockReceiptRequest request);
}