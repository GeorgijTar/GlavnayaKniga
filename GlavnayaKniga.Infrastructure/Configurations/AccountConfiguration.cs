using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Code)
                .IsUnique();

            builder.HasIndex(e => e.FullCode);

            builder.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Type)
                .IsRequired()
                .HasDefaultValue(AccountType.ActivePassive);

            builder.Property(e => e.IsSynthetic)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);

            builder.Property(e => e.ArchivedAt)
                .IsRequired(false);

            builder.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.DebitEntries)
                .WithOne(e => e.DebitAccount)
                .HasForeignKey(e => e.DebitAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.CreditEntries)
                .WithOne(e => e.CreditAccount)
                .HasForeignKey(e => e.CreditAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            builder.HasIndex(e => e.IsArchived);
            builder.HasIndex(e => e.Type);
            builder.HasIndex(e => e.IsSynthetic);

            // Составной индекс для уникальности кода только среди неархивных счетов
            builder.HasIndex(e => new { e.Code, e.IsArchived })
                .IsUnique()
                .HasFilter("\"is_archived\" = false");
        }
    }
}