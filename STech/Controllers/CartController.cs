using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using STech.DTO;
using STech.Identity;
using STech.Models;
using STech.OtherModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace STech.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public async Task<ActionResult> Index()
        {
            try
            {
                using (DbEntities db = new DbEntities())
                {
                    List<GioHangCookie> cartCookie = new List<GioHangCookie>();
                    List<GioHangDTO> cartDTO = new List<GioHangDTO>();
                    CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
                    ViewBag.cul = cul;


                    if (User.Identity.IsAuthenticated)
                    {
                        string userID = User.Identity.GetUserId();

                        cartCookie = getCartFromCookie();
                        if (cartCookie.Count > 0)
                        {
                            foreach (GioHangCookie cCookie in cartCookie)
                            {
                                GioHang cartExist = await db.GioHangs.FirstOrDefaultAsync(t => t.MaSP == cCookie.MaSP && t.AccountId == userID);
                                if (cartExist != null)
                                {
                                    if (cartExist.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong) <= 0)
                                    {
                                        db.GioHangs.Remove(cartExist);
                                    }
                                    else
                                    {
                                        cartExist.SoLuong += cCookie.SoLuong;
                                        if (cartExist.SoLuong >= cartExist.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong))
                                        {
                                            cartExist.SoLuong = cartExist.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong);
                                        }
                                        db.GioHangs.AddOrUpdate(cartExist);
                                    }
                                }
                                else
                                {
                                    GioHang cart = new GioHang();
                                    cart.MaSP = cCookie.MaSP;
                                    cart.SoLuong = cCookie.SoLuong;
                                    cart.AccountId = userID;
                                    db.GioHangs.Add(cart);
                                }
                            }
                            db.SaveChanges();
                        }

                        List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                        List<GioHang> cartToDelete = new List<GioHang>();
                        foreach (GioHang cart in userCart)
                        {
                            if (cart.SanPham.ChiTietKhoes.Count() <= 0 || cart.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong) <= 0)
                            {
                                cartToDelete.Add(cart);
                            }
                        }

                        if (cartToDelete.Count > 0)
                        {
                            db.GioHangs.RemoveRange(cartToDelete);
                            db.SaveChanges();
                            userCart = await db.GioHangs.Where(c => c.AccountId == userID).ToListAsync();
                        }

                        ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext()));
                        ApplicationUser user = await userManager.FindByIdAsync(User.Identity.GetUserId());
                        ViewBag.User = user;

                        //Delete cookie
                        Response.Cookies["GioHangCookie"].Expires = DateTime.Now.AddDays(-10);

                        cartDTO = userCart.Select(c => new GioHangDTO()
                        {
                            SanPham = new SanPhamDTO()
                            {
                                MaSP = c.SanPham.MaSP,
                                TenSP = c.SanPham.TenSP,
                                GiaGoc = c.SanPham.GiaGoc,
                                GiaBan = c.SanPham.GiaBan,
                                HinhAnh = c.SanPham.HinhAnhSPs != null ? c.SanPham.HinhAnhSPs.FirstOrDefault().DuongDan : null
                            },
                            SoLuong = c.SoLuong
                        }).ToList();
                    }
                    else
                    {
                        cartCookie = getCartFromCookie();
                        List<GioHangCookie> cartToDelete = new List<GioHangCookie>();
                        if (cartCookie.Count > 0)
                        {
                            foreach (GioHangCookie cart in cartCookie)
                            {
                                int quantity = cart.SoLuong;
                                SanPham sp = await db.SanPhams.FirstOrDefaultAsync(t => t.MaSP == cart.MaSP);

                                if (sp.ChiTietKhoes.Count <= 0 || sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong) <= 0)
                                {
                                    cartToDelete.Add(cart);
                                    continue;
                                }
                                SanPhamDTO spDTO = new SanPhamDTO()
                                {
                                    MaSP = sp.MaSP,
                                    TenSP = sp.TenSP,
                                    GiaGoc = sp.GiaGoc,
                                    GiaBan = sp.GiaBan,
                                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null
                                };
                                GioHangDTO cDTO = new GioHangDTO(spDTO, quantity);
                                cartDTO.Add(cDTO);
                            }

                            if (cartToDelete.Count > 0)
                            {
                                foreach (GioHangCookie item in cartToDelete)
                                {
                                    cartCookie.Remove(item);
                                }

                                saveCartToCookie(cartCookie);
                            }
                        }
                    }


                    return View(cartDTO);
                }
            }
            catch (Exception ex)
            {
                return Redirect("/");
            }
        }


        [HttpPost]
        public async Task<ActionResult> AddToCart(GioHangCookie cart)
        {
            try
            {
                using (DbEntities db = new DbEntities())
                {
                    SanPham sanpham = await db.SanPhams.FirstOrDefaultAsync(t => t.MaSP == cart.MaSP);
                    if (sanpham == null || sanpham.ChiTietKhoes.Count <= 0 || sanpham.ChiTietKhoes.Sum(ctk => ctk.SoLuong) <= 0)
                    {
                        return Json(new { success = false });
                    }

                    if (User.Identity.IsAuthenticated)
                    {

                        string userID = User.Identity.GetUserId();
                        List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();

                        GioHang existCart = userCart.FirstOrDefault(t => t.MaSP == cart.MaSP);
                        if (existCart != null)
                        {
                            if (existCart.SanPham.ChiTietKhoes.Count <= 0 || existCart.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong) <= 0)
                            {
                                db.GioHangs.Remove(existCart);
                            }
                            else
                            {
                                existCart.SoLuong += 1;
                                if (existCart.SoLuong >= existCart.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong))
                                {
                                    existCart.SoLuong = existCart.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong);
                                }

                                db.GioHangs.AddOrUpdate(existCart);
                            }

                            db.SaveChanges();
                        }
                        else
                        {
                            GioHang cartItem = new GioHang();
                            cartItem.MaSP = cart.MaSP;
                            cartItem.SoLuong = cart.SoLuong;
                            cartItem.AccountId = userID;
                            db.GioHangs.Add(cartItem);
                            db.SaveChanges();
                        }

                        return Json(new { success = true });
                    }
                    else
                    {
                        List<GioHangCookie> cartItems = getCartFromCookie();

                        //----------
                        GioHangCookie cartItem = cartItems.FirstOrDefault(t => t.MaSP == cart.MaSP);

                        if (cartItem != null)
                        {
                            cartItem.SoLuong += 1;
                            int inventory = sanpham.ChiTietKhoes.Sum(ctk => ctk.SoLuong);
                            if (cartItem.SoLuong >= inventory) cartItem.SoLuong = inventory;
                        }
                        else
                        {
                            cartItems.Add(new GioHangCookie()
                            {
                                MaSP = cart.MaSP,
                                SoLuong = cart.SoLuong
                            });

                        }

                        saveCartToCookie(cartItems);

                        return Json(new { success = true });
                    }
                }
            }
            catch (Exception ex)
            {
                return Redirect("/");
            }
        }

        public async Task<ActionResult> DeleteCartItem(int line = 0)
        {
            try
            {
                if (line > 0)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            string userID = User.Identity.GetUserId();
                            List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                            if (line > userCart.Count)
                            {
                                line = userCart.Count;
                            }
                            db.GioHangs.Remove(userCart[line - 1]);
                            db.SaveChanges();
                        }
                        else
                        {   //Add to cart when user not logged in
                            List<GioHangCookie> cartCookie = getCartFromCookie();
                            if (line > cartCookie.Count)
                            {
                                line = cartCookie.Count;
                            }

                            cartCookie.RemoveAt(line - 1);

                            saveCartToCookie(cartCookie);
                        }
                    }    
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Redirect("/cart");
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateQuantity(string maSP, string updateType, int sluong = 0)
        {
            try
            {
                int quantity = 0;
                decimal totalPrice = 0;
                string updateError = "";

                if (User.Identity.IsAuthenticated)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        string userID = User.Identity.GetUserId();
                        GioHang cart = await db.GioHangs.FirstOrDefaultAsync(t => t.AccountId == userID && t.MaSP == maSP);

                        if (cart != null)
                        {
                            int inventory = cart.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong);
                            if (updateType == "increase")
                            {
                                cart.SoLuong += 1;
                                if (cart.SoLuong > inventory)
                                {
                                    cart.SoLuong = inventory;
                                    updateError = "Sản phẩm này chỉ còn " + inventory + " sản phẩm trong kho.";
                                }
                            }
                            else if (updateType == "decrease")
                            {
                                cart.SoLuong -= 1;
                                if (cart.SoLuong <= 0) cart.SoLuong = 1;
                            }
                            else
                            {
                                if (sluong <= 0) sluong = 1;
                                if (sluong >= inventory)
                                {
                                    sluong = inventory;
                                    updateError = "Sản phẩm này chỉ còn " + inventory + " sản phẩm trong kho.";
                                }

                                cart.SoLuong = sluong;
                            }
                            quantity = cart.SoLuong;
                            db.GioHangs.AddOrUpdate(cart);
                            db.SaveChanges();

                            List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                            totalPrice = userCart.Sum(t => t.SanPham.GiaBan * t.SoLuong);
                        }
                    }
                }
                else
                {
                    List<GioHangCookie> cartCookie = getCartFromCookie();
                    GioHangCookie cartCItem = cartCookie.FirstOrDefault(t => t.MaSP == maSP);
                    if (cartCItem != null)
                    {
                        using (DbEntities db = new DbEntities())
                        {
                            int inventory = (int)db.SanPhams.FirstOrDefault(t => t.MaSP == cartCItem.MaSP).ChiTietKhoes.Sum(ctk => ctk.SoLuong);

                            if (updateType == "increase")
                            {
                                cartCItem.SoLuong += 1;
                                if (cartCItem.SoLuong > inventory)
                                {
                                    cartCItem.SoLuong = inventory;
                                    updateError = "Sản phẩm này chỉ còn " + inventory + " sản phẩm trong kho.";
                                }
                            }
                            else if (updateType == "decrease")
                            {
                                cartCItem.SoLuong -= 1;
                                if (cartCItem.SoLuong <= 0) cartCItem.SoLuong = 1;
                            }
                            else
                            {
                                if (sluong <= 0) sluong = 1;
                                if (sluong >= inventory)
                                {
                                    sluong = inventory;
                                    updateError = "Sản phẩm này chỉ còn " + inventory + " sản phẩm trong kho.";
                                }

                                cartCItem.SoLuong = sluong;
                            }
                            quantity = cartCItem.SoLuong;
                            cartCookie.Remove(cartCItem);
                            cartCookie.Add(cartCItem);

                            saveCartToCookie(cartCookie);

                            //Update total price in cart page
                            List<GioHangDTO> cartDTO = new List<GioHangDTO>();
                            if (cartCookie.Count > 0)
                            {
                                foreach (GioHangCookie item in cartCookie)
                                {
                                    SanPham sp = await db.SanPhams.FirstOrDefaultAsync(t => t.MaSP == item.MaSP);
                                    SanPhamDTO spDTO = new SanPhamDTO()
                                    {
                                        GiaBan = sp.GiaBan
                                    };

                                    cartDTO.Add(new GioHangDTO(spDTO, item.SoLuong));
                                }
                            }

                            totalPrice = (decimal)cartDTO.Sum(t => t.SanPham.GiaBan * t.SoLuong);
                        }
                    }
                }

                return Json(new { qty = quantity, total = totalPrice, error = updateError });
            }
            catch (Exception ex)
            {
                return Redirect("/cart");
            }
        }

        [HttpGet]
        public async Task<JsonResult> CartCount()
        {
            if (User.Identity.IsAuthenticated)
            {
                using (DbEntities db = new DbEntities())
                {
                    string userID = User.Identity.GetUserId();
                    List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                    int cartCount = userCart.Count();

                    return Json(new { count = cartCount }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                List<GioHangCookie> cartCookie = getCartFromCookie();

                return Json(new { count = cartCookie.Count() }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<GioHangCookie> getCartFromCookie()
        {
            List<GioHangCookie> cartCookie = new List<GioHangCookie>();
            var base64String = Request.Cookies["GioHangCookie"]?.Value;

            if (!string.IsNullOrEmpty(base64String))
            {
                var bytesToDecode = Convert.FromBase64String(base64String);
                var GioHangCookiesJson = Encoding.UTF8.GetString(bytesToDecode);
                cartCookie = JsonConvert.DeserializeObject<List<GioHangCookie>>(GioHangCookiesJson);
            }

            return cartCookie;
        }

        private void saveCartToCookie(List<GioHangCookie> cartCookie)
        {
            var cartJson = JsonConvert.SerializeObject(cartCookie);
            var bytesToEncode = Encoding.UTF8.GetBytes(cartJson);
            var base64String = Convert.ToBase64String(bytesToEncode);

            Response.Cookies["GioHangCookie"].Value = base64String;
            Response.Cookies["GioHangCookie"].Expires = DateTime.Now.AddDays(90);
        }

    }
}