using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.WPF.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class BankStatementImportViewModel : BaseViewModel
    {
        private readonly IBankStatementService _statementService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IBankStatementParser _parser;
        private readonly IEntryService _entryService;
        private readonly IAccountService _accountService; // Добавляем поле
        private readonly IRepository<TransactionBasis> _basisRepository;

        [ObservableProperty]
        private ObservableCollection<BankStatementDto> _statements;

        [ObservableProperty]
        private BankStatementDto? _selectedStatement;

        [ObservableProperty]
        private ObservableCollection<BankAccountDto> _bankAccounts;

        [ObservableProperty]
        private BankAccountDto? _selectedBankAccount;

        [ObservableProperty]
        private ObservableCollection<TransactionBasis> _bases;

        [ObservableProperty]
        private TransactionBasis? _selectedBasis;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private bool _showOnlyNew;

        public BankStatementImportViewModel(
            IBankStatementService statementService,
            IBankAccountService bankAccountService,
            IBankStatementParser parser,
            IEntryService entryService,
            IAccountService accountService, // Добавляем параметр
            IRepository<TransactionBasis> basisRepository)
        {
            _statementService = statementService;
            _bankAccountService = bankAccountService;
            _parser = parser;
            _entryService = entryService;
            _accountService = accountService; // Инициализируем поле
            _basisRepository = basisRepository;

            _statements = new ObservableCollection<BankStatementDto>();
            _bankAccounts = new ObservableCollection<BankAccountDto>();
            _bases = new ObservableCollection<TransactionBasis>();

            // Устанавливаем период по умолчанию (текущий месяц)
            var today = DateTime.Today;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);

            ShowOnlyNew = true;

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка данных...";

                // Загружаем банковские счета
                var bankAccounts = await _bankAccountService.GetAllBankAccountsAsync();
                BankAccounts.Clear();

                // Создаем элемент "Все счета"
                var allAccountsItem = new BankAccountDto
                {
                    Id = 0,
                    AccountNumber = "Все счета",
                    BankName = "",
                    SubaccountCode = ""
                };
                BankAccounts.Add(allAccountsItem);

                foreach (var account in bankAccounts.OrderBy(a => a.AccountNumber))
                {
                    BankAccounts.Add(account);
                }
                SelectedBankAccount = BankAccounts.FirstOrDefault();

                // Загружаем основания
                var bases = await _basisRepository.GetAllAsync();
                Bases.Clear();
                foreach (var basis in bases.OrderBy(b => b.Name))
                {
                    Bases.Add(basis);
                }
                SelectedBasis = Bases.FirstOrDefault();

                // Загружаем выписки
                await LoadStatementsAsync();

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

        private async Task LoadStatementsAsync()
        {
            var statements = await _statementService.GetAllStatementsAsync();

            var filtered = statements.AsEnumerable();

            // Фильтр по датам (используем ImportedAt)
            filtered = filtered.Where(s => s.ImportedAt.Date >= StartDate.Date && s.ImportedAt.Date <= EndDate.Date);

            // Фильтр по счету
            if (SelectedBankAccount?.Id > 0)
            {
                filtered = filtered.Where(s => s.BankAccountId == SelectedBankAccount.Id);
            }

            // Фильтр по статусу
            if (ShowOnlyNew)
            {
                // Показываем только новые И частично обработанные
                filtered = filtered.Where(s => s.Status == "New" || s.Status == "PartiallyProcessed");
            }
            else
            {
                // Показываем все
            }

            Statements.Clear();
            foreach (var statement in filtered.OrderByDescending(s => s.ImportedAt))
            {
                Statements.Add(statement);
            }
        }

        partial void OnSelectedBankAccountChanged(BankAccountDto? value)
        {
            _ = ApplyFilterAsync();
        }

        partial void OnStartDateChanged(DateTime value)
        {
            _ = ApplyFilterAsync();
        }

        partial void OnEndDateChanged(DateTime value)
        {
            _ = ApplyFilterAsync();
        }

        partial void OnShowOnlyNewChanged(bool value)
        {
            _ = ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            await LoadStatementsAsync();
        }

        [RelayCommand]
        private async Task ImportStatementAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Выберите файл выписки в формате 1CClientBankExchange",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Импорт выписки...";

                    var result = await _statementService.ImportStatementAsync(openFileDialog.FileName);

                    if (result.Success)
                    {
                        var message = $"✅ Выписка успешно импортирована.\n\n" +
                                     $"📅 Период: {result.StartDate:d} - {result.EndDate:d}\n" +
                                     $"📄 Документов: {result.DocumentsCount}\n" +
                                     $"🏦 Счет: {result.AccountNumber}";

                        if (!result.BankAccountFound)
                        {
                            message += "\n\n⚠️ Внимание! Не найден соответствующий банковский счет.\n" +
                                      "Создайте его в справочнике банковских счетов.";
                        }

                        if (result.CreatedCounterpartiesCount > 0)
                        {
                            message += $"\n\n👥 Добавлено контрагентов: {result.CreatedCounterpartiesCount}";
                        }

                        // Используем CreatedBankAccountsCount из объекта result
                        if (result.CreatedBankAccountsCount > 0)
                        {
                            message += $"\n\n🏦 Добавлено банковских счетов: {result.CreatedBankAccountsCount}";
                        }

                        if (result.Warnings.Any())
                        {
                            message += "\n\n⚠️ Предупреждения:\n" + string.Join("\n", result.Warnings);
                        }

                        MessageBox.Show(message, "Импорт завершен",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        await LoadStatementsAsync();
                    }
                    else
                    {
                        MessageBox.Show($"❌ Ошибка импорта: {result.ErrorMessage}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreateEntriesAsync()
        {
            try
            {
                if (SelectedStatement == null)
                {
                    MessageBox.Show("Выберите выписку для создания проводок", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedBasis == null)
                {
                    MessageBox.Show("Выберите основание для проводок", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Создать проводки по выписке от {SelectedStatement.ImportedAt:d}?\n" +
                    $"Документов: {SelectedStatement.DocumentsCount}",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Создание проводок...";

                    var generationResult = await _statementService.CreateEntriesFromStatementAsync(
                        SelectedStatement.Id, SelectedBasis.Id);

                    if (generationResult.Success)
                    {
                        var message = $"Создано проводок: {generationResult.EntriesCreated}\n" +
                                     $"Обработано документов: {generationResult.DocumentsProcessed}";

                        if (generationResult.Warnings.Any())
                        {
                            message += "\n\nПредупреждения:\n" + string.Join("\n", generationResult.Warnings);
                        }

                        MessageBox.Show(message, "Создание проводок завершено",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        await LoadStatementsAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка: {generationResult.ErrorMessage}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ViewStatementDetailsAsync()
        {
            if (SelectedStatement == null) return;

            var window = new BankStatementDetailsWindow();

            // Создаем ViewModel со всеми необходимыми сервисами
            var viewModel = new BankStatementDetailsViewModel(
                _statementService,
                _bankAccountService,
                _entryService,
                _accountService,  // Используем _accountService
                _basisRepository,
                SelectedStatement.Id,
                window);

            window.DataContext = viewModel;
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();

            // Обновляем список после закрытия окна (на случай, если были созданы проводки)
            await LoadStatementsAsync();
        }

        [RelayCommand]
        private async Task DeleteStatementAsync()
        {
            try
            {
                if (SelectedStatement == null)
                {
                    MessageBox.Show("Выберите выписку для удаления", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить выписку от {SelectedStatement.ImportedAt:d}?\n" +
                    $"Все связанные документы будут также удалены.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Удаление выписки...";

                    var success = await _statementService.DeleteStatementAsync(SelectedStatement.Id);

                    if (success)
                    {
                        StatusMessage = "Выписка успешно удалена";
                        await LoadStatementsAsync();
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
            await LoadStatementsAsync();
        }

        [RelayCommand]
        private void ManageBankAccounts()
        {
            var window = new BankAccountsWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();

            // Обновляем список банковских счетов после закрытия окна
            _ = LoadDataAsync();
        }
    }
}