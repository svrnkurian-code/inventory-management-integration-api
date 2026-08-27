using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Data;
using InventoryManagement.Api.Models;
using InventoryManagement.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagement.Api.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task ProcessInboundReceiptAsync_DuplicateReceipt_DoesNotIncreaseStockTwice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new InventoryDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var product = new Product
        {
            Name = "Test Monitor",
            Price = 12000,
            QuantityInStock = 5
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        var request = new InboundStockReceiptRequest
        {
            PartnerCode = "WAREHOUSE-A",
            ReceiptReference = "TEST-RECEIPT-001",
            Lines =
            [
                new InboundStockReceiptLine
                {
                    ProductId = product.Id,
                    QuantityReceived = 3
                }
            ]
        };

        var result = await service.ProcessInboundReceiptAsync(request);
        var duplicateResult = await service.ProcessInboundReceiptAsync(request);

        var savedProduct = await context.Products.SingleAsync();

        Assert.True(result.Succeeded);

        Assert.False(duplicateResult.Succeeded);
        Assert.Equal(
            InboundReceiptFailure.DuplicateReceipt,
            duplicateResult.Failure);

        Assert.Equal(8, savedProduct.QuantityInStock);
        Assert.Equal(1, await context.InboundReceipts.CountAsync());
        Assert.Equal(1, await context.StockAdjustments.CountAsync());
    }

    [Fact]
    public async Task ProcessInboundReceiptAsync_UnknownProduct_DoesNotChangeStock()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new InventoryDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var product = new Product
        {
            Name = "Test Monitor",
            Price = 12000,
            QuantityInStock = 5
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        var request = new InboundStockReceiptRequest
        {
            PartnerCode = "WAREHOUSE-A",
            ReceiptReference = "TEST-INVALID-001",
            Lines =
            [
                new InboundStockReceiptLine
            {
                ProductId = product.Id,
                QuantityReceived = 3
            },
            new InboundStockReceiptLine
            {
                ProductId = 999,
                QuantityReceived = 2
            }
            ]
        };

        var result = await service.ProcessInboundReceiptAsync(request);

        var savedProduct = await context.Products.SingleAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(InboundReceiptFailure.ProductNotFound, result.Failure);
        Assert.Equal(999, result.ProductId);

        Assert.Equal(5, savedProduct.QuantityInStock);
        Assert.Equal(0, await context.InboundReceipts.CountAsync());
        Assert.Equal(0, await context.StockAdjustments.CountAsync());
    }
}