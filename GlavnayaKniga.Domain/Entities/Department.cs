using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Отдел/подразделение организации
    /// </summary>
    public class Department
    {
        public int Id { get; set; }

        /// <summary>
        /// Код отдела
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Наименование отдела
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Полное наименование
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Родительский отдел (для иерархии)
        /// </summary>
        public int? ParentId { get; set; }
        public Department? Parent { get; set; }

        /// <summary>
        /// Дочерние отделы
        /// </summary>
        public ICollection<Department> Children { get; set; } = new List<Department>();

        /// <summary>
        /// Руководитель отдела (ссылка на сотрудника)
        /// </summary>
        public int? HeadEmployeeId { get; set; }
        public Employee? HeadEmployee { get; set; }

        /// <summary>
        /// Телефон отдела
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Email отдела
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Местоположение
        /// </summary>
        public string? Location { get; set; }

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

        // Навигационные свойства
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}