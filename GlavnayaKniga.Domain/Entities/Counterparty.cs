using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Тип контрагента
    /// </summary>
    public enum CounterpartyType
    {
        LegalEntity = 1,      // Юридическое лицо
        IndividualEntrepreneur = 2, // Индивидуальный предприниматель
        Individual = 3         // Физическое лицо
    }

    /// <summary>
    /// Контрагент (поставщик, покупатель, подотчетное лицо и т.д.)
    /// </summary>
    public class Counterparty
    {
        public int Id { get; set; }

        /// <summary>
        /// Полное наименование
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Краткое наименование (для отображения)
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// ИНН
        /// </summary>
        public string? INN { get; set; }

        /// <summary>
        /// КПП
        /// </summary>
        public string? KPP { get; set; }

        /// <summary>
        /// ОГРН / ОГРНИП
        /// </summary>
        public string? OGRN { get; set; }

        /// <summary>
        /// Тип контрагента
        /// </summary>
        public CounterpartyType Type { get; set; }

        /// <summary>
        /// Юридический адрес
        /// </summary>
        public string? LegalAddress { get; set; }

        /// <summary>
        /// Фактический адрес
        /// </summary>
        public string? ActualAddress { get; set; }

        /// <summary>
        /// Телефон
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Контактное лицо
        /// </summary>
        public string? ContactPerson { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Архивный (не используется в новых документах)
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Дата архивации
        /// </summary>
        public DateTime? ArchivedAt { get; set; }

        // Навигационные свойства
        public ICollection<CounterpartyBankAccount> BankAccounts { get; set; } = new List<CounterpartyBankAccount>();
    }
}