using GlavnayaKniga.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IBankStatementService
    {
        /// <summary>
        /// Импорт выписки из файла
        /// </summary>
        Task<BankStatementImportResult> ImportStatementAsync(string filePath);

        /// <summary>
        /// Получить все импортированные выписки
        /// </summary>
        Task<IEnumerable<BankStatementDto>> GetAllStatementsAsync();

        /// <summary>
        /// Получить выписку по ID
        /// </summary>
        Task<BankStatementDto?> GetStatementByIdAsync(int id);

        /// <summary>
        /// Создать проводки по документам выписки
        /// </summary>
        Task<EntriesGenerationResult> CreateEntriesFromStatementAsync(int statementId, int defaultBasisId);

        /// <summary>
        /// Обновить документ выписки после создания проводки
        /// </summary>
        Task UpdateDocumentEntryIdAsync(int documentId, int entryId);

        /// <summary>
        /// Обновить статус выписки на основе наличия проводок у документов
        /// </summary>
        Task UpdateStatementStatusAsync(int statementId);

        /// <summary>
        /// Удалить выписку
        /// </summary>
        Task<bool> DeleteStatementAsync(int id);

        /// <summary>
        /// Проверить дубликат выписки
        /// </summary>
        Task<bool> IsDuplicateStatementAsync(string accountNumber, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Обновить контрагента для документа
        /// </summary>
        Task UpdateDocumentCounterpartyAsync(int documentId, int counterpartyId, bool isPayer);

        /// <summary>
        /// Автоматически найти или создать контрагента по ИНН и наименованию
        /// </summary>
        Task<int?> FindOrCreateCounterpartyAsync(string inn, string name, bool isPayer);

        
    }

    /// <summary>
    /// Результат импорта банковской выписки
    /// </summary>
    public class BankStatementImportResult
    {
        /// <summary>
        /// Успешность импорта
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если есть)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// ID созданной выписки
        /// </summary>
        public int StatementId { get; set; }

        /// <summary>
        /// Количество импортированных документов
        /// </summary>
        public int DocumentsCount { get; set; }

        /// <summary>
        /// Номер расчетного счета
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Дата начала периода
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания периода
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Найден ли банковский счет в справочнике
        /// </summary>
        public bool BankAccountFound { get; set; }

        /// <summary>
        /// ID банковского счета (если найден)
        /// </summary>
        public int? BankAccountId { get; set; }

        /// <summary>
        /// Количество созданных контрагентов
        /// </summary>
        public int CreatedCounterpartiesCount { get; set; }

        /// <summary>
        /// Количество созданных банковских счетов
        /// </summary>
        public int CreatedBankAccountsCount { get; set; }

        /// <summary>
        /// Список предупреждений
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Результат создания проводок по выписке
    /// </summary>
    public class EntriesGenerationResult
    {
        /// <summary>
        /// Успешность создания
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если есть)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Количество созданных проводок
        /// </summary>
        public int EntriesCreated { get; set; }

        /// <summary>
        /// Количество обработанных документов
        /// </summary>
        public int DocumentsProcessed { get; set; }

        /// <summary>
        /// Список предупреждений
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
}