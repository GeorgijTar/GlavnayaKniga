using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
    {
        public void Configure(EntityTypeBuilder<StorageLocation> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Code)
                .IsUnique();

            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.Type);

            builder.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.Address)
                .HasMaxLength(500);

            builder.Property(e => e.TemperatureRegime)
                .HasMaxLength(100);

            builder.Property(e => e.Area)
                .HasPrecision(18, 2);

            builder.Property(e => e.Capacity)
                .HasPrecision(18, 2);

            builder.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Обновляем связь с ответственным лицом (теперь Employee вместо Counterparty)
            builder.HasOne(e => e.ResponsibleEmployee)
                .WithMany()
                .HasForeignKey(e => e.ResponsibleEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}