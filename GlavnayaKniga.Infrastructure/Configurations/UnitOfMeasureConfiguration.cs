using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
    {
        public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Code)
                .IsUnique();

            builder.HasIndex(e => e.ShortName);

            builder.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.InternationalCode)
                .HasMaxLength(10);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}