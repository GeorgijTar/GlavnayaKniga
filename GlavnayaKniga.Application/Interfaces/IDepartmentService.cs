using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(bool includeArchived = false);
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto?> GetDepartmentByCodeAsync(string code);
        Task<IEnumerable<DepartmentDto>> GetDepartmentsHierarchyAsync(bool includeArchived = false);
        Task<IEnumerable<DepartmentDto>> SearchDepartmentsAsync(string searchText, bool includeArchived = false);
        Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto departmentDto);
        Task<DepartmentDto> UpdateDepartmentAsync(DepartmentDto departmentDto);
        Task<bool> ArchiveDepartmentAsync(int id);
        Task<bool> UnarchiveDepartmentAsync(int id);
        Task<bool> DeleteDepartmentAsync(int id);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<bool> HasChildrenAsync(int id);
    }
}