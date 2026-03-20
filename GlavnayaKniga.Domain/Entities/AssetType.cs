using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Тип объекта (основного средства или оборудования)
    /// </summary>
    public class AssetType
    {
        public int Id { get; set; }

        /// <summary>
        /// Наименование типа
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание типа
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Архивный (не используется в новых объектах)
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
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}