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
    public partial class DepartmentEditViewModel : BaseViewModel
    {
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;
        private readonly DepartmentDto? _originalDepartment;
        private readonly DepartmentDto? _parentDepartment;
        private readonly bool _isChildMode;
        private readonly Window _window;

        [ObservableProperty]
        private DepartmentDto _department;

        [ObservableProperty]
        private ObservableCollection<DepartmentDto> _parentDepartments;

        [ObservableProperty]
        private DepartmentDto? _selectedParentDepartment;

        [ObservableProperty]
        private ObservableCollection<EmployeeDto> _heads;

        [ObservableProperty]
        private EmployeeDto? _selectedHead;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public DepartmentEditViewModel(
            IDepartmentService departmentService,
            IEmployeeService employeeService,
            DepartmentDto? departmentToEdit,
            Window window,
            bool isChildMode = false)
        {
            _departmentService = departmentService;
            _employeeService = employeeService;
            _originalDepartment = departmentToEdit;
            _window = window;
            _isChildMode = isChildMode;

            _parentDepartments = new ObservableCollection<DepartmentDto>();
            _heads = new ObservableCollection<EmployeeDto>();

            if (_originalDepartment != null)
            {
                _department = new DepartmentDto
                {
                    Id = _originalDepartment.Id,
                    Code = _originalDepartment.Code,
                    Name = _originalDepartment.Name,
                    FullName = _originalDepartment.FullName,
                    ParentId = _originalDepartment.ParentId,
                    ParentName = _originalDepartment.ParentName,
                    ParentCode = _originalDepartment.ParentCode,
                    HeadEmployeeId = _originalDepartment.HeadEmployeeId,
                    HeadEmployeeName = _originalDepartment.HeadEmployeeName,
                    Phone = _originalDepartment.Phone,
                    Email = _originalDepartment.Email,
                    Location = _originalDepartment.Location,
                    Note = _originalDepartment.Note,
                    IsArchived = _originalDepartment.IsArchived,
                    EmployeeCount = _originalDepartment.EmployeeCount
                };
                Title = _isChildMode
                    ? $"Добавление дочернего отдела к '{_originalDepartment.Name}'"
                    : $"Редактирование отдела '{_originalDepartment.Name}'";
                IsEditMode = true;
            }
            else
            {
                _department = new DepartmentDto();
                Title = _isChildMode ? "Добавление дочернего отдела" : "Добавление отдела";
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

                // Загружаем возможные родительские отделы
                var allDepartments = await _departmentService.GetAllDepartmentsAsync(true);

                ParentDepartments.Clear();

                // Создаем элемент для корневого уровня
                var rootItem = new DepartmentDto
                {
                    Id = 0,
                    Code = "",
                    Name = "Корневой уровень"
                };
                ParentDepartments.Add(rootItem);

                // Исключаем текущий отдел и его потомков из списка родительских
                var excludedIds = new HashSet<int>();
                if (_originalDepartment != null)
                {
                    excludedIds.Add(_originalDepartment.Id);
                    await AddChildIdsAsync(_originalDepartment.Id, excludedIds);
                }

                foreach (var dept in allDepartments.Where(d => !excludedIds.Contains(d.Id)).OrderBy(d => d.Code))
                {
                    ParentDepartments.Add(dept);
                }

                // Загружаем возможных руководителей (активные сотрудники)
                var employees = await _employeeService.GetAllEmployeesAsync(false);
                Heads.Clear();

                // Создаем элемент для "Не выбран"
                var noneHead = new EmployeeDto { Id = 0, IndividualShortName = "— Не выбран —" };
                Heads.Add(noneHead);

                foreach (var emp in employees.OrderBy(e => e.IndividualShortName))
                {
                    Heads.Add(emp);
                }

                // Устанавливаем выбранные значения
                if (_originalDepartment != null)
                {
                    if (_originalDepartment.ParentId.HasValue)
                    {
                        SelectedParentDepartment = ParentDepartments.FirstOrDefault(p => p.Id == _originalDepartment.ParentId.Value);
                    }
                    else
                    {
                        SelectedParentDepartment = ParentDepartments.FirstOrDefault(p => p.Id == 0);
                    }

                    if (_originalDepartment.HeadEmployeeId.HasValue)
                    {
                        SelectedHead = Heads.FirstOrDefault(h => h.Id == _originalDepartment.HeadEmployeeId.Value);
                    }
                    else
                    {
                        SelectedHead = Heads.FirstOrDefault(h => h.Id == 0);
                    }
                }
                else if (_parentDepartment != null)
                {
                    // Если это дочерний отдел, устанавливаем родителя
                    SelectedParentDepartment = ParentDepartments.FirstOrDefault(p => p.Id == _parentDepartment.Id);
                }
                else
                {
                    SelectedParentDepartment = ParentDepartments.FirstOrDefault(p => p.Id == 0);
                    SelectedHead = Heads.FirstOrDefault(h => h.Id == 0);
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
            var children = await _departmentService.GetAllDepartmentsAsync(true);
            var childDepartments = children.Where(c => c.ParentId == parentId);

            foreach (var child in childDepartments)
            {
                ids.Add(child.Id);
                await AddChildIdsAsync(child.Id, ids);
            }
        }

        partial void OnSelectedParentDepartmentChanged(DepartmentDto? value)
        {
            if (value != null)
            {
                if (value.Id > 0)
                {
                    Department.ParentId = value.Id;
                    Department.ParentName = value.Name;
                    Department.ParentCode = value.Code;
                }
                else
                {
                    Department.ParentId = null;
                    Department.ParentName = null;
                    Department.ParentCode = null;
                }
            }
        }

        partial void OnSelectedHeadChanged(EmployeeDto? value)
        {
            if (value != null)
            {
                if (value.Id > 0)
                {
                    Department.HeadEmployeeId = value.Id;
                    Department.HeadEmployeeName = value.IndividualShortName;
                }
                else
                {
                    Department.HeadEmployeeId = null;
                    Department.HeadEmployeeName = null;
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
                if (string.IsNullOrWhiteSpace(Department.Code))
                {
                    MessageBox.Show(_window, "Введите код отдела", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Department.Name))
                {
                    MessageBox.Show(_window, "Введите наименование отдела", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка уникальности кода
                if (!await _departmentService.IsCodeUniqueAsync(Department.Code, Department.Id > 0 ? Department.Id : null))
                {
                    MessageBox.Show(_window, $"Код отдела '{Department.Code}' уже используется", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка циклической ссылки
                if (Department.ParentId.HasValue && Department.ParentId.Value == Department.Id)
                {
                    MessageBox.Show(_window, "Отдел не может быть родителем для самого себя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Department.Id > 0)
                {
                    // Редактирование
                    await _departmentService.UpdateDepartmentAsync(Department);
                    MessageBox.Show(_window, "Отдел успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _departmentService.CreateDepartmentAsync(Department);
                    MessageBox.Show(_window, "Отдел успешно добавлен", "Успех",
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