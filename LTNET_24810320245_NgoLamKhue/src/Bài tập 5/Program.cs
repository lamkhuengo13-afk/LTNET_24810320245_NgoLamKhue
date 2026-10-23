namespace BaiTap5_IoTMonitor;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new frmIoTMonitor());
    }
}
