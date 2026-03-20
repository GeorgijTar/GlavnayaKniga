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
    public class PositionService : IPositionService
    {
        private readonly IRepository<Position> _positionRepository;
        private readonly IRepository<Employee> _employeeRepository;

        public PositionService(
            IRepository<Position> positionRepository,
            IRepository<Employee> employeeRepository)
        {
            _positionRepository = positionRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<PositionDto>> GetAllPositionsAsync(bool includeArchived = false)
        {
            var positions = await _positionRepository.FindAsync(p => includeArchived || !p.IsArchived);

            return positions
                .OrderBy(p => p.Name)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<PositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            return position != null ? MapToDto(position) : null;
        }

        public async Task<PositionDto?> GetPositionByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var positions = await _positionRepository.FindAsync(p => p.Name == name);
            var position = positions.FirstOrDefault();

            return position != null ? MapToDto(position) : null;
        }

        public async Task<IEnumerable<PositionDto>> GetPositionsByCategoryAsync(string category, bool includeArchived = false)
        {
            var categoryEnum = ParsePositionCategory(category);
            var positions = await _positionRepository.FindAsync(p =>
                (includeArchived || !p.IsArchived) && p.Category == categoryEnum);

            return positions
                .OrderBy(p => p.Name)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IEnumerable<PositionDto>> SearchPositionsAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllPositionsAsync(includeArchived);

            var searchLower = searchText.ToLower();
            var positions = await _positionRepository.FindAsync(p =>
                (includeArchived || !p.IsArchived) &&
                (p.Name.ToLower().Contains(searchLower) ||
                 (p.ShortName != null && p.ShortName.ToLower().Contains(searchLower)) ||
                 (p.Description != null && p.Description.ToLower().Contains(searchLower))));

            return positions
                .OrderBy(p => p.Name)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<PositionDto> CreatePositionAsync(PositionDto positionDto)
        {
            // Проверка уникальности наименования
            if (!await IsNameUniqueAsync(positionDto.Name))
            {
                throw new InvalidOperationException($"Должность с наименованием '{positionDto.Name}' уже существует");
            }

            var position = new Position
            {
                Name = positionDto.Name,
                ShortName = positionDto.ShortName,
                Category = ParsePositionCategory(positionDto.Category),
                Description = positionDto.Description,
                EducationRequirements = positionDto.EducationRequirements,
                ExperienceYears = positionDto.ExperienceYears,
                BaseSalary = positionDto.BaseSalary,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _positionRepository.AddAsync(position);
            return MapToDto(created);
        }

        public async Task<PositionDto> UpdatePositionAsync(PositionDto positionDto)
        {
            var position = await _positionRepository.GetByIdAsync(positionDto.Id);
            if (position == null)
            {
                throw new InvalidOperationException($"Должность с ID {positionDto.Id} не найдена");
            }

            // Проверка уникальности наименования (если изменилось)
            if (position.Name != positionDto.Name && !await IsNameUniqueAsync(positionDto.Name, positionDto.Id))
            {
                throw new InvalidOperationException($"Должность с наименованием '{positionDto.Name}' уже существует");
            }

            position.Name = positionDto.Name;
            position.ShortName = positionDto.ShortName;
            position.Category = ParsePositionCategory(positionDto.Category);
            position.Description = positionDto.Description;
            position.EducationRequirements = positionDto.EducationRequirements;
            position.ExperienceYears = positionDto.ExperienceYears;
            position.BaseSalary = positionDto.BaseSalary;
            position.UpdatedAt = DateTime.UtcNow;

            await _positionRepository.UpdateAsync(position);
            return MapToDto(position);
        }

        public async Task<bool> ArchivePositionAsync(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null) return false;

            // Проверяем, есть ли активные сотрудники на этой должности
            var employees = await _employeeRepository.FindAsync(e =>
                e.CurrentPositionId == id && e.Status == EmployeeStatus.Active);

            if (employees.Any())
            {
                throw new InvalidOperationException("Нельзя архивировать должность, на которой есть активные сотрудники");
            }

            position.IsArchived = true;
            position.ArchivedAt = DateTime.UtcNow;
            position.UpdatedAt = DateTime.UtcNow;

            await _positionRepository.UpdateAsync(position);
            return true;
        }

        public async Task<bool> UnarchivePositionAsync(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null || !position.IsArchived) return false;

            position.IsArchived = false;
            position.ArchivedAt = null;
            position.UpdatedAt = DateTime.UtcNow;

            await _positionRepository.UpdateAsync(position);
            return true;
        }

        public async Task<bool> DeletePositionAsync(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null) return false;

            // Проверяем, есть ли связанные сотрудники
            var employees = await _employeeRepository.FindAsync(e => e.CurrentPositionId == id);
            if (employees.Any())
            {
                throw new InvalidOperationException("Нельзя удалить должность, которая используется");
            }

            await _positionRepository.DeleteAsync(position);
            return true;
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            var positions = await _positionRepository.FindAsync(p => p.Name == name);

            if (excludeId.HasValue)
            {
                return !positions.Any(p => p.Id != excludeId.Value);
            }

            return !positions.Any();
        }

        private PositionCategory ParsePositionCategory(string category)
        {
            return category switch
            {
                "Manager" => PositionCategory.Manager,
                "Specialist" => PositionCategory.Specialist,
                "Worker" => PositionCategory.Worker,
                _ => PositionCategory.Other
            };
        }

        private PositionDto MapToDto(Position position)
        {
            return new PositionDto
            {
                Id = position.Id,
                Name = position.Name,
                ShortName = position.ShortName,
                Category = position.Category.ToString(),
                Description = position.Description,
                EducationRequirements = position.EducationRequirements,
                ExperienceYears = position.ExperienceYears,
                BaseSalary = position.BaseSalary,
                IsArchived = position.IsArchived,
                CreatedAt = position.CreatedAt,
                UpdatedAt = position.UpdatedAt
            };
        }
    }
}