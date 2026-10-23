namespace BaiTap3_MDI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new frmSchoolMain());
    }
}
