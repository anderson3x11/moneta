using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Moneta.Infrastructure.Persistence;

/// <summary>Lets the EF Core tools build the context when adding migrations.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MonetaDbContext>
{
    public MonetaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MonetaDbContext>()
            .UseSqlite("Data Source=moneta.db")
            .Options;

        return new MonetaDbContext(options);
    }
}
