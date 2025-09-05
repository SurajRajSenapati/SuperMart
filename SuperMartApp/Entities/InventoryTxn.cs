namespace SuperMartApp.Domain.Entities;

public enum InventoryTxnType { Grn = 1, Sale = 2, Return = 3, Adjust = 4, Transfer = 5 }

public class InventoryTxn
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int? BatchId { get; set; }
    public decimal Qty { get; set; } // +in, -out
    public InventoryTxnType TxnType { get; set; }
    public string? RefEntity { get; set; }  // "SO", "RETURN", "GRN"
    public int? RefId { get; set; }
    public decimal UnitCost { get; set; }   // for GRN/avg cost (0 for sale path initially)
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
