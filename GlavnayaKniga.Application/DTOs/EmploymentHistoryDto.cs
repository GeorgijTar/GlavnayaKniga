using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class EmploymentHistoryDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public int PositionId { get; set; }
        public string? PositionName { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }

        public string ChangeType { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ChangeTypeDisplay => GetChangeTypeDisplay();
        public string PeriodDisplay => $"{StartDate:d} - {EndDate:d}";
        public bool IsCurrent => !EndDate.HasValue;

        private string GetChangeTypeDisplay()
        {
            return ChangeType switch
            {
                "Hire" => "Прием на работу",
                "Transfer" => "Перевод",
                "Dismissal" => "Увольнение",
                _ => ChangeType
            };
        }
    }
}