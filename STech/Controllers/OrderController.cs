using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Stripe.Checkout;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using STech.DTO;
using STech.Identity;
using System.Threading.Tasks;
using STech.Models;
using System.Data.Entity;
using System.Net;
using STech.Filters;
using PayPal.Api;
using System.Data.Entity.Migrations;
using STech.ConfigModels;
using STech.Utils;
using STech.OtherModels;
using Stripe.Climate;

namespace STech.Controllers
{
    [AuthenticationFilter]
    public class OrderController : Controller
    {
        private const decimal USD_EXCHANGE_RATE = 25000;

        CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");

        [HttpPost]
        public async Task<ActionResult> CheckOrderInfo(string gender, string customerName, string customerPhone, string address, string note)
        {
            if (string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerPhone) || string.IsNullOrEmpty(address))
            {
                return Json(new { status = HttpStatusCode.BadRequest, message = "Vui lòng nhập đầy đủ thông tin" });
            }

            if (customerPhone == null || !Regex.IsMatch(customerPhone, @"^(0[1-9]\d{8,9})$|^(84[1-9]\d{8,9})$|^\+84[1-9]\d{8,9}$"))
            {
                return Json(new { status = HttpStatusCode.BadRequest, message = "Số điện thoại không hợp lệ" });
            }

            using (DbEntities db = new DbEntities())
            {
                string userID = User.Identity.GetUserId();
                ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext()));
                ApplicationUser user = await userManager.FindByIdAsync(userID);
                user.PhoneNumber = customerPhone;
                user.UserFullName = customerName;
                user.Gender = gender;
                await userManager.UpdateAsync(user);

                KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(k => k.AccountId == userID);
                if (kh != null)
                {
                    kh.SDT = customerPhone;
                    kh.HoTen = customerName;
                    kh.GioiTinh = gender;

                    db.SaveChanges();
                }

                List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();

                List<string> errors = new List<string>();
                int coutProductOutOfStock = 0;
                foreach (GioHang c in userCart)
                {
                    if (c.SanPham.ChiTietKhoes.Count <= 0)
                    {
                        int sum = c.SanPham.ChiTietKhoes.Sum(ctk => ctk.SoLuong);
                        coutProductOutOfStock += 1;
                        string err = "";

                        if (sum <= 0)
                        {
                            err = "Sản phẩm --" + c.SanPham.TenSP + "-- đã hết hàng";
                        }
                        else if (sum < c.SoLuong)
                        {
                            err = "Sản phẩm --" + c.SanPham.TenSP + "-- chỉ còn " + sum + " trong kho";
                        }

                        errors.Add(err);
                    }
                }

                if (coutProductOutOfStock > 0)
                {
                    Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
                    return Json(new { status = HttpStatusCode.BadRequest, error = errors });
                }
            }

            HoaDonDTO orderTemp = new HoaDonDTO();
            orderTemp.DiaChiGiao = address;
            orderTemp.GhiChu = note;

            string orderTempJson = JsonConvert.SerializeObject(orderTemp);
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(orderTempJson);
            string base64String = Convert.ToBase64String(bytesToEncode);

            Response.Cookies["OrderTemp"].Value = base64String;
            Response.Cookies["OrderTemp"].Expires = DateTime.Now.AddMinutes(5);

            return Json(new { status = HttpStatusCode.OK, redirectUrl = "/order/orderinfo" });
        }

        public async Task<ActionResult> OrderInfo()
        {
            using (ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext())))
            {
                string base64String = Request.Cookies["OrderTemp"]?.Value;
                HoaDonDTO orderTemp = new HoaDonDTO();
                if (string.IsNullOrEmpty(base64String))
                {
                    return Redirect("/cart");
                }

                byte[] bytesToDecode = Convert.FromBase64String(base64String);
                string orderTempJson = Encoding.UTF8.GetString(bytesToDecode);
                orderTemp = JsonConvert.DeserializeObject<HoaDonDTO>(orderTempJson);
                if (orderTemp == null)
                {
                    if (Request.Cookies["OrderTemp"] != null)
                    {
                        Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
                    }
                    return Redirect("/cart");
                }
            ;
                ApplicationUser user = await userManager.FindByIdAsync(User.Identity.GetUserId());
                string userID = User.Identity.GetUserId();

                DbEntities db = new DbEntities();
                List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                if (userCart.Count <= 0)
                {
                    Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
                    return Redirect("/cart");
                }

                orderTemp.TongTien = userCart.Sum(t => t.SoLuong * t.SanPham.GiaBan);

                Tuple<ApplicationUser, HoaDonDTO> tuple = new Tuple<ApplicationUser, HoaDonDTO>(user, orderTemp);

                return View(tuple);
            }
        }

        private void saveDatabase(DbEntities db, HoaDon hd, List<ChiTietHD> cthd, List<PhieuXuatKho> dsPxk, List<GioHang> userCart, TichDiem td)
        {
            db.HoaDons.Add(hd);
            db.ChiTietHDs.AddRange(cthd);
            db.PhieuXuatKhoes.AddRange(dsPxk);
            db.GioHangs.RemoveRange(userCart);
            db.SaveChanges();

            if (td != null && td.TheThanhVien != null)
            {
                hd.MaTichDiem = td.MaTD;
                db.TichDiems.Add(td);
                db.SaveChanges();
                db.Entry(hd).State = EntityState.Modified;
                db.SaveChanges();
            }

            Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
        }

        public string getCurrentDomain()
        {
            var url = HttpContext.Request.Url;

            if (url != null)
            {
                var scheme = url.Scheme;
                var host = url.Host;
                var port = url.Port;
                var domain = $"{scheme}://{host}:{port}";

                return domain;
            }

            return null;
        }

        public async Task<string> addNewCustomer(DbEntities db, string userID)
        {

            using (ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext())))
            {
                ApplicationUser user = await userManager.FindByIdAsync(userID);

                string customerID = "KH" + DateTime.Now.ToString("ddMMyy") + RandomString.random(5).ToUpper();
                KhachHang kh = new KhachHang();
                kh.AccountId = userID;
                kh.MaKH = customerID;
                kh.HoTen = user.UserFullName;
                kh.DiaChi = user.Address;
                kh.SDT = user.PhoneNumber;
                kh.Email = user.Email;
                kh.NgaySinh = user.DOB;
                kh.GioiTinh = user.Gender;

                db.KhachHangs.Add(kh);
                db.SaveChanges();

                return customerID;
            }
        }

        private HoaDonDTO getCookieOrder()
        {
            string base64String = Request.Cookies["OrderTemp"]?.Value;
            HoaDonDTO orderTemp = new HoaDonDTO();

            if (!string.IsNullOrEmpty(base64String))
            {
                var bytesToDecode = Convert.FromBase64String(base64String);
                var orderTempJson = Encoding.UTF8.GetString(bytesToDecode);
                orderTemp = JsonConvert.DeserializeObject<HoaDonDTO>(orderTempJson);
            }

            if (orderTemp == null)
            {
                Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
            }

            return orderTemp;
        }

        private async Task<HoaDon> createOrder(DbEntities db, string userID, List<GioHang> userCart, KhachHang kh)
        {
            if (kh == null)
            {
                await addNewCustomer(db, userID);
            }

            string orderID = "DH" + DateTime.Now.ToString("ddMMyy") + RandomString.random(5).ToUpper();
            decimal totalPrice = userCart.Sum(t => t.SoLuong * t.SanPham.GiaBan);
            kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);

            HoaDonDTO orderTemp = getCookieOrder();
            if(orderTemp == null)
            {
                return null;
            }

            HoaDon hd = new HoaDon();
            hd.MaHD = orderID;
            hd.MaKH = kh.MaKH;
            hd.NgayDat = DateTime.Now;
            hd.GhiChu = orderTemp.GhiChu;
            hd.TongTien = totalPrice;
            hd.TrangThaiThanhToan = "unpaid";
            hd.TrangThai = "unconfirmed";
            hd.DiaChiGiao = orderTemp.DiaChiGiao;

            return hd;
        }

        private List<ChiTietHD> createOrderDetail(HoaDon hd, List<GioHang> userCart)
        {
            List<ChiTietHD> chitietHD = new List<ChiTietHD>();
            foreach (GioHang c in userCart)
            {
                ChiTietHD ctHD = new ChiTietHD();
                ctHD.MaHD = hd.MaHD;
                ctHD.MaSP = c.MaSP;
                ctHD.SoLuong = c.SoLuong;
                ctHD.ThanhTien = c.SanPham.GiaBan;
                chitietHD.Add(ctHD);
            }

            return chitietHD;
        }

        private List<PhieuXuatKho> createWarehouseExport(List<GioHang> userCart, HoaDon hd)
        {
            List<PhieuXuatKho> dsPXK = new List<PhieuXuatKho>();

            foreach (GioHang c in userCart)
            {
                List<ChiTietKho> chitietKho = c.SanPham.ChiTietKhoes.ToList();
                int remainingQty = c.SoLuong;
                foreach (ChiTietKho ctk in chitietKho)
                {
                    if (remainingQty <= 0)
                    {
                        break;
                    }

                    if (ctk.SoLuong > 0)
                    {
                        int qtyToDelete = Math.Min(remainingQty, ctk.SoLuong);
                        ctk.SoLuong -= qtyToDelete;
                        remainingQty -= qtyToDelete;

                        PhieuXuatKho pxk = dsPXK.FirstOrDefault(p => p.MaKho == ctk.MaKho);
                        if (pxk == null)
                        {
                            string maPXK = "PXK" + DateTime.Now.ToString("ddMMyy") + RandomString.random(5).ToUpper();
                            pxk = new PhieuXuatKho();
                            pxk.MaHD = hd.MaHD;
                            pxk.MaPXK = maPXK;
                            pxk.TongSoLuong = 0;
                            pxk.MaKho = ctk.MaKho;
                            pxk.ChiTietPXKs = new List<ChiTietPXK>();

                            dsPXK.Add(pxk);
                        }

                        pxk.ChiTietPXKs.Add(new ChiTietPXK()
                        {
                            MaPXK = pxk.MaPXK,
                            MaSP = ctk.MaSP,
                            SoLuong = qtyToDelete,
                        });
                        pxk.TongSoLuong += qtyToDelete;
                    }
                }
            }

            return dsPXK;
        }

        private TichDiem createAccumulationPoint(KhachHang kh, HoaDon hd)
        {
            TheThanhVien ttv = kh.TheThanhVien;
            TichDiem td = new TichDiem();
            if (ttv != null)
            {
                td.MaThe = ttv.MaThe;
                td.MaTD = RandomString.random(30);
                td.MaHD = hd.MaHD;
                td.NgayTD = DateTime.Now;
                td.SoDiem = hd.TongTien.Value * (decimal)0.01;
                td.TrangThai = "unconfirmed";
            }

            return td;
        }

        //Thanh toán
        [HttpPost]
        public async Task<ActionResult> CheckOut(string paymentMethod)
        {
            try
            {
                using (DbEntities db = new DbEntities())
                {
                    if (paymentMethod == "paypal")
                    {
                        return Json(new { status = HttpStatusCode.OK, redirectUrl = "/order/paymentwithpaypal" });
                    }

                    string userID = User.Identity.GetUserId();
                    List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                    KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);

                    HoaDon hd = await createOrder(db, userID, userCart, kh);
                    if(hd == null)
                    {
                        return Redirect("/cart");
                    }

                    List<ChiTietHD> chitietHD = createOrderDetail(hd, userCart);
                    List<PhieuXuatKho> dsPXK = createWarehouseExport(userCart, hd);
                    TichDiem td = createAccumulationPoint(kh, hd);

                    //--------------------------------------------------------------------------------
                    if (paymentMethod == "COD")
                    {
                        hd.PhuongThucThanhToan = "COD";
                        saveDatabase(db, hd, chitietHD, dsPXK, userCart, td);
                        Session["OrderStatus"] = true;
                        return Json(new { status = HttpStatusCode.OK, redirectUrl = "/order/succeeded" });
                    }
                    else if (paymentMethod == "card")
                    {
                        string domain = getCurrentDomain();

                        //Stripe api key
                        StripeConfiguration.ApiKey = StripeConfig.GetApiKey();

                        // Số tiền cần thanh toán => đổi sang USD (đơn vị là cents - 1 USD  = 100 cents)
                        long amount = ((long)Math.Round(hd.TongTien.Value / USD_EXCHANGE_RATE)) * 100;
                        //-------

                        SessionCreateOptions options = new SessionCreateOptions
                        {
                            PaymentMethodTypes = new List<string> { "card" },
                            LineItems = new List<SessionLineItemOptions>
                            {
                                new SessionLineItemOptions
                                {
                                    PriceData = new SessionLineItemPriceDataOptions
                                    {
                                        Currency = "usd",
                                        ProductData = new SessionLineItemPriceDataProductDataOptions
                                        {
                                            Name = "Thanh toán đơn hàng " + hd.MaHD,
                                        },
                                        UnitAmount = amount,
                                    },
                                    Quantity = 1,
                                }
                            },
                            Metadata = new Dictionary<string, string> { { "order_id", hd.MaHD } },
                            Mode = "payment",
                            SuccessUrl = domain + "/order/updatepaymentstatus",
                            CancelUrl = domain + "/order/failed"

                        };
                        
                        var service = new SessionService();
                        var session = service.Create(options);
                        TempData["Session"] = session.Id;

                        hd.PhuongThucThanhToan = "Visa/Mastercard";
                        saveDatabase(db, hd, chitietHD, dsPXK, userCart, td);

                        return Json(new { status = HttpStatusCode.OK, redirectUrl = session.Url });
                    }
                    else
                    {
                        return Json(new { status = HttpStatusCode.OK, redirectUrl = "/order/orderinfo" });
                    }
                }    
            }
            catch (Exception ex)
            {
                return Redirect("/error");
            }
        }

        //Thanh toán bằng paypal
        public async Task<ActionResult> PaymentWithPaypal(string Cancel = null)
        { 
            using (DbEntities db = new DbEntities())
            {
                APIContext apiContext = PaypalConfig.GetAPIContext();
                string userID = User.Identity.GetUserId();
                List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);

                HoaDon hd = await createOrder(db, userID, userCart, kh);
                if (hd == null)
                {
                    return Redirect("/cart");
                }

                List<ChiTietHD> chitietHD = createOrderDetail(hd, userCart);
                List<PhieuXuatKho> dsPXK = createWarehouseExport(userCart, hd);
                TichDiem td = createAccumulationPoint(kh, hd);

                try
                {
                    string payerId = Request.Params["PayerID"];
                    if (string.IsNullOrEmpty(payerId))
                    {
                        string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/order/PaymentWithPayPal?";
                        string guid = Convert.ToString((new Random()).Next(100000));
                        Payment createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid, chitietHD);
                        var links = createdPayment.links.GetEnumerator();
                        string paypalRedirectUrl = null;
                        while (links.MoveNext())
                        {
                            Links lnk = links.Current;
                            if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                            {
                                paypalRedirectUrl = lnk.href;
                            }
                        }
                        Session.Add(guid, createdPayment.id);
                        return Redirect(paypalRedirectUrl);
                    }
                    else
                    {
                        var guid = Request.Params["guid"];
                        var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                        if (executedPayment.state.ToLower() != "approved")
                        {
                            hd.TrangThaiThanhToan = "failed";
                            saveDatabase(db, hd, chitietHD, dsPXK, userCart, td);
                            Session["OrderStatus"] = false;
                            return RedirectToAction("Failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    hd.TrangThaiThanhToan = "failed";
                    saveDatabase(db, hd, chitietHD, dsPXK, userCart, td);
                    Session["OrderStatus"] = false;
                    return RedirectToAction("Failed");
                }

                hd.TrangThaiThanhToan = "paid";
                saveDatabase(db, hd, chitietHD, dsPXK, userCart, td);
                Session["OrderStatus"] = true;
                return RedirectToAction("Succeeded");
            }
        }
        private Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl, List<ChiTietHD> chitietHD)
        {
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };

            decimal totalPrice = 0;

            foreach (ChiTietHD cthd in chitietHD)
            {
                decimal _price = Math.Round(cthd.ThanhTien / USD_EXCHANGE_RATE);
                totalPrice += _price * cthd.SoLuong;
                itemList.items.Add(new Item()
                {
                    name = "x " + cthd.MaSP,
                    currency = "USD",
                    price = _price.ToString(),
                    quantity = Convert.ToString(cthd.SoLuong),
                    sku = cthd.MaHD
                });
            }


            Payer payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = totalPrice.ToString(),
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = totalPrice.ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(),
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
             
            return payment.Create(apiContext);
        }

        //Thanh toán thành công
        public ActionResult Succeeded()
        {
            if(Session["OrderStatus"] == null)
            {
                return Redirect("/");
            }

            bool status = (bool)Session["OrderStatus"];
            if(!status)
            {
                return RedirectToAction("Failed");
            }

            Session.Remove("OrderStatus");

            return View();
        }

        //Thanh toán thất bại
        public ActionResult Failed()
        {
            if (Session["OrderStatus"] == null)
            {
                return Redirect("/");
            }

            bool status = (bool)Session["OrderStatus"];
            if (status)
            {
                return RedirectToAction("Succeeded");
            }

            Session.Remove("OrderStatus");
            return View();
        }

        //Cập nhật trạng thái giao dịch  - Stripe
        public async Task<ActionResult> UpdatePaymentStatus()
        {
            using(DbEntities db = new DbEntities())
            {
                SessionService service = new SessionService();
                Session session = service.Get(TempData["Session"].ToString());
                string orderID = session.Metadata["order_id"];
                HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(t => t.MaHD == orderID);

                if (session.PaymentStatus == "paid")
                {
                    hd.TrangThaiThanhToan = "paid";
                    db.HoaDons.AddOrUpdate(hd);
                    db.SaveChanges();

                    Session["OrderStatus"] = true;
                    return RedirectToAction("Succeeded");
                }

                hd.TrangThaiThanhToan = "failed";
                db.HoaDons.AddOrUpdate(hd);
                db.SaveChanges();

                Session["OrderStatus"] = false;
                return RedirectToAction("Failed");
            }    
        }

        public async Task<ActionResult> Detail(string id)
        {
            try
            {
                using (DbEntities db = new DbEntities())
                {
                    string userID = User.Identity.GetUserId();
                    HoaDon hd = await db.HoaDons
                        .Include(t => t.ChiTietHDs)
                        .Include(t => t.TichDiems)
                        .Include(t => t.KhachHang)
                        .FirstOrDefaultAsync(t => t.MaHD == id && t.KhachHang.AccountId == userID);

                    if (hd == null)
                    {
                        return Redirect("/account#orders");
                    }

                    TichDiem td = hd.TichDiems.FirstOrDefault();

                    HoaDonDTO hdDTO = new HoaDonDTO()
                    {
                        MaHD = hd.MaHD,
                        NgayDat = hd.NgayDat,
                        TongTien = hd.TongTien,
                        DiaChiGiao = hd.DiaChiGiao,
                        GhiChu = hd.GhiChu,
                        TrangThai = hd.TrangThai,
                        PhuongThucThanhToan = hd.PhuongThucThanhToan,
                        TrangThaiThanhToan = hd.TrangThaiThanhToan,
                        KhachHang = new KhachHangDTO()
                        {
                            MaKH = hd.KhachHang.MaKH,
                            HoTen = hd.KhachHang.HoTen,
                            Email = hd.KhachHang.Email,
                            DiaChi = hd.KhachHang.DiaChi,
                            SDT = hd.KhachHang.SDT
                        },
                        TichDiem = td == null ? null : new TichDiemDTO()
                        {
                            SoDiem = td.SoDiem,
                            TrangThai = td.TrangThai
                        },
                        ChiTietHD = hd.ChiTietHDs.Select(cthd => new ChiTietHDDTO()
                        {
                            SanPham = new SanPhamDTO()
                            {
                                MaSP = cthd.SanPham.MaSP,
                                TenSP = cthd.SanPham.TenSP,
                                HinhAnh = cthd.SanPham.HinhAnhSPs != null ? cthd.SanPham.HinhAnhSPs.FirstOrDefault().DuongDan : null
                            },
                            SoLuong = cthd.SoLuong,
                            ThanhTien = cthd.ThanhTien,
                        }).ToList()
                        
                    };

                    List<Breadcrumb> breadcrumb = new List<Breadcrumb>();
                    breadcrumb.Add(new Breadcrumb("Trang chủ", "/"));
                    breadcrumb.Add(new Breadcrumb("Đơn hàng", "/account#orders"));
                    breadcrumb.Add(new Breadcrumb("Chi tiết đơn hàng " + hd.MaHD, ""));
                    CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");

                    Tuple<HoaDonDTO, List<ChiTietHDDTO>, TichDiemDTO, KhachHangDTO> tuple
                        = new Tuple<HoaDonDTO, List<ChiTietHDDTO>, TichDiemDTO, KhachHangDTO>(hdDTO, hdDTO.ChiTietHD, hdDTO.TichDiem, hdDTO.KhachHang);

                    ViewBag.Breadcrumb = breadcrumb;
                    ViewBag.cul = cul;
                    return View(tuple);
                }
            }
            catch (Exception ex)
            {
                return Redirect("/error/notfound");
            }

        }

        public async Task<ActionResult> PrintInvoice(string orderID)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        string userID = User.Identity.GetUserId();
                        KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);
                        if (kh == null)
                        {
                            return Redirect("#");
                        }

                        HoaDonDTO hd = kh.HoaDons
                            .Select(t => new HoaDonDTO()
                            {
                                MaHD = t.MaHD,
                                TongTien = t.TongTien,
                                NgayDat = t.NgayDat,
                                PhuongThucThanhToan = t.PhuongThucThanhToan,
                                DiaChiGiao = t.DiaChiGiao,
                                GhiChu = t.GhiChu,
                                KhachHang = new KhachHangDTO()
                                {
                                    HoTen = t.KhachHang.HoTen,
                                    SDT = t.KhachHang.SDT

                                },
                                ChiTietHD = t.ChiTietHDs.Select(cthd => new ChiTietHDDTO()
                                {
                                    SoLuong = cthd.SoLuong,
                                    ThanhTien = cthd.ThanhTien,
                                    SanPham = new SanPhamDTO()
                                    {
                                        TenSP = cthd.SanPham.TenSP,
                                        BaoHanh = cthd.SanPham.BaoHanh
                                    }
                                }).ToList(),

                            })
                            .FirstOrDefault(t => t.MaHD == orderID);

                        if (hd == null)
                        {
                            return Redirect("#");
                        }

                        PrintInvoice printInvoice = new PrintInvoice(hd);
                        byte[] file = printInvoice.Print();

                        return File(file, printInvoice.ContentType, printInvoice.FileName);
                    }    
                }
                else
                {
                    return Redirect("#");
                }

            }
            catch (Exception ex)
            {
                return Redirect("#");
            }
        }

        //Xóa hóa đơn có trạng thái chờ thanh toán
        public async Task<ActionResult> CancelOrder(string orderID)
        {
            try
            {
                using (DbEntities db = new DbEntities())
                {
                    string userID = User.Identity.GetUserId();
                    HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(t => t.MaHD == orderID && t.KhachHang.AccountId == userID);

                    if (hd != null  && hd.TrangThai == "unconfirmed")
                    {
                        List<PhieuXuatKho> dsPXK = hd.PhieuXuatKhoes.ToList();
                        TichDiem td = hd.TichDiem;
                        hd.TrangThai = "cancelled";

                        foreach(PhieuXuatKho pxk in dsPXK)
                        {
                            List<ChiTietPXK> chitietPXK = pxk.ChiTietPXKs.ToList();
                            foreach(ChiTietPXK ctpxk in chitietPXK)
                            {
                                ChiTietKho ctk = pxk.Kho.ChiTietKhoes.Where(t => t.MaKho == pxk.MaKho && t.MaSP == ctpxk.MaSP).FirstOrDefault();
                                if(ctk != null)
                                {
                                    ctk.SoLuong += ctpxk.SoLuong;
                                    db.ChiTietKhoes.AddOrUpdate(ctk);
                                }
                            }
                        }

                        if (td != null)
                        {
                            db.TichDiems.Remove(td);
                        }

                        db.PhieuXuatKhoes.RemoveRange(dsPXK);
                        db.HoaDons.AddOrUpdate(hd);
                        db.SaveChanges();
                    }
                }    
            }
            catch (Exception ex) { }
            return Redirect("/account#orders");
        }

        //private Stripe.Refund StripeRefund()
        //{

        //}
    }
}