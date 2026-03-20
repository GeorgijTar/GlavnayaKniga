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
    public class AssetService : IAssetService
    {
        private readonly IRepository<Asset> _assetRepository;
        private readonly IRepository<AssetType> _assetTypeRepository;
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<Counterparty> _counterpartyRepository;

        public AssetService(
            IRepository<Asset> assetRepository,
            IRepository<AssetType> assetTypeRepository,
            IRepository<Account> accountRepository,
            IRepository<Counterparty> counterpartyRepository)
        {
            _assetRepository = assetRepository;
            _assetTypeRepository = assetTypeRepository;
            _accountRepository = accountRepository;
            _counterpartyRepository = counterpartyRepository;
        }

        public async Task<IEnumerable<AssetDto>> GetAllAssetsAsync(bool includeArchived = false)
        {
            var assets = await _assetRepository.FindAsync(a => includeArchived || !a.IsArchived);
            var result = new List<AssetDto>();

            foreach (var asset in assets.OrderBy(a => a.Name))
            {
                var dto = await MapToDto(asset);
                result.Add(dto);
            }

            return result;
        }

        public async Task<AssetDto?> GetAssetByIdAsync(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            return asset != null ? await MapToDto(asset) : null;
        }

        public async Task<AssetDto?> GetAssetByInventoryNumberAsync(string inventoryNumber)
        {
            if (string.IsNullOrWhiteSpace(inventoryNumber)) return null;

            var assets = await _assetRepository.FindAsync(a => a.InventoryNumber == inventoryNumber);
            var asset = assets.FirstOrDefault();

            return asset != null ? await MapToDto(asset) : null;
        }

        public async Task<IEnumerable<AssetDto>> GetAssetsByTypeAsync(int assetTypeId, bool includeArchived = false)
        {
            var assets = await _assetRepository.FindAsync(a =>
                a.AssetTypeId == assetTypeId && (includeArchived || !a.IsArchived));

            var result = new List<AssetDto>();
            foreach (var asset in assets.OrderBy(a => a.Name))
            {
                var dto = await MapToDto(asset);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<AssetDto>> SearchAssetsAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllAssetsAsync(includeArchived);

            var searchLower = searchText.ToLower();
            var assets = await _assetRepository.FindAsync(a =>
                (includeArchived || !a.IsArchived) &&
                (a.Name.ToLower().Contains(searchLower) ||
                 (a.RegistrationNumber != null && a.RegistrationNumber.ToLower().Contains(searchLower)) ||
                 (a.InventoryNumber != null && a.InventoryNumber.ToLower().Contains(searchLower)) ||
                 (a.Model != null && a.Model.ToLower().Contains(searchLower)) ||
                 (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchLower))));

            var result = new List<AssetDto>();
            foreach (var asset in assets.OrderBy(a => a.Name))
            {
                var dto = await MapToDto(asset);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<AssetGroupDto>> GetAssetsGroupedByTypeAsync(bool includeArchived = false)
        {
            var types = await _assetTypeRepository.FindAsync(t => includeArchived || !t.IsArchived);
            var result = new List<AssetGroupDto>();

            foreach (var type in types.OrderBy(t => t.Name))
            {
                var assets = await GetAssetsByTypeAsync(type.Id, includeArchived);

                result.Add(new AssetGroupDto
                {
                    AssetType = await MapToDto(type),
                    Assets = assets.ToList()
                });
            }

            return result;
        }

        public async Task<AssetDto> CreateAssetAsync(AssetDto assetDto)
        {
            // Проверяем уникальность инвентарного номера
            if (!string.IsNullOrWhiteSpace(assetDto.InventoryNumber))
            {
                if (!await IsInventoryNumberUniqueAsync(assetDto.InventoryNumber))
                {
                    throw new InvalidOperationException($"Объект с инвентарным номером '{assetDto.InventoryNumber}' уже существует");
                }
            }

            // Проверяем существование типа
            var assetType = await _assetTypeRepository.GetByIdAsync(assetDto.AssetTypeId);
            if (assetType == null)
            {
                throw new InvalidOperationException($"Тип объекта с ID {assetDto.AssetTypeId} не найден");
            }

            // Проверяем существование счета учета
            var account = await _accountRepository.GetByIdAsync(assetDto.AccountId);
            if (account == null)
            {
                throw new InvalidOperationException($"Счет учета с ID {assetDto.AccountId} не найден");
            }

            // Проверяем существование ответственного лица (если указано)
            if (assetDto.ResponsiblePersonId.HasValue)
            {
                var responsible = await _counterpartyRepository.GetByIdAsync(assetDto.ResponsiblePersonId.Value);
                if (responsible == null)
                {
                    throw new InvalidOperationException($"Ответственное лицо с ID {assetDto.ResponsiblePersonId} не найдено");
                }
            }

            var asset = new Asset
            {
                Name = assetDto.Name,
                RegistrationNumber = assetDto.RegistrationNumber,
                InventoryNumber = assetDto.InventoryNumber,
                AssetTypeId = assetDto.AssetTypeId,
                YearOfManufacture = assetDto.YearOfManufacture,
                Model = assetDto.Model,
                Manufacturer = assetDto.Manufacturer,
                SerialNumber = assetDto.SerialNumber,
                PurchaseDate = assetDto.PurchaseDate,
                CommissioningDate = assetDto.CommissioningDate,
                DecommissioningDate = assetDto.DecommissioningDate,
                InitialCost = assetDto.InitialCost,
                ResidualValue = assetDto.ResidualValue,
                Location = assetDto.Location,
                ResponsiblePersonId = assetDto.ResponsiblePersonId,
                AccountId = assetDto.AccountId,
                Note = assetDto.Note,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _assetRepository.AddAsync(asset);
            return await MapToDto(created);
        }

        public async Task<AssetDto> UpdateAssetAsync(AssetDto assetDto)
        {
            var asset = await _assetRepository.GetByIdAsync(assetDto.Id);
            if (asset == null)
            {
                throw new InvalidOperationException($"Объект с ID {assetDto.Id} не найден");
            }

            // Проверяем уникальность инвентарного номера (если он изменился)
            if (asset.InventoryNumber != assetDto.InventoryNumber && !string.IsNullOrWhiteSpace(assetDto.InventoryNumber))
            {
                if (!await IsInventoryNumberUniqueAsync(assetDto.InventoryNumber, assetDto.Id))
                {
                    throw new InvalidOperationException($"Объект с инвентарным номером '{assetDto.InventoryNumber}' уже существует");
                }
            }

            // Проверяем существование типа
            var assetType = await _assetTypeRepository.GetByIdAsync(assetDto.AssetTypeId);
            if (assetType == null)
            {
                throw new InvalidOperationException($"Тип объекта с ID {assetDto.AssetTypeId} не найден");
            }

            // Проверяем существование счета учета
            var account = await _accountRepository.GetByIdAsync(assetDto.AccountId);
            if (account == null)
            {
                throw new InvalidOperationException($"Счет учета с ID {assetDto.AccountId} не найден");
            }

            // Проверяем существование ответственного лица (если указано)
            if (assetDto.ResponsiblePersonId.HasValue)
            {
                var responsible = await _counterpartyRepository.GetByIdAsync(assetDto.ResponsiblePersonId.Value);
                if (responsible == null)
                {
                    throw new InvalidOperationException($"Ответственное лицо с ID {assetDto.ResponsiblePersonId} не найдено");
                }
            }

            asset.Name = assetDto.Name;
            asset.RegistrationNumber = assetDto.RegistrationNumber;
            asset.InventoryNumber = assetDto.InventoryNumber;
            asset.AssetTypeId = assetDto.AssetTypeId;
            asset.YearOfManufacture = assetDto.YearOfManufacture;
            asset.Model = assetDto.Model;
            asset.Manufacturer = assetDto.Manufacturer;
            asset.SerialNumber = assetDto.SerialNumber;
            asset.PurchaseDate = assetDto.PurchaseDate;
            asset.CommissioningDate = assetDto.CommissioningDate;
            asset.DecommissioningDate = assetDto.DecommissioningDate;
            asset.InitialCost = assetDto.InitialCost;
            asset.ResidualValue = assetDto.ResidualValue;
            asset.Location = assetDto.Location;
            asset.ResponsiblePersonId = assetDto.ResponsiblePersonId;
            asset.AccountId = assetDto.AccountId;
            asset.Note = assetDto.Note;
            asset.UpdatedAt = DateTime.UtcNow;

            await _assetRepository.UpdateAsync(asset);
            return await MapToDto(asset);
        }

        public async Task<bool> ArchiveAssetAsync(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null) return false;

            asset.IsArchived = true;
            asset.DecommissioningDate = DateTime.UtcNow;
            asset.ArchivedAt = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;

            await _assetRepository.UpdateAsync(asset);
            return true;
        }

        public async Task<bool> UnarchiveAssetAsync(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null || !asset.IsArchived) return false;

            asset.IsArchived = false;
            asset.DecommissioningDate = null;
            asset.ArchivedAt = null;
            asset.UpdatedAt = DateTime.UtcNow;

            await _assetRepository.UpdateAsync(asset);
            return true;
        }

        public async Task<bool> DeleteAssetAsync(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null) return false;

            // TODO: Проверить, есть ли движения по этому объекту

            await _assetRepository.DeleteAsync(asset);
            return true;
        }

        public async Task<bool> IsInventoryNumberUniqueAsync(string inventoryNumber, int? excludeId = null)
        {
            var assets = await _assetRepository.FindAsync(a => a.InventoryNumber == inventoryNumber);

            if (excludeId.HasValue)
            {
                return !assets.Any(a => a.Id != excludeId.Value);
            }

            return !assets.Any();
        }

        private async Task<AssetDto> MapToDto(Asset asset)
        {
            var assetType = await _assetTypeRepository.GetByIdAsync(asset.AssetTypeId);
            var account = await _accountRepository.GetByIdAsync(asset.AccountId);
            Counterparty? responsible = null;

            if (asset.ResponsiblePersonId.HasValue)
            {
                responsible = await _counterpartyRepository.GetByIdAsync(asset.ResponsiblePersonId.Value);
            }

            return new AssetDto
            {
                Id = asset.Id,
                Name = asset.Name,
                RegistrationNumber = asset.RegistrationNumber,
                InventoryNumber = asset.InventoryNumber,
                AssetTypeId = asset.AssetTypeId,
                AssetTypeName = assetType?.Name,
                YearOfManufacture = asset.YearOfManufacture,
                Model = asset.Model,
                Manufacturer = asset.Manufacturer,
                SerialNumber = asset.SerialNumber,
                PurchaseDate = asset.PurchaseDate,
                CommissioningDate = asset.CommissioningDate,
                DecommissioningDate = asset.DecommissioningDate,
                InitialCost = asset.InitialCost,
                ResidualValue = asset.ResidualValue,
                Location = asset.Location,
                ResponsiblePersonId = asset.ResponsiblePersonId,
                ResponsiblePersonName = responsible?.ShortName,
                AccountId = asset.AccountId,
                AccountCode = account?.Code,
                AccountName = account?.Name,
                Note = asset.Note,
                IsArchived = asset.IsArchived,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt
            };
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