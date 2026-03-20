using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class GeneralLedgerEntryDto
    {
        public DateTime Date { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public int? CorrespondingAccountId { get; set; }
        public string? CorrespondingAccountCode { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance { get; set; }
    }
}