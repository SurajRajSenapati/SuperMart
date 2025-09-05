using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Web.Controllers;

[Authorize(Roles="InventoryClerk,StoreManager,Admin")]
public class BatchesController : Controller
{
    private readonly AppDbContext _db;
    public BatchesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? productId)
    {
        var qry = _db.ProductBatches.AsQueryable();
        if (productId.HasValue) 
            qry = qry.Where(b => b.ProductId == productId.Value);
        
        var data = await qry.OrderBy(b=>b.ProductId).ThenBy(b=>b.ExpiryDate ?? DateTime.MaxValue).ToListAsync();
        
        ViewBag.Products = new SelectList(await _db.Products.OrderBy(p=>p.Name).ToListAsync(), "Id", "Name");
        
        return View(data);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = new SelectList(await _db.Products.OrderBy(p=>p.Name).ToListAsync(), "Id", "Name");
        return View(new BatchCreateVM());
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BatchCreateVM vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Products = new SelectList(await _db.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
                return View(vm);
            }

            var pb = await _db.ProductBatches.FirstOrDefaultAsync(x => x.ProductId == vm.ProductId && x.BatchNo == vm.BatchNo);

            if (vm.MfgDate.HasValue)
            {
                var dt = vm.MfgDate.Value;
                if (dt.Kind == DateTimeKind.Unspecified) dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
                vm.MfgDate = dt.ToUniversalTime();
            }
            if (vm.ExpiryDate.HasValue)
            {
                var dt = vm.ExpiryDate.Value;
                if (dt.Kind == DateTimeKind.Unspecified) dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
                vm.ExpiryDate = dt.ToUniversalTime();
            }

            if (pb is null)
            {
                pb = new ProductBatch
                {
                    ProductId = vm.ProductId,
                    BatchNo = vm.BatchNo,
                    MfgDate = vm.MfgDate,
                    ExpiryDate = vm.ExpiryDate,
                    Cost = vm.Cost,
                    QtyOnHand = 0
                };

                _db.ProductBatches.Add(pb);
                await _db.SaveChangesAsync();
            }

            pb.QtyOnHand += vm.Qty;
            var newInventoRyTxn = new InventoryTxn { 
                ProductId = vm.ProductId, 
                BatchId = pb.Id, 
                Qty = vm.Qty, 
                TxnType = InventoryTxnType.Grn, 
                RefEntity = "GRN", 
                RefId = pb.Id, 
                UnitCost = vm.Cost 
            };

            _db.InventoryTxns.Add(newInventoRyTxn);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
        }
        catch(Exception ex)
        {
            return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
        }
    }

    public class BatchCreateVM
    {
        public int ProductId { get; set; }
        public string BatchNo { get; set; } = string.Empty;
        public DateTime? MfgDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; } = DateTime.UtcNow;
        public decimal Cost { get; set; }
        public decimal Qty { get; set; }
    }
}
