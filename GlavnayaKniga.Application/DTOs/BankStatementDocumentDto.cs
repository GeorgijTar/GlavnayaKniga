using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class BankStatementDocumentDto
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        // Плательщик
        public string PayerAccount { get; set; } = string.Empty;
        public string? PayerINN { get; set; }
        public string? PayerName { get; set; }
        public string? PayerBIK { get; set; }

        // Получатель
        public string RecipientAccount { get; set; } = string.Empty;
        public string? RecipientINN { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientBIK { get; set; }

        // Платеж
        public string? PaymentPurpose { get; set; }
        public string? PaymentType { get; set; }
        public int? Priority { get; set; }

        // Даты
        public DateTime? ReceivedDate { get; set; }
        public DateTime? WithdrawnDate { get; set; }

        // Связи
        public int? EntryId { get; set; }
        public bool IsIncoming { get; set; }

        // Контрагенты
        public int? PayerCounterpartyId { get; set; }
        public string? PayerCounterpartyName { get; set; }
        public int? RecipientCounterpartyId { get; set; }
        public string? RecipientCounterpartyName { get; set; }

        // Хэш для обнаружения дубликатов
        public string Hash { get; set; } = string.Empty;

        // Вычисляемые свойства
        public bool IsImported => EntryId.HasValue;
        public string Direction => IsIncoming ? "Входящий" : "Исходящий";
        public string DisplayAmount => Amount.ToString("N2");
        public string Counterparty => IsIncoming ? PayerName ?? PayerAccount : RecipientName ?? RecipientAccount;
        public string CounterpartyINN => IsIncoming ? PayerINN ?? "" : RecipientINN ?? "";
        public string DisplayDate => Date.ToString("dd.MM.yyyy");

        // ID контрагента для текущего документа
        public int? CounterpartyId => IsIncoming ? PayerCounterpartyId : RecipientCounterpartyId;
        public string? CounterpartyDisplayName => IsIncoming ? PayerCounterpartyName : RecipientCounterpartyName;
    }
}