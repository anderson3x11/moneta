using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moneta.Application.Abstractions;
using Moneta.Infrastructure.FacturX;
using Moneta.Infrastructure.Persistence;
using Moneta.Infrastructure.Persistence.Repositories;
using Moneta.Infrastructure.Security;
using QuestPDF.Infrastructure;

namespace Moneta.Application.Tests;

/// <summary>
/// Spins up the application services over an isolated in-memory SQLite database,
/// wired with the real infrastructure implementations.
/// </summary>
public sealed class TestHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    public MonetaDbContext Db { get; }
    public IClientRepository Clients { get; }
    public IInvoiceRepository Invoices { get; }
    public ISellerProfileRepository Sellers { get; }
    public IUserRepository Users { get; }
    public IUnitOfWork UnitOfWork { get; }
    public IInvoiceNumberGenerator Numbering { get; }
    public IFacturXGenerator FacturX { get; }
    public IPasswordHasher PasswordHasher { get; }
    public IJwtTokenService Jwt { get; }

    static TestHarness()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public TestHarness()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MonetaDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new MonetaDbContext(options);
        Db.Database.EnsureCreated();

        Clients = new ClientRepository(Db);
        Invoices = new InvoiceRepository(Db);
        Sellers = new SellerProfileRepository(Db);
        Users = new UserRepository(Db);
        UnitOfWork = new UnitOfWork(Db);
        Numbering = new InvoiceNumberGenerator(Invoices);
        FacturX = new FacturXGenerator();
        PasswordHasher = new BcryptPasswordHasher();

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "MonetaTest",
            Audience = "MonetaTest",
            SigningKey = "test-signing-key-at-least-32-bytes-long-xx",
            LifetimeMinutes = 60
        });
        Jwt = new JwtTokenService(jwtOptions);
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
