using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Web.Controllers;

[Authorize(Roles="InventoryClerk,StoreManager,Admin")]
public class InventoryController : Controller
{
    private readonly AppDbContext _db;
    public InventoryController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Ledger(string? q, DateTime? from, DateTime? to)
    {
        var qry = _db.InventoryTxns.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            qry = from t in qry
                  join p in _db.Products on t.ProductId equals p.Id
                  where p.Name.Contains(q) || p.Barcode == q || p.Sku == q
                  select t;
        }
        if (from.HasValue) qry = qry.Where(t => t.OccurredAt >= from.Value);
        if (to.HasValue) qry = qry.Where(t => t.OccurredAt < to.Value.AddDays(1));
        var data = await qry.OrderByDescending(t => t.OccurredAt).Take(500).ToListAsync();
        ViewBag.Query = q; ViewBag.From = from?.ToString("yyyy-MM-dd"); ViewBag.To = to?.ToString("yyyy-MM-dd");
        return View(data);
    }
}
