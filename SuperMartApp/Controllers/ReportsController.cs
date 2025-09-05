using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Web.Controllers;

[Authorize(Roles="StoreManager,Admin")]
public class ReportsController : Controller
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> ZReport(DateTime? date)
    {
        var model = new ZModel();
        try
        {
            var day = (date ?? DateTime.UtcNow).Date;
            var next = day.AddDays(1);

            var sales = await _db.SalesOrders
                .Where(o => o.CreatedAt >= day && o.CreatedAt < next)
                .ToListAsync();

            var pays = await _db.SalesPayments
                .Where(p => _db.SalesOrders.Where(o => o.CreatedAt >= day && o.CreatedAt < next).Select(o => o.Id).Contains(p.SalesOrderId))
                .ToListAsync();

            var byMethod = pays.GroupBy(p => p.Method).Select(g => new { Method = g.Key, Total = g.Sum(x => x.Amount) }).ToList();
            model = new ZModel
            {
                Date = day,
                Orders = sales.Count,
                Subtotal = sales.Sum(s => s.Subtotal),
                Tax = sales.Sum(s => s.Tax),
                Discount = sales.Sum(s => s.Discount),
                Total = sales.Sum(s => s.Total),
                Payments = byMethod.ToDictionary(x => x.Method.ToString(), x => x.Total)
            };
        }
        catch(Exception ex)
        {
        }
        return View(model);
    }

    public class ZModel
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int Orders { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, decimal> Payments { get; set; } = new();
    }
}
