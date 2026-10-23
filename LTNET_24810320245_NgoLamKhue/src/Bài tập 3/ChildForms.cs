namespace BaiTap3_MDI;

public class frmStudentList : SimpleListForm
{ public frmStudentList() : base("DANH SÁCH SINH VIÊN", ["Mã sinh viên", "Họ và tên", "Lớp", "Email"], [["SV001", "Nguyễn Minh Anh", "D19ANM1", "minhanh@example.com"], ["SV002", "Trần Hoàng Nam", "D19ANM1", "hoangnam@example.com"]]) { } }
public class frmCourseList : SimpleListForm
{ public frmCourseList() : base("DANH MỤC MÔN HỌC", ["Mã môn", "Tên môn học", "Số tín chỉ", "Bộ môn"], [["MH001", "Mạng máy tính", "3", "Mạng"], ["MH002", "An toàn hệ điều hành", "3", "An toàn thông tin"]]) { } }
public class frmLecturerList : SimpleListForm
{ public frmLecturerList() : base("DANH SÁCH GIẢNG VIÊN", ["Mã giảng viên", "Họ và tên", "Bộ môn", "Email"], [["GV001", "Nguyễn Văn Bình", "Mạng máy tính", "binh@example.com"], ["GV002", "Lê Thu Hà", "An toàn thông tin", "ha@example.com"]]) { } }
public class frmClassList : SimpleListForm
{ public frmClassList() : base("DANH MỤC LỚP HỌC", ["Mã lớp", "Tên lớp", "Khóa", "Cố vấn"], [["D19ANM1", "Quản trị An ninh mạng 1", "K19", "Nguyễn Văn Bình"], ["D19ANM2", "Quản trị An ninh mạng 2", "K19", "Lê Thu Hà"]]) { } }

public class SimpleListForm : Form
{
    public SimpleListForm(string title, string[] columns, string[][] rows)
    {
        Text = title; Width = 850; Height = 500; MinimumSize = new Size(650, 350); StartPosition = FormStartPosition.CenterParent;
        var header = new Label { Text = title, Dock = DockStyle.Top, Height = 64, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(18, 0, 0, 0), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(37, 73, 111), BackColor = Color.FromArgb(230, 239, 248) };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, BackgroundColor = Color.White, RowHeadersVisible = false };
        foreach (var column in columns) grid.Columns.Add(column, column);
        foreach (var row in rows) grid.Rows.Add(row);
        Controls.Add(grid); Controls.Add(header);
    }
}
