using System;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Банковский счет контрагента
    /// </summary>
    public class CounterpartyBankAccount
    {
        public int Id { get; set; }

        /// <summary>
        /// Контрагент
        /// </summary>
        public int CounterpartyId { get; set; }
        public Counterparty Counterparty { get; set; } = null!;

        /// <summary>
        /// Номер счета
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Наименование банка
        /// </summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// БИК
        /// </summary>
        public string BIK { get; set; } = string.Empty;

        /// <summary>
        /// Корреспондентский счет
        /// </summary>
        public string CorrespondentAccount { get; set; } = string.Empty;

        /// <summary>
        /// Валюта счета
        /// </summary>
        public string Currency { get; set; } = "RUB";

        /// <summary>
        /// Основной счет (для выбора по умолчанию)
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}