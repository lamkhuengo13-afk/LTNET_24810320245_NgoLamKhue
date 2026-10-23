namespace BaiTap3_MDI;

public class frmSchoolMain : Form
{
    private readonly ToolStripStatusLabel _userStatus = new() { Text = "Cán bộ quản lý: Quản trị viên" };
    private readonly ToolStripStatusLabel _hintStatus = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Sẵn sàng" };
    private readonly ToolStripProgressBar _progress = new() { Minimum = 0, Maximum = 100, Value = 0, Width = 140 };

    public frmSchoolMain()
    {
        Text = "HỆ THỐNG QUẢN LÝ TRƯỜNG HỌC & ĐÀO TẠO";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1200; Height = 760; MinimumSize = new Size(900, 600);
        IsMdiContainer = true;
        BackColor = Color.FromArgb(238, 243, 248);
        MainMenuStrip = BuildMenu();
        Controls.Add(MainMenuStrip);
        var status = new StatusStrip();
        status.Items.Add(_userStatus); status.Items.Add(_hintStatus); status.Items.Add(new ToolStripStatusLabel("Tiến trình:")); status.Items.Add(_progress);
        Controls.Add(status);
        var welcome = new Label { Dock = DockStyle.Fill, Text = "HỆ THỐNG QUẢN LÝ TRƯỜNG HỌC\n\nChọn chức năng trong menu Quản lý để bắt đầu.", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(46, 79, 115) };
        Controls.Add(welcome); welcome.SendToBack();
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();
        var system = new ToolStripMenuItem("Hệ thống");
        system.DropDownItems.Add("Đăng xuất", null, (_, _) => Logout());
        system.DropDownItems.Add("Đổi mật khẩu", null, (_, _) => ChangePassword());
        system.DropDownItems.Add(new ToolStripSeparator());
        system.DropDownItems.Add("Thoát", null, (_, _) => Close());

        var manage = new ToolStripMenuItem("Quản lý");
        manage.DropDownItems.Add("Danh sách Sinh viên", null, (_, _) => OpenChild<frmStudentList>());
        manage.DropDownItems.Add("Danh sách Giảng viên", null, (_, _) => OpenChild<frmLecturerList>());
        manage.DropDownItems.Add("Danh mục Lớp học", null, (_, _) => OpenChild<frmClassList>());
        manage.DropDownItems.Add("Danh mục Môn học", null, (_, _) => OpenChild<frmCourseList>());

        var windows = new ToolStripMenuItem("Cửa sổ");
        windows.DropDownItems.Add("Sắp xếp Xếp tầng (Cascade)", null, (_, _) => LayoutChildren(MdiLayout.Cascade));
        windows.DropDownItems.Add("Sắp xếp Ngang (Tile Horizontal)", null, (_, _) => LayoutChildren(MdiLayout.TileHorizontal));
        windows.DropDownItems.Add("Sắp xếp Dọc (Tile Vertical)", null, (_, _) => LayoutChildren(MdiLayout.TileVertical));
        windows.DropDownItems.Add(new ToolStripSeparator());
        windows.DropDownItems.Add("Đóng tất cả", null, (_, _) => CloseAllChildren());
        menu.Items.AddRange([system, manage, windows]);
        return menu;
    }

    private void OpenChild<T>() where T : Form, new()
    {
        foreach (Form child in MdiChildren)
        {
            if (child is T) { child.Activate(); _hintStatus.Text = $"Đang mở: {child.Text}"; return; }
        }
        var form = new T { MdiParent = this, WindowState = FormWindowState.Maximized };
        form.FormClosed += (_, _) => { _progress.Value = Math.Min(100, MdiChildren.Length * 20); _hintStatus.Text = "Sẵn sàng"; };
        form.Show(); _progress.Value = Math.Min(100, MdiChildren.Length * 20); _hintStatus.Text = $"Đã mở: {form.Text}";
    }

    private void LayoutChildren(MdiLayout layout)
    {
        if (MdiChildren.Length == 0) { _hintStatus.Text = "Chưa có cửa sổ con để sắp xếp."; return; }
        LayoutMdi(layout); _hintStatus.Text = "Đã sắp xếp cửa sổ con.";
    }

    private void CloseAllChildren()
    {
        foreach (Form child in MdiChildren.ToArray()) child.Close();
        _progress.Value = 0; _hintStatus.Text = "Đã đóng tất cả cửa sổ con.";
    }

    private void Logout()
    {
        if (MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        { CloseAllChildren(); _hintStatus.Text = "Đã đăng xuất (mô phỏng)."; MessageBox.Show("Chức năng đăng xuất hiện được mô phỏng trong bài tập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    }

    private void ChangePassword() => MessageBox.Show("Chức năng đổi mật khẩu là giao diện minh họa; bài tập chưa sử dụng hệ thống tài khoản.", "Đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
