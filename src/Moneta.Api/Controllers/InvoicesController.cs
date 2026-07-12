using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moneta.Application.Contracts;
using Moneta.Application.Invoices;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public sealed class InvoicesController(InvoiceService invoices) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceSummaryResponse>>> List(CancellationToken ct) =>
        Ok(await invoices.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceResponse>> Get(Guid id, CancellationToken ct) =>
        Ok(await invoices.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> Create(CreateInvoiceRequest request, CancellationToken ct)
    {
        var invoice = await invoices.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id:guid}/issue")]
    public async Task<ActionResult<InvoiceResponse>> Issue(Guid id, CancellationToken ct) =>
        Ok(await invoices.IssueAsync(id, ct));

    [HttpPost("{id:guid}/pay")]
    public async Task<ActionResult<InvoiceResponse>> Pay(Guid id, CancellationToken ct) =>
        Ok(await invoices.MarkPaidAsync(id, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<InvoiceResponse>> Cancel(Guid id, CancellationToken ct) =>
        Ok(await invoices.CancelAsync(id, ct));

    [HttpGet("{id:guid}/facturx")]
    public async Task<IActionResult> FacturX(Guid id, CancellationToken ct)
    {
        var document = await invoices.GenerateFacturXAsync(id, ct);
        return File(document.Pdf, "application/pdf", document.FileName);
    }
}
