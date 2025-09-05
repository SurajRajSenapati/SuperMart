using SuperMartApp.Domain.Entities;

namespace SuperMartApp.Models
{

    public class SalesOrderViewModel : SalesOrder
    {
        public List<SalesOrderLine2> Lines { get; set; } = new();
    }

    public class SalesOrderLine2 : SalesOrderLine
    {
        public string ProductName { get; set; }
    }

}
