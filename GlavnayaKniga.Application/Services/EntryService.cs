using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class EntryService : IEntryService
    {
        private readonly IRepository<Entry> _entryRepository;
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<TransactionBasis> _basisRepository;

        public EntryService(
            IRepository<Entry> entryRepository,
            IRepository<Account> accountRepository,
            IRepository<TransactionBasis> basisRepository)
        {
            _entryRepository = entryRepository;
            _accountRepository = accountRepository;
            _basisRepository = basisRepository;
        }

        public async Task<IEnumerable<EntryDto>> GetAllEntriesAsync()
        {
            var entries = await _entryRepository.GetAllAsync();

            // Загружаем связанные данные
            var entryList = entries.ToList();
            foreach (var entry in entryList)
            {
                await LoadEntryReferences(entry);
            }

            return entryList.OrderByDescending(e => e.Date).Select(MapToDto);
        }

        public async Task<EntryDto?> GetEntryByIdAsync(int id)
        {
            var entry = await _entryRepository.GetByIdAsync(id);
            if (entry == null) return null;

            await LoadEntryReferences(entry);
            return MapToDto(entry);
        }

        public async Task<IEnumerable<EntryDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var entries = await _entryRepository.FindAsync(e =>
                e.Date.Date >= startDate.Date && e.Date.Date <= endDate.Date);

            var entryList = entries.ToList();
            foreach (var entry in entryList)
            {
                await LoadEntryReferences(entry);
            }

            return entryList.OrderByDescending(e => e.Date).Select(MapToDto);
        }

        public async Task<IEnumerable<EntryDto>> GetEntriesByAccountAsync(int accountId)
        {
            var entries = await _entryRepository.FindAsync(e =>
                e.DebitAccountId == accountId || e.CreditAccountId == accountId);

            var entryList = entries.ToList();
            foreach (var entry in entryList)
            {
                await LoadEntryReferences(entry);
            }

            return entryList.OrderByDescending(e => e.Date).Select(MapToDto);
        }

        public async Task<EntryDto> CreateEntryAsync(EntryDto entryDto)
        {
            // Валидация
            if (entryDto.DebitAccountId == entryDto.CreditAccountId)
            {
                throw new InvalidOperationException("Дебетуемый и кредитуемый счета должны быть разными");
            }

            if (entryDto.Amount <= 0)
            {
                throw new InvalidOperationException("Сумма проводки должна быть положительной");
            }

            // Проверяем существование счетов и что они не архивные
            var debitAccount = await _accountRepository.GetByIdAsync(entryDto.DebitAccountId);
            if (debitAccount == null || debitAccount.IsArchived)
            {
                throw new InvalidOperationException("Дебетуемый счет не найден или архивный");
            }

            var creditAccount = await _accountRepository.GetByIdAsync(entryDto.CreditAccountId);
            if (creditAccount == null || creditAccount.IsArchived)
            {
                throw new InvalidOperationException("Кредитуемый счет не найден или архивный");
            }

            // Проверяем существование основания
            var basis = await _basisRepository.GetByIdAsync(entryDto.BasisId);
            if (basis == null)
            {
                throw new InvalidOperationException("Основание проводки не найдено");
            }

            var entry = new Entry
            {
                Date = entryDto.Date,
                DebitAccountId = entryDto.DebitAccountId,
                CreditAccountId = entryDto.CreditAccountId,
                Amount = entryDto.Amount,
                BasisId = entryDto.BasisId,
                Note = entryDto.Note,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _entryRepository.AddAsync(entry);
            await LoadEntryReferences(created);

            return MapToDto(created);
        }

        public async Task<EntryDto> UpdateEntryAsync(EntryDto entryDto)
        {
            var entry = await _entryRepository.GetByIdAsync(entryDto.Id);
            if (entry == null)
            {
                throw new InvalidOperationException($"Проводка с ID {entryDto.Id} не найдена");
            }

            // Валидация
            if (entryDto.DebitAccountId == entryDto.CreditAccountId)
            {
                throw new InvalidOperationException("Дебетуемый и кредитуемый счета должны быть разными");
            }

            if (entryDto.Amount <= 0)
            {
                throw new InvalidOperationException("Сумма проводки должна быть положительной");
            }

            // Проверяем счета
            var debitAccount = await _accountRepository.GetByIdAsync(entryDto.DebitAccountId);
            if (debitAccount == null || debitAccount.IsArchived)
            {
                throw new InvalidOperationException("Дебетуемый счет не найден или архивный");
            }

            var creditAccount = await _accountRepository.GetByIdAsync(entryDto.CreditAccountId);
            if (creditAccount == null || creditAccount.IsArchived)
            {
                throw new InvalidOperationException("Кредитуемый счет не найден или архивный");
            }

            // Проверяем основание
            var basis = await _basisRepository.GetByIdAsync(entryDto.BasisId);
            if (basis == null)
            {
                throw new InvalidOperationException("Основание проводки не найдено");
            }

            entry.Date = entryDto.Date;
            entry.DebitAccountId = entryDto.DebitAccountId;
            entry.CreditAccountId = entryDto.CreditAccountId;
            entry.Amount = entryDto.Amount;
            entry.BasisId = entryDto.BasisId;
            entry.Note = entryDto.Note;
            entry.UpdatedAt = DateTime.UtcNow;

            await _entryRepository.UpdateAsync(entry);
            await LoadEntryReferences(entry);

            return MapToDto(entry);
        }

        public async Task<bool> DeleteEntryAsync(int id)
        {
            var entry = await _entryRepository.GetByIdAsync(id);
            if (entry == null) return false;

            await _entryRepository.DeleteAsync(entry);
            return true;
        }

        public async Task<decimal> GetAccountTurnoverAsync(int accountId, DateTime startDate, DateTime endDate, bool isDebit)
        {
            var entries = await _entryRepository.FindAsync(e =>
                e.Date.Date >= startDate.Date &&
                e.Date.Date <= endDate.Date &&
                (isDebit ? e.DebitAccountId == accountId : e.CreditAccountId == accountId));

            return entries.Sum(e => e.Amount);
        }

        public async Task<decimal> GetAccountBalanceAsync(int accountId, DateTime asOfDate)
        {
            // Сальдо = Дебетовый оборот - Кредитовый оборот (упрощенно)
            var debitTurnover = await GetAccountTurnoverAsync(accountId, DateTime.MinValue, asOfDate, true);
            var creditTurnover = await GetAccountTurnoverAsync(accountId, DateTime.MinValue, asOfDate, false);

            return debitTurnover - creditTurnover;
        }

        private async Task LoadEntryReferences(Entry entry)
        {
            // Явно загружаем связанные данные
            if (entry.DebitAccount == null)
            {
                entry.DebitAccount = await _accountRepository.GetByIdAsync(entry.DebitAccountId);
            }

            if (entry.CreditAccount == null)
            {
                entry.CreditAccount = await _accountRepository.GetByIdAsync(entry.CreditAccountId);
            }

            if (entry.Basis == null)
            {
                entry.Basis = await _basisRepository.GetByIdAsync(entry.BasisId);
            }
        }

        private EntryDto MapToDto(Entry entry)
        {
            return new EntryDto
            {
                Id = entry.Id,
                Date = entry.Date,
                DebitAccountId = entry.DebitAccountId,
                DebitAccountCode = entry.DebitAccount?.Code,
                DebitAccountName = entry.DebitAccount?.Name,
                CreditAccountId = entry.CreditAccountId,
                CreditAccountCode = entry.CreditAccount?.Code,
                CreditAccountName = entry.CreditAccount?.Name,
                Amount = entry.Amount,
                BasisId = entry.BasisId,
                BasisName = entry.Basis?.Name,
                Note = entry.Note,
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt
            };
        }
    }
}