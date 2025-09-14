using System;

namespace QuanLySinhVien
{
    public static class RoleUtils
    {
        
        public static string NormalizeRole(string role)
            => (role ?? "").Trim().ToLowerInvariant();

        public static bool IsLecturer(string role)
        {
            var r = NormalizeRole(role);
            return r == "giáo viên" || r == "giao vien" || r == "giang vien" || r == "gv"
                || r.Contains("giáo") || r.Contains("giang") || r.Contains("giao");
        }

        public static bool IsAdmin(string role)
        {
            var r = NormalizeRole(role);
            return r == "quản trị viên" || r == "quan tri vien" || r == "admin"
                || r.Contains("quản trị") || r.Contains("quan tri");
        }
    }

    
}
