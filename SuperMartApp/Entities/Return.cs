namespace SuperMartApp.Domain.Entities;

public class Return
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ReturnLine> Lines { get; set; } = new();
}

public class ReturnLine
{
    public int Id { get; set; }
    public int ReturnId { get; set; }
    public int SalesOrderLineId { get; set; }
    public int ProductId { get; set; }
    public int? BatchId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
