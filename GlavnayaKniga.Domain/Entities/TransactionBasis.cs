namespace GlavnayaKniga.Domain.Entities;

public class TransactionBasis
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;      // Наименование основания
    public string? Description { get; set; }               // Описание

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}