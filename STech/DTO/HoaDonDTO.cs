using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class HoaDonDTO
    {
        public string MaHD { get; set; }
        public Nullable<System.DateTime> NgayDat { get; set; }
        public Nullable<decimal> TongTien { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public string TrangThai { get; set; }
        public string DiaChiGiao { get; set; }
        public string GhiChu { get; set; }
        public string MaKH { get; set; }
        public string MaNV { get; set; }
        public KhachHangDTO KhachHang { get; set; }
        public NhanVienDTO NhanVien { get; set; }
        public List<ChiTietHDDTO> ChiTietHD { get; set; }

        public HoaDonDTO()
        {
        }

        public HoaDonDTO(string maHD, DateTime? ngayDat, decimal? tongTien, string phuongThucThanhToan, string trangThaiThanhToan, string trangThai, string diaChiGiao, string ghiChu, string maKH, string maNV, KhachHangDTO khachHang, NhanVienDTO nhanVien, List<ChiTietHDDTO> chiTietHD)
        {
            MaHD = maHD;
            NgayDat = ngayDat;
            TongTien = tongTien;
            PhuongThucThanhToan = phuongThucThanhToan;
            TrangThaiThanhToan = trangThaiThanhToan;
            TrangThai = trangThai;
            DiaChiGiao = diaChiGiao;
            GhiChu = ghiChu;
            MaKH = maKH;
            MaNV = maNV;
            KhachHang = khachHang;
            NhanVien = nhanVien;
            ChiTietHD = chiTietHD;
        }
    }
}