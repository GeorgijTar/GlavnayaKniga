using GlavnayaKniga.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(bool includeDismissed = false);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto?> GetEmployeeByPersonnelNumberAsync(string personnelNumber);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByStatusAsync(string status);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department);
        Task<IEnumerable<EmployeeDto>> SearchEmployeesAsync(string searchText, bool includeDismissed = false);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employeeDto);
        Task<EmployeeDto> DismissEmployeeAsync(int id, DateTime dismissalDate, string orderNumber, string reason);
        Task<EmployeeDto> TransferEmployeeAsync(int id, int newPositionId, DateTime transferDate, string orderNumber);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<string> GeneratePersonnelNumberAsync();
        Task<IEnumerable<EmploymentHistoryDto>> GetEmploymentHistoryAsync(int employeeId);
    }
}