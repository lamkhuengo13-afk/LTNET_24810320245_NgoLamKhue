using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace BaiTap2_EmployeeManager;

public sealed class frmEmployeeManager : Form
{
    private readonly BindingList<EmployeeModel> _employees = new();
    private readonly BindingSource _source = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtId = new();
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtDepartment = new();
    private readonly NumericUpDown _numSalary = new();
    private readonly TextBox _txtImage = new();
    private readonly PictureBox _picture = new();
    private readonly TextBox _txtSearch = new();
    private readonly Label _lblCount = new();
    private readonly Label _lblTotal = new();
    private readonly ToolStripStatusLabel _statusText = new();
    private readonly ToolStripStatusLabel _clockText = new();
    private readonly System.Windows.Forms.Timer _clock = new();
    private readonly ErrorProvider _errors = new();
    private int _nextId = 1;

    public frmEmployeeManager()
    {
        Text = "Quản lý hồ sơ nhân sự & thông tin lương";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 650);
        Size = new Size(1180, 760);
        Font = new Font("Segoe UI", 9F);
        BuildInterface();
        ConfigureGrid();
        _source.DataSource = _employees;
        _grid.DataSource = _source;
        _grid.SelectionChanged += (_, _) => LoadSelectedEmployee();
        _clock.Interval = 1000;
        _clock.Tick += (_, _) => _clockText.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        _clock.Start();
        UpdateSummary();
        _statusText.Text = "Sẵn sàng";
        FormClosed += (_, _) => { _clock.Stop(); _errors.Dispose(); };
    }

    private void BuildInterface()
    {
        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("Tệp");
        fileMenu.DropDownItems.Add("Mở dữ liệu CSV...", null, (_, _) => LoadCsv());
        fileMenu.DropDownItems.Add("Lưu dữ liệu CSV...", null, (_, _) => SaveCsv());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Thoát", null, (_, _) => Close());
        var helpMenu = new ToolStripMenuItem("Trợ giúp");
        helpMenu.DropDownItems.Add("Thông tin", null, (_, _) => MessageBox.Show(this,
            "Bài tập 2 - Quản lý hồ sơ nhân sự & thông tin lương\nWinForms .NET 10", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information));
        menu.Items.AddRange([fileMenu, helpMenu]);
        MainMenuStrip = menu;
        Controls.Add(menu);

        var tool = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tool.Items.Add(new ToolStripButton("Mở CSV", null, (_, _) => LoadCsv()));
        tool.Items.Add(new ToolStripButton("Lưu CSV", null, (_, _) => SaveCsv()));
        tool.Items.Add(new ToolStripSeparator());
        tool.Items.Add(new ToolStripButton("Thêm nhân viên", null, (_, _) => AddEmployee()));
        tool.Items.Add(new ToolStripButton("Xóa nhân viên", null, (_, _) => DeleteEmployee()));
        Controls.Add(tool);

        var status = new StatusStrip();
        status.Items.Add(_statusText);
        status.Items.Add(new ToolStripStatusLabel { Spring = true });
        status.Items.Add(_clockText);
        Controls.Add(status);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(10) };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        Controls.Add(main);
        main.BringToFront();
        menu.BringToFront();
        tool.BringToFront();
        status.BringToFront();

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        var searchPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        searchPanel.Controls.Add(new Label { Text = "Tìm kiếm:", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        _txtSearch.Width = 260;
        _txtSearch.PlaceholderText = "Mã, họ tên hoặc phòng ban...";
        _txtSearch.TextChanged += (_, _) => ApplySearch();
        searchPanel.Controls.Add(_txtSearch);
        var btnClearSearch = new Button { Text = "Xóa tìm kiếm", AutoSize = true };
        btnClearSearch.Click += (_, _) => _txtSearch.Clear();
        searchPanel.Controls.Add(btnClearSearch);
        left.Controls.Add(searchPanel, 0, 0);
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        left.Controls.Add(_grid, 0, 1);
        var summary = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(4, 12, 0, 0) };
        _lblCount.AutoSize = true;
        _lblCount.Font = new Font(Font, FontStyle.Bold);
        _lblTotal.AutoSize = true;
        _lblTotal.Margin = new Padding(25, 0, 0, 0);
        _lblTotal.Font = new Font(Font, FontStyle.Bold);
        summary.Controls.Add(_lblCount);
        summary.Controls.Add(_lblTotal);
        left.Controls.Add(summary, 0, 2);
        main.Controls.Add(left, 0, 0);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7, Padding = new Padding(14, 0, 0, 0) };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 185));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        var heading = new Label { Text = "THÔNG TIN NHÂN VIÊN", Dock = DockStyle.Fill, Font = new Font(Font.FontFamily, 11, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        right.Controls.Add(heading, 0, 0); right.SetColumnSpan(heading, 2);
        AddField(right, "Mã nhân viên:", _txtId, 1); _txtId.ReadOnly = true;
        AddField(right, "Họ và tên:", _txtName, 2);
        AddField(right, "Phòng ban:", _txtDepartment, 3);
        _numSalary.Minimum = 0; _numSalary.Maximum = 1000000000; _numSalary.DecimalPlaces = 0; _numSalary.ThousandsSeparator = true; _numSalary.Dock = DockStyle.Fill;
        AddField(right, "Lương (VNĐ):", _numSalary, 4);
        var imagePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        _picture.Size = new Size(180, 125); _picture.SizeMode = PictureBoxSizeMode.Zoom; _picture.BorderStyle = BorderStyle.FixedSingle;
        _txtImage.ReadOnly = true; _txtImage.Width = 205; _txtImage.PlaceholderText = "Chưa chọn ảnh";
        var chooseImage = new Button { Text = "Chọn ảnh nhân viên...", AutoSize = true };
        chooseImage.Click += (_, _) => ChooseImage();
        imagePanel.Controls.Add(_picture); imagePanel.Controls.Add(_txtImage); imagePanel.Controls.Add(chooseImage);
        right.Controls.Add(new Label { Text = "Ảnh:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 0) }, 0, 5);
        right.Controls.Add(imagePanel, 1, 5);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(0, 8, 0, 0) };
        var add = new Button { Text = "Thêm mới", Width = 95, Height = 32 };
        var update = new Button { Text = "Cập nhật", Width = 95, Height = 32 };
        var delete = new Button { Text = "Xóa", Width = 75, Height = 32 };
        var clear = new Button { Text = "Làm mới", Width = 85, Height = 32 };
        add.Click += (_, _) => AddEmployee(); update.Click += (_, _) => UpdateEmployee(); delete.Click += (_, _) => DeleteEmployee(); clear.Click += (_, _) => ClearForm();
        buttons.Controls.AddRange([add, update, delete, clear]);
        right.Controls.Add(buttons, 0, 6); right.SetColumnSpan(buttons, 2);
        main.Controls.Add(right, 1, 0);
        _errors.ContainerControl = this;
    }

    private static void AddField(TableLayoutPanel panel, string label, Control input, int row)
    {
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0) }, 0, row);
        input.Dock = DockStyle.Fill;
        panel.Controls.Add(input, 1, row);
    }

    private void ConfigureGrid()
    {
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeModel.EmployeeId), HeaderText = "Mã NV", FillWeight = 65 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeModel.FullName), HeaderText = "Họ và tên", FillWeight = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeModel.Department), HeaderText = "Phòng ban", FillWeight = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeModel.Salary), HeaderText = "Lương (VNĐ)", FillWeight = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
    }

    private void AddEmployee()
    {
        if (!ValidateForm()) return;
        var employee = new EmployeeModel { EmployeeId = $"NV{_nextId++:000}", FullName = _txtName.Text.Trim(), Department = _txtDepartment.Text.Trim(), Salary = _numSalary.Value, ImagePath = _txtImage.Text };
        _employees.Add(employee);
        _source.ResetBindings(false);
        SelectEmployee(employee);
        UpdateSummary();
        _statusText.Text = $"Đã thêm nhân viên {employee.EmployeeId}";
    }

    private void UpdateEmployee()
    {
        var employee = SelectedEmployee();
        if (employee is null) { MessageBox.Show(this, "Vui lòng chọn nhân viên cần cập nhật.", "Chưa chọn nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        if (!ValidateForm()) return;
        employee.FullName = _txtName.Text.Trim(); employee.Department = _txtDepartment.Text.Trim(); employee.Salary = _numSalary.Value; employee.ImagePath = _txtImage.Text;
        _source.ResetBindings(false); UpdateSummary(); SelectEmployee(employee);
        _statusText.Text = $"Đã cập nhật {employee.EmployeeId}";
    }

    private void DeleteEmployee()
    {
        var employee = SelectedEmployee();
        if (employee is null) { MessageBox.Show(this, "Vui lòng chọn nhân viên cần xóa.", "Chưa chọn nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        if (MessageBox.Show(this, $"Bạn có chắc muốn xóa nhân viên {employee.FullName}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        _employees.Remove(employee); ClearForm(); UpdateSummary(); _statusText.Text = $"Đã xóa {employee.EmployeeId}";
    }

    private bool ValidateForm()
    {
        _errors.Clear(); bool valid = true;
        if (string.IsNullOrWhiteSpace(_txtName.Text)) { _errors.SetError(_txtName, "Họ và tên không được để trống."); valid = false; }
        if (string.IsNullOrWhiteSpace(_txtDepartment.Text)) { _errors.SetError(_txtDepartment, "Vui lòng nhập phòng ban."); valid = false; }
        if (_numSalary.Value <= 0) { _errors.SetError(_numSalary, "Lương phải lớn hơn 0."); valid = false; }
        if (!valid) _statusText.Text = "Vui lòng kiểm tra các trường bắt buộc.";
        return valid;
    }

    private EmployeeModel? SelectedEmployee() => _grid.CurrentRow?.DataBoundItem as EmployeeModel;
    private void SelectEmployee(EmployeeModel employee)
    {
        for (int i = 0; i < _grid.Rows.Count; i++)
            if (_grid.Rows[i].DataBoundItem is EmployeeModel row && row.EmployeeId == employee.EmployeeId) { _grid.CurrentCell = _grid.Rows[i].Cells[0]; _grid.Rows[i].Selected = true; break; }
        ShowEmployee(employee);
    }
    private void LoadSelectedEmployee() { if (SelectedEmployee() is { } employee) ShowEmployee(employee); }
    private void ShowEmployee(EmployeeModel employee)
    {
        _txtId.Text = employee.EmployeeId; _txtName.Text = employee.FullName; _txtDepartment.Text = employee.Department; _numSalary.Value = Math.Min(_numSalary.Maximum, Math.Max(_numSalary.Minimum, employee.Salary)); _txtImage.Text = employee.ImagePath; LoadPicture(employee.ImagePath);
    }
    private void ClearForm()
    {
        _grid.ClearSelection(); _txtId.Clear(); _txtName.Clear(); _txtDepartment.Clear(); _numSalary.Value = 0; _txtImage.Clear(); _picture.Image = null; _errors.Clear(); _txtName.Focus();
    }
    private void ChooseImage()
    {
        using var dialog = new OpenFileDialog { Title = "Chọn ảnh nhân viên", Filter = "Tệp ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Tất cả tệp (*.*)|*.*", Multiselect = false };
        if (dialog.ShowDialog(this) == DialogResult.OK) { _txtImage.Text = dialog.FileName; LoadPicture(dialog.FileName); }
    }
    private void LoadPicture(string path)
    {
        Image? image = null;
        try { if (File.Exists(path)) { using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); using var temp = Image.FromStream(stream); image = new Bitmap(temp); } }
        catch (Exception ex) when (ex is IOException or ArgumentException or OutOfMemoryException) { _statusText.Text = "Không thể tải ảnh: " + ex.Message; }
        var old = _picture.Image; _picture.Image = image; old?.Dispose();
    }

    private void ApplySearch()
    {
        var keyword = _txtSearch.Text.Trim();
        _source.DataSource = string.IsNullOrEmpty(keyword)
            ? _employees
            : new BindingList<EmployeeModel>(_employees.Where(e => e.EmployeeId.Contains(keyword, StringComparison.OrdinalIgnoreCase) || e.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) || e.Department.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList());
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var visible = _source.List.Cast<object>().OfType<EmployeeModel>().ToList();
        _lblCount.Text = $"Số nhân viên: {visible.Count}";
        _lblTotal.Text = $"Tổng lương: {visible.Sum(e => e.Salary).ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} VNĐ";
    }

    private void SaveCsv()
    {
        using var dialog = new SaveFileDialog { Title = "Lưu danh sách nhân viên", Filter = "CSV UTF-8 (*.csv)|*.csv", DefaultExt = "csv", AddExtension = true, FileName = "DanhSachNhanVien.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true));
            writer.WriteLine("EmployeeId,FullName,Department,Salary,ImagePath");
            foreach (var e in _employees) writer.WriteLine(string.Join(',', new[] { e.EmployeeId, e.FullName, e.Department, e.Salary.ToString(CultureInfo.InvariantCulture), e.ImagePath }.Select(EscapeCsv)));
            _statusText.Text = $"Đã lưu {_employees.Count} nhân viên vào CSV";
            MessageBox.Show(this, "Lưu dữ liệu CSV thành công.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(this, "Không thể lưu file:\n" + ex.Message, "Lỗi lưu file", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void LoadCsv()
    {
        using var dialog = new OpenFileDialog { Title = "Mở danh sách nhân viên", Filter = "CSV (*.csv)|*.csv|Tất cả tệp (*.*)|*.*", Multiselect = false };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var lines = File.ReadAllLines(dialog.FileName, Encoding.UTF8);
            var imported = new List<EmployeeModel>();
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = ParseCsvLine(line);
                if (fields.Count < 5) throw new FormatException("Một dòng CSV không đủ 5 cột.");
                if (!decimal.TryParse(fields[3], NumberStyles.Number, CultureInfo.InvariantCulture, out var salary)) throw new FormatException($"Lương không hợp lệ ở nhân viên {fields[0]}.");
                imported.Add(new EmployeeModel { EmployeeId = fields[0], FullName = fields[1], Department = fields[2], Salary = salary, ImagePath = fields[4] });
            }
            _employees.Clear(); foreach (var e in imported) _employees.Add(e);
            _nextId = _employees.Select(e => { var digits = new string(e.EmployeeId.Where(char.IsDigit).ToArray()); return int.TryParse(digits, out var n) ? n : 0; }).DefaultIfEmpty(0).Max() + 1;
            _txtSearch.Clear(); _source.DataSource = _employees; _source.ResetBindings(false); UpdateSummary(); ClearForm();
            _statusText.Text = $"Đã tải {imported.Count} nhân viên từ CSV";
            MessageBox.Show(this, $"Đã đọc {imported.Count} nhân viên.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(this, "Không thể đọc CSV. Hãy kiểm tra định dạng file.\n" + ex.Message, "Lỗi đọc file", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r')) return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>(); var field = new StringBuilder(); bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else field.Append(c);
            }
            else if (c == ',' ) { result.Add(field.ToString()); field.Clear(); }
            else if (c == '"' && field.Length == 0) quoted = true;
            else field.Append(c);
        }
        if (quoted) throw new FormatException("Có dấu ngoặc kép chưa được đóng trong CSV.");
        result.Add(field.ToString()); return result;
    }
}
