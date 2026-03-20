using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Единица измерения
    /// </summary>
    public class UnitOfMeasure
    {
        public int Id { get; set; }

        /// <summary>
        /// Код единицы измерения (ОКЕИ)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Краткое наименование (шт, кг, м и т.д.)
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Полное наименование
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Международное обозначение
        /// </summary>
        public string? InternationalCode { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

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
        public ICollection<Nomenclature> Nomenclatures { get; set; } = new List<Nomenclature>();
    }
}