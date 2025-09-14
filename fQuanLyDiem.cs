using ClosedXML.Excel;
using QLSV;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QuanLySinhVien.GUI
{
    public partial class fQuanLyDiem : Form
    {
        connectData c = new connectData();
        DataTable dtMon;

        public fQuanLyDiem()
        {
            InitializeComponent();
        }

        
        private bool MaSVExists(string maSV)
        {
            if (string.IsNullOrWhiteSpace(maSV)) return false;

            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM SinhVien WHERE MaSV=@sv", c.conn))
                {
                    cmd.Parameters.Add("@sv", SqlDbType.NVarChar, 20).Value = maSV.Trim();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
            catch { return false; }
            finally { c.disconnect(); }
        }

        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"
            SELECT  IDDiem AS ID,
                    MaSV, MaMon, LanThi,
                    CONVERT(date, NgayNhap) AS NgayNhap,
                    DiemQT, DiemThi, DiemTong,
                    ISNULL(GhiChu,N'') AS GhiChu
            FROM Diem
            ORDER BY IDDiem DESC";

                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                    dgvDiem.AutoGenerateColumns = false;
                    dgvDiem.DataSource = dt;

                    dgvDiem.Columns["Column1"].DataPropertyName = "ID";
                    dgvDiem.Columns["Column2"].DataPropertyName = "MaSV";
                    dgvDiem.Columns["Column3"].DataPropertyName = "MaMon";
                    dgvDiem.Columns["Column4"].DataPropertyName = "LanThi";
                    dgvDiem.Columns["Column5"].DataPropertyName = "NgayNhap";
                    dgvDiem.Columns["Column6"].DataPropertyName = "DiemQT";
                    dgvDiem.Columns["Column7"].DataPropertyName = "DiemThi";
                    dgvDiem.Columns["Column8"].DataPropertyName = "DiemTong";
                    dgvDiem.Columns["Column9"].DataPropertyName = "GhiChu";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void fQuanLyDiem_Load(object sender, EventArgs e)
        {
            LoadCboMon();
            LoadGrid();

            // kiểm tra ngay khi rời ô mã SV
            txtMaSinhVien.Leave += txtMaSinhVien_Leave;
        }

        private void txtMaSinhVien_Leave(object sender, EventArgs e)
        {
            var ma = txtMaSinhVien.Text.Trim();
            if (ma.Length == 0) return;

            if (!MaSVExists(ma))
            {
                MessageBox.Show("Mã sinh viên không tồn tại trong hệ thống.",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaSinhVien.Focus();
                txtMaSinhVien.SelectAll();
            }
        }

        private void LoadCboMon()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = "SELECT MaMon, TenMon FROM MonHoc ORDER BY TenMon";
                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    dtMon = new DataTable();
                    da.Fill(dtMon);
                }

                cboMaMon.DropDownStyle = ComboBoxStyle.DropDownList;
                cboMaMon.DataSource = dtMon;
                cboMaMon.DisplayMember = "TenMon";
                cboMaMon.ValueMember = "MaMon";
                cboMaMon.SelectedIndex = -1;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp môn học: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void dgvDiem_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = dgvDiem.Rows[e.RowIndex];

            txtID.Text = r.Cells["Column1"].Value?.ToString();
            txtMaSinhVien.Text = r.Cells["Column2"].Value?.ToString();
            var maMon = r.Cells["Column3"].Value?.ToString();
            try { cboMaMon.SelectedValue = maMon; } catch { cboMaMon.SelectedIndex = -1; }

            if (int.TryParse(r.Cells["Column4"].Value?.ToString(), out int lan)) nudLanThi.Value = lan; else nudLanThi.Value = 0;

            if (DateTime.TryParse(r.Cells["Column5"].Value?.ToString(), out DateTime nn))
                dtpNgayThi.Value = nn;
            else
                dtpNgayThi.Value = DateTime.Today;

            if (decimal.TryParse(r.Cells["Column6"].Value?.ToString(), out decimal dqt)) nudDiemQT.Value = dqt; else nudDiemQT.Value = 0;
            if (decimal.TryParse(r.Cells["Column7"].Value?.ToString(), out decimal dthi)) nudDiemThi.Value = dthi; else nudDiemThi.Value = 0;
            if (decimal.TryParse(r.Cells["Column8"].Value?.ToString(), out decimal dtg)) nudDiemTong.Value = dtg; else nudDiemTong.Value = 0;

            txtGhiChu.Text = r.Cells["Column9"].Value?.ToString();
        }

        private void ClearForm()
        {
            txtID.Clear();
            txtMaSinhVien.Clear();
            cboMaMon.SelectedIndex = -1;
            nudLanThi.Value = 0;
            dtpNgayThi.Value = DateTime.Today;
            nudDiemQT.Value = 0;
            nudDiemThi.Value = 0;
            nudDiemTong.Value = 0;
            txtGhiChu.Clear();

            if (dgvDiem.Rows.Count > 0) dgvDiem.ClearSelection();
            txtMaSinhVien.Focus();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSinhVien.Text) || cboMaMon.SelectedValue == null)
            { MessageBox.Show("Nhập Mã SV và chọn Môn học."); return; }

            // Chặn ngay nếu mã SV không tồn tại
            if (!MaSVExists(txtMaSinhVien.Text.Trim()))
            {
                MessageBox.Show("Mã sinh viên không tồn tại trong hệ thống.",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaSinhVien.Focus(); txtMaSinhVien.SelectAll();
                return;
            }

            try
            {
                c.connect();

                using (var chk = new SqlCommand(
                    "SELECT COUNT(1) FROM Diem WHERE MaSV=@sv AND MaMon=@mon AND LanThi=@lan", c.conn))
                {
                    chk.Parameters.AddWithValue("@sv", txtMaSinhVien.Text.Trim());
                    chk.Parameters.AddWithValue("@mon", cboMaMon.SelectedValue.ToString());
                    chk.Parameters.Add("@lan", SqlDbType.Int).Value = (int)nudLanThi.Value;

                    if ((int)chk.ExecuteScalar() > 0)
                    { MessageBox.Show("Điểm cho SV/Môn/Lần thi này đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"INSERT INTO Diem(MaSV, MaMon, LanThi, NgayNhap, DiemQT, DiemThi, GhiChu)
                      VALUES(@sv, @mon, @lan, @ngay, @dqt, @dthi, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@sv", txtMaSinhVien.Text.Trim());
                    cmd.Parameters.AddWithValue("@mon", cboMaMon.SelectedValue.ToString());
                    cmd.Parameters.Add("@lan", SqlDbType.Int).Value = (int)nudLanThi.Value;
                    cmd.Parameters.Add("@ngay", SqlDbType.Date).Value = dtpNgayThi.Value.Date;

                    var p1 = cmd.Parameters.Add("@dqt", SqlDbType.Decimal); p1.Precision = 5; p1.Scale = 2; p1.Value = nudDiemQT.Value;
                    var p2 = cmd.Parameters.Add("@dthi", SqlDbType.Decimal); p2.Precision = 5; p2.Scale = 2; p2.Value = nudDiemThi.Value;

                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text.Trim();

                    cmd.ExecuteNonQuery();
                }

                LoadGrid();
                ClearForm();
                MessageBox.Show("Thêm thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            { MessageBox.Show("Chưa chọn đúng ID để sửa."); return; }
            if (string.IsNullOrWhiteSpace(txtMaSinhVien.Text) || cboMaMon.SelectedValue == null)
            { MessageBox.Show("Nhập Mã SV và chọn Môn học."); return; }

            // Chặn sửa nếu mã SV không tồn tại
            if (!MaSVExists(txtMaSinhVien.Text.Trim()))
            {
                MessageBox.Show("Mã sinh viên không tồn tại trong hệ thống.",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaSinhVien.Focus(); txtMaSinhVien.SelectAll();
                return;
            }

            try
            {
                c.connect();

                using (var cmd = new SqlCommand(
                    @"UPDATE Diem SET
                         MaSV=@sv, MaMon=@mon, LanThi=@lan, NgayNhap=@ngay,
                         DiemQT=@dqt, DiemThi=@dthi, GhiChu=@gc
                      WHERE IDDiem=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@sv", txtMaSinhVien.Text.Trim());
                    cmd.Parameters.AddWithValue("@mon", cboMaMon.SelectedValue.ToString());
                    cmd.Parameters.Add("@lan", SqlDbType.Int).Value = (int)nudLanThi.Value;
                    cmd.Parameters.Add("@ngay", SqlDbType.Date).Value = dtpNgayThi.Value.Date;

                    var p1 = cmd.Parameters.Add("@dqt", SqlDbType.Decimal); p1.Precision = 5; p1.Scale = 2; p1.Value = nudDiemQT.Value;
                    var p2 = cmd.Parameters.Add("@dthi", SqlDbType.Decimal); p2.Precision = 5; p2.Scale = 2; p2.Value = nudDiemThi.Value;

                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text.Trim();
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    cmd.ExecuteNonQuery();
                }

                LoadGrid();
                ClearForm();
                MessageBox.Show("Sửa thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi sửa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            { MessageBox.Show("Chưa chọn đúng ID để xóa."); return; }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                using (var cmd = new SqlCommand("DELETE FROM Diem WHERE IDDiem=@id", c.conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    int n = cmd.ExecuteNonQuery();
                    if (n == 0) { MessageBox.Show("Không tìm thấy ID để xóa."); return; }
                }

                LoadGrid();
                ClearForm();
                MessageBox.Show("Xóa thành công!");
            }
            catch (SqlException sx) when (sx.Number == 547)
            { MessageBox.Show("Không thể xóa: bản ghi đang được tham chiếu."); }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
        private void label1_Click(object sender, EventArgs e) { /* no-op */ }
        private void label4_Click(object sender, EventArgs e) { /* no-op */ }

        private void button1_Click(object sender, EventArgs e)
        {

            if (dgvDiem.Rows.Count > 0)
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                var ws = wb.Worksheets.Add("Diem");

                                ws.Cell(1, 1).Value = "Danh sách Điểm";
                                ws.Range(1, 1, 1, dgvDiem.Columns.Count).Merge();
                                ws.Range(1, 1, 1, dgvDiem.Columns.Count)
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                    .Font.SetBold()
                                    .Font.SetFontName("Times New Roman")
                                    .Font.SetFontSize(12);

                                for (int i = 0; i < dgvDiem.Columns.Count; i++)
                                {
                                    ws.Cell(2, i + 1).Value = dgvDiem.Columns[i].HeaderText;
                                }


                                for (int i = 0; i < dgvDiem.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dgvDiem.Columns.Count; j++)
                                    {
                                        ws.Cell(i + 3, j + 1).Value = dgvDiem.Rows[i].Cells[j].Value?.ToString();
                                    }
                                }


                                var range = ws.Range(1, 1, dgvDiem.Rows.Count + 1, dgvDiem.Columns.Count);
                                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                                ws.Columns().AdjustToContents();


                                wb.SaveAs(sfd.FileName);
                            }

                            MessageBox.Show("Xuất Excel thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
