using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Helpers;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class NomenclatureEditViewModel : BaseViewModel
    {
        private readonly INomenclatureService _nomenclatureService;
        private readonly IAccountService _accountService;
        private readonly IStorageLocationService _storageLocationService;
        private readonly IUnitOfMeasureService _unitService;
        private readonly NomenclatureDto? _originalItem;
        private readonly Window _window;

        [ObservableProperty]
        private NomenclatureDto _item;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _accounts;

        [ObservableProperty]
        private AccountDto? _selectedAccount;

        [ObservableProperty]
        private ObservableCollection<string> _types;

        [ObservableProperty]
        private string _selectedType;

        [ObservableProperty]
        private ObservableCollection<UnitOfMeasureDto> _units;

        [ObservableProperty]
        private UnitOfMeasureDto? _selectedUnit;

        [ObservableProperty]
        private ObservableCollection<StorageLocationDto> _storageLocations;

        [ObservableProperty]
        private StorageLocationDto? _selectedStorageLocation;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _vatAccounts;

        [ObservableProperty]
        private AccountDto? _selectedVatAccount;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public NomenclatureEditViewModel(
            INomenclatureService nomenclatureService,
            IAccountService accountService,
            IStorageLocationService storageLocationService,
            IUnitOfMeasureService unitService,
            NomenclatureDto? itemToEdit,
            Window window)
        {
            _nomenclatureService = nomenclatureService;
            _accountService = accountService;
            _storageLocationService = storageLocationService;
            _unitService = unitService;
            _originalItem = itemToEdit;
            _window = window;

            _accounts = new ObservableCollection<AccountDto>();
            _vatAccounts = new ObservableCollection<AccountDto>();
            _units = new ObservableCollection<UnitOfMeasureDto>();
            _storageLocations = new ObservableCollection<StorageLocationDto>();
            _types = new ObservableCollection<string>(NomenclatureHelper.GetAllRussianTypes());

            if (_originalItem != null)
            {
                _item = new NomenclatureDto
                {
                    Id = _originalItem.Id,
                    Name = _originalItem.Name,
                    FullName = _originalItem.FullName,
                    Article = _originalItem.Article,
                    Barcode = _originalItem.Barcode,
                    Type = _originalItem.Type,
                    TypeDisplay = _originalItem.TypeDisplay,
                    UnitId = _originalItem.UnitId,
                    AccountId = _originalItem.AccountId,
                    DefaultVatAccountId = _originalItem.DefaultVatAccountId,
                    PurchasePrice = _originalItem.PurchasePrice,
                    SalePrice = _originalItem.SalePrice,
                    CurrentStock = _originalItem.CurrentStock,
                    MinStock = _originalItem.MinStock,
                    MaxStock = _originalItem.MaxStock,
                    Description = _originalItem.Description,
                    Note = _originalItem.Note,
                    IsArchived = _originalItem.IsArchived
                };
                _selectedType = _originalItem.TypeDisplay;
                Title = "Редактирование номенклатуры";
                IsEditMode = true;
            }
            else
            {
                _item = new NomenclatureDto
                {
                    Type = "Material",
                    TypeDisplay = "Материалы",
                    CurrentStock = 0
                };
                _selectedType = "Материалы";
                Title = "Добавление номенклатуры";
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

                // Загружаем счета
                var accounts = await _accountService.GetAllAccountsAsync(false);

                // Фильтруем счета для учета (дебетовые счета)
                var debitAccounts = accounts.Where(a =>
                    a.Code.StartsWith("10") ||  // Материалы
                    a.Code.StartsWith("41") ||  // Товары
                    a.Code.StartsWith("43") ||  // Готовая продукция
                    a.Code.StartsWith("07") ||  // Оборудование к установке
                    a.Code.StartsWith("08") ||  // Вложения
                    a.Code.StartsWith("20") ||  // Основное производство
                    a.Code.StartsWith("23") ||  // Вспомогательное производство
                    a.Code.StartsWith("25") ||  // Общепроизводственные расходы
                    a.Code.StartsWith("26"));   // Общехозяйственные расходы

                Accounts.Clear();
                foreach (var account in debitAccounts.OrderBy(a => a.Code))
                {
                    Accounts.Add(account);
                }

                // Загружаем счета для НДС (счета 19)
                var vatAccounts = accounts.Where(a => a.Code.StartsWith("19"));
                VatAccounts.Clear();
                foreach (var account in vatAccounts.OrderBy(a => a.Code))
                {
                    VatAccounts.Add(account);
                }

                // Загружаем единицы измерения
                var units = await _unitService.GetAllUnitsAsync(false);
                Units.Clear();
                foreach (var unit in units.OrderBy(u => u.Code))
                {
                    Units.Add(unit);
                }

                // Загружаем места хранения
                var storageLocations = await _storageLocationService.GetLocationsHierarchyAsync(false);
                StorageLocations.Clear();
                StorageLocations.Add(new StorageLocationDto { Id = 0, Name = "— Не выбрано —" });
                foreach (var location in storageLocations)
                {
                    AddLocationWithChildren(location, 0);
                }

                // Устанавливаем выбранные значения
                if (_originalItem != null)
                {
                    SelectedAccount = Accounts.FirstOrDefault(a => a.Id == _originalItem.AccountId);
                    SelectedVatAccount = VatAccounts.FirstOrDefault(a => a.Id == _originalItem.DefaultVatAccountId);
                    SelectedUnit = Units.FirstOrDefault(u => u.Id == _originalItem.UnitId);

                    if (_originalItem.StorageLocationId.HasValue)
                    {
                        SelectedStorageLocation = StorageLocations.FirstOrDefault(s => s.Id == _originalItem.StorageLocationId.Value);
                    }
                    else
                    {
                        SelectedStorageLocation = StorageLocations.FirstOrDefault(s => s.Id == 0);
                    }
                }
                else
                {
                    // По умолчанию выбираем первый счет 10.1
                    SelectedAccount = Accounts.FirstOrDefault(a => a.Code == "10.1") ?? Accounts.FirstOrDefault();
                    SelectedVatAccount = VatAccounts.FirstOrDefault(a => a.Code == "19.1") ?? VatAccounts.FirstOrDefault();
                    SelectedUnit = Units.FirstOrDefault();
                    SelectedStorageLocation = StorageLocations.FirstOrDefault(s => s.Id == 0);
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

        partial void OnSelectedTypeChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            try
            {
                var typeEnum = NomenclatureHelper.GetTypeFromRussian(value);
                Item.Type = typeEnum.ToString();
                Item.TypeDisplay = value;

                AutoSelectAccountByType(typeEnum);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при смене типа: {ex.Message}");
            }
        }

        partial void OnSelectedUnitChanged(UnitOfMeasureDto? value)
        {
            if (value != null)
            {
                Item.UnitId = value.Id;
            }
        }

        partial void OnSelectedAccountChanged(AccountDto? value)
        {
            if (value != null)
            {
                Item.AccountId = value.Id;
                Item.AccountCode = value.Code;
                Item.AccountName = value.Name;

                // Автоматически подбираем счет НДС в зависимости от счета учета
                AutoSelectVatAccountByAccount(value);
            }
        }

        partial void OnSelectedVatAccountChanged(AccountDto? value)
        {
            if (value != null)
            {
                Item.DefaultVatAccountId = value.Id;
                Item.DefaultVatAccountCode = value.Code;
                Item.DefaultVatAccountName = value.Name;
            }
        }

        partial void OnSelectedStorageLocationChanged(StorageLocationDto? value)
        {
            if (value != null)
            {
                Item.StorageLocationId = value.Id > 0 ? value.Id : null;
                Item.StorageLocationName = value.Name;
            }
        }

        private void AutoSelectAccountByType(NomenclatureType type)
        {
            if (Accounts == null || !Accounts.Any()) return;

            string? targetAccountCode = type switch
            {
                NomenclatureType.Material => "10.1",
                NomenclatureType.Inventory => "10.9",
                NomenclatureType.Fertilizer => "10.1",
                NomenclatureType.PlantProtection => "10.1",
                NomenclatureType.Seeds => "10.1",
                NomenclatureType.Fuel => "10.3",
                NomenclatureType.SpareParts => "10.5",
                NomenclatureType.Equipment => "07",
                NomenclatureType.Work => "20",
                NomenclatureType.Service => "20",
                _ => "10.6"
            };

            if (!string.IsNullOrWhiteSpace(targetAccountCode))
            {
                var targetAccount = Accounts.FirstOrDefault(a => a.Code == targetAccountCode);
                if (targetAccount != null)
                {
                    SelectedAccount = targetAccount;
                }
            }
        }

        private void AutoSelectVatAccountByAccount(AccountDto account)
        {
            if (VatAccounts == null || !VatAccounts.Any()) return;

            // Для счетов 41 (товары) используем 19.2, для остальных 19.1
            string targetVatAccountCode = account.Code.StartsWith("41") ? "19.2" : "19.1";

            var targetVatAccount = VatAccounts.FirstOrDefault(a => a.Code == targetVatAccountCode);
            if (targetVatAccount != null)
            {
                SelectedVatAccount = targetVatAccount;
            }
            else
            {
                SelectedVatAccount = VatAccounts.FirstOrDefault();
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Item.Name))
                {
                    MessageBox.Show(_window, "Введите наименование", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedAccount == null)
                {
                    MessageBox.Show(_window, "Выберите счет учета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedVatAccount == null)
                {
                    MessageBox.Show(_window, "Выберите счет учета НДС", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedUnit == null)
                {
                    MessageBox.Show(_window, "Выберите единицу измерения", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedType))
                {
                    MessageBox.Show(_window, "Выберите тип номенклатуры", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка артикула на уникальность
                if (!string.IsNullOrWhiteSpace(Item.Article))
                {
                    if (!await _nomenclatureService.IsArticleUniqueAsync(Item.Article, Item.Id > 0 ? Item.Id : null))
                    {
                        MessageBox.Show(_window, $"Артикул '{Item.Article}' уже используется", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Валидация цен
                if (Item.PurchasePrice.HasValue && Item.PurchasePrice < 0)
                {
                    MessageBox.Show(_window, "Цена закупки не может быть отрицательной", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Item.SalePrice.HasValue && Item.SalePrice < 0)
                {
                    MessageBox.Show(_window, "Цена продажи не может быть отрицательной", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Валидация остатков
                if (Item.MinStock.HasValue && Item.MaxStock.HasValue && Item.MinStock > Item.MaxStock)
                {
                    MessageBox.Show(_window, "Минимальный остаток не может быть больше максимального", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Item.Id > 0)
                {
                    await _nomenclatureService.UpdateNomenclatureAsync(Item);
                    MessageBox.Show(_window, "Номенклатура успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _nomenclatureService.CreateNomenclatureAsync(Item);
                    MessageBox.Show(_window, "Номенклатура успешно добавлена", "Успех",
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

        [RelayCommand]
        private void ClearArticle()
        {
            Item.Article = null;
        }

        [RelayCommand]
        private void ClearBarcode()
        {
            Item.Barcode = null;
        }

        [RelayCommand]
        private void ClearStorageLocation()
        {
            SelectedStorageLocation = StorageLocations.FirstOrDefault(s => s.Id == 0);
        }
    }
}