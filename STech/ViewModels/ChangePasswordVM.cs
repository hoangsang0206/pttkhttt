using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STech.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "* Mật khẩu cũ không để trống")]
        public string OldPassword { get; set;}
        [Required(ErrorMessage = "* Mật khẩu mới không để trống")]
        public string NewPassword { get; set;}
        [Required(ErrorMessage = "* Xác nhận mật mới khẩu không để trống")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không đúng")]
        public string ConfirmNewPassword { get; set;}
    }
}