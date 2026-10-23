using System.Drawing;
using System.Globalization;

namespace BaiTap5_IoTMonitor;

public class frmIoTMonitor : Form
{
    private readonly Button btnStart = new() { Text = "Bắt đầu Giám sát", AutoSize = true };
    private readonly Button btnStop = new() { Text = "Dừng Giám sát", AutoSize = true, Enabled = false };
    private readonly Label lblTemperature = new() { Text = "--.- °C", AutoSize = true, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.ForestGreen };
    private readonly DataGridView dgvLogs = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    private readonly ToolStripStatusLabel statusConnection = new("● Đã dừng") { Spring = false };
    private readonly ToolStripStatusLabel statusCycles = new("Số chu kỳ: 0") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;
    private int _cycles;

    public frmIoTMonitor()
    {
        Text = "Bài tập 5 - Giám sát cảnh báo IoT Real-time";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(780, 520);
        Size = new Size(1000, 650);
        BuildUi();
        FormClosing += async (_, _) => await StopMonitoringAsync();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var controls = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 12) };
        btnStart.Click += async (_, _) => await StartMonitoringAsync();
        btnStop.Click += async (_, _) => await StopMonitoringAsync();
        controls.Controls.Add(btnStart); controls.Controls.Add(btnStop);

        var sensor = new GroupBox { Text = "Nhiệt độ cảm biến hiện tại", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(15) };
        var sensorLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        sensorLayout.Controls.Add(lblTemperature);
        var note = new Label { Text = "  |  Bình thường: ≤ 37 °C     Cảnh báo: > 37 °C", AutoSize = true, Padding = new Padding(0, 16, 0, 0) };
        sensorLayout.Controls.Add(note); sensor.Controls.Add(sensorLayout);

        var logsGroup = new GroupBox { Text = "Nhật ký nhiệt độ theo thời gian thực", Dock = DockStyle.Fill, Padding = new Padding(8) };
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTime", HeaderText = "Thời gian", DataPropertyName = "Time", FillWeight = 40 });
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTemperature", HeaderText = "Nhiệt độ (°C)", DataPropertyName = "Temperature", FillWeight = 25 });
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Trạng thái", DataPropertyName = "Status", FillWeight = 35 });
        logsGroup.Controls.Add(dgvLogs);

        root.Controls.Add(controls, 0, 0); root.Controls.Add(sensor, 0, 1); root.Controls.Add(logsGroup, 0, 2);
        var status = new StatusStrip(); status.Items.Add(statusConnection); status.Items.Add(statusCycles);
        Controls.Add(root); Controls.Add(status);
    }

    private async Task StartMonitoringAsync()
    {
        if (_monitorTask is { IsCompleted: false }) return;
        _cts?.Dispose(); _cts = new CancellationTokenSource();
        var token = _cts.Token;
        btnStart.Enabled = false; btnStop.Enabled = true;
        statusConnection.Text = "● Đang kết nối / giám sát";
        _monitorTask = Task.Run(async () =>
        {
            var random = new Random();
            try
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    double temperature = Math.Round(random.NextDouble() * 12 + 30, 1);
                    string timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                    int cycle = Interlocked.Increment(ref _cycles);
                    string state = temperature > 37 ? "CẢNH BÁO NHIỆT ĐỘ CAO" : "Bình thường";
                    UpdateUiSafely(() =>
                    {
                        lblTemperature.Text = $"{temperature:0.0} °C";
                        lblTemperature.ForeColor = temperature > 37 ? Color.Crimson : Color.ForestGreen;
                        statusConnection.Text = temperature > 37 ? "● Có cảnh báo" : "● Đang giám sát";
                        statusCycles.Text = $"Số chu kỳ: {cycle}";
                        dgvLogs.Rows.Insert(0, timestamp, temperature.ToString("0.0"), state);
                        if (dgvLogs.Rows.Count > 500) dgvLogs.Rows.RemoveAt(dgvLogs.Rows.Count - 1);
                        if (temperature > 37) dgvLogs.Rows[0].DefaultCellStyle.ForeColor = Color.Crimson;
                    });
                }
            }
            catch (OperationCanceledException) { }
        }, token);
        try { await _monitorTask; }
        catch (OperationCanceledException) { }
        finally
        {
            if (!IsDisposed)
            {
                btnStart.Enabled = true; btnStop.Enabled = false;
                statusConnection.Text = "● Đã dừng";
            }
        }
    }

    private async Task StopMonitoringAsync()
    {
        if (_cts is null) return;
        _cts.Cancel();
        if (_monitorTask is not null)
        {
            try { await _monitorTask; } catch (OperationCanceledException) { }
        }
        _cts.Dispose(); _cts = null;
        if (!IsDisposed)
        {
            btnStart.Enabled = true; btnStop.Enabled = false;
            statusConnection.Text = "● Đã dừng";
        }
    }

    private void UpdateUiSafely(Action update)
    {
        if (IsDisposed || Disposing) return;
        if (InvokeRequired)
        {
            try { Invoke(new Action(() => { if (!IsDisposed && !Disposing) update(); })); }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }
        else update();
    }
}
