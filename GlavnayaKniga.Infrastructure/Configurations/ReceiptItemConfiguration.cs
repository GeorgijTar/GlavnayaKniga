using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class ReceiptItemConfiguration : IEntityTypeConfiguration<ReceiptItem>
    {
        public void Configure(EntityTypeBuilder<ReceiptItem> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(e => e.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.VatRate)
                .HasPrecision(4, 2);

            builder.Property(e => e.VatAmount)
                .HasPrecision(18, 2);

            builder.Property(e => e.VatAccountId)
            .IsRequired();

            builder.Property(e => e.AmountWithVat)
                .HasPrecision(18, 2);

            builder.Property(e => e.Note)
                .HasMaxLength(500);

            builder.HasOne(e => e.Receipt)
                .WithMany(r => r.Items)
                .HasForeignKey(e => e.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Nomenclature)
                .WithMany()
                .HasForeignKey(e => e.NomenclatureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.DebitAccount)
                .WithMany()
                .HasForeignKey(e => e.DebitAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.VatAccount)
                .WithMany()
                .HasForeignKey(e => e.VatAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.StorageLocation)
                .WithMany()
                .HasForeignKey(e => e.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}