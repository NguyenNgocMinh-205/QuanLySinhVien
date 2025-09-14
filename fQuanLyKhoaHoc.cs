using QLSV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLySinhVien.GUI
{
    public partial class fQuanLyKhoaHoc : Form
    {
        connectData c = new connectData();

        public fQuanLyKhoaHoc()
        {
            InitializeComponent();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void fQuanLyKhoaHoc_Load(object sender, EventArgs e)
        {
            try
            {
                c.connect();
                Column1.DataPropertyName = "ID";          // hoặc "IDKhoaHoc" nếu không dùng alias
                Column2.DataPropertyName = "MaKhoaHoc";
                Column3.DataPropertyName = "NamBatDau";
                Column4.DataPropertyName = "NamKetThuc";
                Column5.DataPropertyName = "GhiChu";
                LoadGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally { c.disconnect(); }

        }
        private void ClearForm()
        {
            txtID.Text = "";
            txtMaKhoaHoc.Text = "";
            dtpBatDau.Value = DateTime.Today;
            dtpKetThuc.Value = DateTime.Today;
            txtGhiChu.Text = "";
            txtMaKhoaHoc.Focus();
        }

        private void LoadGrid()
        {
            string sql = @"SELECT IDKhoaHoc AS ID, MaKhoaHoc, NamBatDau, NamKetThuc, GhiChu
                           FROM KhoaHoc ORDER BY IDKhoaHoc DESC";

            using (SqlDataAdapter da = new SqlDataAdapter(sql, c.conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvKhoaHoc.DataSource = dt;
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtMaKhoaHoc.Text))
            {
                MessageBox.Show("Nhập Mã khóa học.");
                txtMaKhoaHoc.Focus();
                return false;
            }
            if (dtpKetThuc.Value.Date < dtpBatDau.Value.Date)
            {
                MessageBox.Show("Năm kết thúc phải >= năm bắt đầu.");
                return false;
            }
            return true;
        }
        // ---- Helpers --------------------------------------------------------
        private static string SafeN(string s)
        {
            return string.IsNullOrEmpty(s) ? "" : s.Replace("'", "''");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dtpKetThuc.Value.Year < dtpBatDau.Value.Year)
            {
                MessageBox.Show("Năm kết thúc phải >= năm bắt đầu"); return;
            }

            try
            {
                c.connect();
                using (var cmd = new SqlCommand(
                    "INSERT INTO KhoaHoc(MaKhoaHoc, NamBatDau, NamKetThuc, GhiChu) " +
                    "VALUES(@ma, @bd, @kt, @gc)", c.conn))
                {
                    cmd.Parameters.AddWithValue("@ma", txtMaKhoaHoc.Text.Trim());
                    cmd.Parameters.Add("@bd", SqlDbType.Int).Value = dtpBatDau.Value.Year;   // INT
                    cmd.Parameters.Add("@kt", SqlDbType.Int).Value = dtpKetThuc.Value.Year; // INT
                    cmd.Parameters.Add("@gc", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text.Trim();

                    cmd.ExecuteNonQuery();
                }
                LoadGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm (chi tiết): " + ex.Message);
            }
            finally { c.disconnect(); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "" || txtMaKhoaHoc.Text == "")
            {
                MessageBox.Show("Chưa nhập đủ thông tin", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (dtpKetThuc.Value.Year < dtpBatDau.Value.Year)
            {
                MessageBox.Show("Năm kết thúc phải >= năm bắt đầu", "Thông báo", MessageBoxButtons.OK);
                return;
            }

            // chuẩn hóa chuỗi để khỏi vỡ dấu nháy
            string ma = txtMaKhoaHoc.Text.Replace("'", "''");
            string gc = (txtGhiChu.Text ?? "").Replace("'", "''");
            string bd = dtpBatDau.Value.ToString("yyyy-MM-dd");
            string kt = dtpKetThuc.Value.ToString("yyyy-MM-dd");

            c.connect();
            string query =
                "UPDATE KhoaHoc SET " +
                "MaKhoaHoc = N'" + ma + "', " +
                "NamBatDau = " + bd + ", " +
                "NamKetThuc = " + kt + ", " +
                "GhiChu = N'" + gc + "' " +
                "WHERE IDKhoaHoc = " + textBox3.Text.Trim();

            bool kq = c.exeSQL(query);
            if (kq)
            {
                MessageBox.Show("Sửa thành công!!", "Thông báo", MessageBoxButtons.OK);
                LoadGrid();   
                ClearForm(); 
            }
            else
            {
                MessageBox.Show("Sửa thất bại!!", "Thông báo", MessageBoxButtons.OK);
            }
            c.disconnect();
        }

        private void dgvKhoaHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int i = dgvKhoaHoc.CurrentRow.Index;

            // Sắp xếp cột theo đúng thứ tự bạn đang hiển thị trên lưới:
            // 0: ID, 1: MaKhoaHoc, 2: NamBatDau, 3: NamKetThuc, 4: GhiChu
            textBox3.Text = dgvKhoaHoc.Rows[i].Cells[0].Value?.ToString();
            txtMaKhoaHoc.Text = dgvKhoaHoc.Rows[i].Cells[1].Value?.ToString();

            string bd = dgvKhoaHoc.Rows[i].Cells[2].Value?.ToString();
            string kt = dgvKhoaHoc.Rows[i].Cells[3].Value?.ToString();
            if (int.TryParse(bd, out var y1)) dtpBatDau.Value = new DateTime(y1, 1, 1);
            else if (DateTime.TryParse(bd, out var d1)) dtpBatDau.Value = d1;

            if (int.TryParse(kt, out var y2)) dtpKetThuc.Value = new DateTime(y2, 1, 1);
            else if (DateTime.TryParse(kt, out var d2)) dtpKetThuc.Value = d2;

            txtGhiChu.Text = dgvKhoaHoc.Rows[i].Cells[4].Value?.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox3.Text.Trim(), out int id))
            {
                MessageBox.Show("Chưa chọn đúng ID để xóa.", "Thông báo", MessageBoxButtons.OK);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                
                using (var cmd = new SqlCommand("DELETE FROM KhoaHoc WHERE IDKhoaHoc = @id", c.conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    int n = cmd.ExecuteNonQuery();   // số dòng bị xóa

                    if (n > 0)
                    {
                        MessageBox.Show("Xóa thành công!!", "Thông báo", MessageBoxButtons.OK);
                        LoadGrid();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy ID " + id + " để xóa.", "Thông báo");
                    }
                }
            }
            catch (SqlException sx) when (sx.Number == 547)
            {
                MessageBox.Show("Không thể xóa: bản ghi đang được tham chiếu (Ngành/Lớp...).", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message, "Thông báo");
            }
            finally { c.disconnect(); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
    }
    
}
