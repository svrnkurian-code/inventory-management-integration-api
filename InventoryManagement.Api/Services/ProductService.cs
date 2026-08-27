using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Data;
using InventoryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Api.Services;

public class ProductService : IProductService
{
    private readonly InventoryDbContext _context;

    public ProductService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            QuantityInStock = request.QuantityInStock
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return false;
        }

        product.Name = request.Name;
        product.Price = request.Price;
        product.QuantityInStock = request.QuantityInStock;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<StockAdjustmentResult> CreateStockAdjustmentAsync(
    int productId,
    CreateStockAdjustmentRequest request)
    {
        var referenceAlreadyUsed = await _context.StockAdjustments.AnyAsync(
    adjustment => adjustment.ExternalReference == request.ExternalReference);

        if (referenceAlreadyUsed)
        {
            return new StockAdjustmentResult
            {
                Failure = StockAdjustmentFailure.DuplicateExternalReference
            };
        }

        var product = await _context.Products.FindAsync(productId);

        if (product is null)
        {
            return new StockAdjustmentResult
            {
                Failure = StockAdjustmentFailure.ProductNotFound
            };
        }

        var invalidQuantity =
            request.QuantityChange == 0 ||
            (request.Type == StockAdjustmentType.Received &&
             request.QuantityChange < 0) ||
            (request.Type == StockAdjustmentType.Issued &&
             request.QuantityChange > 0);

        if (invalidQuantity)
        {
            return new StockAdjustmentResult
            {
                Failure = StockAdjustmentFailure.InvalidQuantity
            };
        }

        if (product.QuantityInStock + request.QuantityChange < 0)
        {
            return new StockAdjustmentResult
            {
                Failure = StockAdjustmentFailure.InsufficientStock
            };
        }

        var adjustment = new StockAdjustment
        {
            ProductId = productId,
            Type = request.Type,
            QuantityChange = request.QuantityChange,
            Reason = request.Reason,
            CreatedAtUtc = DateTime.UtcNow,
            ExternalReference = request.ExternalReference
        };

        product.QuantityInStock += request.QuantityChange;

        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();

        return new StockAdjustmentResult
        {
            Adjustment = adjustment,
            Failure = StockAdjustmentFailure.None
        };
    }

    public async Task<InboundReceiptResult> ProcessInboundReceiptAsync(
    InboundStockReceiptRequest request)
    {
        var receiptAlreadyProcessed = await _context.InboundReceipts.AnyAsync(
            receipt => receipt.PartnerCode == request.PartnerCode &&
                       receipt.ReceiptReference == request.ReceiptReference);

        if (receiptAlreadyProcessed)
        {
            return new InboundReceiptResult
            {
                Failure = InboundReceiptFailure.DuplicateReceipt
            };
        }

        var productIds = request.Lines
            .Select(line => line.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id);

        var missingProductId = productIds.FirstOrDefault(
            productId => !products.ContainsKey(productId));

        if (missingProductId != 0)
        {
            return new InboundReceiptResult
            {
                Failure = InboundReceiptFailure.ProductNotFound,
                ProductId = missingProductId
            };
        }

        var receipt = new InboundReceipt
        {
            PartnerCode = request.PartnerCode,
            ReceiptReference = request.ReceiptReference,
            ReceivedAtUtc = DateTime.UtcNow
        };

        _context.InboundReceipts.Add(receipt);

        foreach (var line in request.Lines)
        {
            var product = products[line.ProductId];

            product.QuantityInStock += line.QuantityReceived;

            var adjustment = new StockAdjustment
            {
                ProductId = product.Id,
                InboundReceipt = receipt,
                Type = StockAdjustmentType.Received,
                QuantityChange = line.QuantityReceived,
                Reason = $"Inbound receipt {request.PartnerCode}/{request.ReceiptReference}"
            };

            _context.StockAdjustments.Add(adjustment);
        }

        await _context.SaveChangesAsync();

        return new InboundReceiptResult
        {
            Receipt = receipt,
            Failure = InboundReceiptFailure.None
        };
    }
}