using GlavnayaKniga.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IWordExportService
    {
        Task<byte[]> ExportAccountsToWordAsync(IEnumerable<AccountDto> accounts, string title = "План счетов");
        Task<string> SaveAccountsToWordFileAsync(IEnumerable<AccountDto> accounts, string filePath, string title = "План счетов");
    }
}