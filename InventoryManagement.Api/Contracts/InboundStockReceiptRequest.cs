using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Api.Contracts;

public class InboundStockReceiptRequest
{
    [Required]
    [StringLength(50)]
    public string PartnerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ReceiptReference { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<InboundStockReceiptLine> Lines { get; set; } = [];
}

public class InboundStockReceiptLine
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantityReceived { get; set; }
}