using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.DTO
{
    public class DanhMucDTO
    {
        public string MaDM { get; set; }
        public string TenDM { get; set; }
        public string HinhAnh { get; set; }

        public DanhMucDTO()
        {
        }

        public DanhMucDTO(string maDM, string tenDM, string hinhAnh)
        {
            MaDM = maDM;
            TenDM = tenDM;
            HinhAnh = hinhAnh;
        }
    }
}