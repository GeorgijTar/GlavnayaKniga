using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IBankAccountService
    {
        Task<IEnumerable<BankAccountDto>> GetAllBankAccountsAsync();
        Task<BankAccountDto?> GetBankAccountByIdAsync(int id);
        Task<BankAccountDto?> GetBankAccountByNumberAsync(string accountNumber);
        Task<BankAccountDto> CreateBankAccountAsync(BankAccountDto bankAccountDto);
        Task<BankAccountDto> UpdateBankAccountAsync(BankAccountDto bankAccountDto);
        Task<bool> DeleteBankAccountAsync(int id);
        Task<BankAccountDto?> FindBankAccountByNumberAsync(string accountNumber);
    }
}