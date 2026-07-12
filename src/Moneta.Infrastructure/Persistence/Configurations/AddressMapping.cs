using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moneta.Domain.ValueObjects;

namespace Moneta.Infrastructure.Persistence.Configurations;

/// <summary>Shared column mapping for the owned <see cref="Address"/> value object.</summary>
internal static class AddressMapping
{
    public static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, Address> address)
        where TOwner : class
    {
        address.Property(a => a.Line1).HasColumnName("address_line1").HasMaxLength(200).IsRequired();
        address.Property(a => a.Line2).HasColumnName("address_line2").HasMaxLength(200);
        address.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(16).IsRequired();
        address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(120).IsRequired();
        address.Property(a => a.CountryCode).HasColumnName("address_country").HasMaxLength(2).IsRequired();
    }
}
