using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class BankAccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BIK { get; set; } = string.Empty;
        public string CorrespondentAccount { get; set; } = string.Empty;
        public int SubaccountId { get; set; }
        public string? SubaccountCode { get; set; }
        public string? SubaccountName { get; set; }
        public string Currency { get; set; } = "RUB";
        public bool IsActive { get; set; } = true;

        // Даты открытия и закрытия
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string? CloseReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Вычисляемые свойства
        public string DisplayName => $"{AccountNumber} - {BankName} ({SubaccountCode})";

        public string StatusDisplay => IsActive ? "Активен" : "Закрыт";

        public string OpenDateDisplay => OpenDate?.ToString("dd.MM.yyyy") ?? "—";

        public string CloseDateDisplay => CloseDate?.ToString("dd.MM.yyyy") ?? "—";

        public string PeriodDisplay => OpenDate.HasValue
            ? $"с {OpenDate:dd.MM.yyyy}" + (CloseDate.HasValue ? $" по {CloseDate:dd.MM.yyyy}" : " по н.в.")
            : "—";
    }
}