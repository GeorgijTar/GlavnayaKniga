using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Individual> _individualRepository;
        private readonly IRepository<Position> _positionRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<EmploymentHistory> _historyRepository;

        public EmployeeService(
            IRepository<Employee> employeeRepository,
            IRepository<Individual> individualRepository,
            IRepository<Position> positionRepository,
            IRepository<Department> departmentRepository,
            IRepository<EmploymentHistory> historyRepository)
        {
            _employeeRepository = employeeRepository;
            _individualRepository = individualRepository;
            _positionRepository = positionRepository;
            _departmentRepository = departmentRepository;
            _historyRepository = historyRepository;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(bool includeDismissed = false)
        {
            var employees = await _employeeRepository.FindAsync(e =>
                includeDismissed || e.Status != EmployeeStatus.Dismissed);

            var result = new List<EmployeeDto>();
            foreach (var employee in employees.OrderBy(e => e.Individual?.LastName))
            {
                var dto = await MapToDto(employee);
                result.Add(dto);
            }

            return result;
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            return employee != null ? await MapToDto(employee) : null;
        }

        public async Task<EmployeeDto?> GetEmployeeByPersonnelNumberAsync(string personnelNumber)
        {
            if (string.IsNullOrWhiteSpace(personnelNumber)) return null;

            var employees = await _employeeRepository.FindAsync(e => e.PersonnelNumber == personnelNumber);
            var employee = employees.FirstOrDefault();

            return employee != null ? await MapToDto(employee) : null;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByStatusAsync(string status)
        {
            var statusEnum = ParseEmployeeStatus(status);
            var employees = await _employeeRepository.FindAsync(e => e.Status == statusEnum);

            var result = new List<EmployeeDto>();
            foreach (var employee in employees.OrderBy(e => e.Individual?.LastName))
            {
                var dto = await MapToDto(employee);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string departmentName)
        {
            var employees = await _employeeRepository.FindAsync(e =>
                e.Department != null && e.Department.Name == departmentName);

            var result = new List<EmployeeDto>();
            foreach (var employee in employees.OrderBy(e => e.Individual?.LastName))
            {
                var dto = await MapToDto(employee);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            var employees = await _employeeRepository.FindAsync(e => e.DepartmentId == departmentId);

            var result = new List<EmployeeDto>();
            foreach (var employee in employees.OrderBy(e => e.Individual?.LastName))
            {
                var dto = await MapToDto(employee);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<EmployeeDto>> SearchEmployeesAsync(string searchText, bool includeDismissed = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllEmployeesAsync(includeDismissed);

            var searchLower = searchText.ToLower();
            var employees = await _employeeRepository.FindAsync(e =>
                includeDismissed || e.Status != EmployeeStatus.Dismissed);

            var filtered = employees.Where(e =>
                (e.Individual != null && (
                    e.Individual.LastName.ToLower().Contains(searchLower) ||
                    e.Individual.FirstName.ToLower().Contains(searchLower) ||
                    (e.Individual.MiddleName != null && e.Individual.MiddleName.ToLower().Contains(searchLower)))) ||
                e.PersonnelNumber.Contains(searchText) ||
                (e.Department != null && e.Department.Name.ToLower().Contains(searchLower)) ||
                (e.CurrentPosition != null && e.CurrentPosition.Name.ToLower().Contains(searchLower)));

            var result = new List<EmployeeDto>();
            foreach (var employee in filtered.OrderBy(e => e.Individual?.LastName))
            {
                var dto = await MapToDto(employee);
                result.Add(dto);
            }

            return result;
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto)
        {
            // Проверка существования физического лица
            var individual = await _individualRepository.GetByIdAsync(employeeDto.IndividualId);
            if (individual == null)
            {
                throw new InvalidOperationException($"Физическое лицо с ID {employeeDto.IndividualId} не найдено");
            }

            // Проверка существования должности
            var position = await _positionRepository.GetByIdAsync(employeeDto.CurrentPositionId);
            if (position == null)
            {
                throw new InvalidOperationException($"Должность с ID {employeeDto.CurrentPositionId} не найдена");
            }

            // Проверка существования отдела (если указан)
            if (employeeDto.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(employeeDto.DepartmentId.Value);
                if (department == null)
                {
                    throw new InvalidOperationException($"Отдел с ID {employeeDto.DepartmentId} не найден");
                }
            }

            // Проверка существования руководителя (если указан)
            if (employeeDto.ManagerId.HasValue)
            {
                var manager = await _employeeRepository.GetByIdAsync(employeeDto.ManagerId.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException($"Руководитель с ID {employeeDto.ManagerId} не найден");
                }
            }

            // Генерация табельного номера, если не указан
            if (string.IsNullOrWhiteSpace(employeeDto.PersonnelNumber))
            {
                employeeDto.PersonnelNumber = await GeneratePersonnelNumberAsync();
            }

            // Проверка уникальности табельного номера
            var existing = await GetEmployeeByPersonnelNumberAsync(employeeDto.PersonnelNumber);
            if (existing != null)
            {
                throw new InvalidOperationException($"Сотрудник с табельным номером {employeeDto.PersonnelNumber} уже существует");
            }

            var employee = new Employee
            {
                IndividualId = employeeDto.IndividualId,
                PersonnelNumber = employeeDto.PersonnelNumber,
                CurrentPositionId = employeeDto.CurrentPositionId,
                DepartmentId = employeeDto.DepartmentId,
                Status = ParseEmployeeStatus(employeeDto.Status),
                HireDate = employeeDto.HireDate,
                HireOrderNumber = employeeDto.HireOrderNumber,
                HireOrderDate = employeeDto.HireOrderDate,
                WorkPhone = employeeDto.WorkPhone,
                WorkEmail = employeeDto.WorkEmail,
                ManagerId = employeeDto.ManagerId,
                Note = employeeDto.Note,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _employeeRepository.AddAsync(employee);

            // Создаем запись в истории о приеме на работу
            var history = new EmploymentHistory
            {
                EmployeeId = created.Id,
                PositionId = employeeDto.CurrentPositionId,
                StartDate = employeeDto.HireDate,
                OrderNumber = employeeDto.HireOrderNumber,
                OrderDate = employeeDto.HireOrderDate,
                ChangeType = "Hire",
                CreatedAt = DateTime.UtcNow
            };
            await _historyRepository.AddAsync(history);

            return await MapToDto(created);
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employeeDto)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeDto.Id);
            if (employee == null)
            {
                throw new InvalidOperationException($"Сотрудник с ID {employeeDto.Id} не найден");
            }

            // Проверка существования должности
            var position = await _positionRepository.GetByIdAsync(employeeDto.CurrentPositionId);
            if (position == null)
            {
                throw new InvalidOperationException($"Должность с ID {employeeDto.CurrentPositionId} не найдена");
            }

            // Проверка существования отдела (если указан)
            if (employeeDto.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(employeeDto.DepartmentId.Value);
                if (department == null)
                {
                    throw new InvalidOperationException($"Отдел с ID {employeeDto.DepartmentId} не найден");
                }
            }

            // Проверка существования руководителя (если указан)
            if (employeeDto.ManagerId.HasValue)
            {
                var manager = await _employeeRepository.GetByIdAsync(employeeDto.ManagerId.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException($"Руководитель с ID {employeeDto.ManagerId} не найден");
                }
            }

            // Проверяем, изменилась ли должность
            bool positionChanged = employee.CurrentPositionId != employeeDto.CurrentPositionId;

            // Проверяем, изменилась ли дата приема (важно для истории)
            bool hireDateChanged = employee.HireDate != employeeDto.HireDate;

            // Обновляем основные поля
            employee.CurrentPositionId = employeeDto.CurrentPositionId;
            employee.DepartmentId = employeeDto.DepartmentId;
            employee.WorkPhone = employeeDto.WorkPhone;
            employee.WorkEmail = employeeDto.WorkEmail;
            employee.ManagerId = employeeDto.ManagerId;
            employee.Note = employeeDto.Note;
            employee.HireDate = employeeDto.HireDate;
            employee.HireOrderNumber = employeeDto.HireOrderNumber;
            employee.HireOrderDate = employeeDto.HireOrderDate;
            employee.UpdatedAt = DateTime.UtcNow;

            await _employeeRepository.UpdateAsync(employee);

            // Если изменилась должность, создаем запись в истории
            if (positionChanged)
            {
                // Находим последнюю запись в истории (текущую должность)
                var currentHistory = (await _historyRepository.FindAsync(h =>
                    h.EmployeeId == employee.Id && !h.EndDate.HasValue))
                    .FirstOrDefault();

                if (currentHistory != null)
                {
                    // Закрываем текущую запись
                    currentHistory.EndDate = DateTime.UtcNow;
                    await _historyRepository.UpdateAsync(currentHistory);
                }

                // Создаем новую запись в истории
                var newHistory = new EmploymentHistory
                {
                    EmployeeId = employee.Id,
                    PositionId = employeeDto.CurrentPositionId,
                    StartDate = DateTime.UtcNow,
                    OrderNumber = employeeDto.HireOrderNumber ?? "АВТО",
                    OrderDate = employeeDto.HireOrderDate ?? DateTime.UtcNow,
                    ChangeType = "Transfer",
                    Note = "Автоматическая запись при редактировании сотрудника",
                    CreatedAt = DateTime.UtcNow
                };
                await _historyRepository.AddAsync(newHistory);
            }
            // Если изменилась только дата приема, обновляем запись о приеме
            else if (hireDateChanged)
            {
                var hireHistory = (await _historyRepository.FindAsync(h =>
                    h.EmployeeId == employee.Id && h.ChangeType == "Hire"))
                    .FirstOrDefault();

                if (hireHistory != null)
                {
                    hireHistory.StartDate = employeeDto.HireDate;
                    hireHistory.OrderNumber = employeeDto.HireOrderNumber ?? hireHistory.OrderNumber;
                    hireHistory.OrderDate = employeeDto.HireOrderDate ?? hireHistory.OrderDate;
                    await _historyRepository.UpdateAsync(hireHistory);
                }
            }

            return await MapToDto(employee);
        }



        public async Task<EmployeeDto> DismissEmployeeAsync(int id, DateTime dismissalDate, string orderNumber, string reason)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new InvalidOperationException($"Сотрудник с ID {id} не найден");
            }

            if (employee.Status == EmployeeStatus.Dismissed)
            {
                throw new InvalidOperationException("Сотрудник уже уволен");
            }

            var oldStatus = employee.Status;
            employee.Status = EmployeeStatus.Dismissed;
            employee.DismissalDate = dismissalDate;
            employee.DismissalOrderNumber = orderNumber;
            employee.DismissalReason = reason;
            employee.UpdatedAt = DateTime.UtcNow;

            await _employeeRepository.UpdateAsync(employee);

            // Создаем запись в истории об увольнении
            var history = new EmploymentHistory
            {
                EmployeeId = employee.Id,
                PositionId = employee.CurrentPositionId,
                StartDate = employee.HireDate,
                EndDate = dismissalDate,
                OrderNumber = orderNumber,
                OrderDate = dismissalDate,
                ChangeType = "Dismissal",
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };
            await _historyRepository.AddAsync(history);

            return await MapToDto(employee);
        }

        public async Task<EmployeeDto> TransferEmployeeAsync(int id, int newPositionId, DateTime transferDate, string orderNumber)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new InvalidOperationException($"Сотрудник с ID {id} не найден");
            }

            var newPosition = await _positionRepository.GetByIdAsync(newPositionId);
            if (newPosition == null)
            {
                throw new InvalidOperationException($"Должность с ID {newPositionId} не найдена");
            }

            if (employee.Status == EmployeeStatus.Dismissed)
            {
                throw new InvalidOperationException("Нельзя переводить уволенного сотрудника");
            }

            // Находим текущую активную запись в истории
            var currentHistory = (await _historyRepository.FindAsync(h =>
                h.EmployeeId == employee.Id && !h.EndDate.HasValue))
                .FirstOrDefault();

            if (currentHistory != null)
            {
                // Закрываем текущую запись
                currentHistory.EndDate = transferDate;
                await _historyRepository.UpdateAsync(currentHistory);
            }

            // Создаем новую запись в истории
            var newHistory = new EmploymentHistory
            {
                EmployeeId = employee.Id,
                PositionId = newPositionId,
                StartDate = transferDate,
                OrderNumber = orderNumber,
                OrderDate = transferDate,
                ChangeType = "Transfer",
                Note = $"Перевод с должности ID {employee.CurrentPositionId}",
                CreatedAt = DateTime.UtcNow
            };
            await _historyRepository.AddAsync(newHistory);

            // Обновляем текущую должность сотрудника
            employee.CurrentPositionId = newPositionId;
            employee.UpdatedAt = DateTime.UtcNow;

            await _employeeRepository.UpdateAsync(employee);

            return await MapToDto(employee);
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;

            // Удаляем историю (каскадно благодаря настройкам в конфигурации)
            await _employeeRepository.DeleteAsync(employee);
            return true;
        }

        public async Task<string> GeneratePersonnelNumberAsync()
        {
            var currentYear = DateTime.Now.Year.ToString().Substring(2);
            var employees = await _employeeRepository.FindAsync(e =>
                e.HireDate.Year == DateTime.Now.Year);

            int nextNumber = employees.Count() + 1;
            return $"Е{currentYear}-{nextNumber:D4}";
        }

        public async Task<IEnumerable<EmploymentHistoryDto>> GetEmploymentHistoryAsync(int employeeId)
        {
            var history = await _historyRepository.FindAsync(h => h.EmployeeId == employeeId);

            var result = new List<EmploymentHistoryDto>();
            foreach (var item in history.OrderByDescending(h => h.StartDate))
            {
                var position = await _positionRepository.GetByIdAsync(item.PositionId);
                result.Add(new EmploymentHistoryDto
                {
                    Id = item.Id,
                    EmployeeId = item.EmployeeId,
                    PositionId = item.PositionId,
                    PositionName = position?.Name,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    OrderNumber = item.OrderNumber,
                    OrderDate = item.OrderDate,
                    ChangeType = item.ChangeType,
                    Reason = item.Reason,
                    Note = item.Note,
                    CreatedAt = item.CreatedAt
                });
            }

            return result;
        }

        private async Task<EmployeeDto> MapToDto(Employee employee)
        {
            var individual = await _individualRepository.GetByIdAsync(employee.IndividualId);
            var position = await _positionRepository.GetByIdAsync(employee.CurrentPositionId);

            Department? department = null;
            if (employee.DepartmentId.HasValue)
            {
                department = await _departmentRepository.GetByIdAsync(employee.DepartmentId.Value);
            }

            var manager = employee.ManagerId.HasValue
                ? await _employeeRepository.GetByIdAsync(employee.ManagerId.Value)
                : null;
            var managerIndividual = manager != null
                ? await _individualRepository.GetByIdAsync(manager.IndividualId)
                : null;

            var history = await GetEmploymentHistoryAsync(employee.Id);

            return new EmployeeDto
            {
                Id = employee.Id,
                IndividualId = employee.IndividualId,
                IndividualFullName = individual?.FullName,
                IndividualShortName = individual?.ShortName,
                IndividualPhone = individual?.Phone,
                IndividualEmail = individual?.Email,
                PersonnelNumber = employee.PersonnelNumber,
                CurrentPositionId = employee.CurrentPositionId,
                CurrentPositionName = position?.Name,
                DepartmentId = employee.DepartmentId,
                DepartmentName = department?.Name,
                DepartmentCode = department?.Code,
                Status = employee.Status.ToString(),
                HireDate = employee.HireDate,
                HireOrderNumber = employee.HireOrderNumber,
                HireOrderDate = employee.HireOrderDate,
                DismissalDate = employee.DismissalDate,
                DismissalOrderNumber = employee.DismissalOrderNumber,
                DismissalOrderDate = employee.DismissalOrderDate,
                DismissalReason = employee.DismissalReason,
                WorkPhone = employee.WorkPhone,
                WorkEmail = employee.WorkEmail,
                ManagerId = employee.ManagerId,
                ManagerName = managerIndividual?.ShortName,
                Note = employee.Note,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt,
                EmploymentHistory = history.ToList()
            };
        }

        private EmployeeStatus ParseEmployeeStatus(string status)
        {
            return status switch
            {
                "Active" => EmployeeStatus.Active,
                "OnLeave" => EmployeeStatus.OnLeave,
                "Dismissed" => EmployeeStatus.Dismissed,
                "Probation" => EmployeeStatus.Probation,
                _ => EmployeeStatus.Active
            };
        }
    }
}