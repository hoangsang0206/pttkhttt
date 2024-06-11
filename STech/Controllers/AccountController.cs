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
using STech.Models;
using System.Collections;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using STech.DTO;
using System;

namespace STech.Controllers
{
    public class AccountController : Controller
    {
        public async Task<ActionResult> Index()
        {
            try
            {
                using (DbEntities db = new DbEntities())
                {
                    string userId = User.Identity.GetUserId();
                    ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext()));
                    ApplicationUser user = userManager.FindByIdAsync(userId).Result;

                    List<HoaDonDTO> dsHD = await db.HoaDons
                        .Where(hd => hd.KhachHang.AccountId.Equals(user.Id))
                        .Select(hd => new HoaDonDTO()
                        {
                            MaHD = hd.MaHD,
                            NgayDat = hd.NgayDat,
                            TongTien = hd.TongTien,
                            PhuongThucThanhToan = hd.PhuongThucThanhToan,
                            TrangThaiThanhToan = hd.TrangThaiThanhToan,
                            TrangThai = hd.TrangThai
                        })
                        .ToListAsync();


                    KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(k => k.AccountId.Equals(user.Id));
                    if (kh != null)
                    {
                        user.UserFullName = kh.HoTen;
                        user.PhoneNumber = kh.SDT;
                        user.Address = kh.DiaChi;
                        user.Email = kh.Email;
                        user.Gender = kh.GioiTinh;
                        user.DOB = kh.NgaySinh;

                        await userManager.UpdateAsync(user);
                    }

                    Tuple<ApplicationUser, List<HoaDonDTO>> tuple = new Tuple<ApplicationUser, List<HoaDonDTO>>(user, dsHD);
                    ViewBag.ActiveBotNav = "account";

                    return View(tuple);
                }
            } 
            catch (Exception ex) 
            {
                return Redirect("/");
            }
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

                        return Json(new { status = 200 });
                    }
                }

                return Json(new { status = 400, message = "Sai tên đăng nhập hoặc mật khẩu" });
            }

            return Json(new { status = 400, message = "Đăng nhập thất bại" });
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
                        return Json(new { status = 400, message = "Tài khoản này đã tồn tại" });
                    }

                    if (await userManager.FindByEmailAsync(register.RegEmail) != null)
                    {
                        return Json(new { status = 400, message = "Email này đã tồn tại" });
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

            return Json(new { status = 400, message = "Thông tin không hợp lệ" });
        }

        public ActionResult Logout()
        {
            IAuthenticationManager authenManager = HttpContext.GetOwinContext().Authentication;
            authenManager.SignOut();

            return Redirect("/");
        }
    }
}