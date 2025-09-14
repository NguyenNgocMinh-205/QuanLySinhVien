
using System;
using System.Linq;
using System.Windows.Forms;

namespace QuanLySinhVien.GUI
{
    public partial class fTrangChu : Form
    {
        public fTrangChu()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
        }

       
        private void OpenChild<T>(bool adminOnly = false) where T : Form, new()
        {
            if (adminOnly && RoleUtils.IsLecturer(CurrentUser.Role))
            {
                MessageBox.Show("Bạn không có quyền truy cập mục này.", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

           
            foreach (var child in this.MdiChildren.Where(f => !(f is T)).ToArray())
                child.Close();

            var opened = this.MdiChildren.FirstOrDefault(f => f is T);
            if (opened != null)
            {
                opened.Activate();
                return;
            }

            
            var frm = new T
            {
                MdiParent = this,
                ShowIcon = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual
            };

            frm.FormBorderStyle = FormBorderStyle.None;
            frm.ControlBox = false;
            frm.MaximizeBox = false;
            frm.MinimizeBox = false;
            frm.Text = string.Empty;

            frm.Dock = DockStyle.Fill;        // quan trọng
            frm.Show();
        }

        private void fTrangChu_Load(object sender, EventArgs e) => ApplyRole();

        public void ApplyRole()
        {
            bool isGV = RoleUtils.IsLecturer(CurrentUser.Role);

           
            quảnLýGiảngViênToolStripMenuItem.Visible = !isGV;
            quảnLýTàiKhoảnToolStripMenuItem.Visible = !isGV;
            quảnLýGiảngViênToolStripMenuItem.Enabled = !isGV;
            quảnLýTàiKhoảnToolStripMenuItem.Enabled = !isGV;

            this.Text = $"Trang chủ - {CurrentUser.Username} ({CurrentUser.Role})";
        }

       
        private void quảnLýToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case "quảnLýKhóaHọcToolStripMenuItem": OpenChild<fQuanLyKhoaHoc>(); break;
                case "quanToolStripMenuItem": OpenChild<fNganh>(); break;
                case "quảnLýLớpToolStripMenuItem": OpenChild<fQuanLyLop>(); break;
                case "quảnLýMônHọcToolStripMenuItem": OpenChild<fQuanLyMonHoc>(); break;
                case "quảnLýGiảngViênToolStripMenuItem": OpenChild<fQuanLyGiangVien>(adminOnly: true); break;
                case "quảnLýSinhViênToolStripMenuItem": OpenChild<fSinhVien>(); break;
                case "quảnLýĐiểmToolStripMenuItem": OpenChild<fQuanLyDiem>(); break;
                case "quảnLýDânTộcToolStripMenuItem": OpenChild<fQuanLyDanToc>(); break;
                case "quảnLýĐịaChỉToolStripMenuItem": OpenChild<fQuanLyDiaChi>(); break;
                case "quảnLýTônGiáoToolStripMenuItem": OpenChild<fQuanLyTonGiao>(); break;
                case "quảnLýTàiKhoảnToolStripMenuItem": OpenChild<fQuanLytaiKhoan>(adminOnly: true); break;
            }
        }

       

       
        private void quảnLýKhóaHọcToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
            => quảnLýToolStripMenuItem_DropDownItemClicked(sender, e);

       
        private void quảnLýKhóaHọcToolStripMenuItem_Click(object sender, EventArgs e)
            => OpenChild<fQuanLyKhoaHoc>();

       
        private void quanlynganh(object sender, EventArgs e)
            => OpenChild<fNganh>();

    
        private void chứcNăngToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case "đổiMậtKhẩuToolStripMenuItem": OpenChild<fDoiMatKhau>(); break;
                case "thoátToolStripMenuItem":
                    // Tuỳ bạn: Close form hoặc mở màn hình thông tin
                    this.Close();
                    break;
            }
        }

        // (Nếu vẫn có handler click riêng cho 2 menu dưới, giữ lại để chắc chắn chặn GV)
        private void quảnLýGiảngViênToolStripMenuItem_Click(object sender, EventArgs e)
            => OpenChild<fQuanLyGiangVien>(adminOnly: true);

        private void quảnLýTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
            => OpenChild<fQuanLytaiKhoan>(adminOnly: true);

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn muốn đăng xuất?", "Xác nhận",
        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            CurrentUser.Username = null;
            CurrentUser.Role = null;

            BeginInvoke(new Action(() =>
            {
                Application.Restart();  
            }));
        }

        private void báoCáoThốngKêToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<fThongKe>();
        }
    }
}
