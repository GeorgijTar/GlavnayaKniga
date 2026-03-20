using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IBikService
    {
        /// <summary>
        /// Получить информацию о банке по БИК из онлайн-источника
        /// </summary>
        Task<BankInfo?> GetBankInfoByBikAsync(string bik);

        /// <summary>
        /// Проверить расчетный счет на соответствие БИК и корсчету
        /// </summary>
        bool ValidateAccount(string accountNumber, string bik, string? corrAccount = null);

        /// <summary>
        /// Валидация БИК с детальными ошибками
        /// </summary>
        bool ValidateBik(string bik, out string? errorMessage, out int? errorCode);

        /// <summary>
        /// Валидация расчетного счета с детальными ошибками
        /// </summary>
        bool ValidateRs(string rs, string bik, out string? errorMessage, out int? errorCode);

        /// <summary>
        /// Валидация корреспондентского счета с детальными ошибками
        /// </summary>
        bool ValidateKs(string ks, string bik, out string? errorMessage, out int? errorCode);
    }

    public class BankInfo
    {
        public string Bik { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string CorrespondentAccount { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Okato { get; set; }
        public string? Okpo { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Srok { get; set; }
        public string? DateAdd { get; set; }
        public string? DateChange { get; set; }
    }
}