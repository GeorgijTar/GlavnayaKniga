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
    public partial class PositionsViewModel : BaseViewModel
    {
        private readonly IPositionService _positionService;

        [ObservableProperty]
        private ObservableCollection<PositionDto> _positions;

        [ObservableProperty]
        private ObservableCollection<PositionDto> _filteredPositions;

        [ObservableProperty]
        private PositionDto? _selectedPosition;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        [ObservableProperty]
        private ObservableCollection<string> _categoryFilters;

        [ObservableProperty]
        private string? _selectedCategoryFilter;

        public PositionsViewModel(IPositionService positionService)
        {
            _positionService = positionService;
            _positions = new ObservableCollection<PositionDto>();
            _filteredPositions = new ObservableCollection<PositionDto>();

            _categoryFilters = new ObservableCollection<string>
            {
                "Все категории",
                "Руководитель",
                "Специалист",
                "Рабочий",
                "Прочее"
            };
            _selectedCategoryFilter = "Все категории";

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка должностей...";

                var positions = await _positionService.GetAllPositionsAsync(ShowArchived);

                Positions.Clear();
                foreach (var position in positions.OrderBy(p => p.Name))
                {
                    Positions.Add(position);
                }

                ApplyFilter();

                StatusMessage = $"Загружено: {Positions.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}", "Ошибка",
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

        partial void OnSelectedCategoryFilterChanged(string? value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = Positions.AsEnumerable();

            // Фильтр по категории
            if (!string.IsNullOrWhiteSpace(SelectedCategoryFilter) && SelectedCategoryFilter != "Все категории")
            {
                filtered = filtered.Where(p => p.CategoryDisplay == SelectedCategoryFilter);
            }

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.ShortName != null && p.ShortName.ToLower().Contains(searchLower)));
            }

            FilteredPositions.Clear();
            foreach (var item in filtered)
            {
                FilteredPositions.Add(item);
            }
        }

        [RelayCommand]
        private async Task AddPositionAsync()
        {
            try
            {
                var window = new PositionEditWindow();
                var viewModel = new PositionEditViewModel(_positionService, null, window);

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
        private async Task EditPositionAsync()
        {
            try
            {
                if (SelectedPosition == null)
                {
                    MessageBox.Show("Выберите должность для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var positionToEdit = await _positionService.GetPositionByIdAsync(SelectedPosition.Id);
                if (positionToEdit == null)
                {
                    MessageBox.Show("Должность не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new PositionEditWindow();
                var viewModel = new PositionEditViewModel(_positionService, positionToEdit, window);

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
        private async Task ArchivePositionAsync()
        {
            try
            {
                if (SelectedPosition == null)
                {
                    MessageBox.Show("Выберите должность для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedPosition.IsArchived)
                {
                    MessageBox.Show("Должность уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать должность '{SelectedPosition.Name}'?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация...";

                    var success = await _positionService.ArchivePositionAsync(SelectedPosition.Id);

                    if (success)
                    {
                        StatusMessage = "Должность архивирована";
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
        private async Task UnarchivePositionAsync()
        {
            try
            {
                if (SelectedPosition == null)
                {
                    MessageBox.Show("Выберите должность для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedPosition.IsArchived)
                {
                    MessageBox.Show("Должность не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать должность '{SelectedPosition.Name}'?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация...";

                    var success = await _positionService.UnarchivePositionAsync(SelectedPosition.Id);

                    if (success)
                    {
                        StatusMessage = "Должность разархивирована";
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