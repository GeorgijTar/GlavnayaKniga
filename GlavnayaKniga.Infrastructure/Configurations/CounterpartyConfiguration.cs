using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class CounterpartyConfiguration : IEntityTypeConfiguration<Counterparty>
    {
        public void Configure(EntityTypeBuilder<Counterparty> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.INN)
                .IsUnique()
                .HasFilter("\"i_n_n\" IS NOT NULL AND \"i_n_n\" != ''");

            builder.HasIndex(e => new { e.ShortName, e.IsArchived });

            builder.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.INN)
                .HasMaxLength(12);

            builder.Property(e => e.KPP)
                .HasMaxLength(9);

            builder.Property(e => e.OGRN)
                .HasMaxLength(15);

            builder.Property(e => e.LegalAddress)
                .HasMaxLength(500);

            builder.Property(e => e.ActualAddress)
                .HasMaxLength(500);

            builder.Property(e => e.Phone)
                .HasMaxLength(50);

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.ContactPerson)
                .HasMaxLength(200);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}