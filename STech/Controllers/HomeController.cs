using STech.DTO;
using STech.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace STech.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            using (DbEntities db = new DbEntities())
            {
                List<List<SanPhamDTO>> _dsSanPham = new List<List<SanPhamDTO>>();

                List<Slider> sliders = await db.Sliders.ToListAsync();
                List<Banner> banner = await db.Banners.ToListAsync();
                List<HangSX> hangSx = await db.HangSXes.Where(t => t.MaHSX != "khac" && t.HinhAnh != null).ToListAsync();

                List<DanhMuc> danhmuc = await db.DanhMucs.Where(t => t.MaDM != "khac").ToListAsync();
                List<DanhMuc> randomDM = danhmuc.Where(d => d.SanPhams.Count >= 6).OrderBy(d => Guid.NewGuid()).Take(8).ToList();

                foreach (DanhMuc d in randomDM)
                {
                    _dsSanPham.Add(d.SanPhams.Select(sp => new SanPhamDTO()
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                        DanhMuc = new DanhMucDTO() 
                        { 
                            MaDM = sp.DanhMuc.MaDM,
                            TenDM = sp.DanhMuc.TenDM
                        }
                    }).Take(15).ToList());
                }

                //----------
                ViewBag.Sliders = sliders;
                ViewBag.Banner = banner;
                ViewBag.HangSX = hangSx;
                ViewBag.DanhMuc = danhmuc;
                ViewBag.ActiveBotNav = "home";

                CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
                ViewBag.cul = cul;
                return View(_dsSanPham);
            }
        }
    }
}
