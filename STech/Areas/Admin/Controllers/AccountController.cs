using Microsoft.Owin.Security;
using STech.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace STech.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [AuthenticationFilter, Authorize(Roles = "Admin, Manager, Employee")]
        public ActionResult Logout()
        {
            IAuthenticationManager authenManager = HttpContext.GetOwinContext().Authentication;
            authenManager.SignOut();

            return Redirect("/admin/login");
        }
    }
}