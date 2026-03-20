using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Статус документа поступления
    /// </summary>
    public enum ReceiptStatus
    {
        Draft = 0,      // Черновик
        Posted = 1      // Проведен
    }

    /// <summary>
    /// Способ расчета НДС для всего документа
    /// </summary>
    public enum VatCalculationMethod
    {
        IncludedInPrice = 0,  // НДС включен в цену
        AbovePrice = 1        // НДС сверху цены
    }

    /// <summary>
    /// Документ поступления товаров, работ, услуг
    /// </summary>
    public class Receipt
    {
        public int Id { get; set; }

        /// <summary>
        /// Номер документа
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Дата документа
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Дата принятия к учету (дата проводки)
        /// </summary>
        public DateTime AccountingDate { get; set; }

        /// <summary>
        /// Контрагент (поставщик)
        /// </summary>
        public int ContractorId { get; set; }
        public Counterparty Contractor { get; set; } = null!;

        /// <summary>
        /// Счет учета поставщика (кредит) - общий для всего документа
        /// </summary>
        public int CreditAccountId { get; set; }
        public Account CreditAccount { get; set; } = null!;

        /// <summary>
        /// Способ расчета НДС для всего документа
        /// </summary>
        public VatCalculationMethod VatCalculationMethod { get; set; }

        /// <summary>
        /// Договор (опционально)
        /// </summary>
        public string? ContractNumber { get; set; }
        public DateTime? ContractDate { get; set; }

        /// <summary>
        /// Основание (счет, накладная и т.д.)
        /// </summary>
        public string? Basis { get; set; }

        /// <summary>
        /// Сумма документа без НДС
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Общая сумма НДС по документу
        /// </summary>
        public decimal? TotalVatAmount { get; set; }

        /// <summary>
        /// Общая сумма документа с НДС
        /// </summary>
        public decimal? TotalAmountWithVat { get; set; }

        /// <summary>
        /// Является ли документ УПД (универсальный передаточный документ)
        /// </summary>
        public bool IsUPD { get; set; }

        /// <summary>
        /// Номер счета-фактуры (для УПД заполняется автоматически, иначе вручную)
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Дата счета-фактуры
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Статус документа
        /// </summary>
        public ReceiptStatus Status { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Дата проведения
        /// </summary>
        public DateTime? PostedAt { get; set; }

        /// <summary>
        /// Пользователь, создавший документ
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Пользователь, проведший документ
        /// </summary>
        public string? PostedBy { get; set; }

        // Навигационные свойства
        public ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}