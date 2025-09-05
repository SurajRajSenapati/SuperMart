using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Infrastructure.Data;
using SuperMartApp.Models;
using SuperMartApp.Services;

namespace SuperMartApp.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;


    public async Task<IActionResult> Receipt(int id)
    {
        try
        {
            var salesOrder = await _db.SalesOrders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (salesOrder is null)
                return NotFound();

            var productIds = salesOrder.Lines.Any() ? salesOrder.Lines.Select(x => x.ProductId).ToList() : new List<int>();

            var products = await _db.Products.Where(x => productIds.Contains(x.Id)).ToListAsync();
         
            var result = DataMapper.MapSalesOrderViewModel(salesOrder, products);


            var pays = await _db.SalesPayments
                .Where(p => p.SalesOrderId == id)
                .ToListAsync();

            ViewBag.Payments = pays;

            return View(result);
        }
        catch (Exception ex)
        {
            // You could log the exception here (Serilog, NLog, ILogger, etc.)
            // _logger.LogError(ex, "Error fetching receipt for SalesOrder Id {Id}", id);

            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
