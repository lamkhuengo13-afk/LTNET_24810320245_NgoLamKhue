namespace BaiTap2_EmployeeManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new frmEmployeeManager());
    }
}
