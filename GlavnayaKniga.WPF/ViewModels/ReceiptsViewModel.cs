using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Application.Services;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class ReceiptsViewModel : BaseViewModel
    {
        private readonly IReceiptService _receiptService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly IStorageLocationService _storageLocationService;
        private readonly IAccountService _accountService;
        private readonly INomenclatureService _nomenclatureService;
        private readonly IBikService _bikService;
        private readonly ICheckoService _checkoService;
        private readonly IUnitOfMeasureService _unitService;

        [ObservableProperty]
        private ObservableCollection<ReceiptDto> _receipts;

        [ObservableProperty]
        private ObservableCollection<ReceiptDto> _filteredReceipts;

        [ObservableProperty]
        private ReceiptDto? _selectedReceipt;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private bool _showDraft = true;

        [ObservableProperty]
        private bool _showPosted = true;

        [ObservableProperty]
        private CounterpartyDto? _selectedContractorFilter;

        [ObservableProperty]
        private ObservableCollection<CounterpartyDto> _contractors;

        public ReceiptsViewModel(
            IReceiptService receiptService,
            ICounterpartyService counterpartyService,
            IStorageLocationService storageLocationService,
            IAccountService accountService,
            INomenclatureService nomenclatureService,
            IUnitOfMeasureService unitService, 
            IBikService bikService, 
            ICheckoService checkoService)
        {
            _receiptService = receiptService;
            _counterpartyService = counterpartyService;
            _storageLocationService = storageLocationService;
            _accountService = accountService;
            _nomenclatureService = nomenclatureService;
            _bikService=bikService;
            _checkoService=checkoService;

            _receipts = new ObservableCollection<ReceiptDto>();
            _filteredReceipts = new ObservableCollection<ReceiptDto>();
            _contractors = new ObservableCollection<CounterpartyDto>();

            // Устанавливаем период по умолчанию (текущий месяц)
            var today = DateTime.Today;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);

            LoadDataAsync();
            _unitService = unitService;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка документов поступления...";

                // Загружаем контрагентов для фильтра
                var contractors = await _counterpartyService.GetAllCounterpartiesAsync(false);
                Contractors.Clear();
                Contractors.Add(new CounterpartyDto { Id = 0, ShortName = "Все контрагенты" });
                foreach (var contractor in contractors.OrderBy(c => c.ShortName))
                {
                    Contractors.Add(contractor);
                }
                SelectedContractorFilter = Contractors.FirstOrDefault();

                // Загружаем документы
                await LoadReceiptsAsync();

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadReceiptsAsync()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync(ShowDraft, ShowPosted);

            // Применяем фильтры
            var filtered = receipts.Where(r => r.Date.Date >= StartDate.Date && r.Date.Date <= EndDate.Date);

            if (SelectedContractorFilter?.Id > 0)
            {
                filtered = filtered.Where(r => r.ContractorId == SelectedContractorFilter.Id);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(r =>
                    r.Number.ToLower().Contains(searchLower) ||
                    (r.ContractorName != null && r.ContractorName.ToLower().Contains(searchLower)) ||
                    (r.Basis != null && r.Basis.ToLower().Contains(searchLower)));
            }

            Receipts.Clear();
            FilteredReceipts.Clear();

            foreach (var receipt in filtered.OrderByDescending(r => r.Date).ThenByDescending(r => r.Number))
            {
                Receipts.Add(receipt);
                FilteredReceipts.Add(receipt);
            }
        }

        partial void OnSearchTextChanged(string value)
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

        partial void OnShowDraftChanged(bool value)
        {
            _ = ApplyFilterAsync();
        }

        partial void OnShowPostedChanged(bool value)
        {
            _ = ApplyFilterAsync();
        }

        partial void OnSelectedContractorFilterChanged(CounterpartyDto? value)
        {
            _ = ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            await LoadReceiptsAsync();
        }

        [RelayCommand]
        private async Task AddReceiptAsync()
        {
            try
            {
                var window = new ReceiptEditWindow();
                var viewModel = new ReceiptEditViewModel(
                    _receiptService,
                    _counterpartyService,
                    _storageLocationService,
                    _accountService,
                    _nomenclatureService,
                    _bikService,
                    _checkoService,
                    _unitService,
                    null,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadReceiptsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditReceiptAsync()
        {
            try
            {
                if (SelectedReceipt == null)
                {
                    MessageBox.Show("Выберите документ для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedReceipt.CanEdit)
                {
                    MessageBox.Show("Нельзя редактировать проведенный документ", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var receiptToEdit = await _receiptService.GetReceiptByIdAsync(SelectedReceipt.Id);
                if (receiptToEdit == null)
                {
                    MessageBox.Show("Документ не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new ReceiptEditWindow();
                var viewModel = new ReceiptEditViewModel(
                    _receiptService,
                    _counterpartyService,
                    _storageLocationService,
                    _accountService,
                    _nomenclatureService,
                    _bikService,
                    _checkoService,
                    _unitService,
                    receiptToEdit,
                    window);

                window.DataContext = viewModel;
                window.Owner =System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadReceiptsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteReceiptAsync()
        {
            try
            {
                if (SelectedReceipt == null)
                {
                    MessageBox.Show("Выберите документ для удаления", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var message = SelectedReceipt.Status == "Posted"
                    ? $"Документ {SelectedReceipt.DisplayName} проведен. При удалении также будут удалены все созданные им проводки.\n\nПродолжить?"
                    : $"Вы действительно хотите удалить документ {SelectedReceipt.DisplayName}?";

                var result = MessageBox.Show(message, "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Удаление документа...";

                    var success = await _receiptService.DeleteReceiptAsync(SelectedReceipt.Id);

                    if (success)
                    {
                        StatusMessage = "Документ успешно удален";
                        await LoadReceiptsAsync();
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
        private async Task PostReceiptAsync()
        {
            try
            {
                if (SelectedReceipt == null)
                {
                    MessageBox.Show("Выберите документ для проведения", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedReceipt.CanPost)
                {
                    MessageBox.Show("Документ не может быть проведен. Проверьте наличие строк и корректность заполнения.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Провести документ {SelectedReceipt.DisplayName}?\n\nБудут созданы проводки и обновлены остатки.",
                    "Подтверждение проведения",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Проведение документа...";

                    // TODO: Получить ID основания из настроек
                    var defaultBasisId = 1; // Временно

                    var posted = await _receiptService.PostReceiptAsync(SelectedReceipt.Id, defaultBasisId);

                    StatusMessage = "Документ успешно проведен";
                    await LoadReceiptsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проведении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UnpostReceiptAsync()
        {
            try
            {
                if (SelectedReceipt == null)
                {
                    MessageBox.Show("Выберите документ для отмены проведения", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedReceipt.CanUnpost)
                {
                    MessageBox.Show("Документ не проведен", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Отменить проведение документа {SelectedReceipt.DisplayName}?\n\nБудут удалены все связанные проводки и скорректированы остатки.",
                    "Подтверждение отмены проведения",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Отмена проведения...";

                    var unposted = await _receiptService.UnpostReceiptAsync(SelectedReceipt.Id);

                    StatusMessage = "Проведение отменено";
                    await LoadReceiptsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене проведения: {ex.Message}", "Ошибка",
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
            await LoadReceiptsAsync();
        }

        [RelayCommand]
        private async Task ViewReceiptAsync()
        {
            try
            {
                if (SelectedReceipt == null)
                {
                    MessageBox.Show("Выберите документ для просмотра", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var receiptToView = await _receiptService.GetReceiptByIdAsync(SelectedReceipt.Id);
                if (receiptToView == null)
                {
                    MessageBox.Show("Документ не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new ReceiptViewWindow();
                var viewModel = new ReceiptViewViewModel(
                    _receiptService,
                    _counterpartyService,
                    _storageLocationService,
                    _accountService,
                    _nomenclatureService,
                    _unitService,
                    receiptToView,
                    window);

                window.DataContext = viewModel;
                window.Owner =System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        [RelayCommand]
        private void ViewStocks()
        {
            var window = new NomenclatureStocksWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
    }
}