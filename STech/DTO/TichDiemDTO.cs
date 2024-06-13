using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class TichDiemDTO
    {
        public string MaTD { get; set; }
        public Nullable<System.DateTime> NgayTD { get; set; }
        public decimal SoDiem { get; set; }
        public string TrangThai { get; set; }
        public TheTVDTO TTV { get; set; }
        public HoaDonDTO HoaDon { get; set; }

        public TichDiemDTO()
        {
        }

        public TichDiemDTO(string maTD, DateTime? ngayTD, decimal soDiem, string trangThai, TheTVDTO tTV, HoaDonDTO hoaDon)
        {
            MaTD = maTD;
            NgayTD = ngayTD;
            SoDiem = soDiem;
            TrangThai = trangThai;
            TTV = tTV;
            HoaDon = hoaDon;
        }
    }
}