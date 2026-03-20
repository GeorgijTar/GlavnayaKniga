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
    public class CounterpartyService : ICounterpartyService
    {
        private readonly IRepository<Counterparty> _counterpartyRepository;
        private readonly IRepository<CounterpartyBankAccount> _bankAccountRepository;

        public CounterpartyService(
            IRepository<Counterparty> counterpartyRepository,
            IRepository<CounterpartyBankAccount> bankAccountRepository)
        {
            _counterpartyRepository = counterpartyRepository;
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<IEnumerable<CounterpartyDto>> GetAllCounterpartiesAsync(bool includeArchived = false)
        {
            var counterparties = await _counterpartyRepository.FindAsync(c => includeArchived || !c.IsArchived);
            var result = new List<CounterpartyDto>();

            foreach (var counterparty in counterparties.OrderBy(c => c.ShortName))
            {
                var dto = await MapToDto(counterparty);
                result.Add(dto);
            }

            return result;
        }

        public async Task<CounterpartyDto?> GetCounterpartyByIdAsync(int id)
        {
            var counterparty = await _counterpartyRepository.GetByIdAsync(id);
            return counterparty != null ? await MapToDto(counterparty) : null;
        }

        public async Task<CounterpartyDto?> GetCounterpartyByINNAsync(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn)) return null;

            var counterparties = await _counterpartyRepository.FindAsync(c => c.INN == inn);
            var counterparty = counterparties.FirstOrDefault();

            return counterparty != null ? await MapToDto(counterparty) : null;
        }

        public async Task<IEnumerable<CounterpartyDto>> SearchCounterpartiesAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllCounterpartiesAsync(includeArchived);

            var counterparties = await _counterpartyRepository.FindAsync(c =>
                (includeArchived || !c.IsArchived) &&
                (c.ShortName.Contains(searchText) ||
                 c.FullName.Contains(searchText) ||
                 (c.INN != null && c.INN.Contains(searchText)) ||
                 (c.Email != null && c.Email.Contains(searchText))));

            var result = new List<CounterpartyDto>();
            foreach (var counterparty in counterparties.OrderBy(c => c.ShortName))
            {
                var dto = await MapToDto(counterparty);
                result.Add(dto);
            }

            return result;
        }

        public async Task<CounterpartyDto> CreateCounterpartyAsync(CounterpartyDto counterpartyDto)
        {
            // Проверяем уникальность ИНН
            if (!string.IsNullOrWhiteSpace(counterpartyDto.INN))
            {
                if (!await IsINNUniqueAsync(counterpartyDto.INN))
                {
                    throw new InvalidOperationException($"Контрагент с ИНН {counterpartyDto.INN} уже существует");
                }
            }

            var counterparty = new Counterparty
            {
                FullName = counterpartyDto.FullName,
                ShortName = counterpartyDto.ShortName,
                INN = counterpartyDto.INN,
                KPP = counterpartyDto.KPP,
                OGRN = counterpartyDto.OGRN,
                Type = ParseCounterpartyType(counterpartyDto.Type),
                LegalAddress = counterpartyDto.LegalAddress,
                ActualAddress = counterpartyDto.ActualAddress,
                Phone = counterpartyDto.Phone,
                Email = counterpartyDto.Email,
                ContactPerson = counterpartyDto.ContactPerson,
                Note = counterpartyDto.Note,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _counterpartyRepository.AddAsync(counterparty);

            // Если есть банковские счета, добавляем их
            if (counterpartyDto.BankAccounts != null && counterpartyDto.BankAccounts.Any())
            {
                foreach (var bankAccountDto in counterpartyDto.BankAccounts)
                {
                    bankAccountDto.CounterpartyId = created.Id;
                    await AddBankAccountAsync(bankAccountDto);
                }
            }

            return await MapToDto(created);
        }

        public async Task<CounterpartyDto> UpdateCounterpartyAsync(CounterpartyDto counterpartyDto)
        {
            var counterparty = await _counterpartyRepository.GetByIdAsync(counterpartyDto.Id);
            if (counterparty == null)
            {
                throw new InvalidOperationException($"Контрагент с ID {counterpartyDto.Id} не найден");
            }

            // Проверяем уникальность ИНН (если он изменился)
            if (counterparty.INN != counterpartyDto.INN && !string.IsNullOrWhiteSpace(counterpartyDto.INN))
            {
                if (!await IsINNUniqueAsync(counterpartyDto.INN, counterpartyDto.Id))
                {
                    throw new InvalidOperationException($"Контрагент с ИНН {counterpartyDto.INN} уже существует");
                }
            }

            counterparty.FullName = counterpartyDto.FullName;
            counterparty.ShortName = counterpartyDto.ShortName;
            counterparty.INN = counterpartyDto.INN;
            counterparty.KPP = counterpartyDto.KPP;
            counterparty.OGRN = counterpartyDto.OGRN;
            counterparty.Type = ParseCounterpartyType(counterpartyDto.Type);
            counterparty.LegalAddress = counterpartyDto.LegalAddress;
            counterparty.ActualAddress = counterpartyDto.ActualAddress;
            counterparty.Phone = counterpartyDto.Phone;
            counterparty.Email = counterpartyDto.Email;
            counterparty.ContactPerson = counterpartyDto.ContactPerson;
            counterparty.Note = counterpartyDto.Note;
            counterparty.UpdatedAt = DateTime.UtcNow;

            await _counterpartyRepository.UpdateAsync(counterparty);
            return await MapToDto(counterparty);
        }

        public async Task<bool> ArchiveCounterpartyAsync(int id)
        {
            var counterparty = await _counterpartyRepository.GetByIdAsync(id);
            if (counterparty == null) return false;

            counterparty.IsArchived = true;
            counterparty.ArchivedAt = DateTime.UtcNow;
            counterparty.UpdatedAt = DateTime.UtcNow;

            await _counterpartyRepository.UpdateAsync(counterparty);
            return true;
        }

        public async Task<bool> UnarchiveCounterpartyAsync(int id)
        {
            var counterparty = await _counterpartyRepository.GetByIdAsync(id);
            if (counterparty == null || !counterparty.IsArchived) return false;

            counterparty.IsArchived = false;
            counterparty.ArchivedAt = null;
            counterparty.UpdatedAt = DateTime.UtcNow;

            await _counterpartyRepository.UpdateAsync(counterparty);
            return true;
        }

        public async Task<bool> DeleteCounterpartyAsync(int id)
        {
            var counterparty = await _counterpartyRepository.GetByIdAsync(id);
            if (counterparty == null) return false;

            // Проверяем, есть ли связанные проводки
            // TODO: Добавить проверку наличия проводок с этим контрагентом

            await _counterpartyRepository.DeleteAsync(counterparty);
            return true;
        }

        public async Task<bool> IsINNUniqueAsync(string inn, int? excludeId = null)
        {
            var counterparties = await _counterpartyRepository.FindAsync(c => c.INN == inn);

            if (excludeId.HasValue)
            {
                return !counterparties.Any(c => c.Id != excludeId.Value);
            }

            return !counterparties.Any();
        }

        // Реализация методов для банковских счетов контрагента
        public async Task<CounterpartyBankAccountDto> AddBankAccountAsync(CounterpartyBankAccountDto bankAccountDto)
        {
            var bankAccount = new CounterpartyBankAccount
            {
                CounterpartyId = bankAccountDto.CounterpartyId,
                AccountNumber = bankAccountDto.AccountNumber,
                BankName = bankAccountDto.BankName,
                BIK = bankAccountDto.BIK,
                CorrespondentAccount = bankAccountDto.CorrespondentAccount,
                Currency = bankAccountDto.Currency,
                IsDefault = bankAccountDto.IsDefault,
                Note = bankAccountDto.Note,
                CreatedAt = DateTime.UtcNow
            };

            // Если это основной счет, сбрасываем флаг у других счетов
            if (bankAccount.IsDefault)
            {
                await ResetDefaultBankAccountAsync(bankAccountDto.CounterpartyId);
            }

            var created = await _bankAccountRepository.AddAsync(bankAccount);
            return MapToDto(created);
        }

        public async Task<CounterpartyBankAccountDto> UpdateBankAccountAsync(CounterpartyBankAccountDto bankAccountDto)
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountDto.Id);
            if (bankAccount == null)
            {
                throw new InvalidOperationException($"Банковский счет с ID {bankAccountDto.Id} не найден");
            }

            // Если это основной счет, сбрасываем флаг у других счетов
            if (bankAccountDto.IsDefault && !bankAccount.IsDefault)
            {
                await ResetDefaultBankAccountAsync(bankAccountDto.CounterpartyId);
            }

            bankAccount.AccountNumber = bankAccountDto.AccountNumber;
            bankAccount.BankName = bankAccountDto.BankName;
            bankAccount.BIK = bankAccountDto.BIK;
            bankAccount.CorrespondentAccount = bankAccountDto.CorrespondentAccount;
            bankAccount.Currency = bankAccountDto.Currency;
            bankAccount.IsDefault = bankAccountDto.IsDefault;
            bankAccount.Note = bankAccountDto.Note;
            bankAccount.UpdatedAt = DateTime.UtcNow;

            await _bankAccountRepository.UpdateAsync(bankAccount);
            return MapToDto(bankAccount);
        }

        public async Task<bool> DeleteBankAccountAsync(int id)
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(id);
            if (bankAccount == null) return false;

            await _bankAccountRepository.DeleteAsync(bankAccount);
            return true;
        }

        public async Task<IEnumerable<CounterpartyBankAccountDto>> GetBankAccountsAsync(int counterpartyId)
        {
            var accounts = await _bankAccountRepository.FindAsync(a => a.CounterpartyId == counterpartyId);
            return accounts.OrderByDescending(a => a.IsDefault).ThenBy(a => a.BankName).Select(MapToDto);
        }

        // Вспомогательные методы
        private async Task ResetDefaultBankAccountAsync(int counterpartyId)
        {
            var defaultAccounts = await _bankAccountRepository.FindAsync(a =>
                a.CounterpartyId == counterpartyId && a.IsDefault);

            foreach (var account in defaultAccounts)
            {
                account.IsDefault = false;
                await _bankAccountRepository.UpdateAsync(account);
            }
        }

        private CounterpartyType ParseCounterpartyType(string type)
        {
            return type switch
            {
                "Юридическое лицо" => CounterpartyType.LegalEntity,
                "Индивидуальный предприниматель" => CounterpartyType.IndividualEntrepreneur,
                "Физическое лицо" => CounterpartyType.Individual,
                _ => CounterpartyType.LegalEntity
            };
        }

        private async Task<CounterpartyDto> MapToDto(Counterparty counterparty)
        {
            var bankAccounts = await GetBankAccountsAsync(counterparty.Id);

            return new CounterpartyDto
            {
                Id = counterparty.Id,
                FullName = counterparty.FullName,
                ShortName = counterparty.ShortName,
                INN = counterparty.INN,
                KPP = counterparty.KPP,
                OGRN = counterparty.OGRN,
                Type = counterparty.Type switch
                {
                    CounterpartyType.LegalEntity => "Юридическое лицо",
                    CounterpartyType.IndividualEntrepreneur => "Индивидуальный предприниматель",
                    CounterpartyType.Individual => "Физическое лицо",
                    _ => "Юридическое лицо"
                },
                LegalAddress = counterparty.LegalAddress,
                ActualAddress = counterparty.ActualAddress,
                Phone = counterparty.Phone,
                Email = counterparty.Email,
                ContactPerson = counterparty.ContactPerson,
                Note = counterparty.Note,
                IsArchived = counterparty.IsArchived,
                CreatedAt = counterparty.CreatedAt,
                UpdatedAt = counterparty.UpdatedAt,
                ArchivedAt = counterparty.ArchivedAt,
                BankAccounts = bankAccounts.ToList()
            };
        }

        private CounterpartyBankAccountDto MapToDto(CounterpartyBankAccount account)
        {
            return new CounterpartyBankAccountDto
            {
                Id = account.Id,
                CounterpartyId = account.CounterpartyId,
                AccountNumber = account.AccountNumber,
                BankName = account.BankName,
                BIK = account.BIK,
                CorrespondentAccount = account.CorrespondentAccount,
                Currency = account.Currency,
                IsDefault = account.IsDefault,
                Note = account.Note,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            };
        }


        public async Task<CounterpartyBankAccountDto?> GetBankAccountByNumberAsync(string accountNumber, int counterpartyId)
        {
            var accounts = await _bankAccountRepository.FindAsync(a =>
                a.CounterpartyId == counterpartyId && a.AccountNumber == accountNumber);

            var account = accounts.FirstOrDefault();
            return account != null ? MapToDto(account) : null;
        }

        public async Task<bool> IsBankAccountExistsAsync(string accountNumber, int counterpartyId)
        {
            var accounts = await _bankAccountRepository.FindAsync(a =>
                a.CounterpartyId == counterpartyId && a.AccountNumber == accountNumber);

            return accounts.Any();
        }
    }
}