using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STech.ViewModels
{
    public class CreateCustomerVM
    {
        [Required(ErrorMessage = "Họ tên không để trống")]
        public string HoTen { get; set; }
        [Required(ErrorMessage = "SĐT không để trống")]
        [RegularExpression(@"^(0[1-9]\d{8,9})$|^(84[1-9]\d{8,9})$|^\+84[1-9]\d{8,9}$", ErrorMessage = "* Số điện thoại không hợp lệ")]
        public string SDT { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "Địa chỉ không để trống")]
        public string DiaChi { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string GioiTinh { get; set; }
    }
}