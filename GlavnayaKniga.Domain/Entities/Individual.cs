using System;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Физическое лицо
    /// </summary>
    public class Individual
    {
        public int Id { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Отчество
        /// </summary>
        public string? MiddleName { get; set; }

        /// <summary>
        /// Полное имя (ФИО)
        /// </summary>
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        /// <summary>
        /// Краткое имя (Фамилия И.О.)
        /// </summary>
        public string ShortName => $"{LastName} {FirstName?[0]}. {MiddleName?[0]}.".Trim();

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Место рождения
        /// </summary>
        public string? BirthPlace { get; set; }

        /// <summary>
        /// Пол (М/Ж)
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Гражданство
        /// </summary>
        public string? Citizenship { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        public string? RegistrationAddress { get; set; }

        /// <summary>
        /// Адрес проживания (фактический)
        /// </summary>
        public string? ActualAddress { get; set; }

        /// <summary>
        /// Телефон
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Серия паспорта
        /// </summary>
        public string? PassportSeries { get; set; }

        /// <summary>
        /// Номер паспорта
        /// </summary>
        public string? PassportNumber { get; set; }

        /// <summary>
        /// Дата выдачи паспорта
        /// </summary>
        public DateTime? PassportIssueDate { get; set; }

        /// <summary>
        /// Кем выдан паспорт
        /// </summary>
        public string? PassportIssuedBy { get; set; }

        /// <summary>
        /// Код подразделения
        /// </summary>
        public string? PassportDepartmentCode { get; set; }

        /// <summary>
        /// ИНН
        /// </summary>
        public string? INN { get; set; }

        /// <summary>
        /// СНИЛС
        /// </summary>
        public string? SNILS { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Архивный
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Дата архивации
        /// </summary>
        public DateTime? ArchivedAt { get; set; }
    }
}