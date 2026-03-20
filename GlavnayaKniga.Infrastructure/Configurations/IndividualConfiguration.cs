using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Infrastructure.Configurations
{
    public class IndividualConfiguration : IEntityTypeConfiguration<Individual>
    {
        public void Configure(EntityTypeBuilder<Individual> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.INN)
                .IsUnique()
                .HasFilter("\"i_n_n\" IS NOT NULL AND \"i_n_n\" != ''");

            builder.HasIndex(e => e.SNILS)
                .IsUnique()
                .HasFilter("\"s_n_i_l_s\" IS NOT NULL AND \"s_n_i_l_s\" != ''");

            builder.HasIndex(e => new { e.LastName, e.FirstName, e.MiddleName, e.BirthDate });

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.MiddleName)
                .HasMaxLength(100);

            builder.Property(e => e.BirthPlace)
                .HasMaxLength(500);

            builder.Property(e => e.Gender)
                .HasMaxLength(10);

            builder.Property(e => e.Citizenship)
                .HasMaxLength(100);

            builder.Property(e => e.RegistrationAddress)
                .HasMaxLength(500);

            builder.Property(e => e.ActualAddress)
                .HasMaxLength(500);

            builder.Property(e => e.Phone)
                .HasMaxLength(50);

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.PassportSeries)
                .HasMaxLength(10);

            builder.Property(e => e.PassportNumber)
                .HasMaxLength(20);

            builder.Property(e => e.PassportIssuedBy)
                .HasMaxLength(500);

            builder.Property(e => e.PassportDepartmentCode)
                .HasMaxLength(20);

            builder.Property(e => e.INN)
                .HasMaxLength(12);

            builder.Property(e => e.SNILS)
                .HasMaxLength(14);

            builder.Property(e => e.Note)
                .HasMaxLength(1000);

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
        }
    }
}