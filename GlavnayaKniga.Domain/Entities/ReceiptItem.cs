using System;

namespace GlavnayaKniga.Domain.Entities
{
    public class ReceiptItem
    {
        public int Id { get; set; }

        /// <summary>
        /// Ссылка на документ
        /// </summary>
        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; } = null!;

        /// <summary>
        /// Номенклатура
        /// </summary>
        public int NomenclatureId { get; set; }
        public Nomenclature Nomenclature { get; set; } = null!;

        /// <summary>
        /// Количество
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Сумма без НДС
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Ставка НДС (5, 7, 10, 20, 22)
        /// </summary>
        public decimal? VatRate { get; set; }

        /// <summary>
        /// Сумма НДС
        /// </summary>
        public decimal? VatAmount { get; set; }

        /// <summary>
        /// Сумма с НДС
        /// </summary>
        public decimal? AmountWithVat { get; set; }

        /// <summary>
        /// Счет учета (дебет)
        /// </summary>
        public int DebitAccountId { get; set; }
        public Account DebitAccount { get; set; } = null!;

        /// <summary>
        /// Счет учета НДС (по умолчанию 19.1)
        /// </summary>
        public int VatAccountId { get; set; }
        public Account VatAccount { get; set; } = null!;

        /// <summary>
        /// Место хранения (для товаров/материалов)
        /// </summary>
        public int? StorageLocationId { get; set; }
        public StorageLocation? StorageLocation { get; set; }

        /// <summary>
        /// Примечание к строке
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Порядковый номер строки
        /// </summary>
        public int LineNumber { get; set; }
    }
}