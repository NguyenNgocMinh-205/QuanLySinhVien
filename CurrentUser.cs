using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLySinhVien
{
    internal class CurrentUser
    {
        public static string Username { get; set; }
        public static string Role { get; set; }   

        private static string Clean(string s) => (s ?? "").Trim();

        public static bool IsAdmin => string.Equals(Clean(Role), "QuanTriVien", StringComparison.OrdinalIgnoreCase);
        public static bool IsGiangVien => string.Equals(Clean(Role), "GiangVien", StringComparison.OrdinalIgnoreCase);

        public static void SignOut() { Username = null; Role = null; }
    }
    public static class Auth
    {
        public const string FeatureQLGV = "QLGV";
        public const string FeatureQLTK = "QLTK";

        public static bool Allow(string feature)
        {
            if (CurrentUser.IsAdmin) return true;
            if (CurrentUser.IsGiangVien)
                return !(feature == FeatureQLGV || feature == FeatureQLTK);
            return false; 
        }
    }
}
