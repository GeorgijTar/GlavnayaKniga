using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Статус сотрудника
    /// </summary>
    public enum EmployeeStatus
    {
        Active = 1,         // Работает
        OnLeave = 2,        // В отпуске
        Dismissed = 3,      // Уволен
        Probation = 4       // Испытательный срок
    }

    /// <summary>
    /// Сотрудник
    /// </summary>
    public class Employee
    {
        public int Id { get; set; }

        /// <summary>
        /// Ссылка на физическое лицо
        /// </summary>
        public int IndividualId { get; set; }
        public Individual Individual { get; set; } = null!;

        /// <summary>
        /// Табельный номер
        /// </summary>
        public string PersonnelNumber { get; set; } = string.Empty;

        /// <summary>
        /// Текущая должность
        /// </summary>
        public int CurrentPositionId { get; set; }
        public Position CurrentPosition { get; set; } = null!;

        /// <summary>
        /// Отдел
        /// </summary>
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        /// <summary>
        /// Статус сотрудника
        /// </summary>
        public EmployeeStatus Status { get; set; }

        /// <summary>
        /// Дата приема на работу
        /// </summary>
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Номер приказа о приеме
        /// </summary>
        public string? HireOrderNumber { get; set; }

        /// <summary>
        /// Дата приказа о приеме
        /// </summary>
        public DateTime? HireOrderDate { get; set; }

        /// <summary>
        /// Дата увольнения
        /// </summary>
        public DateTime? DismissalDate { get; set; }

        /// <summary>
        /// Номер приказа об увольнении
        /// </summary>
        public string? DismissalOrderNumber { get; set; }

        /// <summary>
        /// Дата приказа об увольнении
        /// </summary>
        public DateTime? DismissalOrderDate { get; set; }

        /// <summary>
        /// Причина увольнения
        /// </summary>
        public string? DismissalReason { get; set; }

        /// <summary>
        /// Рабочий телефон
        /// </summary>
        public string? WorkPhone { get; set; }

        /// <summary>
        /// Рабочий email
        /// </summary>
        public string? WorkEmail { get; set; }

        /// <summary>
        /// Руководитель (ссылка на другого сотрудника)
        /// </summary>
        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        public ICollection<EmploymentHistory> EmploymentHistory { get; set; } = new List<EmploymentHistory>();
        public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    }
}