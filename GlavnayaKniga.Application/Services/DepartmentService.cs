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
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Employee> _employeeRepository;

        public DepartmentService(
            IRepository<Department> departmentRepository,
            IRepository<Employee> employeeRepository)
        {
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(bool includeArchived = false)
        {
            var departments = await _departmentRepository.FindAsync(d => includeArchived || !d.IsArchived);
            var result = new List<DepartmentDto>();

            foreach (var department in departments.OrderBy(d => d.Code))
            {
                var dto = await MapToDto(department);
                result.Add(dto);
            }

            return result;
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            return department != null ? await MapToDto(department) : null;
        }

        public async Task<DepartmentDto?> GetDepartmentByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var departments = await _departmentRepository.FindAsync(d => d.Code == code);
            var department = departments.FirstOrDefault();

            return department != null ? await MapToDto(department) : null;
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsHierarchyAsync(bool includeArchived = false)
        {
            var allDepartments = await _departmentRepository.FindAsync(d => includeArchived || !d.IsArchived);
            var departmentDict = new Dictionary<int, DepartmentDto>();

            // Сначала создаем все DTO
            foreach (var department in allDepartments)
            {
                var dto = await MapToDto(department);
                departmentDict[department.Id] = dto;
            }

            // Строим иерархию
            foreach (var department in allDepartments)
            {
                if (department.ParentId.HasValue && departmentDict.ContainsKey(department.ParentId.Value))
                {
                    departmentDict[department.ParentId.Value].Children.Add(departmentDict[department.Id]);
                }
            }

            // Возвращаем только корневые элементы
            return allDepartments
                .Where(d => !d.ParentId.HasValue)
                .Select(d => departmentDict[d.Id])
                .OrderBy(d => d.Code);
        }

        public async Task<IEnumerable<DepartmentDto>> SearchDepartmentsAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllDepartmentsAsync(includeArchived);

            var searchLower = searchText.ToLower();
            var departments = await _departmentRepository.FindAsync(d =>
                (includeArchived || !d.IsArchived) &&
                (d.Code.ToLower().Contains(searchLower) ||
                 d.Name.ToLower().Contains(searchLower) ||
                 (d.FullName != null && d.FullName.ToLower().Contains(searchLower))));

            var result = new List<DepartmentDto>();
            foreach (var department in departments.OrderBy(d => d.Code))
            {
                var dto = await MapToDto(department);
                result.Add(dto);
            }

            return result;
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto departmentDto)
        {
            // Проверяем уникальность кода
            if (!await IsCodeUniqueAsync(departmentDto.Code))
            {
                throw new InvalidOperationException($"Отдел с кодом '{departmentDto.Code}' уже существует");
            }

            // Проверяем существование родителя (если указан)
            if (departmentDto.ParentId.HasValue)
            {
                var parent = await _departmentRepository.GetByIdAsync(departmentDto.ParentId.Value);
                if (parent == null)
                {
                    throw new InvalidOperationException($"Родительский отдел с ID {departmentDto.ParentId} не найден");
                }
            }

            // Проверяем существование руководителя (если указан)
            if (departmentDto.HeadEmployeeId.HasValue)
            {
                var head = await _employeeRepository.GetByIdAsync(departmentDto.HeadEmployeeId.Value);
                if (head == null)
                {
                    throw new InvalidOperationException($"Сотрудник с ID {departmentDto.HeadEmployeeId} не найден");
                }
            }

            var department = new Department
            {
                Code = departmentDto.Code,
                Name = departmentDto.Name,
                FullName = departmentDto.FullName,
                ParentId = departmentDto.ParentId,
                HeadEmployeeId = departmentDto.HeadEmployeeId,
                Phone = departmentDto.Phone,
                Email = departmentDto.Email,
                Location = departmentDto.Location,
                Note = departmentDto.Note,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _departmentRepository.AddAsync(department);
            return await MapToDto(created);
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(DepartmentDto departmentDto)
        {
            var department = await _departmentRepository.GetByIdAsync(departmentDto.Id);
            if (department == null)
            {
                throw new InvalidOperationException($"Отдел с ID {departmentDto.Id} не найден");
            }

            // Проверяем уникальность кода (если изменился)
            if (department.Code != departmentDto.Code && !await IsCodeUniqueAsync(departmentDto.Code, departmentDto.Id))
            {
                throw new InvalidOperationException($"Отдел с кодом '{departmentDto.Code}' уже существует");
            }

            // Проверяем существование родителя (если указан)
            if (departmentDto.ParentId.HasValue)
            {
                // Проверяем, не пытаемся ли мы назначить родителем самого себя
                if (departmentDto.ParentId.Value == departmentDto.Id)
                {
                    throw new InvalidOperationException("Отдел не может быть родителем для самого себя");
                }

                // Проверяем, не создается ли цикл в иерархии
                if (await WouldCreateCycleAsync(departmentDto.Id, departmentDto.ParentId.Value))
                {
                    throw new InvalidOperationException("Невозможно назначить родителя: это создаст цикл в иерархии");
                }

                var parent = await _departmentRepository.GetByIdAsync(departmentDto.ParentId.Value);
                if (parent == null)
                {
                    throw new InvalidOperationException($"Родительский отдел с ID {departmentDto.ParentId} не найден");
                }
            }

            // Проверяем существование руководителя (если указан)
            if (departmentDto.HeadEmployeeId.HasValue)
            {
                var head = await _employeeRepository.GetByIdAsync(departmentDto.HeadEmployeeId.Value);
                if (head == null)
                {
                    throw new InvalidOperationException($"Сотрудник с ID {departmentDto.HeadEmployeeId} не найден");
                }
            }

            department.Code = departmentDto.Code;
            department.Name = departmentDto.Name;
            department.FullName = departmentDto.FullName;
            department.ParentId = departmentDto.ParentId;
            department.HeadEmployeeId = departmentDto.HeadEmployeeId;
            department.Phone = departmentDto.Phone;
            department.Email = departmentDto.Email;
            department.Location = departmentDto.Location;
            department.Note = departmentDto.Note;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(department);
            return await MapToDto(department);
        }

        public async Task<bool> ArchiveDepartmentAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return false;

            // Проверяем, есть ли дочерние отделы
            if (await HasChildrenAsync(id))
            {
                throw new InvalidOperationException("Нельзя архивировать отдел, у которого есть дочерние отделы");
            }

            // Проверяем, есть ли сотрудники в отделе
            var employees = await _employeeRepository.FindAsync(e => e.DepartmentId == id);
            if (employees.Any())
            {
                throw new InvalidOperationException("Нельзя архивировать отдел, в котором есть сотрудники");
            }

            department.IsArchived = true;
            department.ArchivedAt = DateTime.UtcNow;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(department);
            return true;
        }

        public async Task<bool> UnarchiveDepartmentAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null || !department.IsArchived) return false;

            department.IsArchived = false;
            department.ArchivedAt = null;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(department);
            return true;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return false;

            // Проверяем, есть ли дочерние отделы
            if (await HasChildrenAsync(id))
            {
                throw new InvalidOperationException("Нельзя удалить отдел, у которого есть дочерние отделы");
            }

            // Проверяем, есть ли сотрудники в отделе
            var employees = await _employeeRepository.FindAsync(e => e.DepartmentId == id);
            if (employees.Any())
            {
                throw new InvalidOperationException("Нельзя удалить отдел, в котором есть сотрудники");
            }

            await _departmentRepository.DeleteAsync(department);
            return true;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            var departments = await _departmentRepository.FindAsync(d => d.Code == code);

            if (excludeId.HasValue)
            {
                return !departments.Any(d => d.Id != excludeId.Value);
            }

            return !departments.Any();
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            var children = await _departmentRepository.FindAsync(d => d.ParentId == id);
            return children.Any();
        }

        private async Task<bool> WouldCreateCycleAsync(int departmentId, int parentId)
        {
            // Проверяем, не приведет ли назначение parentId родителем для departmentId к циклу
            var current = await _departmentRepository.GetByIdAsync(parentId);
            while (current != null)
            {
                if (current.Id == departmentId)
                    return true;

                if (!current.ParentId.HasValue)
                    break;

                current = await _departmentRepository.GetByIdAsync(current.ParentId.Value);
            }

            return false;
        }

        private async Task<DepartmentDto> MapToDto(Department department)
        {
            Employee? headEmployee = null;
            if (department.HeadEmployeeId.HasValue)
            {
                headEmployee = await _employeeRepository.GetByIdAsync(department.HeadEmployeeId.Value);
            }

            Department? parent = null;
            if (department.ParentId.HasValue)
            {
                parent = await _departmentRepository.GetByIdAsync(department.ParentId.Value);
            }

            // Подсчитываем количество сотрудников в отделе по DepartmentId
            var employees = await _employeeRepository.FindAsync(e => e.DepartmentId == department.Id);

            return new DepartmentDto
            {
                Id = department.Id,
                Code = department.Code,
                Name = department.Name,
                FullName = department.FullName,
                ParentId = department.ParentId,
                ParentName = parent?.Name,
                ParentCode = parent?.Code,
                HeadEmployeeId = department.HeadEmployeeId,
                HeadEmployeeName = headEmployee?.Individual?.ShortName,
                Phone = department.Phone,
                Email = department.Email,
                Location = department.Location,
                Note = department.Note,
                IsArchived = department.IsArchived,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                EmployeeCount = employees.Count()
            };
        }
    }
}