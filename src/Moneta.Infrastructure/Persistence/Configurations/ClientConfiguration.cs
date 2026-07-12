using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moneta.Domain.Invoicing;

namespace Moneta.Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(320).IsRequired();
        builder.Property(c => c.Siret).HasMaxLength(14);
        builder.Property(c => c.VatNumber).HasMaxLength(20);

        builder.OwnsOne(c => c.Address, AddressMapping.Configure);
        builder.Navigation(c => c.Address).IsRequired();
    }
}
