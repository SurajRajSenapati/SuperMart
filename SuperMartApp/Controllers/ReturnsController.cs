using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Web.Controllers;

[Authorize(Roles="Cashier,StoreManager,Admin")]
public class ReturnsController : Controller
{
    private readonly AppDbContext _db;
    public ReturnsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Create(int salesOrderId)
    {
        var so = await _db.SalesOrders.Include(o=>o.Lines).FirstOrDefaultAsync(o=>o.Id==salesOrderId);
        if (so is null) return NotFound();
        return View(so);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int salesOrderId, [FromForm] int lineId, [FromForm] decimal qty, [FromForm] string reason)
    {
        var line = await _db.SalesOrderLines.FindAsync(lineId);
        if (line is null) 
            return NotFound();
        if (qty <= 0 || qty > line.Qty) 
            return BadRequest("Invalid qty");

        var ret = new Return { SalesOrderId = line.SalesOrderId, Reason = reason, TotalAmount = qty * line.UnitPrice };
        await _db.Returns.AddAsync(ret);
        await _db.SaveChangesAsync();

        // Restock to earliest batch
        var batch = await _db.ProductBatches
            .Where(b => b.ProductId == line.ProductId)
            .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue).FirstOrDefaultAsync();

        int? bid = batch?.Id;
        if (batch != null) batch.QtyOnHand += qty;

        _db.ReturnLines.Add(new ReturnLine {
            ReturnId = ret.Id,
            SalesOrderLineId = line.Id,
            ProductId = line.ProductId,
            BatchId = bid,
            Qty = qty,
            UnitPrice = line.UnitPrice,
            LineTotal = qty * line.UnitPrice
        });

        _db.InventoryTxns.Add(new InventoryTxn {
            ProductId = line.ProductId,
            BatchId = bid,
            Qty = qty,
            TxnType = InventoryTxnType.Return,
            RefEntity = "RETURN",
            RefId = ret.Id,
            UnitCost = 0
        });

        await _db.SaveChangesAsync();
        return RedirectToAction("Receipt", "Orders", new { id = salesOrderId });
    }
}
