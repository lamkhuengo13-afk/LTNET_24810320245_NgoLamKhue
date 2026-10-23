namespace BaiTap4_WarehouseAnalytics;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new frmWarehouseAnalytics());
    }
}
