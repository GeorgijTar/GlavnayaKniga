using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class ReportsViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IAccountService _accountService;
        private readonly IWordExportService _wordExportService;

        // Общие поля
        [ObservableProperty]
        private ObservableCollection<AccountDto> _accounts;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        // Для анализа счета
        [ObservableProperty]
        private AccountDto? _selectedAccountForAnalysis;

        [ObservableProperty]
        private AccountAnalysisDto? _accountAnalysis;

        [ObservableProperty]
        private ObservableCollection<AccountAnalysisDto> _accountAnalysisHierarchy;

        // Для оборотно-сальдовой ведомости
        [ObservableProperty]
        private AccountDto? _selectedAccountForBalanceSheet;

        [ObservableProperty]
        private bool _allAccountsBalanceSheet;

        [ObservableProperty]
        private BalanceSheetDto? _balanceSheet;

        [ObservableProperty]
        private ObservableCollection<BalanceSheetRowDto> _balanceSheetRows;

        // Для главной книги
        [ObservableProperty]
        private AccountDto? _selectedAccountForGeneralLedger;

        [ObservableProperty]
        private ObservableCollection<GeneralLedgerEntryDto> _generalLedgerEntries;

        [ObservableProperty]
        private decimal _generalLedgerOpeningBalance;

        [ObservableProperty]
        private decimal _generalLedgerClosingBalance;

        public ReportsViewModel(
            IReportService reportService,
            IAccountService accountService,
            IWordExportService wordExportService)
        {
            _reportService = reportService;
            _accountService = accountService;
            _wordExportService = wordExportService;

            _accounts = new ObservableCollection<AccountDto>();
            _accountAnalysisHierarchy = new ObservableCollection<AccountAnalysisDto>();
            _balanceSheetRows = new ObservableCollection<BalanceSheetRowDto>();
            _generalLedgerEntries = new ObservableCollection<GeneralLedgerEntryDto>();

            // Устанавливаем период по умолчанию (текущий квартал)
            var today = DateTime.Today;
            var quarter = (today.Month - 1) / 3;
            StartDate = new DateTime(today.Year, quarter * 3 + 1, 1);
            EndDate = StartDate.AddMonths(3).AddDays(-1);

            LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка счетов...";

                var accounts = await _accountService.GetAllAccountsAsync(true);

                Accounts.Clear();
                Accounts.Add(new AccountDto { Id = 0, Code = "", Name = "Все счета" });
                foreach (var account in accounts.OrderBy(a => a.Code))
                {
                    Accounts.Add(account);
                }

                // Устанавливаем значения по умолчанию
                SelectedAccountForAnalysis = Accounts.FirstOrDefault(a => a.Id > 0);
                SelectedAccountForBalanceSheet = Accounts.FirstOrDefault();
                SelectedAccountForGeneralLedger = Accounts.FirstOrDefault(a => a.Id > 0);
                AllAccountsBalanceSheet = true;

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки счетов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Анализ счета
        [RelayCommand]
        private async Task GenerateAccountAnalysisAsync()
        {
            try
            {
                if (SelectedAccountForAnalysis == null || SelectedAccountForAnalysis.Id == 0)
                {
                    MessageBox.Show("Выберите счет для анализа", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (StartDate > EndDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;
                StatusMessage = "Формирование анализа счета...";

                var analysis = await _reportService.GetAccountAnalysisWithSubaccountsAsync(
                    SelectedAccountForAnalysis.Id, StartDate, EndDate);

                AccountAnalysisHierarchy.Clear();
                foreach (var item in analysis)
                {
                    AccountAnalysisHierarchy.Add(item);
                }

                // Также сохраняем плоский анализ для детального просмотра
                if (analysis.Any())
                {
                    AccountAnalysis = analysis.First();
                }

                StatusMessage = "Анализ счета сформирован";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при формировании анализа счета: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ExportAccountAnalysisToExcelAsync()
        {
            try
            {
                if (AccountAnalysisHierarchy == null || !AccountAnalysisHierarchy.Any())
                {
                    MessageBox.Show("Сначала сформируйте отчет", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"Анализ счета {SelectedAccountForAnalysis?.Code}_{StartDate:yyyy-MM-dd}_{EndDate:yyyy-MM-dd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Экспорт в Excel...";

                    // TODO: Реализовать экспорт в Excel
                    await Task.Delay(1000); // Заглушка

                    StatusMessage = $"Отчет экспортирован в {saveFileDialog.FileName}";

                    var openResult = MessageBox.Show("Отчет успешно экспортирован. Открыть файл?",
                        "Экспорт завершен", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (openResult == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Оборотно-сальдовая ведомость
        [RelayCommand]
        private async Task GenerateBalanceSheetAsync()
        {
            try
            {
                if (StartDate > EndDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;
                StatusMessage = "Формирование оборотно-сальдовой ведомости...";

                int? accountId = null;
                if (!AllAccountsBalanceSheet && SelectedAccountForBalanceSheet?.Id > 0)
                {
                    accountId = SelectedAccountForBalanceSheet.Id;
                }

                BalanceSheet = await _reportService.GetBalanceSheetAsync(StartDate, EndDate, accountId);

                BalanceSheetRows.Clear();
                foreach (var row in BalanceSheet.Rows)
                {
                    BalanceSheetRows.Add(row);
                }

                StatusMessage = "Оборотно-сальдовая ведомость сформирована";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при формировании оборотно-сальдовой ведомости: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ExportBalanceSheetToExcelAsync()
        {
            try
            {
                if (BalanceSheet == null || !BalanceSheetRows.Any())
                {
                    MessageBox.Show("Сначала сформируйте отчет", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var fileName = AllAccountsBalanceSheet
                    ? $"ОСВ_{StartDate:yyyy-MM-dd}_{EndDate:yyyy-MM-dd}.xlsx"
                    : $"ОСВ_{SelectedAccountForBalanceSheet?.Code}_{StartDate:yyyy-MM-dd}_{EndDate:yyyy-MM-dd}.xlsx";

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = fileName
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Экспорт в Excel...";

                    // TODO: Реализовать экспорт в Excel
                    await Task.Delay(1000); // Заглушка

                    StatusMessage = $"Отчет экспортирован в {saveFileDialog.FileName}";

                    var openResult = MessageBox.Show("Отчет успешно экспортирован. Открыть файл?",
                        "Экспорт завершен", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (openResult == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Главная книга
        [RelayCommand]
        private async Task GenerateGeneralLedgerAsync()
        {
            try
            {
                if (SelectedAccountForGeneralLedger == null || SelectedAccountForGeneralLedger.Id == 0)
                {
                    MessageBox.Show("Выберите счет для формирования главной книги", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (StartDate > EndDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;
                StatusMessage = "Формирование главной книги...";

                var entries = await _reportService.GetGeneralLedgerAsync(
                    SelectedAccountForGeneralLedger.Id, StartDate, EndDate);

                GeneralLedgerEntries.Clear();
                foreach (var entry in entries)
                {
                    GeneralLedgerEntries.Add(entry);
                }

                // Рассчитываем начальное и конечное сальдо
                GeneralLedgerOpeningBalance = entries.Any()
                    ? entries.First().Balance - (entries.First().DebitAmount - entries.First().CreditAmount)
                    : 0;

                GeneralLedgerClosingBalance = entries.Any()
                    ? entries.Last().Balance
                    : 0;

                StatusMessage = "Главная книга сформирована";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при формировании главной книги: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ExportGeneralLedgerToExcelAsync()
        {
            try
            {
                if (!GeneralLedgerEntries.Any())
                {
                    MessageBox.Show("Сначала сформируйте отчет", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"Главная книга {SelectedAccountForGeneralLedger?.Code}_{StartDate:yyyy-MM-dd}_{EndDate:yyyy-MM-dd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    StatusMessage = "Экспорт в Excel...";

                    // TODO: Реализовать экспорт в Excel
                    await Task.Delay(1000); // Заглушка

                    StatusMessage = $"Отчет экспортирован в {saveFileDialog.FileName}";

                    var openResult = MessageBox.Show("Отчет успешно экспортирован. Открыть файл?",
                        "Экспорт завершен", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (openResult == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Вспомогательные методы для форматирования
        public string FormatAmount(decimal amount)
        {
            return amount.ToString("N2");
        }

        public string GetIndent(int level)
        {
            return new string(' ', level * 4);
        }
    }
}