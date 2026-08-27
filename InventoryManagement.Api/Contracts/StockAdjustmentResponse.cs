using InventoryManagement.Api.Models;

namespace InventoryManagement.Api.Contracts;

public class StockAdjustmentResponse
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public StockAdjustmentType Type { get; set; }

    public int QuantityChange { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}