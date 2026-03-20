using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class EntryDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        // Счета
        public int DebitAccountId { get; set; }
        public string? DebitAccountCode { get; set; }
        public string? DebitAccountName { get; set; }

        public int CreditAccountId { get; set; }
        public string? CreditAccountCode { get; set; }
        public string? CreditAccountName { get; set; }

        public decimal Amount { get; set; }

        // Основание
        public int BasisId { get; set; }
        public string? BasisName { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Для отображения в списке
        public string DisplayDate => Date.ToString("dd.MM.yyyy");
        public string DisplayAmount => Amount.ToString("N2");
        public string DisplayDebit => $"{DebitAccountCode} {DebitAccountName}";
        public string DisplayCredit => $"{CreditAccountCode} {CreditAccountName}";
    }
}