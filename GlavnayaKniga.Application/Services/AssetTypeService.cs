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
    public class AssetTypeService : IAssetTypeService
    {
        private readonly IRepository<AssetType> _assetTypeRepository;
        private readonly IRepository<Asset> _assetRepository;

        public AssetTypeService(
            IRepository<AssetType> assetTypeRepository,
            IRepository<Asset> assetRepository)
        {
            _assetTypeRepository = assetTypeRepository;
            _assetRepository = assetRepository;
        }

        public async Task<IEnumerable<AssetTypeDto>> GetAllAssetTypesAsync(bool includeArchived = false)
        {
            var types = await _assetTypeRepository.FindAsync(t => includeArchived || !t.IsArchived);
            var result = new List<AssetTypeDto>();

            foreach (var type in types.OrderBy(t => t.Name))
            {
                var dto = await MapToDto(type);
                result.Add(dto);
            }

            return result;
        }

        public async Task<AssetTypeDto?> GetAssetTypeByIdAsync(int id)
        {
            var type = await _assetTypeRepository.GetByIdAsync(id);
            return type != null ? await MapToDto(type) : null;
        }

        public async Task<AssetTypeDto?> GetAssetTypeByNameAsync(string name)
        {
            var types = await _assetTypeRepository.FindAsync(t => t.Name == name);
            var type = types.FirstOrDefault();
            return type != null ? await MapToDto(type) : null;
        }

        public async Task<AssetTypeDto> CreateAssetTypeAsync(AssetTypeDto assetTypeDto)
        {
            // Проверяем уникальность наименования
            if (!await IsNameUniqueAsync(assetTypeDto.Name))
            {
                throw new InvalidOperationException($"Тип с наименованием '{assetTypeDto.Name}' уже существует");
            }

            var type = new AssetType
            {
                Name = assetTypeDto.Name,
                Description = assetTypeDto.Description,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _assetTypeRepository.AddAsync(type);
            return await MapToDto(created);
        }

        public async Task<AssetTypeDto> UpdateAssetTypeAsync(AssetTypeDto assetTypeDto)
        {
            var type = await _assetTypeRepository.GetByIdAsync(assetTypeDto.Id);
            if (type == null)
            {
                throw new InvalidOperationException($"Тип с ID {assetTypeDto.Id} не найден");
            }

            // Проверяем уникальность наименования (если оно изменилось)
            if (type.Name != assetTypeDto.Name && !await IsNameUniqueAsync(assetTypeDto.Name, assetTypeDto.Id))
            {
                throw new InvalidOperationException($"Тип с наименованием '{assetTypeDto.Name}' уже существует");
            }

            type.Name = assetTypeDto.Name;
            type.Description = assetTypeDto.Description;
            type.UpdatedAt = DateTime.UtcNow;

            await _assetTypeRepository.UpdateAsync(type);
            return await MapToDto(type);
        }

        public async Task<bool> ArchiveAssetTypeAsync(int id)
        {
            var type = await _assetTypeRepository.GetByIdAsync(id);
            if (type == null) return false;

            // Проверяем, есть ли объекты этого типа
            var assets = await _assetRepository.FindAsync(a => a.AssetTypeId == id && !a.IsArchived);
            if (assets.Any())
            {
                throw new InvalidOperationException("Нельзя архивировать тип, у которого есть активные объекты");
            }

            type.IsArchived = true;
            type.ArchivedAt = DateTime.UtcNow;
            type.UpdatedAt = DateTime.UtcNow;

            await _assetTypeRepository.UpdateAsync(type);
            return true;
        }

        public async Task<bool> UnarchiveAssetTypeAsync(int id)
        {
            var type = await _assetTypeRepository.GetByIdAsync(id);
            if (type == null || !type.IsArchived) return false;

            type.IsArchived = false;
            type.ArchivedAt = null;
            type.UpdatedAt = DateTime.UtcNow;

            await _assetTypeRepository.UpdateAsync(type);
            return true;
        }

        public async Task<bool> DeleteAssetTypeAsync(int id)
        {
            var type = await _assetTypeRepository.GetByIdAsync(id);
            if (type == null) return false;

            // Проверяем, есть ли объекты этого типа
            var assets = await _assetRepository.FindAsync(a => a.AssetTypeId == id);
            if (assets.Any())
            {
                throw new InvalidOperationException("Нельзя удалить тип, к которому привязаны объекты");
            }

            await _assetTypeRepository.DeleteAsync(type);
            return true;
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            var types = await _assetTypeRepository.FindAsync(t => t.Name == name);

            if (excludeId.HasValue)
            {
                return !types.Any(t => t.Id != excludeId.Value);
            }

            return !types.Any();
        }

        private async Task<AssetTypeDto> MapToDto(AssetType type)
        {
            var assets = await _assetRepository.FindAsync(a => a.AssetTypeId == type.Id);

            return new AssetTypeDto
            {
                Id = type.Id,
                Name = type.Name,
                Description = type.Description,
                IsArchived = type.IsArchived,
                CreatedAt = type.CreatedAt,
                UpdatedAt = type.UpdatedAt,
                AssetsCount = assets.Count()
            };
        }
    }
}