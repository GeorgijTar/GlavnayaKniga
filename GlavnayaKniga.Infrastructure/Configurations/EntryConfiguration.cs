using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class EntryConfiguration : IEntityTypeConfiguration<Entry>
    {
        public void Configure(EntityTypeBuilder<Entry> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Date)
                .IsRequired();

            builder.HasOne(e => e.DebitAccount)
                .WithMany(a => a.DebitEntries)
                .HasForeignKey(e => e.DebitAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.CreditAccount)
                .WithMany(a => a.CreditEntries)
                .HasForeignKey(e => e.CreditAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Basis)
                .WithMany(b => b.Entries)
                .HasForeignKey(e => e.BasisId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.Date);
            builder.HasIndex(e => e.DebitAccountId);
            builder.HasIndex(e => e.CreditAccountId);
        }
    }
}