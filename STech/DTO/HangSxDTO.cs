using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class HangSxDTO
    {
        public string MaHSX { get; set; }
        public string TenHang { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }
        public string HinhAnh { get; set; }

        public HangSxDTO()
        {
        }

        public HangSxDTO(string maHSX, string tenHang, string diaChi, string sDT, string hinhAnh)
        {
            MaHSX = maHSX;
            TenHang = tenHang;
            DiaChi = diaChi;
            SDT = sDT;
            HinhAnh = hinhAnh;
        }
    }
}