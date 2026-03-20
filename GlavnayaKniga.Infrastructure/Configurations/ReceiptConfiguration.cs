using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Number)
                .IsUnique();

            builder.HasIndex(e => e.Date);
            builder.HasIndex(e => e.AccountingDate);
            builder.HasIndex(e => e.Status);

            builder.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Date)
                .IsRequired();

            builder.Property(e => e.AccountingDate)
                .IsRequired();

            builder.Property(e => e.ContractNumber)
                .HasMaxLength(50);

            builder.Property(e => e.Basis)
                .HasMaxLength(200);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.TotalVatAmount)
                .HasPrecision(18, 2);

            builder.Property(e => e.TotalAmountWithVat)
                .HasPrecision(18, 2);

            builder.Property(e => e.InvoiceNumber)
                .HasMaxLength(50);

            builder.Property(e => e.IsUPD)
                .HasDefaultValue(false);

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            builder.Property(e => e.PostedBy)
                .HasMaxLength(100);

            builder.HasOne(e => e.Contractor)
                .WithMany()
                .HasForeignKey(e => e.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.CreditAccount)
                .WithMany()
                .HasForeignKey(e => e.CreditAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Entries)
                .WithOne()
                .HasForeignKey("ReceiptId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}