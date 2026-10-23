using System.ComponentModel;

namespace BaiTap4_WarehouseAnalytics;

public class frmWarehouseAnalytics : Form
{
    private readonly ComboBox cboWarehouse = new();
    private readonly DateTimePicker dtpFrom = new();
    private readonly DateTimePicker dtpTo = new();
    private readonly Button btnAnalyze = new();
    private readonly Button btnCancel = new();
    private readonly ProgressBar pgbStatus = new();
    private readonly Label lblPercent = new();
    private readonly Label lblStatus = new();
    private readonly DataGridView dgvReportData = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public frmWarehouseAnalytics()
    {
        Text = "Bài tập 4 - Bảng điều khiển tổng hợp báo cáo kho hàng";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1120, 720);
        Font = new Font("Segoe UI", 10F);
        BuildInterface();
        Load += (_, _) => LoadSampleData();
        FormClosing += (_, _) => _cts?.Cancel();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(14) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = new Label { Text = "BÁO CÁO TỔNG HỢP TỒN KHO", Dock = DockStyle.Fill, AutoSize = true, Font = new Font("Segoe UI", 17F, FontStyle.Bold), Padding = new Padding(0, 0, 0, 12) };
        root.Controls.Add(title, 0, 0);

        var criteria = new GroupBox { Text = "Tiêu chí thống kê", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 10) };
        var criteriaGrid = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 6, RowCount = 2 };
        criteriaGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        criteriaGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        criteriaGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        criteriaGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        criteriaGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        criteriaGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        criteriaGrid.Controls.Add(new Label { Text = "Kho hàng:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        cboWarehouse.DropDownStyle = ComboBoxStyle.DropDownList;
        cboWarehouse.Items.AddRange(new object[] { "Tất cả kho", "Kho Hà Nội", "Kho Đà Nẵng", "Kho TP. Hồ Chí Minh" });
        cboWarehouse.SelectedIndex = 0;
        cboWarehouse.Dock = DockStyle.Fill;
        criteriaGrid.Controls.Add(cboWarehouse, 1, 0);
        criteriaGrid.Controls.Add(new Label { Text = "Từ ngày:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 6, 3, 3) }, 2, 0);
        dtpFrom.Format = DateTimePickerFormat.Short; dtpFrom.Value = DateTime.Today.AddDays(-30); dtpFrom.Dock = DockStyle.Fill;
        criteriaGrid.Controls.Add(dtpFrom, 3, 0);
        criteriaGrid.Controls.Add(new Label { Text = "Đến ngày:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 6, 3, 3) }, 4, 0);
        dtpTo.Format = DateTimePickerFormat.Short; dtpTo.Value = DateTime.Today; dtpTo.Dock = DockStyle.Fill;
        criteriaGrid.Controls.Add(dtpTo, 5, 0);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 10, 0, 0), WrapContents = true };
        btnAnalyze.Text = "Thống Kê Kho"; btnAnalyze.AutoSize = true; btnAnalyze.Padding = new Padding(10, 5, 10, 5); btnAnalyze.Click += async (_, _) => await AnalyzeAsync();
        btnCancel.Text = "Hủy Tác Vụ"; btnCancel.AutoSize = true; btnCancel.Padding = new Padding(10, 5, 10, 5); btnCancel.Enabled = false; btnCancel.Click += (_, _) => _cts?.Cancel();
        actions.Controls.Add(btnAnalyze); actions.Controls.Add(btnCancel);
        criteriaGrid.Controls.Add(actions, 0, 1); criteriaGrid.SetColumnSpan(actions, 6);
        criteria.Controls.Add(criteriaGrid);
        root.Controls.Add(criteria, 0, 1);

        var resultPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0, 8, 0, 0) };
        resultPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); resultPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); resultPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var progressRow = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 2 };
        progressRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); progressRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        progressRow.Controls.Add(new Label { Text = "Tiến trình xử lý", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        lblPercent.Text = "0%"; lblPercent.AutoSize = true; lblPercent.Anchor = AnchorStyles.Right;
        progressRow.Controls.Add(lblPercent, 1, 0);
        pgbStatus.Minimum = 0; pgbStatus.Maximum = 100; pgbStatus.Value = 0; pgbStatus.Dock = DockStyle.Fill; pgbStatus.Height = 20;
        progressRow.Controls.Add(pgbStatus, 0, 1); progressRow.SetColumnSpan(pgbStatus, 2);
        resultPanel.Controls.Add(progressRow, 0, 0);
        lblStatus.Text = "Sẵn sàng thống kê."; lblStatus.AutoSize = true; lblStatus.Padding = new Padding(0, 7, 0, 7);
        resultPanel.Controls.Add(lblStatus, 0, 1);
        dgvReportData.Dock = DockStyle.Fill; dgvReportData.ReadOnly = true; dgvReportData.AllowUserToAddRows = false; dgvReportData.AllowUserToDeleteRows = false;
        dgvReportData.AutoGenerateColumns = false; dgvReportData.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvReportData.MultiSelect = false; dgvReportData.RowHeadersVisible = false; dgvReportData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        AddColumn("Warehouse", "Kho hàng"); AddColumn("ProductCode", "Mã sản phẩm"); AddColumn("ProductName", "Tên sản phẩm"); AddColumn("Quantity", "Số lượng tồn"); AddColumn("UnitPrice", "Đơn giá", "N0"); AddColumn("TotalValue", "Giá trị tồn", "N0"); AddColumn("ReportDate", "Ngày cập nhật", "dd/MM/yyyy");
        resultPanel.Controls.Add(dgvReportData, 0, 2);
        root.Controls.Add(resultPanel, 0, 2);
    }

    private void AddColumn(string property, string header, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = header, Name = property, SortMode = DataGridViewColumnSortMode.Automatic };
        if (format != null) column.DefaultCellStyle.Format = format;
        dgvReportData.Columns.Add(column);
    }

    private async Task AnalyzeAsync()
    {
        if (_isRunning) return;
        if (dtpFrom.Value.Date > dtpTo.Value.Date)
        {
            MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        btnAnalyze.Enabled = false; btnCancel.Enabled = true; cboWarehouse.Enabled = false; dtpFrom.Enabled = false; dtpTo.Enabled = false;
        pgbStatus.Value = 0; lblPercent.Text = "0%"; lblStatus.Text = "Đang quét và tổng hợp dữ liệu kho...";

        var progress = new Progress<int>(percent =>
        {
            pgbStatus.Value = Math.Clamp(percent, 0, 100);
            lblPercent.Text = $"{percent}%";
            lblStatus.Text = percent < 100 ? $"Đang xử lý dữ liệu... {percent}%" : "Đang hoàn tất báo cáo...";
        });

        try
        {
            string selectedWarehouse = cboWarehouse.SelectedItem?.ToString() ?? "Tất cả kho";
            DateTime from = dtpFrom.Value.Date, to = dtpTo.Value.Date;
            List<WarehouseItem> result = await Task.Run(() => GenerateReport(selectedWarehouse, from, to, progress, token), token);
            dgvReportData.DataSource = new BindingList<WarehouseItem>(result);
            lblStatus.Text = $"Hoàn tất! Có {result.Count:N0} mặt hàng trong báo cáo.";
            pgbStatus.Value = 100; lblPercent.Text = "100%";
        }
        catch (OperationCanceledException)
        {
            pgbStatus.Value = 0; lblPercent.Text = "0%"; lblStatus.Text = "Tác vụ đã được hủy.";
            MessageBox.Show("Đã hủy tác vụ thống kê kho.", "Đã hủy", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Có lỗi khi tạo báo cáo.";
            MessageBox.Show($"Không thể tạo báo cáo:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cts?.Dispose(); _cts = null; _isRunning = false;
            btnAnalyze.Enabled = true; btnCancel.Enabled = false; cboWarehouse.Enabled = true; dtpFrom.Enabled = true; dtpTo.Enabled = true;
        }
    }

    private static List<WarehouseItem> GenerateReport(string warehouse, DateTime from, DateTime to, IProgress<int> progress, CancellationToken token)
    {
        var warehouses = warehouse == "Tất cả kho" ? new[] { "Kho Hà Nội", "Kho Đà Nẵng", "Kho TP. Hồ Chí Minh" } : new[] { warehouse };
        var products = new[] { "Bàn phím cơ", "Chuột không dây", "Màn hình 24 inch", "Cáp mạng Cat6", "Bộ phát Wi-Fi", "Ổ cứng SSD", "RAM 16GB", "Switch 24 port", "Camera IP", "Laptop văn phòng" };
        var random = new Random(2026);
        var result = new List<WarehouseItem>();
        const int steps = 50;
        for (int i = 1; i <= steps; i++)
        {
            token.ThrowIfCancellationRequested();
            // Giả lập xử lý dữ liệu lớn trong khoảng 4-5 giây, có kiểm tra hủy thường xuyên.
            if (token.WaitHandle.WaitOne(90)) token.ThrowIfCancellationRequested();
            for (int j = 0; j < 8; j++)
            {
                token.ThrowIfCancellationRequested();
                string wh = warehouses[(i + j) % warehouses.Length];
                DateTime date = from.AddDays(random.Next(Math.Max(1, (to - from).Days + 1)));
                int quantity = random.Next(0, 501);
                decimal price = random.Next(5, 1501) * 1000m;
                result.Add(new WarehouseItem { Warehouse = wh, ProductCode = $"SP{i:0000}{j:00}", ProductName = products[(i + j) % products.Length], Quantity = quantity, UnitPrice = price, ReportDate = date });
            }
            progress.Report(i * 100 / steps);
        }
        return result.OrderBy(x => x.Warehouse).ThenBy(x => x.ProductCode).ToList();
    }

    private void LoadSampleData()
    {
        dgvReportData.DataSource = new BindingList<WarehouseItem>(new List<WarehouseItem>
        {
            new() { Warehouse = "Kho Hà Nội", ProductCode = "SP000101", ProductName = "Bàn phím cơ", Quantity = 42, UnitPrice = 850000, ReportDate = DateTime.Today },
            new() { Warehouse = "Kho Đà Nẵng", ProductCode = "SP000202", ProductName = "Chuột không dây", Quantity = 85, UnitPrice = 320000, ReportDate = DateTime.Today },
            new() { Warehouse = "Kho TP. Hồ Chí Minh", ProductCode = "SP000303", ProductName = "Màn hình 24 inch", Quantity = 17, UnitPrice = 2450000, ReportDate = DateTime.Today }
        });
    }
}
