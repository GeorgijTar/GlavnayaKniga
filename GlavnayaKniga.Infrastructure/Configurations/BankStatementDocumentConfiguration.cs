using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class BankStatementDocumentConfiguration : IEntityTypeConfiguration<BankStatementDocument>
    {
        public void Configure(EntityTypeBuilder<BankStatementDocument> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.DocumentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.PayerAccount)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(e => e.PayerINN)
                .HasMaxLength(20);

            builder.Property(e => e.PayerName)
                .HasMaxLength(500);

            builder.Property(e => e.PayerBIK)
                .HasMaxLength(20);

            builder.Property(e => e.RecipientAccount)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(e => e.RecipientINN)
                .HasMaxLength(20);

            builder.Property(e => e.RecipientName)
                .HasMaxLength(500);

            builder.Property(e => e.RecipientBIK)
                .HasMaxLength(20);

            builder.Property(e => e.PaymentPurpose)
                .HasMaxLength(1000);

            builder.Property(e => e.PaymentType)
                .HasMaxLength(50);

            builder.Property(e => e.Hash)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasOne(e => e.BankStatement)
                .WithMany(s => s.Documents)
                .HasForeignKey(e => e.BankStatementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.PayerCounterparty)
                .WithMany()
                .HasForeignKey(e => e.PayerCounterpartyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.RecipientCounterparty)
                .WithMany()
                .HasForeignKey(e => e.RecipientCounterpartyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(e => e.Hash).IsUnique();
            builder.HasIndex(e => e.Date);
            builder.HasIndex(e => new { e.PayerAccount, e.RecipientAccount });
            builder.HasIndex(e => e.PayerCounterpartyId);
            builder.HasIndex(e => e.RecipientCounterpartyId);
        }
    }
}