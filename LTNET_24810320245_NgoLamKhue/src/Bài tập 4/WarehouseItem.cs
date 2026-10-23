namespace BaiTap4_WarehouseAnalytics;

public class WarehouseItem
{
    public string Warehouse { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue => Quantity * UnitPrice;
    public DateTime ReportDate { get; set; }
}
