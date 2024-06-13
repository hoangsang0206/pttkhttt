using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class ChiTietHDDTO
    {
        public string MaHD { get; set; }
        public string MaSP { get; set; }
        public decimal ThanhTien { get; set; }
        public int SoLuong { get; set; }
        public SanPhamDTO SanPham { get; set; }

        public ChiTietHDDTO()
        {
        }

        public ChiTietHDDTO(string maHD, decimal thanhTien, int soLuong, SanPhamDTO sanPham, string maSP)
        {
            MaHD = maHD;
            ThanhTien = thanhTien;
            SoLuong = soLuong;
            SanPham = sanPham;
            MaSP = maSP;
        }
    }
}