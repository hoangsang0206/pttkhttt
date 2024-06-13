using STech.DTO;
using STech.Models;
using STech.OtherModels;
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
    public class ProductController : Controller
    {
        // GET: Product
        public async Task<ActionResult> Index(string id)
        {
            if(id != null && id.Length > 0)
            {
                using (DbEntities db = new DbEntities())
                {
                    SanPham sp = await db.SanPhams.FirstOrDefaultAsync(s => s.MaSP == id);
                    if(sp == null)
                    {
                        return Redirect("/error/notfound");
                    }

                    SanPhamDTO _sp = new SanPhamDTO()
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        DsHinhAnh = sp.HinhAnhSPs.Select(t => t.DuongDan).ToList(),
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                        ThongSoKyThuat = sp.ThongSoKyThuats.Select(t => new ThongSoDTO()
                        {
                            Id = t.Id,
                            TenTS = t.TenTS,
                            MoTa = t.MoTa,
                        }).ToList(),
                        HangSX = new HangSxDTO()
                        {
                            MaHSX = sp.HangSX.MaHSX,
                            TenHang = sp.HangSX.TenHang
                        }
                    };

                    DanhMuc danhmuc = sp.DanhMuc;

                    List<SanPham> dsSP = await db.SanPhams
                        .Where(t => t.MaDM == danhmuc.MaDM && danhmuc.MaDM != "khac")
                        .Include(t => t.HinhAnhSPs)
                        .OrderBy(t => Guid.NewGuid())
                        .Take(15)
                        .ToListAsync();
                    List<SanPhamDTO> _dsSP = dsSP
                        .Select(t => new SanPhamDTO()
                        {
                            MaSP = t.MaSP,
                            TenSP = t.TenSP,
                            GiaBan = t.GiaBan,
                            GiaGoc = t.GiaGoc,
                            HinhAnh = t.HinhAnhSPs != null ? t.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                            Tonkho = t.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                            HangSX = new HangSxDTO()
                            {
                                MaHSX = t.HangSX.MaHSX,
                                TenHang = t.HangSX.TenHang
                            }
                        }).ToList();

                    List<Breadcrumb> breadcrumb = new List<Breadcrumb>
                    {
                        new Breadcrumb("Trang chủ", "/"),
                        new Breadcrumb(danhmuc.TenDM, "/collections/" + danhmuc.MaDM),
                        new Breadcrumb(sp.TenSP, "")
                    };

                    ViewBag.Breadcrumb = breadcrumb;

                    CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
                    ViewBag.cul = cul;

                    return View(new Tuple<SanPhamDTO, List<SanPhamDTO>>(_sp, _dsSP));
                }
            }

            return Redirect("/error/notfound");
        }
    }
}