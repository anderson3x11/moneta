using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moneta.Application.Clients;
using Moneta.Application.Contracts;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public sealed class ClientsController(ClientService clients) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> List(CancellationToken ct) =>
        Ok(await clients.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientResponse>> Get(Guid id, CancellationToken ct) =>
        Ok(await clients.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ClientResponse>> Create(CreateClientRequest request, CancellationToken ct)
    {
        var client = await clients.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await clients.DeleteAsync(id, ct);
        return NoContent();
    }
}
