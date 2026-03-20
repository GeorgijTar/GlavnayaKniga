using System;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Запись в истории трудоустройства (кадровые переводы)
    /// </summary>
    public class EmploymentHistory
    {
        public int Id { get; set; }

        /// <summary>
        /// Ссылка на сотрудника
        /// </summary>
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        /// <summary>
        /// Должность
        /// </summary>
        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        /// <summary>
        /// Дата назначения
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания (для переводов - дата перевода, для увольнения - дата увольнения)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Номер приказа
        /// </summary>
        public string? OrderNumber { get; set; }

        /// <summary>
        /// Дата приказа
        /// </summary>
        public DateTime? OrderDate { get; set; }

        /// <summary>
        /// Тип изменения (прием, перевод, увольнение)
        /// </summary>
        public string ChangeType { get; set; } = string.Empty; // Hire, Transfer, Dismissal

        /// <summary>
        /// Причина изменения
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}