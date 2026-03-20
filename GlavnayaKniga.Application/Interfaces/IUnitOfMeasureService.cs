using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IUnitOfMeasureService
    {
        Task<IEnumerable<UnitOfMeasureDto>> GetAllUnitsAsync(bool includeArchived = false);
        Task<UnitOfMeasureDto?> GetUnitByIdAsync(int id);
        Task<UnitOfMeasureDto?> GetUnitByCodeAsync(string code);
        Task<IEnumerable<UnitOfMeasureDto>> SearchUnitsAsync(string searchText, bool includeArchived = false);
        Task<UnitOfMeasureDto> CreateUnitAsync(UnitOfMeasureDto unitDto);
        Task<UnitOfMeasureDto> UpdateUnitAsync(UnitOfMeasureDto unitDto);
        Task<bool> ArchiveUnitAsync(int id);
        Task<bool> UnarchiveUnitAsync(int id);
        Task<bool> DeleteUnitAsync(int id);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    }
}