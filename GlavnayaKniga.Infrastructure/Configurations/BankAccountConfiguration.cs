using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.AccountNumber)
                .IsUnique();

            builder.Property(e => e.AccountNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(e => e.BankName)
                .HasMaxLength(200);

            builder.Property(e => e.BIK)
                .HasMaxLength(20);

            builder.Property(e => e.CorrespondentAccount)
                .HasMaxLength(30);

            builder.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("RUB");

            builder.Property(e => e.CloseReason)
                .HasMaxLength(500);

            builder.HasOne(e => e.Subaccount)
                .WithMany()
                .HasForeignKey(e => e.SubaccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.CreatedAt)
                .IsRequired();
        }
    }
}