using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.OtherModels
{
    public class GioHangCookie
    {
        public string MaSP { get; set; }
        public int SoLuong { get; set; }

        public GioHangCookie() { }
        public GioHangCookie(string MaSP, int SoLuong)
        {
            this.MaSP = MaSP;
            this.SoLuong = SoLuong;
        }
    }
}