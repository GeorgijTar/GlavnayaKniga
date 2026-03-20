using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class NomenclatureService : INomenclatureService
    {
        private readonly IRepository<Nomenclature> _nomenclatureRepository;
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<StorageLocation> _storageLocationRepository;
        private readonly IRepository<UnitOfMeasure> _unitRepository;

        public NomenclatureService(
            IRepository<Nomenclature> nomenclatureRepository,
            IRepository<Account> accountRepository,
            IRepository<StorageLocation> storageLocationRepository,
            IRepository<UnitOfMeasure> unitRepository)
        {
            _nomenclatureRepository = nomenclatureRepository;
            _accountRepository = accountRepository;
            _storageLocationRepository = storageLocationRepository;
            _unitRepository = unitRepository;
        }

        public async Task<IEnumerable<NomenclatureDto>> GetAllNomenclatureAsync(bool includeArchived = false)
        {
            var nomenclatures = await _nomenclatureRepository.FindAsync(n => includeArchived || !n.IsArchived);
            var result = new List<NomenclatureDto>();

            foreach (var item in nomenclatures.OrderBy(n => n.Name))
            {
                var dto = await MapToDto(item);
                result.Add(dto);
            }

            return result;
        }

        public async Task<NomenclatureDto?> GetNomenclatureByIdAsync(int id)
        {
            var nomenclature = await _nomenclatureRepository.GetByIdAsync(id);
            return nomenclature != null ? await MapToDto(nomenclature) : null;
        }

        public async Task<NomenclatureDto?> GetNomenclatureByArticleAsync(string article)
        {
            if (string.IsNullOrWhiteSpace(article)) return null;

            var items = await _nomenclatureRepository.FindAsync(n => n.Article == article);
            var item = items.FirstOrDefault();

            return item != null ? await MapToDto(item) : null;
        }

        public async Task<IEnumerable<NomenclatureDto>> SearchNomenclatureAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllNomenclatureAsync(includeArchived);

            var searchLower = searchText.ToLower();
            var items = await _nomenclatureRepository.FindAsync(n =>
                (includeArchived || !n.IsArchived) &&
                (n.Name.ToLower().Contains(searchLower) ||
                 (n.FullName != null && n.FullName.ToLower().Contains(searchLower)) ||
                 (n.Article != null && n.Article.ToLower().Contains(searchLower)) ||
                 (n.Barcode != null && n.Barcode.Contains(searchText))));

            var result = new List<NomenclatureDto>();
            foreach (var item in items.OrderBy(n => n.Name))
            {
                var dto = await MapToDto(item);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<NomenclatureDto>> GetNomenclatureByTypeAsync(string type, bool includeArchived = false)
        {
            var typeEnum = ParseNomenclatureType(type);
            var items = await _nomenclatureRepository.FindAsync(n =>
                (includeArchived || !n.IsArchived) &&
                n.Type == typeEnum);

            var result = new List<NomenclatureDto>();
            foreach (var item in items.OrderBy(n => n.Name))
            {
                var dto = await MapToDto(item);
                result.Add(dto);
            }

            return result;
        }

        public async Task<NomenclatureDto> CreateNomenclatureAsync(NomenclatureDto nomenclatureDto)
        {
            // Проверяем уникальность артикула
            if (!string.IsNullOrWhiteSpace(nomenclatureDto.Article))
            {
                if (!await IsArticleUniqueAsync(nomenclatureDto.Article))
                {
                    throw new InvalidOperationException($"Номенклатура с артикулом {nomenclatureDto.Article} уже существует");
                }
            }

            // Проверяем существование счета учета
            var account = await _accountRepository.GetByIdAsync(nomenclatureDto.AccountId);
            if (account == null)
            {
                throw new InvalidOperationException($"Счет учета с ID {nomenclatureDto.AccountId} не найден");
            }

            // Проверяем существование счета учета НДС
            var vatAccount = await _accountRepository.GetByIdAsync(nomenclatureDto.DefaultVatAccountId);
            if (vatAccount == null)
            {
                throw new InvalidOperationException($"Счет учета НДС с ID {nomenclatureDto.DefaultVatAccountId} не найден");
            }

            // Проверяем существование места хранения (если указано)
            StorageLocation? storageLocation = null;
            if (nomenclatureDto.StorageLocationId.HasValue)
            {
                storageLocation = await _storageLocationRepository.GetByIdAsync(nomenclatureDto.StorageLocationId.Value);
                if (storageLocation == null)
                {
                    throw new InvalidOperationException($"Место хранения с ID {nomenclatureDto.StorageLocationId} не найдено");
                }
            }
            // Проверяем существование единицы измерения
            var unit = await _unitRepository.GetByIdAsync(nomenclatureDto.UnitId);
            if (unit == null)
            {
                throw new InvalidOperationException($"Единица измерения с ID {nomenclatureDto.UnitId} не найдена");
            }


            var nomenclature = new Nomenclature
            {
                Name = nomenclatureDto.Name,
                FullName = nomenclatureDto.FullName,
                Article = nomenclatureDto.Article,
                Barcode = nomenclatureDto.Barcode,
                Type = ParseNomenclatureType(nomenclatureDto.Type),
                UnitId = nomenclatureDto.UnitId,
                AccountId = nomenclatureDto.AccountId,
                DefaultVatAccountId = nomenclatureDto.DefaultVatAccountId,
                StorageLocationId = nomenclatureDto.StorageLocationId,
                PurchasePrice = nomenclatureDto.PurchasePrice,
                SalePrice = nomenclatureDto.SalePrice,
                CurrentStock = nomenclatureDto.CurrentStock ?? 0,
                MinStock = nomenclatureDto.MinStock,
                MaxStock = nomenclatureDto.MaxStock,
                Description = nomenclatureDto.Description,
                Note = nomenclatureDto.Note,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _nomenclatureRepository.AddAsync(nomenclature);
            return await MapToDto(created);
        }

        public async Task<NomenclatureDto> UpdateNomenclatureAsync(NomenclatureDto nomenclatureDto)
        {
            var nomenclature = await _nomenclatureRepository.GetByIdAsync(nomenclatureDto.Id);
            if (nomenclature == null)
            {
                throw new InvalidOperationException($"Номенклатура с ID {nomenclatureDto.Id} не найдена");
            }

            // Проверяем уникальность артикула (если он изменился)
            if (nomenclature.Article != nomenclatureDto.Article && !string.IsNullOrWhiteSpace(nomenclatureDto.Article))
            {
                if (!await IsArticleUniqueAsync(nomenclatureDto.Article, nomenclatureDto.Id))
                {
                    throw new InvalidOperationException($"Номенклатура с артикулом {nomenclatureDto.Article} уже существует");
                }
            }

            // Проверяем существование счета учета
            var account = await _accountRepository.GetByIdAsync(nomenclatureDto.AccountId);
            if (account == null)
            {
                throw new InvalidOperationException($"Счет учета с ID {nomenclatureDto.AccountId} не найден");
            }

            // Проверяем существование счета учета НДС
            var vatAccount = await _accountRepository.GetByIdAsync(nomenclatureDto.DefaultVatAccountId);
            if (vatAccount == null)
            {
                throw new InvalidOperationException($"Счет учета НДС с ID {nomenclatureDto.DefaultVatAccountId} не найден");
            }

            // Проверяем существование места хранения (если указано)
            if (nomenclatureDto.StorageLocationId.HasValue)
            {
                var storageLocation = await _storageLocationRepository.GetByIdAsync(nomenclatureDto.StorageLocationId.Value);
                if (storageLocation == null)
                {
                    throw new InvalidOperationException($"Место хранения с ID {nomenclatureDto.StorageLocationId} не найдено");
                }
            }

            var unit = await _unitRepository.GetByIdAsync(nomenclatureDto.UnitId);
            if (unit == null)
            {
                throw new InvalidOperationException($"Единица измерения с ID {nomenclatureDto.UnitId} не найдена");
            }

            nomenclature.Name = nomenclatureDto.Name;
            nomenclature.FullName = nomenclatureDto.FullName;
            nomenclature.Article = nomenclatureDto.Article;
            nomenclature.Barcode = nomenclatureDto.Barcode;
            nomenclature.Type = ParseNomenclatureType(nomenclatureDto.Type);
            nomenclature.UnitId = nomenclatureDto.UnitId;
            nomenclature.AccountId = nomenclatureDto.AccountId;
            nomenclature.DefaultVatAccountId = nomenclatureDto.DefaultVatAccountId;
            nomenclature.StorageLocationId = nomenclatureDto.StorageLocationId;
            nomenclature.PurchasePrice = nomenclatureDto.PurchasePrice;
            nomenclature.SalePrice = nomenclatureDto.SalePrice;
            nomenclature.MinStock = nomenclatureDto.MinStock;
            nomenclature.MaxStock = nomenclatureDto.MaxStock;
            nomenclature.Description = nomenclatureDto.Description;
            nomenclature.Note = nomenclatureDto.Note;
            nomenclature.UpdatedAt = DateTime.UtcNow;

            await _nomenclatureRepository.UpdateAsync(nomenclature);
            return await MapToDto(nomenclature);
        }

        public async Task<bool> ArchiveNomenclatureAsync(int id)
        {
            var nomenclature = await _nomenclatureRepository.GetByIdAsync(id);
            if (nomenclature == null) return false;

            nomenclature.IsArchived = true;
            nomenclature.ArchivedAt = DateTime.UtcNow;
            nomenclature.UpdatedAt = DateTime.UtcNow;

            await _nomenclatureRepository.UpdateAsync(nomenclature);
            return true;
        }

        public async Task<bool> UnarchiveNomenclatureAsync(int id)
        {
            var nomenclature = await _nomenclatureRepository.GetByIdAsync(id);
            if (nomenclature == null || !nomenclature.IsArchived) return false;

            nomenclature.IsArchived = false;
            nomenclature.ArchivedAt = null;
            nomenclature.UpdatedAt = DateTime.UtcNow;

            await _nomenclatureRepository.UpdateAsync(nomenclature);
            return true;
        }

        public async Task<bool> DeleteNomenclatureAsync(int id)
        {
            var nomenclature = await _nomenclatureRepository.GetByIdAsync(id);
            if (nomenclature == null) return false;

            // TODO: Проверить, есть ли движения по этой номенклатуре

            await _nomenclatureRepository.DeleteAsync(nomenclature);
            return true;
        }

        public async Task<bool> IsArticleUniqueAsync(string article, int? excludeId = null)
        {
            var items = await _nomenclatureRepository.FindAsync(n => n.Article == article);

            if (excludeId.HasValue)
            {
                return !items.Any(i => i.Id != excludeId.Value);
            }

            return !items.Any();
        }

        public async Task UpdateStockAsync(int id, decimal quantity)
        {
            var nomenclature = await _nomenclatureRepository.GetByIdAsync(id);
            if (nomenclature == null) return;

            nomenclature.CurrentStock = (nomenclature.CurrentStock ?? 0) + quantity;
            nomenclature.UpdatedAt = DateTime.UtcNow;

            await _nomenclatureRepository.UpdateAsync(nomenclature);
        }

        // Вспомогательные методы
        private async Task<NomenclatureDto> MapToDto(Nomenclature nomenclature)
        {
            var account = await _accountRepository.GetByIdAsync(nomenclature.AccountId);
            var vatAccount = await _accountRepository.GetByIdAsync(nomenclature.DefaultVatAccountId);
            var unit = await _unitRepository.GetByIdAsync(nomenclature.UnitId);

            string? storageLocationFullPath = null;
            StorageLocation? storageLocation = null;

            if (nomenclature.StorageLocationId.HasValue)
            {
                storageLocation = await _storageLocationRepository.GetByIdAsync(nomenclature.StorageLocationId.Value);
                if (storageLocation != null)
                {
                    storageLocationFullPath = await GetStorageLocationFullPath(storageLocation);
                }
            }

            return new NomenclatureDto
            {
                Id = nomenclature.Id,
                Name = nomenclature.Name,
                FullName = nomenclature.FullName,
                Article = nomenclature.Article,
                Barcode = nomenclature.Barcode,
                Type = nomenclature.Type.ToString(),
                TypeDisplay = NomenclatureHelper.GetRussianType(nomenclature.Type),
                UnitId = nomenclature.UnitId,
                UnitCode = unit?.Code,
                UnitShortName = unit?.ShortName,
                UnitFullName = unit?.FullName,
                AccountId = nomenclature.AccountId,
                AccountCode = account?.Code,
                AccountName = account?.Name,
                DefaultVatAccountId = nomenclature.DefaultVatAccountId,
                DefaultVatAccountCode = vatAccount?.Code,
                DefaultVatAccountName = vatAccount?.Name,
                StorageLocationId = nomenclature.StorageLocationId,
                StorageLocationCode = storageLocation?.Code,
                StorageLocationName = storageLocation?.Name,
                StorageLocationFullPath = storageLocationFullPath,
                PurchasePrice = nomenclature.PurchasePrice,
                SalePrice = nomenclature.SalePrice,
                CurrentStock = nomenclature.CurrentStock,
                MinStock = nomenclature.MinStock,
                MaxStock = nomenclature.MaxStock,
                Description = nomenclature.Description,
                Note = nomenclature.Note,
                IsArchived = nomenclature.IsArchived,
                CreatedAt = nomenclature.CreatedAt,
                UpdatedAt = nomenclature.UpdatedAt
            };
        }

        private async Task<string> GetStorageLocationFullPath(StorageLocation location)
        {
            var path = location.Name;
            var current = location;

            while (current.ParentId.HasValue)
            {
                current = await _storageLocationRepository.GetByIdAsync(current.ParentId.Value);
                if (current != null)
                {
                    path = current.Name + " / " + path;
                }
                else
                {
                    break;
                }
            }

            return path;
        }

        private NomenclatureType ParseNomenclatureType(string type)
        {
            // Пробуем распарсить как английское название
            if (Enum.TryParse<NomenclatureType>(type, out var enumType))
            {
                return enumType;
            }

            // Если не получилось, пробуем преобразовать из русского
            return NomenclatureHelper.GetTypeFromRussian(type);
        }       
    }
}