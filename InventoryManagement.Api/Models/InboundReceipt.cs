using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Api.Models;

public class InboundReceipt
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string PartnerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ReceiptReference { get; set; } = string.Empty;

    public DateTime ReceivedAtUtc { get; set; }

    public List<StockAdjustment> StockAdjustments { get; set; } = [];
}