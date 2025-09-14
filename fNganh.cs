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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QuanLySinhVien.GUI
{
    
    public partial class fNganh : Form
    {
        connectData c = new connectData();
        public fNganh()
        {
            InitializeComponent();
        }
        private void clear_form()
        {
            txtID.Clear();
            txtMaNganh.Clear();
            txtTenNganh.Clear();
            txtGhiChu.Clear();
            cboMaKhoaHoc.SelectedIndex = -1;
            if (dgvNganh.Rows.Count > 0) dgvNganh.ClearSelection();
            txtMaNganh.Focus();
        }
        private void loaddata()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                string sql = @"SELECT IDNganh AS ID, MaNganh, TenNganh, MaKhoaHoc, GhiChu
                               FROM Nganh ORDER BY IDNganh DESC";

                using (SqlDataAdapter da = new SqlDataAdapter(sql, c.conn))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvNganh.AutoGenerateColumns = false;
                    dgvNganh.DataSource = dt;

                    dgvNganh.Columns["ID"].DataPropertyName = "ID";
                    dgvNganh.Columns["MaNganh"].DataPropertyName = "MaNganh";
                    dgvNganh.Columns["TenNganh"].DataPropertyName = "TenNganh";
                    dgvNganh.Columns["MaKhoaHoc"].DataPropertyName = "MaKhoaHoc";
                    dgvNganh.Columns["GhiChu"].DataPropertyName = "GhiChu";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally { c.disconnect(); }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void LoadCboMaKhoaHoc()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

               
                string sql = @"
            SELECT 
                IDKhoaHoc,
                MaKhoaHoc,
                CAST(MaKhoaHoc AS nvarchar(20)) + N' - ' 
                + CAST(NamBatDau AS nvarchar(4)) + N'→' + CAST(NamKetThuc AS nvarchar(4)) AS TenHienThi
            FROM KhoaHoc
            ORDER BY MaKhoaHoc";

                using (var da = new SqlDataAdapter(sql, c.conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                   
                    cboMaKhoaHoc.DataSource = dt;
                    cboMaKhoaHoc.DisplayMember = "TenHienThi";
                    cboMaKhoaHoc.ValueMember = "MaKhoaHoc";     
                    cboMaKhoaHoc.SelectedIndex = -1;            
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp ComboBox: " + ex.Message);
            }
            finally { c.disconnect(); }
        }

        private void fNganh_Load(object sender, EventArgs e)
        {
            LoadCboMaKhoaHoc();
            loaddata();
        }

        private void dgvNganh_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
                if (e.RowIndex < 0) return;
                int i = dgvNganh.CurrentRow.Index;
                txtID.Text = dgvNganh.Rows[i].Cells[0].Value?.ToString();
                txtMaNganh.Text = dgvNganh.Rows[i].Cells[1].Value?.ToString();
                txtTenNganh.Text = dgvNganh.Rows[i].Cells[2].Value?.ToString();
                var mk = dgvNganh.Rows[i].Cells[3].Value?.ToString();  
                if (!string.IsNullOrEmpty(mk)) cboMaKhoaHoc.SelectedValue = mk;
                else cboMaKhoaHoc.SelectedIndex = -1;
                txtGhiChu.Text = dgvNganh.Rows[i].Cells[4].Value?.ToString();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNganh.Text) ||
        string.IsNullOrWhiteSpace(txtTenNganh.Text) ||
        cboMaKhoaHoc.SelectedValue == null)
            { MessageBox.Show("Nhập Mã ngành, Tên ngành và chọn Mã khóa học."); return; }

            try

            {
                c.connect();
                using (var cmd = new SqlCommand(
                    "INSERT INTO Nganh(MaNganh, TenNganh, MaKhoaHoc, GhiChu) VALUES(@ma, @ten, @mk, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ma", txtMaNganh.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenNganh.Text.Trim());
                    cmd.Parameters.AddWithValue("@mk", cboMaKhoaHoc.SelectedValue.ToString()); 

                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text.Trim();

                    cmd.ExecuteNonQuery();
                }
                loaddata();
                clear_form();
                MessageBox.Show("Thêm thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
            finally { c.disconnect(); }
        }
        

        private string SafeN(string v)
        {
            throw new NotImplementedException();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNganh.Text) || cboMaKhoaHoc.SelectedValue == null)
            { MessageBox.Show("Chọn/nhập đủ thông tin."); return; }

            try
            {
                c.connect();
                using (var cmd = new SqlCommand(
                    "UPDATE Nganh SET TenNganh=@ten, MaKhoaHoc=@mk, GhiChu=@gc WHERE MaNganh=@ma", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ma", txtMaNganh.Text.Trim());
                    cmd.Parameters.AddWithValue("@ten", txtTenNganh.Text.Trim());
                    cmd.Parameters.AddWithValue("@mk", cboMaKhoaHoc.SelectedValue.ToString()); 
                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text.Trim();

                    cmd.ExecuteNonQuery();
                }
                loaddata();
                MessageBox.Show("Sửa thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi sửa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text.Trim(), out int id)) 
            { MessageBox.Show("Chưa chọn đúng ID để xóa.", "Thông báo"); return; }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                using (var cmd = new SqlCommand("DELETE FROM Nganh WHERE IDNganh=@id", c.conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    int n = cmd.ExecuteNonQuery();

                    if (n > 0)
                    {
                        MessageBox.Show("Xóa thành công!!");
                        loaddata();
                        clear_form();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy ID " + id + " để xóa.");
                    }
                }
            }
            catch (SqlException sx) when (sx.Number == 547)
            { MessageBox.Show("Không thể xóa: bản ghi đang được tham chiếu (Ngành/Lớp...)."); }
            catch (Exception ex)
            { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { c.disconnect(); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            clear_form();
        }
    }
}
