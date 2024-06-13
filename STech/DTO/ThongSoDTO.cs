using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class ThongSoDTO
    {
        public int Id { get; set; }
        public string MaSP { get; set; }
        public string TenTS { get; set; }
        public string MoTa { get; set; }

        public ThongSoDTO()
        {
        }

        public ThongSoDTO(int id, string maSP, string tenTS, string moTa)
        {
            Id = id;
            MaSP = maSP;
            TenTS = tenTS;
            MoTa = moTa;
        }
    }
}