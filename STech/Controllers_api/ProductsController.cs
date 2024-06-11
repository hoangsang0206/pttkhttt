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
                List<SanPham> dsSP = await db.SanPhams
                    .Include(sp => sp.HinhAnhSPs)
                    .Include(sp => sp.ChiTietKhoes)
                    .Include(sp => sp.DanhMuc)
                    .Include(sp => sp.HangSX)
                    .ToListAsync(); 

                return dsSP.Select(sp => new SanPhamDTO()
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaGoc = sp.GiaGoc,
                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                    Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                    DanhMuc = new DanhMucDTO()
                    {
                        MaDM = sp.MaDM,
                        TenDM = sp.DanhMuc.TenDM
                    },
                    HangSX = new HangSxDTO()
                    {
                        MaHSX = sp.MaHSX,
                        TenHang = sp.HangSX.TenHang
                    }
                });
            }
        }

        public async Task<IEnumerable<SanPhamDTO>> GetSearch(string q)
        {
            using (DbEntities db = new DbEntities())
            {
                string[] keywords = q.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<SanPham> dsSP = await db.SanPhams
                    .Where(sp => keywords.All(keyw => sp.TenSP.Contains(keyw)))
                    .Include(sp => sp.HinhAnhSPs)
                    .Include(sp => sp.ChiTietKhoes)
                    .Include(sp => sp.DanhMuc)
                    .Include(sp => sp.HangSX)
                    .ToListAsync();

                return dsSP.Select(sp => new SanPhamDTO()
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaGoc = sp.GiaGoc,
                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                    Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                    DanhMuc = new DanhMucDTO()
                    {
                        MaDM = sp.MaDM,
                        TenDM = sp.DanhMuc.TenDM
                    },
                    HangSX = new HangSxDTO()
                    {
                        MaHSX = sp.MaHSX,
                        TenHang = sp.HangSX.TenHang
                    }
                });
            }
        }

        public async Task<IEnumerable<SanPhamDTO>> GetOutOfStock(string oot)
        {
            using (DbEntities db = new DbEntities())
            {
                List<SanPham> dsSP = await db.SanPhams
                    .Where(sp => sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong) <= 0)
                    .Include(sp => sp.HinhAnhSPs)
                    .Include(sp => sp.ChiTietKhoes)
                    .Include(sp => sp.DanhMuc)
                    .Include(sp => sp.HangSX)
                    .ToListAsync();

                return dsSP.Select(sp => new SanPhamDTO()
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaGoc = sp.GiaGoc,
                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                    Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                    DanhMuc = new DanhMucDTO()
                    {
                        MaDM = sp.MaDM,
                        TenDM = sp.DanhMuc.TenDM
                    },
                    HangSX = new HangSxDTO()
                    {
                        MaHSX = sp.MaHSX,
                        TenHang = sp.HangSX.TenHang
                    }
                });
            }   
        }

        public async Task<IEnumerable<SanPhamDTO>> GetByCategory(string category)
        {
            using (DbEntities db = new DbEntities())
            {
                List<SanPham> dsSP = await db.SanPhams
                .Where(sp => sp.MaDM.Equals(category))
                .Include(sp => sp.HinhAnhSPs)
                .Include(sp => sp.ChiTietKhoes)
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.HangSX)
                .ToListAsync();

                return dsSP.Select(sp => new SanPhamDTO()
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaGoc = sp.GiaGoc,
                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                    Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                    DanhMuc = new DanhMucDTO()
                    {
                        MaDM = sp.MaDM,
                        TenDM = sp.DanhMuc.TenDM
                    },
                    HangSX = new HangSxDTO()
                    {
                        MaHSX = sp.MaHSX,
                        TenHang = sp.HangSX.TenHang
                    }
                });
            }
        }

        public async Task<IEnumerable<SanPhamDTO>> GetByBrand(string brand)
        {
            using (DbEntities db = new DbEntities())
            {
                List<SanPham> dsSP = await db.SanPhams
                    .Where(sp => sp.MaHSX.Equals(brand))
                    .Include(sp => sp.HinhAnhSPs)
                    .Include(sp => sp.ChiTietKhoes)
                    .Include(sp => sp.DanhMuc)
                    .Include(sp => sp.HangSX)
                    .ToListAsync();

                return dsSP.Select(sp => new SanPhamDTO()
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaGoc = sp.GiaGoc,
                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                    Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                    DanhMuc = new DanhMucDTO()
                    {
                        MaDM = sp.MaDM,
                        TenDM = sp.DanhMuc.TenDM
                    },
                    HangSX = new HangSxDTO()
                    {
                        MaHSX = sp.MaHSX,
                        TenHang = sp.HangSX.TenHang
                    }
                });
            }    
        }

        public async Task<IEnumerable<SanPhamDTO>> GetByCategoryAndBrand(string category, string brand)
        {
            using (DbEntities db = new DbEntities())
            {
                List<SanPham> dsSP = await db.SanPhams
                    .Where(sp => sp.MaDM.Equals(category) && sp.MaHSX.Equals(brand))
                    .Include(sp => sp.HinhAnhSPs)
                    .Include(sp => sp.ChiTietKhoes)
                    .Include(sp => sp.DanhMuc)
                    .Include(sp => sp.HangSX)
                    .ToListAsync();

                return dsSP.Select(sp => new SanPhamDTO()
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaGoc = sp.GiaGoc,
                    HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                    Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                    DanhMuc = new DanhMucDTO()
                    {
                        MaDM = sp.MaDM,
                        TenDM = sp.DanhMuc.TenDM
                    },
                    HangSX = new HangSxDTO()
                    {
                        MaHSX = sp.MaHSX,
                        TenHang = sp.HangSX.TenHang
                    }
                });
            }  
        }
    }
}