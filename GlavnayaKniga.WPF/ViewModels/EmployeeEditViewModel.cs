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
    public partial class EmployeeEditViewModel : BaseViewModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IPositionService _positionService;
        private readonly IIndividualService _individualService;
        private readonly IDepartmentService _departmentService;
        private readonly EmployeeDto? _originalEmployee;
        private readonly Window _window;

        [ObservableProperty]
        private EmployeeDto _employee;

        [ObservableProperty]
        private ObservableCollection<IndividualDto> _individuals;

        [ObservableProperty]
        private IndividualDto? _selectedIndividual;

        [ObservableProperty]
        private ObservableCollection<PositionDto> _positions;

        [ObservableProperty]
        private PositionDto? _selectedPosition;

        [ObservableProperty]
        private ObservableCollection<DepartmentDto> _departments;

        [ObservableProperty]
        private DepartmentDto? _selectedDepartment;

        [ObservableProperty]
        private ObservableCollection<EmployeeDto> _managers;

        [ObservableProperty]
        private EmployeeDto? _selectedManager;

        [ObservableProperty]
        private ObservableCollection<string> _statuses;

        [ObservableProperty]
        private string _selectedStatus;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public EmployeeEditViewModel(
            IEmployeeService employeeService,
            IPositionService positionService,
            IIndividualService individualService,
            IDepartmentService departmentService,
            EmployeeDto? employeeToEdit,
            Window window)
        {
            _employeeService = employeeService;
            _positionService = positionService;
            _individualService = individualService;
            _departmentService = departmentService;
            _originalEmployee = employeeToEdit;
            _window = window;

            _individuals = new ObservableCollection<IndividualDto>();
            _positions = new ObservableCollection<PositionDto>();
            _departments = new ObservableCollection<DepartmentDto>();
            _managers = new ObservableCollection<EmployeeDto>();

            _statuses = new ObservableCollection<string>
            {
                "Active",
                "Probation",
                "OnLeave"
            };

            if (_originalEmployee != null)
            {
                _employee = new EmployeeDto
                {
                    Id = _originalEmployee.Id,
                    IndividualId = _originalEmployee.IndividualId,
                    IndividualFullName = _originalEmployee.IndividualFullName,
                    IndividualShortName = _originalEmployee.IndividualShortName,
                    PersonnelNumber = _originalEmployee.PersonnelNumber,
                    CurrentPositionId = _originalEmployee.CurrentPositionId,
                    CurrentPositionName = _originalEmployee.CurrentPositionName,
                    DepartmentId = _originalEmployee.DepartmentId,
                    DepartmentName = _originalEmployee.DepartmentName,
                    Status = _originalEmployee.Status,
                    HireDate = _originalEmployee.HireDate,
                    HireOrderNumber = _originalEmployee.HireOrderNumber,
                    HireOrderDate = _originalEmployee.HireOrderDate,
                    WorkPhone = _originalEmployee.WorkPhone,
                    WorkEmail = _originalEmployee.WorkEmail,
                    ManagerId = _originalEmployee.ManagerId,
                    ManagerName = _originalEmployee.ManagerName,
                    Note = _originalEmployee.Note
                };
                _selectedStatus = _originalEmployee.Status;
                Title = $"Редактирование: {_originalEmployee.IndividualShortName}";
                IsEditMode = true;
            }
            else
            {
                _employee = new EmployeeDto
                {
                    HireDate = DateTime.Today,
                    Status = "Active"
                };
                _selectedStatus = "Active";
                Title = "Добавление сотрудника";
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

                // Загружаем физические лица
                var individuals = await _individualService.GetAllIndividualsAsync(false);
                Individuals.Clear();
                foreach (var individual in individuals.OrderBy(i => i.LastName))
                {
                    Individuals.Add(individual);
                }

                // Загружаем должности
                var positions = await _positionService.GetAllPositionsAsync(false);
                Positions.Clear();
                foreach (var position in positions.OrderBy(p => p.Name))
                {
                    Positions.Add(position);
                }

                // Загружаем отделы
                var departments = await _departmentService.GetAllDepartmentsAsync(false);
                Departments.Clear();
                Departments.Add(new DepartmentDto { Id = 0, Name = "— Не выбран —" });
                foreach (var dept in departments.OrderBy(d => d.Name))
                {
                    Departments.Add(dept);
                }

                // Загружаем руководителей (только активные сотрудники)
                var employees = await _employeeService.GetAllEmployeesAsync(false);
                Managers.Clear();
                Managers.Add(new EmployeeDto { Id = 0, IndividualShortName = "— Не выбран —" });
                foreach (var emp in employees.OrderBy(e => e.IndividualShortName))
                {
                    Managers.Add(emp);
                }

                // Устанавливаем выбранные значения
                if (_originalEmployee != null)
                {
                    SelectedIndividual = Individuals.FirstOrDefault(i => i.Id == _originalEmployee.IndividualId);
                    SelectedPosition = Positions.FirstOrDefault(p => p.Id == _originalEmployee.CurrentPositionId);

                    if (_originalEmployee.DepartmentId.HasValue)
                    {
                        SelectedDepartment = Departments.FirstOrDefault(d => d.Id == _originalEmployee.DepartmentId.Value);
                    }
                    else
                    {
                        SelectedDepartment = Departments.FirstOrDefault(d => d.Id == 0);
                    }

                    if (_originalEmployee.ManagerId.HasValue)
                    {
                        SelectedManager = Managers.FirstOrDefault(m => m.Id == _originalEmployee.ManagerId.Value);
                    }
                    else
                    {
                        SelectedManager = Managers.FirstOrDefault(m => m.Id == 0);
                    }
                }
                else
                {
                    // Генерируем табельный номер для нового сотрудника
                    Employee.PersonnelNumber = await _employeeService.GeneratePersonnelNumberAsync();
                    SelectedDepartment = Departments.FirstOrDefault(d => d.Id == 0);
                    SelectedManager = Managers.FirstOrDefault(m => m.Id == 0);
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

        partial void OnSelectedIndividualChanged(IndividualDto? value)
        {
            if (value != null)
            {
                Employee.IndividualId = value.Id;
                Employee.IndividualFullName = value.FullName;
                Employee.IndividualShortName = value.ShortName;
                Employee.IndividualPhone = value.Phone;
                Employee.IndividualEmail = value.Email;
            }
        }

        partial void OnSelectedPositionChanged(PositionDto? value)
        {
            if (value != null)
            {
                Employee.CurrentPositionId = value.Id;
                Employee.CurrentPositionName = value.Name;
            }
        }

        partial void OnSelectedDepartmentChanged(DepartmentDto? value)
        {
            if (value != null)
            {
                Employee.DepartmentId = value.Id > 0 ? value.Id : null;
                Employee.DepartmentName = value.Name;
            }
        }

        partial void OnSelectedManagerChanged(EmployeeDto? value)
        {
            if (value != null)
            {
                Employee.ManagerId = value.Id > 0 ? value.Id : null;
                Employee.ManagerName = value.IndividualShortName;
            }
        }

        partial void OnSelectedStatusChanged(string value)
        {
            Employee.Status = value;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (SelectedIndividual == null)
                {
                    MessageBox.Show(_window, "Выберите физическое лицо", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedPosition == null)
                {
                    MessageBox.Show(_window, "Выберите должность", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Employee.PersonnelNumber))
                {
                    MessageBox.Show(_window, "Введите табельный номер", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Employee.Id > 0)
                {
                    // Редактирование
                    await _employeeService.UpdateEmployeeAsync(Employee);
                    MessageBox.Show(_window, "Сотрудник успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _employeeService.CreateEmployeeAsync(Employee);
                    MessageBox.Show(_window, "Сотрудник успешно добавлен", "Успех",
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

        // Добавляем команды для добавления физического лица и должности
        [RelayCommand]
        private void AddIndividual()
        {
            try
            {
                var window = new IndividualEditWindow();
                var viewModel = new IndividualEditViewModel(_individualService, null, window);

                window.DataContext = viewModel;
                window.Owner = _window;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = window.ShowDialog();
                if (result == true)
                {
                    // Обновляем список физических лиц
                    _ = LoadIndividualsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при создании физического лица: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditIndividualAsync(IndividualDto? individual)
        {
            try
            {
                if (individual == null)
                {
                    if (SelectedIndividual == null)
                    {
                        MessageBox.Show(_window, "Выберите физическое лицо для редактирования", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    individual = SelectedIndividual;
                }

                var individualToEdit = await _individualService.GetIndividualByIdAsync(individual.Id);
                if (individualToEdit == null)
                {
                    MessageBox.Show(_window, "Физическое лицо не найдено", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new IndividualEditWindow();
                var viewModel = new IndividualEditViewModel(_individualService, individualToEdit, window);

                window.DataContext = viewModel;
                window.Owner = _window;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = window.ShowDialog();
                if (result == true)
                {
                    // Обновляем список физических лиц
                    await LoadIndividualsAsync();

                    // Обновляем выбранного физического лица
                    var updated = Individuals.FirstOrDefault(i => i.Id == individual.Id);
                    if (updated != null)
                    {
                        SelectedIndividual = updated;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при редактировании физического лица: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddPosition()
        {
            try
            {
                var window = new PositionEditWindow();
                var viewModel = new PositionEditViewModel(_positionService, null, window);

                window.DataContext = viewModel;
                window.Owner = _window;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = window.ShowDialog();
                if (result == true)
                {
                    // Обновляем список должностей
                    _ = LoadPositionsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при создании должности: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditPositionAsync(PositionDto? position)
        {
            try
            {
                if (position == null)
                {
                    if (SelectedPosition == null)
                    {
                        MessageBox.Show(_window, "Выберите должность для редактирования", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    position = SelectedPosition;
                }

                var positionToEdit = await _positionService.GetPositionByIdAsync(position.Id);
                if (positionToEdit == null)
                {
                    MessageBox.Show(_window, "Должность не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new PositionEditWindow();
                var viewModel = new PositionEditViewModel(_positionService, positionToEdit, window);

                window.DataContext = viewModel;
                window.Owner = _window;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = window.ShowDialog();
                if (result == true)
                {
                    // Обновляем список должностей
                    await LoadPositionsAsync();

                    // Обновляем выбранную должность
                    var updated = Positions.FirstOrDefault(p => p.Id == position.Id);
                    if (updated != null)
                    {
                        SelectedPosition = updated;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при редактировании должности: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Добавляем методы для загрузки списков
        private async Task LoadIndividualsAsync()
        {
            var individuals = await _individualService.GetAllIndividualsAsync(false);
            Individuals.Clear();
            foreach (var individual in individuals.OrderBy(i => i.LastName))
            {
                Individuals.Add(individual);
            }
        }

        private async Task LoadPositionsAsync()
        {
            var positions = await _positionService.GetAllPositionsAsync(false);
            Positions.Clear();
            foreach (var position in positions.OrderBy(p => p.Name))
            {
                Positions.Add(position);
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