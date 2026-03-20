using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Helpers;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class StorageLocationService : IStorageLocationService
    {
        private readonly IRepository<StorageLocation> _locationRepository;
        private readonly IRepository<Employee> _employeeRepository;

        public StorageLocationService(
            IRepository<StorageLocation> locationRepository,
            IRepository<Employee> employeeRepository)
        {
            _locationRepository = locationRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<StorageLocationDto>> GetAllLocationsAsync(bool includeArchived = false)
        {
            var locations = await _locationRepository.FindAsync(l => includeArchived || !l.IsArchived);
            var result = new List<StorageLocationDto>();

            foreach (var location in locations.OrderBy(l => l.Code))
            {
                var dto = await MapToDto(location);
                result.Add(dto);
            }

            return result;
        }

        public async Task<StorageLocationDto?> GetLocationByIdAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            return location != null ? await MapToDto(location) : null;
        }

        public async Task<StorageLocationDto?> GetLocationByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var locations = await _locationRepository.FindAsync(l => l.Code == code);
            var location = locations.FirstOrDefault();

            return location != null ? await MapToDto(location) : null;
        }

        public async Task<IEnumerable<StorageLocationDto>> GetLocationsByTypeAsync(string type, bool includeArchived = false)
        {
            var typeEnum = ParseStorageLocationType(type);
            var locations = await _locationRepository.FindAsync(l =>
                (includeArchived || !l.IsArchived) && l.Type == typeEnum);

            var result = new List<StorageLocationDto>();
            foreach (var location in locations.OrderBy(l => l.Code))
            {
                var dto = await MapToDto(location);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<StorageLocationDto>> GetLocationsHierarchyAsync(bool includeArchived = false)
        {
            var allLocations = await _locationRepository.FindAsync(l => includeArchived || !l.IsArchived);
            var locationDict = new Dictionary<int, StorageLocationDto>();

            // Сначала создаем все DTO
            foreach (var location in allLocations)
            {
                var dto = await MapToDto(location);
                locationDict[location.Id] = dto;
            }

            // Строим иерархию
            foreach (var location in allLocations)
            {
                if (location.ParentId.HasValue && locationDict.ContainsKey(location.ParentId.Value))
                {
                    locationDict[location.ParentId.Value].Children.Add(locationDict[location.Id]);
                }
            }

            // Возвращаем только корневые элементы
            return allLocations
                .Where(l => !l.ParentId.HasValue)
                .Select(l => locationDict[l.Id])
                .OrderBy(l => l.Code);
        }

        public async Task<IEnumerable<StorageLocationDto>> SearchLocationsAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllLocationsAsync(includeArchived);

            var searchLower = searchText.ToLower();
            var locations = await _locationRepository.FindAsync(l =>
                (includeArchived || !l.IsArchived) &&
                (l.Code.ToLower().Contains(searchLower) ||
                 l.Name.ToLower().Contains(searchLower) ||
                 (l.Description != null && l.Description.ToLower().Contains(searchLower)) ||
                 (l.Address != null && l.Address.ToLower().Contains(searchLower))));

            var result = new List<StorageLocationDto>();
            foreach (var location in locations.OrderBy(l => l.Code))
            {
                var dto = await MapToDto(location);
                result.Add(dto);
            }

            return result;
        }

        public async Task<StorageLocationDto> CreateLocationAsync(StorageLocationDto locationDto)
        {
            // Проверяем уникальность кода
            if (!await IsCodeUniqueAsync(locationDto.Code))
            {
                throw new InvalidOperationException($"Место хранения с кодом '{locationDto.Code}' уже существует");
            }

            // Проверяем существование родителя (если указан)
            if (locationDto.ParentId.HasValue)
            {
                var parent = await _locationRepository.GetByIdAsync(locationDto.ParentId.Value);
                if (parent == null)
                {
                    throw new InvalidOperationException($"Родительское место хранения с ID {locationDto.ParentId} не найдено");
                }
            }

            // Проверяем существование ответственного сотрудника (если указан)
            if (locationDto.ResponsibleEmployeeId.HasValue)
            {
                var responsible = await _employeeRepository.GetByIdAsync(locationDto.ResponsibleEmployeeId.Value);
                if (responsible == null)
                {
                    throw new InvalidOperationException($"Сотрудник с ID {locationDto.ResponsibleEmployeeId} не найден");
                }
            }

            var location = new StorageLocation
            {
                Code = locationDto.Code,
                Name = locationDto.Name,
                Type = ParseStorageLocationType(locationDto.Type),
                ParentId = locationDto.ParentId,
                Description = locationDto.Description,
                Address = locationDto.Address,
                ResponsibleEmployeeId = locationDto.ResponsibleEmployeeId,
                Area = locationDto.Area,
                Capacity = locationDto.Capacity,
                TemperatureRegime = locationDto.TemperatureRegime,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _locationRepository.AddAsync(location);
            return await MapToDto(created);
        }

        public async Task<StorageLocationDto> UpdateLocationAsync(StorageLocationDto locationDto)
        {
            var location = await _locationRepository.GetByIdAsync(locationDto.Id);
            if (location == null)
            {
                throw new InvalidOperationException($"Место хранения с ID {locationDto.Id} не найдено");
            }

            // Проверяем уникальность кода (если он изменился)
            if (location.Code != locationDto.Code && !await IsCodeUniqueAsync(locationDto.Code, locationDto.Id))
            {
                throw new InvalidOperationException($"Место хранения с кодом '{locationDto.Code}' уже существует");
            }

            // Проверяем существование родителя (если указан)
            if (locationDto.ParentId.HasValue)
            {
                // Проверяем, не пытаемся ли мы назначить родителем самого себя
                if (locationDto.ParentId.Value == locationDto.Id)
                {
                    throw new InvalidOperationException("Место хранения не может быть родителем для самого себя");
                }

                // Проверяем, не создается ли цикл в иерархии
                if (await WouldCreateCycleAsync(locationDto.Id, locationDto.ParentId.Value))
                {
                    throw new InvalidOperationException("Невозможно назначить родителя: это создаст цикл в иерархии");
                }

                var parent = await _locationRepository.GetByIdAsync(locationDto.ParentId.Value);
                if (parent == null)
                {
                    throw new InvalidOperationException($"Родительское место хранения с ID {locationDto.ParentId} не найдено");
                }
            }

            // Проверяем существование ответственного сотрудника (если указан)
            if (locationDto.ResponsibleEmployeeId.HasValue)
            {
                var responsible = await _employeeRepository.GetByIdAsync(locationDto.ResponsibleEmployeeId.Value);
                if (responsible == null)
                {
                    throw new InvalidOperationException($"Сотрудник с ID {locationDto.ResponsibleEmployeeId} не найден");
                }
            }

            location.Code = locationDto.Code;
            location.Name = locationDto.Name;
            location.Type = ParseStorageLocationType(locationDto.Type);
            location.ParentId = locationDto.ParentId;
            location.Description = locationDto.Description;
            location.Address = locationDto.Address;
            location.ResponsibleEmployeeId = locationDto.ResponsibleEmployeeId;
            location.Area = locationDto.Area;
            location.Capacity = locationDto.Capacity;
            location.TemperatureRegime = locationDto.TemperatureRegime;
            location.UpdatedAt = DateTime.UtcNow;

            await _locationRepository.UpdateAsync(location);
            return await MapToDto(location);
        }

        public async Task<bool> ArchiveLocationAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null) return false;

            // Проверяем, есть ли дочерние места хранения
            if (await HasChildrenAsync(id))
            {
                throw new InvalidOperationException("Нельзя архивировать место хранения, у которого есть дочерние элементы");
            }

            location.IsArchived = true;
            location.ArchivedAt = DateTime.UtcNow;
            location.UpdatedAt = DateTime.UtcNow;

            await _locationRepository.UpdateAsync(location);
            return true;
        }

        public async Task<bool> UnarchiveLocationAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null || !location.IsArchived) return false;

            location.IsArchived = false;
            location.ArchivedAt = null;
            location.UpdatedAt = DateTime.UtcNow;

            await _locationRepository.UpdateAsync(location);
            return true;
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null) return false;

            // Проверяем, есть ли дочерние места хранения
            if (await HasChildrenAsync(id))
            {
                throw new InvalidOperationException("Нельзя удалить место хранения, у которого есть дочерние элементы");
            }

            // Проверяем, есть ли связанные номенклатуры
            // TODO: Проверить наличие связанных номенклатур

            await _locationRepository.DeleteAsync(location);
            return true;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            var locations = await _locationRepository.FindAsync(l => l.Code == code);

            if (excludeId.HasValue)
            {
                return !locations.Any(l => l.Id != excludeId.Value);
            }

            return !locations.Any();
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            var children = await _locationRepository.FindAsync(l => l.ParentId == id);
            return children.Any();
        }

        private async Task<bool> WouldCreateCycleAsync(int locationId, int parentId)
        {
            // Проверяем, не приведет ли назначение parentId родителем для locationId к циклу
            var current = await _locationRepository.GetByIdAsync(parentId);
            while (current != null)
            {
                if (current.Id == locationId)
                    return true;

                if (!current.ParentId.HasValue)
                    break;

                current = await _locationRepository.GetByIdAsync(current.ParentId.Value);
            }

            return false;
        }

        private async Task<StorageLocationDto> MapToDto(StorageLocation location)
        {
            Employee? responsible = null;
            if (location.ResponsibleEmployeeId.HasValue)
            {
                responsible = await _employeeRepository.GetByIdAsync(location.ResponsibleEmployeeId.Value);
            }

            StorageLocation? parent = null;
            if (location.ParentId.HasValue)
            {
                parent = await _locationRepository.GetByIdAsync(location.ParentId.Value);
            }

            return new StorageLocationDto
            {
                Id = location.Id,
                Code = location.Code,
                Name = location.Name,
                Type = location.Type.ToString(),
                TypeDisplay = GetRussianType(location.Type),
                ParentId = location.ParentId,
                ParentName = parent?.Name,
                ParentCode = parent?.Code,
                Description = location.Description,
                Address = location.Address,
                ResponsibleEmployeeId = location.ResponsibleEmployeeId,
                ResponsibleEmployeeName = responsible?.Individual?.ShortName,
                Area = location.Area,
                Capacity = location.Capacity,
                TemperatureRegime = location.TemperatureRegime,
                IsArchived = location.IsArchived,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt
            };
        }

        private StorageLocationType ParseStorageLocationType(string type)
        {
            return type switch
            {
                "Warehouse" => StorageLocationType.Warehouse,
                "Section" => StorageLocationType.Section,
                "Cell" => StorageLocationType.Cell,
                "Rack" => StorageLocationType.Rack,
                "Outdoor" => StorageLocationType.Outdoor,
                "Vehicle" => StorageLocationType.Vehicle,
                _ => StorageLocationType.Other
            };
        }

        private string GetRussianType(StorageLocationType type)
        {
            return type switch
            {
                StorageLocationType.Warehouse => "Склад",
                StorageLocationType.Section => "Участок/Отделение",
                StorageLocationType.Cell => "Ячейка",
                StorageLocationType.Rack => "Стеллаж",
                StorageLocationType.Outdoor => "Открытая площадка",
                StorageLocationType.Vehicle => "Транспортное средство",
                StorageLocationType.Other => "Прочее",
                _ => "Неизвестно"
            };
        }
    }
}