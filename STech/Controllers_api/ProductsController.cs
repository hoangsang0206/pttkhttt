using STech.DTO;
using STech.Models;
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
    public class ProductsController : ApiController
    {
        public async Task<IEnumerable<SanPhamDTO>> Get()
        {
            using (DbEntities db = new DbEntities())
            {
                return await db.SanPhams
                    .Select(sp => new SanPhamDTO
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong)
                    }).ToListAsync();
            }
        }

        public async Task<IEnumerable<SanPhamDTO>> GetSearch(string q)
        {
            using (DbEntities db = new DbEntities())
            {

                string[] keywords = q.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return await db.SanPhams
                    .Where(sp => keywords.All(keyw => sp.TenSP.Contains(keyw)))
                    .Select(sp => new SanPhamDTO
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong)
                    }).ToListAsync();
            }
        }

        public async Task<IEnumerable<SanPhamDTO>> GetOutOfStock(string oot)
        {
            using (DbEntities db = new DbEntities())
            {
                return await db.SanPhams
                    .Where(sp => sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong) <= 0)
                    .Select(sp => new SanPhamDTO
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong)
                    }).ToListAsync();
            }   
        }

        public async Task<IEnumerable<SanPhamDTO>> GetByCategory(string category)
        {
            using (DbEntities db = new DbEntities())
            {
                return await db.SanPhams
                .Where(sp => sp.MaDM.Equals(category))
                .Select(sp => new SanPhamDTO
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaGoc = sp.GiaGoc,
                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                    Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong)
                }).ToListAsync();
            }
        }

        public async Task<IEnumerable<SanPhamDTO>> GetByBrand(string brand)
        {
            using (DbEntities db = new DbEntities())
            {
                return await db.SanPhams
                    .Where(sp => sp.MaHSX.Equals(brand))
                    .Select(sp => new SanPhamDTO
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong)
                    }).ToListAsync();
            }    
        }

        public IEnumerable<SanPhamDTO> GetByCategoryAndBrand(string category, string brand)
        {
            using (DbEntities db = new DbEntities())
            {
                return db.SanPhams
                    .Where(sp => sp.MaDM.Equals(category) && sp.MaHSX.Equals(brand))
                    .Select(sp => new SanPhamDTO
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong)
                    });
            }  
        }
    }
}