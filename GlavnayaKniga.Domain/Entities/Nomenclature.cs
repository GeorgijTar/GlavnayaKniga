using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Тип номенклатуры
    /// </summary>
    public enum NomenclatureType
    {
        Material = 1,           // Материалы
        Inventory = 2,          // Инвентарь
        Fertilizer = 3,         // Удобрения
        PlantProtection = 4,    // Средства защиты растений
        Seeds = 5,              // Семена
        Fuel = 6,               // Топливо
        SpareParts = 7,         // Запчасти
        Equipment = 8,          // Оборудование
        Work = 9,               // Работы (НОВЫЙ)
        Service = 10,           // Услуги (НОВЫЙ)
        Other = 99              // Прочее
    }
    

    /// <summary>
    /// Номенклатура (товары, материалы, услуги)
    /// </summary>
    public class Nomenclature
    {
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Полное наименование (для печати)
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Артикул
        /// </summary>
        public string? Article { get; set; }

        /// <summary>
        /// Штрих-код
        /// </summary>
        public string? Barcode { get; set; }

        /// <summary>
        /// Тип номенклатуры
        /// </summary>
        public NomenclatureType Type { get; set; }

        //// <summary>
        /// Единица измерения
        /// </summary>
        public int UnitId { get; set; }
        public UnitOfMeasure Unit { get; set; } = null!;

        /// <summary>
        /// Счет учета (субсчет, например 10.1, 10.3, 10.5 и т.д.)
        /// </summary>
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        /// <summary>
        /// Счет учета НДС по умолчанию
        /// </summary>
        public int DefaultVatAccountId { get; set; }
        public Account DefaultVatAccount { get; set; } = null!;

        /// <summary>
        /// Место хранения (склад, ячейка и т.д.)
        /// </summary>
        public int? StorageLocationId { get; set; }
        public StorageLocation? StorageLocation { get; set; }

        /// <summary>
        /// Цена закупки (последняя)
        /// </summary>
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Цена продажи
        /// </summary>
        public decimal? SalePrice { get; set; }

        /// <summary>
        /// Остаток на складе
        /// </summary>
        public decimal? CurrentStock { get; set; }

        /// <summary>
        /// Минимальный остаток
        /// </summary>
        public decimal? MinStock { get; set; }

        /// <summary>
        /// Максимальный остаток
        /// </summary>
        public decimal? MaxStock { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Архивный (не используется в новых документах)
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