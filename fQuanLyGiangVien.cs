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
    public partial class fQuanLyGiangVien : Form
    {
        connectData c = new connectData();
        public fQuanLyGiangVien()
        {
            InitializeComponent();
        }

        private void fQuanLyGiangVien_Load(object sender, EventArgs e)
        {
            LoadCboMon();
            LoadGrid();
        }
        private void LoadCboMon()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT MaMon, TenMon FROM MonHoc ORDER BY TenMon";
                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable(); da.Fill(dt);
                    cboMaMon.DataSource = dt;
                    cboMaMon.DisplayMember = "TenMon";
                    cboMaMon.ValueMember = "MaMon";
                    cboMaMon.SelectedIndex = -1;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp mã môn: " + ex.Message); }
            finally { c.disconnect(); }
        }
        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT IDGiaoVien AS ID, MaGV, Ho, Ten, DienThoai, Email, MaMon, GhiChu
                               FROM GiaoVien ORDER BY IDGiaoVien DESC";
                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable(); da.Fill(dt);
                    dgvGiangVien.AutoGenerateColumns = false;
                    dgvGiangVien.DataSource = dt;

                    dgvGiangVien.Columns["Column1"].DataPropertyName = "ID";
                    dgvGiangVien.Columns["Column2"].DataPropertyName = "MaGV";
                    dgvGiangVien.Columns["Column3"].DataPropertyName = "Ho";
                    dgvGiangVien.Columns["Column4"].DataPropertyName = "Ten";
                    dgvGiangVien.Columns["Column5"].DataPropertyName = "DienThoai";
                    dgvGiangVien.Columns["Column6"].DataPropertyName = "Email";
                    dgvGiangVien.Columns["Column7"].DataPropertyName = "MaMon";
                    dgvGiangVien.Columns["GhiChu"].DataPropertyName = "GhiChu";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
            finally { c.disconnect(); }
        }
        private void ClearForm()
        {
            txtID.Clear();
            txtMaGV.Clear();
            txtHo.Clear();
            txtTen.Clear();
            txtEmail.Clear();
            textBox8.Clear();
            nudSoDIenThoai.Value = 0;
            cboMaMon.SelectedIndex = -1;

            if (dgvGiangVien.Rows.Count > 0) dgvGiangVien.ClearSelection();
            txtMaGV.Focus();
        }

        private void dgvGiangVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int i = dgvGiangVien.CurrentRow.Index;


            txtID.Text = dgvGiangVien.Rows[i].Cells[0].Value?.ToString();
            txtMaGV.Text = dgvGiangVien.Rows[i].Cells[1].Value?.ToString();
            txtHo.Text = dgvGiangVien.Rows[i].Cells[2].Value?.ToString();
            txtTen.Text = dgvGiangVien.Rows[i].Cells[3].Value?.ToString();

            var sdt = dgvGiangVien.Rows[i].Cells[4].Value?.ToString();
            if (decimal.TryParse(sdt, out var phone)) nudSoDIenThoai.Value = phone; else nudSoDIenThoai.Value = 0;

            txtEmail.Text = dgvGiangVien.Rows[i].Cells[5].Value?.ToString();

            var mamon = dgvGiangVien.Rows[i].Cells[6].Value?.ToString();
            if (mamon != null) cboMaMon.SelectedValue = mamon;
            textBox8.Text = dgvGiangVien.Rows[i].Cells[7].Value?.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaGV.Text) ||
                string.IsNullOrWhiteSpace(txtHo.Text) ||
                string.IsNullOrWhiteSpace(txtTen.Text) ||
                cboMaMon.SelectedValue == null)
            { MessageBox.Show("Nhập Mã GV, Họ, Tên và chọn Mã môn."); return; }

            try
            {
                c.connect();


                using (var chk = new SqlCommand("SELECT COUNT(1) FROM GiaoVien WHERE MaGV=@magv", c.conn))
                {
                    chk.Parameters.AddWithValue("@magv", txtMaGV.Text.Trim());
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã GV đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    "INSERT INTO GiaoVien(MaGV, Ho, Ten, DienThoai, Email, MaMon, GhiChu) " +
                    "VALUES(@magv, @ho, @ten, @sdt, @email, @mamon, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@magv", txtMaGV.Text.Trim());
                    cmd.Parameters.AddWithValue("@ho", txtHo.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTen.Text.Trim());
                    cmd.Parameters.Add("@sdt", SqlDbType.NVarChar, 20).Value = nudSoDIenThoai.Value.ToString();
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@mamon", cboMaMon.SelectedValue.ToString());
                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar, 500).Value = (object)textBox8.Text ?? "";


                    cmd.ExecuteNonQuery();
                }

                LoadGrid();
                ClearForm();
                MessageBox.Show("Thêm thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            { MessageBox.Show("Chưa chọn đúng ID để sửa."); return; }

            if (string.IsNullOrWhiteSpace(txtMaGV.Text) ||
                string.IsNullOrWhiteSpace(txtHo.Text) ||
                string.IsNullOrWhiteSpace(txtTen.Text) ||
                cboMaMon.SelectedValue == null)
            { MessageBox.Show("Nhập đủ thông tin."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand(
                    "SELECT COUNT(1) FROM GiaoVien WHERE MaGV=@magv AND IDGiaoVien<>@id", c.conn))
                {
                    chk.Parameters.AddWithValue("@magv", txtMaGV.Text.Trim());
                    chk.Parameters.AddWithValue("@id", id);
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã GV đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE GiaoVien
                      SET MaGV=@magv, Ho=@ho, Ten=@ten, DienThoai=@sdt, Email=@email, MaMon=@mamon, GhiChu=@gc
                      WHERE IDGiaoVien=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@magv", txtMaGV.Text.Trim());
                    cmd.Parameters.AddWithValue("@ho", txtHo.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTen.Text.Trim());
                    cmd.Parameters.Add("@sdt", SqlDbType.NVarChar, 20).Value = nudSoDIenThoai.Value.ToString();
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@mamon", cboMaMon.SelectedValue.ToString());
                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar, 500).Value = (object)textBox8.Text ?? "";
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            { MessageBox.Show("Chưa chọn đúng ID để xóa."); return; }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                using (var cmd = new SqlCommand("DELETE FROM GiaoVien WHERE IDGiaoVien=@id", c.conn))
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
            { MessageBox.Show("Không thể xóa: giảng viên đang được tham chiếu (Môn dạy/Lịch...)."); }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void button9_Click(object sender, EventArgs e)
        {

            if (dgvGiangVien.Rows.Count > 0)
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                var ws = wb.Worksheets.Add("GiangVien");

                                ws.Cell(1, 1).Value = "Danh sách giảng viên";
                                ws.Range(1, 1, 1, dgvGiangVien.Columns.Count).Merge();
                                ws.Range(1, 1, 1, dgvGiangVien.Columns.Count)
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                    .Font.SetBold()
                                    .Font.SetFontName("Times New Roman")
                                    .Font.SetFontSize(12);

                                for (int i = 0; i < dgvGiangVien.Columns.Count; i++)
                                {
                                    ws.Cell(2, i + 1).Value = dgvGiangVien.Columns[i].HeaderText;
                                }

                                
                                for (int i = 0; i < dgvGiangVien.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dgvGiangVien.Columns.Count; j++)
                                    {
                                        ws.Cell(i + 3, j + 1).Value = dgvGiangVien.Rows[i].Cells[j].Value?.ToString();
                                    }
                                }

                                
                                var range = ws.Range(1, 1, dgvGiangVien.Rows.Count + 1, dgvGiangVien.Columns.Count);
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
