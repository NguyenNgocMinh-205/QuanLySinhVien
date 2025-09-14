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
    public partial class fQuanLyDiaChi : Form
    {
        connectData c = new connectData();
        public fQuanLyDiaChi()
        {
            InitializeComponent();
        }

        private void fQuanLyDiaChi_Load(object sender, EventArgs e)
        {
            LoadGrid();
        }
        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"
                    SELECT  IDDiaChi AS ID,
                            MaSV, Loai, Xom, Xa, Tinh,
                            ISNULL(GhiChu,N'') AS GhiChu
                    FROM DiaChi
                    ORDER BY IDDiaChi DESC";

                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                    dgvDiaChi.AutoGenerateColumns = false; 
                    dgvDiaChi.DataSource = dt;

                   
                    dgvDiaChi.Columns["Column1"].DataPropertyName = "ID";
                    dgvDiaChi.Columns["Column2"].DataPropertyName = "MaSV";
                    dgvDiaChi.Columns["Column3"].DataPropertyName = "Loai";
                    dgvDiaChi.Columns["Column4"].DataPropertyName = "Xom";
                    dgvDiaChi.Columns["Column5"].DataPropertyName = "Xa";
                    dgvDiaChi.Columns["Column6"].DataPropertyName = "Tinh";
                    dgvDiaChi.Columns["Column7"].DataPropertyName = "GhiChu";
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
            txtMaSV.Clear();
            txtLoai.Clear();
            txtXom.Clear();
            txtXa.Clear();
            txtTinh.Clear();
            txtGhiChu.Clear();

            if (dgvDiaChi.Rows.Count > 0) dgvDiaChi.ClearSelection();
            txtMaSV.Focus();
        }
        private bool SinhVienExists(string maSv)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM SinhVien WHERE MaSV=@m", c.conn))
            {
                cmd.Parameters.AddWithValue("@m", maSv);
                var n = (int)cmd.ExecuteScalar();
                return n > 0;
            }
        }

        private void dgvDiaChi_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var r = dgvDiaChi.Rows[e.RowIndex];

            txtID.Text = r.Cells["Column1"].Value?.ToString();
            txtMaSV.Text = r.Cells["Column2"].Value?.ToString();
            txtLoai.Text = r.Cells["Column3"].Value?.ToString();
            txtXom.Text = r.Cells["Column4"].Value?.ToString();
            txtXa.Text = r.Cells["Column5"].Value?.ToString();
            txtTinh.Text = r.Cells["Column6"].Value?.ToString();
            txtGhiChu.Text = r.Cells["Column7"].Value?.ToString();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                MessageBox.Show("Nhập Mã sinh viên.");
                return;
            }

            try
            {
                c.connect();

                // Kiểm tra MaSV có tồn tại không -> báo rõ cho người dùng
                if (!SinhVienExists(txtMaSV.Text.Trim()))
                {
                    MessageBox.Show("Mã sinh viên không tồn tại trong hệ thống.");
                    return;
                }

                using (var cmd = new SqlCommand(
                    @"INSERT INTO DiaChi(MaSV, Loai, Xom, Xa, Tinh, GhiChu)
                      VALUES(@sv, @loai, @xom, @xa, @tinh, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@sv", txtMaSV.Text.Trim());
                    cmd.Parameters.AddWithValue("@loai", (object)txtLoai.Text?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@xom", (object)txtXom.Text?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@xa", (object)txtXa.Text?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tinh", (object)txtTinh.Text?.Trim() ?? DBNull.Value);
                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text.Trim();

                    cmd.ExecuteNonQuery();
                }

                LoadGrid();
                ClearForm();
                MessageBox.Show("Thêm thành công!");
            }
            catch (SqlException sx) when (sx.Number == 547)
            {
                // Trường hợp FK MaSV vi phạm
                MessageBox.Show("Không thể thêm: Mã sinh viên không tồn tại hoặc bị ràng buộc.");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id))
            {
                MessageBox.Show("Chưa chọn đúng ID để sửa.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                MessageBox.Show("Nhập Mã sinh viên.");
                return;
            }

            try
            {
                c.connect();

                if (!SinhVienExists(txtMaSV.Text.Trim()))
                {
                    MessageBox.Show("Mã sinh viên không tồn tại trong hệ thống.");
                    return;
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE DiaChi SET
                          MaSV=@sv, Loai=@loai, Xom=@xom, Xa=@xa, Tinh=@tinh, GhiChu=@gc
                      WHERE IDDiaChi=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@sv", txtMaSV.Text.Trim());
                    cmd.Parameters.AddWithValue("@loai", (object)txtLoai.Text?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@xom", (object)txtXom.Text?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@xa", (object)txtXa.Text?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tinh", (object)txtTinh.Text?.Trim() ?? DBNull.Value);
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
            {
                MessageBox.Show("Chưa chọn đúng ID để xóa.");
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                using (var cmd = new SqlCommand("DELETE FROM DiaChi WHERE IDDiaChi=@id", c.conn))
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
            {
                MessageBox.Show("Không thể xóa: bản ghi đang được tham chiếu.");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
    }
}
