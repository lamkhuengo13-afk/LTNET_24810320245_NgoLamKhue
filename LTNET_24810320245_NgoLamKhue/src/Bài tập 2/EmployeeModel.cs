namespace BaiTap2_EmployeeManager;

public sealed class EmployeeModel
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string ImagePath { get; set; } = string.Empty;
}
