using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class StorageLocationsViewModel : BaseViewModel
    {
        private readonly IStorageLocationService _storageLocationService;
        private readonly IEmployeeService _employeeService;

        [ObservableProperty]
        private ObservableCollection<StorageLocationDto> _locations;

        [ObservableProperty]
        private ObservableCollection<StorageLocationDto> _filteredLocations;

        [ObservableProperty]
        private StorageLocationDto? _selectedLocation;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        [ObservableProperty]
        private bool _showTreeView = true;

        public StorageLocationsViewModel(
            IStorageLocationService storageLocationService,
             IEmployeeService employeeService)
        {
            _storageLocationService = storageLocationService;
            _employeeService = employeeService;
            _locations = new ObservableCollection<StorageLocationDto>();
            _filteredLocations = new ObservableCollection<StorageLocationDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка мест хранения...";

                var locations = await _storageLocationService.GetLocationsHierarchyAsync(ShowArchived);

                Locations.Clear();
                foreach (var location in locations)
                {
                    Locations.Add(location);
                }

                ApplyFilter();

                StatusMessage = $"Загружено мест хранения: {CountLocations(Locations)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки мест хранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private int CountLocations(ObservableCollection<StorageLocationDto> locations)
        {
            int count = locations.Count;
            foreach (var location in locations)
            {
                count += CountLocations(location.Children);
            }
            return count;
        }

        private int CountLocations(List<StorageLocationDto> locations)
        {
            int count = locations.Count;
            foreach (var location in locations)
            {
                count += CountLocations(location.Children);
            }
            return count;
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
                // В древовидном режиме показываем все
                if (ShowTreeView)
                {
                    FilteredLocations.Clear();
                    foreach (var item in Locations)
                    {
                        FilteredLocations.Add(item);
                    }
                }
                else
                {
                    // В табличном режиме нужно собрать все элементы
                    var allItems = FlattenLocationsToList(Locations);
                    FilteredLocations.Clear();
                    foreach (var item in allItems.OrderBy(l => l.Code))
                    {
                        FilteredLocations.Add(item);
                    }
                }
                return;
            }

            var searchLower = SearchText.ToLower();

            if (ShowTreeView)
            {
                // Для древовидного режима фильтруем с сохранением иерархии
                var filtered = FilterLocationHierarchy(Locations, searchLower);
                FilteredLocations.Clear();
                foreach (var item in filtered)
                {
                    FilteredLocations.Add(item);
                }
            }
            else
            {
                // Для табличного режима просто фильтруем плоский список
                var allItems = FlattenLocationsToList(Locations);
                var filtered = allItems.Where(l =>
                    l.Code.ToLower().Contains(searchLower) ||
                    l.Name.ToLower().Contains(searchLower) ||
                    (l.Description != null && l.Description.ToLower().Contains(searchLower)) ||
                    (l.Address != null && l.Address.ToLower().Contains(searchLower)));

                FilteredLocations.Clear();
                foreach (var item in filtered.OrderBy(l => l.Code))
                {
                    FilteredLocations.Add(item);
                }
            }
        }

        // Исправлено: возвращаем List<StorageLocationDto>
        private List<StorageLocationDto> FlattenLocationsToList(ObservableCollection<StorageLocationDto> locations)
        {
            var result = new List<StorageLocationDto>();
            foreach (var location in locations)
            {
                result.Add(location);
                result.AddRange(FlattenLocationsToList(location.Children));
            }
            return result;
        }

        // Перегрузка для List
        private List<StorageLocationDto> FlattenLocationsToList(List<StorageLocationDto> locations)
        {
            var result = new List<StorageLocationDto>();
            foreach (var location in locations)
            {
                result.Add(location);
                result.AddRange(FlattenLocationsToList(location.Children));
            }
            return result;
        }

        // Исправлено: возвращаем ObservableCollection<StorageLocationDto>
        private ObservableCollection<StorageLocationDto> FilterLocationHierarchy(
            ObservableCollection<StorageLocationDto> locations,
            string searchLower)
        {
            var result = new ObservableCollection<StorageLocationDto>();

            foreach (var location in locations)
            {
                bool locationMatches = location.Code.ToLower().Contains(searchLower) ||
                                      location.Name.ToLower().Contains(searchLower) ||
                                      (location.Description != null && location.Description.ToLower().Contains(searchLower));

                var filteredChildren = FilterLocationHierarchy(location.Children, searchLower);

                if (locationMatches || filteredChildren.Any())
                {
                    var filteredLocation = new StorageLocationDto
                    {
                        Id = location.Id,
                        Code = location.Code,
                        Name = location.Name,
                        Type = location.Type,
                        TypeDisplay = location.TypeDisplay,
                        ParentId = location.ParentId,
                        ParentName = location.ParentName,
                        ParentCode = location.ParentCode,
                        Description = location.Description,
                        Address = location.Address,
                        ResponsibleEmployeeId = location.ResponsibleEmployeeId,
                        ResponsibleEmployeeName = location.ResponsibleEmployeeName,
                        Area = location.Area,
                        Capacity = location.Capacity,
                        TemperatureRegime = location.TemperatureRegime,
                        IsArchived = location.IsArchived,
                        Children = filteredChildren.ToList() // Конвертируем ObservableCollection в List
                    };
                    result.Add(filteredLocation);
                }
            }

            return result;
        }

        // Перегрузка для List
        private ObservableCollection<StorageLocationDto> FilterLocationHierarchy(
            List<StorageLocationDto> locations,
            string searchLower)
        {
            var result = new ObservableCollection<StorageLocationDto>();

            foreach (var location in locations)
            {
                bool locationMatches = location.Code.ToLower().Contains(searchLower) ||
                                      location.Name.ToLower().Contains(searchLower) ||
                                      (location.Description != null && location.Description.ToLower().Contains(searchLower));

                var filteredChildren = FilterLocationHierarchy(location.Children, searchLower);

                if (locationMatches || filteredChildren.Any())
                {
                    var filteredLocation = new StorageLocationDto
                    {
                        Id = location.Id,
                        Code = location.Code,
                        Name = location.Name,
                        Type = location.Type,
                        TypeDisplay = location.TypeDisplay,
                        ParentId = location.ParentId,
                        ParentName = location.ParentName,
                        ParentCode = location.ParentCode,
                        Description = location.Description,
                        Address = location.Address,
                        ResponsibleEmployeeId = location.ResponsibleEmployeeId,
                        ResponsibleEmployeeName = location.ResponsibleEmployeeName,
                        Area = location.Area,
                        Capacity = location.Capacity,
                        TemperatureRegime = location.TemperatureRegime,
                        IsArchived = location.IsArchived,
                        Children = filteredChildren.ToList()
                    };
                    result.Add(filteredLocation);
                }
            }

            return result;
        }

        partial void OnShowTreeViewChanged(bool value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        private async Task AddLocationAsync()
        {
            try
            {
                var window = new StorageLocationEditWindow();
                var viewModel = new StorageLocationEditViewModel(
                    _storageLocationService,
                    _employeeService,
                    null,
                    window);

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
        private async Task EditLocationAsync()
        {
            try
            {
                if (SelectedLocation == null)
                {
                    MessageBox.Show("Выберите место хранения для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var locationToEdit = await _storageLocationService.GetLocationByIdAsync(SelectedLocation.Id);
                if (locationToEdit == null)
                {
                    MessageBox.Show("Место хранения не найдено", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new StorageLocationEditWindow();
                var viewModel = new StorageLocationEditViewModel(
                    _storageLocationService,
                    _employeeService,
                    locationToEdit,
                    window);

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
        private async Task AddChildLocationAsync()
        {
            try
            {
                if (SelectedLocation == null)
                {
                    MessageBox.Show("Выберите родительское место хранения", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var window = new StorageLocationEditWindow();
                var viewModel = new StorageLocationEditViewModel(
                    _storageLocationService,
                    _employeeService,
                    SelectedLocation,
                    window,
                    true); // Режим добавления дочернего элемента

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
        private async Task ArchiveLocationAsync()
        {
            try
            {
                if (SelectedLocation == null)
                {
                    MessageBox.Show("Выберите место хранения для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedLocation.IsArchived)
                {
                    MessageBox.Show("Место хранения уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать место хранения '{SelectedLocation.DisplayName}'?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация места хранения...";

                    var success = await _storageLocationService.ArchiveLocationAsync(SelectedLocation.Id);

                    if (success)
                    {
                        StatusMessage = "Место хранения успешно архивировано";
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
        private async Task UnarchiveLocationAsync()
        {
            try
            {
                if (SelectedLocation == null)
                {
                    MessageBox.Show("Выберите место хранения для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedLocation.IsArchived)
                {
                    MessageBox.Show("Место хранения не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать место хранения '{SelectedLocation.DisplayName}'?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация места хранения...";

                    var success = await _storageLocationService.UnarchiveLocationAsync(SelectedLocation.Id);

                    if (success)
                    {
                        StatusMessage = "Место хранения успешно разархивировано";
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