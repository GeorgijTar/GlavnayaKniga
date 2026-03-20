using System;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Объект (основное средство, оборудование, транспорт)
    /// </summary>
    public class Asset
    {
        public int Id { get; set; }

        /// <summary>
        /// Наименование объекта
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Регистрационный номер (госномер, заводской номер и т.д.)
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Инвентарный номер
        /// </summary>
        public string? InventoryNumber { get; set; }

        /// <summary>
        /// Тип объекта
        /// </summary>
        public int AssetTypeId { get; set; }
        public AssetType AssetType { get; set; } = null!;

        /// <summary>
        /// Год выпуска
        /// </summary>
        public int? YearOfManufacture { get; set; }

        /// <summary>
        /// Модель
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Производитель
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Заводской номер
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Дата приобретения
        /// </summary>
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// Дата ввода в эксплуатацию
        /// </summary>
        public DateTime? CommissioningDate { get; set; }

        /// <summary>
        /// Дата списания
        /// </summary>
        public DateTime? DecommissioningDate { get; set; }

        /// <summary>
        /// Первоначальная стоимость
        /// </summary>
        public decimal? InitialCost { get; set; }

        /// <summary>
        /// Остаточная стоимость
        /// </summary>
        public decimal? ResidualValue { get; set; }

        /// <summary>
        /// Местоположение (подразделение, склад и т.д.)
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Ответственное лицо (контрагент - сотрудник)
        /// </summary>
        public int? ResponsiblePersonId { get; set; }
        public Counterparty? ResponsiblePerson { get; set; }

        /// <summary>
        /// Счет учета (например, 01.1)
        /// </summary>
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        /// <summary>
        /// Примечание
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Архивный (списан или выведен из эксплуатации)
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата обновления записи
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Дата архивации
        /// </summary>
        public DateTime? ArchivedAt { get; set; }
    }
}