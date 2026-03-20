using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Domain.Entities
{
    /// <summary>
    /// Соответствие расчетного счета организации и субсчета в плане счетов
    /// </summary>
    public class BankAccount
    {
        public int Id { get; set; }

        /// <summary>
        /// Расчетный счет организации (например, 407028*)
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Наименование банка
        /// </summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// БИК банка
        /// </summary>
        public string BIK { get; set; } = string.Empty;

        /// <summary>
        /// Корреспондентский счет банка
        /// </summary>
        public string CorrespondentAccount { get; set; } = string.Empty;

        /// <summary>
        /// Субсчет в плане счетов (например, 51.4)
        /// </summary>
        public int SubaccountId { get; set; }
        public Account Subaccount { get; set; } = null!;

        /// <summary>
        /// Валюта счета (по умолчанию RUB)
        /// </summary>
        public string Currency { get; set; } = "RUB";

        /// <summary>
        /// Счет активен
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Дата открытия счета
        /// </summary>
        public DateTime? OpenDate { get; set; }

        /// <summary>
        /// Дата закрытия счета (если счет закрыт)
        /// </summary>
        public DateTime? CloseDate { get; set; }

        /// <summary>
        /// Причина закрытия счета
        /// </summary>
        public string? CloseReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}