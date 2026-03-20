using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }

        public int IndividualId { get; set; }
        public string? IndividualFullName { get; set; }
        public string? IndividualShortName { get; set; }
        public string? IndividualPhone { get; set; }
        public string? IndividualEmail { get; set; }

        public string PersonnelNumber { get; set; } = string.Empty;

        public int CurrentPositionId { get; set; }
        public string? CurrentPositionName { get; set; }

        // Отдел
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCode { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime HireDate { get; set; }
        public string? HireOrderNumber { get; set; }
        public DateTime? HireOrderDate { get; set; }

        public DateTime? DismissalDate { get; set; }
        public string? DismissalOrderNumber { get; set; }
        public DateTime? DismissalOrderDate { get; set; }
        public string? DismissalReason { get; set; }

        public string? WorkPhone { get; set; }
        public string? WorkEmail { get; set; }

        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<EmploymentHistoryDto> EmploymentHistory { get; set; } = new();

        public string DisplayName => $"{IndividualShortName} ({PersonnelNumber})";
        public string StatusDisplay => GetStatusDisplay();
        public string HireInfo => $"Принят: {HireDate:d} {HireOrderNumber}";
        public bool IsActive => Status == "Active";
        public string DepartmentDisplay => DepartmentName ?? "—";

        private string GetStatusDisplay()
        {
            return Status switch
            {
                "Active" => "Работает",
                "OnLeave" => "В отпуске",
                "Dismissed" => "Уволен",
                "Probation" => "Испытательный срок",
                _ => "Неизвестно"
            };
        }
    }
}