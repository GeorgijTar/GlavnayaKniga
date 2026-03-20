using System;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Application.DTOs
{
    public class NomenclatureDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Article { get; set; }
        public string? Barcode { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TypeDisplay { get; set; } = string.Empty;

        public int UnitId { get; set; }
        public string? UnitCode { get; set; }
        public string? UnitShortName { get; set; }
        public string? UnitFullName { get; set; }

        public int AccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }

        // Поля для счета учета НДС по умолчанию
        public int DefaultVatAccountId { get; set; }
        public string? DefaultVatAccountCode { get; set; }
        public string? DefaultVatAccountName { get; set; }

        // Поля для связи с местом хранения
        public int? StorageLocationId { get; set; }
        public string? StorageLocationCode { get; set; }
        public string? StorageLocationName { get; set; }
        public string? StorageLocationFullPath { get; set; }

        public decimal? PurchasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? CurrentStock { get; set; }
        public decimal? MinStock { get; set; }
        public decimal? MaxStock { get; set; }

        public string? Description { get; set; }
        public string? Note { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Вычисляемые свойства
        public string DisplayName => $"{Name} ({Article})";
        public string DisplayAccount => $"{AccountCode} - {AccountName}";
        public string DisplayDefaultVatAccount => $"{DefaultVatAccountCode} - {DefaultVatAccountName}";
        public string DisplayStorageLocation => StorageLocationFullPath ?? StorageLocationName ?? "—";
        public string DisplayPrice => PurchasePrice?.ToString("N2") ?? "—";
        public string DisplayStock => CurrentStock?.ToString("N3") ?? "0";
        public string StatusDisplay => IsArchived ? "Архивный" : "Активный";
        public string UnitDisplay => UnitShortName ?? UnitCode ?? "—";
    }
}