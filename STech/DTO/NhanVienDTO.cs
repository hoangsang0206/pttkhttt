using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class NhanVienDTO
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string CCCD { get; set; }

        public NhanVienDTO()
        {
        }

        public NhanVienDTO(string maNV, string hoTen, string sDT, string email, string diaChi, DateTime? ngaySinh, string gioiTinh, string cCCD)
        {
            MaNV = maNV;
            HoTen = hoTen;
            SDT = sDT;
            Email = email;
            DiaChi = diaChi;
            NgaySinh = ngaySinh;
            GioiTinh = gioiTinh;
            CCCD = cCCD;
        }
    }
}