using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDto>> GetAllAccountsAsync(bool includeArchived = false);
        Task<AccountDto?> GetAccountByIdAsync(int id);
        Task<AccountDto?> GetAccountByCodeAsync(string code);
        Task<AccountDto> CreateAccountAsync(AccountDto accountDto);
        Task<AccountDto> UpdateAccountAsync(AccountDto accountDto);
        Task<bool> DeleteAccountAsync(int id);
        Task<bool> ArchiveAccountAsync(int id);
        Task<bool> UnarchiveAccountAsync(int id);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<IEnumerable<AccountDto>> GetAccountHierarchyAsync(bool includeArchived = false);
        Task<bool> HasChildrenAsync(int id);
        Task<bool> IsAccountUsedInEntriesAsync(int id);
        Task<IEnumerable<AccountDto>> GetAvailableParentAccountsAsync(int? accountId = null);
    }
}