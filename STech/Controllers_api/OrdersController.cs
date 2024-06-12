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

        public async Task<HoaDonDTO> Get(string id)
        {
            using (DbEntities db = new DbEntities())
            {
                if(checkCompanyRole())
                {
                    HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(h => h.MaHD == id);
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
                else if (checkCustomerRole())
                {
                    string userId = User.Identity.GetUserId();
                    HoaDon hd = await db.HoaDons.FirstOrDefaultAsync(h => h.MaHD == id && h.KhachHang.AccountId == userId);
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

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}