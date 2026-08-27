using InventoryManagement.Api.Models;

namespace InventoryManagement.Api.Services;

public enum InboundReceiptFailure
{
    None,
    DuplicateReceipt,
    ProductNotFound
}

public class InboundReceiptResult
{
    public InboundReceipt? Receipt { get; init; }

    public InboundReceiptFailure Failure { get; init; }

    public int? ProductId { get; init; }

    public bool Succeeded => Failure == InboundReceiptFailure.None;
}