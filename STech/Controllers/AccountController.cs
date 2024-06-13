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
using STech.Filters;
using Azure.Storage.Blobs;
using Stripe;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using STech.Utils;

namespace STech.Controllers
{
    public class AccountController : Controller
    {
        [AuthenticationFilter]
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
                        .OrderByDescending(hd => hd.NgayDat)
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
                    CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
                    ViewBag.cul = cul;

                    return View(tuple);
                }
            } 
            catch (Exception ex) 
            {
                return Redirect("/");
            }
        }

        [HttpPost]
        public async Task<JsonResult> Login(LoginVM login)
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
                            return Json(new {status = HttpStatusCode.OK, redirectUrl = "/admin"});
                        } else if(await userManager.IsInRoleAsync(user.Id, "employee"))
                        {
                            return Json(new { status = HttpStatusCode.OK, redirectUrl = "/employee" });
                        }

                        return Json(new { status = HttpStatusCode.OK });
                    }
                }

                return Json(new { status = HttpStatusCode.BadRequest, message = "Sai tên đăng nhập hoặc mật khẩu" });
            }

            return Json(new { status = HttpStatusCode.BadRequest, message = "Đăng nhập thất bại" });
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
                        return Json(new { status = HttpStatusCode.BadRequest, message = "Tài khoản này đã tồn tại" });
                    }

                    if (await userManager.FindByEmailAsync(register.RegEmail) != null)
                    {
                        return Json(new { status = HttpStatusCode.BadRequest, message = "Email này đã tồn tại" });
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

            return Json(new { status = HttpStatusCode.BadRequest, message = "Thông tin không hợp lệ" });
        }

        [AuthenticationFilter]
        public ActionResult Logout()
        {
            IAuthenticationManager authenManager = HttpContext.GetOwinContext().Authentication;
            authenManager.SignOut();

            return Redirect("/");
        }

        [HttpPut, AuthenticationFilter]
        public async Task<JsonResult> Update(UpdateAccountVM update)
        {
            if (User.Identity.IsAuthenticated && ModelState.IsValid)
            {
                using (ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext())))
                {
                    string userID = User.Identity.GetUserId();
                    ApplicationUser user = await userManager.FindByIdAsync(userID);

                    if (user != null)
                    {
                        List<ApplicationUser> allUsers = await userManager.Users.ToListAsync();
                        if (allUsers.Any(t => t.Id != userID && t.Email == update.Email))
                        {
                            string err = "Email này đã tồn tại.";
                            return Json(new { status = HttpStatusCode.BadRequest, message = err });
                        }

                        if (allUsers.Any(t => t.Id != userID && t.PhoneNumber == update.PhoneNumber))
                        {
                            string err = "Số điện thoại này đã tồn tại.";
                            return Json(new { status = HttpStatusCode.BadRequest, message = err });
                        }

                        user.UserFullName = update.UserFullName;
                        user.Gender = update.Gender;
                        user.PhoneNumber = update.PhoneNumber;
                        user.Email = update.Email;
                        user.DOB = update.DOB;
                        user.Address = update.Address;

                        var updateCheck = userManager.Update(user);
                        if (updateCheck.Succeeded)
                        {
                            DbEntities db = new DbEntities();
                            KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(c => c.AccountId == userID);
                            if (kh != null)
                            {
                                kh.HoTen = user.UserFullName;
                                kh.GioiTinh = user.Gender;
                                kh.SDT = user.PhoneNumber;
                                kh.Email = user.Email;
                                kh.NgaySinh = user.DOB;
                                kh.DiaChi = user.Address;

                                db.SaveChanges();
                            }

                            return Json(new { status = HttpStatusCode.OK });
                        }
                    }
                }
            }

            return Json(new { status = HttpStatusCode.BadRequest, message = "Không thể cập nhật." });
        }

        //Đổi mật khẩu
        [HttpPost, AuthenticationFilter]
        public JsonResult ChangePassword(ChangePasswordVM changePwd)
        {
            if (User.Identity.IsAuthenticated && ModelState.IsValid)
            {
                using (ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext())))
                {
                    string userID = User.Identity.GetUserId();
                    ApplicationUser user = userManager.FindById(userID);

                    if (user != null)
                    {
                        if (Crypto.VerifyHashedPassword(user.PasswordHash, changePwd.OldPassword) == false)
                        {
                            return Json(new { status = 400, message = "Mật khẩu cũ không đúng." });
                        }

                        user.PasswordHash = Crypto.HashPassword(changePwd.NewPassword);
                        var updateCheck = userManager.Update(user);
                        if (updateCheck.Succeeded)
                        {
                            return Json(new { status = 200 });
                        }
                    }
                }
                
            }

            return Json(new { status = 400, message = "Không thể đổi mật khẩu" });
        }

        //Upload hình ảnh
        [HttpPost, AuthenticationFilter]
        public async Task<JsonResult> UploadImage()
        {
            try
            {
                HttpPostedFileBase imageFile = null;
                if (HttpContext.Request.Files.Count > 0)
                {
                    imageFile = HttpContext.Request.Files[0];
                }

                if (imageFile == null || imageFile.ContentLength <= 0)
                {
                    return Json(new { status = HttpStatusCode.BadRequest, message = "Hình ảnh không được để trống." });
                }

                if (imageFile.ContentLength > 512000)
                {
                    return Json(new { success = false, message = "Kích thước hình ảnh không lớn hơn 5MB." });
                }

                var allowExtensions = new[] { ".jpg", ".png", ".jpeg", ".webp" };
                string fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowExtensions.Contains(fileEx))
                {
                    return Json(new { status = HttpStatusCode.BadRequest, message = "Vui lòng tải lên hình ảnh dạng .jpg, .jpeg, .png, .webp." });
                }

                string userID = User.Identity.GetUserId();
                ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext()));
                ApplicationUser user = userManager.FindById(userID);

                string fileName = user.UserName + '-' + RandomString.random(30);
                string imgSrc = null;

                //Upload hình ảnh lên Azure Storage -----
                string blobConnectionString = "";
                string blobContainerName = "stechweb";
                string blobFilePath = "user-images/" + fileName + fileEx;

                BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobFilePath);

                await blobClient.UploadAsync(imageFile.InputStream);

                if (!String.IsNullOrEmpty(user.Avatar))
                {
                    blobClient = blobContainerClient.GetBlobClient(user.Avatar.Replace("https://lhswebstorage.blob.core.windows.net/stechweb/", ""));

                    await blobClient.DeleteIfExistsAsync();

                }

                user.Avatar = "https://lhswebstorage.blob.core.windows.net/stechweb/" + blobFilePath;
                userManager.Update(user);


                return Json(new { success = true, src = user.Avatar });

            }
            catch (Exception ex)
            {
                return Json(new { status = HttpStatusCode.BadRequest, message = "Tải lên thất bại." });
            }
        }
    }
}