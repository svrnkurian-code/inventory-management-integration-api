using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Api.Models;

public class StockAdjustment
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int? InboundReceiptId { get; set; }

    public InboundReceipt? InboundReceipt { get; set; }

    public int QuantityChange { get; set; }

    public StockAdjustmentType Type { get; set; }

    [StringLength(200)]
    public string? Reason { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}