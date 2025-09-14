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
    public partial class fQuanLyDanToc : Form
    {
        connectData c = new connectData();
        public fQuanLyDanToc()
        {
            InitializeComponent();
        }
       
       
        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT IDDanToc AS ID, MaDanToc, TenDanToc, GhiChu
                               FROM DanToc ORDER BY IDDanToc DESC";

                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                    dgvDanToc.AutoGenerateColumns = false;
                    dgvDanToc.DataSource = dt;

                  
                    dgvDanToc.Columns["Column1"].DataPropertyName = "ID";
                    dgvDanToc.Columns["Column2"].DataPropertyName = "MaDanToc";
                    dgvDanToc.Columns["Column3"].DataPropertyName = "TenDanToc";
                    dgvDanToc.Columns["Column4"].DataPropertyName = "GhiChu";
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
            txtMaDanToc.Clear();
            txtTenDanToc.Clear();
            txtGhiChu.Clear();
            if (dgvDanToc.Rows.Count > 0) dgvDanToc.ClearSelection();
            txtMaDanToc.Focus();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dgvDanToc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int i = dgvDanToc.CurrentRow.Index;

            txtID.Text = dgvDanToc.Rows[i].Cells[0].Value?.ToString();
            txtMaDanToc.Text = dgvDanToc.Rows[i].Cells[1].Value?.ToString();
            txtTenDanToc.Text = dgvDanToc.Rows[i].Cells[2].Value?.ToString();
            txtGhiChu.Text = dgvDanToc.Rows[i].Cells[3].Value?.ToString();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaDanToc.Text) ||
                string.IsNullOrWhiteSpace(txtTenDanToc.Text))
            { MessageBox.Show("Nhập Mã dân tộc và Tên dân tộc."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand("SELECT COUNT(1) FROM DanToc WHERE MaDanToc=@ma", c.conn))
                {
                    chk.Parameters.AddWithValue("@ma", txtMaDanToc.Text.Trim());
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã dân tộc đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    "INSERT INTO DanToc(MaDanToc, TenDanToc, GhiChu) VALUES(@ma, @ten, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ma", txtMaDanToc.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenDanToc.Text.Trim());
                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar, 500).Value = (object)txtGhiChu.Text ?? "";
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

            if (string.IsNullOrWhiteSpace(txtMaDanToc.Text) ||
                string.IsNullOrWhiteSpace(txtTenDanToc.Text))
            { MessageBox.Show("Nhập đủ thông tin."); return; }

            try
            {
                c.connect();

                using (var chk = new SqlCommand(
                    "SELECT COUNT(1) FROM DanToc WHERE MaDanToc=@ma AND IDDanToc<>@id", c.conn))
                {
                    chk.Parameters.AddWithValue("@ma", txtMaDanToc.Text.Trim());
                    chk.Parameters.AddWithValue("@id", id);
                    if ((int)chk.ExecuteScalar() > 0) { MessageBox.Show("Mã dân tộc đã tồn tại."); return; }
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE DanToc SET MaDanToc=@ma, TenDanToc=@ten, GhiChu=@gc
                      WHERE IDDanToc=@id", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ma", txtMaDanToc.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenDanToc.Text.Trim());
                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar, 500).Value = (object)txtGhiChu.Text ?? "";
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

                using (var cmd = new SqlCommand("DELETE FROM DanToc WHERE IDDanToc=@id", c.conn))
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

        private void fQuanLyDanToc_Load(object sender, EventArgs e)
        {
           LoadGrid();
        }
    }
}
