using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Тип места хранения
    /// </summary>
    public enum StorageLocationType
    {
        Warehouse = 1,      // Склад
        Section = 2,        // Участок/Отделение
        Cell = 3,           // Ячейка
        Rack = 4,           // Стеллаж
        Outdoor = 5,        // Открытая площадка
        Vehicle = 6,        // Транспортное средство
        Other = 99          // Прочее
    }

    /// <summary>
    /// Место хранения (склады, участки, ячейки и т.д.)
    /// </summary>
    public class StorageLocation
    {
        public int Id { get; set; }

        /// <summary>
        /// Код места хранения
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип места хранения
        /// </summary>
        public StorageLocationType Type { get; set; }

        /// <summary>
        /// Родительское место хранения (для иерархии)
        /// </summary>
        public int? ParentId { get; set; }
        public StorageLocation? Parent { get; set; }

        /// <summary>
        /// Дочерние места хранения
        /// </summary>
        public ICollection<StorageLocation> Children { get; set; } = new List<StorageLocation>();

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Адрес/местоположение
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Ответственное лицо (сотрудник)
        /// </summary>
        public int? ResponsibleEmployeeId { get; set; }
        public Employee? ResponsibleEmployee { get; set; }

        /// <summary>
        /// Площадь (кв.м)
        /// </summary>
        public decimal? Area { get; set; }

        /// <summary>
        /// Вместимость (объем, куб.м)
        /// </summary>
        public decimal? Capacity { get; set; }

        /// <summary>
        /// Температурный режим
        /// </summary>
        public string? TemperatureRegime { get; set; }

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