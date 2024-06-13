using Microsoft.AspNet.Identity;
using STech.DTO;
using STech.Models;
using Stripe.Climate;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity.Migrations;

namespace STech.Controllers_api
{
    [Authorize(Roles = "Admin, Manager, Employee, Customer")]
    public class OrdersController : ApiController
    {
        private bool checkCompanyRole()
        {
            return User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Employee"));
        }

        private bool checkCustomerRole() 
        {
            return User.Identity.IsAuthenticated && User.IsInRole("Customer");
        }



        public async Task<IEnumerable<HoaDonDTO>> Get()
        {
            if (checkCompanyRole())
            {
                using (DbEntities db = new DbEntities())
                {

                    return await db.HoaDons
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
                                HoTen = hd.KhachHang.HoTen
                            }
                        })
                        .ToListAsync();
                }
            }

            return null;
        }

        public async Task<IEnumerable<HoaDonDTO>> GetByStatus(string status)
        {
            if (checkCompanyRole())
            {
                using (DbEntities db = new DbEntities())
                {

                    return await db.HoaDons
                        .OrderByDescending(hd => hd.NgayDat)
                        .Where(hd => hd.TrangThai == status)
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
                                HoTen = hd.KhachHang.HoTen
                            }
                        })
                        .ToListAsync();
                }
            }

            return null;
        }

        public async Task<IEnumerable<HoaDonDTO>> GetByPaymentStatus(string pStatus)
        {
            if (checkCompanyRole())
            {
                using (DbEntities db = new DbEntities())
                {

                    return await db.HoaDons
                        .OrderByDescending(hd => hd.NgayDat)
                        .Where(hd => hd.TrangThaiThanhToan == pStatus)
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
                                HoTen = hd.KhachHang.HoTen
                            }
                        })
                        .ToListAsync();
                }
                
            }

            return null;
        }

        public async Task<IEnumerable<HoaDonDTO>> GetToDayByStatus(string todayStatus)
        {
            if (checkCompanyRole())
            {
                using (DbEntities db = new DbEntities())
                {

                    return await db.HoaDons
                        .OrderByDescending(hd => hd.NgayDat)
                        .Where(hd => hd.TrangThai == todayStatus && DbFunctions.TruncateTime(hd.NgayDat) == DbFunctions.TruncateTime(DateTime.Now))
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
                                HoTen = hd.KhachHang.HoTen
                            }
                        })
                        .ToListAsync();
                }
            }
            return null;
        }


        public async Task<IEnumerable<HoaDonDTO>> GetSearch(string searchId)
        {

            using (DbEntities db = new DbEntities())
            {
                if (checkCompanyRole())
                {
                    return await db.HoaDons
                        .Where(h => h.MaHD.Contains(searchId))
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
                                HoTen = hd.KhachHang.HoTen
                            }
                        })
                        .ToListAsync();

                } else if(checkCustomerRole())
                {
                    string userId = User.Identity.GetUserId();
                    return await db.HoaDons
                        .Where(h => h.MaHD.Contains(searchId) && h.KhachHang.AccountId == userId)
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
                                HoTen = hd.KhachHang.HoTen
                            }
                        })
                        .ToListAsync();
                }
            }

            return null;
        }

        public async Task<IEnumerable<HoaDonDTO>> GetCustomerSearch(string type)
        {
            if (checkCustomerRole())
            {
                using (DbEntities db = new DbEntities())
                {

                    List<HoaDon> dsHD = new List<HoaDon>();
                    string userId = User.Identity.GetUserId();
                    KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(k => k.AccountId == userId);
                    switch (type)
                    {
                        case "all":
                            dsHD = kh.HoaDons.ToList();
                            break;
                        case "new":
                            dsHD = kh.HoaDons.Where(t => t.NgayDat >= DateTime.Now.AddDays(-1)).ToList();
                            break;
                        case "unpaid":
                            dsHD = kh.HoaDons.Where(t => t.TrangThaiThanhToan == "unpaid").ToList();
                            break;
                        case "paid":
                            dsHD = kh.HoaDons.Where(t => t.TrangThaiThanhToan == "paid").ToList();
                            break;
                    }

                    return dsHD.OrderByDescending(hd => hd.NgayDat).Select(hd => new HoaDonDTO()
                    {
                        MaHD = hd.MaHD,
                        TongTien = hd.TongTien,
                        NgayDat = hd.NgayDat,
                        TrangThaiThanhToan = hd.TrangThaiThanhToan,
                        PhuongThucThanhToan = hd.PhuongThucThanhToan,
                        TrangThai = hd.TrangThai,
                    });
                }
            }

            return null;
        }

        public async Task<HoaDonDTO> Get(string orderId)
        {
            using (DbEntities db = new DbEntities())
            {
                if(checkCompanyRole())
                {
                    HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(h => h.MaHD == orderId);
                    return new HoaDonDTO()
                    {
                        MaHD = hd.MaHD,
                        TongTien = hd.TongTien,
                        NgayDat = hd.NgayDat,
                        TrangThaiThanhToan = hd.TrangThaiThanhToan,
                        PhuongThucThanhToan = hd.PhuongThucThanhToan,
                        TrangThai = hd.TrangThai,
                        DiaChiGiao = hd.DiaChiGiao,
                        GhiChu = hd.GhiChu,
                        KhachHang = new KhachHangDTO()
                        {
                            MaKH = hd.KhachHang.MaKH,
                            HoTen = hd.KhachHang.HoTen,
                            SDT = hd.KhachHang.SDT,
                            Email = hd.KhachHang.Email,
                        },
                        //ChiTietHD = hd.ChiTietHDs.Select(ct => new ChiTietHDDTO()
                        //{

                        //})
                    };
                } 
                else if (checkCustomerRole())
                {
                    string userId = User.Identity.GetUserId();
                    HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(h => h.MaHD == orderId && h.KhachHang.AccountId == userId);
                    return new HoaDonDTO()
                    {
                        MaHD = hd.MaHD,
                        TongTien = hd.TongTien,
                        NgayDat = hd.NgayDat,
                        TrangThaiThanhToan = hd.TrangThaiThanhToan,
                        PhuongThucThanhToan = hd.PhuongThucThanhToan,
                        TrangThai = hd.TrangThai,
                    };
                }

                return null;
            }
        }

        public async Task<int> GetCount(string t)
        {
            try
            {
                if (t != "count") return 0;
                if (checkCompanyRole())
                {
                    using (DbEntities db = new DbEntities())
                    {
                        List<HoaDon> dsHD = await db.HoaDons.Where(hd => hd.TrangThai == "unconfirmed").ToListAsync();

                        return dsHD.Count;
                    }
                }

            }
            catch (Exception ex)
            {
                return 0;
            }

            return 0;
        }

        public void Post(KhachHangDTO kh, NhanVienDTO nv, string paymentMed, string note, List<ChiTietHDDTO> dsSP)
        {

        }

        public async Task PutStatus(string orderId, string status)
        {
            if (checkCompanyRole())
            {
                using (DbEntities db = new DbEntities())
                {
                    string userId = User.Identity.GetUserId();
                    NhanVien nv = await db.NhanViens.FirstOrDefaultAsync(t => t.AccountId == userId);
                    if (nv == null) return;

                    HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(t => t.MaHD == orderId && t.TrangThai == "unconfirmed");
                    if(hd != null)
                    {
                        if (status == "confirmed")
                        {
                            hd.TrangThai = status;
                            hd.MaNV = nv.MaNV;
                            TichDiem td = await db.TichDiems.FirstOrDefaultAsync(t => t.MaHD == hd.MaHD && t.TrangThai == "unconfirmed");
                            if (td != null)
                            {
                                TheThanhVien ttv = td.TheThanhVien;
                                if (ttv != null)
                                {
                                    td.TrangThai = "confirmed";
                                    ttv.TongDiem += td.SoDiem;
                                }
                            }
                            db.SaveChanges();
                        }
                        else if (status == "cancelled")
                        {
                            List<PhieuXuatKho> dsPXK = hd.PhieuXuatKhoes.ToList();
                            TichDiem td = hd.TichDiem;
                            hd.TrangThai = "cancelled";
                            hd.MaNV = nv.MaNV;

                            foreach (PhieuXuatKho pxk in dsPXK)
                            {
                                List<ChiTietPXK> chitietPXK = pxk.ChiTietPXKs.ToList();
                                foreach (ChiTietPXK ctpxk in chitietPXK)
                                {
                                    ChiTietKho ctk = pxk.Kho.ChiTietKhoes.Where(t => t.MaKho == pxk.MaKho && t.MaSP == ctpxk.MaSP).FirstOrDefault();
                                    if (ctk != null)
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
            }
        }

        public void Delete(int id)
        {
        }
    }
}