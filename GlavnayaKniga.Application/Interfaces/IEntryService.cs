using GlavnayaKniga.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IEntryService
    {
        Task<IEnumerable<EntryDto>> GetAllEntriesAsync();
        Task<EntryDto?> GetEntryByIdAsync(int id);
        Task<IEnumerable<EntryDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<EntryDto>> GetEntriesByAccountAsync(int accountId);
        Task<EntryDto> CreateEntryAsync(EntryDto entryDto);
        Task<EntryDto> UpdateEntryAsync(EntryDto entryDto);
        Task<bool> DeleteEntryAsync(int id);
        Task<decimal> GetAccountTurnoverAsync(int accountId, DateTime startDate, DateTime endDate, bool isDebit);
        Task<decimal> GetAccountBalanceAsync(int accountId, DateTime asOfDate);
    }
}