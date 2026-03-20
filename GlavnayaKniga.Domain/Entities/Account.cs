using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    public enum AccountType
    {
        Active = 1,        // Активный - сальдо только по дебету
        Passive = 2,       // Пассивный - сальдо только по кредиту
        ActivePassive = 3  // Активно-пассивный - может иметь сальдо по дебету и кредиту
    }

    public class Account
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? FullCode { get; set; }

        // Тип счета
        public AccountType Type { get; set; }

        // Является ли счет синтетическим (счет первого порядка)
        public bool IsSynthetic { get; set; }

        // Новое поле для архивации
        public bool IsArchived { get; set; } = false;

        // Навигационные свойства для иерархии
        public int? ParentId { get; set; }
        public Account? Parent { get; set; }
        public ICollection<Account> Children { get; set; } = new List<Account>();

        // Проводки по дебету и кредиту
        public ICollection<Entry> DebitEntries { get; set; } = new List<Entry>();
        public ICollection<Entry> CreditEntries { get; set; } = new List<Entry>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }
}