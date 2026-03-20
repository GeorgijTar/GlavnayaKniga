using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class CounterpartyBankAccountConfiguration : IEntityTypeConfiguration<CounterpartyBankAccount>
    {
        public void Configure(EntityTypeBuilder<CounterpartyBankAccount> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.AccountNumber);

            builder.Property(e => e.AccountNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(e => e.BankName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.BIK)
                .IsRequired()
                .HasMaxLength(9);

            builder.Property(e => e.CorrespondentAccount)
                .HasMaxLength(30);

            builder.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("RUB");

            builder.Property(e => e.Note)
                .HasMaxLength(500);

            builder.HasOne(e => e.Counterparty)
                .WithMany(c => c.BankAccounts)
                .HasForeignKey(e => e.CounterpartyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}