using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Банковская выписка (файл)
    /// </summary>
    public class BankStatement
    {
        public int Id { get; set; }

        /// <summary>
        /// Имя исходного файла
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Дата начала периода в выписке
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания периода в выписке
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Расчетный счет
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Банковский счет организации (связь)
        /// </summary>
        public int? BankAccountId { get; set; }
        public BankAccount? BankAccount { get; set; }

        /// <summary>
        /// Дата импорта
        /// </summary>
        public DateTime ImportedAt { get; set; }

        /// <summary>
        /// Пользователь, импортировавший выписку
        /// </summary>
        public string? ImportedBy { get; set; }

        /// <summary>
        /// Статус импорта
        /// </summary>
        public StatementImportStatus Status { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если есть)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Документы выписки
        /// </summary>
        public ICollection<BankStatementDocument> Documents { get; set; } = new List<BankStatementDocument>();
    }

    public enum StatementImportStatus
    {
        New = 0,           // Новая, не обработана
        PartiallyProcessed = 1, // Частично обработана (есть проводки у части документов)
        Processed = 2,     // Полностью обработана (все документы имеют проводки)
        Error = 3,         // Ошибка при импорте
        Duplicate = 4      // Дубликат
    }


}