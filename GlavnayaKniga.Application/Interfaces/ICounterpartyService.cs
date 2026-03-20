using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface ICounterpartyService
    {
        Task<IEnumerable<CounterpartyDto>> GetAllCounterpartiesAsync(bool includeArchived = false);
        Task<CounterpartyDto?> GetCounterpartyByIdAsync(int id);
        Task<CounterpartyDto?> GetCounterpartyByINNAsync(string inn);
        Task<IEnumerable<CounterpartyDto>> SearchCounterpartiesAsync(string searchText, bool includeArchived = false);
        Task<CounterpartyDto> CreateCounterpartyAsync(CounterpartyDto counterpartyDto);
        Task<CounterpartyDto> UpdateCounterpartyAsync(CounterpartyDto counterpartyDto);
        Task<bool> ArchiveCounterpartyAsync(int id);
        Task<bool> UnarchiveCounterpartyAsync(int id);
        Task<bool> DeleteCounterpartyAsync(int id);
        Task<bool> IsINNUniqueAsync(string inn, int? excludeId = null);

        // Методы для работы с банковскими счетами контрагента
        Task<CounterpartyBankAccountDto> AddBankAccountAsync(CounterpartyBankAccountDto bankAccountDto);
        Task<CounterpartyBankAccountDto> UpdateBankAccountAsync(CounterpartyBankAccountDto bankAccountDto);
        Task<bool> DeleteBankAccountAsync(int id);
        Task<IEnumerable<CounterpartyBankAccountDto>> GetBankAccountsAsync(int counterpartyId);
        Task<CounterpartyBankAccountDto?> GetBankAccountByNumberAsync(string accountNumber, int counterpartyId);
        Task<bool> IsBankAccountExistsAsync(string accountNumber, int counterpartyId);
    }
}