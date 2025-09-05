namespace SuperMartApp.Domain.Entities;

public enum PaymentMethod { Cash = 1, Card = 2, UPI = 3 }

public class SalesPayment
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
}
