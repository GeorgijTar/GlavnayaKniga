using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.DTOs
{
    public class StorageLocationDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeDisplay { get; set; } = string.Empty;

        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? ParentCode { get; set; }

        public string? Description { get; set; }
        public string? Address { get; set; }

        // Заменяем ResponsiblePerson на ResponsibleEmployee
        public int? ResponsibleEmployeeId { get; set; }
        public string? ResponsibleEmployeeName { get; set; }

        public decimal? Area { get; set; }
        public decimal? Capacity { get; set; }
        public string? TemperatureRegime { get; set; }

        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Для иерархического отображения
        public List<StorageLocationDto> Children { get; set; } = new();

        // Вычисляемые свойства
        public string DisplayName => $"{Code} - {Name}";
        public string FullPath => GetFullPath();
        public string StatusDisplay => IsArchived ? "Архивный" : "Активный";
        public string AreaDisplay => Area?.ToString("N2") ?? "—";
        public string CapacityDisplay => Capacity?.ToString("N2") ?? "—";
        public string ResponsibleDisplay => ResponsibleEmployeeName ?? "—";

        private string GetFullPath()
        {
            return ParentName != null ? $"{ParentName} / {Name}" : Name;
        }
    }
}