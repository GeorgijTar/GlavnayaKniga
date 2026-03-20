using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GlavnayaKniga.Application.DTOs
{
    public class CheckoCompanyResponse
    {
        [JsonPropertyName("data")]
        public CheckoCompanyData? Data { get; set; }

        [JsonPropertyName("meta")]
        public CheckoMeta? Meta { get; set; }
    }

    public class CheckoCompanyData
    {
        [JsonPropertyName("ОГРН")]
        public string? OGRN { get; set; }

        [JsonPropertyName("ИНН")]
        public string? INN { get; set; }

        [JsonPropertyName("КПП")]
        public string? KPP { get; set; }

        [JsonPropertyName("ОКПО")]
        public string? OKPO { get; set; }

        [JsonPropertyName("ДатаРег")]
        public string? DateReg { get; set; }

        [JsonPropertyName("ДатаОГРН")]
        public string? DateOGRN { get; set; }

        [JsonPropertyName("НаимСокр")]
        public string? ShortName { get; set; }

        [JsonPropertyName("НаимПолн")]
        public string? FullName { get; set; }

        [JsonPropertyName("Статус")]
        public CheckoStatus? Status { get; set; }

        [JsonPropertyName("ЮрАдрес")]
        public CheckoAddress? LegalAddress { get; set; }

        [JsonPropertyName("Контакты")]
        public CheckoContacts? Contacts { get; set; }

        [JsonPropertyName("Руковод")]
        public List<CheckoHead>? Management { get; set; }
    }

    public class CheckoStatus
    {
        [JsonPropertyName("Код")]
        public string? Code { get; set; }

        [JsonPropertyName("Наим")]
        public string? Name { get; set; }
    }

    public class CheckoAddress
    {
        [JsonPropertyName("АдресРФ")]
        public string? Address { get; set; }

        [JsonPropertyName("НасПункт")]
        public string? Locality { get; set; }
    }

    public class CheckoContacts
    {
        [JsonPropertyName("Тел")]
        public List<string>? Phones { get; set; }

        [JsonPropertyName("Емэйл")]
        public List<string>? Emails { get; set; }
    }

    public class CheckoHead
    {
        [JsonPropertyName("ФИО")]
        public string? FullName { get; set; }

        [JsonPropertyName("НаимДолжн")]
        public string? Position { get; set; }
    }

    public class CheckoMeta
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("today_request_count")]
        public int TodayRequestCount { get; set; }

        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }
    }
}