using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Code)
                .IsUnique();

            builder.HasIndex(e => e.Name);

            builder.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.FullName)
                .HasMaxLength(500);

            builder.Property(e => e.Phone)
                .HasMaxLength(50);

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.Location)
                .HasMaxLength(200);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Departments_Departments_ParentId");

            builder.HasOne(e => e.HeadEmployee)
                .WithMany()
                .HasForeignKey(e => e.HeadEmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Departments_Employees_HeadEmployeeId");

            // Настраиваем связь с сотрудниками (обратная сторона уже настроена в EmployeeConfiguration)
            builder.HasMany(e => e.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}