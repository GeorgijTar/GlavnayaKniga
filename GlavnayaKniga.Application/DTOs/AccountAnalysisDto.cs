using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class AccountAnalysisDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Начальное сальдо
        public decimal OpeningBalance { get; set; }

        // Обороты
        public List<CorrespondingAccountDto> CorrespondingAccounts { get; set; } = new();

        // Итоговые обороты
        public decimal TotalDebitTurnover { get; set; }
        public decimal TotalCreditTurnover { get; set; }

        // Конечное сальдо
        public decimal ClosingBalance { get; set; }

        // Дочерние счета (для иерархического отображения)
        public List<AccountAnalysisDto> Children { get; set; } = new();
    }

    public class CorrespondingAccountDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}