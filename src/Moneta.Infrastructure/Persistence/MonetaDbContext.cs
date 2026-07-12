using Microsoft.EntityFrameworkCore;
using Moneta.Domain.Identity;
using Moneta.Domain.Invoicing;

namespace Moneta.Infrastructure.Persistence;

public sealed class MonetaDbContext(DbContextOptions<MonetaDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MonetaDbContext).Assembly);
    }
}
