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
using STech.Utils;
using STech.ViewModels;

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


        [Authorize(Roles = "Admin, Manager, Employee")]
        public async Task<IEnumerable<HoaDonDTO>> Get()
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

        [Authorize(Roles = "Admin, Manager, Employee")]
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

        [Authorize(Roles = "Admin, Manager, Employee")]
        public async Task<IEnumerable<HoaDonDTO>> GetByPaymentStatus(string pStatus)
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

        [Authorize(Roles = "Admin, Manager, Employee")]
        public async Task<IEnumerable<HoaDonDTO>> GetToDayByStatus(string todayStatus)
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
                            DiaChi = hd.KhachHang.DiaChi
                        },
                        ChiTietHD = hd.ChiTietHDs.Select(ctk => new ChiTietHDDTO()
                        {
                            MaSP = ctk.MaSP,
                            SoLuong = ctk.SoLuong,
                            ThanhTien = ctk.ThanhTien,
                            SanPham = new SanPhamDTO()
                            {
                                MaSP = ctk.SanPham.MaSP,
                                TenSP = ctk.SanPham.TenSP,
                            }
                        }).ToList()
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

        [Authorize(Roles = "Admin, Manager, Employee")]
        public async Task<int> GetCount(string t)
        {
            try
            {
                if (t != "count") return 0;
                using (DbEntities db = new DbEntities())
                {
                    List<HoaDon> dsHD = await db.HoaDons.Where(hd => hd.TrangThai == "unconfirmed").ToListAsync();

                    return dsHD.Count;
                }

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [Authorize(Roles = "Admin, Manager, Employee")]
        private async Task<List<PhieuXuatKho>> createWarehouseExport(DbEntities db, List<ChiTietHD> cthd, HoaDon hd)
        {
            List<PhieuXuatKho> dsPXK = new List<PhieuXuatKho>();

            foreach (ChiTietHD ct in cthd)
            {
                List<ChiTietKho> chitietKho = await db.ChiTietKhoes.Where(ctk => ctk.MaSP == ct.MaSP).ToListAsync();
                int remainingQty = ct.SoLuong;
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

        [Authorize(Roles = "Admin, Manager, Employee")]
        public async Task<bool> Post(CreateOrderVM order)
        {
            using (DbEntities db = new DbEntities())
            {
                string userId = User.Identity.GetUserId();
                NhanVien nv = await db.NhanViens.FirstOrDefaultAsync(t => t.AccountId == userId);
                if (nv == null) return false;

                KhachHang _kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.MaKH == order.KhachHang.MaKH);
                if (_kh == null) return false;

                string orderID = "DH" + DateTime.Now.ToString("ddMMyy") + RandomString.random(5).ToUpper();
                decimal totalPrice = 0;

                //Cập nhật thông tin khách hàng
                _kh.HoTen = order.KhachHang.HoTen;
                _kh.SDT = order.KhachHang.SDT;
                _kh.DiaChi = order.KhachHang.DiaChi;
                _kh.GioiTinh = order.KhachHang.GioiTinh;
                _kh.Email = order.KhachHang.Email;

                HoaDon hd = new HoaDon();
                hd.MaHD = orderID;
                hd.MaKH = _kh.MaKH;
                hd.NgayDat = DateTime.Now;
                hd.GhiChu = order.Note;
                hd.TrangThaiThanhToan = "unpaid";
                hd.TrangThai = "unconfirmed";
                hd.PhuongThucThanhToan = order.PaymentMed;
                hd.DiaChiGiao = _kh.DiaChi;
                hd.MaNV = nv.MaNV;

                //Tạo chi tiết đơn hàng
                List<ChiTietHD> cthd = new List<ChiTietHD>();
                foreach (ChiTietHDDTO sp in order.ChiTietHD)
                {
                    SanPham _sp = await db.SanPhams.FirstOrDefaultAsync(t => t.MaSP == sp.MaSP);
                    totalPrice += sp.SoLuong * _sp.GiaBan;
                    ChiTietHD ct = new ChiTietHD();
                    ct.MaHD = orderID;
                    ct.MaSP = _sp.MaSP;
                    ct.SoLuong = sp.SoLuong;
                    ct.ThanhTien = _sp.GiaBan;
                    cthd.Add(ct);
                }

                hd.TongTien = totalPrice;

                List<PhieuXuatKho> dsPXK = await createWarehouseExport(db, cthd, hd);
                TheThanhVien ttv = _kh.TheThanhVien;
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

                db.HoaDons.Add(hd);
                db.ChiTietHDs.AddRange(cthd);
                db.PhieuXuatKhoes.AddRange(dsPXK);
                db.SaveChanges();

                if (ttv != null && td != null)
                {
                    hd.MaTichDiem = td.MaTD;
                    db.TichDiems.Add(td);
                    db.SaveChanges();
                    db.Entry(hd).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return true;
            }
        }

        [Authorize(Roles = "Admin, Manager, Employee")]
        public async Task<bool> PutStatus(string orderId, string status)
        {
            using (DbEntities db = new DbEntities())
            {
                string userId = User.Identity.GetUserId();
                NhanVien nv = await db.NhanViens.FirstOrDefaultAsync(t => t.AccountId == userId);
                if (nv == null) return false;

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

                    return true;
                }

                return false;
            }
        }

        [Authorize(Roles = "Admin, Manager, Employee")]
        public async Task<bool> PutPaymentStatus(string orderId, string pstatus)
        {
            using (DbEntities db = new DbEntities())
            {
                string userId = User.Identity.GetUserId();
                NhanVien nv = await db.NhanViens.FirstOrDefaultAsync(t => t.AccountId == userId);
                if (nv == null) return false;

                HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(t => t.MaHD == orderId && t.TrangThai != "cancelled");
                if (hd != null)
                {
                    if (pstatus == "paid" && hd.TrangThai != "cancelled")
                    {
                        hd.TrangThaiThanhToan = pstatus;
                        hd.MaNV = nv.MaNV;
                        db.SaveChanges();

                        return true;
                    }
                }

                return false;
            }
        }
    }
}