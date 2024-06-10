using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STech.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "* Tài khoản không để trống")]
        public string RegUsername { get; set; }
        [Required(ErrorMessage = "* Mật khẩu không để trống")]
        public string RegPassword { get; set; }
        [Required(ErrorMessage = "* Xác nhận mật khẩu không để trống")]
        [Compare("RegPassword", ErrorMessage = "* Xác nhận mật khẩu không đúng")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "* Email không để trống")]
        [EmailAddress(ErrorMessage = "* Email không hợp lệ")]
        public string RegEmail { get; set; }
    }
}