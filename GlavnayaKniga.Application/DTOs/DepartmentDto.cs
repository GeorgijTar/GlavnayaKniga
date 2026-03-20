using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? FullName { get; set; }

        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? ParentCode { get; set; }

        public int? HeadEmployeeId { get; set; }
        public string? HeadEmployeeName { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
        public string? Note { get; set; }

        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Для иерархического отображения
        public List<DepartmentDto> Children { get; set; } = new();

        // Вычисляемые свойства
        public string DisplayName => $"{Code} - {Name}";
        public string FullPath => GetFullPath();
        public string StatusDisplay => IsArchived ? "Архивный" : "Активный";
        public int EmployeeCount { get; set; }

        private string GetFullPath()
        {
            return ParentName != null ? $"{ParentName} / {Name}" : Name;
        }
    }
}