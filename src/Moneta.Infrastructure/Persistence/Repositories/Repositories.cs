using Microsoft.EntityFrameworkCore;
using Moneta.Application.Abstractions;
using Moneta.Domain.Identity;
using Moneta.Domain.Invoicing;

namespace Moneta.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository(MonetaDbContext db) : IClientRepository
{
    public Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Client>> ListAsync(CancellationToken ct = default) =>
        await db.Clients.OrderBy(c => c.Name).ToListAsync(ct);

    public async Task AddAsync(Client client, CancellationToken ct = default) =>
        await db.Clients.AddAsync(client, ct);

    public void Remove(Client client) => db.Clients.Remove(client);
}

public sealed class InvoiceRepository(MonetaDbContext db) : IInvoiceRepository
{
    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken ct = default) =>
        await db.Invoices.OrderByDescending(i => i.Number).ToListAsync(ct);

    public async Task AddAsync(Invoice invoice, CancellationToken ct = default) =>
        await db.Invoices.AddAsync(invoice, ct);

    public Task<int> CountForYearAsync(int year, CancellationToken ct = default) =>
        db.Invoices.CountAsync(i => i.IssueDate.Year == year, ct);
}

public sealed class SellerProfileRepository(MonetaDbContext db) : ISellerProfileRepository
{
    public Task<SellerProfile?> GetAsync(CancellationToken ct = default) =>
        db.SellerProfiles.FirstOrDefaultAsync(ct);
}

public sealed class UserRepository(MonetaDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExistsAsync(string email, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);
}

public sealed class UnitOfWork(MonetaDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
