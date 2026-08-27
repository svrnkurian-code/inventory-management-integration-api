using InventoryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Api.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    public DbSet<StockAdjustment> StockAdjustments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockAdjustment>()
            .HasIndex(adjustment => adjustment.ExternalReference)
            .IsUnique();

        modelBuilder.Entity<InboundReceipt>()
    .HasIndex(receipt => new
    {
        receipt.PartnerCode,
        receipt.ReceiptReference
    })
    .IsUnique();

        modelBuilder.Entity<InboundReceipt>()
            .HasMany(receipt => receipt.StockAdjustments)
            .WithOne(adjustment => adjustment.InboundReceipt)
            .HasForeignKey(adjustment => adjustment.InboundReceiptId);
    }

    public DbSet<InboundReceipt> InboundReceipts { get; set; }
}