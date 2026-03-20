using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class AssetTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Для отображения в списке
        public string DisplayName => Name;
        public string StatusDisplay => IsArchived ? "Архивный" : "Активный";
        public int AssetsCount { get; set; }
    }
}