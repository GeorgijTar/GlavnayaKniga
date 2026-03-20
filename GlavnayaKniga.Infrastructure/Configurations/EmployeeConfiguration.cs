using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.PersonnelNumber)
                .IsUnique();

            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.HireDate);
            builder.HasIndex(e => e.DismissalDate);
            builder.HasIndex(e => e.DepartmentId);

            builder.Property(e => e.PersonnelNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.HireOrderNumber)
                .HasMaxLength(50);

            builder.Property(e => e.DismissalOrderNumber)
                .HasMaxLength(50);

            builder.Property(e => e.DismissalReason)
                .HasMaxLength(500);

            builder.Property(e => e.WorkPhone)
                .HasMaxLength(50);

            builder.Property(e => e.WorkEmail)
                .HasMaxLength(100);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.HasOne(e => e.Individual)
                .WithMany()
                .HasForeignKey(e => e.IndividualId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.CurrentPosition)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.CurrentPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Явно настраиваем связь с отделом
            builder.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Employees_Departments_DepartmentId");

            builder.HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}