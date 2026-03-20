using System;

namespace GlavnayaKniga.Domain.Entities;

public class Entry
{
    public int Id { get; set; }
    public DateTime Date { get; set; }                     // Дата проводки

    // Счета
    public int DebitAccountId { get; set; }
    public Account DebitAccount { get; set; } = null!;

    public int CreditAccountId { get; set; }
    public Account CreditAccount { get; set; } = null!;

    public decimal Amount { get; set; }                    // Сумма

    // Основание
    public int BasisId { get; set; }
    public TransactionBasis Basis { get; set; } = null!;

    public string? Note { get; set; }                      // Примечание

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}