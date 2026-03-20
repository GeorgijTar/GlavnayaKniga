using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class BankStatementDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int? BankAccountId { get; set; }
        public string? BankAccountDisplay { get; set; }
        public DateTime ImportedAt { get; set; }

        // Свойство для совместимости (алиас для ImportedAt)
        public DateTime ImportDate => ImportedAt;

        public string? ImportedBy { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DocumentsCount { get; set; }
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<BankStatementDocumentDto> Documents { get; set; } = new();

        // Вычисляемые свойства для статуса обработки
        public int ProcessedDocumentsCount => Documents?.Count(d => d.EntryId.HasValue) ?? 0;
        public int PendingDocumentsCount => DocumentsCount - ProcessedDocumentsCount;

        public string ProcessingStatus => Status switch
        {
            "New" => "Новая",
            "PartiallyProcessed" => "Частично обработана",
            "Processed" => "Обработана",
            "Error" => "Ошибка",
            "Duplicate" => "Дубликат",
            _ => Status
        };

        public string ProcessingStatusColor => Status switch
        {
            "New" => "Orange",
            "PartiallyProcessed" => "Gold",
            "Processed" => "Green",
            "Error" => "Red",
            "Duplicate" => "Gray",
            _ => "Blue"
        };

        public string ProcessingInfo => $"Обработано: {ProcessedDocumentsCount} из {DocumentsCount}";
    }
}
