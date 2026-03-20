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
    public partial class UnitsOfMeasureViewModel : BaseViewModel
    {
        private readonly IUnitOfMeasureService _unitService;

        [ObservableProperty]
        private ObservableCollection<UnitOfMeasureDto> _units;

        [ObservableProperty]
        private ObservableCollection<UnitOfMeasureDto> _filteredUnits;

        [ObservableProperty]
        private UnitOfMeasureDto? _selectedUnit;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        public UnitsOfMeasureViewModel(IUnitOfMeasureService unitService)
        {
            _unitService = unitService;
            _units = new ObservableCollection<UnitOfMeasureDto>();
            _filteredUnits = new ObservableCollection<UnitOfMeasureDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка единиц измерения...";

                var units = await _unitService.GetAllUnitsAsync(ShowArchived);

                Units.Clear();
                foreach (var unit in units.OrderBy(u => u.Code))
                {
                    Units.Add(unit);
                }

                ApplyFilter();

                StatusMessage = $"Загружено: {Units.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки единиц измерения: {ex.Message}", "Ошибка",
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
                FilteredUnits.Clear();
                foreach (var item in Units)
                {
                    FilteredUnits.Add(item);
                }
                return;
            }

            var searchLower = SearchText.ToLower();
            var filtered = Units.Where(u =>
                u.Code.ToLower().Contains(searchLower) ||
                u.ShortName.ToLower().Contains(searchLower) ||
                u.FullName.ToLower().Contains(searchLower) ||
                (u.InternationalCode != null && u.InternationalCode.ToLower().Contains(searchLower)));

            FilteredUnits.Clear();
            foreach (var item in filtered)
            {
                FilteredUnits.Add(item);
            }
        }

        [RelayCommand]
        private async Task AddUnitAsync()
        {
            try
            {
                var window = new UnitOfMeasureEditWindow();
                var viewModel = new UnitOfMeasureEditViewModel(_unitService, null, window);

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
        private async Task EditUnitAsync()
        {
            try
            {
                if (SelectedUnit == null)
                {
                    MessageBox.Show("Выберите единицу измерения для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var unitToEdit = await _unitService.GetUnitByIdAsync(SelectedUnit.Id);
                if (unitToEdit == null)
                {
                    MessageBox.Show("Единица измерения не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new UnitOfMeasureEditWindow();
                var viewModel = new UnitOfMeasureEditViewModel(_unitService, unitToEdit, window);

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
        private async Task ArchiveUnitAsync()
        {
            try
            {
                if (SelectedUnit == null)
                {
                    MessageBox.Show("Выберите единицу измерения для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedUnit.IsArchived)
                {
                    MessageBox.Show("Единица измерения уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать единицу измерения '{SelectedUnit.DisplayName}'?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация...";

                    var success = await _unitService.ArchiveUnitAsync(SelectedUnit.Id);

                    if (success)
                    {
                        StatusMessage = "Единица измерения архивирована";
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
        private async Task UnarchiveUnitAsync()
        {
            try
            {
                if (SelectedUnit == null)
                {
                    MessageBox.Show("Выберите единицу измерения для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedUnit.IsArchived)
                {
                    MessageBox.Show("Единица измерения не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать единицу измерения '{SelectedUnit.DisplayName}'?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация...";

                    var success = await _unitService.UnarchiveUnitAsync(SelectedUnit.Id);

                    if (success)
                    {
                        StatusMessage = "Единица измерения разархивирована";
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