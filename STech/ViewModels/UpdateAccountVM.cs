using STech.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STech.ViewModels
{
    public class UpdateAccountVM
    {
        [Required(ErrorMessage = "* Vui lòng nhập họ tên")]
        public String UserFullName { get; set; }
        public String Gender { get; set; }
        [DateOfBirth(ErrorMessage = "* Ngày sinh không hợp lệ")]
        public DateTime? DOB { get; set; }
        [Required(ErrorMessage = "* Vui lòng nhập địa chỉ")]
        public String Address { get; set; }
        [Required(ErrorMessage = "* Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0[1-9]\d{8,9})$|^(84[1-9]\d{8,9})$|^\+84[1-9]\d{8,9}$", ErrorMessage = "* Số điện thoại không hợp lệ")]
        public String PhoneNumber { get; set; }
        [Required(ErrorMessage = "* Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "* Email không hợp lệ")]
        public String Email { get; set; }
    }
}