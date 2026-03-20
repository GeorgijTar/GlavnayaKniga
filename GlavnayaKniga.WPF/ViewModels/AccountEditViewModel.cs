using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class AccountEditViewModel : BaseViewModel
    {
        private readonly IAccountService _accountService;
        private readonly AccountDto? _originalAccount;
        private readonly Window _window;

        [ObservableProperty]
        private AccountDto _account;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _parentAccounts;

        [ObservableProperty]
        private AccountDto? _selectedParentAccount;

        [ObservableProperty]
        private ObservableCollection<AccountType> _accountTypes;

        [ObservableProperty]
        private AccountType _selectedAccountType;

        [ObservableProperty]
        private bool _isSynthetic;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public AccountEditViewModel(
            IAccountService accountService,
            AccountDto? accountToEdit,
            Window window)
        {
            _accountService = accountService;
            _originalAccount = accountToEdit;
            _window = window;

            _parentAccounts = new ObservableCollection<AccountDto>();
            _accountTypes = new ObservableCollection<AccountType>(Enum.GetValues<AccountType>());

            // Отладка: выводим все доступные типы
            Debug.WriteLine("Доступные типы счетов:");
            foreach (var type in _accountTypes)
            {
                Debug.WriteLine($"  - {type} (значение: {(int)type})");
            }

            if (_originalAccount != null)
            {
                // Редактирование
                _account = new AccountDto
                {
                    Id = _originalAccount.Id,
                    Code = _originalAccount.Code,
                    Name = _originalAccount.Name,
                    ParentId = _originalAccount.ParentId,
                    Type = _originalAccount.Type,
                    IsSynthetic = _originalAccount.IsSynthetic
                };

                // Отладка: проверяем тип счета из оригинального объекта
                Debug.WriteLine($"Загружен счет для редактирования:");
                Debug.WriteLine($"  - Тип (enum): {_originalAccount.Type}");
                Debug.WriteLine($"  - Тип (int): {(int)_originalAccount.Type}");
                Debug.WriteLine($"  - Тип (string): {_originalAccount.Type.ToString()}");

                // Устанавливаем выбранный тип счета
                _selectedAccountType = _originalAccount.Type;
                _isSynthetic = _originalAccount.IsSynthetic;
                Title = "Редактирование счета";
                IsEditMode = true;
            }
            else
            {
                // Новый счет
                _account = new AccountDto();
                _selectedAccountType = AccountType.ActivePassive; // По умолчанию активно-пассивный
                _account.Type = AccountType.ActivePassive;
                _isSynthetic = false; // По умолчанию субсчет
                Title = "Добавление счета";
                IsEditMode = false;

                Debug.WriteLine("Создан новый счет:");
                Debug.WriteLine($"  - Тип по умолчанию: {_selectedAccountType}");
            }

            _ = LoadParentAccountsAsync();
        }

        private async Task LoadParentAccountsAsync()
        {
            try
            {
                IsBusy = true;
                var accounts = await _accountService.GetAvailableParentAccountsAsync(_originalAccount?.Id);

                ParentAccounts.Clear();

                // Добавляем пустой элемент для возможности сделать счет корневым
                ParentAccounts.Add(new AccountDto
                {
                    Id = 0,
                    Code = "",
                    Name = "Корневой счет (счет первого порядка)",
                    IsSynthetic = true
                });

                foreach (var account in accounts.OrderBy(a => a.Code))
                {
                    ParentAccounts.Add(account);
                }

                if (_originalAccount?.ParentId.HasValue == true)
                {
                    SelectedParentAccount = ParentAccounts.FirstOrDefault(p => p.Id == _originalAccount.ParentId.Value);
                }
                else
                {
                    SelectedParentAccount = ParentAccounts.FirstOrDefault(p => p.Id == 0);

                    // Если выбран корневой счет, автоматически устанавливаем флаг синтетического счета
                    if (SelectedParentAccount?.Id == 0)
                    {
                        IsSynthetic = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка загрузки счетов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedParentAccountChanged(AccountDto? value)
        {
            // Если выбран корневой счет, счет становится синтетическим
            if (value?.Id == 0)
            {
                IsSynthetic = true;
            }
            else
            {
                // Если выбран родительский счет, то текущий счет - субсчет
                IsSynthetic = false;

                // Для субсчетов тип наследуется от родительского, если родитель существует
                if (value != null && value.Id > 0)
                {
                    SelectedAccountType = value.Type;
                    Account.Type = value.Type;

                    Debug.WriteLine($"Унаследован тип от родителя: {value.Type}");
                }
            }
        }

        partial void OnSelectedAccountTypeChanged(AccountType value)
        {
            // Синхронизируем с объектом Account
            Account.Type = value;
            Debug.WriteLine($"Изменен тип счета на: {value}");
        }

        partial void OnIsSyntheticChanged(bool value)
        {
            Account.IsSynthetic = value;

            // Если счет синтетический, он не может иметь родителя (кроме корневого)
            if (value && SelectedParentAccount?.Id != 0)
            {
                SelectedParentAccount = ParentAccounts.FirstOrDefault(p => p.Id == 0);
            }

            Debug.WriteLine($"IsSynthetic изменен на: {value}");
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Account.Code))
                {
                    MessageBox.Show(_window, "Введите код счета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Account.Name))
                {
                    MessageBox.Show(_window, "Введите наименование счета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка уникальности кода
                if (!await _accountService.IsCodeUniqueAsync(Account.Code, Account.Id > 0 ? Account.Id : null))
                {
                    MessageBox.Show(_window, $"Счет с кодом {Account.Code} уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка типа счета для синтетических счетов
                if (IsSynthetic && SelectedParentAccount?.Id != 0)
                {
                    MessageBox.Show(_window, "Синтетический счет (первого порядка) должен быть корневым", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Устанавливаем родителя
                Account.ParentId = SelectedParentAccount?.Id > 0 ? SelectedParentAccount.Id : null;

                // Убеждаемся, что тип счета установлен
                Account.Type = SelectedAccountType;
                Account.IsSynthetic = IsSynthetic;

                Debug.WriteLine($"Сохранение счета:");
                Debug.WriteLine($"  - Код: {Account.Code}");
                Debug.WriteLine($"  - Тип: {Account.Type}");
                Debug.WriteLine($"  - IsSynthetic: {Account.IsSynthetic}");
                Debug.WriteLine($"  - ParentId: {Account.ParentId}");

                if (Account.Id > 0)
                {
                    // Редактирование
                    await _accountService.UpdateAccountAsync(Account);
                    MessageBox.Show(_window, "Счет успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _accountService.CreateAccountAsync(Account);
                    MessageBox.Show(_window, "Счет успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}