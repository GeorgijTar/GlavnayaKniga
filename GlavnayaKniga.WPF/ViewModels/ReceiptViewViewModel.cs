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
    public partial class ReceiptViewViewModel : BaseViewModel
    {
        private readonly IReceiptService _receiptService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly IStorageLocationService _storageLocationService;
        private readonly IAccountService _accountService;
        private readonly INomenclatureService _nomenclatureService;
        private readonly IUnitOfMeasureService _unitService;
        private readonly ReceiptDto _receipt;
        private readonly Window _window;

        [ObservableProperty]
        private ReceiptDto _document;

        [ObservableProperty]
        private ObservableCollection<ReceiptItemDto> _items;

        [ObservableProperty]
        private CounterpartyDto? _contractor;

        [ObservableProperty]
        private AccountDto? _creditAccount;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private decimal? _totalVatAmount;

        [ObservableProperty]
        private decimal _totalAmountWithVat;

        [ObservableProperty]
        private string _contractorInfo;

        [ObservableProperty]
        private string _documentInfo;

        [ObservableProperty]
        private string? _storageLocationsSummary; // Сводка по местам хранения

        public ReceiptViewViewModel(
            IReceiptService receiptService,
            ICounterpartyService counterpartyService,
            IStorageLocationService storageLocationService,
            IAccountService accountService,
            INomenclatureService nomenclatureService,
            IUnitOfMeasureService unitService,
            ReceiptDto receipt,
            Window window)
        {
            _receiptService = receiptService;
            _counterpartyService = counterpartyService;
            _storageLocationService = storageLocationService;
            _accountService = accountService;
            _nomenclatureService = nomenclatureService;
            _unitService = unitService;
            _receipt = receipt;
            _window = window;

            _document = receipt;
            _items = new ObservableCollection<ReceiptItemDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка данных...";

                // Загружаем строки документа
                var items = await _receiptService.GetItemsAsync(_document.Id);
                Items.Clear();
                foreach (var item in items.OrderBy(i => i.LineNumber))
                {
                    Items.Add(item);
                }

                // Загружаем контрагента
                var contractor = await _counterpartyService.GetCounterpartyByIdAsync(_document.ContractorId);
                Contractor = contractor;

                // Загружаем счет учета
                var creditAccount = await _accountService.GetAccountByIdAsync(_document.CreditAccountId);
                CreditAccount = creditAccount;

                // Формируем сводку по местам хранения
                await LoadStorageLocationsSummary();

                // Формируем информационные строки
                ContractorInfo = $"{contractor?.ShortName} (ИНН: {contractor?.INN})";
                DocumentInfo = $"Документ №{_document.Number} от {_document.Date:d}";

                CalculateTotals();
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

        private async Task LoadStorageLocationsSummary()
        {
            try
            {
                var storageGroups = Items
                    .Where(i => i.StorageLocationId.HasValue)
                    .GroupBy(i => i.StorageLocationId)
                    .Select(g => new
                    {
                        LocationId = g.Key,
                        Count = g.Count(),
                        Items = g.ToList()
                    })
                    .ToList();

                if (!storageGroups.Any())
                {
                    StorageLocationsSummary = "Места хранения не указаны";
                    return;
                }

                var summaries = new System.Collections.Generic.List<string>();
                foreach (var group in storageGroups)
                {
                    if (group.LocationId.HasValue)
                    {
                        var location = await _storageLocationService.GetLocationByIdAsync(group.LocationId.Value);
                        if (location != null)
                        {
                            summaries.Add($"{location.DisplayName} ({group.Count} поз.)");
                        }
                    }
                }

                StorageLocationsSummary = string.Join(", ", summaries);
            }
            catch (Exception ex)
            {
                StorageLocationsSummary = "Ошибка загрузки мест хранения";
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки мест хранения: {ex.Message}");
            }
        }

        private void CalculateTotals()
        {
            TotalAmount = Items.Sum(i => i.Amount);
            TotalVatAmount = Items.Sum(i => i.VatAmount ?? 0);
            TotalAmountWithVat = Items.Sum(i => i.AmountWithVat ?? i.Amount);
        }

        [RelayCommand]
        private void Close()
        {
            _window.Close();
        }
    }
}