using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IRepository<BankAccount> _bankAccountRepository;
        private readonly IRepository<Account> _accountRepository;

        public BankAccountService(
            IRepository<BankAccount> bankAccountRepository,
            IRepository<Account> accountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<BankAccountDto>> GetAllBankAccountsAsync()
        {
            var bankAccounts = await _bankAccountRepository.GetAllAsync();
            var result = new List<BankAccountDto>();

            foreach (var bankAccount in bankAccounts.OrderBy(b => b.AccountNumber))
            {
                var dto = await MapToDto(bankAccount);
                result.Add(dto);
            }

            return result;
        }

        public async Task<BankAccountDto?> GetBankAccountByIdAsync(int id)
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(id);
            return bankAccount != null ? await MapToDto(bankAccount) : null;
        }

        public async Task<BankAccountDto?> GetBankAccountByNumberAsync(string accountNumber)
        {
            var bankAccounts = await _bankAccountRepository.FindAsync(b => b.AccountNumber == accountNumber);
            var bankAccount = bankAccounts.FirstOrDefault();
            return bankAccount != null ? await MapToDto(bankAccount) : null;
        }

        public async Task<BankAccountDto?> FindBankAccountByNumberAsync(string accountNumber)
        {
            return await GetBankAccountByNumberAsync(accountNumber);
        }

        public async Task<BankAccountDto> CreateBankAccountAsync(BankAccountDto bankAccountDto)
        {
            // Проверяем уникальность номера счета
            var existing = await GetBankAccountByNumberAsync(bankAccountDto.AccountNumber);
            if (existing != null)
            {
                throw new InvalidOperationException($"Расчетный счет {bankAccountDto.AccountNumber} уже существует");
            }

            // Проверяем существование субсчета
            var subaccount = await _accountRepository.GetByIdAsync(bankAccountDto.SubaccountId);
            if (subaccount == null)
            {
                throw new InvalidOperationException($"Субсчет с ID {bankAccountDto.SubaccountId} не найден");
            }

            var bankAccount = new BankAccount
            {
                AccountNumber = bankAccountDto.AccountNumber,
                BankName = bankAccountDto.BankName,
                BIK = bankAccountDto.BIK,
                CorrespondentAccount = bankAccountDto.CorrespondentAccount,
                SubaccountId = bankAccountDto.SubaccountId,
                Currency = bankAccountDto.Currency,
                IsActive = bankAccountDto.IsActive,
                OpenDate = bankAccountDto.OpenDate,           // Добавляем дату открытия
                CloseDate = bankAccountDto.CloseDate,         // Добавляем дату закрытия
                CloseReason = bankAccountDto.CloseReason,     // Добавляем причину закрытия
                CreatedAt = DateTime.UtcNow
            };

            var created = await _bankAccountRepository.AddAsync(bankAccount);
            return await MapToDto(created);
        }

        public async Task<BankAccountDto> UpdateBankAccountAsync(BankAccountDto bankAccountDto)
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountDto.Id);
            if (bankAccount == null)
            {
                throw new InvalidOperationException($"Банковский счет с ID {bankAccountDto.Id} не найден");
            }

            // Проверяем уникальность номера счета (если он изменился)
            if (bankAccount.AccountNumber != bankAccountDto.AccountNumber)
            {
                var existing = await GetBankAccountByNumberAsync(bankAccountDto.AccountNumber);
                if (existing != null)
                {
                    throw new InvalidOperationException($"Расчетный счет {bankAccountDto.AccountNumber} уже существует");
                }
            }

            // Проверяем существование субсчета
            var subaccount = await _accountRepository.GetByIdAsync(bankAccountDto.SubaccountId);
            if (subaccount == null)
            {
                throw new InvalidOperationException($"Субсчет с ID {bankAccountDto.SubaccountId} не найден");
            }

            // Обновляем все поля
            bankAccount.AccountNumber = bankAccountDto.AccountNumber;
            bankAccount.BankName = bankAccountDto.BankName;
            bankAccount.BIK = bankAccountDto.BIK;
            bankAccount.CorrespondentAccount = bankAccountDto.CorrespondentAccount;
            bankAccount.SubaccountId = bankAccountDto.SubaccountId;
            bankAccount.Currency = bankAccountDto.Currency;
            bankAccount.IsActive = bankAccountDto.IsActive;
            bankAccount.OpenDate = bankAccountDto.OpenDate;           // Обновляем дату открытия
            bankAccount.CloseDate = bankAccountDto.CloseDate;         // Обновляем дату закрытия
            bankAccount.CloseReason = bankAccountDto.CloseReason;     // Обновляем причину закрытия
            bankAccount.UpdatedAt = DateTime.UtcNow;

            await _bankAccountRepository.UpdateAsync(bankAccount);
            return await MapToDto(bankAccount);
        }

        public async Task<bool> DeleteBankAccountAsync(int id)
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(id);
            if (bankAccount == null) return false;

            // Проверяем, есть ли связанные выписки
            // TODO: Добавить проверку на наличие связанных выписок

            await _bankAccountRepository.DeleteAsync(bankAccount);
            return true;
        }

        private async Task<BankAccountDto> MapToDto(BankAccount bankAccount)
        {
            var subaccount = await _accountRepository.GetByIdAsync(bankAccount.SubaccountId);

            return new BankAccountDto
            {
                Id = bankAccount.Id,
                AccountNumber = bankAccount.AccountNumber,
                BankName = bankAccount.BankName,
                BIK = bankAccount.BIK,
                CorrespondentAccount = bankAccount.CorrespondentAccount,
                SubaccountId = bankAccount.SubaccountId,
                SubaccountCode = subaccount?.Code,
                SubaccountName = subaccount?.Name,
                Currency = bankAccount.Currency,
                IsActive = bankAccount.IsActive,
                OpenDate = bankAccount.OpenDate,           // Добавляем дату открытия
                CloseDate = bankAccount.CloseDate,         // Добавляем дату закрытия
                CloseReason = bankAccount.CloseReason,     // Добавляем причину закрытия
                CreatedAt = bankAccount.CreatedAt,
                UpdatedAt = bankAccount.UpdatedAt
            };
        }
    }
}