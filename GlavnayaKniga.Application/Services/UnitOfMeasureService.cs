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
    public class UnitOfMeasureService : IUnitOfMeasureService
    {
        private readonly IRepository<UnitOfMeasure> _unitRepository;
        private readonly IRepository<Nomenclature> _nomenclatureRepository;

        public UnitOfMeasureService(
            IRepository<UnitOfMeasure> unitRepository,
            IRepository<Nomenclature> nomenclatureRepository)
        {
            _unitRepository = unitRepository;
            _nomenclatureRepository = nomenclatureRepository;
        }

        public async Task<IEnumerable<UnitOfMeasureDto>> GetAllUnitsAsync(bool includeArchived = false)
        {
            var units = await _unitRepository.FindAsync(u => includeArchived || !u.IsArchived);

            return units
                .OrderBy(u => u.Code)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<UnitOfMeasureDto?> GetUnitByIdAsync(int id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            return unit != null ? MapToDto(unit) : null;
        }

        public async Task<UnitOfMeasureDto?> GetUnitByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var units = await _unitRepository.FindAsync(u => u.Code == code);
            var unit = units.FirstOrDefault();

            return unit != null ? MapToDto(unit) : null;
        }

        public async Task<IEnumerable<UnitOfMeasureDto>> SearchUnitsAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllUnitsAsync(includeArchived);

            var searchLower = searchText.ToLower();
            var units = await _unitRepository.FindAsync(u =>
                (includeArchived || !u.IsArchived) &&
                (u.Code.ToLower().Contains(searchLower) ||
                 u.ShortName.ToLower().Contains(searchLower) ||
                 u.FullName.ToLower().Contains(searchLower) ||
                 (u.InternationalCode != null && u.InternationalCode.ToLower().Contains(searchLower))));

            return units
                .OrderBy(u => u.Code)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<UnitOfMeasureDto> CreateUnitAsync(UnitOfMeasureDto unitDto)
        {
            // Проверка уникальности кода
            if (!await IsCodeUniqueAsync(unitDto.Code))
            {
                throw new InvalidOperationException($"Единица измерения с кодом '{unitDto.Code}' уже существует");
            }

            var unit = new UnitOfMeasure
            {
                Code = unitDto.Code,
                ShortName = unitDto.ShortName,
                FullName = unitDto.FullName,
                InternationalCode = unitDto.InternationalCode,
                Description = unitDto.Description,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _unitRepository.AddAsync(unit);
            return MapToDto(created);
        }

        public async Task<UnitOfMeasureDto> UpdateUnitAsync(UnitOfMeasureDto unitDto)
        {
            var unit = await _unitRepository.GetByIdAsync(unitDto.Id);
            if (unit == null)
            {
                throw new InvalidOperationException($"Единица измерения с ID {unitDto.Id} не найдена");
            }

            // Проверка уникальности кода (если изменился)
            if (unit.Code != unitDto.Code && !await IsCodeUniqueAsync(unitDto.Code, unitDto.Id))
            {
                throw new InvalidOperationException($"Единица измерения с кодом '{unitDto.Code}' уже существует");
            }

            unit.Code = unitDto.Code;
            unit.ShortName = unitDto.ShortName;
            unit.FullName = unitDto.FullName;
            unit.InternationalCode = unitDto.InternationalCode;
            unit.Description = unitDto.Description;
            unit.UpdatedAt = DateTime.UtcNow;

            await _unitRepository.UpdateAsync(unit);
            return MapToDto(unit);
        }

        public async Task<bool> ArchiveUnitAsync(int id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null) return false;

            // Проверяем, используется ли единица измерения
            var nomenclatures = await _nomenclatureRepository.FindAsync(n => n.UnitId == id);
            if (nomenclatures.Any())
            {
                throw new InvalidOperationException("Нельзя архивировать единицу измерения, которая используется в номенклатуре");
            }

            unit.IsArchived = true;
            unit.ArchivedAt = DateTime.UtcNow;
            unit.UpdatedAt = DateTime.UtcNow;

            await _unitRepository.UpdateAsync(unit);
            return true;
        }

        public async Task<bool> UnarchiveUnitAsync(int id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null || !unit.IsArchived) return false;

            unit.IsArchived = false;
            unit.ArchivedAt = null;
            unit.UpdatedAt = DateTime.UtcNow;

            await _unitRepository.UpdateAsync(unit);
            return true;
        }

        public async Task<bool> DeleteUnitAsync(int id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null) return false;

            // Проверяем, используется ли единица измерения
            var nomenclatures = await _nomenclatureRepository.FindAsync(n => n.UnitId == id);
            if (nomenclatures.Any())
            {
                throw new InvalidOperationException("Нельзя удалить единицу измерения, которая используется в номенклатуре");
            }

            await _unitRepository.DeleteAsync(unit);
            return true;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            var units = await _unitRepository.FindAsync(u => u.Code == code);

            if (excludeId.HasValue)
            {
                return !units.Any(u => u.Id != excludeId.Value);
            }

            return !units.Any();
        }

        private UnitOfMeasureDto MapToDto(UnitOfMeasure unit)
        {
            return new UnitOfMeasureDto
            {
                Id = unit.Id,
                Code = unit.Code,
                ShortName = unit.ShortName,
                FullName = unit.FullName,
                InternationalCode = unit.InternationalCode,
                Description = unit.Description,
                IsArchived = unit.IsArchived,
                CreatedAt = unit.CreatedAt,
                UpdatedAt = unit.UpdatedAt
            };
        }
    }
}