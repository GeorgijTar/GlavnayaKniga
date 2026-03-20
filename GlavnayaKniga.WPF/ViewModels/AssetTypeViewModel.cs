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
    public partial class AssetTypeViewModel : BaseViewModel
    {
        private readonly IAssetTypeService _assetTypeService;

        [ObservableProperty]
        private ObservableCollection<AssetTypeDto> _assetTypes;

        [ObservableProperty]
        private ObservableCollection<AssetTypeDto> _filteredAssetTypes;

        [ObservableProperty]
        private AssetTypeDto? _selectedAssetType;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        public AssetTypeViewModel(IAssetTypeService assetTypeService)
        {
            _assetTypeService = assetTypeService;
            _assetTypes = new ObservableCollection<AssetTypeDto>();
            _filteredAssetTypes = new ObservableCollection<AssetTypeDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка типов объектов...";

                var types = await _assetTypeService.GetAllAssetTypesAsync(ShowArchived);

                AssetTypes.Clear();
                foreach (var type in types.OrderBy(t => t.Name))
                {
                    AssetTypes.Add(type);
                }

                ApplyFilter();

                StatusMessage = $"Загружено типов: {AssetTypes.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки типов объектов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnShowArchivedChanged(bool value)
        {
            _ = LoadDataAsync();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredAssetTypes.Clear();
                foreach (var item in AssetTypes)
                {
                    FilteredAssetTypes.Add(item);
                }
                return;
            }

            var searchLower = SearchText.ToLower();
            var filtered = AssetTypes.Where(t =>
                t.Name.ToLower().Contains(searchLower) ||
                (t.Description != null && t.Description.ToLower().Contains(searchLower)));

            FilteredAssetTypes.Clear();
            foreach (var item in filtered)
            {
                FilteredAssetTypes.Add(item);
            }
        }

        [RelayCommand]
        private async Task AddAssetTypeAsync()
        {
            try
            {
                var window = new AssetTypeEditWindow();
                var viewModel = new AssetTypeEditViewModel(_assetTypeService, null, window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditAssetTypeAsync()
        {
            try
            {
                if (SelectedAssetType == null)
                {
                    MessageBox.Show("Выберите тип для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var typeToEdit = await _assetTypeService.GetAssetTypeByIdAsync(SelectedAssetType.Id);
                if (typeToEdit == null)
                {
                    MessageBox.Show("Тип не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new AssetTypeEditWindow();
                var viewModel = new AssetTypeEditViewModel(_assetTypeService, typeToEdit, window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ArchiveAssetTypeAsync()
        {
            try
            {
                if (SelectedAssetType == null)
                {
                    MessageBox.Show("Выберите тип для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedAssetType.IsArchived)
                {
                    MessageBox.Show("Тип уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedAssetType.AssetsCount > 0)
                {
                    var result = MessageBox.Show(
                        $"Тип '{SelectedAssetType.Name}' используется в {SelectedAssetType.AssetsCount} объектах.\n\n" +
                        "Архивация типа возможна только после архивации всех связанных объектов.\n\n" +
                        "Перейти к списку объектов этого типа?",
                        "Невозможно архивировать",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Переход к объектам данного типа будет реализован позже
                    }
                    return;
                }

                var confirmResult = MessageBox.Show(
                    $"Вы действительно хотите архивировать тип '{SelectedAssetType.Name}'?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация типа...";

                    var success = await _assetTypeService.ArchiveAssetTypeAsync(SelectedAssetType.Id);

                    if (success)
                    {
                        StatusMessage = "Тип успешно архивирован";
                        await LoadDataAsync();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
        private async Task UnarchiveAssetTypeAsync()
        {
            try
            {
                if (SelectedAssetType == null)
                {
                    MessageBox.Show("Выберите тип для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedAssetType.IsArchived)
                {
                    MessageBox.Show("Тип не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать тип '{SelectedAssetType.Name}'?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация типа...";

                    var success = await _assetTypeService.UnarchiveAssetTypeAsync(SelectedAssetType.Id);

                    if (success)
                    {
                        StatusMessage = "Тип успешно разархивирован";
                        await LoadDataAsync();
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
    }
}