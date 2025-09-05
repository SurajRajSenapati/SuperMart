using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Web.Controllers;

[Authorize(Roles = "Admin,StoreManager,InventoryClerk")]
public class ProductsController : Controller
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        var qry = _db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            qry = qry.Where(p => p.Name.Contains(q) || p.Barcode == q || p.Sku == q);
        var list = await qry.OrderBy(p => p.Name).ToListAsync();
        return View(list);
    }

    public IActionResult Create() => View(new Product());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product vm)
    {
        if (!ModelState.IsValid) return View(vm);
        _db.Add(vm); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Products.FindAsync(id);
        return p is null ? NotFound() : View(p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product vm)
    {
        if (!ModelState.IsValid) return View(vm);
        _db.Update(vm); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null) return NotFound();
        _db.Remove(p); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
