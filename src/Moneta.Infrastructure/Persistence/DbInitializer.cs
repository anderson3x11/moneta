using Microsoft.EntityFrameworkCore;
using Moneta.Application.Abstractions;
using Moneta.Domain.Identity;
using Moneta.Domain.Invoicing;
using Moneta.Domain.ValueObjects;

namespace Moneta.Infrastructure.Persistence;

/// <summary>
/// Creates the database and seeds a demo dataset so the app is explorable out
/// of the box. Demo credentials are documented in the README.
/// </summary>
public static class DbInitializer
{
    public const string DemoEmail = "demo@moneta.fr";
    public const string DemoPassword = "Password123!";

    public static async Task SeedAsync(MonetaDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (!await db.Users.AnyAsync(ct))
            db.Users.Add(new User(DemoEmail, hasher.Hash(DemoPassword)));

        if (!await db.SellerProfiles.AnyAsync(ct))
        {
            db.SellerProfiles.Add(new SellerProfile(
                legalName: "Studio Moneta SAS",
                siret: "73282932000074",
                address: new Address("12 rue de la Facture", "75002", "Paris"),
                contactEmail: "contact@moneta.fr",
                vatNumber: "FR40732829320",
                iban: "FR7630006000011234567890189"));
        }

        if (!await db.Clients.AnyAsync(ct))
        {
            var client = new Client(
                name: "Boulangerie Léon",
                email: "compta@boulangerie-leon.fr",
                address: new Address("5 place du Marché", "69001", "Lyon"),
                siret: "56202000000047");

            db.Clients.Add(client);
            await db.SaveChangesAsync(ct);

            var invoice = new Invoice("F-2026-0001", client.Id, new DateOnly(2026, 1, 10));
            invoice.AddLine(new InvoiceLine("Prestation de conseil", 3, 450m, VatRate.Standard20));
            invoice.AddLine(new InvoiceLine("Formation sur site (journée)", 1, 900m, VatRate.Standard20));
            invoice.AddLine(new InvoiceLine("Denrées alimentaires", 20, 4.50m, VatRate.Reduced55));
            invoice.Issue();
            db.Invoices.Add(invoice);
        }

        await db.SaveChangesAsync(ct);
    }
}
