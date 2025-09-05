using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Domain.Identity;
using SuperMartApp.Web.Entities;

namespace SuperMartApp.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<InventoryTxn> InventoryTxns => Set<InventoryTxn>();
    public DbSet<ProductBatch> ProductBatches => Set<ProductBatch>();
    public DbSet<SalesPayment> SalesPayments => Set<SalesPayment>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnLine> ReturnLines => Set<ReturnLine>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Product>().HasIndex(x => x.Barcode).IsUnique();
        b.Entity<ProductBatch>().HasIndex(x => new { x.ProductId, x.BatchNo }).IsUnique();
        b.Entity<SalesOrder>().HasIndex(x => x.OrderNo).IsUnique();
        b.Entity<SalesOrder>().HasIndex(x => x.OrderNo).IsUnique();
        b.Entity<ProductBatch>().HasIndex(x => new { x.ProductId, x.BatchNo }).IsUnique();
        b.Entity<SalesOrder>().HasIndex(x => x.OrderNo).IsUnique();
        b.Entity<RefreshToken>().HasIndex(x => x.Token).IsUnique();

        // decimal precision defaults
        foreach (var p in b.Model.GetEntityTypes()
                 .SelectMany(t => t.GetProperties())
                 .Where(p => p.ClrType == typeof(decimal)))
        {
            p.SetPrecision(18);
            p.SetScale(2);
        }
    }
}
