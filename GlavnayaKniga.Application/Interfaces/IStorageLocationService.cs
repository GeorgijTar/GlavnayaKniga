using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IStorageLocationService
    {
        Task<IEnumerable<StorageLocationDto>> GetAllLocationsAsync(bool includeArchived = false);
        Task<StorageLocationDto?> GetLocationByIdAsync(int id);
        Task<StorageLocationDto?> GetLocationByCodeAsync(string code);
        Task<IEnumerable<StorageLocationDto>> GetLocationsByTypeAsync(string type, bool includeArchived = false);
        Task<IEnumerable<StorageLocationDto>> GetLocationsHierarchyAsync(bool includeArchived = false);
        Task<IEnumerable<StorageLocationDto>> SearchLocationsAsync(string searchText, bool includeArchived = false);
        Task<StorageLocationDto> CreateLocationAsync(StorageLocationDto locationDto);
        Task<StorageLocationDto> UpdateLocationAsync(StorageLocationDto locationDto);
        Task<bool> ArchiveLocationAsync(int id);
        Task<bool> UnarchiveLocationAsync(int id);
        Task<bool> DeleteLocationAsync(int id);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<bool> HasChildrenAsync(int id);
    }
}