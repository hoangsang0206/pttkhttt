using STech.DTO;
using STech.Models;
using STech.Utils;
using STech.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace STech.Controllers_api
{
    [Authorize(Roles = "Admin, Employee")]
    public class CustomersController : ApiController
    {
        public async Task<KhachHangDTO> Get(string id)
        {
            using (DbEntities db = new DbEntities())
            {
                KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.MaKH == id);

                if (kh == null) return null;

                return new KhachHangDTO()
                {
                    MaKH = kh.MaKH,
                    HoTen = kh.HoTen,
                    SDT = kh.SDT,
                    Email = kh.Email,
                    DiaChi = kh.DiaChi,
                    GioiTinh = kh.GioiTinh,
                    NgaySinh = kh.NgaySinh,
                };
            }
        }

        public async Task<KhachHangDTO> GetByPhone(string sdt)
        {
            using (DbEntities db = new DbEntities())
            {
                KhachHang kh = await db.KhachHangs.FirstOrDefaultAsync(t => t.SDT == sdt);

                if (kh == null) return null;

                return new KhachHangDTO()
                {
                    MaKH = kh.MaKH,
                    HoTen = kh.HoTen,
                    SDT = kh.SDT,
                    Email = kh.Email,
                    DiaChi = kh.DiaChi,
                    GioiTinh = kh.GioiTinh,
                    NgaySinh = kh.NgaySinh,
                };
            }
        }

        public async Task<IEnumerable<KhachHangDTO>> GetSearch(string phone)
        {
            using (DbEntities db = new DbEntities())
            {
                return await db.KhachHangs
                    .Where(kh => kh.SDT.Contains(phone))
                    .Select(kh => new KhachHangDTO()
                    {
                        MaKH = kh.MaKH,
                        HoTen = kh.HoTen,
                        SDT = kh.SDT,
                        Email = kh.Email,
                        DiaChi = kh.DiaChi,
                        GioiTinh = kh.GioiTinh,
                        NgaySinh = kh.NgaySinh,
                    })
                    .ToArrayAsync();
            }
        }

        public bool Post(CreateCustomerVM customer)
        {
            if (ModelState.IsValid)
            {
                using (DbEntities db = new DbEntities())
                {
                    KhachHang kh = new KhachHang();
                    kh.MaKH = "KH" + DateTime.Now.ToString("ddMMyy") + RandomString.random(5);
                    kh.HoTen = customer.HoTen;
                    kh.SDT = customer.SDT;
                    kh.Email = customer.Email;
                    kh.DiaChi = customer.DiaChi;
                    kh.GioiTinh = customer.GioiTinh;
                    kh.NgaySinh = customer.NgaySinh;

                    db.KhachHangs.Add(kh);
                    db.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        // PUT api/<controller>/5
        public async Task Put()
        {

        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}