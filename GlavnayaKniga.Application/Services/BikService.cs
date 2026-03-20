using GlavnayaKniga.Application.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class BikService : IBikService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://bik-info.ru/api.html";

        public BikService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GlavnayaKniga");
        }

        public async Task<BankInfo?> GetBankInfoByBikAsync(string bik)
        {
            if (string.IsNullOrWhiteSpace(bik) || !ValidateBik(bik, out _, out _))
                return null;

            try
            {
                string requestUrl = $"{BASE_API_URL}?type=json&bik={bik}";
                Debug.WriteLine($"Запрос к API: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Получен JSON: {json}");

                // Парсим JSON с учетом возможных HTML-сущностей
                return ParseBankInfoFromJson(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Общая ошибка: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Парсинг JSON с последующим декодированием HTML-сущностей
        /// </summary>
        private BankInfo? ParseBankInfoFromJson(string json)
        {
            try
            {
                // Создаем временную модель для десериализации
                var tempModel = JsonSerializer.Deserialize<BankInfoTemp>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tempModel == null)
                    return null;

                var bankInfo = new BankInfo
                {
                    Bik = tempModel.Bik ?? string.Empty,
                    Name = DecodeHtmlEntities(tempModel.Name ?? string.Empty),
                    ShortName = DecodeHtmlEntities(tempModel.Namemini ?? tempModel.Shortname ?? string.Empty),
                    CorrespondentAccount = tempModel.Ks ?? tempModel.CorrAccount ?? string.Empty,
                    City = DecodeHtmlEntities(tempModel.City),
                    Address = DecodeHtmlEntities(tempModel.Address),
                    Phone = tempModel.Phone,
                    Okato = tempModel.Okato,
                    Okpo = tempModel.Okpo,
                    RegistrationNumber = tempModel.Regnum ?? tempModel.RegistrationNumber,
                    Srok = tempModel.Srok,
                    DateAdd = tempModel.Dateadd ?? tempModel.DateAdded,
                    DateChange = tempModel.Datechange ?? tempModel.DateChanged
                };

                if (string.IsNullOrEmpty(bankInfo.Bik) || string.IsNullOrEmpty(bankInfo.Name))
                {
                    Debug.WriteLine("Не удалось извлечь основные поля БИК");
                    return null;
                }

                Debug.WriteLine($"Успешно распарсен БИК: {bankInfo.Bik}, {bankInfo.Name}");
                return bankInfo;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Ошибка десериализации JSON: {ex.Message}");

                // Если не удалось десериализовать, пробуем ручной парсинг как запасной вариант
                return ParseBankInfoManually(json);
            }
        }

        /// <summary>
        /// Временная модель для десериализации
        /// </summary>
        private class BankInfoTemp
        {
            public string? Bik { get; set; }
            public string? Ks { get; set; }
            public string? CorrAccount { get; set; }
            public string? Name { get; set; }
            public string? Namemini { get; set; }
            public string? Shortname { get; set; }
            public string? Index { get; set; }
            public string? City { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? Okato { get; set; }
            public string? Okpo { get; set; }
            public string? Regnum { get; set; }
            public string? RegistrationNumber { get; set; }
            public string? Srok { get; set; }
            public string? Dateadd { get; set; }
            public string? DateAdded { get; set; }
            public string? Datechange { get; set; }
            public string? DateChanged { get; set; }
        }

        /// <summary>
        /// Ручной парсинг JSON (запасной вариант)
        /// </summary>
        private BankInfo? ParseBankInfoManually(string json)
        {
            try
            {
                var bankInfo = new BankInfo();

                bankInfo.Bik = ExtractValue(json, "bik") ?? string.Empty;
                bankInfo.Name = DecodeHtmlEntities(ExtractValue(json, "name") ?? string.Empty);
                bankInfo.ShortName = DecodeHtmlEntities(ExtractValue(json, "namemini") ?? ExtractValue(json, "shortname") ?? string.Empty);
                bankInfo.CorrespondentAccount = ExtractValue(json, "ks") ?? ExtractValue(json, "corrAccount") ?? string.Empty;
                bankInfo.City = DecodeHtmlEntities(ExtractValue(json, "city"));
                bankInfo.Address = DecodeHtmlEntities(ExtractValue(json, "address"));
                bankInfo.Phone = ExtractValue(json, "phone");
                bankInfo.Okato = ExtractValue(json, "okato");
                bankInfo.Okpo = ExtractValue(json, "okpo");
                bankInfo.RegistrationNumber = ExtractValue(json, "regnum") ?? ExtractValue(json, "registrationNumber");
                bankInfo.Srok = ExtractValue(json, "srok");
                bankInfo.DateAdd = ExtractValue(json, "dateadd") ?? ExtractValue(json, "dateAdded");
                bankInfo.DateChange = ExtractValue(json, "datechange") ?? ExtractValue(json, "dateChanged");

                if (string.IsNullOrEmpty(bankInfo.Bik) || string.IsNullOrEmpty(bankInfo.Name))
                {
                    Debug.WriteLine("Не удалось извлечь основные поля БИК");
                    return null;
                }

                Debug.WriteLine($"Успешно распарсен БИК (ручной режим): {bankInfo.Bik}, {bankInfo.Name}");
                return bankInfo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при ручном парсинге: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Извлечение значения из JSON (ручной режим)
        /// </summary>
        private string? ExtractValue(string json, string key)
        {
            // Ищем паттерн: "key":"значение"
            string pattern = $"\"{key}\"\\s*:\\s*\"([^\"]*)\"";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);

            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            // Если не нашли в кавычках, пробуем без кавычек (для числовых значений)
            pattern = $"\"{key}\"\\s*:\\s*([^,\\}}]*)";
            match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                var value = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(value) && !value.Equals("null", StringComparison.OrdinalIgnoreCase))
                    return value;
            }

            return null;
        }

        /// <summary>
        /// Декодирование HTML-сущностей в строке
        /// </summary>
        private string DecodeHtmlEntities(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&apos;", "'")
                .Replace("&#39;", "'");
        }
 

        /// <summary>
        /// Валидация БИК (из PHP кода)
        /// </summary>
        public bool ValidateBik(string bik, out string? errorMessage, out int? errorCode)
        {
            errorMessage = null;
            errorCode = null;

            if (string.IsNullOrEmpty(bik))
            {
                errorCode = 1;
                errorMessage = "БИК пуст";
                return false;
            }

            if (!Regex.IsMatch(bik, @"^[0-9]+$"))
            {
                errorCode = 2;
                errorMessage = "БИК может состоять только из цифр";
                return false;
            }

            if (bik.Length != 9)
            {
                errorCode = 3;
                errorMessage = "БИК может состоять только из 9 цифр";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Валидация расчетного счета (из PHP кода)
        /// </summary>
        public bool ValidateRs(string rs, string bik, out string? errorMessage, out int? errorCode)
        {
            errorMessage = null;
            errorCode = null;

            // Сначала проверяем БИК
            if (!ValidateBik(bik, out errorMessage, out errorCode))
                return false;

            // Проверяем расчетный счет
            if (string.IsNullOrEmpty(rs))
            {
                errorCode = 1;
                errorMessage = "Р/С пуст";
                return false;
            }

            if (!Regex.IsMatch(rs, @"^[0-9]+$"))
            {
                errorCode = 2;
                errorMessage = "Р/С может состоять только из цифр";
                return false;
            }

            if (rs.Length != 20)
            {
                errorCode = 3;
                errorMessage = "Р/С может состоять только из 20 цифр";
                return false;
            }

            // Формируем строку для расчета: последние 3 цифры БИК + весь расчетный счет
            string bikRs = bik.Substring(6, 3) + rs;
            Debug.WriteLine($"Строка для расчета Р/С (последние 3 БИК + Р/С): {bikRs}");

            int checksum = 0;
            int[] weights = { 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1 };

            Debug.Write("Расчет Р/С: ");
            for (int i = 0; i < bikRs.Length && i < weights.Length; i++)
            {
                int digit = int.Parse(bikRs[i].ToString());
                int product = digit * weights[i];
                checksum += product;
                Debug.Write($"{digit}*{weights[i]}={product} ");
            }

            Debug.WriteLine($"\nСумма: {checksum}, Остаток от деления на 10: {checksum % 10}");

            if (checksum % 10 == 0)
            {
                return true;
            }
            else
            {
                errorCode = 4;
                errorMessage = "Неправильное контрольное число";
                return false;
            }
        }

        /// <summary>
        /// Валидация корреспондентского счета (из PHP кода)
        /// </summary>
        public bool ValidateKs(string ks, string bik, out string? errorMessage, out int? errorCode)
        {
            errorMessage = null;
            errorCode = null;

            // Сначала проверяем БИК
            if (!ValidateBik(bik, out errorMessage, out errorCode))
                return false;

            // Проверяем корсчет
            if (string.IsNullOrEmpty(ks))
            {
                errorCode = 1;
                errorMessage = "К/С пуст";
                return false;
            }

            if (!Regex.IsMatch(ks, @"^[0-9]+$"))
            {
                errorCode = 2;
                errorMessage = "К/С может состоять только из цифр";
                return false;
            }

            if (ks.Length != 20)
            {
                errorCode = 3;
                errorMessage = "К/С может состоять только из 20 цифр";
                return false;
            }

            // Формируем строку для расчета: "0" + (последние 5 цифр БИК, первые 2 из них) + весь корсчет
            // В PHP: '0' . substr($bik, -5, 2) . $ks
            string bikPart = bik.Substring(bik.Length - 5, 2); // последние 5 цифр, берем первые 2 из них
            string bikKs = "0" + bikPart + ks;
            Debug.WriteLine($"Строка для расчета К/С (0 + {bikPart} + {ks}): {bikKs}");

            int checksum = 0;
            int[] weights = { 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1 };

            Debug.Write("Расчет К/С: ");
            for (int i = 0; i < bikKs.Length && i < weights.Length; i++)
            {
                int digit = int.Parse(bikKs[i].ToString());
                int product = digit * weights[i];
                checksum += product;
                Debug.Write($"{digit}*{weights[i]}={product} ");
            }

            Debug.WriteLine($"\nСумма: {checksum}, Остаток от деления на 10: {checksum % 10}");

            if (checksum % 10 == 0)
            {
                return true;
            }
            else
            {
                errorCode = 4;
                errorMessage = "Неправильное контрольное число";
                return false;
            }
        }

        /// <summary>
        /// Проверка расчетного счета на соответствие БИК (обертка для удобства)
        /// </summary>
        public bool ValidateAccount(string accountNumber, string bik, string? corrAccount = null)
        {
            Debug.WriteLine($"\n=== Начало валидации счета ===");
            Debug.WriteLine($"Расчетный счет: {accountNumber}");
            Debug.WriteLine($"БИК: {bik}");
            Debug.WriteLine($"Корсчет: {corrAccount ?? "не указан"}");

            if (!string.IsNullOrWhiteSpace(corrAccount))
            {
                if (!ValidateKs(corrAccount, bik, out string? ksError, out int? ksCode))
                {
                    Debug.WriteLine($"Ошибка валидации корсчета: {ksError} (код {ksCode})");
                    return false;
                }
                Debug.WriteLine($"Корсчет прошел валидацию");
            }

            bool result = ValidateRs(accountNumber, bik, out string? rsError, out int? rsCode);

            if (result)
            {
                Debug.WriteLine($"Расчетный счет прошел валидацию");
            }
            else
            {
                Debug.WriteLine($"Ошибка валидации Р/С: {rsError} (код {rsCode})");
            }

            Debug.WriteLine($"=== Конец валидации ===\n");

            return result;
        }

        public string CalculateCorrAccountKey(string bik, string accountWithoutKey)
        {
            throw new NotImplementedException();
        }
    }
}