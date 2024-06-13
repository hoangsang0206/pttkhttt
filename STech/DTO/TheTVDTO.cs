using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class TheTVDTO
    {
        public string MaThe { get; set; }
        public Nullable<decimal> TongDiem { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public KhachHangDTO KhachHang { get; set; }
        public List<TichDiemDTO> DSTichDiem { get; set; }

        public TheTVDTO()
        {
        }

        public TheTVDTO(string maThe, decimal? tongDiem, DateTime? ngayTao, KhachHangDTO khachHang, List<TichDiemDTO> dSTichDiem)
        {
            MaThe = maThe;
            TongDiem = tongDiem;
            NgayTao = ngayTao;
            KhachHang = khachHang;
            DSTichDiem = dSTichDiem;
        }
    }
}