using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Domain.Identity;
using SuperMartApp.Infrastructure.Data;

namespace SuperMartApp.Web.Controllers
{
    [Authorize(Roles = "Cashier,StoreManager,Admin")]
    public class POSController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _um;
        public POSController(AppDbContext db, UserManager<AppUser> um) { _db = db; _um = um; }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Scan(string barcode)
        {
            var p = await _db.Products.FirstOrDefaultAsync(x => x.Barcode == barcode && x.IsActive);
            if (p is null) return NotFound("Item not found");
            var price = p.BasePrice * (1 + (p.GstRate / 100m));
            return Json(new { productId = p.Id, name = p.Name, price });
        }

        [HttpGet]
        public async Task<IActionResult> FindCustomer(string q)
        {
            q = (q ?? "").Trim();
            var qry = _db.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                qry = qry.Where(c => c.Code.Contains(q) || c.Name.Contains(q) || (c.Phone ?? "").Contains(q) || (c.Email ?? "").Contains(q));
            }
            var results = await qry
                .OrderBy(c => c.Name)
                .Take(15)
                .Select(c => new { c.Id, c.Code, c.Name, c.Phone, c.Email })
                .ToListAsync();
            return Json(results);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            if (dto is null || dto.Lines.Count == 0) 
                return BadRequest("Empty cart");
            if (dto.Customer is null)
                return BadRequest("Customer Details is Empty");

            // Resolve or create customer
            int? customerId = null;
            if (dto.Customer != null)
            {
                Customer? c = null;
                if (dto.Customer.Id.HasValue)
                {
                    c = await _db.Customers.FindAsync(dto.Customer.Id.Value);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(dto.Customer.Phone))
                        c = await _db.Customers.FirstOrDefaultAsync(x => x.Phone == dto.Customer.Phone);
                    if (c == null && !string.IsNullOrWhiteSpace(dto.Customer.Code))
                        c = await _db.Customers.FirstOrDefaultAsync(x => x.Code == dto.Customer.Code);
                    if (c == null && !string.IsNullOrWhiteSpace(dto.Customer.Email))
                        c = await _db.Customers.FirstOrDefaultAsync(x => x.Email == dto.Customer.Email);
                    if (c == null)
                    {
                        var code = string.IsNullOrWhiteSpace(dto.Customer.Code)
                            ? "CUST" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
                            : dto.Customer.Code!.Trim();
                        c = new Customer
                        {
                            Code = code,
                            Name = string.IsNullOrWhiteSpace(dto.Customer.Name) ? "Walk-in" : dto.Customer.Name!.Trim(),
                            Phone = dto.Customer.Phone?.Trim(),
                            Email = dto.Customer.Email?.Trim(),
                            ConsentSms = true,
                            ConsentEmail = true
                        };
                        _db.Customers.Add(c);
                        await _db.SaveChangesAsync();
                    }
                }
                if (c != null) 
                    customerId = c.Id;
            }

            var user = await _um.GetUserAsync(User);
            decimal subtotal = 0, tax = 0, discount = 0;
            var lines = new List<SalesOrderLine>();

            foreach (var l in dto.Lines)
            {
                var p = await _db.Products.FindAsync(l.ProductId);
                if (p is null) return BadRequest($"Product {l.ProductId} not found");
                var lineSubtotal = l.Qty * p.BasePrice;
                var lineTax = lineSubtotal * (p.GstRate / 100m);
                subtotal += lineSubtotal; tax += lineTax;
                lines.Add(new SalesOrderLine
                {
                    ProductId = p.Id,
                    Qty = l.Qty,
                    UnitPrice = p.BasePrice + (l.Qty == 0 ? 0 : lineTax / l.Qty),
                    LineDiscount = 0,
                    Tax = lineTax,
                    LineTotal = lineSubtotal + lineTax
                });
            }
            var total = subtotal - discount + tax;
            var paid = dto.Payments.Sum(p => p.Amount);
            if (paid < total) return BadRequest("Paid amount less than total");

            var so = new SalesOrder
            {
                OrderNo = $"SO{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                CustomerId = customerId,
                CashierId = user!.Id,
                Subtotal = subtotal,
                Discount = discount,
                Tax = tax,
                Total = total,
                PaidTotal = paid,
                Lines = lines
            };
            await _db.SalesOrders.AddAsync(so);
            await _db.SaveChangesAsync();

            foreach (var pay in dto.Payments)
                _db.SalesPayments.Add(new SalesPayment { SalesOrderId = so.Id, Method = pay.Method, Amount = pay.Amount });

            // FEFO reduce stock
            foreach (var line in so.Lines)
            {
                decimal remain = line.Qty;
                var batches = await _db.ProductBatches
                    .Where(b => b.ProductId == line.ProductId && b.QtyOnHand > 0)
                    .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue).ThenBy(b => b.Id)
                    .ToListAsync();
                foreach (var b in batches)
                {
                    if (remain <= 0) break;
                    var take = Math.Min(remain, b.QtyOnHand);
                    b.QtyOnHand -= take; remain -= take;
                    _db.InventoryTxns.Add(new InventoryTxn
                    {
                        ProductId = line.ProductId,
                        BatchId = b.Id,
                        Qty = -take,
                        TxnType = InventoryTxnType.Sale,
                        RefEntity = "SO",
                        RefId = so.Id,
                        UnitCost = b.Cost
                    });
                }
                if (remain > 0) return BadRequest("Insufficient stock");
            }

            await _db.SaveChangesAsync();
            return Ok(new { so.Id, so.OrderNo, so.Total, so.CustomerId });
        }




        public class CheckoutDto
        {
            public CustomerDto? Customer { get; set; } = new();
            public List<CartLine> Lines { get; set; } = new();
            public List<PaymentDto> Payments { get; set; } = new();
        }


        public class CustomerDto
        {
            public int? Id { get; set; }
            public string? Code { get; set; }
            public string? Name { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
        }


        public class CartLine 
        { 
            public int ProductId { get; set; } 
            public decimal Qty { get; set; } 
            public decimal UnitPrice { get; set; } 
        }


        public class PaymentDto 
        {
            public PaymentMethod Method { get; set; } 
            public decimal Amount { get; set; } 
        }


    }
}
