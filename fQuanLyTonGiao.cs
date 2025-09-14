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
    public partial class fQuanLyTonGiao : Form
    {
        connectData c = new connectData();
        public fQuanLyTonGiao()
        {
            InitializeComponent();
        }

        private void fQuanLyTonGiao_Load(object sender, EventArgs e)
        {
            LoadGrid();
        }
        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT IDTonGiao AS ID, MaTonGiao, TenTonGiao, GhiChu
                               FROM TonGiao ORDER BY IDTonGiao DESC";

                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                    dgvTonGiao.AutoGenerateColumns = false;
                    dgvTonGiao.DataSource = dt;

                 
                    dgvTonGiao.Columns["Column1"].DataPropertyName = "ID";
                    dgvTonGiao.Columns["Column2"].DataPropertyName = "MaTonGiao";
                    dgvTonGiao.Columns["Column3"].DataPropertyName = "TenTonGiao";
                    dgvTonGiao.Columns["Column4"].DataPropertyName = "GhiChu";
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
            txtMaTonGiao.Clear();
            txtTenTonGiao.Clear();
            txtGhiChu.Clear();
            if (dgvTonGiao.Rows.Count > 0) dgvTonGiao.ClearSelection();
            txtMaTonGiao.Focus();
        }

        private void dgvTonGiao_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int i = dgvTonGiao.CurrentRow.Index;

            txtID.Text = dgvTonGiao.Rows[i].Cells[0].Value?.ToString();
            txtMaTonGiao.Text = dgvTonGiao.Rows[i].Cells[1].Value?.ToString();
            txtTenTonGiao.Text = dgvTonGiao.Rows[i].Cells[2].Value?.ToString();
            txtGhiChu.Text = dgvTonGiao.Rows[i].Cells[3].Value?.ToString();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTonGiao.Text) ||
                string.IsNullOrWhiteSpace(txtTenTonGiao.Text))
            { MessageBox.Show("Nhập Mã tôn giáo và Tên tôn giáo."); return; }

            try
            {
                c.connect();

                // Chống trùng mã
                using (var chk = new SqlCommand("SELECT COUNT(1) FROM TonGiao WHERE MaTonGiao=@ma", c.conn))
                {
                    chk.Parameters.AddWithValue("@ma", txtMaTonGiao.Text.Trim());
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã tôn giáo đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    "INSERT INTO TonGiao(MaTonGiao, TenTonGiao, GhiChu) VALUES(@ma, @ten, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ma", txtMaTonGiao.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenTonGiao.Text.Trim());
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

            if (string.IsNullOrWhiteSpace(txtMaTonGiao.Text) ||
                string.IsNullOrWhiteSpace(txtTenTonGiao.Text))
            { MessageBox.Show("Nhập đủ thông tin."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand(
                    "SELECT COUNT(1) FROM TonGiao WHERE MaTonGiao=@ma AND IDTonGiao<>@id", c.conn))
                {
                    chk.Parameters.AddWithValue("@ma", txtMaTonGiao.Text.Trim());
                    chk.Parameters.AddWithValue("@id", id);
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã tôn giáo đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE TonGiao SET MaTonGiao=@ma, TenTonGiao=@ten, GhiChu=@gc
                      WHERE IDTonGiao=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ma", txtMaTonGiao.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenTonGiao.Text.Trim());
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

                using (var cmd = new SqlCommand("DELETE FROM TonGiao WHERE IDTonGiao=@id", c.conn))
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
    }
}
