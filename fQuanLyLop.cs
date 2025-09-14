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
    public partial class fQuanLyLop : Form
    {
        connectData c = new connectData();
        DataTable dtNganh;
        public fQuanLyLop()
        { 
            InitializeComponent();

        }
        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT IDLop AS ID, MaLop, TenLop, MaNganh, GhiChu
                               FROM Lop ORDER BY IDLop DESC";

                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvLop.AutoGenerateColumns = false;
                    dgvLop.DataSource = dt;

                    dgvLop.Columns["Column1"].DataPropertyName = "ID";
                    dgvLop.Columns["Column2"].DataPropertyName = "MaLop";
                    dgvLop.Columns["Column3"].DataPropertyName = "TenLop";
                    dgvLop.Columns["Column4"].DataPropertyName = "MaNganh";
                    dgvLop.Columns["Column5"].DataPropertyName = "GhiChu";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally { c.disconnect(); }
        }
        private void ClearForm()
        {
            txtID.Clear();
            txtMaLop.Clear();
            txtTenLop.Clear();
            txtGhiChu.Clear();
            CboNganh.SelectedIndex = -1;
            if (dgvLop.Rows.Count > 0) dgvLop.ClearSelection();
            txtMaLop.Focus();
        }

        private void dgvLop_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int i = dgvLop.CurrentRow.Index;

            txtID.Text = dgvLop.Rows[i].Cells["Column1"].Value?.ToString();
            txtMaLop.Text = dgvLop.Rows[i].Cells["Column2"].Value?.ToString();
            txtTenLop.Text = dgvLop.Rows[i].Cells["Column3"].Value?.ToString();

            var maNganh = dgvLop.Rows[i].Cells["Column4"].Value?.ToString();   
            if (!string.IsNullOrEmpty(maNganh)) CboNganh.SelectedValue = maNganh;
            else CboNganh.SelectedIndex = -1;

            txtGhiChu.Text = dgvLop.Rows[i].Cells["Column5"].Value?.ToString();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaLop.Text) ||
        string.IsNullOrWhiteSpace(txtTenLop.Text) ||
       CboNganh.SelectedValue == null)
            { MessageBox.Show("Nhập Mã lớp, Tên lớp và chọn Mã ngành."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand("SELECT COUNT(1) FROM Lop WHERE MaLop=@ma", c.conn))
                {
                    chk.Parameters.AddWithValue("@ma", txtMaLop.Text.Trim());
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã lớp đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    "INSERT INTO Lop(MaLop, TenLop, MaNganh, GhiChu) VALUES(@ml, @tl, @mn, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ml", txtMaLop.Text.Trim());
                    cmd.Parameters.AddWithValue("@tl", txtTenLop.Text.Trim());
                    cmd.Parameters.AddWithValue("@mn", CboNganh.SelectedValue.ToString()); 
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

        private void fQuanLyLop_Load(object sender, EventArgs e)
        {
            LoadCboNganh();
            LoadGrid();
        }
        private void LoadCboNganh()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                dtNganh = new DataTable();
                string sql = @"SELECT MaNganh, TenNganh FROM Nganh ORDER BY TenNganh";
                using (var da = new SqlDataAdapter(sql, c.conn)) da.Fill(dtNganh);

                CboNganh.DropDownStyle = ComboBoxStyle.DropDownList;
                CboNganh.DataSource = dtNganh;
                CboNganh.DisplayMember = "TenNganh";
                CboNganh.ValueMember = "MaNganh";   
                CboNganh.SelectedIndex = -1;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp ngành: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            { MessageBox.Show("Chưa chọn đúng ID để sửa."); return; }
            if (string.IsNullOrWhiteSpace(txtMaLop.Text) ||
                string.IsNullOrWhiteSpace(txtTenLop.Text) ||
                CboNganh.SelectedValue == null)
            { MessageBox.Show("Nhập đủ thông tin."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand("SELECT COUNT(1) FROM Lop WHERE MaLop=@ma AND IDLop<>@id", c.conn))
                {
                    chk.Parameters.AddWithValue("@ma", txtMaLop.Text.Trim());
                    chk.Parameters.AddWithValue("@id", id);
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã lớp đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE Lop SET MaLop=@ml, TenLop=@tl, MaNganh=@mn, GhiChu=@gc
              WHERE IDLop=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ml", txtMaLop.Text.Trim());
                    cmd.Parameters.AddWithValue("@tl", txtTenLop.Text.Trim());
                    cmd.Parameters.AddWithValue("@mn", CboNganh.SelectedValue.ToString()); 
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

                using (var cmd = new SqlCommand("DELETE FROM Lop WHERE IDLop=@id", c.conn))
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
            { MessageBox.Show("Không thể xóa: lớp đang được tham chiếu (Sinh viên/Điểm...)."); }
            catch (Exception ex)
            { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
    }
    
}
