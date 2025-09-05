using SuperMartApp.Domain.Entities;
using SuperMartApp.Domain.Identity;
using SuperMartApp.Models;

namespace SuperMartApp.Services
{

    public class DataMapper 
    {
        public static TTarget MapDataTo<TSource, TTarget>(TSource source) where TTarget : new()
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            TTarget target = new TTarget();
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sProp in sourceProps)
            {
                var tProp = targetProps.FirstOrDefault(p => p.Name == sProp.Name && p.PropertyType == sProp.PropertyType);
                if (tProp != null && tProp.CanWrite)
                {
                    var value = sProp.GetValue(source);
                    tProp.SetValue(target, value);
                }
            }

            return target;
        }


        public static SalesOrderViewModel MapSalesOrderViewModel(SalesOrder source, List<Product> products)
        {
            if (source == null) 
                throw new ArgumentNullException(nameof(source));

            var target = new SalesOrderViewModel
            {
                Id = source.Id,
                OrderNo = source.OrderNo,
                CustomerId = source.CustomerId,
                CashierId = source.CashierId,
                Subtotal = source.Subtotal,
                Discount = source.Discount,
                Tax = source.Tax,
                Total = source.Total,
                PaidTotal = source.PaidTotal,
                CreatedAt = source.CreatedAt,
                // Handle Lines separately
                Lines = source.Lines.Select(line => new SalesOrderLine2
                {
                    Id = line.Id,
                    SalesOrderId = line.SalesOrderId,
                    ProductId = line.ProductId,
                    BatchId = line.BatchId,
                    Qty = line.Qty,
                    UnitPrice = line.UnitPrice,
                    LineDiscount = line.LineDiscount,
                    Tax = line.Tax,
                    LineTotal = line.LineTotal,

                    // Extra field in ViewModel
                    ProductName = products.Any(x => x.Id == line.ProductId) ? products.FirstOrDefault(x => x.Id == line.ProductId).Name : string.Empty
                }).ToList()
            };

            return target;
        }

    }



    

}
