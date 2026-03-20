using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IIndividualService
    {
        Task<IEnumerable<IndividualDto>> GetAllIndividualsAsync(bool includeArchived = false);
        Task<IndividualDto?> GetIndividualByIdAsync(int id);
        Task<IndividualDto?> GetIndividualByINNAsync(string inn);
        Task<IndividualDto?> GetIndividualBySNILSAsync(string snils);
        Task<IEnumerable<IndividualDto>> SearchIndividualsAsync(string searchText, bool includeArchived = false);
        Task<IndividualDto> CreateIndividualAsync(IndividualDto individualDto);
        Task<IndividualDto> UpdateIndividualAsync(IndividualDto individualDto);
        Task<bool> ArchiveIndividualAsync(int id);
        Task<bool> UnarchiveIndividualAsync(int id);
        Task<bool> DeleteIndividualAsync(int id);
        Task<bool> IsINNUniqueAsync(string inn, int? excludeId = null);
        Task<bool> IsSNILSUniqueAsync(string snils, int? excludeId = null);
    }
}