using System;

namespace GlavnayaKniga.Domain.Entities
{
    public class BankStatementDocument
    {
        public int Id { get; set; }

        /// <summary>
        /// Ссылка на выписку
        /// </summary>
        public int BankStatementId { get; set; }
        public BankStatement BankStatement { get; set; } = null!;

        /// <summary>
        /// Тип документа (Платежное поручение, Банковский ордер и т.д.)
        /// </summary>
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>
        /// Номер документа
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Дата документа
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Сумма
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Счет плательщика
        /// </summary>
        public string PayerAccount { get; set; } = string.Empty;

        /// <summary>
        /// ИНН плательщика
        /// </summary>
        public string? PayerINN { get; set; }

        /// <summary>
        /// Наименование плательщика
        /// </summary>
        public string? PayerName { get; set; }

        /// <summary>
        /// БИК банка плательщика
        /// </summary>
        public string? PayerBIK { get; set; }

        /// <summary>
        /// Счет получателя
        /// </summary>
        public string RecipientAccount { get; set; } = string.Empty;

        /// <summary>
        /// ИНН получателя
        /// </summary>
        public string? RecipientINN { get; set; }

        /// <summary>
        /// Наименование получателя
        /// </summary>
        public string? RecipientName { get; set; }

        /// <summary>
        /// БИК банка получателя
        /// </summary>
        public string? RecipientBIK { get; set; }

        /// <summary>
        /// Назначение платежа
        /// </summary>
        public string? PaymentPurpose { get; set; }

        /// <summary>
        /// Вид платежа
        /// </summary>
        public string? PaymentType { get; set; }

        /// <summary>
        /// Очередность платежа
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// Дата поступления (для входящих)
        /// </summary>
        public DateTime? ReceivedDate { get; set; }

        /// <summary>
        /// Дата списания (для исходящих)
        /// </summary>
        public DateTime? WithdrawnDate { get; set; }

        /// <summary>
        /// Созданная проводка (если документ импортирован)
        /// </summary>
        public int? EntryId { get; set; }
        public Entry? Entry { get; set; }

        /// <summary>
        /// Контрагент-плательщик
        /// </summary>
        public int? PayerCounterpartyId { get; set; }
        public Counterparty? PayerCounterparty { get; set; }

        /// <summary>
        /// Контрагент-получатель
        /// </summary>
        public int? RecipientCounterpartyId { get; set; }
        public Counterparty? RecipientCounterparty { get; set; }

        /// <summary>
        /// Признак входящего/исходящего
        /// </summary>
        public bool IsIncoming => RecipientAccount == BankStatement?.AccountNumber;

        /// <summary>
        /// Хэш для обнаружения дубликатов
        /// </summary>
        public string Hash { get; set; } = string.Empty;
    }
}