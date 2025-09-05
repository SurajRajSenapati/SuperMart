namespace SuperMartApp.Domain.Entities
{
    public class SalesOrder
    {
        public int Id { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CashierId { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal PaidTotal { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<SalesOrderLine> Lines { get; set; } = new();
    }

    public class SalesOrderLine
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }
        public int? BatchId { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineDiscount { get; set; }
        public decimal Tax { get; set; }
        public decimal LineTotal { get; set; }
    }

}

