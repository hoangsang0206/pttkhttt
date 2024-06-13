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
                if(kh != null)
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
            Response.Cookies["OrderTemp"].Expires = DateTime.Now.AddMinutes(15);

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
                    if(Request.Cookies["OrderTemp"] != null)
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
                        return Redirect("/cart");
                    }

                    List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();
                    
                    KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);
                    if (kh == null)
                    {
                        await addNewCustomer(db, userID);
                    }

                    
                    HoaDon last_hd = await db.HoaDons.OrderByDescending(t => t.MaHD).FirstOrDefaultAsync();
                    int orderNumber = 1;
                    if (last_hd != null)
                    {
                        orderNumber = int.Parse(last_hd.MaHD.Substring(2)) + 1;
                    }

                    string orderID = "DH" + orderNumber.ToString().PadLeft(5, '0');
                    decimal totalPrice = userCart.Sum(t => t.SoLuong * t.SanPham.GiaBan);
                    kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);

                    HoaDon hd = new HoaDon();
                    hd.MaHD = orderID;
                    hd.MaKH = kh.MaKH;
                    hd.NgayDat = DateTime.Now;
                    hd.GhiChu = orderTemp.GhiChu;
                    hd.TongTien = totalPrice;
                    hd.TrangThaiThanhToan = "unpaid";
                    hd.TrangThai = "unconfirmed";
                    hd.DiaChiGiao = orderTemp.DiaChiGiao;

                    TichDiem td = new TichDiem();
                    td.MaTD = RandomString.random(30);
                    td.MaHD = orderID;
                    td.NgayTD = DateTime.Now;
                    td.SoDiem = totalPrice * (decimal)0.01;
                    td.TrangThai = "unconfirmed";

                    List<ChiTietHD> chitietHD = new List<ChiTietHD>();
                    foreach (GioHang c in userCart)
                    {
                        ChiTietHD ctHD = new ChiTietHD();
                        ctHD.MaHD = orderID;
                        ctHD.MaSP = c.MaSP;
                        ctHD.SoLuong = c.SoLuong;
                        ctHD.ThanhTien = c.SanPham.GiaBan;

                        chitietHD.Add(ctHD);

                        //Trừ số lượng khỏi kho
                        List<ChiTietKho> chitietKho = c.SanPham.ChiTietKhoes.ToList();
                        int qty_toDelete = c.SoLuong;
                        foreach(ChiTietKho ctk in chitietKho)
                        {
                            if(ctk.SoLuong < qty_toDelete)
                            {
                                qty_toDelete -= ctk.SoLuong;
                                ctk.SoLuong = 0;
                            } 
                            else
                            {
                                ctk.SoLuong -= qty_toDelete;
                                qty_toDelete = 0;
                                break;
                            }

                        }
                    }


                    //--------------------------------------------------------------------------------
                    if (paymentMethod == "COD")
                    {
                        hd.PhuongThucThanhToan = "COD";
                        db.HoaDons.Add(hd);
                        db.ChiTietHDs.AddRange(chitietHD);
                        db.GioHangs.RemoveRange(userCart);
                        db.TichDiems.Add(td);
                        db.SaveChanges();

                        Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
                        Session["OrderStatus"] = true;
                        return Json(new { status = HttpStatusCode.OK, redirectUrl = "/order/succeeded" });
                    }
                    else if (paymentMethod == "card")
                    {
                        string domain = getCurrentDomain();

                        //Stripe api key
                        StripeConfiguration.ApiKey = StripeConfig.GetApiKey();

                        // Số tiền cần thanh toán => đổi sang USD (đơn vị là cents - 1 USD  = 100 cents)
                        long amount = ((long)Math.Round(totalPrice / USD_EXCHANGE_RATE)) * 100;
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
                                            Name = "Thanh toán đơn hàng " + orderID,
                                        },
                                        UnitAmount = amount,
                                    },
                                    Quantity = 1,
                                }
                            },
                            Metadata = new Dictionary<string, string> { { "order_id", orderID } },
                            Mode = "payment",
                            SuccessUrl = domain + "/order/updatepaymentstatus",
                            CancelUrl = domain + "/order/failed"

                        };

                        var service = new SessionService();
                        var session = service.Create(options);
                        TempData["Session"] = session.Id;

                        hd.PhuongThucThanhToan = "Visa/Mastercard";
                        db.HoaDons.Add(hd);
                        db.ChiTietHDs.AddRange(chitietHD);
                        db.GioHangs.RemoveRange(userCart);
                        db.TichDiems.Add(td);
                        db.SaveChanges();

                        Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
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
                var user = await userManager.FindByIdAsync(userID);

                KhachHang last_kh = db.KhachHangs.OrderByDescending(k => k.MaKH).FirstOrDefaultAsync().Result;
                int customerNumber = 1;
                if (last_kh != null)
                {
                    customerNumber = int.Parse(last_kh.MaKH.Substring(2)) + 1;
                }

                string customerID = "KH" + customerNumber.ToString().PadLeft(4, '0');
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

        //Thanh toán bằng paypal
        public async Task<ActionResult> PaymentWithPaypal(string Cancel = null)
        { 
            using (DbEntities db = new DbEntities())
            {
                APIContext apiContext = PaypalConfig.GetAPIContext();

                string userID = User.Identity.GetUserId();
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
                    return Redirect("/cart");
                }

                List<GioHang> userCart = await db.GioHangs.Where(t => t.AccountId == userID).ToListAsync();

                KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);
                if (kh == null)
                {
                    await addNewCustomer(db, userID);
                }


                HoaDon last_hd = await db.HoaDons.OrderByDescending(t => t.MaHD).FirstOrDefaultAsync();
                int orderNumber = 1;
                if (last_hd != null)
                {
                    orderNumber = int.Parse(last_hd.MaHD.Substring(2)) + 1;
                }

                string orderID = "DH" + orderNumber.ToString().PadLeft(5, '0');
                decimal totalPrice = userCart.Sum(t => t.SoLuong * t.SanPham.GiaBan);
                kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.AccountId == userID);

                HoaDon hd = new HoaDon();
                hd.MaHD = orderID;
                hd.MaKH = kh.MaKH;
                hd.NgayDat = DateTime.Now;
                hd.GhiChu = orderTemp.GhiChu;
                hd.TongTien = totalPrice;
                hd.PhuongThucThanhToan = "Paypal";
                hd.TrangThaiThanhToan = "unpaid";
                hd.TrangThai = "unconfirmed";
                hd.DiaChiGiao = orderTemp.DiaChiGiao;

                TichDiem td = new TichDiem();
                td.MaTD = RandomString.random(30);
                td.MaHD = orderID;
                td.NgayTD = DateTime.Now;
                td.SoDiem = totalPrice * (decimal)0.01;
                td.TrangThai = "unconfirmed";

                List<ChiTietHD> chitietHD = new List<ChiTietHD>();
                foreach (GioHang c in userCart)
                {
                    ChiTietHD ctHD = new ChiTietHD();
                    ctHD.MaHD = orderID;
                    ctHD.MaSP = c.MaSP;
                    ctHD.SoLuong = c.SoLuong;
                    ctHD.ThanhTien = c.SanPham.GiaBan;

                    chitietHD.Add(ctHD);

                    //Trừ số lượng khỏi kho
                    List<ChiTietKho> chitietKho = c.SanPham.ChiTietKhoes.ToList();
                    int qty_toDelete = c.SoLuong;
                    foreach (ChiTietKho ctk in chitietKho)
                    {
                        if (ctk.SoLuong < qty_toDelete)
                        {
                            qty_toDelete -= ctk.SoLuong;
                            ctk.SoLuong = 0;
                        }
                        else
                        {
                            ctk.SoLuong -= qty_toDelete;
                            qty_toDelete = 0;
                            break;
                        }

                    }
                }

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
                            db.HoaDons.Add(hd);
                            db.ChiTietHDs.AddRange(chitietHD);
                            db.GioHangs.RemoveRange(userCart);
                            db.TichDiems.Add(td);
                            db.SaveChanges();

                            Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
                            Session["OrderStatus"] = false;
                            return RedirectToAction("Failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    hd.TrangThaiThanhToan = "failed";
                    db.HoaDons.Add(hd);
                    db.ChiTietHDs.AddRange(chitietHD);
                    db.GioHangs.RemoveRange(userCart);
                    db.TichDiems.Add(td);
                    db.SaveChanges();

                    Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
                    Session["OrderStatus"] = false;
                    return RedirectToAction("Failed");
                }

                hd.TrangThaiThanhToan = "paid";
                db.HoaDons.Add(hd);
                db.ChiTietHDs.AddRange(chitietHD);
                db.GioHangs.RemoveRange(userCart);
                db.TichDiems.Add(td);
                db.SaveChanges();

                Request.Cookies["OrderTemp"].Expires = DateTime.Now.AddDays(-10);
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
                    name = "Pay for " + cthd.MaSP,
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

        ////Kiểm tra chi tiết đơn hàng
        //public ActionResult Detail(string id)
        //{
        //    try
        //    {
        //        string userID = User.Identity.GetUserId();
        //        DbEntities db = new DbEntities();
        //        STech_Web.Models.Order order = db.Orders.FirstOrDefault(t => t.OrderID == id && t.Customer.AccountID == userID);

        //        if (order == null)
        //        {
        //            return Redirect("/account#orders");
        //        }

        //        //Tạo danh sách Breadcrumb
        //        List<Breadcrumb> breadcrumb = new List<Breadcrumb>();
        //        breadcrumb.Add(new Breadcrumb("Trang chủ", "/"));
        //        breadcrumb.Add(new Breadcrumb("Đơn hàng", "/account#orders"));
        //        breadcrumb.Add(new Breadcrumb("Chi tiết " + id, ""));

        //        //Dùng để chuyển sang định dạng số có dấu phân cách phần nghìn
        //        CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");

        //        //Lấy chi tiết đơn hàng
        //        List<OrderDetail> orderDetail = order.OrderDetails.ToList();

        //        //Lấy thông tin khách hàng
        //        STech_Web.Models.Customer customer = order.Customer;

        //        ViewBag.Breadcrumb = breadcrumb;
        //        ViewBag.Order = order;
        //        ViewBag.Customer = customer;
        //        ViewBag.cul = cul;
        //        return View(orderDetail);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Redirect("/error/notfound");
        //    }

        //}

        ////In hóa đơn
        //public ActionResult PrintOrder(string orderID)
        //{
        //    try
        //    {
        //        if (User.Identity.IsAuthenticated)
        //        {
        //            string userID = User.Identity.GetUserId();
        //            DbEntities db = new DbEntities();
        //            STech_Web.Models.Customer customer = db.Customers.FirstOrDefault(t => t.AccountID == userID);
        //            if (customer == null)
        //            {
        //                return Redirect("#");
        //            }

        //            STech_Web.Models.Order order = customer.Orders.FirstOrDefault(t => t.OrderID == orderID);
        //            if (order == null)
        //            {
        //                return Redirect("#");
        //            }

        //            PrintInvoice printInvoice = new PrintInvoice(order);
        //            byte[] file = printInvoice.Print();

        //            return File(file, printInvoice.ContentType, printInvoice.FileName);
        //        }
        //        else
        //        {
        //            return Redirect("#");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return Redirect("#");
        //    }
        //}

        ////Xóa hóa đơn có trạng thái chờ thanh toán
        //public ActionResult Delete(string orderID)
        //{
        //    try
        //    {
        //        string userID = User.Identity.GetUserId();
        //        DbEntities db = new DbEntities();
        //        STech_Web.Models.Customer customer = db.Customers.FirstOrDefault(t => t.AccountID == userID);
        //        STech_Web.Models.Order order = customer.Orders.FirstOrDefault(t => t.OrderID == orderID);

        //        if (order != null && order.PaymentStatus == "Chờ thanh toán" && order.Status == "Chờ xác nhận")
        //        {
        //            List<OrderDetail> orderDetails = order.OrderDetails.ToList();
        //            //Cập nhật lại số lượng của sản phẩm
        //            foreach (OrderDetail orderDetail in orderDetails)
        //            {
        //                WareHouse wh = orderDetail.Product.WareHouse;
        //                wh.Quantity += orderDetail.Quantity;
        //            }

        //            db.OrderDetails.RemoveRange(orderDetails);
        //            db.Orders.Remove(order);
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex) { }
        //    return Redirect("/account#orders");
        //}
    }
}