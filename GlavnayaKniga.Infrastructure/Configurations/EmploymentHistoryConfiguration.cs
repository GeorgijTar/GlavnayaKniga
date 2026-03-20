using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class EmploymentHistoryConfiguration : IEntityTypeConfiguration<EmploymentHistory>
    {
        public void Configure(EntityTypeBuilder<EmploymentHistory> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.StartDate);
            builder.HasIndex(e => e.EndDate);
            builder.HasIndex(e => e.ChangeType);

            builder.Property(e => e.ChangeType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.OrderNumber)
                .HasMaxLength(50);

            builder.Property(e => e.Reason)
                .HasMaxLength(500);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.HasOne(e => e.Employee)
                .WithMany(e => e.EmploymentHistory)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Position)
                .WithMany(p => p.EmploymentHistory)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}