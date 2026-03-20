using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class IndividualDto
    {
        public int Id { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
        public string ShortName => $"{LastName} {FirstName?[0]}. {MiddleName?[0]}.".Trim();

        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? Gender { get; set; }
        public string? Citizenship { get; set; }
        public string? RegistrationAddress { get; set; }
        public string? ActualAddress { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public string? PassportSeries { get; set; }
        public string? PassportNumber { get; set; }
        public DateTime? PassportIssueDate { get; set; }
        public string? PassportIssuedBy { get; set; }
        public string? PassportDepartmentCode { get; set; }

        public string? INN { get; set; }
        public string? SNILS { get; set; }
        public string? Note { get; set; }

        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string DisplayName => ShortName;
        public string StatusDisplay => IsArchived ? "Архивный" : "Активный";
    }
}