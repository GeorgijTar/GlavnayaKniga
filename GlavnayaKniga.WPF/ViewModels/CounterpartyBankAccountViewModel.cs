using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class CounterpartyBankAccountViewModel : BaseViewModel
    {
        private readonly ICounterpartyService _counterpartyService;
        private readonly IBikService _bikService;
        private readonly int _counterpartyId;
        private readonly CounterpartyBankAccountDto? _originalAccount;
        private readonly Window _window;

        [ObservableProperty]
        private CounterpartyBankAccountDto _account;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _bikValidationMessage;

        [ObservableProperty]
        private bool _isBikValid;

        [ObservableProperty]
        private string _accountValidationMessage;

        [ObservableProperty]
        private bool _isAccountValid;

        public CounterpartyBankAccountViewModel(
            ICounterpartyService counterpartyService,
            IBikService bikService,
            int counterpartyId,
            CounterpartyBankAccountDto? accountToEdit,
            Window window)
        {
            _counterpartyService = counterpartyService;
            _bikService = bikService;
            _counterpartyId = counterpartyId;
            _originalAccount = accountToEdit;
            _window = window;

            if (_originalAccount != null)
            {
                _account = new CounterpartyBankAccountDto
                {
                    Id = _originalAccount.Id,
                    CounterpartyId = _originalAccount.CounterpartyId,
                    AccountNumber = _originalAccount.AccountNumber,
                    BankName = _originalAccount.BankName,
                    BIK = _originalAccount.BIK,
                    CorrespondentAccount = _originalAccount.CorrespondentAccount,
                    Currency = _originalAccount.Currency,
                    IsDefault = _originalAccount.IsDefault,
                    Note = _originalAccount.Note
                };
                Title = "Редактирование банковского счета";
                IsEditMode = true;

                // Проверяем БИК при загрузке
                if (!string.IsNullOrWhiteSpace(Account.BIK))
                {
                    _ = CheckBikAsync();
                }
            }
            else
            {
                _account = new CounterpartyBankAccountDto
                {
                    CounterpartyId = _counterpartyId,
                    Currency = "RUB",
                    IsDefault = false
                };
                Title = "Добавление банковского счета";
                IsEditMode = false;
            }
        }

        /// <summary>
        /// Проверка БИК и автозаполнение данных банка
        /// </summary>
        [RelayCommand]
        private async Task CheckBikAsync()
        {
            if (string.IsNullOrWhiteSpace(Account.BIK) || Account.BIK.Length != 9 || !IsAllDigits(Account.BIK))
            {
                BikValidationMessage = "БИК должен содержать 9 цифр";
                IsBikValid = false;
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Поиск информации о банке...";

                var bankInfo = await _bikService.GetBankInfoByBikAsync(Account.BIK);

                if (bankInfo != null)
                {
                    var updatedAccount = new CounterpartyBankAccountDto
                    {
                        Id = Account.Id,
                        CounterpartyId = Account.CounterpartyId,
                        AccountNumber = Account.AccountNumber,
                        BankName = bankInfo.Name,
                        BIK = Account.BIK,
                        CorrespondentAccount = bankInfo.CorrespondentAccount,
                        Currency = Account.Currency,
                        IsDefault = Account.IsDefault,
                        Note = Account.Note
                    };

                    Account = updatedAccount; // Это вызовет полное обновление UI

                    BikValidationMessage = $"✓ Банк найден: {bankInfo.ShortName}";
                    IsBikValid = true;

                    if (!string.IsNullOrWhiteSpace(Account.AccountNumber))
                    {
                        CheckAccountNumber();
                    }

                    Debug.WriteLine($"Банк найден: {bankInfo.Name}, КС: {bankInfo.CorrespondentAccount}");
                }
                else
                {
                    BikValidationMessage = "✗ Банк с таким БИК не найден";
                    IsBikValid = false;
                }
            }
            catch (Exception ex)
            {
                BikValidationMessage = $"Ошибка проверки БИК: {ex.Message}";
                IsBikValid = false;
                Debug.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        /// <summary>
        /// Проверка номера счета
        /// </summary>
        [RelayCommand]
        private void CheckAccountNumber()
        {
            if (string.IsNullOrWhiteSpace(Account.AccountNumber))
            {
                AccountValidationMessage = "Введите номер счета";
                IsAccountValid = false;
                return;
            }

            if (Account.AccountNumber.Length != 20 || !IsAllDigits(Account.AccountNumber))
            {
                AccountValidationMessage = "Номер счета должен содержать 20 цифр";
                IsAccountValid = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(Account.BIK))
            {
                AccountValidationMessage = "Сначала введите БИК";
                IsAccountValid = false;
                return;
            }

            // Для счетов контрагентов используем валидацию расчетного счета
            var isValid = _bikService.ValidateAccount(
                Account.AccountNumber,
                Account.BIK,
                Account.CorrespondentAccount);

            if (isValid)
            {
                AccountValidationMessage = "✓ Номер счета корректен";
                IsAccountValid = true;
            }
            else
            {
                AccountValidationMessage = "⚠ Номер счета не прошел проверку по алгоритму ЦБ РФ";
                IsAccountValid = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Account.AccountNumber))
                {
                    MessageBox.Show(_window, "Введите номер счета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Account.AccountNumber.Length != 20 || !IsAllDigits(Account.AccountNumber))
                {
                    MessageBox.Show(_window, "Номер счета должен содержать 20 цифр", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Account.BIK))
                {
                    MessageBox.Show(_window, "Введите БИК", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Account.BIK.Length != 9 || !IsAllDigits(Account.BIK))
                {
                    MessageBox.Show(_window, "БИК должен содержать 9 цифр", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем БИК
                await CheckBikAsync();
                if (!IsBikValid)
                {
                    var result = MessageBox.Show(_window,
                        $"БИК не прошел проверку: {BikValidationMessage}\n\nВсё равно сохранить?",
                        "Предупреждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                // Проверяем номер счета (не блокируем, только предупреждаем)
                CheckAccountNumber();
                if (!IsAccountValid && !string.IsNullOrWhiteSpace(Account.AccountNumber))
                {
                    var result = MessageBox.Show(_window,
                        $"Номер счета не прошел проверку: {AccountValidationMessage}\n\nВсё равно сохранить?",
                        "Предупреждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                if (Account.Id > 0)
                {
                    // Редактирование
                    await _counterpartyService.UpdateBankAccountAsync(Account);
                    MessageBox.Show(_window, "Банковский счет успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _counterpartyService.AddBankAccountAsync(Account);
                    MessageBox.Show(_window, "Банковский счет успешно добавлен", "Успех",
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

        private bool IsAllDigits(string value)
        {
            foreach (char c in value)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
    }
}