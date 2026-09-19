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
    public async Task GetAllAsync_ReturnsTheRequestedPage()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new InventoryDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Products.AddRange(
            new Product { Name = "Product 1", Price = 100, QuantityInStock = 1 },
            new Product { Name = "Product 2", Price = 200, QuantityInStock = 2 },
            new Product { Name = "Product 3", Price = 300, QuantityInStock = 3 },
            new Product { Name = "Product 4", Price = 400, QuantityInStock = 4 },
            new Product { Name = "Product 5", Price = 500, QuantityInStock = 5 });

        await context.SaveChangesAsync();

        var service = new ProductService(context);

        var result = await service.GetAllAsync(    pageNumber: 2,    pageSize: 2,    searchTerm: null);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);

        Assert.Equal("Product 3", result.Items[0].Name);
        Assert.Equal("Product 4", result.Items[1].Name);
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

    [Fact]
    public async Task GetAllAsync_SearchTerm_ReturnsMatchingProductsOnly()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new InventoryDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Products.AddRange(
            new Product { Name = "Keyboard", Price = 1500, QuantityInStock = 10 },
            new Product { Name = "Mouse", Price = 800, QuantityInStock = 25 },
            new Product { Name = "Wireless Mouse", Price = 1200, QuantityInStock = 15 });

        await context.SaveChangesAsync();

        var service = new ProductService(context);

        var result = await service.GetAllAsync(
            pageNumber: 1,
            pageSize: 10,
            searchTerm: "Mouse");

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, product => Assert.Contains("Mouse", product.Name));
    }
}