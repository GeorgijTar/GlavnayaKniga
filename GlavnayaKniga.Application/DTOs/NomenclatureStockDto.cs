using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class NomenclatureStockDto
    {
        public int NomenclatureId { get; set; }
        public string NomenclatureName { get; set; } = string.Empty;
        public string NomenclatureArticle { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public int? StorageLocationId { get; set; }
        public string? StorageLocationName { get; set; }

        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }

        // Вычисляемое свойство
        public decimal AvailableStock => CurrentStock - ReservedStock;

        public decimal? MinStock { get; set; }
        public decimal? MaxStock { get; set; }

        public decimal AveragePrice { get; set; }

        // Вычисляемое свойство
        public decimal TotalValue => CurrentStock * AveragePrice;

        public DateTime LastMovementDate { get; set; }
        public string? LastMovementType { get; set; }

        // Вычисляемое свойство
        public bool NeedRestock => MinStock.HasValue && CurrentStock < MinStock.Value;
    }
}