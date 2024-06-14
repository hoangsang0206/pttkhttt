using STech.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.ViewModels
{
    public class CreateOrderVM
    {
        public KhachHangDTO KhachHang { get; set; }
        public string PaymentMed {  get; set; }
        public string Note { get; set; }
        public List<ChiTietHDDTO> ChiTietHD { get; set; }
    }
}