using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moneta.Application.Auth;
using Moneta.Application.Clients;
using Moneta.Application.Invoices;

namespace Moneta.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ClientService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<AuthService>();

        services.AddValidatorsFromAssemblyContaining<ClientService>(ServiceLifetime.Scoped);

        return services;
    }
}
