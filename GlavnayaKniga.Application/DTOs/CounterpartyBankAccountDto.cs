using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class CounterpartyBankAccountDto
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BIK { get; set; } = string.Empty;
        public string? CorrespondentAccount { get; set; }
        public string Currency { get; set; } = "RUB";
        public bool IsDefault { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string DisplayName => $"{AccountNumber} ({BankName})";
    }
}