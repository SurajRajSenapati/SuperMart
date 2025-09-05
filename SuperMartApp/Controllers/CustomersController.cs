using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Web.Controllers;

[Authorize(Roles = "Admin,StoreManager,Marketing")]
public class CustomersController : Controller
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Name.Contains(q) || c.Code.Contains(q) || (c.Phone ?? "").Contains(q));
        var list = await query.OrderBy(c => c.Name).ToListAsync();
        return View(list);
    }

    public IActionResult Create() => View(new Customer());

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer vm)
    {
        if (!ModelState.IsValid) return View(vm);
        _db.Add(vm); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        return c is null ? NotFound() : View(c);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer vm)
    {
        if (!ModelState.IsValid) return View(vm);
        _db.Update(vm); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        _db.Remove(c); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
