using Microsoft.AspNet.Identity;
using STech.Identity;
using STech.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using Microsoft.Owin.Security;
using System.Web.Mvc;
using System.Net;

namespace STech.Controllers
{
    public class AccountController : Controller
    {
        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginVM login)
        {
            if(ModelState.IsValid)
            {
                using (ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext()))) {
                    ApplicationUser user = await userManager.FindAsync(login.Username, login.Password);
                    if (user != null)
                    {
                        IAuthenticationManager authenManager = HttpContext.GetOwinContext().Authentication;
                        ClaimsIdentity identityUser = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                        authenManager.SignIn(new AuthenticationProperties(), identityUser);

                        if (await userManager.IsInRoleAsync(user.Id, "admin"))
                        {
                            return Json(new {status = 200, redirectUrl = "/admin"});
                        }

                        return Json(new { status = 200, redirectUrl = "" });
                    }
                }

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sai tên đăng nhập hoặc mật khẩu");
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Đăng nhập thất bại");
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterVM register)
        {
            if(ModelState.IsValid)
            {
                using (ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext())))
                {
                    ApplicationUser user = new ApplicationUser()
                    {
                        UserName = register.RegUsername,
                        Email = register.RegEmail,
                        PasswordHash = Crypto.HashPassword(register.RegPassword)
                    };

                    if (await userManager.FindByNameAsync(register.RegUsername) != null) 
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Tài khoản này đã tồn tại");
                    }

                    if (await userManager.FindByEmailAsync(register.RegEmail) != null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Email này đã tồn tại");
                    }

                    IdentityResult result = await userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user.Id, "Customer");
                        IAuthenticationManager authenManager = HttpContext.GetOwinContext().Authentication;
                        ClaimsIdentity identityUser = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                        authenManager.SignIn(new AuthenticationProperties(), identityUser);
                    }

                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                    
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Thông tin không hợp lệ");
        }

        public ActionResult Logout()
        {
            IAuthenticationManager authenManager = HttpContext.GetOwinContext().Authentication;
            authenManager.SignOut();

            return Redirect("/");
        }
    }
}