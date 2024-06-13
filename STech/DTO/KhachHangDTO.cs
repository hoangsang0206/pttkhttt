using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class KhachHangDTO
    {
        public string MaKH { get; set; }
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public TheTVDTO TheTV { get; set; }

        public KhachHangDTO()
        {
        }

        public KhachHangDTO(string maKH, string hoTen, string sDT, string email, string diaChi, DateTime? ngaySinh, string gioiTinh, TheTVDTO theTV)
        {
            MaKH = maKH;
            HoTen = hoTen;
            SDT = sDT;
            Email = email;
            DiaChi = diaChi;
            NgaySinh = ngaySinh;
            GioiTinh = gioiTinh;
            TheTV = theTV;
        }
    }
}