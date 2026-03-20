using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface INomenclatureService
    {
        Task<IEnumerable<NomenclatureDto>> GetAllNomenclatureAsync(bool includeArchived = false);
        Task<NomenclatureDto?> GetNomenclatureByIdAsync(int id);
        Task<NomenclatureDto?> GetNomenclatureByArticleAsync(string article);
        Task<IEnumerable<NomenclatureDto>> SearchNomenclatureAsync(string searchText, bool includeArchived = false);
        Task<IEnumerable<NomenclatureDto>> GetNomenclatureByTypeAsync(string type, bool includeArchived = false);
        Task<NomenclatureDto> CreateNomenclatureAsync(NomenclatureDto nomenclatureDto);
        Task<NomenclatureDto> UpdateNomenclatureAsync(NomenclatureDto nomenclatureDto);
        Task<bool> ArchiveNomenclatureAsync(int id);
        Task<bool> UnarchiveNomenclatureAsync(int id);
        Task<bool> DeleteNomenclatureAsync(int id);
        Task<bool> IsArticleUniqueAsync(string article, int? excludeId = null);
        Task UpdateStockAsync(int id, decimal quantity);
    }
}