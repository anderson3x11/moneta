using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moneta.Application.Abstractions;
using Moneta.Infrastructure.FacturX;
using Moneta.Infrastructure.Persistence;
using Moneta.Infrastructure.Persistence.Repositories;
using Moneta.Infrastructure.Security;
using QuestPDF.Infrastructure;

namespace Moneta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // QuestPDF is used under its Community licence.
        QuestPDF.Settings.License = LicenseType.Community;

        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=moneta.db";
        services.AddDbContext<MonetaDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ISellerProfileRepository, SellerProfileRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IFacturXGenerator, FacturXGenerator>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        return services;
    }
}
