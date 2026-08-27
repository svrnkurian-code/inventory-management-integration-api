using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controllers;

[ApiController]
[Route("api/inbound-receipts")]
public class InboundReceiptsController : ControllerBase
{
    private readonly IProductService _productService;

    public InboundReceiptsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<ActionResult<InboundReceiptResponse>> Receive(
        InboundStockReceiptRequest request)
    {
        var result = await _productService.ProcessInboundReceiptAsync(request);

        if (!result.Succeeded)
        {
            return result.Failure switch
            {
                InboundReceiptFailure.DuplicateReceipt => Conflict(new
                {
                    message = "This partner receipt has already been processed."
                }),

                InboundReceiptFailure.ProductNotFound => BadRequest(new
                {
                    message = $"Product with ID {result.ProductId} was not found."
                }),

                _ => BadRequest()
            };
        }

        var response = new InboundReceiptResponse
        {
            Id = result.Receipt!.Id,
            PartnerCode = result.Receipt.PartnerCode,
            ReceiptReference = result.Receipt.ReceiptReference,
            ReceivedAtUtc = result.Receipt.ReceivedAtUtc,
            ProcessedLineCount = request.Lines.Count
        };

        return Created($"/api/inbound-receipts/{response.Id}", response);
    }
}