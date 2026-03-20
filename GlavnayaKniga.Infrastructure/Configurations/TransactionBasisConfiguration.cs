using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class TransactionBasisConfiguration : IEntityTypeConfiguration<TransactionBasis>
    {
        public void Configure(EntityTypeBuilder<TransactionBasis> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Name)
                .IsUnique();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasMaxLength(500);
        }
    }
}