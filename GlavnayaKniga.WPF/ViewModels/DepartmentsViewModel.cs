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
    public partial class DepartmentsViewModel : BaseViewModel
    {
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;

        [ObservableProperty]
        private ObservableCollection<DepartmentDto> _departments;

        [ObservableProperty]
        private ObservableCollection<DepartmentDto> _filteredDepartments;

        [ObservableProperty]
        private DepartmentDto? _selectedDepartment;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        [ObservableProperty]
        private bool _showTreeView = true;

        public DepartmentsViewModel(
            IDepartmentService departmentService,
            IEmployeeService employeeService)
        {
            _departmentService = departmentService;
            _employeeService = employeeService;
            _departments = new ObservableCollection<DepartmentDto>();
            _filteredDepartments = new ObservableCollection<DepartmentDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка отделов...";

                var departments = await _departmentService.GetDepartmentsHierarchyAsync(ShowArchived);

                Departments.Clear();
                foreach (var department in departments)
                {
                    Departments.Add(department);
                }

                ApplyFilter();

                StatusMessage = $"Загружено отделов: {CountDepartments(Departments)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private int CountDepartments(ObservableCollection<DepartmentDto> departments)
        {
            int count = departments.Count;
            foreach (var department in departments)
            {
                count += CountDepartments(department.Children);
            }
            return count;
        }

        private int CountDepartments(List<DepartmentDto> departments)
        {
            int count = departments.Count;
            foreach (var department in departments)
            {
                count += CountDepartments(department.Children);
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
                    FilteredDepartments.Clear();
                    foreach (var item in Departments)
                    {
                        FilteredDepartments.Add(item);
                    }
                }
                else
                {
                    // В табличном режиме нужно собрать все элементы
                    var allItems = FlattenDepartmentsToObservable(Departments);
                    FilteredDepartments.Clear();
                    foreach (var item in allItems.OrderBy(d => d.Code))
                    {
                        FilteredDepartments.Add(item);
                    }
                }
                return;
            }

            var searchLower = SearchText.ToLower();

            if (ShowTreeView)
            {
                // Для древовидного режима фильтруем с сохранением иерархии
                var filtered = FilterDepartmentHierarchy(Departments, searchLower);
                FilteredDepartments.Clear();
                foreach (var item in filtered)
                {
                    FilteredDepartments.Add(item);
                }
            }
            else
            {
                // Для табличного режима просто фильтруем плоский список
                var allItems = FlattenDepartmentsToObservable(Departments);
                var filtered = allItems.Where(d =>
                    d.Code.ToLower().Contains(searchLower) ||
                    d.Name.ToLower().Contains(searchLower) ||
                    (d.FullName != null && d.FullName.ToLower().Contains(searchLower)));

                FilteredDepartments.Clear();
                foreach (var item in filtered.OrderBy(d => d.Code))
                {
                    FilteredDepartments.Add(item);
                }
            }
        }

        // Исправлено: возвращаем ObservableCollection
        private ObservableCollection<DepartmentDto> FlattenDepartmentsToObservable(ObservableCollection<DepartmentDto> departments)
        {
            var result = new ObservableCollection<DepartmentDto>();
            foreach (var department in departments)
            {
                result.Add(department);
                foreach (var child in FlattenDepartmentsToObservable(department.Children))
                {
                    result.Add(child);
                }
            }
            return result;
        }

        // Перегрузка для List
        private ObservableCollection<DepartmentDto> FlattenDepartmentsToObservable(List<DepartmentDto> departments)
        {
            var result = new ObservableCollection<DepartmentDto>();
            foreach (var department in departments)
            {
                result.Add(department);
                foreach (var child in FlattenDepartmentsToObservable(department.Children))
                {
                    result.Add(child);
                }
            }
            return result;
        }

        private ObservableCollection<DepartmentDto> FilterDepartmentHierarchy(
            ObservableCollection<DepartmentDto> departments,
            string searchLower)
        {
            var result = new ObservableCollection<DepartmentDto>();

            foreach (var department in departments)
            {
                bool departmentMatches = department.Code.ToLower().Contains(searchLower) ||
                                      department.Name.ToLower().Contains(searchLower) ||
                                      (department.FullName != null && department.FullName.ToLower().Contains(searchLower));

                var filteredChildren = FilterDepartmentHierarchy(department.Children, searchLower);

                if (departmentMatches || filteredChildren.Any())
                {
                    var filteredDepartment = new DepartmentDto
                    {
                        Id = department.Id,
                        Code = department.Code,
                        Name = department.Name,
                        FullName = department.FullName,
                        ParentId = department.ParentId,
                        ParentName = department.ParentName,
                        ParentCode = department.ParentCode,
                        HeadEmployeeId = department.HeadEmployeeId,
                        HeadEmployeeName = department.HeadEmployeeName,
                        Phone = department.Phone,
                        Email = department.Email,
                        Location = department.Location,
                        Note = department.Note,
                        IsArchived = department.IsArchived,
                        EmployeeCount = department.EmployeeCount,
                        Children = filteredChildren.ToList()
                    };
                    result.Add(filteredDepartment);
                }
            }

            return result;
        }

        private ObservableCollection<DepartmentDto> FilterDepartmentHierarchy(
            List<DepartmentDto> departments,
            string searchLower)
        {
            var result = new ObservableCollection<DepartmentDto>();

            foreach (var department in departments)
            {
                bool departmentMatches = department.Code.ToLower().Contains(searchLower) ||
                                      department.Name.ToLower().Contains(searchLower) ||
                                      (department.FullName != null && department.FullName.ToLower().Contains(searchLower));

                var filteredChildren = FilterDepartmentHierarchy(department.Children, searchLower);

                if (departmentMatches || filteredChildren.Any())
                {
                    var filteredDepartment = new DepartmentDto
                    {
                        Id = department.Id,
                        Code = department.Code,
                        Name = department.Name,
                        FullName = department.FullName,
                        ParentId = department.ParentId,
                        ParentName = department.ParentName,
                        ParentCode = department.ParentCode,
                        HeadEmployeeId = department.HeadEmployeeId,
                        HeadEmployeeName = department.HeadEmployeeName,
                        Phone = department.Phone,
                        Email = department.Email,
                        Location = department.Location,
                        Note = department.Note,
                        IsArchived = department.IsArchived,
                        EmployeeCount = department.EmployeeCount,
                        Children = filteredChildren.ToList()
                    };
                    result.Add(filteredDepartment);
                }
            }

            return result;
        }

        partial void OnShowTreeViewChanged(bool value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        private async Task AddDepartmentAsync()
        {
            try
            {
                var window = new DepartmentEditWindow();
                var viewModel = new DepartmentEditViewModel(
                    _departmentService,
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
        private async Task EditDepartmentAsync()
        {
            try
            {
                if (SelectedDepartment == null)
                {
                    MessageBox.Show("Выберите отдел для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var departmentToEdit = await _departmentService.GetDepartmentByIdAsync(SelectedDepartment.Id);
                if (departmentToEdit == null)
                {
                    MessageBox.Show("Отдел не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new DepartmentEditWindow();
                var viewModel = new DepartmentEditViewModel(
                    _departmentService,
                    _employeeService,
                    departmentToEdit,
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
        private async Task AddChildDepartmentAsync()
        {
            try
            {
                if (SelectedDepartment == null)
                {
                    MessageBox.Show("Выберите родительский отдел", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var window = new DepartmentEditWindow();
                var viewModel = new DepartmentEditViewModel(
                    _departmentService,
                    _employeeService,
                    SelectedDepartment,
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
        private async Task ArchiveDepartmentAsync()
        {
            try
            {
                if (SelectedDepartment == null)
                {
                    MessageBox.Show("Выберите отдел для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedDepartment.IsArchived)
                {
                    MessageBox.Show("Отдел уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать отдел '{SelectedDepartment.DisplayName}'?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация отдела...";

                    var success = await _departmentService.ArchiveDepartmentAsync(SelectedDepartment.Id);

                    if (success)
                    {
                        StatusMessage = "Отдел успешно архивирован";
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
        private async Task UnarchiveDepartmentAsync()
        {
            try
            {
                if (SelectedDepartment == null)
                {
                    MessageBox.Show("Выберите отдел для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedDepartment.IsArchived)
                {
                    MessageBox.Show("Отдел не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать отдел '{SelectedDepartment.DisplayName}'?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация отдела...";

                    var success = await _departmentService.UnarchiveDepartmentAsync(SelectedDepartment.Id);

                    if (success)
                    {
                        StatusMessage = "Отдел успешно разархивирован";
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