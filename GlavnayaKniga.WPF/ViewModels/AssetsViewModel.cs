using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class AssetsViewModel : BaseViewModel
    {
        private readonly IAssetService _assetService;
        private readonly IAssetTypeService _assetTypeService;
        private readonly IAccountService _accountService;
        private readonly ICounterpartyService _counterpartyService;

        [ObservableProperty]
        private ObservableCollection<AssetGroupDto> _assetGroups;

        [ObservableProperty]
        private ObservableCollection<AssetTypeDto> _assetTypes;

        [ObservableProperty]
        private AssetTypeDto? _selectedAssetType;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        [ObservableProperty]
        private bool _isGroupedView = true;

        public AssetsViewModel(
            IAssetService assetService,
            IAssetTypeService assetTypeService,
            IAccountService accountService,
            ICounterpartyService counterpartyService)
        {
            _assetService = assetService;
            _assetTypeService = assetTypeService;
            _accountService = accountService;
            _counterpartyService = counterpartyService;

            _assetGroups = new ObservableCollection<AssetGroupDto>();
            _assetTypes = new ObservableCollection<AssetTypeDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка объектов...";

                // Загружаем типы для фильтра
                var types = await _assetTypeService.GetAllAssetTypesAsync(ShowArchived);
                AssetTypes.Clear();
                AssetTypes.Add(new AssetTypeDto { Id = 0, Name = "Все типы" });
                foreach (var type in types.OrderBy(t => t.Name))
                {
                    AssetTypes.Add(type);
                }
                SelectedAssetType = AssetTypes.FirstOrDefault();

                // Загружаем объекты
                await LoadAssetsAsync();

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки объектов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadAssetsAsync()
        {
            if (IsGroupedView)
            {
                var groups = await _assetService.GetAssetsGroupedByTypeAsync(ShowArchived);

                // Применяем фильтр по типу, если выбран конкретный тип
                if (SelectedAssetType?.Id > 0)
                {
                    groups = groups.Where(g => g.AssetType.Id == SelectedAssetType.Id);
                }

                // Применяем поиск
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    foreach (var group in groups)
                    {
                        group.Assets = group.Assets
                            .Where(a => a.Name.ToLower().Contains(searchLower) ||
                                       (a.RegistrationNumber != null && a.RegistrationNumber.ToLower().Contains(searchLower)) ||
                                       (a.InventoryNumber != null && a.InventoryNumber.ToLower().Contains(searchLower)))
                            .ToList();
                    }
                    groups = groups.Where(g => g.Assets.Any());
                }

                AssetGroups.Clear();
                foreach (var group in groups)
                {
                    AssetGroups.Add(group);
                }
            }
            else
            {
                // Для табличного представления реализуем позже
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = LoadAssetsAsync();
        }

        partial void OnShowArchivedChanged(bool value)
        {
            _ = LoadDataAsync();
        }

        partial void OnSelectedAssetTypeChanged(AssetTypeDto? value)
        {
            _ = LoadAssetsAsync();
        }

        partial void OnIsGroupedViewChanged(bool value)
        {
            _ = LoadAssetsAsync();
        }

        [RelayCommand]
        private async Task AddAssetAsync()
        {
            try
            {
                var window = new AssetEditWindow();
                var viewModel = new AssetEditViewModel(
                    _assetService,
                    _assetTypeService,
                    _accountService,
                    _counterpartyService,
                    null,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadAssetsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditAssetAsync(AssetDto? asset)
        {
            try
            {
                if (asset == null)
                {
                    MessageBox.Show("Выберите объект для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var assetToEdit = await _assetService.GetAssetByIdAsync(asset.Id);
                if (assetToEdit == null)
                {
                    MessageBox.Show("Объект не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new AssetEditWindow();
                var viewModel = new AssetEditViewModel(
                    _assetService,
                    _assetTypeService,
                    _accountService,
                    _counterpartyService,
                    assetToEdit,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadAssetsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ArchiveAssetAsync(AssetDto? asset)
        {
            try
            {
                if (asset == null)
                {
                    MessageBox.Show("Выберите объект для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (asset.IsArchived)
                {
                    MessageBox.Show("Объект уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать объект '{asset.Name}'?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация объекта...";

                    var success = await _assetService.ArchiveAssetAsync(asset.Id);

                    if (success)
                    {
                        StatusMessage = "Объект успешно архивирован";
                        await LoadAssetsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при архивации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UnarchiveAssetAsync(AssetDto? asset)
        {
            try
            {
                if (asset == null)
                {
                    MessageBox.Show("Выберите объект для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!asset.IsArchived)
                {
                    MessageBox.Show("Объект не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать объект '{asset.Name}'?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация объекта...";

                    var success = await _assetService.UnarchiveAssetAsync(asset.Id);

                    if (success)
                    {
                        StatusMessage = "Объект успешно разархивирован";
                        await LoadAssetsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при разархивации: {ex.Message}", "Ошибка",
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
            await LoadDataAsync();
        }

        [RelayCommand]
        private void ManageAssetTypes()
        {
            var window = new AssetTypesWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();

            // Обновляем типы после закрытия окна
            _ = LoadDataAsync();
        }
    }
}