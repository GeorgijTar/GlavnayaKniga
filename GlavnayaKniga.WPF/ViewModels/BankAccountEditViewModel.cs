using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class BankAccountEditViewModel : BaseViewModel
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAccountService _accountService;
        private readonly IBikService _bikService;
        private readonly BankAccountDto? _originalAccount;
        private readonly Window _window;

        [ObservableProperty]
        private BankAccountDto _account;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _subaccounts;

        [ObservableProperty]
        private AccountDto? _selectedSubaccount;

        [ObservableProperty]
        private ObservableCollection<string> _accountTypes;

        [ObservableProperty]
        private string _selectedAccountType;

        [ObservableProperty]
        private ObservableCollection<string> _currencies;

        [ObservableProperty]
        private string _selectedCurrency;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private bool _isAccountClosed;

        [ObservableProperty]
        private string _bikValidationMessage;

        [ObservableProperty]
        private bool _isBikValid;

        [ObservableProperty]
        private bool _isAccountValid;

        [ObservableProperty]
        private string _accountValidationMessage;

        public BankAccountEditViewModel(
            IBankAccountService bankAccountService,
            IAccountService accountService,
            IBikService bikService,
            BankAccountDto? accountToEdit,
            Window window)
        {
            _bankAccountService = bankAccountService;
            _accountService = accountService;
            _bikService = bikService;
            _originalAccount = accountToEdit;
            _window = window;

            _subaccounts = new ObservableCollection<AccountDto>();
            _accountTypes = new ObservableCollection<string>
            {
                "Расчетный счет (51)",
                "Валютный счет (52)",
                "Специальный счет (55)"
            };

            _currencies = new ObservableCollection<string>
            {
                "RUB",
                "USD",
                "EUR"
            };

            if (_originalAccount != null)
            {
                _account = new BankAccountDto
                {
                    Id = _originalAccount.Id,
                    AccountNumber = _originalAccount.AccountNumber,
                    BankName = _originalAccount.BankName,
                    BIK = _originalAccount.BIK,
                    CorrespondentAccount = _originalAccount.CorrespondentAccount,
                    SubaccountId = _originalAccount.SubaccountId,
                    Currency = _originalAccount.Currency,
                    IsActive = _originalAccount.IsActive,
                    OpenDate = _originalAccount.OpenDate,
                    CloseDate = _originalAccount.CloseDate,
                    CloseReason = _originalAccount.CloseReason
                };
                _isAccountClosed = !_originalAccount.IsActive;
                _selectedCurrency = _originalAccount.Currency;
                Title = "Редактирование банковского счета";
                IsEditMode = true;
            }
            else
            {
                _account = new BankAccountDto
                {
                    Currency = "RUB",
                    IsActive = true,
                    OpenDate = DateTime.Today
                };
                _isAccountClosed = false;
                _selectedCurrency = "RUB";
                Title = "Добавление банковского счета";
                IsEditMode = false;
            }

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка данных...";

                // Загружаем все счета
                var allAccounts = await _accountService.GetAllAccountsAsync(false);

                // Определяем тип счета по умолчанию
                if (_originalAccount != null)
                {
                    if (_originalAccount.SubaccountCode?.StartsWith("51") == true)
                        SelectedAccountType = "Расчетный счет (51)";
                    else if (_originalAccount.SubaccountCode?.StartsWith("52") == true)
                        SelectedAccountType = "Валютный счет (52)";
                    else if (_originalAccount.SubaccountCode?.StartsWith("55") == true)
                        SelectedAccountType = "Специальный счет (55)";
                    else
                        SelectedAccountType = "Расчетный счет (51)";
                }
                else
                {
                    SelectedAccountType = "Расчетный счет (51)";
                }

                await LoadSubaccountsByType(SelectedAccountType);

                if (!string.IsNullOrWhiteSpace(Account.BIK))
                {
                    await CheckBikAsync();
                }

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show(_window, $"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadSubaccountsByType(string accountType)
        {
            try
            {
                var allAccounts = await _accountService.GetAllAccountsAsync(false);
                Subaccounts.Clear();

                string accountPrefix = accountType switch
                {
                    "Расчетный счет (51)" => "51",
                    "Валютный счет (52)" => "52",
                    "Специальный счет (55)" => "55",
                    _ => "51"
                };

                var bankSubaccounts = allAccounts
                    .Where(a => a.Code.StartsWith(accountPrefix))
                    .OrderBy(a => a.Code);

                foreach (var account in bankSubaccounts)
                {
                    Subaccounts.Add(account);
                }

                if (_originalAccount != null && Subaccounts.Any())
                {
                    var savedSubaccount = Subaccounts.FirstOrDefault(s => s.Id == _originalAccount.SubaccountId);
                    if (savedSubaccount != null)
                    {
                        SelectedSubaccount = savedSubaccount;
                    }
                    else if (Subaccounts.Any())
                    {
                        SelectedSubaccount = Subaccounts.FirstOrDefault();
                    }
                }
                else if (Subaccounts.Any())
                {
                    SelectedSubaccount = Subaccounts.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка загрузки субсчетов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedAccountTypeChanged(string value)
        {
            _ = LoadSubaccountsByType(value);
        }

        partial void OnSelectedSubaccountChanged(AccountDto? value)
        {
            if (value != null)
            {
                Account.SubaccountId = value.Id;
                Account.SubaccountCode = value.Code;
                Account.SubaccountName = value.Name;

                // Автоматически устанавливаем валюту в зависимости от типа счета
                if (value.Code.StartsWith("52"))
                {
                    SelectedCurrency = "USD";
                    Account.Currency = "USD";
                }
                else
                {
                    SelectedCurrency = "RUB";
                    Account.Currency = "RUB";
                }
            }
        }

        partial void OnSelectedCurrencyChanged(string value)
        {
            if (Account != null)
            {
                Account.Currency = value;
            }
        }

        partial void OnIsAccountClosedChanged(bool value)
        {
            if (value)
            {
                Account.IsActive = false;
                if (Account.CloseDate == null)
                {
                    Account.CloseDate = DateTime.Today;
                }
            }
            else
            {
                Account.IsActive = true;
                Account.CloseDate = null;
                Account.CloseReason = null;
            }
        }

        [RelayCommand]
        private async Task CheckBikAsync()
        {
            if (string.IsNullOrWhiteSpace(Account.BIK) || Account.BIK.Length != 9 || !Account.BIK.All(char.IsDigit))
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
                    Account.BankName = bankInfo.Name;
                    Account.CorrespondentAccount = bankInfo.CorrespondentAccount;

                    OnPropertyChanged(nameof(Account.BankName));
                    OnPropertyChanged(nameof(Account.CorrespondentAccount));

                    BikValidationMessage = $"✓ Банк найден: {bankInfo.ShortName}";
                    IsBikValid = true;

                    if (!string.IsNullOrWhiteSpace(Account.AccountNumber))
                    {
                        CheckAccountNumber();
                    }
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
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void CheckAccountNumber()
        {
            if (string.IsNullOrWhiteSpace(Account.AccountNumber) ||
                string.IsNullOrWhiteSpace(Account.BIK))
            {
                AccountValidationMessage = "Введите БИК и номер счета";
                IsAccountValid = false;
                return;
            }

            if (Account.AccountNumber.Length != 20 || !Account.AccountNumber.All(char.IsDigit))
            {
                AccountValidationMessage = "Номер счета должен содержать 20 цифр";
                IsAccountValid = false;
                return;
            }

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
                AccountValidationMessage = "⚠ Номер счета не прошел проверку по алгоритму ЦБ РФ. Рекомендуется проверить правильность ввода.";
                IsAccountValid = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Account.AccountNumber))
                {
                    MessageBox.Show(_window, "Введите номер расчетного счета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Account.BIK))
                {
                    MessageBox.Show(_window, "Введите БИК банка", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedSubaccount == null)
                {
                    MessageBox.Show(_window, "Выберите субсчет учета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await CheckBikAsync();
                if (!IsBikValid)
                {
                    MessageBox.Show(_window, $"БИК не прошел проверку: {BikValidationMessage}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CheckAccountNumber();
                if (!IsAccountValid)
                {
                    var result = MessageBox.Show(_window,
                        $"Номер счета не прошел проверку по алгоритму ЦБ РФ.\n\n{AccountValidationMessage}\n\nВсё равно сохранить?",
                        "Предупреждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                if (Account.OpenDate.HasValue && Account.CloseDate.HasValue &&
                    Account.OpenDate > Account.CloseDate)
                {
                    MessageBox.Show(_window, "Дата открытия не может быть позже даты закрытия", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Account.IsActive && string.IsNullOrWhiteSpace(Account.CloseReason))
                {
                    MessageBox.Show(_window, "Укажите причину закрытия счета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Убеждаемся, что валюта установлена
                Account.Currency = SelectedCurrency;

                if (Account.Id > 0)
                {
                    await _bankAccountService.UpdateBankAccountAsync(Account);
                    MessageBox.Show(_window, "Банковский счет успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _bankAccountService.CreateBankAccountAsync(Account);
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
    }
}