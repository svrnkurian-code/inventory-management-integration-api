using InventoryManagement.Api.Models;

namespace InventoryManagement.Api.Services;

public enum StockAdjustmentFailure
{
    None,
    ProductNotFound,
    InvalidQuantity,
    InsufficientStock,
    DuplicateExternalReference
}

public class StockAdjustmentResult
{
    public StockAdjustment? Adjustment { get; init; }

    public StockAdjustmentFailure Failure { get; init; }

    public bool Succeeded => Failure == StockAdjustmentFailure.None;
}