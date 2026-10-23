using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HotelBookingManager;

public class frmBookingManager : Form
{
    private readonly TextBox txtBookingId = new();
    private readonly TextBox txtCustomerName = new();
    private readonly TextBox txtIdentityCard = new();
    private readonly ComboBox cboRoomType = new();
    private readonly DateTimePicker dtpCheckIn = new();
    private readonly NumericUpDown numDays = new();
    private readonly TextBox txtPricePerNight = new();
    private readonly DataGridView dgvBookings = new();
    private readonly ErrorProvider errorProvider = new();
    private readonly BindingList<BookingModel> bookings = new();
    private readonly BindingSource bookingSource = new();
    private bool loadingFields;

    public frmBookingManager()
    {
        Text = "Quản lý đặt phòng khách sạn";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 620);
        Size = new Size(1120, 700);
        Font = new Font("Segoe UI", 10F);
        BuildInterface();
        ConfigureGrid();
        bookingSource.DataSource = bookings;
        dgvBookings.DataSource = bookingSource;
        dgvBookings.SelectionChanged += DgvBookings_SelectionChanged;
        errorProvider.ContainerControl = this;
        txtBookingId.ReadOnly = true;
        txtBookingId.TabStop = false;
        cboRoomType.DropDownStyle = ComboBoxStyle.DropDownList;
        cboRoomType.Items.AddRange(new object[] { "Standard", "Deluxe", "VIP" });
        cboRoomType.SelectedIndex = 0;
        dtpCheckIn.Format = DateTimePickerFormat.Custom;
        dtpCheckIn.CustomFormat = "dd/MM/yyyy";
        numDays.Minimum = 1;
        numDays.Maximum = 365;
        numDays.Value = 1;
        AddSampleData();
        RefreshBookingId();
        ClearFields(false);
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var inputGroup = new GroupBox { Text = "Thông tin đặt phòng", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12) };
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4, RowCount = 4 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 4; i++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        AddField(fields, "Mã đặt phòng:", txtBookingId, 0, 0);
        AddField(fields, "Tên khách hàng:", txtCustomerName, 1, 0);
        AddField(fields, "Số CCCD/CMND:", txtIdentityCard, 2, 0);
        AddField(fields, "Loại phòng:", cboRoomType, 3, 0);
        AddField(fields, "Ngày nhận phòng:", dtpCheckIn, 0, 2);
        AddField(fields, "Số ngày ở:", numDays, 1, 2);
        AddField(fields, "Giá phòng/đêm:", txtPricePerNight, 2, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 4, 0, 0) };
        buttons.Controls.Add(MakeButton("Thêm đặt phòng", (_, _) => AddBooking()));
        buttons.Controls.Add(MakeButton("Cập nhật", (_, _) => UpdateBooking()));
        buttons.Controls.Add(MakeButton("Xóa", (_, _) => DeleteBooking()));
        buttons.Controls.Add(MakeButton("Làm mới", (_, _) => ClearFields()));
        fields.Controls.Add(buttons, 1, 3);
        fields.SetColumnSpan(buttons, 3);
        inputGroup.Controls.Add(fields);
        root.Controls.Add(inputGroup, 0, 0);

        var gridGroup = new GroupBox { Text = "Danh sách đặt phòng", Dock = DockStyle.Fill, Padding = new Padding(8) };
        dgvBookings.Dock = DockStyle.Fill;
        dgvBookings.BackgroundColor = Color.White;
        dgvBookings.BorderStyle = BorderStyle.Fixed3D;
        dgvBookings.AllowUserToAddRows = false;
        dgvBookings.AllowUserToDeleteRows = false;
        dgvBookings.MultiSelect = false;
        dgvBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvBookings.ReadOnly = true;
        dgvBookings.AutoGenerateColumns = false;
        dgvBookings.RowHeadersVisible = false;
        dgvBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBookings.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        gridGroup.Controls.Add(dgvBookings);
        root.Controls.Add(gridGroup, 0, 1);
    }

    private static void AddField(TableLayoutPanel panel, string labelText, Control control, int row, int column)
    {
        var label = new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(3, 5, 10, 5);
        panel.Controls.Add(label, column, row);
        panel.Controls.Add(control, column + 1, row);
    }

    private static Button MakeButton(string text, EventHandler handler)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 34, Padding = new Padding(10, 2, 10, 2), Margin = new Padding(3, 2, 8, 2), UseVisualStyleBackColor = true };
        button.Click += handler;
        return button;
    }

    private void ConfigureGrid()
    {
        dgvBookings.Columns.Clear();
        AddColumn("BookingId", "Mã Đặt", 85);
        AddColumn("CustomerName", "Tên Khách", 160);
        AddColumn("IdentityCard", "CCCD", 115);
        AddColumn("RoomType", "Loại Phòng", 100);
        AddColumn("CheckIn", "Ngày Nhận", 110, "dd/MM/yyyy");
        AddColumn("Days", "Số Đêm", 75);
        AddColumn("PricePerNight", "Giá/Đêm", 110, "N0");
    }

    private void AddColumn(string property, string header, int minWidth, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = header,
            Name = "col" + property,
            MinimumWidth = minWidth,
            SortMode = DataGridViewColumnSortMode.Automatic
        };
        if (format != null) column.DefaultCellStyle.Format = format;
        if (property == "PricePerNight") column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvBookings.Columns.Add(column);
    }

    private void AddSampleData()
    {
        bookings.Add(new BookingModel { BookingId = "BK001", CustomerName = "Nguyễn Minh Anh", IdentityCard = "001234567890", RoomType = "Deluxe", CheckIn = DateTime.Today, Days = 2, PricePerNight = 850000m });
        bookings.Add(new BookingModel { BookingId = "BK002", CustomerName = "Trần Hoàng Nam", IdentityCard = "123456789", RoomType = "Standard", CheckIn = DateTime.Today.AddDays(1), Days = 1, PricePerNight = 500000m });
    }

    private void DgvBookings_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvBookings.CurrentRow?.DataBoundItem is not BookingModel booking) return;
        loadingFields = true;
        txtBookingId.Text = booking.BookingId;
        txtCustomerName.Text = booking.CustomerName;
        txtIdentityCard.Text = booking.IdentityCard;
        cboRoomType.SelectedItem = booking.RoomType;
        dtpCheckIn.Value = booking.CheckIn;
        numDays.Value = Math.Clamp(booking.Days, (int)numDays.Minimum, (int)numDays.Maximum);
        txtPricePerNight.Text = booking.PricePerNight.ToString("0.##", CultureInfo.CurrentCulture);
        errorProvider.Clear();
        loadingFields = false;
    }

    private bool ValidateInput(out decimal price)
    {
        bool valid = true;
        errorProvider.Clear();
        price = 0;
        if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
        {
            errorProvider.SetError(txtCustomerName, "Tên khách hàng không được để trống.");
            valid = false;
        }
        string identity = txtIdentityCard.Text.Trim();
        if (!Regex.IsMatch(identity, @"^(\d{9}|\d{12})$"))
        {
            errorProvider.SetError(txtIdentityCard, "CCCD/CMND phải gồm đúng 9 hoặc 12 chữ số.");
            valid = false;
        }
        if (!decimal.TryParse(txtPricePerNight.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out price) || price <= 0)
        {
            errorProvider.SetError(txtPricePerNight, "Giá phòng phải là số lớn hơn 0.");
            valid = false;
        }
        return valid;
    }

    private BookingModel ReadFields(string id, decimal price) => new()
    {
        BookingId = id,
        CustomerName = txtCustomerName.Text.Trim(),
        IdentityCard = txtIdentityCard.Text.Trim(),
        RoomType = cboRoomType.SelectedItem?.ToString() ?? "Standard",
        CheckIn = dtpCheckIn.Value.Date,
        Days = (int)numDays.Value,
        PricePerNight = price
    };

    private void AddBooking()
    {
        if (!ValidateInput(out decimal price)) return;
        string id = GetNextBookingId();
        bookings.Add(ReadFields(id, price));
        RefreshBookingId();
        SelectBooking(id);
        MessageBox.Show("Đã thêm đặt phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpdateBooking()
    {
        if (dgvBookings.CurrentRow?.DataBoundItem is not BookingModel selected)
        {
            MessageBox.Show("Em hãy chọn một đặt phòng cần cập nhật.", "Chưa chọn dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!ValidateInput(out decimal price)) return;
        var updated = ReadFields(selected.BookingId, price);
        int index = bookings.IndexOf(selected);
        if (index < 0) return;
        // Copy values into the bound object so BindingList/BindingSource retain the same row identity.
        selected.CustomerName = updated.CustomerName;
        selected.IdentityCard = updated.IdentityCard;
        selected.RoomType = updated.RoomType;
        selected.CheckIn = updated.CheckIn;
        selected.Days = updated.Days;
        selected.PricePerNight = updated.PricePerNight;
        bookingSource.ResetItem(index);
        MessageBox.Show("Đã cập nhật thông tin đặt phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DeleteBooking()
    {
        if (dgvBookings.CurrentRow?.DataBoundItem is not BookingModel selected)
        {
            MessageBox.Show("Em hãy chọn một đặt phòng cần xóa.", "Chưa chọn dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var result = MessageBox.Show($"Em có chắc muốn xóa đặt phòng {selected.BookingId} của khách {selected.CustomerName}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result != DialogResult.Yes) return;
        int oldIndex = bookings.IndexOf(selected);
        bookings.Remove(selected);
        if (bookings.Count > 0) bookingSource.Position = Math.Min(oldIndex, bookings.Count - 1);
        else ClearFields();
        RefreshBookingId();
    }

    private void ClearFields() => ClearFields(true);
    private void ClearFields(bool focusName)
    {
        loadingFields = true;
        dgvBookings.ClearSelection();
        dgvBookings.CurrentCell = null;
        txtBookingId.Text = GetNextBookingId();
        txtCustomerName.Clear();
        txtIdentityCard.Clear();
        cboRoomType.SelectedIndex = 0;
        dtpCheckIn.Value = DateTime.Today;
        numDays.Value = 1;
        txtPricePerNight.Clear();
        errorProvider.Clear();
        loadingFields = false;
        if (focusName) txtCustomerName.Focus();
    }

    private string GetNextBookingId()
    {
        int max = 0;
        foreach (var item in bookings)
        {
            if (item.BookingId.StartsWith("BK", StringComparison.OrdinalIgnoreCase) && int.TryParse(item.BookingId[2..], out int number))
                max = Math.Max(max, number);
        }
        return $"BK{max + 1:000}";
    }

    private void RefreshBookingId() => txtBookingId.Text = GetNextBookingId();

    private void SelectBooking(string id)
    {
        for (int i = 0; i < bookings.Count; i++)
        {
            if (bookings[i].BookingId == id)
            {
                bookingSource.Position = i;
                return;
            }
        }
    }
}
