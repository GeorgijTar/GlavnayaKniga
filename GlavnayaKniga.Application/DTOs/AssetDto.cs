using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string? InventoryNumber { get; set; }

        public int AssetTypeId { get; set; }
        public string? AssetTypeName { get; set; }

        public int? YearOfManufacture { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public string? SerialNumber { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? CommissioningDate { get; set; }
        public DateTime? DecommissioningDate { get; set; }

        public decimal? InitialCost { get; set; }
        public decimal? ResidualValue { get; set; }

        public string? Location { get; set; }

        public int? ResponsiblePersonId { get; set; }
        public string? ResponsiblePersonName { get; set; }

        public int AccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }

        public string? Note { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Вычисляемые свойства
        public string DisplayName => $"{Name} ({InventoryNumber})";
        public string FullInfo => $"{Name} - {RegistrationNumber} - {InventoryNumber}";
        public string StatusDisplay => IsArchived ? "Списан" : "В эксплуатации";
        public string YearDisplay => YearOfManufacture?.ToString() ?? "—";
        public string CostDisplay => InitialCost?.ToString("N2") ?? "—";
        public string ResponsibleDisplay => ResponsiblePersonName ?? "—";
    }
}