using Microsoft.AspNet.Identity;
using STech.DTO;
using STech.Models;
using STech.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace STech.Areas.Employee.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class OrdersController : Controller
    {
        // GET: Employee/Orders
        public async Task<ActionResult> Index()
        {
            using (DbEntities db = new DbEntities())
            {
                List<HoaDonDTO> dsHD = await db.HoaDons
                        .OrderByDescending(hd => hd.NgayDat)
                        .Select(hd => new HoaDonDTO()
                        {
                            MaHD = hd.MaHD,
                            TongTien = hd.TongTien,
                            NgayDat = hd.NgayDat,
                            TrangThaiThanhToan = hd.TrangThaiThanhToan,
                            PhuongThucThanhToan = hd.PhuongThucThanhToan,
                            TrangThai = hd.TrangThai,
                            KhachHang = new KhachHangDTO()
                            {
                                MaKH = hd.KhachHang.MaKH,
                                HoTen = hd.KhachHang.HoTen,
                            },
                        })
                        .ToListAsync();

                ViewBag.ActiveNav = "orders";
                return View(dsHD);
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

                        HoaDonDTO hd = await db.HoaDons
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
                            .FirstOrDefaultAsync(t => t.MaHD == orderID);

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
    }
}