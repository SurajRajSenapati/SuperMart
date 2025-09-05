namespace SuperMartApp.Domain.Entities;

public class ProductBatch
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? MfgDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Cost { get; set; }
    public decimal QtyOnHand { get; set; }
}
