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
    public partial class fQuanLyMonHoc : Form
    {
        connectData c = new connectData();
        DataTable dtNganh;
        public fQuanLyMonHoc()
        {
            InitializeComponent();
        }

        private void fQuanLyMonHoc_Load(object sender, EventArgs e)
        {
            LoadCboNganh();
            LoadGrid();
        }
        private void LoadCboNganh()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT MaNganh, TenNganh FROM Nganh ORDER BY TenNganh";
                dtNganh = new DataTable();
                using (var da = new SqlDataAdapter(sql, c.conn)) da.Fill(dtNganh);

                cboMaNganh.DropDownStyle = ComboBoxStyle.DropDownList;
                cboMaNganh.DataSource = dtNganh;
                cboMaNganh.DisplayMember = "TenNganh";
                cboMaNganh.ValueMember = "MaNganh";   
                cboMaNganh.SelectedIndex = -1;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp ngành: " + ex.Message); }
            finally { c.disconnect(); }
        }
        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT IDMonHoc AS ID, MaMon, TenMon, SoTinChi, MaNganh, GhiChu
                               FROM MonHoc ORDER BY IDMonHoc DESC";
                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvMonHoc.AutoGenerateColumns = false;  
                    dgvMonHoc.DataSource = dt;

                    dgvMonHoc.Columns["Column1"].DataPropertyName = "ID";
                    dgvMonHoc.Columns["Column2"].DataPropertyName = "MaMon";
                    dgvMonHoc.Columns["Column3"].DataPropertyName = "TenMon";
                    dgvMonHoc.Columns["Column4"].DataPropertyName = "SoTinChi";
                    dgvMonHoc.Columns["Column5"].DataPropertyName = "MaNganh";
                    dgvMonHoc.Columns["Column6"].DataPropertyName = "GhiChu";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
            finally { c.disconnect(); }
        }
        private void ClearForm()
        {
            txtID.Clear();
            txtMaMon.Clear();
            txtTenMon.Clear();
            txtGhiChu.Clear();
            nudSoTinChi.Value = 0;
            cboMaNganh.SelectedIndex = -1;
            if (dgvMonHoc.Rows.Count > 0) dgvMonHoc.ClearSelection();
            txtMaMon.Focus();
        }
        private void dgvMonHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int i = dgvMonHoc.CurrentRow.Index;

            
            txtID.Text = dgvMonHoc.Rows[i].Cells[0].Value?.ToString();
            txtMaMon.Text = dgvMonHoc.Rows[i].Cells[1].Value?.ToString();
            txtTenMon.Text = dgvMonHoc.Rows[i].Cells[2].Value?.ToString();

            var stc = dgvMonHoc.Rows[i].Cells[3].Value?.ToString();
            if (int.TryParse(stc, out var so)) nudSoTinChi.Value = so; else nudSoTinChi.Value = 0;

            var mn = dgvMonHoc.Rows[i].Cells[4].Value?.ToString();
            if (!string.IsNullOrEmpty(mn)) cboMaNganh.SelectedValue = mn;
            else cboMaNganh.SelectedIndex = -1;
            txtGhiChu.Text = dgvMonHoc.Rows[i].Cells[5].Value?.ToString();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaMon.Text) ||
                string.IsNullOrWhiteSpace(txtTenMon.Text) ||
                cboMaNganh.SelectedValue == null)
            { MessageBox.Show("Nhập Mã môn, Tên môn và chọn Mã ngành."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand("SELECT COUNT(1) FROM MonHoc WHERE MaMon=@m", c.conn))
                {
                    chk.Parameters.AddWithValue("@m", txtMaMon.Text.Trim());
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã môn đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    "INSERT INTO MonHoc(MaMon, TenMon, SoTinChi, MaNganh, GhiChu) VALUES(@m, @ten, @stc, @mn, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@m", txtMaMon.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenMon.Text.Trim());
                    cmd.Parameters.Add("@stc", SqlDbType.Int).Value = (int)nudSoTinChi.Value;
                    cmd.Parameters.AddWithValue("@mn", cboMaNganh.SelectedValue.ToString()); 
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
            if (string.IsNullOrWhiteSpace(txtMaMon.Text) ||
                string.IsNullOrWhiteSpace(txtTenMon.Text) ||
                cboMaNganh.SelectedValue == null)
            { MessageBox.Show("Nhập đủ thông tin."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand("SELECT COUNT(1) FROM MonHoc WHERE MaMon=@m AND IDMonHoc<>@id", c.conn))
                {
                    chk.Parameters.AddWithValue("@m", txtMaMon.Text.Trim());
                    chk.Parameters.AddWithValue("@id", id);
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã môn đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE MonHoc SET MaMon=@m, TenMon=@ten, SoTinChi=@stc, MaNganh=@mn, GhiChu=@gc
                      WHERE IDMonHoc=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@m", txtMaMon.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenMon.Text.Trim());
                    cmd.Parameters.Add("@stc", SqlDbType.Int).Value = (int)nudSoTinChi.Value;
                    cmd.Parameters.AddWithValue("@mn", cboMaNganh.SelectedValue.ToString()); 
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

                using (var cmd = new SqlCommand("DELETE FROM MonHoc WHERE IDMonHoc=@id", c.conn))
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
            { MessageBox.Show("Không thể xóa: môn học đang được tham chiếu (Điểm/Giảng viên...)."); }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
    }
}
