using System;
using System.Collections.Generic;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.Application.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? FullCode { get; set; }
        public int? ParentId { get; set; }
        public string? ParentCode { get; set; }

        // Тип счета - убедитесь, что это свойство правильно сериализуется
        public AccountType Type { get; set; }

        // Является ли счет синтетическим
        public bool IsSynthetic { get; set; }

        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public List<AccountDto> Children { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Для отображения в TreeView
        public string DisplayName => IsArchived
            ? $"{Code} {Name} (Архивный)"
            : $"{Code} {Name}";

        // Цвет для архивных счетов
        public string DisplayColor => IsArchived ? "Gray" : "Black";

        // Иконка типа счета
        public string TypeIcon => GetTypeIcon();

        // Для отладки - показать числовое значение
        public int TypeValue => (int)Type;

        private string GetTypeIcon()
        {
            return Type switch
            {
                AccountType.Active => "📈",
                AccountType.Passive => "📉",
                AccountType.ActivePassive => "📊",
                _ => "📄"
            };
        }
    }
}