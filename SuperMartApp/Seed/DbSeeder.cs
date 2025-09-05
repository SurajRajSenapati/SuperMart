using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Domain.Identity;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // For demo: EnsureCreated so it's ready-to-run without dotnet-ef
        await ctx.Database.MigrateAsync();

        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = new[] { "Admin","StoreManager","Cashier","InventoryClerk","Marketing" };
        string[] roles2 = new[] { "Admin", "StoreManager" };

        foreach (var r in roles)
            if (!await rm.RoleExistsAsync(r))
                await rm.CreateAsync(new IdentityRole(r));

        if (await um.FindByNameAsync("suraj@mailinator.com") is null)
        {
            var u = new AppUser { UserName = "suraj@mailinator.com", Email = "suraj@mailinator.com" };
            await um.CreateAsync(u, "Test@1234");
            await um.AddToRolesAsync(u, roles);
        }

        if (await um.FindByNameAsync("raj@mailinator.com") is null)
        {
            var u = new AppUser { UserName = "raj@mailinator.com", Email = "raj@mailinator.com" };
            await um.CreateAsync(u, "Test@1234");
            await um.AddToRolesAsync(u, roles2);
        }

        if (!ctx.Products.Any())
        {
            ctx.Products.AddRange(
                new Product{ Sku="MILK500", Barcode="8901001", Name="Milk 500ml", GstRate=5, BasePrice=30, Mrp=35, ReorderLevel=20, ExpiryRequired=true },
                new Product{ Sku="BREAD1", Barcode="8902001", Name="Bread 400g", GstRate=5, BasePrice=50, Mrp=60, ReorderLevel=15, ExpiryRequired=true },
                new Product{ Sku="SUGAR1", Barcode="8903001", Name="Sugar 1kg", GstRate=5, BasePrice=60, Mrp=65, ReorderLevel=30, ExpiryRequired = true },
                new Product { Sku = "RICE-W1", Barcode = "8904001", Name = "White Rice 1kg", GstRate = 5, BasePrice = 80, Mrp = 90, ReorderLevel = 25, ExpiryRequired = true },
                new Product { Sku = "RICE-B1", Barcode = "8904002", Name = "Basmati Rice 1kg", GstRate = 5, BasePrice = 100, Mrp = 120, ReorderLevel = 25, ExpiryRequired = true }
            );
            ctx.Customers.Add(new Customer{ Code="WALKIN", Name="Walk-in Customer" });
            await ctx.SaveChangesAsync();
        }

        if (!ctx.ProductBatches.Any())
        {
            var milk = ctx.Products.First(x => x.Sku == "MILK500");
            var bread = ctx.Products.First(x => x.Sku == "BREAD1");
            var sugar = ctx.Products.First(x => x.Sku == "SUGAR1");
            var rice_white = ctx.Products.First(x => x.Sku == "RICE-W1");
            var rice_basmati = ctx.Products.First(x => x.Sku == "RICE-B1");

            ctx.ProductBatches.AddRange(
                new ProductBatch{ ProductId = milk.Id, BatchNo="MLK01", MfgDate=DateTime.UtcNow.AddDays(-2), ExpiryDate=DateTime.UtcNow.AddDays(30), Cost=25, QtyOnHand=100 },
                new ProductBatch{ ProductId = bread.Id, BatchNo="BRD01", MfgDate=DateTime.UtcNow.AddDays(-1), ExpiryDate=DateTime.UtcNow.AddDays(60), Cost=40, QtyOnHand=150 },
                new ProductBatch{ ProductId = sugar.Id, BatchNo="SGR01", MfgDate=DateTime.UtcNow.AddDays(-30), ExpiryDate = DateTime.UtcNow.AddDays(90), Cost =55, QtyOnHand=80 },
                new ProductBatch { ProductId = rice_white.Id, BatchNo = "RICE01", MfgDate = DateTime.UtcNow.AddDays(-20), ExpiryDate = DateTime.UtcNow.AddDays(120), Cost = 70, QtyOnHand = 100 },
                new ProductBatch { ProductId = rice_basmati.Id, BatchNo = "RICE02", MfgDate = DateTime.UtcNow.AddDays(-30), ExpiryDate = DateTime.UtcNow.AddDays(180), Cost = 90, QtyOnHand = 100 }
            );
            await ctx.SaveChangesAsync();
        }

    }
}
