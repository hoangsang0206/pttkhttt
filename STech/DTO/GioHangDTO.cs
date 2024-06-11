using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class GioHangDTO
    {
        public SanPhamDTO SanPham { get; set; }
        public int SoLuong { get; set; }

        public GioHangDTO() { }

        public GioHangDTO(SanPhamDTO sp, int soluong)
        {
            this.SanPham = sp;
            this.SoLuong = soluong;
        }
    }
}