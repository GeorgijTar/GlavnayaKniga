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
    public partial class StorageLocationEditViewModel : BaseViewModel
    {
        private readonly IStorageLocationService _storageLocationService;
        private readonly IEmployeeService _employeeService;
        private readonly StorageLocationDto? _originalLocation;
        private readonly StorageLocationDto? _parentLocation;
        private readonly bool _isChildMode;
        private readonly Window _window;

        [ObservableProperty]
        private StorageLocationDto _location;

        [ObservableProperty]
        private ObservableCollection<string> _locationTypes;

        [ObservableProperty]
        private string _selectedType;

        [ObservableProperty]
        private ObservableCollection<StorageLocationDto> _parentLocations;

        [ObservableProperty]
        private StorageLocationDto? _selectedParentLocation;

        [ObservableProperty]
        private ObservableCollection<EmployeeDto> _responsibleEmployees;

        [ObservableProperty]
        private EmployeeDto? _selectedResponsibleEmployee;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public StorageLocationEditViewModel(
            IStorageLocationService storageLocationService,
            IEmployeeService employeeService,
            StorageLocationDto? locationToEdit,
            Window window,
            bool isChildMode = false)
        {
            _storageLocationService = storageLocationService;
            _employeeService = employeeService;
            _originalLocation = locationToEdit;
            _window = window;
            _isChildMode = isChildMode;

            _locationTypes = new ObservableCollection<string>
            {
                "Склад",
                "Участок/Отделение",
                "Ячейка",
                "Стеллаж",
                "Открытая площадка",
                "Транспортное средство",
                "Прочее"
            };

            _parentLocations = new ObservableCollection<StorageLocationDto>();
            _responsibleEmployees = new ObservableCollection<EmployeeDto>();

            if (_originalLocation != null)
            {
                _location = new StorageLocationDto
                {
                    Id = _originalLocation.Id,
                    Code = _originalLocation.Code,
                    Name = _originalLocation.Name,
                    Type = _originalLocation.Type,
                    TypeDisplay = _originalLocation.TypeDisplay,
                    ParentId = _originalLocation.ParentId,
                    ParentName = _originalLocation.ParentName,
                    ParentCode = _originalLocation.ParentCode,
                    Description = _originalLocation.Description,
                    Address = _originalLocation.Address,
                    ResponsibleEmployeeId = _originalLocation.ResponsibleEmployeeId,
                    ResponsibleEmployeeName = _originalLocation.ResponsibleEmployeeName,
                    Area = _originalLocation.Area,
                    Capacity = _originalLocation.Capacity,
                    TemperatureRegime = _originalLocation.TemperatureRegime,
                    IsArchived = _originalLocation.IsArchived
                };
                _selectedType = _originalLocation.TypeDisplay;
                Title = "Редактирование места хранения";
                IsEditMode = true;
            }
            else
            {
                _location = new StorageLocationDto
                {
                    Type = "Warehouse",
                    TypeDisplay = "Склад"
                };
                _selectedType = "Склад";
                Title = _isChildMode ? "Добавление дочернего места хранения" : "Добавление места хранения";
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

                // Загружаем возможные родительские места хранения
                var allLocations = await _storageLocationService.GetAllLocationsAsync(true);

                ParentLocations.Clear();

                // Создаем элемент для корневого уровня
                var rootItem = new StorageLocationDto
                {
                    Id = 0,
                    Code = "",
                    Name = "Корневой уровень"
                };
                ParentLocations.Add(rootItem);

                // Исключаем текущее место и его потомков из списка родительских
                var excludedIds = new HashSet<int>();
                if (_originalLocation != null)
                {
                    excludedIds.Add(_originalLocation.Id);
                    await AddChildIdsAsync(_originalLocation.Id, excludedIds);
                }

                foreach (var location in allLocations.Where(l => !excludedIds.Contains(l.Id)).OrderBy(l => l.Code))
                {
                    ParentLocations.Add(location);
                }

                // Загружаем ответственных сотрудников (активные сотрудники)
                var employees = await _employeeService.GetAllEmployeesAsync(false);
                ResponsibleEmployees.Clear();

                // Создаем элемент для "Не выбран"
                var noneEmployee = new EmployeeDto { Id = 0, IndividualShortName = "— Не выбран —" };
                ResponsibleEmployees.Add(noneEmployee);

                foreach (var emp in employees.OrderBy(e => e.IndividualShortName))
                {
                    ResponsibleEmployees.Add(emp);
                }

                if (_originalLocation != null)
                {
                    if (_originalLocation.ParentId.HasValue)
                    {
                        SelectedParentLocation = ParentLocations.FirstOrDefault(p => p.Id == _originalLocation.ParentId.Value);
                    }
                    else
                    {
                        SelectedParentLocation = ParentLocations.FirstOrDefault(p => p.Id == 0);
                    }

                    if (_originalLocation.ResponsibleEmployeeId.HasValue)
                    {
                        SelectedResponsibleEmployee = ResponsibleEmployees.FirstOrDefault(e => e.Id == _originalLocation.ResponsibleEmployeeId.Value);
                    }
                    else
                    {
                        SelectedResponsibleEmployee = ResponsibleEmployees.FirstOrDefault(e => e.Id == 0);
                    }
                }
                else if (_parentLocation != null)
                {
                    SelectedParentLocation = ParentLocations.FirstOrDefault(p => p.Id == _parentLocation.Id);
                    SelectedResponsibleEmployee = ResponsibleEmployees.FirstOrDefault(e => e.Id == 0);
                }
                else
                {
                    SelectedParentLocation = ParentLocations.FirstOrDefault(p => p.Id == 0);
                    SelectedResponsibleEmployee = ResponsibleEmployees.FirstOrDefault(e => e.Id == 0);
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

        private async Task AddChildIdsAsync(int parentId, HashSet<int> ids)
        {
            var children = await _storageLocationService.GetAllLocationsAsync(true);
            var childLocations = children.Where(c => c.ParentId == parentId);

            foreach (var child in childLocations)
            {
                ids.Add(child.Id);
                await AddChildIdsAsync(child.Id, ids);
            }
        }

        partial void OnSelectedTypeChanged(string value)
        {
            Location.TypeDisplay = value;
            Location.Type = value switch
            {
                "Склад" => "Warehouse",
                "Участок/Отделение" => "Section",
                "Ячейка" => "Cell",
                "Стеллаж" => "Rack",
                "Открытая площадка" => "Outdoor",
                "Транспортное средство" => "Vehicle",
                _ => "Other"
            };
        }

        partial void OnSelectedParentLocationChanged(StorageLocationDto? value)
        {
            if (value != null)
            {
                if (value.Id > 0)
                {
                    Location.ParentId = value.Id;
                    Location.ParentName = value.Name;
                    Location.ParentCode = value.Code;
                }
                else
                {
                    Location.ParentId = null;
                    Location.ParentName = null;
                    Location.ParentCode = null;
                }
            }
        }

        partial void OnSelectedResponsibleEmployeeChanged(EmployeeDto? value)
        {
            if (value != null)
            {
                if (value.Id > 0)
                {
                    Location.ResponsibleEmployeeId = value.Id;
                    Location.ResponsibleEmployeeName = value.IndividualShortName;
                }
                else
                {
                    Location.ResponsibleEmployeeId = null;
                    Location.ResponsibleEmployeeName = null;
                }
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Location.Code))
                {
                    MessageBox.Show(_window, "Введите код места хранения", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Location.Name))
                {
                    MessageBox.Show(_window, "Введите наименование", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка уникальности кода
                if (!await _storageLocationService.IsCodeUniqueAsync(Location.Code, Location.Id > 0 ? Location.Id : null))
                {
                    MessageBox.Show(_window, $"Код '{Location.Code}' уже используется", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка циклической ссылки
                if (Location.ParentId.HasValue && Location.ParentId.Value == Location.Id)
                {
                    MessageBox.Show(_window, "Место хранения не может быть родителем для самого себя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Валидация площади и вместимости
                if (Location.Area.HasValue && Location.Area < 0)
                {
                    MessageBox.Show(_window, "Площадь не может быть отрицательной", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Location.Capacity.HasValue && Location.Capacity < 0)
                {
                    MessageBox.Show(_window, "Вместимость не может быть отрицательной", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Location.Id > 0)
                {
                    // Редактирование
                    await _storageLocationService.UpdateLocationAsync(Location);
                    MessageBox.Show(_window, "Место хранения успешно обновлено", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _storageLocationService.CreateLocationAsync(Location);
                    MessageBox.Show(_window, "Место хранения успешно добавлено", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _window.DialogResult = true;
                _window.Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(_window, ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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