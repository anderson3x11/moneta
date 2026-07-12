using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moneta.Domain.Invoicing;

namespace Moneta.Infrastructure.Persistence.Configurations;

public sealed class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.ToTable("seller_profile");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Siret).HasMaxLength(14).IsRequired();
        builder.Property(s => s.VatNumber).HasMaxLength(20);
        builder.Property(s => s.Iban).HasMaxLength(34);
        builder.Property(s => s.ContactEmail).HasMaxLength(320).IsRequired();

        builder.OwnsOne(s => s.Address, AddressMapping.Configure);
        builder.Navigation(s => s.Address).IsRequired();
    }
}
