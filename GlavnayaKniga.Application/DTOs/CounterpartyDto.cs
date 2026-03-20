using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class CounterpartyDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? INN { get; set; }
        public string? KPP { get; set; }
        public string? OGRN { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? LegalAddress { get; set; }
        public string? ActualAddress { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public string? Note { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }

        public List<CounterpartyBankAccountDto> BankAccounts { get; set; } = new();

        // Вычисляемые свойства
        public string DisplayName => string.IsNullOrEmpty(ShortName) ? FullName : ShortName;
        public string DisplayINN => INN ?? "—";
        public string DisplayKPP => KPP ?? "—";
        public string StatusDisplay => IsArchived ? "Архивный" : "Активный";
    }
}