using System;
using System.Data;
using System.Data.SqlClient;

namespace QLSV   // Nếu bạn muốn y hệt namespace repo cũ, đổi thành: namespace Quanlybanhangluuniem
{
    class connectData
    {
        public SqlConnection conn;

        public void connect()
        {
            // Đổi RUANMEI\\SQLEXPRESS theo máy của bạn nếu khác
            string strCon = @"Data Source=RUANMEI\SQLEXPRESS;Initial Catalog=QLSV;Integrated Security=True;TrustServerCertificate=True";
            try
            {
                conn = new SqlConnection(strCon);
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public void disconnect()
        {
            if (conn != null)
            {
                try
                {
                    if (conn.State != ConnectionState.Closed) conn.Close();
                    conn.Dispose();
                }
                finally
                {
                    conn = null;
                }
            }
        }

        // Thực thi lệnh INSERT/UPDATE/DELETE (trả về true/false như bản gốc)
        public bool exeSQL(string cmd)
        {
            try
            {
                using (SqlCommand sc = new SqlCommand(cmd, conn))
                {
                    sc.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // (tiện dụng) Lấy dữ liệu SELECT về DataTable – vẫn giữ đúng phong cách class này
        public DataTable getTable(string selectSql)
        {
            using (SqlDataAdapter da = new SqlDataAdapter(selectSql, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
