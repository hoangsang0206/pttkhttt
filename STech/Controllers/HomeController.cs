using STech.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace STech.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            DbEntities db = new DbEntities();

            List<List<SanPham>> _dsSanPham = new List<List<SanPham>>();

            List<Slider> sliders = db.Sliders.ToList();
            List<Banner> banner = db.Banners.ToList();
            List<HangSX> hangSx = db.HangSXes.Where(t => t.MaHSX != "khac" && t.HinhAnh != null).ToList();

            List<DanhMuc> danhmuc = db.DanhMucs.Where(t => t.MaDM != "khac").ToList();
            List<DanhMuc> randomDM = danhmuc.Where(d => d.SanPhams.Count >= 6).OrderBy(d => Guid.NewGuid()).Take(8).ToList();

            foreach(DanhMuc d in randomDM)
            {
                _dsSanPham.Add(d.SanPhams.Take(15).ToList());
            }

            //----------
            ViewBag.Sliders = sliders;
            ViewBag.Banner = banner;
            ViewBag.HangSX = hangSx;
            ViewBag.DanhMuc = danhmuc;
            ViewBag.ActiveBotNav = "home";

            //Dùng để chuyển sang định dạng số có dấu phân cách phần nghìn
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            ViewBag.cul = cul;
            return View(_dsSanPham);
        }
    }
}
