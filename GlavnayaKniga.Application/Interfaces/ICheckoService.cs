using GlavnayaKniga.Application.DTOs;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface ICheckoService
    {
        /// <summary>
        /// Получить информацию о юридическом лице по ИНН или ОГРН
        /// </summary>
        Task<CheckoCompanyData?> GetCompanyByInnAsync(string inn);

        /// <summary>
        /// Получить информацию об индивидуальном предпринимателе по ИНН или ОГРНИП
        /// </summary>
        Task<CheckoEntrepreneurData?> GetEntrepreneurByInnAsync(string inn);

        /// <summary>
        /// Определить тип контрагента по ИНН и получить данные
        /// </summary>
        Task<object?> GetCounterpartyDataAsync(string inn);
    }
}