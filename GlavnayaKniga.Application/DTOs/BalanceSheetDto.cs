using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class BalanceSheetDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<BalanceSheetRowDto> Rows { get; set; } = new();

        // Итоговые значения
        public decimal TotalOpeningBalanceDebit { get; set; }
        public decimal TotalOpeningBalanceCredit { get; set; }
        public decimal TotalTurnoverDebit { get; set; }
        public decimal TotalTurnoverCredit { get; set; }
        public decimal TotalClosingBalanceDebit { get; set; }
        public decimal TotalClosingBalanceCredit { get; set; }
    }

    public class BalanceSheetRowDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;

        // Начальное сальдо
        public decimal OpeningBalanceDebit { get; set; }
        public decimal OpeningBalanceCredit { get; set; }

        // Обороты за период
        public decimal TurnoverDebit { get; set; }
        public decimal TurnoverCredit { get; set; }

        // Конечное сальдо
        public decimal ClosingBalanceDebit { get; set; }
        public decimal ClosingBalanceCredit { get; set; }

        // Для иерархии
        public int Level { get; set; }
        public List<BalanceSheetRowDto> Children { get; set; } = new();
    }
}