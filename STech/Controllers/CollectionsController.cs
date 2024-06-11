using Microsoft.Ajax.Utilities;
using STech.DTO;
using STech.Models;
using STech.OtherModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace STech.Controllers
{
    public class CollectionsController : Controller
    {
        // GET: Collections
        public async Task<ActionResult> Index(string id = "", string sort = "", string brand = "", string q = "", decimal? minprice = null, decimal? maxprice = null, int page = 1)
        {
            try
            {
                using (DbEntities db = new DbEntities())
                {
                    if (id.Length > 0 && (id != null || id.Equals("all")))
                    {
                        List<SanPhamDTO> dsSP = new List<SanPhamDTO>();
                        List<HangSxDTO> dsHSX = new List<HangSxDTO>();

                        string breadcrumbItem = "";

                        if (id == "all")
                        {
                            List<SanPham> db_dsSP = await db.SanPhams.ToListAsync();
                            dsSP = db_dsSP.Select(sp => new SanPhamDTO()
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
                            }).ToList();

                            breadcrumbItem = "Tất cả sản phẩm";
                        }
                        else
                        {
                            List<SanPham> db_dsSP = await db.SanPhams.Where(sp => sp.DanhMuc.MaDM.Equals(id)).ToListAsync();
                            dsSP = db_dsSP.Select(sp => new SanPhamDTO()
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
                            }).ToList();

                            DanhMuc dm = await db.DanhMucs.Where(t => t.MaDM.Equals(id)).FirstOrDefaultAsync();
                            breadcrumbItem = dm != null ? dm.TenDM : "";
                        }

                        dsHSX = dsSP.Select(p => p.HangSX).DistinctBy(hsx => hsx.MaHSX).ToList();

                        if (sort.Length > 0)
                        {
                            dsSP = Sort(sort, dsSP);
                        }

                        if (dsSP.Count > 0)
                        {
                            dsSP = Filter(dsSP, brand, q, minprice, maxprice);
                            ViewBag.Brand = brand;
                            ViewBag.Query = q;
                            ViewBag.MinPrice = minprice;
                            ViewBag.MaxPrice = maxprice;
                            if (!string.IsNullOrEmpty(ViewBag.filterName))
                            {
                                breadcrumbItem += " " + ViewBag.filterName;
                            }
                        }

                        List<Breadcrumb> breadcrumb = new List<Breadcrumb>();
                        breadcrumb.Add(new Breadcrumb("Trang chủ", "/"));
                        breadcrumb.Add(new Breadcrumb(breadcrumbItem, ""));
                        
                        if (sort.Length > 0)
                        {
                            dsSP = Sort(sort, dsSP);
                        }

                        dsSP = Pagination(dsSP, page);

                        ViewBag.MaDM = id;
                        ViewBag.title = breadcrumbItem;
                        ViewBag.Breadcrumb = breadcrumb;
                        ViewBag.sortValue = sort;

                        ViewBag.SanPham_HSX = dsHSX;

                        CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
                        ViewBag.cul = cul;
                        return View(dsSP);
                    }
                    return Redirect("/error/notfound");
                }    
            } 
            catch(Exception ex)
            {
                return Redirect("/error/notfound");
            }
        }

        public async Task<ActionResult> Search(string q = "", string sort = "", int page = 1)
        {
            using (DbEntities db = new DbEntities())
            {
                List<SanPhamDTO> dsSP = new List<SanPhamDTO>();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    string[] keywords = q.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<SanPham> db_dsSP = await db.SanPhams
                        .Where(sp => keywords.All(keyw => sp.TenSP.Contains(keyw)))
                        .ToListAsync();

                    dsSP = db_dsSP.Select(sp => new SanPhamDTO()
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = sp.GiaBan,
                        GiaGoc = sp.GiaGoc,
                        HinhAnh = sp.HinhAnhSPs != null ? sp.HinhAnhSPs.FirstOrDefault().DuongDan : null,
                        Tonkho = sp.ChiTietKhoes.Sum(ctk => ctk.SoLuong),
                        HangSX = new HangSxDTO()
                        {
                            MaHSX = sp.MaHSX,
                            TenHang = sp.HangSX.TenHang
                        }
                    })
                    .ToList();
                }

                if (sort.Length > 0)
                {
                    dsSP = Sort(sort, dsSP);
                }

                dsSP = Pagination(dsSP, page);

                ViewBag.searchValue = q;
                ViewBag.sortValue = sort;

                return View(dsSP);
            }  
        }

        public List<SanPhamDTO> Sort(string value, List<SanPhamDTO> dsSP)
        {
            List<SanPhamDTO> dsSp_sorted = new List<SanPhamDTO>();
            if (value == "price-ascending")
            {
                ViewBag.SortName = "Giá tăng dần";
                dsSp_sorted = dsSP.OrderBy(t => t.GiaBan).ToList();
            }
            else if (value == "price-descending")
            {
                ViewBag.SortName = "Giá giảm dần";
                dsSp_sorted = dsSP.OrderByDescending(t => t.GiaBan).ToList();
            }
            else if (value == "name-az")
            {
                ViewBag.SortName = "Tên A - Z";
                dsSp_sorted = dsSP.OrderBy(t => t.TenSP).ToList();
            }
            else if (value == "name-za")
            {
                ViewBag.SortName = "Tên Z - A";
                dsSp_sorted = dsSP.OrderByDescending(t => t.TenSP).ToList();
            }
            else
            {
                dsSp_sorted = dsSP.OrderBy(sp => Guid.NewGuid()).ToList();
                ViewBag.SortName = "Ngẫu nhiên";
            }

            return dsSp_sorted;
        }

        public List<SanPhamDTO> Pagination(List<SanPhamDTO> dsSP, int page)
        {
            //Paging ------
            int NoOfSanPhamPerPage = 40;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(
                Convert.ToDouble(dsSP.Count) / Convert.ToDouble(NoOfSanPhamPerPage)));
            int NoOfSanPhamToSkip = (page - 1) * NoOfSanPhamPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPages = NoOfPages;
            dsSP = dsSP.Skip(NoOfSanPhamToSkip).Take(NoOfSanPhamPerPage).ToList();

            return dsSP;
        }

        public List<SanPhamDTO> Filter(List<SanPhamDTO> dsSP, string brand, string query, decimal? minprice, decimal? maxprice)
        {
            if (dsSP.Count <= 0)
            {
                return new List<SanPhamDTO>();
            }

            string filterName = "";
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            List<string> brands = brand.Split(',').ToList();

            if (brands.Count > 1)
            {
                //Lọc sản phẩm theo nhiều thương hiệu
                dsSP = dsSP.Where(t => brands.Contains(t.HangSX.MaHSX)).ToList();
                filterName = textInfo.ToTitleCase(brands[0]);
                for(int i = 1; i < brands.Count; i++)
                {
                    filterName += ", " + textInfo.ToTitleCase(brands[i]);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(brand))
                {
                    //Lọc sản phẩm theo 1 thương hiệu
                    dsSP = dsSP.Where(t => t.HangSX.MaHSX.Equals(brand)).ToList();
                    filterName = textInfo.ToTitleCase(brand);
                }
            }

            if (!string.IsNullOrEmpty(query))
            {
                dsSP = dsSP.Where(t => t.TenSP.Contains(query)).ToList();
                if (dsSP.Count > 0)
                {
                    SanPhamDTO sp = dsSP[0];
                    Regex regex = new Regex(query, RegexOptions.IgnoreCase);
                    Match match = regex.Match(sp.TenSP);
                    filterName = sp.HangSX.TenHang.ToUpper() + " " + match.Value;
                }
                else
                {
                    filterName = textInfo.ToTitleCase(brand) + " " + textInfo.ToTitleCase(query);
                }
            }

            //Lọc sản phẩm theo giá cho trước
            if (minprice == null && maxprice != null)
            {
                dsSP = dsSP.Where(t => t.GiaBan <= maxprice).ToList();

            }
            else if (maxprice == null && minprice != null)
            {
                dsSP = dsSP.Where(t => t.GiaBan >= minprice).ToList();

            }
            else if (minprice != null && maxprice != null)
            {
                dsSP = dsSP.Where(t => t.GiaBan >= minprice && t.GiaBan <= maxprice).ToList();
            }

            ViewBag.filterName = filterName;
            return dsSP;
        }
    }
}