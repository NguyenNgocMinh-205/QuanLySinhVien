using QLSV;
using QuanLySinhVien.GUI;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QuanLySinhVien
{
    public partial class fDangNhap : Form
    {
        private readonly connectData c = new connectData();

        public fDangNhap()
        {
            InitializeComponent();
            txtMatKhau.UseSystemPasswordChar = true;     // ẩn mật khẩu
            this.AcceptButton = btnDangNhap;             // Enter = Đăng nhập
        }

        private void chkHienthiMatKhau_CheckedChanged(object sender, EventArgs e)
        {
            txtMatKhau.UseSystemPasswordChar = !chkHienthiMatKhau.Checked;
        }

        // Nút Đăng nhập (button1 / btnDangNhap)
        private void button1_Click(object sender, EventArgs e)
        {
            string user = txtTenDangNhap.Text.Trim();
            string pass = txtMatKhau.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Thông báo");
                return;
            }

            try
            {
                c.connect();

                using (var cmd = new SqlCommand(
                    "SELECT TenDangNhap, MatKhau, LoaiTaiKhoan FROM TaiKhoan WHERE TenDangNhap = @u", c.conn))
                {
                    cmd.Parameters.AddWithValue("@u", user);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            string dbPass = rd["MatKhau"]?.ToString() ?? "";
                            if (string.Equals(dbPass, pass))
                            {
                                // Lưu phiên đăng nhập
                                CurrentUser.Username = rd["TenDangNhap"].ToString();
                                CurrentUser.Role = RoleUtils.NormalizeRole(rd["LoaiTaiKhoan"]?.ToString());

                                // Mở trang chủ và áp quyền trong fTrangChu_Load (ApplyRole() không tham số)
                                Hide();
                                var main = new fTrangChu();
                                main.Show();                          // fTrangChu_Load sẽ tự gọi ApplyRole()
                                main.FormClosed += (_, __) => Close();
                                return;
                            }
                        }
                    }
                }

                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message, "Thông báo");
            }
            finally { c.disconnect(); }
        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // no-op
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // no-op
        }

    }
}
    
