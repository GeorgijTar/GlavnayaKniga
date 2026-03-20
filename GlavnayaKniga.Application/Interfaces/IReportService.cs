using GlavnayaKniga.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IReportService
    {
        // Анализ счета
        Task<AccountAnalysisDto> GetAccountAnalysisAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<List<AccountAnalysisDto>> GetAccountAnalysisWithSubaccountsAsync(int accountId, DateTime startDate, DateTime endDate);

        // Оборотно-сальдовая ведомость
        Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime startDate, DateTime endDate, int? accountId = null);

        // Главная книга
        Task<List<GeneralLedgerEntryDto>> GetGeneralLedgerAsync(int accountId, DateTime startDate, DateTime endDate);
    }
}