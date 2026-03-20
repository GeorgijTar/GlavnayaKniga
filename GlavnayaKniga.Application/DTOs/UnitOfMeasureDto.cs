using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class UnitOfMeasureDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? InternationalCode { get; set; }
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string DisplayName => $"{Code} - {ShortName}";
        public string StatusDisplay => IsArchived ? "Архивная" : "Активная";
    }
}