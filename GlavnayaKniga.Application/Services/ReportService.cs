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
    public class ReportService : IReportService
    {
        private readonly IRepository<Entry> _entryRepository;
        private readonly IRepository<Account> _accountRepository;
        private readonly IEntryService _entryService;

        public ReportService(
            IRepository<Entry> entryRepository,
            IRepository<Account> accountRepository,
            IEntryService entryService)
        {
            _entryRepository = entryRepository;
            _accountRepository = accountRepository;
            _entryService = entryService;
        }

        public async Task<AccountAnalysisDto> GetAccountAnalysisAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new InvalidOperationException($"Счет с ID {accountId} не найден");

            // Получаем все проводки за период, где участвует счет
            var entries = await _entryRepository.FindAsync(e =>
                e.Date.Date >= startDate.Date &&
                e.Date.Date <= endDate.Date &&
                (e.DebitAccountId == accountId || e.CreditAccountId == accountId));

            var entryList = entries.ToList();

            // Загружаем корреспондирующие счета
            foreach (var entry in entryList)
            {
                if (entry.DebitAccount == null && entry.DebitAccountId != accountId)
                    entry.DebitAccount = await _accountRepository.GetByIdAsync(entry.DebitAccountId);

                if (entry.CreditAccount == null && entry.CreditAccountId != accountId)
                    entry.CreditAccount = await _accountRepository.GetByIdAsync(entry.CreditAccountId);
            }

            // Вычисляем начальное сальдо (на дату начала периода)
            var openingBalance = await _entryService.GetAccountBalanceAsync(accountId, startDate.AddDays(-1));

            // Группируем по корреспондирующим счетам
            var correspondingAccounts = new Dictionary<int, CorrespondingAccountDto>();

            foreach (var entry in entryList)
            {
                if (entry.DebitAccountId == accountId)
                {
                    // По дебету анализируемого счета - кредитуется кор. счет
                    var corrAccountId = entry.CreditAccountId;
                    if (!correspondingAccounts.ContainsKey(corrAccountId))
                    {
                        correspondingAccounts[corrAccountId] = new CorrespondingAccountDto
                        {
                            AccountId = corrAccountId,
                            AccountCode = entry.CreditAccount?.Code ?? "",
                            AccountName = entry.CreditAccount?.Name ?? ""
                        };
                    }
                    correspondingAccounts[corrAccountId].DebitAmount += entry.Amount;
                }
                else if (entry.CreditAccountId == accountId)
                {
                    // По кредиту анализируемого счета - дебетуется кор. счет
                    var corrAccountId = entry.DebitAccountId;
                    if (!correspondingAccounts.ContainsKey(corrAccountId))
                    {
                        correspondingAccounts[corrAccountId] = new CorrespondingAccountDto
                        {
                            AccountId = corrAccountId,
                            AccountCode = entry.DebitAccount?.Code ?? "",
                            AccountName = entry.DebitAccount?.Name ?? ""
                        };
                    }
                    correspondingAccounts[corrAccountId].CreditAmount += entry.Amount;
                }
            }

            var result = new AccountAnalysisDto
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                StartDate = startDate,
                EndDate = endDate,
                OpeningBalance = openingBalance,
                CorrespondingAccounts = correspondingAccounts.Values
                    .OrderBy(c => c.AccountCode)
                    .ToList(),
                TotalDebitTurnover = entryList.Where(e => e.DebitAccountId == accountId).Sum(e => e.Amount),
                TotalCreditTurnover = entryList.Where(e => e.CreditAccountId == accountId).Sum(e => e.Amount),
                ClosingBalance = openingBalance +
                    entryList.Where(e => e.DebitAccountId == accountId).Sum(e => e.Amount) -
                    entryList.Where(e => e.CreditAccountId == accountId).Sum(e => e.Amount)
            };

            return result;
        }

        public async Task<List<AccountAnalysisDto>> GetAccountAnalysisWithSubaccountsAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var result = new List<AccountAnalysisDto>();

            // Получаем сам счет и все его подчиненные счета
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new InvalidOperationException($"Счет с ID {accountId} не найден");

            var allAccounts = await _accountRepository.GetAllAsync();
            var subaccountIds = GetSubaccountIds(allAccounts, accountId).ToList();
            subaccountIds.Add(accountId); // Добавляем сам счет

            // Получаем анализ для каждого счета в иерархии
            foreach (var subaccountId in subaccountIds.OrderBy(id =>
            {
                var acc = allAccounts.FirstOrDefault(a => a.Id == id);
                return acc?.Code ?? "";
            }))
            {
                var analysis = await GetAccountAnalysisAsync(subaccountId, startDate, endDate);
                result.Add(analysis);
            }

            // Строим иерархию
            var accountDict = result.ToDictionary(a => a.AccountId);

            foreach (var analysis in result)
            {
                var acc = allAccounts.FirstOrDefault(a => a.Id == analysis.AccountId);
                if (acc?.ParentId != null && accountDict.ContainsKey(acc.ParentId.Value))
                {
                    accountDict[acc.ParentId.Value].Children.Add(analysis);
                }
            }

            // Возвращаем только корневые элементы (начиная с запрошенного счета)
            return result.Where(a => a.AccountId == accountId).ToList();
        }

        private IEnumerable<int> GetSubaccountIds(IEnumerable<Account> accounts, int parentId)
        {
            var children = accounts.Where(a => a.ParentId == parentId);
            foreach (var child in children)
            {
                yield return child.Id;
                foreach (var grandChild in GetSubaccountIds(accounts, child.Id))
                {
                    yield return grandChild;
                }
            }
        }

        public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime startDate, DateTime endDate, int? accountId = null)
        {
            var accounts = await _accountRepository.GetAllAsync();

            // Фильтруем счета, если указан конкретный счет
            var filteredAccounts = accounts.AsEnumerable();
            if (accountId.HasValue)
            {
                var account = accounts.FirstOrDefault(a => a.Id == accountId.Value);
                if (account != null)
                {
                    var accountIds = GetSubaccountIds(accounts, accountId.Value).ToList();
                    accountIds.Add(accountId.Value);
                    filteredAccounts = accounts.Where(a => accountIds.Contains(a.Id));
                }
            }

            var result = new BalanceSheetDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Rows = new List<BalanceSheetRowDto>()
            };

            foreach (var account in filteredAccounts.OrderBy(a => a.Code))
            {
                // Начальное сальдо
                var openingBalance = await _entryService.GetAccountBalanceAsync(account.Id, startDate.AddDays(-1));

                // Обороты за период
                var entries = await _entryRepository.FindAsync(e =>
                    e.Date.Date >= startDate.Date &&
                    e.Date.Date <= endDate.Date &&
                    (e.DebitAccountId == account.Id || e.CreditAccountId == account.Id));

                var entryList = entries.ToList();
                var debitTurnover = entryList.Where(e => e.DebitAccountId == account.Id).Sum(e => e.Amount);
                var creditTurnover = entryList.Where(e => e.CreditAccountId == account.Id).Sum(e => e.Amount);

                // Конечное сальдо
                var closingBalance = openingBalance + debitTurnover - creditTurnover;

                var row = new BalanceSheetRowDto
                {
                    AccountId = account.Id,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Level = GetAccountLevel(account, accounts),
                    OpeningBalanceDebit = openingBalance > 0 ? openingBalance : 0,
                    OpeningBalanceCredit = openingBalance < 0 ? -openingBalance : 0,
                    TurnoverDebit = debitTurnover,
                    TurnoverCredit = creditTurnover,
                    ClosingBalanceDebit = closingBalance > 0 ? closingBalance : 0,
                    ClosingBalanceCredit = closingBalance < 0 ? -closingBalance : 0
                };

                result.Rows.Add(row);

                // Обновляем итоги
                result.TotalOpeningBalanceDebit += row.OpeningBalanceDebit;
                result.TotalOpeningBalanceCredit += row.OpeningBalanceCredit;
                result.TotalTurnoverDebit += row.TurnoverDebit;
                result.TotalTurnoverCredit += row.TurnoverCredit;
                result.TotalClosingBalanceDebit += row.ClosingBalanceDebit;
                result.TotalClosingBalanceCredit += row.ClosingBalanceCredit;
            }

            // Строим иерархию
            var rowDict = result.Rows.ToDictionary(r => r.AccountId);
            foreach (var row in result.Rows)
            {
                var account = accounts.FirstOrDefault(a => a.Id == row.AccountId);
                if (account?.ParentId != null && rowDict.ContainsKey(account.ParentId.Value))
                {
                    rowDict[account.ParentId.Value].Children.Add(row);
                }
            }

            // Оставляем только корневые элементы
            result.Rows = result.Rows.Where(r =>
            {
                var account = accounts.FirstOrDefault(a => a.Id == r.AccountId);
                return account?.ParentId == null;
            }).ToList();

            return result;
        }

        public async Task<List<GeneralLedgerEntryDto>> GetGeneralLedgerAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var entries = await _entryRepository.FindAsync(e =>
                e.Date.Date >= startDate.Date &&
                e.Date.Date <= endDate.Date &&
                (e.DebitAccountId == accountId || e.CreditAccountId == accountId));

            var entryList = entries.OrderBy(e => e.Date).ToList();

            // Загружаем корреспондирующие счета
            foreach (var entry in entryList)
            {
                if (entry.DebitAccountId == accountId)
                {
                    entry.CreditAccount ??= await _accountRepository.GetByIdAsync(entry.CreditAccountId);
                }
                else
                {
                    entry.DebitAccount ??= await _accountRepository.GetByIdAsync(entry.DebitAccountId);
                }
            }

            var result = new List<GeneralLedgerEntryDto>();
            var runningBalance = await _entryService.GetAccountBalanceAsync(accountId, startDate.AddDays(-1));

            foreach (var entry in entryList)
            {
                var isDebit = entry.DebitAccountId == accountId;
                var amount = isDebit ? entry.Amount : entry.Amount;

                if (isDebit)
                    runningBalance += amount;
                else
                    runningBalance -= amount;

                result.Add(new GeneralLedgerEntryDto
                {
                    Date = entry.Date,
                    DocumentNumber = "", // Можно добавить номер документа если есть
                    Operation = entry.Note ?? "",
                    CorrespondingAccountId = isDebit ? entry.CreditAccountId : entry.DebitAccountId,
                    CorrespondingAccountCode = isDebit ? entry.CreditAccount?.Code : entry.DebitAccount?.Code,
                    DebitAmount = isDebit ? amount : 0,
                    CreditAmount = isDebit ? 0 : amount,
                    Balance = runningBalance
                });
            }

            return result;
        }

        private int GetAccountLevel(Account account, IEnumerable<Account> allAccounts)
        {
            var level = 0;
            var current = account;
            while (current.ParentId != null)
            {
                level++;
                current = allAccounts.FirstOrDefault(a => a.Id == current.ParentId);
                if (current == null) break;
            }
            return level;
        }
    }
}