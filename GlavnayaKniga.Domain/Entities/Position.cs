using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Категория должности
    /// </summary>
    public enum PositionCategory
    {
        Manager = 1,        // Руководитель
        Specialist = 2,     // Специалист
        Worker = 3,         // Рабочий
        Other = 99          // Прочее
    }

    /// <summary>
    /// Должность
    /// </summary>
    public class Position
    {
        public int Id { get; set; }

        /// <summary>
        /// Наименование должности
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Краткое наименование
        /// </summary>
        public string? ShortName { get; set; }

        /// <summary>
        /// Категория должности
        /// </summary>
        public PositionCategory Category { get; set; }

        /// <summary>
        /// Описание должности
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Требования к образованию
        /// </summary>
        public string? EducationRequirements { get; set; }

        /// <summary>
        /// Требования к опыту работы (лет)
        /// </summary>
        public int? ExperienceYears { get; set; }

        /// <summary>
        /// Оклад по штатному расписанию
        /// </summary>
        public decimal? BaseSalary { get; set; }

        /// <summary>
        /// Архивная
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

        // Навигационные свойства
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<EmploymentHistory> EmploymentHistory { get; set; } = new List<EmploymentHistory>();
    }
}