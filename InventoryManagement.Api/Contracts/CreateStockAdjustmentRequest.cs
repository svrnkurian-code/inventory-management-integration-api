using System.ComponentModel.DataAnnotations;
using InventoryManagement.Api.Models;

namespace InventoryManagement.Api.Contracts;

public class CreateStockAdjustmentRequest
{
    [EnumDataType(typeof(StockAdjustmentType))]
    public StockAdjustmentType Type { get; set; }

    public int QuantityChange { get; set; }

    [Required]
    [StringLength(200)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ExternalReference { get; set; } = string.Empty;
}