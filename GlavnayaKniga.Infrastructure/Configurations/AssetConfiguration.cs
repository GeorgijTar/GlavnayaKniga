using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.InventoryNumber)
                .IsUnique()
                .HasFilter("\"inventory_number\" IS NOT NULL");

            builder.HasIndex(e => e.RegistrationNumber);
            builder.HasIndex(e => e.SerialNumber);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.RegistrationNumber)
                .HasMaxLength(50);

            builder.Property(e => e.InventoryNumber)
                .HasMaxLength(50);

            builder.Property(e => e.Model)
                .HasMaxLength(200);

            builder.Property(e => e.Manufacturer)
                .HasMaxLength(200);

            builder.Property(e => e.SerialNumber)
                .HasMaxLength(100);

            builder.Property(e => e.Location)
                .HasMaxLength(200);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.Property(e => e.InitialCost)
                .HasPrecision(18, 2);

            builder.Property(e => e.ResidualValue)
                .HasPrecision(18, 2);

            builder.HasOne(e => e.AssetType)
                .WithMany(t => t.Assets)
                .HasForeignKey(e => e.AssetTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ResponsiblePerson)
                .WithMany()
                .HasForeignKey(e => e.ResponsiblePersonId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}