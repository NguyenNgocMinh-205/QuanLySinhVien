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
    public partial class fDoiMatKhau : Form
    {
        private readonly connectData c = new connectData();
        public fDoiMatKhau()
        {
            InitializeComponent();
            this.AcceptButton = btnCapNhat;
        }
        
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void fDoiMatKhau_Load(object sender, EventArgs e)
        {
            txtMatKhauCu.UseSystemPasswordChar = true;
            txtMatKhauMoi.UseSystemPasswordChar = true;
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            string username = CurrentUser.Username ?? "";
            string oldPass = txtMatKhauCu.Text;
            string newPass = txtMatKhauMoi.Text;

            if (string.IsNullOrWhiteSpace(username))
            { MessageBox.Show("Chưa xác định người dùng hiện tại."); return; }

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            { MessageBox.Show("Nhập đầy đủ Mật khẩu cũ và Mật khẩu mới."); return; }

            if (newPass.Length < 6)
            { MessageBox.Show("Mật khẩu mới phải từ 6 ký tự trở lên."); return; }

            if (newPass == oldPass)
            { MessageBox.Show("Mật khẩu mới không được trùng mật khẩu cũ."); return; }

            try
            {
                c.connect();

               
                string dbPass = null;
                using (var cmd = new SqlCommand(
                    "SELECT MatKhau FROM TaiKhoan WHERE TenDangNhap=@u", c.conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    var obj = cmd.ExecuteScalar();
                    dbPass = obj?.ToString();
                }

                if (dbPass == null)
                { MessageBox.Show("Tài khoản không tồn tại."); return; }

                if (!string.Equals(dbPass, oldPass))
                { MessageBox.Show("Mật khẩu cũ không đúng."); return; }

                
                using (var up = new SqlCommand(
                    "UPDATE TaiKhoan SET MatKhau=@m WHERE TenDangNhap=@u", c.conn))
                {
                    up.Parameters.AddWithValue("@u", username);
                    up.Parameters.AddWithValue("@m", newPass);

                    int n = up.ExecuteNonQuery();
                    if (n > 0)
                    {
                        MessageBox.Show("Đổi mật khẩu thành công!");
                        txtMatKhauCu.Clear();
                        txtMatKhauMoi.Clear();
                        this.DialogResult = DialogResult.OK; 
                        this.Close();                         
                    }
                    else
                    {
                        MessageBox.Show("Không thể cập nhật mật khẩu.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi mật khẩu: " + ex.Message);
            }
            finally { c.disconnect(); }
        }
    }
    }

