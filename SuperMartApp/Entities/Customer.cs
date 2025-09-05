namespace SuperMartApp.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int LoyaltyPoints { get; set; }
    public bool ConsentSms { get; set; }
    public bool ConsentEmail { get; set; }
}
