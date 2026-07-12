using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moneta.Domain.Invoicing;
using Moneta.Domain.ValueObjects;

namespace Moneta.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Number).HasMaxLength(32).IsRequired();
        builder.HasIndex(i => i.Number).IsUnique();

        builder.Property(i => i.ClientId).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(i => i.Notes).HasMaxLength(2000);

        // Totals are computed from the lines, never stored.
        builder.Ignore(i => i.VatBreakdowns);
        builder.Ignore(i => i.TotalExclVat);
        builder.Ignore(i => i.TotalVat);
        builder.Ignore(i => i.TotalInclVat);

        // Lines are part of the invoice aggregate, stored as an owned collection.
        builder.OwnsMany(i => i.Lines, lines =>
        {
            lines.ToTable("invoice_lines");
            lines.WithOwner().HasForeignKey("InvoiceId");
            lines.HasKey(l => l.Id);

            lines.Property(l => l.Description).HasMaxLength(500).IsRequired();
            lines.Property(l => l.Quantity).HasPrecision(18, 3);
            lines.Property(l => l.UnitPriceExclVat).HasPrecision(18, 2);

            // The VAT rate value object is persisted as its percentage.
            lines.Property(l => l.VatRate)
                .HasColumnName("vat_percentage")
                .HasPrecision(5, 2)
                .HasConversion(v => v.Percentage, p => VatRate.FromPercentage(p));

            // Computed from quantity and unit price.
            lines.Ignore(l => l.NetAmount);
        });

        builder.Navigation(i => i.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
