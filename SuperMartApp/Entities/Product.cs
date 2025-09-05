namespace SuperMartApp.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public decimal GstRate { get; set; }   // e.g., 5/12/18
    public decimal BasePrice { get; set; } // pre-tax
    public decimal Mrp { get; set; }
    public int ReorderLevel { get; set; }
    public bool ExpiryRequired { get; set; }
    public bool IsActive { get; set; } = true;
}
