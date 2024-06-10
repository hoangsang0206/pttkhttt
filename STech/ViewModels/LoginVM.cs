using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STech.ViewModels
{
    public class LoginVM
    {

        [Required(ErrorMessage = "* Tài khoản không để trống")]
        public string Username { get; set; }
        [Required(ErrorMessage = "* Mật khẩu không để trống")]
        public string Password { get; set; }
    }
}