using Moneta.Application.Abstractions;
using Moneta.Application.Common;
using Moneta.Application.Contracts;
using Moneta.Domain.Invoicing;

namespace Moneta.Application.Clients;

public sealed class ClientService(IClientRepository clients, IUnitOfWork uow)
{
    public async Task<IReadOnlyList<ClientResponse>> ListAsync(CancellationToken ct = default)
    {
        var items = await clients.ListAsync(ct);
        return items.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ClientResponse> GetAsync(Guid id, CancellationToken ct = default)
    {
        var client = await clients.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Client {id} was not found.");
        return client.ToResponse();
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        var client = new Client(
            request.Name,
            request.Email,
            request.Address.ToDomain(),
            request.Siret,
            request.VatNumber);

        await clients.AddAsync(client, ct);
        await uow.SaveChangesAsync(ct);
        return client.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var client = await clients.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Client {id} was not found.");
        clients.Remove(client);
        await uow.SaveChangesAsync(ct);
    }
}
