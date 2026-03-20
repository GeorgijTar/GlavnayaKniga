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
    public partial class EmployeesViewModel : BaseViewModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IPositionService _positionService;
        private readonly IIndividualService _individualService;
        private readonly IDepartmentService _departmentService; // Добавляем сервис отделов

        [ObservableProperty]
        private ObservableCollection<EmployeeDto> _employees;

        [ObservableProperty]
        private ObservableCollection<EmployeeDto> _filteredEmployees;

        [ObservableProperty]
        private EmployeeDto? _selectedEmployee;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _includeDismissed;

        [ObservableProperty]
        private ObservableCollection<string> _departmentFilters;

        [ObservableProperty]
        private string? _selectedDepartmentFilter;

        [ObservableProperty]
        private ObservableCollection<PositionDto> _positions;

        public EmployeesViewModel(
            IEmployeeService employeeService,
            IPositionService positionService,
            IIndividualService individualService,
            IDepartmentService departmentService) // Добавляем параметр
        {
            _employeeService = employeeService;
            _positionService = positionService;
            _individualService = individualService;
            _departmentService = departmentService; // Инициализируем
            _employees = new ObservableCollection<EmployeeDto>();
            _filteredEmployees = new ObservableCollection<EmployeeDto>();
            _positions = new ObservableCollection<PositionDto>();
            _departmentFilters = new ObservableCollection<string>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка сотрудников...";

                // Загружаем должности для фильтров
                var positions = await _positionService.GetAllPositionsAsync(false);
                Positions.Clear();
                foreach (var position in positions.OrderBy(p => p.Name))
                {
                    Positions.Add(position);
                }

                // Загружаем сотрудников
                await LoadEmployeesAsync();

                StatusMessage = $"Загружено: {Employees.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadEmployeesAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync(IncludeDismissed);

            Employees.Clear();
            foreach (var employee in employees.OrderBy(e => e.IndividualShortName))
            {
                Employees.Add(employee);
            }

            // Обновляем список отделов для фильтра - используем DepartmentName вместо Department
            var departments = employees
                .Where(e => !string.IsNullOrWhiteSpace(e.DepartmentName))
                .Select(e => e.DepartmentName!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            DepartmentFilters.Clear();
            DepartmentFilters.Add("Все отделы");
            foreach (var dept in departments)
            {
                DepartmentFilters.Add(dept);
            }
            SelectedDepartmentFilter = "Все отделы";

            ApplyFilter();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnIncludeDismissedChanged(bool value)
        {
            _ = LoadEmployeesAsync();
        }

        partial void OnSelectedDepartmentFilterChanged(string? value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = Employees.AsEnumerable();

            // Фильтр по отделу - используем DepartmentName
            if (!string.IsNullOrWhiteSpace(SelectedDepartmentFilter) && SelectedDepartmentFilter != "Все отделы")
            {
                filtered = filtered.Where(e => e.DepartmentName == SelectedDepartmentFilter);
            }

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(e =>
                    (e.IndividualShortName != null && e.IndividualShortName.ToLower().Contains(searchLower)) ||
                    e.PersonnelNumber.Contains(SearchText) ||
                    (e.CurrentPositionName != null && e.CurrentPositionName.ToLower().Contains(searchLower)) ||
                    (e.DepartmentName != null && e.DepartmentName.ToLower().Contains(searchLower)));
            }

            FilteredEmployees.Clear();
            foreach (var item in filtered)
            {
                FilteredEmployees.Add(item);
            }
        }

        [RelayCommand]
        private async Task AddEmployeeAsync()
        {
            try
            {
                var window = new EmployeeEditWindow();
                var viewModel = new EmployeeEditViewModel(
                    _employeeService,
                    _positionService,
                    _individualService,
                    _departmentService, // Добавляем сервис отделов
                    null,
                    window); // Добавляем window

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadEmployeesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditEmployeeAsync()
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Выберите сотрудника для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var employeeToEdit = await _employeeService.GetEmployeeByIdAsync(SelectedEmployee.Id);
                if (employeeToEdit == null)
                {
                    MessageBox.Show("Сотрудник не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new EmployeeEditWindow();
                var viewModel = new EmployeeEditViewModel(
                    _employeeService,
                    _positionService,
                    _individualService,
                    _departmentService, // Добавляем сервис отделов
                    employeeToEdit,
                    window); // Добавляем window

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadEmployeesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DismissEmployeeAsync()
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Выберите сотрудника для увольнения", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedEmployee.Status == "Dismissed")
                {
                    MessageBox.Show("Сотрудник уже уволен", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var window = new EmployeeDismissWindow();
                var viewModel = new EmployeeDismissViewModel(_employeeService, SelectedEmployee.Id, window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadEmployeesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при увольнении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task TransferEmployeeAsync()
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Выберите сотрудника для перевода", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedEmployee.Status == "Dismissed")
                {
                    MessageBox.Show("Нельзя переводить уволенного сотрудника", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var window = new EmployeeTransferWindow();
                var viewModel = new EmployeeTransferViewModel(
                    _employeeService,
                    _positionService,
                    SelectedEmployee.Id,
                    window);

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadEmployeesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переводе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ViewHistoryAsync()
        {
            if (SelectedEmployee == null) return;

            var window = new EmploymentHistoryWindow();
            var viewModel = new EmploymentHistoryViewModel(_employeeService, SelectedEmployee.Id, window);

            window.DataContext = viewModel;
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadEmployeesAsync();
        }
    }
}