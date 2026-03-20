using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class ReceiptEditViewModel : BaseViewModel
    {
        private readonly IReceiptService _receiptService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly IStorageLocationService _storageLocationService;
        private readonly IAccountService _accountService;
        private readonly INomenclatureService _nomenclatureService;
        private readonly IBikService _bikService;
        private readonly ICheckoService _checkoService;
        private readonly IUnitOfMeasureService _unitService;
        private readonly ReceiptDto? _originalReceipt;
        private readonly Window _window;

        [ObservableProperty]
        private ReceiptDto _receipt;

        [ObservableProperty]
        private ObservableCollection<CounterpartyDto> _contractors;

        [ObservableProperty]
        private CounterpartyDto? _selectedContractor;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _creditAccounts;

        [ObservableProperty]
        private AccountDto? _selectedCreditAccount;

        [ObservableProperty]
        private ObservableCollection<StorageLocationDto> _storageLocations;

        [ObservableProperty]
        private ObservableCollection<ReceiptItemDto> _items;

        [ObservableProperty]
        private ReceiptItemDto? _selectedItem;

        [ObservableProperty]
        private ObservableCollection<NomenclatureDto> _nomenclatures;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _debitAccounts;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _vatAccounts;

        [ObservableProperty]
        private ObservableCollection<decimal> _vatRates;

        [ObservableProperty]
        private decimal? _selectedVatRate;

        [ObservableProperty]
        private ObservableCollection<string> _vatCalculationMethods;

        [ObservableProperty]
        private string _selectedVatCalculationMethod;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private bool _isUPD;

        [ObservableProperty]
        private bool _showInvoiceFields;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private decimal? _totalVatAmount;

        [ObservableProperty]
        private decimal _totalAmountWithVat;

        // Поля для добавления новой строки
        [ObservableProperty]
        private NomenclatureDto? _selectedNomenclatureForNew;

        [ObservableProperty]
        private AccountDto? _selectedDebitAccountForNew;

        [ObservableProperty]
        private AccountDto? _selectedVatAccountForNew;

        [ObservableProperty]
        private StorageLocationDto? _selectedStorageLocationForNew;

        [ObservableProperty]
        private decimal _newItemQuantity = 1;

        [ObservableProperty]
        private decimal _newItemPrice;

        [ObservableProperty]
        private string? _newItemNote;

        // Временные свойства для редактирования строк в DataGrid
        private NomenclatureDto? _editingNomenclature;
        private AccountDto? _editingDebitAccount;
        private AccountDto? _editingVatAccount;
        private StorageLocationDto? _editingStorageLocation;

        public NomenclatureDto? EditingNomenclature
        {
            get => _editingNomenclature;
            set
            {
                if (SetProperty(ref _editingNomenclature, value) && value != null && SelectedItem != null)
                {
                    SelectedItem.NomenclatureId = value.Id;
                    SelectedItem.NomenclatureName = value.Name;
                    SelectedItem.NomenclatureArticle = value.Article;
                    SelectedItem.NomenclatureUnit = value.UnitDisplay;

                    var debitAccount = DebitAccounts.FirstOrDefault(a => a.Id == value.AccountId);
                    if (debitAccount != null)
                    {
                        SelectedItem.DebitAccountId = debitAccount.Id;
                        SelectedItem.DebitAccountCode = debitAccount.Code;
                        SelectedItem.DebitAccountName = debitAccount.Name;
                    }

                    RecalculateItemAmounts(SelectedItem);
                    CalculateTotals();
                }
            }
        }

        public AccountDto? EditingDebitAccount
        {
            get => _editingDebitAccount;
            set
            {
                if (SetProperty(ref _editingDebitAccount, value) && value != null && SelectedItem != null)
                {
                    SelectedItem.DebitAccountId = value.Id;
                    SelectedItem.DebitAccountCode = value.Code;
                    SelectedItem.DebitAccountName = value.Name;
                }
            }
        }

        public AccountDto? EditingVatAccount
        {
            get => _editingVatAccount;
            set
            {
                if (SetProperty(ref _editingVatAccount, value) && value != null && SelectedItem != null)
                {
                    SelectedItem.VatAccountId = value.Id;
                    SelectedItem.VatAccountCode = value.Code;
                    SelectedItem.VatAccountName = value.Name;
                }
            }
        }

        public StorageLocationDto? EditingStorageLocation
        {
            get => _editingStorageLocation;
            set
            {
                if (SetProperty(ref _editingStorageLocation, value) && value != null && SelectedItem != null)
                {
                    SelectedItem.StorageLocationId = value.Id > 0 ? value.Id : null;
                    SelectedItem.StorageLocationName = value.Name;
                }
            }
        }

        public ReceiptEditViewModel(
            IReceiptService receiptService,
            ICounterpartyService counterpartyService,
            IStorageLocationService storageLocationService,
            IAccountService accountService,
            INomenclatureService nomenclatureService,
            IBikService bikService,
            ICheckoService checkoService,
            IUnitOfMeasureService unitService,
            ReceiptDto? receiptToEdit,
            Window window)
        {
            _receiptService = receiptService;
            _counterpartyService = counterpartyService;
            _storageLocationService = storageLocationService;
            _accountService = accountService;
            _nomenclatureService = nomenclatureService;
            _bikService = bikService;
            _checkoService = checkoService;
            _unitService = unitService;
            _originalReceipt = receiptToEdit;
            _window = window;

            _contractors = new ObservableCollection<CounterpartyDto>();
            _creditAccounts = new ObservableCollection<AccountDto>();
            _storageLocations = new ObservableCollection<StorageLocationDto>();
            _items = new ObservableCollection<ReceiptItemDto>();
            _nomenclatures = new ObservableCollection<NomenclatureDto>();
            _debitAccounts = new ObservableCollection<AccountDto>();
            _vatAccounts = new ObservableCollection<AccountDto>();

            _vatRates = new ObservableCollection<decimal> { 0, 5, 7, 10, 20, 22 };
            _selectedVatRate = 20;

            _vatCalculationMethods = new ObservableCollection<string> { "НДС в цене", "НДС сверху" };
            _selectedVatCalculationMethod = "НДС в цене";

            if (_originalReceipt != null)
            {
                _receipt = new ReceiptDto
                {
                    Id = _originalReceipt.Id,
                    Number = _originalReceipt.Number,
                    Date = _originalReceipt.Date,
                    AccountingDate = _originalReceipt.AccountingDate,
                    ContractorId = _originalReceipt.ContractorId,
                    ContractorName = _originalReceipt.ContractorName,
                    ContractorINN = _originalReceipt.ContractorINN,
                    CreditAccountId = _originalReceipt.CreditAccountId,
                    CreditAccountCode = _originalReceipt.CreditAccountCode,
                    CreditAccountName = _originalReceipt.CreditAccountName,
                    VatCalculationMethod = _originalReceipt.VatCalculationMethod,
                    ContractNumber = _originalReceipt.ContractNumber,
                    ContractDate = _originalReceipt.ContractDate,
                    Basis = _originalReceipt.Basis,
                    TotalAmount = _originalReceipt.TotalAmount,
                    TotalVatAmount = _originalReceipt.TotalVatAmount,
                    TotalAmountWithVat = _originalReceipt.TotalAmountWithVat,
                    IsUPD = _originalReceipt.IsUPD,
                    InvoiceNumber = _originalReceipt.InvoiceNumber,
                    InvoiceDate = _originalReceipt.InvoiceDate,
                    Status = _originalReceipt.Status,
                    Note = _originalReceipt.Note
                };
                _isUPD = _originalReceipt.IsUPD;
                _showInvoiceFields = !_originalReceipt.IsUPD;
                _selectedVatCalculationMethod = _originalReceipt.VatCalculationMethod == "AbovePrice" ? "НДС сверху" : "НДС в цене";
                Title = $"Редактирование: {_originalReceipt.DisplayName}";
                IsEditMode = true;
            }
            else
            {
                _receipt = new ReceiptDto
                {
                    Date = DateTime.Today,
                    AccountingDate = DateTime.Today,
                    IsUPD = false,
                    Status = "Draft",
                    VatCalculationMethod = "IncludedInPrice"
                };
                _isUPD = false;
                _showInvoiceFields = true;
                _selectedVatCalculationMethod = "НДС в цене";
                Title = "Новое поступление";
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

                await LoadContractorsAsync();
                await LoadAccountsAsync();
                await LoadStorageLocationsAsync();
                await LoadNomenclaturesAsync();

                if (_originalReceipt != null)
                {
                    SelectedContractor = Contractors.FirstOrDefault(c => c.Id == _originalReceipt.ContractorId);
                    SelectedCreditAccount = CreditAccounts.FirstOrDefault(a => a.Id == _originalReceipt.CreditAccountId);
                    await LoadItemsAsync();
                }
                else
                {
                    SelectedCreditAccount = CreditAccounts.FirstOrDefault(a => a.Code == "60.1");
                    Receipt.Number = await _receiptService.GenerateDocumentNumberAsync(Receipt.Date);
                }

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

        private async Task LoadContractorsAsync()
        {
            var contractors = await _counterpartyService.GetAllCounterpartiesAsync(false);
            Contractors.Clear();
            foreach (var contractor in contractors.OrderBy(c => c.ShortName))
            {
                Contractors.Add(contractor);
            }
        }

        private async Task LoadAccountsAsync()
        {
            var accounts = await _accountService.GetAllAccountsAsync(false);

            var creditAccounts = accounts.Where(a => a.Code.StartsWith("60"));
            CreditAccounts.Clear();
            foreach (var account in creditAccounts.OrderBy(a => a.Code))
            {
                CreditAccounts.Add(account);
            }

            var debitAccounts = accounts.Where(a =>
                a.Code.StartsWith("10") ||
                a.Code.StartsWith("41") ||
                a.Code.StartsWith("07") ||
                a.Code.StartsWith("08") ||
                a.Code.StartsWith("20") ||
                a.Code.StartsWith("23") ||
                a.Code.StartsWith("25") ||
                a.Code.StartsWith("26"));
            DebitAccounts.Clear();
            foreach (var account in debitAccounts.OrderBy(a => a.Code))
            {
                DebitAccounts.Add(account);
            }

            var vatAccounts = accounts.Where(a => a.Code.StartsWith("19"));
            VatAccounts.Clear();
            foreach (var account in vatAccounts.OrderBy(a => a.Code))
            {
                VatAccounts.Add(account);
            }
        }

        private async Task LoadStorageLocationsAsync()
        {
            var storageLocations = await _storageLocationService.GetLocationsHierarchyAsync(false);
            StorageLocations.Clear();
            StorageLocations.Add(new StorageLocationDto { Id = 0, Code = "", Name = "— Не выбрано —" });
            foreach (var location in storageLocations)
            {
                AddLocationWithChildren(location, 0);
            }
        }

        private async Task LoadNomenclaturesAsync()
        {
            var nomenclatures = await _nomenclatureService.GetAllNomenclatureAsync(false);
            Nomenclatures.Clear();
            foreach (var item in nomenclatures.OrderBy(n => n.Name))
            {
                Nomenclatures.Add(item);
            }
        }

        private async Task LoadItemsAsync()
        {
            if (_originalReceipt == null) return;

            var items = await _receiptService.GetItemsAsync(_originalReceipt.Id);
            Items.Clear();
            foreach (var item in items.OrderBy(i => i.LineNumber))
            {
                Items.Add(item);
            }
            CalculateTotals();
        }

        private void AddLocationWithChildren(StorageLocationDto location, int level)
        {
            var displayLocation = new StorageLocationDto
            {
                Id = location.Id,
                Code = location.Code,
                Name = location.Name,
                Type = location.Type,
                TypeDisplay = location.TypeDisplay,
                ParentId = location.ParentId,
                ParentName = location.ParentName,
                Children = location.Children
            };

            StorageLocations.Add(displayLocation);

            foreach (var child in location.Children.OrderBy(c => c.Code))
            {
                AddLocationWithChildren(child, level + 1);
            }
        }

        partial void OnSelectedVatCalculationMethodChanged(string value)
        {
            Receipt.VatCalculationMethod = value == "НДС сверху" ? "AbovePrice" : "IncludedInPrice";

            foreach (var item in Items)
            {
                RecalculateItemAmounts(item);
            }
            CalculateTotals();
        }

        public void RecalculateItemAmounts(ReceiptItemDto item)
        {
            if (item == null) return;

            decimal baseAmount = item.Quantity * item.Price;

            if (item.VatRate.HasValue && item.VatRate.Value > 0)
            {
                if (Receipt.VatCalculationMethod == "AbovePrice")
                {
                    item.VatAmount = Math.Round(baseAmount * item.VatRate.Value / 100, 2);
                    item.AmountWithVat = baseAmount + item.VatAmount.Value;
                    item.Amount = baseAmount;
                }
                else
                {
                    item.AmountWithVat = baseAmount;
                    item.VatAmount = Math.Round(baseAmount * item.VatRate.Value / (100 + item.VatRate.Value), 2);
                    item.Amount = item.AmountWithVat.Value - item.VatAmount.Value;
                }
            }
            else
            {
                item.Amount = baseAmount;
                item.AmountWithVat = baseAmount;
                item.VatAmount = null;
            }
        }

        public void RecalculateRow(ReceiptItemDto item)
        {
            if (item == null) return;

            RecalculateItemAmounts(item);
            CalculateTotals();
        }

        public void RenumberLines()
        {
            int lineNumber = 1;
            foreach (var item in Items.OrderBy(i => i.LineNumber))
            {
                if (item.LineNumber != lineNumber)
                {
                    item.LineNumber = lineNumber;
                }
                lineNumber++;
            }
        }

        partial void OnSelectedContractorChanged(CounterpartyDto? value)
        {
            if (value != null)
            {
                Receipt.ContractorId = value.Id;
                Receipt.ContractorName = value.ShortName;
                Receipt.ContractorINN = value.INN;
            }
        }

        partial void OnSelectedCreditAccountChanged(AccountDto? value)
        {
            if (value != null)
            {
                Receipt.CreditAccountId = value.Id;
                Receipt.CreditAccountCode = value.Code;
                Receipt.CreditAccountName = value.Name;
            }
        }

        partial void OnIsUPDChanged(bool value)
        {
            Receipt.IsUPD = value;
            ShowInvoiceFields = !value;

            if (value)
            {
                Receipt.InvoiceNumber = Receipt.Number;
                Receipt.InvoiceDate = Receipt.Date;
            }
        }

        partial void OnSelectedItemChanged(ReceiptItemDto? value)
        {
            if (value != null)
            {
                var nomenclature = Nomenclatures.FirstOrDefault(n => n.Id == value.NomenclatureId);
                if (nomenclature != null) EditingNomenclature = nomenclature;

                var debitAccount = DebitAccounts.FirstOrDefault(a => a.Id == value.DebitAccountId);
                if (debitAccount != null) EditingDebitAccount = debitAccount;

                var vatAccount = VatAccounts.FirstOrDefault(a => a.Id == value.VatAccountId);
                if (vatAccount != null) EditingVatAccount = vatAccount;

                if (value.StorageLocationId.HasValue)
                {
                    var storageLocation = StorageLocations.FirstOrDefault(s => s.Id == value.StorageLocationId.Value);
                    if (storageLocation != null) EditingStorageLocation = storageLocation;
                }
                else
                {
                    EditingStorageLocation = StorageLocations.FirstOrDefault(s => s.Id == 0);
                }
            }
        }

        [RelayCommand]
        private void AddItem()
        {
            try
            {
                if (SelectedNomenclatureForNew == null)
                {
                    MessageBox.Show(_window, "Выберите номенклатуру", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedDebitAccountForNew == null)
                {
                    MessageBox.Show(_window, "Выберите счет учета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (NewItemQuantity <= 0)
                {
                    MessageBox.Show(_window, "Количество должно быть больше 0", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (NewItemPrice <= 0)
                {
                    MessageBox.Show(_window, "Цена должна быть больше 0", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedVatRate.HasValue && SelectedVatRate.Value > 0 && SelectedVatAccountForNew == null)
                {
                    MessageBox.Show(_window, "Для НДС необходимо выбрать счет учета НДС", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newItem = new ReceiptItemDto
                {
                    ReceiptId = Receipt.Id,
                    NomenclatureId = SelectedNomenclatureForNew.Id,
                    NomenclatureName = SelectedNomenclatureForNew.Name,
                    NomenclatureArticle = SelectedNomenclatureForNew.Article,
                    NomenclatureUnit = SelectedNomenclatureForNew.UnitDisplay,
                    Quantity = NewItemQuantity,
                    Price = NewItemPrice,
                    Amount = NewItemQuantity * NewItemPrice,
                    VatRate = SelectedVatRate > 0 ? SelectedVatRate : null,
                    DebitAccountId = SelectedDebitAccountForNew.Id,
                    DebitAccountCode = SelectedDebitAccountForNew.Code,
                    DebitAccountName = SelectedDebitAccountForNew.Name,
                    VatAccountId = SelectedVatAccountForNew?.Id ?? 0,
                    VatAccountCode = SelectedVatAccountForNew?.Code,
                    VatAccountName = SelectedVatAccountForNew?.Name,
                    StorageLocationId = SelectedStorageLocationForNew?.Id > 0 ? SelectedStorageLocationForNew.Id : null,
                    StorageLocationName = SelectedStorageLocationForNew?.Name,
                    Note = NewItemNote,
                    LineNumber = Items.Count + 1
                };

                if (Receipt.Id > 0)
                {
                    _ = AddItemToDatabaseAsync(newItem);
                }
                else
                {
                    Items.Add(newItem);
                    ClearNewItemFields();
                    CalculateTotals();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при добавлении строки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddItemToDatabaseAsync(ReceiptItemDto item)
        {
            try
            {
                var added = await _receiptService.AddItemAsync(item);
                await LoadItemsAsync();
                ClearNewItemFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при добавлении строки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearNewItemFields()
        {
            SelectedNomenclatureForNew = null;
            SelectedDebitAccountForNew = null;
            SelectedVatAccountForNew = null;
            SelectedStorageLocationForNew = null;
            NewItemQuantity = 1;
            NewItemPrice = 0;
            SelectedVatRate = 20;
            NewItemNote = null;
        }

        [RelayCommand]
        private async Task DeleteItemAsync()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show(_window, "Выберите строку для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(_window,
                $"Удалить строку {SelectedItem.LineNumber}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (SelectedItem.Id > 0)
                    {
                        await _receiptService.DeleteItemAsync(SelectedItem.Id);
                        await LoadItemsAsync();
                    }
                    else
                    {
                        Items.Remove(SelectedItem);
                        RenumberLines();
                        CalculateTotals();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_window, $"Ошибка при удалении строки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void CalculateTotals()
        {
            TotalAmount = Items.Sum(i => i.Amount);
            TotalVatAmount = Items.Sum(i => i.VatAmount ?? 0);
            TotalAmountWithVat = Items.Sum(i => i.AmountWithVat ?? i.Amount);
            Receipt.TotalAmount = TotalAmount;
            Receipt.TotalVatAmount = TotalVatAmount;
            Receipt.TotalAmountWithVat = TotalAmountWithVat;
        }

        [RelayCommand]
        private void AddContractor()
        {
            try
            {
                var window = new CounterpartyEditWindow();
                var viewModel = new CounterpartyEditViewModel(
                    _counterpartyService,
                    _bikService,
                    _checkoService,
                    null,
                    window);

                window.DataContext = viewModel;
                window.Owner = _window;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = window.ShowDialog();
                if (result == true)
                {
                    _ = LoadContractorsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при создании контрагента: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddNomenclature()
        {
            try
            {
                var window = new NomenclatureEditWindow();
                var viewModel = new NomenclatureEditViewModel(
                    _nomenclatureService,
                    _accountService,
                    _storageLocationService,
                    _unitService,
                    null,
                    window);

                window.DataContext = viewModel;
                window.Owner = _window;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = window.ShowDialog();
                if (result == true)
                {
                    _ = LoadNomenclaturesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при создании номенклатуры: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Receipt.Number))
                {
                    MessageBox.Show(_window, "Введите номер документа", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedContractor == null)
                {
                    MessageBox.Show(_window, "Выберите контрагента", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCreditAccount == null)
                {
                    MessageBox.Show(_window, "Выберите счет учета поставщика", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Items.Any())
                {
                    MessageBox.Show(_window, "Добавьте хотя бы одну строку в документ", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                foreach (var item in Items)
                {
                    if (item.NomenclatureId == 0)
                    {
                        MessageBox.Show(_window, $"В строке {item.LineNumber} не выбрана номенклатура", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (item.DebitAccountId == 0)
                    {
                        MessageBox.Show(_window, $"В строке {item.LineNumber} не выбран счет учета", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (item.VatRate.HasValue && item.VatRate.Value > 0 && item.VatAccountId == 0)
                    {
                        MessageBox.Show(_window, $"В строке {item.LineNumber} указан НДС, но не выбран счет учета НДС", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (Receipt.Id > 0)
                {
                    await _receiptService.UpdateReceiptAsync(Receipt);

                    foreach (var item in Items)
                    {
                        if (item.Id > 0)
                        {
                            await _receiptService.UpdateItemAsync(item);
                        }
                        else
                        {
                            item.ReceiptId = Receipt.Id;
                            await _receiptService.AddItemAsync(item);
                        }
                    }

                    MessageBox.Show(_window, "Документ успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var created = await _receiptService.CreateReceiptAsync(Receipt);

                    foreach (var item in Items)
                    {
                        item.ReceiptId = created.Id;
                        await _receiptService.AddItemAsync(item);
                    }

                    MessageBox.Show(_window, "Документ успешно создан", "Успех",
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