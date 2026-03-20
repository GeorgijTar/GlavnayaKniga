using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<Entry> _entryRepository;

        public AccountService(
            IRepository<Account> accountRepository,
            IRepository<Entry> entryRepository)
        {
            _accountRepository = accountRepository;
            _entryRepository = entryRepository;
        }

        public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync(bool includeArchived = false)
        {
            var accounts = await _accountRepository.FindAsync(a =>
                includeArchived || !a.IsArchived);

            return accounts.Select(MapToDto);
        }

        public async Task<AccountDto?> GetAccountByIdAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account != null ? MapToDto(account) : null;
        }

        public async Task<AccountDto?> GetAccountByCodeAsync(string code)
        {
            var accounts = await _accountRepository.FindAsync(a => a.Code == code && !a.IsArchived);
            var account = accounts.FirstOrDefault();
            return account != null ? MapToDto(account) : null;
        }

        public async Task<AccountDto> CreateAccountAsync(AccountDto accountDto)
        {
            // Проверяем уникальность кода только среди неархивных счетов
            if (!await IsCodeUniqueAsync(accountDto.Code))
            {
                throw new InvalidOperationException($"Счет с кодом {accountDto.Code} уже существует");
            }

            // Проверяем, не является ли родительский счет архивным
            if (accountDto.ParentId.HasValue)
            {
                var parent = await _accountRepository.GetByIdAsync(accountDto.ParentId.Value);
                if (parent != null && parent.IsArchived)
                {
                    throw new InvalidOperationException("Нельзя создать дочерний счет у архивного счета");
                }
            }

            var account = new Account
            {
                Code = accountDto.Code,
                Name = accountDto.Name,
                ParentId = accountDto.ParentId,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            // Вычисляем полный код
            if (account.ParentId.HasValue)
            {
                var parent = await _accountRepository.GetByIdAsync(account.ParentId.Value);
                if (parent != null)
                {
                    account.FullCode = $"{parent.FullCode ?? parent.Code}.{account.Code}";
                }
            }
            else
            {
                account.FullCode = account.Code;
            }

            var created = await _accountRepository.AddAsync(account);
            return MapToDto(created);
        }

        public async Task<AccountDto> UpdateAccountAsync(AccountDto accountDto)
        {
            var account = await _accountRepository.GetByIdAsync(accountDto.Id);
            if (account == null)
            {
                throw new InvalidOperationException($"Счет с ID {accountDto.Id} не найден");
            }

            // Нельзя редактировать архивный счет
            if (account.IsArchived)
            {
                throw new InvalidOperationException("Нельзя редактировать архивный счет");
            }

            // Проверяем уникальность кода (исключая текущий счет)
            if (account.Code != accountDto.Code && !await IsCodeUniqueAsync(accountDto.Code, accountDto.Id))
            {
                throw new InvalidOperationException($"Счет с кодом {accountDto.Code} уже существует");
            }

            // Проверяем родительский счет
            if (accountDto.ParentId.HasValue)
            {
                var parent = await _accountRepository.GetByIdAsync(accountDto.ParentId.Value);
                if (parent != null && parent.IsArchived)
                {
                    throw new InvalidOperationException("Нельзя переместить счет в архивный родительский счет");
                }
            }

            account.Code = accountDto.Code;
            account.Name = accountDto.Name;
            account.ParentId = accountDto.ParentId;
            account.UpdatedAt = DateTime.UtcNow;

            // Обновляем полный код
            await UpdateFullCodeAsync(account);

            await _accountRepository.UpdateAsync(account);

            // Обновляем полные коды дочерних счетов
            await UpdateChildrenFullCodes(account);

            return MapToDto(account);
        }

        public async Task<bool> ArchiveAccountAsync(int id)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(id);
                if (account == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Счет с ID {id} не найден");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Найден счет: ID={account.Id}, Code={account.Code}, IsArchived={account.IsArchived}");

                // Проверяем, есть ли дочерние счета
                if (await HasChildrenAsync(id))
                {
                    var children = await _accountRepository.FindAsync(a => a.ParentId == id && !a.IsArchived);
                    if (children.Any())
                    {
                        throw new InvalidOperationException("Нельзя архивировать счет, у которого есть неархивные дочерние счета. Сначала архивируйте дочерние счета.");
                    }
                }

                // Проверяем, использовался ли счет в проводках
                var isUsed = await IsAccountUsedInEntriesAsync(id);
                System.Diagnostics.Debug.WriteLine($"Счет использовался в проводках: {isUsed}");

                // Если использовался - архивируем
                account.IsArchived = true;
                account.ArchivedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;

                await _accountRepository.UpdateAsync(account);
                System.Diagnostics.Debug.WriteLine($"Счет {account.Code} успешно архивирован");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в ArchiveAccountAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UnarchiveAccountAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null || !account.IsArchived)
                return false;

            // Проверяем, не является ли родительский счет архивным
            if (account.ParentId.HasValue)
            {
                var parent = await _accountRepository.GetByIdAsync(account.ParentId.Value);
                if (parent != null && parent.IsArchived)
                {
                    throw new InvalidOperationException("Нельзя разархивировать счет, так как родительский счет архивный");
                }
            }

            account.IsArchived = false;
            account.ArchivedAt = null;
            account.UpdatedAt = DateTime.UtcNow;

            await _accountRepository.UpdateAsync(account);
            return true;
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(id);
                if (account == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Счет с ID {id} не найден для удаления");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Попытка удалить счет: ID={account.Id}, Code={account.Code}, IsArchived={account.IsArchived}");

                // Проверяем, есть ли дочерние счета
                if (await HasChildrenAsync(id))
                {
                    throw new InvalidOperationException("Нельзя удалить счет, у которого есть дочерние счета");
                }

                // Проверяем, есть ли проводки с этим счетом
                if (await IsAccountUsedInEntriesAsync(id))
                {
                    throw new InvalidOperationException("Нельзя удалить счет, по которому есть проводки. Используйте архивацию.");
                }

                // Удаляем только если счет архивный
                if (!account.IsArchived)
                {
                    throw new InvalidOperationException("Можно удалить только архивный счет");
                }

                await _accountRepository.DeleteAsync(account);
                System.Diagnostics.Debug.WriteLine($"Счет {account.Code} успешно удален");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в DeleteAccountAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            // Ищем счета с таким же кодом, включая архивные
            var accounts = await _accountRepository.FindAsync(a => a.Code == code);

            if (excludeId.HasValue)
            {
                // Исключаем текущий счет при редактировании
                return !accounts.Any(a => a.Id != excludeId.Value);
            }

            // При создании нового счета проверяем, есть ли уже такой код (включая архивные)
            return !accounts.Any();
        }

        public async Task<IEnumerable<AccountDto>> GetAccountHierarchyAsync(bool includeArchived = false)
        {
            var allAccounts = await _accountRepository.FindAsync(a =>
                includeArchived || !a.IsArchived);

            var accountDict = allAccounts.ToDictionary(a => a.Id, MapToDto);

            // Строим иерархию
            foreach (var account in allAccounts)
            {
                if (account.ParentId.HasValue && accountDict.ContainsKey(account.ParentId.Value))
                {
                    accountDict[account.ParentId.Value].Children.Add(accountDict[account.Id]);
                }
            }

            // Возвращаем только корневые элементы
            var rootAccounts = allAccounts
                .Where(a => !a.ParentId.HasValue)
                .Select(a => accountDict[a.Id])
                .OrderBy(a => a.Code, new AccountCodeComparer())
                .ToList();

            // Сортируем дочерние элементы
            foreach (var root in rootAccounts)
            {
                SortChildren(root);
            }

            return rootAccounts;
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            var children = await _accountRepository.FindAsync(a => a.ParentId == id && !a.IsArchived);
            return children.Any();
        }

        public async Task<bool> IsAccountUsedInEntriesAsync(int id)
        {
            var entries = await _entryRepository.FindAsync(e =>
                e.DebitAccountId == id || e.CreditAccountId == id);

            return entries.Any();
        }

        public async Task<IEnumerable<AccountDto>> GetAvailableParentAccountsAsync(int? accountId = null)
        {
            var accounts = await _accountRepository.FindAsync(a => !a.IsArchived);

            var result = accounts.Select(MapToDto).ToList();

            // Исключаем сам счет и его потомков
            if (accountId.HasValue)
            {
                var excludedIds = new HashSet<int> { accountId.Value };
                await AddChildIdsAsync(accountId.Value, excludedIds);

                result = result.Where(a => !excludedIds.Contains(a.Id)).ToList();
            }

            return result.OrderBy(a => a.Code, new AccountCodeComparer());
        }

        private async Task AddChildIdsAsync(int parentId, HashSet<int> ids)
        {
            var children = await _accountRepository.FindAsync(a => a.ParentId == parentId);

            foreach (var child in children)
            {
                ids.Add(child.Id);
                await AddChildIdsAsync(child.Id, ids);
            }
        }

        private async Task UpdateFullCodeAsync(Account account)
        {
            if (account.ParentId.HasValue)
            {
                var parent = await _accountRepository.GetByIdAsync(account.ParentId.Value);
                if (parent != null)
                {
                    account.FullCode = $"{parent.FullCode ?? parent.Code}.{account.Code}";
                }
            }
            else
            {
                account.FullCode = account.Code;
            }
        }

        private async Task UpdateChildrenFullCodes(Account parent)
        {
            var children = await _accountRepository.FindAsync(a => a.ParentId == parent.Id);

            foreach (var child in children)
            {
                child.FullCode = $"{parent.FullCode}.{child.Code}";
                await _accountRepository.UpdateAsync(child);
                await UpdateChildrenFullCodes(child);
            }
        }

        private void SortChildren(AccountDto account)
        {
            if (account.Children.Any())
            {
                var sortedChildren = account.Children
                    .OrderBy(c => c.Code, new AccountCodeComparer())
                    .ToList();

                account.Children.Clear();
                foreach (var child in sortedChildren)
                {
                    SortChildren(child);
                    account.Children.Add(child);
                }
            }
        }

        private AccountDto MapToDto(Account account)
        {
            if (account == null) return null;

            return new AccountDto
            {
                Id = account.Id,
                Code = account.Code ?? string.Empty,
                Name = account.Name ?? string.Empty,
                FullCode = account.FullCode,
                ParentId = account.ParentId,
                ParentCode = account.Parent?.Code,
                Type = account.Type, // Убедитесь, что это значение правильно передается
                IsSynthetic = account.IsSynthetic,
                IsArchived = account.IsArchived,
                ArchivedAt = account.ArchivedAt,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            };
        }

        private class AccountCodeComparer : IComparer<string>
        {
            public int Compare(string? x, string? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                var xParts = x.Split('.');
                var yParts = y.Split('.');

                for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
                {
                    if (int.TryParse(xParts[i], out int xNum) && int.TryParse(yParts[i], out int yNum))
                    {
                        if (xNum != yNum)
                            return xNum.CompareTo(yNum);
                    }
                    else
                    {
                        int stringCompare = string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);
                        if (stringCompare != 0)
                            return stringCompare;
                    }
                }

                return xParts.Length.CompareTo(yParts.Length);
            }
        }
    }
}