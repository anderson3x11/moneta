using Moneta.Domain.Invoicing;

namespace Moneta.Application.Abstractions;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Client>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Client client, CancellationToken ct = default);
    void Remove(Client client);
}

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    Task<int> CountForYearAsync(int year, CancellationToken ct = default);
}

public interface ISellerProfileRepository
{
    Task<SellerProfile?> GetAsync(CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
