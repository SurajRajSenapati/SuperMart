using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Infrastructure.Data;
using SuperMartApp.Services;

namespace SuperMartApp.Web.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly AppDbContext _db;
    public SalesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.SalesOrders.OrderByDescending(o => o.Id).Take(200).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var salesOrder = await _db.SalesOrders
               .Include(o => o.Lines)
               .FirstOrDefaultAsync(o => o.Id == id);

        if (salesOrder is null)
            return NotFound();


        var productIds = salesOrder.Lines.Any() ? salesOrder.Lines.Select(x => x.ProductId).ToList() : new List<int>();
        var products = await _db.Products.Where(x => productIds.Contains(x.Id)).ToListAsync();

        var result = DataMapper.MapSalesOrderViewModel(salesOrder, products);


        return View(result);
    }
}
