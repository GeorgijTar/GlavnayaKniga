using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IAssetTypeService
    {
        Task<IEnumerable<AssetTypeDto>> GetAllAssetTypesAsync(bool includeArchived = false);
        Task<AssetTypeDto?> GetAssetTypeByIdAsync(int id);
        Task<AssetTypeDto?> GetAssetTypeByNameAsync(string name);
        Task<AssetTypeDto> CreateAssetTypeAsync(AssetTypeDto assetTypeDto);
        Task<AssetTypeDto> UpdateAssetTypeAsync(AssetTypeDto assetTypeDto);
        Task<bool> ArchiveAssetTypeAsync(int id);
        Task<bool> UnarchiveAssetTypeAsync(int id);
        Task<bool> DeleteAssetTypeAsync(int id);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}