using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IAssetService
    {
        Task<IEnumerable<AssetDto>> GetAllAssetsAsync(bool includeArchived = false);
        Task<AssetDto?> GetAssetByIdAsync(int id);
        Task<AssetDto?> GetAssetByInventoryNumberAsync(string inventoryNumber);
        Task<IEnumerable<AssetDto>> GetAssetsByTypeAsync(int assetTypeId, bool includeArchived = false);
        Task<IEnumerable<AssetDto>> SearchAssetsAsync(string searchText, bool includeArchived = false);
        Task<IEnumerable<AssetGroupDto>> GetAssetsGroupedByTypeAsync(bool includeArchived = false);
        Task<AssetDto> CreateAssetAsync(AssetDto assetDto);
        Task<AssetDto> UpdateAssetAsync(AssetDto assetDto);
        Task<bool> ArchiveAssetAsync(int id);
        Task<bool> UnarchiveAssetAsync(int id);
        Task<bool> DeleteAssetAsync(int id);
        Task<bool> IsInventoryNumberUniqueAsync(string inventoryNumber, int? excludeId = null);
    }

    public class AssetGroupDto
    {
        public AssetTypeDto AssetType { get; set; } = null!;
        public List<AssetDto> Assets { get; set; } = new();
        public int Count => Assets.Count;
    }
}