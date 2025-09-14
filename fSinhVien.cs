using ClosedXML.Excel;
using QLSV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLySinhVien.GUI
{
    public partial class fSinhVien : Form
    {
        connectData c = new connectData();
        DataTable dtLop, dtDanToc, dtTonGiao;
        public fSinhVien()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            { MessageBox.Show("Chưa chọn đúng ID để sửa."); return; }

            if (string.IsNullOrWhiteSpace(txtMaSV.Text) ||
                string.IsNullOrWhiteSpace(txtHo.Text) ||
                string.IsNullOrWhiteSpace(txtTen.Text) ||
                cboLop.SelectedValue == null ||
                cboDanToc.SelectedValue == null ||
                cboTonGiao.SelectedValue == null)
            { MessageBox.Show("Nhập đủ thông tin."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand(
                    "SELECT COUNT(1) FROM SinhVien WHERE MaSV=@masv AND IDSinhVien<>@id", c.conn))
                {
                    chk.Parameters.AddWithValue("@masv", txtMaSV.Text.Trim());
                    chk.Parameters.AddWithValue("@id", id);
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã SV đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE SinhVien SET
                        MaSV=@masv, Ho=@ho, Ten=@ten, NgaySinh=@ns, GioiTinh=@gt, Email=@email, DienThoai=@dt,
                        MaLop=@malop, MaDanToc=@mdt, MaTonGiao=@mtg, GhiChu=@gc
                      WHERE IDSinhVien=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@masv", txtMaSV.Text.Trim());
                    cmd.Parameters.AddWithValue("@ho", txtHo.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTen.Text.Trim());
                    cmd.Parameters.Add("@ns", SqlDbType.Date).Value = dtpNgaySinh.Value.Date;
                    cmd.Parameters.AddWithValue("@gt", rdbNu.Checked ? "Nữ" : "Nam");
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.Add("@dt", SqlDbType.NVarChar, 20).Value = nudDienThoai.Value.ToString();
                    cmd.Parameters.AddWithValue("@malop", cboLop.SelectedValue.ToString());
                    cmd.Parameters.AddWithValue("@mdt", cboDanToc.SelectedValue.ToString());
                    cmd.Parameters.AddWithValue("@mtg", cboTonGiao.SelectedValue.ToString());
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

        private void dgvSinhVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int i = dgvSinhVien.CurrentRow.Index;

            txtID.Text = dgvSinhVien.Rows[i].Cells["Column1"].Value?.ToString();
            txtMaSV.Text = dgvSinhVien.Rows[i].Cells["Column2"].Value?.ToString();
            txtHo.Text = dgvSinhVien.Rows[i].Cells["Column3"].Value?.ToString();
            txtTen.Text = dgvSinhVien.Rows[i].Cells["Column4"].Value?.ToString();

            if (DateTime.TryParse(dgvSinhVien.Rows[i].Cells["Column5"].Value?.ToString(), out DateTime ns))
                dtpNgaySinh.Value = ns;

            string gt = dgvSinhVien.Rows[i].Cells["Column6"].Value?.ToString();
            rdbNu.Checked = string.Equals(gt, "Nữ", StringComparison.OrdinalIgnoreCase);
            rdbNam.Checked = !rdbNu.Checked;

            txtEmail.Text = dgvSinhVien.Rows[i].Cells["Column7"].Value?.ToString();

            if (decimal.TryParse(dgvSinhVien.Rows[i].Cells["Column8"].Value?.ToString(), out decimal phone))
                nudDienThoai.Value = phone;
            else nudDienThoai.Value = 0;

            try { cboLop.SelectedValue = dgvSinhVien.Rows[i].Cells["Column9"].Value?.ToString(); } catch { cboLop.SelectedIndex = -1; }
            try { cboDanToc.SelectedValue = dgvSinhVien.Rows[i].Cells["Column10"].Value?.ToString(); } catch { cboDanToc.SelectedIndex = -1; }
            try { cboTonGiao.SelectedValue = dgvSinhVien.Rows[i].Cells["Column11"].Value?.ToString(); } catch { cboTonGiao.SelectedIndex = -1; }

            txtGhiChu.Text = dgvSinhVien.Rows[i].Cells["Column12"].Value?.ToString();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
        private void fSinhVien_Load(object sender, EventArgs e)
        {
            LoadLookups();
            LoadGrid();
        }
        private void LoadLookups()
        {
            if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

          
            dtLop = new DataTable();
            using (var da = new SqlDataAdapter("SELECT MaLop, TenLop FROM Lop ORDER BY TenLop", c.conn))
                da.Fill(dtLop);
            cboLop.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLop.AutoCompleteMode = AutoCompleteMode.None;
            cboLop.AutoCompleteSource = AutoCompleteSource.None;
            cboLop.DataSource = dtLop; cboLop.DisplayMember = "TenLop"; cboLop.ValueMember = "MaLop";
            cboLop.SelectedIndex = -1;

            
            dtDanToc = new DataTable();
            using (var da = new SqlDataAdapter("SELECT MaDanToc, TenDanToc FROM DanToc ORDER BY TenDanToc", c.conn))
                da.Fill(dtDanToc);
            cboDanToc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDanToc.DataSource = dtDanToc; cboDanToc.DisplayMember = "TenDanToc"; cboDanToc.ValueMember = "MaDanToc";
            cboDanToc.SelectedIndex = -1;

         
            dtTonGiao = new DataTable();
            using (var da = new SqlDataAdapter("SELECT MaTonGiao, TenTonGiao FROM TonGiao ORDER BY TenTonGiao", c.conn))
                da.Fill(dtTonGiao);
            cboTonGiao.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTonGiao.DataSource = dtTonGiao; cboTonGiao.DisplayMember = "TenTonGiao"; cboTonGiao.ValueMember = "MaTonGiao";
            cboTonGiao.SelectedIndex = -1;

            c.disconnect();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSV.Text) ||
                string.IsNullOrWhiteSpace(txtHo.Text) ||
                string.IsNullOrWhiteSpace(txtTen.Text) ||
                cboLop.SelectedValue == null ||
                cboDanToc.SelectedValue == null ||
                cboTonGiao.SelectedValue == null)
            { MessageBox.Show("Nhập Mã SV, Họ, Tên và chọn Lớp/Dân tộc/Tôn giáo."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand("SELECT COUNT(1) FROM SinhVien WHERE MaSV=@masv", c.conn))
                {
                    chk.Parameters.AddWithValue("@masv", txtMaSV.Text.Trim());
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã SV đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"INSERT INTO SinhVien
                      (MaSV, Ho, Ten, NgaySinh, GioiTinh, Email, DienThoai, MaLop, MaDanToc, MaTonGiao, GhiChu)
                      VALUES(@masv, @ho, @ten, @ns, @gt, @email, @dt, @malop, @mdt, @mtg, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@masv", txtMaSV.Text.Trim());
                    cmd.Parameters.AddWithValue("@ho", txtHo.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTen.Text.Trim());
                    cmd.Parameters.Add("@ns", SqlDbType.Date).Value = dtpNgaySinh.Value.Date;
                    cmd.Parameters.AddWithValue("@gt", rdbNu.Checked ? "Nữ" : "Nam");
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.Add("@dt", SqlDbType.NVarChar, 20).Value = nudDienThoai.Value.ToString();
                    cmd.Parameters.AddWithValue("@malop", cboLop.SelectedValue.ToString());
                    cmd.Parameters.AddWithValue("@mdt", cboDanToc.SelectedValue.ToString());
                    cmd.Parameters.AddWithValue("@mtg", cboTonGiao.SelectedValue.ToString());
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

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            { MessageBox.Show("Chưa chọn đúng ID để xóa."); return; }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                using (var cmd = new SqlCommand("DELETE FROM SinhVien WHERE IDSinhVien=@id", c.conn))
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
            { MessageBox.Show("Không thể xóa: sinh viên đang được tham chiếu."); }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgvSinhVien.Rows.Count > 0)
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                var ws = wb.Worksheets.Add("SinhVien");

                                ws.Cell(1, 1).Value = "Danh sách sinh viên";
                                ws.Range(1, 1, 1, dgvSinhVien.Columns.Count).Merge();
                                ws.Range(1, 1, 1, dgvSinhVien.Columns.Count)
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                    .Font.SetBold()
                                    .Font.SetFontName("Times New Roman")
                                    .Font.SetFontSize(12);

                                for (int i = 0; i < dgvSinhVien.Columns.Count; i++)
                                {
                                    ws.Cell(2, i + 1).Value = dgvSinhVien.Columns[i].HeaderText;
                                }


                                for (int i = 0; i < dgvSinhVien.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dgvSinhVien.Columns.Count; j++)
                                    {
                                        ws.Cell(i + 3, j + 1).Value = dgvSinhVien.Rows[i].Cells[j].Value?.ToString();
                                    }
                                }


                                var range = ws.Range(1, 1, dgvSinhVien.Rows.Count + 1, dgvSinhVien.Columns.Count);
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

        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"
                    SELECT  IDSinhVien AS ID,
                            MaSV, Ho, Ten,
                            CONVERT(date, NgaySinh) AS NgaySinh,
                            GioiTinh, Email, DienThoai,
                            MaLop, MaDanToc, MaTonGiao,
                            ISNULL(GhiChu,N'') AS GhiChu
                    FROM SinhVien
                    ORDER BY IDSinhVien DESC";

                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable(); da.Fill(dt);

                    dgvSinhVien.AutoGenerateColumns = false;
                    dgvSinhVien.DataSource = dt;

                    dgvSinhVien.Columns["Column1"].DataPropertyName = "ID";
                    dgvSinhVien.Columns["Column2"].DataPropertyName = "MaSV";
                    dgvSinhVien.Columns["Column3"].DataPropertyName = "Ho";
                    dgvSinhVien.Columns["Column4"].DataPropertyName = "Ten";
                    dgvSinhVien.Columns["Column5"].DataPropertyName = "NgaySinh";
                    dgvSinhVien.Columns["Column6"].DataPropertyName = "GioiTinh";
                    dgvSinhVien.Columns["Column7"].DataPropertyName = "Email";
                    dgvSinhVien.Columns["Column8"].DataPropertyName = "DienThoai";
                    dgvSinhVien.Columns["Column9"].DataPropertyName = "MaLop";
                    dgvSinhVien.Columns["Column10"].DataPropertyName = "MaDanToc";
                    dgvSinhVien.Columns["Column11"].DataPropertyName = "MaTonGiao";
                    dgvSinhVien.Columns["Column12"].DataPropertyName = "GhiChu";

                   
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
            finally { c.disconnect(); }
        }
        private void ClearForm()
        {
           txtID.Clear();
            txtMaSV.Clear();
            txtHo.Clear();
            txtTen.Clear();
            txtEmail.Clear();
            txtGhiChu.Clear();
            rdbNam.Checked = true; rdbNu.Checked = false;
            dtpNgaySinh.Value = DateTime.Today;
            nudDienThoai.Value = 0;
            cboLop.SelectedIndex = -1;
            cboDanToc.SelectedIndex = -1;
            cboTonGiao.SelectedIndex = -1;
            if (dgvSinhVien.Rows.Count > 0) dgvSinhVien.ClearSelection();
            txtMaSV.Focus();
        }
    }
}
