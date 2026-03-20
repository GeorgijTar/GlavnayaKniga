using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IBankStatementParser
    {
        /// <summary>
        /// Разобрать файл выписки в формате 1CClientBankExchange
        /// </summary>
        Task<BankStatementParseResult> ParseFileAsync(string filePath);

        /// <summary>
        /// Разобрать содержимое выписки
        /// </summary>
        BankStatementParseResult ParseContent(string content, string fileName);
    }

    public class BankStatementParseResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<BankStatementDocumentDto> Documents { get; set; } = new();
        public Dictionary<DateTime, StatementDayInfo> DailyInfo { get; set; } = new();
    }

    public class StatementDayInfo
    {
        public DateTime Date { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}