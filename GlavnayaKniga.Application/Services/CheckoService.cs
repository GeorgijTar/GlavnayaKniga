using GlavnayaKniga.Application.Configuration;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class CheckoService : ICheckoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BASE_URL = "https://api.checko.ru/v2";

        public CheckoService(HttpClient httpClient, IOptions<CheckoConfig> config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.Timeout = TimeSpan.FromSeconds(15);

            if (config?.Value == null)
            {
                Debug.WriteLine("❌ CheckoService: config.Value is null");
                throw new ArgumentNullException(nameof(config), "Конфигурация Checko не найдена");
            }

            if (string.IsNullOrEmpty(config.Value.ApiKey))
            {
                Debug.WriteLine("❌ CheckoService: ApiKey is null or empty");
                throw new InvalidOperationException("API ключ Checko не найден в конфигурации");
            }

            _apiKey = config.Value.ApiKey;
            Debug.WriteLine($"✅ CheckoService инициализирован с API ключом: {_apiKey?.Substring(0, 5)}...");
        }

        public async Task<CheckoCompanyData?> GetCompanyByInnAsync(string inn)
        {
            try
            {
                // Проверяем API ключ
                if (string.IsNullOrEmpty(_apiKey))
                {
                    Debug.WriteLine("❌ GetCompanyByInnAsync: API ключ не установлен");
                    return null;
                }

                string requestUrl = $"{BASE_URL}/company?key={_apiKey}&inn={inn}";
                Debug.WriteLine($"Запрос к API: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Получен JSON (первые 200 символов): {json.Substring(0, Math.Min(200, json.Length))}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null
                };

                var result = JsonSerializer.Deserialize<CheckoCompanyResponse>(json, options);

                if (result?.Meta?.Status == "ok" && result.Data != null)
                {
                    Debug.WriteLine($"✅ Данные успешно десериализованы: {result.Data.ShortName}");
                    return result.Data;
                }

                Debug.WriteLine($"❌ Ошибка в ответе API: {result?.Meta?.Message}");
                return null;
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"❌ Ошибка десериализации JSON: {jsonEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Общая ошибка: {ex.Message}");
                return null;
            }
        }

        public async Task<CheckoEntrepreneurData?> GetEntrepreneurByInnAsync(string inn)
        {
            try
            {
                // Проверяем API ключ
                if (string.IsNullOrEmpty(_apiKey))
                {
                    Debug.WriteLine("❌ GetEntrepreneurByInnAsync: API ключ не установлен");
                    return null;
                }

                string requestUrl = $"{BASE_URL}/entrepreneur?key={_apiKey}&inn={inn}";
                Debug.WriteLine($"Запрос к API: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Получен JSON: {json}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null
                };

                var result = JsonSerializer.Deserialize<CheckoEntrepreneurResponse>(json, options);

                if (result?.Meta?.Status == "ok" && result.Data != null)
                {
                    return result.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка получения данных ИП: {ex.Message}");
                return null;
            }
        }

        public async Task<object?> GetCounterpartyDataAsync(string inn)
        {
            // Сначала пробуем получить как юридическое лицо
            var company = await GetCompanyByInnAsync(inn);
            if (company != null)
                return company;

            // Если не нашли, пробуем как ИП
            var entrepreneur = await GetEntrepreneurByInnAsync(inn);
            return entrepreneur;
        }
    }
}