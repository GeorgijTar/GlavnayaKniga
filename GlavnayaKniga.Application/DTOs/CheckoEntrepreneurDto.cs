using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GlavnayaKniga.Application.DTOs
{
    public class CheckoEntrepreneurResponse
    {
        [JsonPropertyName("data")]
        public CheckoEntrepreneurData? Data { get; set; }

        [JsonPropertyName("meta")]
        public CheckoMeta? Meta { get; set; }
    }

    public class CheckoEntrepreneurData
    {
        [JsonPropertyName("ОГРНИП")]
        public string? OGRNIP { get; set; }

        [JsonPropertyName("ИНН")]
        public string? INN { get; set; }

        [JsonPropertyName("ФИО")]
        public string? FullName { get; set; }

        [JsonPropertyName("Статус")]
        public CheckoStatus? Status { get; set; }
    }
}