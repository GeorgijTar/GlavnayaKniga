using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.WPF.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class BankAccountsViewModel : BaseViewModel
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAccountService _accountService;
        private readonly IBikService _bikService; // Добавляем поле

        [ObservableProperty]
        private ObservableCollection<BankAccountDto> _bankAccounts;

        [ObservableProperty]
        private BankAccountDto? _selectedBankAccount;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _accounts;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public BankAccountsViewModel(
            IBankAccountService bankAccountService,
            IAccountService accountService,
            IBikService bikService) // Добавляем параметр в конструктор
        {
            _bankAccountService = bankAccountService;
            _accountService = accountService;
            _bikService = bikService; // Сохраняем сервис
            _bankAccounts = new ObservableCollection<BankAccountDto>();
            _accounts = new ObservableCollection<AccountDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка банковских счетов...";

                var accounts = await _accountService.GetAllAccountsAsync(false);
                Accounts.Clear();
                foreach (var account in accounts.Where(a => a.Code.StartsWith("51")).OrderBy(a => a.Code))
                {
                    Accounts.Add(account);
                }

                await LoadBankAccountsAsync();

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadBankAccountsAsync()
        {
            var accounts = await _bankAccountService.GetAllBankAccountsAsync();

            BankAccounts.Clear();
            foreach (var account in accounts.OrderBy(a => a.AccountNumber))
            {
                BankAccounts.Add(account);
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        private async void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadBankAccountsAsync();
                return;
            }

            var allAccounts = await _bankAccountService.GetAllBankAccountsAsync();
            var filtered = allAccounts.Where(a =>
                a.AccountNumber.Contains(SearchText) ||
                (a.BankName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (a.SubaccountCode?.Contains(SearchText) ?? false));

            BankAccounts.Clear();
            foreach (var account in filtered)
            {
                BankAccounts.Add(account);
            }
        }

        [RelayCommand]
        private async Task AddBankAccountAsync()
        {
            try
            {
                var window = new BankAccountEditWindow();
                // Передаем все три сервиса в конструктор
                var viewModel = new BankAccountEditViewModel(
                    _bankAccountService,
                    _accountService,
                    _bikService, // Добавляем bikService
                    null,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadBankAccountsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditBankAccountAsync()
        {
            try
            {
                if (SelectedBankAccount == null)
                {
                    MessageBox.Show("Выберите банковский счет для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var accountToEdit = await _bankAccountService.GetBankAccountByIdAsync(SelectedBankAccount.Id);
                if (accountToEdit == null)
                {
                    MessageBox.Show("Банковский счет не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new BankAccountEditWindow();
                // Передаем все три сервиса в конструктор
                var viewModel = new BankAccountEditViewModel(
                    _bankAccountService,
                    _accountService,
                    _bikService, // Добавляем bikService
                    accountToEdit,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadBankAccountsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteBankAccountAsync()
        {
            try
            {
                if (SelectedBankAccount == null)
                {
                    MessageBox.Show("Выберите банковский счет для удаления", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить банковский счет {SelectedBankAccount.AccountNumber}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Удаление банковского счета...";

                    var success = await _bankAccountService.DeleteBankAccountAsync(SelectedBankAccount.Id);

                    if (success)
                    {
                        StatusMessage = "Банковский счет успешно удален";
                        await LoadBankAccountsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadBankAccountsAsync();
        }
    }
}