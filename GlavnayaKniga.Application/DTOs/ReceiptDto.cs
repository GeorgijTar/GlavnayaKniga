using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class ReceiptDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime AccountingDate { get; set; }

        public int ContractorId { get; set; }
        public string? ContractorName { get; set; }
        public string? ContractorINN { get; set; }

        public int CreditAccountId { get; set; }
        public string? CreditAccountCode { get; set; }
        public string? CreditAccountName { get; set; }

        public string VatCalculationMethod { get; set; } = "IncludedInPrice";

        public string? ContractNumber { get; set; }
        public DateTime? ContractDate { get; set; }

        public string? Basis { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? TotalVatAmount { get; set; }
        public decimal? TotalAmountWithVat { get; set; }

        public bool IsUPD { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public string Status { get; set; } = "Draft";
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? PostedBy { get; set; }

        public List<ReceiptItemDto> Items { get; set; } = new();

        // Вычисляемые свойства
        public string DisplayName => $"Поступление №{Number} от {Date:d}";
        public string StatusDisplay => Status == "Draft" ? "Черновик" : "Проведен";
        public string ContractorDisplay => $"{ContractorName} ({ContractorINN})";
        public string CreditAccountDisplay => $"{CreditAccountCode} - {CreditAccountName}";
        public string VatCalculationDisplay => VatCalculationMethod == "AbovePrice" ? "НДС сверху" : "НДС в цене";

        public bool CanEdit => Status == "Draft";
        public bool CanPost => Status == "Draft" && Items.Count > 0;
        public bool CanUnpost => Status == "Posted";
    }
}