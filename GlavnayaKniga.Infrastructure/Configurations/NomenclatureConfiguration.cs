using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class NomenclatureConfiguration : IEntityTypeConfiguration<Nomenclature>
    {
        public void Configure(EntityTypeBuilder<Nomenclature> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.Article).IsUnique();
            builder.HasIndex(e => e.Barcode);
            builder.HasIndex(e => e.Type);
            builder.HasIndex(e => e.StorageLocationId);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.FullName)
                .HasMaxLength(1000);

            builder.Property(e => e.Article)
                .HasMaxLength(50);

            builder.Property(e => e.Barcode)
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(2000);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.Property(e => e.PurchasePrice)
                .HasPrecision(18, 2);

            builder.Property(e => e.SalePrice)
                .HasPrecision(18, 2);

            builder.Property(e => e.CurrentStock)
                .HasPrecision(18, 3);

            builder.Property(e => e.MinStock)
                .HasPrecision(18, 3);

            builder.Property(e => e.MaxStock)
                .HasPrecision(18, 3);

            builder.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь с местом хранения
            builder.HasOne(e => e.StorageLocation)
                .WithMany()
                .HasForeignKey(e => e.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull); // При удалении места хранения устанавливаем NULL

            // Новая связь для счета учета НДС
            builder.HasOne(e => e.DefaultVatAccount)
                .WithMany()
                .HasForeignKey(e => e.DefaultVatAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь с единицей измерения
            builder.HasOne(e => e.Unit)
                .WithMany(u => u.Nomenclatures)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}