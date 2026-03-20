using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class EntriesViewModel : BaseViewModel
    {
        private readonly IEntryService _entryService;
        private readonly IAccountService _accountService;
        private readonly IRepository<TransactionBasis> _basisRepository;
        private bool _isLoading = false;
        private bool _skipValidation = false;

        [ObservableProperty]
        private ObservableCollection<EntryDto> _entries;

        [ObservableProperty]
        private EntryDto? _selectedEntry;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _accounts;

        [ObservableProperty]
        private AccountDto? _selectedAccount;

        [ObservableProperty]
        private decimal _totalAmount;

        public EntriesViewModel(
            IEntryService entryService,
            IAccountService accountService,
            IRepository<TransactionBasis> basisRepository)
        {
            _entryService = entryService;
            _accountService = accountService;
            _basisRepository = basisRepository;

            _entries = new ObservableCollection<EntryDto>();
            _accounts = new ObservableCollection<AccountDto>();

            // Устанавливаем флаг для пропуска валидации при инициализации
            _skipValidation = true;

            // Устанавливаем период по умолчанию (текущий месяц)
            var today = DateTime.Today;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);

            _skipValidation = false;

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _isLoading = true;
                IsBusy = true;
                StatusMessage = "Загрузка проводок...";

                // Загружаем счета для фильтра
                var accounts = await _accountService.GetAllAccountsAsync(true);
                Accounts.Clear();
                Accounts.Add(new AccountDto { Id = 0, Code = "", Name = "Все счета" });
                foreach (var account in accounts.OrderBy(a => a.Code))
                {
                    Accounts.Add(account);
                }
                SelectedAccount = Accounts.FirstOrDefault();

                // Загружаем проводки
                await LoadEntriesAsync();

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
                _isLoading = false;
                IsBusy = false;
            }
        }

        private async Task LoadEntriesAsync()
        {
            try
            {
                var entries = await _entryService.GetEntriesByDateRangeAsync(StartDate, EndDate);

                // Применяем фильтр по счету
                if (SelectedAccount != null && SelectedAccount.Id > 0)
                {
                    entries = entries.Where(e =>
                        e.DebitAccountId == SelectedAccount.Id ||
                        e.CreditAccountId == SelectedAccount.Id);
                }

                Entries.Clear();
                foreach (var entry in entries.OrderByDescending(e => e.Date))
                {
                    Entries.Add(entry);
                }

                CalculateTotal();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки проводок: {ex.Message}";
            }
        }

        private void CalculateTotal()
        {
            TotalAmount = Entries.Sum(e => e.Amount);
        }

        partial void OnStartDateChanged(DateTime value)
        {
            // Пропускаем валидацию при инициализации или загрузке
            if (_skipValidation || _isLoading) return;

            if (value > EndDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                // Возвращаем предыдущее значение
                StartDate = EndDate;
                return;
            }

            _ = ApplyFilterAsync();
        }

        partial void OnEndDateChanged(DateTime value)
        {
            // Пропускаем валидацию при инициализации или загрузке
            if (_skipValidation || _isLoading) return;

            if (value < StartDate)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                // Возвращаем предыдущее значение
                EndDate = StartDate;
                return;
            }

            _ = ApplyFilterAsync();
        }

        partial void OnSelectedAccountChanged(AccountDto? value)
        {
            if (_isLoading) return;
            _ = ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            if (_isLoading) return;

            _isLoading = true;
            try
            {
                await LoadEntriesAsync();
            }
            finally
            {
                _isLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddEntryAsync()
        {
            try
            {
                var window = new EntryEditWindow();

                var viewModel = new EntryEditViewModel(
                    _entryService,
                    _accountService,
                    _basisRepository,
                    null,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();

                if (result == true)
                {
                    await LoadEntriesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditEntryAsync()
        {
            try
            {
                if (SelectedEntry == null)
                {
                    MessageBox.Show("Выберите проводку для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var entryToEdit = await _entryService.GetEntryByIdAsync(SelectedEntry.Id);
                if (entryToEdit == null)
                {
                    MessageBox.Show("Проводка не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new EntryEditWindow();

                var viewModel = new EntryEditViewModel(
                    _entryService,
                    _accountService,
                    _basisRepository,
                    entryToEdit,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();

                if (result == true)
                {
                    await LoadEntriesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteEntryAsync()
        {
            try
            {
                if (SelectedEntry == null)
                {
                    MessageBox.Show("Выберите проводку для удаления", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить проводку от {SelectedEntry.Date:dd.MM.yyyy} " +
                    $"на сумму {SelectedEntry.Amount:N2}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Удаление проводки...";

                    var success = await _entryService.DeleteEntryAsync(SelectedEntry.Id);

                    if (success)
                    {
                        StatusMessage = "Проводка успешно удалена";
                        await LoadEntriesAsync();
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
            await LoadEntriesAsync();
        }

        [RelayCommand]
        private void SetCurrentMonth()
        {
            _skipValidation = true;
            var today = DateTime.Today;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);
            _skipValidation = false;
            _ = ApplyFilterAsync();
        }

        [RelayCommand]
        private void SetPreviousMonth()
        {
            _skipValidation = true;
            var today = DateTime.Today.AddMonths(-1);
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);
            _skipValidation = false;
            _ = ApplyFilterAsync();
        }

        [RelayCommand]
        private void SetCurrentQuarter()
        {
            _skipValidation = true;
            var today = DateTime.Today;
            var quarter = (today.Month - 1) / 3 + 1;
            StartDate = new DateTime(today.Year, (quarter - 1) * 3 + 1, 1);
            EndDate = StartDate.AddMonths(3).AddDays(-1);
            _skipValidation = false;
            _ = ApplyFilterAsync();
        }

        [RelayCommand]
        private void SetCurrentYear()
        {
            _skipValidation = true;
            var today = DateTime.Today;
            StartDate = new DateTime(today.Year, 1, 1);
            EndDate = new DateTime(today.Year, 12, 31);
            _skipValidation = false;
            _ = ApplyFilterAsync();
        }
    }
}