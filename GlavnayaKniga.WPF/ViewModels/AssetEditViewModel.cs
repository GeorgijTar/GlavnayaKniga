using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class AssetEditViewModel : BaseViewModel
    {
        private readonly IAssetService _assetService;
        private readonly IAssetTypeService _assetTypeService;
        private readonly IAccountService _accountService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly AssetDto? _originalAsset;
        private readonly Window _window;

        [ObservableProperty]
        private AssetDto _asset;

        [ObservableProperty]
        private ObservableCollection<AssetTypeDto> _assetTypes;

        [ObservableProperty]
        private AssetTypeDto? _selectedAssetType;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _accounts;

        [ObservableProperty]
        private AccountDto? _selectedAccount;

        [ObservableProperty]
        private ObservableCollection<CounterpartyDto> _responsiblePersons;

        [ObservableProperty]
        private CounterpartyDto? _selectedResponsiblePerson;

        [ObservableProperty]
        private ObservableCollection<int> _years;

        [ObservableProperty]
        private int? _selectedYear;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public AssetEditViewModel(
            IAssetService assetService,
            IAssetTypeService assetTypeService,
            IAccountService accountService,
            ICounterpartyService counterpartyService,
            AssetDto? assetToEdit,
            Window window)
        {
            _assetService = assetService;
            _assetTypeService = assetTypeService;
            _accountService = accountService;
            _counterpartyService = counterpartyService;
            _originalAsset = assetToEdit;
            _window = window;

            _assetTypes = new ObservableCollection<AssetTypeDto>();
            _accounts = new ObservableCollection<AccountDto>();
            _responsiblePersons = new ObservableCollection<CounterpartyDto>();

            // Годы выпуска (с 1950 по текущий + 1)
            var currentYear = DateTime.Now.Year;
            _years = new ObservableCollection<int>(
                Enumerable.Range(1950, currentYear - 1950 + 2).OrderByDescending(y => y));

            if (_originalAsset != null)
            {
                _asset = new AssetDto
                {
                    Id = _originalAsset.Id,
                    Name = _originalAsset.Name,
                    RegistrationNumber = _originalAsset.RegistrationNumber,
                    InventoryNumber = _originalAsset.InventoryNumber,
                    AssetTypeId = _originalAsset.AssetTypeId,
                    YearOfManufacture = _originalAsset.YearOfManufacture,
                    Model = _originalAsset.Model,
                    Manufacturer = _originalAsset.Manufacturer,
                    SerialNumber = _originalAsset.SerialNumber,
                    PurchaseDate = _originalAsset.PurchaseDate,
                    CommissioningDate = _originalAsset.CommissioningDate,
                    DecommissioningDate = _originalAsset.DecommissioningDate,
                    InitialCost = _originalAsset.InitialCost,
                    ResidualValue = _originalAsset.ResidualValue,
                    Location = _originalAsset.Location,
                    ResponsiblePersonId = _originalAsset.ResponsiblePersonId,
                    AccountId = _originalAsset.AccountId,
                    Note = _originalAsset.Note,
                    IsArchived = _originalAsset.IsArchived
                };
                _selectedYear = _originalAsset.YearOfManufacture;
                Title = "Редактирование объекта";
                IsEditMode = true;
            }
            else
            {
                _asset = new AssetDto();
                Title = "Добавление объекта";
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

                // Загружаем типы объектов
                var types = await _assetTypeService.GetAllAssetTypesAsync(false);
                AssetTypes.Clear();
                foreach (var type in types.OrderBy(t => t.Name))
                {
                    AssetTypes.Add(type);
                }

                // Загружаем счета учета для основных средств (01)
                var accounts = await _accountService.GetAllAccountsAsync(false);
                var assetAccounts = accounts.Where(a => a.Code.StartsWith("01") || a.Code.StartsWith("03") || a.Code.StartsWith("07") || a.Code.StartsWith("08"));
                Accounts.Clear();
                foreach (var account in assetAccounts.OrderBy(a => a.Code))
                {
                    Accounts.Add(account);
                }

                // Загружаем ответственных лиц (сотрудников)
                var persons = await _counterpartyService.SearchCounterpartiesAsync("", false);
                var employees = persons.Where(p => p.Type == "Физическое лицо" || p.Type == "Индивидуальный предприниматель");
                ResponsiblePersons.Clear();
                ResponsiblePersons.Add(new CounterpartyDto { Id = 0, ShortName = "— Не выбран —" });
                foreach (var person in employees.OrderBy(p => p.ShortName))
                {
                    ResponsiblePersons.Add(person);
                }

                // Устанавливаем выбранные значения
                if (_originalAsset != null)
                {
                    SelectedAssetType = AssetTypes.FirstOrDefault(t => t.Id == _originalAsset.AssetTypeId);
                    SelectedAccount = Accounts.FirstOrDefault(a => a.Id == _originalAsset.AccountId);
                    if (_originalAsset.ResponsiblePersonId.HasValue)
                    {
                        SelectedResponsiblePerson = ResponsiblePersons.FirstOrDefault(p => p.Id == _originalAsset.ResponsiblePersonId.Value);
                    }
                }
                else
                {
                    // Значения по умолчанию для нового объекта
                    SelectedAssetType = AssetTypes.FirstOrDefault();
                    SelectedAccount = Accounts.FirstOrDefault(a => a.Code == "01.1");
                    SelectedResponsiblePerson = ResponsiblePersons.FirstOrDefault();
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

        partial void OnSelectedAssetTypeChanged(AssetTypeDto? value)
        {
            if (value != null)
            {
                Asset.AssetTypeId = value.Id;
                Asset.AssetTypeName = value.Name;
            }
        }

        partial void OnSelectedAccountChanged(AccountDto? value)
        {
            if (value != null)
            {
                Asset.AccountId = value.Id;
                Asset.AccountCode = value.Code;
                Asset.AccountName = value.Name;
            }
        }

        partial void OnSelectedResponsiblePersonChanged(CounterpartyDto? value)
        {
            if (value != null)
            {
                if (value.Id > 0)
                {
                    Asset.ResponsiblePersonId = value.Id;
                    Asset.ResponsiblePersonName = value.ShortName;
                }
                else
                {
                    Asset.ResponsiblePersonId = null;
                    Asset.ResponsiblePersonName = null;
                }
            }
        }

        partial void OnSelectedYearChanged(int? value)
        {
            Asset.YearOfManufacture = value;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Asset.Name))
                {
                    MessageBox.Show(_window, "Введите наименование объекта", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedAssetType == null)
                {
                    MessageBox.Show(_window, "Выберите тип объекта", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedAccount == null)
                {
                    MessageBox.Show(_window, "Выберите счет учета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка инвентарного номера
                if (!string.IsNullOrWhiteSpace(Asset.InventoryNumber))
                {
                    if (!await _assetService.IsInventoryNumberUniqueAsync(Asset.InventoryNumber, Asset.Id > 0 ? Asset.Id : null))
                    {
                        MessageBox.Show(_window, $"Инвентарный номер '{Asset.InventoryNumber}' уже используется", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Валидация дат
                if (Asset.PurchaseDate.HasValue && Asset.CommissioningDate.HasValue &&
                    Asset.PurchaseDate > Asset.CommissioningDate)
                {
                    MessageBox.Show(_window, "Дата приобретения не может быть позже даты ввода в эксплуатацию", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Asset.CommissioningDate.HasValue && Asset.DecommissioningDate.HasValue &&
                    Asset.CommissioningDate > Asset.DecommissioningDate)
                {
                    MessageBox.Show(_window, "Дата ввода не может быть позже даты списания", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Asset.Id > 0)
                {
                    // Редактирование
                    await _assetService.UpdateAssetAsync(Asset);
                    MessageBox.Show(_window, "Объект успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _assetService.CreateAssetAsync(Asset);
                    MessageBox.Show(_window, "Объект успешно добавлен", "Успех",
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