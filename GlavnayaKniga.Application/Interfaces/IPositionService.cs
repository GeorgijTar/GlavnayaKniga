using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IPositionService
    {
        Task<IEnumerable<PositionDto>> GetAllPositionsAsync(bool includeArchived = false);
        Task<PositionDto?> GetPositionByIdAsync(int id);
        Task<PositionDto?> GetPositionByNameAsync(string name);
        Task<IEnumerable<PositionDto>> GetPositionsByCategoryAsync(string category, bool includeArchived = false);
        Task<IEnumerable<PositionDto>> SearchPositionsAsync(string searchText, bool includeArchived = false);
        Task<PositionDto> CreatePositionAsync(PositionDto positionDto);
        Task<PositionDto> UpdatePositionAsync(PositionDto positionDto);
        Task<bool> ArchivePositionAsync(int id);
        Task<bool> UnarchivePositionAsync(int id);
        Task<bool> DeletePositionAsync(int id);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}