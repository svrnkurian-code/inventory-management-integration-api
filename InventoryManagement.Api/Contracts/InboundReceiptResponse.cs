namespace InventoryManagement.Api.Contracts;

public class InboundReceiptResponse
{
    public int Id { get; set; }

    public string PartnerCode { get; set; } = string.Empty;

    public string ReceiptReference { get; set; } = string.Empty;

    public DateTime ReceivedAtUtc { get; set; }

    public int ProcessedLineCount { get; set; }
}