using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
    {
        public void Configure(EntityTypeBuilder<BankStatement> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.AccountNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(e => e.ImportedBy)
                .HasMaxLength(100);

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            builder.HasOne(e => e.BankAccount)
                .WithMany()
                .HasForeignKey(e => e.BankAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(e => new { e.AccountNumber, e.StartDate, e.EndDate });
            builder.HasIndex(e => e.Status);
        }
    }
}