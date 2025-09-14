using ClosedXML.Excel;
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
    public partial class fThongKe : Form
    {
        private readonly connectData c = new connectData();
        private DataTable dtKhoaHoc;
        private DataTable dtNganh;
        private DataTable dtLop;
        private DataTable dtMon;
        public fThongKe()
        {
            InitializeComponent();

            var now = DateTime.Today;
            dtpFrom.Value = new DateTime(now.Year, now.Month, 1);
            dtpTo.Value = now;
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void fThongKe_Load(object sender, EventArgs e)
        {
            LoadLookups();
            LoadGrid();
        }
        private void LoadLookups()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();


                using (var da = new SqlDataAdapter(
                    @"SELECT MaKhoaHoc, 
                             CAST(MaKhoaHoc AS nvarchar(20)) + N' - ' 
                               + CAST(NamBatDau AS nvarchar(4)) + N'→' + CAST(NamKetThuc AS nvarchar(4)) AS Ten
                      FROM KhoaHoc ORDER BY MaKhoaHoc", c.conn))
                {
                    dtKhoaHoc = new DataTable();
                    da.Fill(dtKhoaHoc);
                    cboKhoaHoc.DropDownStyle = ComboBoxStyle.DropDownList;
                    cboKhoaHoc.DataSource = dtKhoaHoc;
                    cboKhoaHoc.DisplayMember = "Ten";
                    cboKhoaHoc.ValueMember = "MaKhoaHoc";
                    cboKhoaHoc.SelectedIndex = -1;
                }


                using (var da = new SqlDataAdapter(
                    @"SELECT MaNganh, TenNganh, MaKhoaHoc FROM Nganh ORDER BY TenNganh", c.conn))
                {
                    dtNganh = new DataTable();
                    da.Fill(dtNganh);
                    cboNganh.DropDownStyle = ComboBoxStyle.DropDownList;
                    cboNganh.DataSource = dtNganh;
                    cboNganh.DisplayMember = "TenNganh";
                    cboNganh.ValueMember = "MaNganh";
                    cboNganh.SelectedIndex = -1;
                }


                using (var da = new SqlDataAdapter(
                    @"SELECT MaLop, TenLop, MaNganh FROM Lop ORDER BY TenLop", c.conn))
                {
                    dtLop = new DataTable();
                    da.Fill(dtLop);
                    cboLop.DropDownStyle = ComboBoxStyle.DropDownList;
                    cboLop.DataSource = dtLop;
                    cboLop.DisplayMember = "TenLop";
                    cboLop.ValueMember = "MaLop";
                    cboLop.SelectedIndex = -1;
                }


                using (var da = new SqlDataAdapter(
                    @"SELECT MaMon, TenMon, MaNganh FROM MonHoc ORDER BY TenMon", c.conn))
                {
                    dtMon = new DataTable();
                    da.Fill(dtMon);
                    cboMon.DropDownStyle = ComboBoxStyle.DropDownList;
                    cboMon.DataSource = dtMon;
                    cboMon.DisplayMember = "TenMon";
                    cboMon.ValueMember = "MaMon";
                    cboMon.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp danh mục: " + ex.Message);
            }
            finally { c.disconnect(); }
        }
        private void FilterNganhByKhoa()
        {
            try
            {
                if (cboKhoaHoc.SelectedValue == null)
                {
                    cboNganh.DataSource = dtNganh;
                    cboNganh.SelectedIndex = -1;
                }
                else
                {
                    var view = new DataView(dtNganh);
                    view.RowFilter = $"MaKhoaHoc = '{cboKhoaHoc.SelectedValue.ToString().Replace("'", "''")}'";
                    cboNganh.DataSource = view;
                    cboNganh.SelectedIndex = -1;
                }
                // Sau khi đổi ngành => reset lớp, môn
                FilterLopByNganh();
                FilterMonByNganh();
            }
            catch { }
        }

        private void FilterLopByNganh()
        {
            try
            {
                if (cboNganh.SelectedValue == null)
                {
                    cboLop.DataSource = dtLop;
                    cboLop.SelectedIndex = -1;
                }
                else
                {
                    var view = new DataView(dtLop);
                    view.RowFilter = $"MaNganh = '{cboNganh.SelectedValue.ToString().Replace("'", "''")}'";
                    cboLop.DataSource = view;
                    cboLop.SelectedIndex = -1;
                }
            }
            catch { }
        }

        private void FilterMonByNganh()
        {
            try
            {
                if (cboNganh.SelectedValue == null)
                {
                    cboMon.DataSource = dtMon;
                    cboMon.SelectedIndex = -1;
                }
                else
                {
                    var view = new DataView(dtMon);
                    view.RowFilter = $"MaNganh = '{cboNganh.SelectedValue.ToString().Replace("'", "''")}'";
                    cboMon.DataSource = view;
                    cboMon.SelectedIndex = -1;
                }
            }
            catch { }
        }


        private void LoadGrid()
        {
            try
            {
                if (c.conn == null || c.conn.State != ConnectionState.Open) c.connect();

                var sql = new StringBuilder(@"
                SELECT  d.MaSV,
                        s.Ho,
                        s.Ten,
                        l.TenLop,
                        ng.TenNganh,
                        ng.MaKhoaHoc,
                        mh.TenMon,
                        d.DiemQT,
                        d.DiemThi,
                        d.DiemTong,
                        ISNULL(d.GhiChu, N'') AS GhiChu
                FROM    Diem d
                JOIN    SinhVien s ON s.MaSV = d.MaSV
                JOIN    Lop l      ON l.MaLop = s.MaLop
                JOIN    Nganh ng   ON ng.MaNganh = l.MaNganh
                JOIN    MonHoc mh  ON mh.MaMon  = d.MaMon
                WHERE   CONVERT(date, d.NgayNhap) BETWEEN @from AND @to
                ");

                var cmd = new SqlCommand() { Connection = c.conn };
                cmd.Parameters.Add("@from", SqlDbType.Date).Value = dtpFrom.Value.Date;
                cmd.Parameters.Add("@to", SqlDbType.Date).Value = dtpTo.Value.Date;

                if (cboKhoaHoc.SelectedValue != null)
                {
                    sql.AppendLine("AND ng.MaKhoaHoc = @makh");
                    cmd.Parameters.AddWithValue("@makh", cboKhoaHoc.SelectedValue.ToString());
                }
                if (cboNganh.SelectedValue != null)
                {
                    sql.AppendLine("AND ng.MaNganh = @manganh");
                    cmd.Parameters.AddWithValue("@manganh", cboNganh.SelectedValue.ToString());
                }
                if (cboLop.SelectedValue != null)
                {
                    sql.AppendLine("AND l.MaLop = @malop");
                    cmd.Parameters.AddWithValue("@malop", cboLop.SelectedValue.ToString());
                }
                if (cboMon.SelectedValue != null)
                {
                    sql.AppendLine("AND mh.MaMon = @mamon");
                    cmd.Parameters.AddWithValue("@mamon", cboMon.SelectedValue.ToString());
                }
                if (!string.IsNullOrWhiteSpace(txtTim.Text))
                {
                    sql.AppendLine(@"AND (
                         d.MaSV    LIKE @kw OR
                         s.Ho      LIKE @kw OR
                         s.Ten     LIKE @kw OR
                         mh.TenMon LIKE @kw OR
                         l.TenLop  LIKE @kw OR
                         ng.TenNganh LIKE @kw)");
                    cmd.Parameters.AddWithValue("@kw", "%" + txtTim.Text.Trim() + "%");
                }

                sql.AppendLine("ORDER BY d.IDDiem DESC");
                cmd.CommandText = sql.ToString();

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);

                dgvChiTiet.AutoGenerateColumns = false;
                dgvChiTiet.DataSource = dt;


                dgvChiTiet.Columns["Column1"].DataPropertyName = "MaSV";
                dgvChiTiet.Columns["Column2"].DataPropertyName = "Ho";
                dgvChiTiet.Columns["Column3"].DataPropertyName = "Ten";
                dgvChiTiet.Columns["Column4"].DataPropertyName = "TenLop";
                dgvChiTiet.Columns["Column5"].DataPropertyName = "TenNganh";
                dgvChiTiet.Columns["Column6"].DataPropertyName = "MaKhoaHoc";
                dgvChiTiet.Columns["Column7"].DataPropertyName = "TenMon";
                dgvChiTiet.Columns["Column8"].DataPropertyName = "DiemQT";
                dgvChiTiet.Columns["Column9"].DataPropertyName = "DiemThi";
                dgvChiTiet.Columns["Column10"].DataPropertyName = "DiemTong";
                dgvChiTiet.Columns["Column11"].DataPropertyName = "GhiChu";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally { c.disconnect(); }
        }

        private void btnLoc_Click(object sender, EventArgs e)
        {
            LoadGrid();
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            var now = DateTime.Today;
            dtpFrom.Value = new DateTime(now.Year, now.Month, 1);
            dtpTo.Value = now;

            cboKhoaHoc.SelectedIndex = -1;
            cboNganh.SelectedIndex = -1;
            cboLop.SelectedIndex = -1;
            cboMon.SelectedIndex = -1;
            txtTim.Clear();

            LoadGrid();
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            if (dgvChiTiet.Rows.Count > 0)
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                var ws = wb.Worksheets.Add("ThongKe");

                                ws.Cell(1, 1).Value = "Thong Ke";
                                ws.Range(1, 1, 1, dgvChiTiet.Columns.Count).Merge();
                                ws.Range(1, 1, 1, dgvChiTiet.Columns.Count)
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                    .Font.SetBold()
                                    .Font.SetFontName("Times New Roman")
                                    .Font.SetFontSize(12);

                                for (int i = 0; i < dgvChiTiet.Columns.Count; i++)
                                {
                                    ws.Cell(2, i + 1).Value = dgvChiTiet.Columns[i].HeaderText;
                                }


                                for (int i = 0; i < dgvChiTiet.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dgvChiTiet.Columns.Count; j++)
                                    {
                                        ws.Cell(i + 3, j + 1).Value = dgvChiTiet.Rows[i].Cells[j].Value?.ToString();
                                    }
                                }


                                var range = ws.Range(1, 1, dgvChiTiet.Rows.Count + 1, dgvChiTiet.Columns.Count);
                                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                                ws.Columns().AdjustToContents();


                                wb.SaveAs(sfd.FileName);
                            }

                            MessageBox.Show("Xuất Excel thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    }

