using Models;
using Models.Models.DataModels;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.Areas.Admin.Models;
using Web.Controllers;

namespace Web.Areas.Admin.Controllers
{
    [CustomAuth(Roles = "VIEW")]
    public class ProductsController : BaseController
    {
        MobileShopContext db = new MobileShopContext();
        #region Products
        // GET: Admin/Products
        public ActionResult Index()
        {
            return View(db.Products.Where(x => x.Status == true).ToList());
        }
        #endregion

        #region Create Product
        //[CustomAuth(Roles = "ADD")]
        public async Task<ActionResult> Create()
        {
            var countProvider = await db.Providers.Where(x => x.Status == 1).CountAsync();
            var countCategories = await db.Categories.Where(x => x.Status == 1).CountAsync();
            ViewBag.ProviderId = new SelectList(await db.Providers.Where(x => x.Status == 1).ToListAsync(), "ProviderId", "ProviderName");
            ViewBag.category = await db.Categories.ToListAsync();
            if (countProvider <= 0)
            {
                setAlert("Error !", "Chưa có thương hiệu, vui lòng thêm mới thương hiệu trước khi thêm mới sản phẩm !!", "top-right", "error", 7000);
                return RedirectToAction("Providers", "Categories");
            }
            if (countCategories <= 0)
            {
                setAlert("Error !", "Chưa có danh mục, vui lòng thêm mới danh mục trước khi thêm mới sản phẩm !!", "top-right", "error", 7000);
                return RedirectToAction("Index", "Categories");
            }
            //Lấy thuộc tính sản phẩm
            ViewBag.TypeAttr = await db.TypeAttrs.Include(x => x.Attributes).Where(x => x.Attributes.Count() > 0).ToListAsync();
            return View();
        }
        //[CustomAuth(Roles = "ADD")]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductViewModel p)
        {
            ViewBag.ProviderId = new SelectList(await db.Providers.Where(x => x.Status == 1).ToListAsync(), "ProviderId", "ProviderName");
            ViewBag.category = await db.Categories.ToListAsync();
            ViewBag.TypeAttr = await db.TypeAttrs.Include(x => x.Attributes).Where(x => x.Attributes.Count() > 0).ToListAsync();
            if (ViewBag.ProviderId == null)
            {
                return RedirectToAction("CreateProvider", "Categories");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    Product product = new Product();
                    product.ProductName = p.ProductName;
                    product.CategoryId = p.CategoryId;
                    product.ProviderId = p.ProviderId;
                    product.PriceIn = p.PriceIn;
                    product.PriceOut = p.PriceOut;
                    product.Discount = p.Discount;
                    product.Quantity = p.Quantity;
                    product.FeatureImage = p.FeatureImage;
                    product.Images = p.Images;
                    product.Description = p.Description;
                    product.Specifications = p.Specifications;
                    product.ProductDetail = p.ProductDetail;
                    product.CreateDate = DateTime.Now;
                    product.Condition = p.Condition;//trạng thái sản phẩm
                    product.Status = true;
                    product.ProductAttrs = p.ProductAttrs;
                    db.Products.Add(product);
                    await db.SaveChangesAsync();
                    setAlert("Success !", "Bạn đã thêm mới sản phẩm thành công !", "top-right", "success", 4000);
                    return RedirectToAction("Index", "Products");
                }
                catch (Exception)
                {
                    setAlert("Error !", "Có lỗi khi thêm mới sản phẩm !", "top-right", "error", 5000);
                    return View(p);
                }
            }
            return View(p);
        }
        #endregion

        #region Update Product
        [ValidateInput(false)]
        //[CustomAuth(Roles = "EDIT")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Unauthorized");
            }
            ViewBag.category = await db.Categories.ToListAsync();
            Product product = await db.Products.Include(x => x.ProductAttrs).SingleOrDefaultAsync(x => x.ProductId == id);
            ViewBag.currentCategory = product.CategoryId;
            if (product == null || product.Status == false)
            {
                return View("Unauthorized");
            }
            //Lấy thuộc tính sản phẩm
            ViewBag.TypeAttr = await db.TypeAttrs.Include(x => x.Attributes).Where(x => x.Attributes.Count() > 0).ToListAsync();
            ViewBag.ProviderId = new SelectList(await db.Providers.Where(x => x.Status == 1).ToListAsync(), "ProviderId", "ProviderName", product.ProviderId);
            return View(product);
        }

        //[CustomAuth(Roles = "EDIT")]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product p)
        {
            ViewBag.category = await db.Categories.ToListAsync();
            ViewBag.TypeAttr = await db.TypeAttrs.Include(x => x.Attributes).Where(x => x.Attributes.Count() > 0).ToListAsync();
            ViewBag.ProviderId = new SelectList(await db.Providers.Where(x => x.Status == 1).ToListAsync(), "ProviderId", "ProviderName", p.ProviderId);
            if (ModelState.IsValid)
            {
                try
                {
                    var _product = await db.Products.SingleOrDefaultAsync(x => x.ProductId == p.ProductId && x.Status == true);
                    if (_product != null)
                    {
                        if (p.FeatureImage != null)
                        {
                            _product.ProductName = p.ProductName;
                            _product.CategoryId = p.CategoryId;
                            _product.ProviderId = p.ProviderId;
                            _product.PriceIn = p.PriceIn;
                            _product.PriceOut = p.PriceOut;
                            _product.Discount = p.Discount;
                            _product.Quantity = p.Quantity;
                            _product.Description = p.Description;
                            _product.Specifications = p.Specifications;
                            _product.ProductDetail = p.ProductDetail;
                            _product.FeatureImage = p.FeatureImage;
                            _product.Images = p.Images;
                            _product.Condition = p.Condition;
                            db.ProductAttrs.RemoveRange(db.ProductAttrs.Where(x => x.ProductId == p.ProductId));
                            //thêm attr mới
                            if (p.ProductAttrs != null)
                            {
                                foreach (var item in p.ProductAttrs)
                                {
                                    item.ProductId = p.ProductId;
                                }
                                db.ProductAttrs.AddRange(p.ProductAttrs);
                                _product.ProductAttrs = p.ProductAttrs;
                            }
                            await db.SaveChangesAsync();
                            setAlert("Success !", "Sửa sản phẩm thành công !", "top-right", "success", 4000);
                            ModelState.Clear();
                            return RedirectToAction("Index", "Products");
                        }
                        else
                        {
                            ModelState.AddModelError("FeatureImage", "Vui lòng chọn ảnh sản phẩm");
                            return View(p);
                        }
                    }
                    else
                    {
                        setAlert("Error !", "Khôn tìm thấy sản phẩm !", "top-right", "error", 5000);
                        return View(p);
                    }

                }
                catch (Exception)
                {
                    setAlert("Error !", "Có lỗi khi sửa sản phẩm !", "top-right", "error", 5000);
                    return View(p);
                }
            }
            return View(p);
        }
        #endregion

        #region Delete Product
        //[CustomAuth(Roles = "DELETE")]
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            Product result = await db.Products.Where(x => x.ProductId == id && x.Status == true).SingleOrDefaultAsync();
            if (result == null) return Json(new { error = "Có gì đó không đúng !" }, JsonRequestBehavior.AllowGet);
            result.Status = false;
            await db.SaveChangesAsync();
            return Json(new { success = "Xoá thành công !" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Banners
        public ActionResult Banner()
        {
            return View();
        }

        public JsonResult GetAllBanner()
        {
            var banner = db.Banners.Where(x => x.Status == 1 || x.Status == 0);
            return Json(new { data = banner }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create Banner
        public ActionResult CreateBanner()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateBanner(Banner banner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var countBanner = await db.Banners.Where(x => x.Status == 1 || x.Status == 0).CountAsync();
                    if (banner.DescriptionBanner == null)
                        banner.DescriptionBanner = "Không có mô tả";
                    banner.Orderby = countBanner + 1;
                    db.Banners.Add(banner);
                    await db.SaveChangesAsync();
                    setAlert("Success !", "Thêm mới thành công !", "top-right", "success", 4000);
                    return RedirectToAction("Banner", "Products");
                }
                catch (Exception)
                {
                    setAlert("Error !", "Không thể thêm mới, vui lòng thử lại!", "top-right", "error", 4000);
                    return View(banner);
                }
            }
            return View(banner);
        }
        #endregion

        #region Update Banner
        public ActionResult EditBanner(int? id)
        {
            if (id == null)
            {
                setAlert("Error !", "Không tìm thấy Banner!", "top-right", "error", 4000);
                return RedirectToAction("Banner", "Products");
            }
            Banner banner = db.Banners.Find(id);
            return View(banner);
        }
        [HttpPost]
        public async Task<ActionResult> EditBanner(Banner banner)
        {
            var result = await db.Banners.Where(x => x.BannerId == banner.BannerId).FirstOrDefaultAsync();
            if (result == null)
            {
                setAlert("Error !", "Không tìm thấy Banner!", "top-right", "error", 4000);
                return RedirectToAction("Banner", "Products");
            }
            try
            {
                var sortOrderby = await db.Banners.Where(x => x.Status == 1 || x.Status == 0).ToListAsync();
                result.DescriptionBanner = banner.DescriptionBanner;
                result.BannerImage = banner.BannerImage;
                result.Orderby = banner.Orderby;
                result.Status = banner.Status;
                int count = banner.Orderby;
                foreach (var item in sortOrderby)
                {
                    if (item.BannerId != banner.BannerId && item.Orderby >= banner.Orderby)
                    {
                        item.Orderby = ++count;
                    }
                }
                await db.SaveChangesAsync();
                setAlert("Success !", "Chỉnh sửa thành công thành công !", "top-right", "success", 4000);
                return RedirectToAction("Banner", "Products");
            }
            catch (Exception)
            {
                setAlert("Error !", "Không tìm thấy Banner!", "top-right", "error", 4000);
                return View(banner);
            }
        }
        #endregion

        #region Delete Banner
        [HttpPost]
        public async Task<JsonResult> DeleteBanner(int id)
        {
            var banner = await db.Banners.FirstOrDefaultAsync(x => x.BannerId == id);
            var sortOrderby = db.Banners.Where(x => x.Status == 1 || x.Status == 0);
            if (banner != null)
            {
                banner.Status = -1; //
                await db.SaveChangesAsync();
                int count = 1;
                foreach (var item in sortOrderby)
                {
                    item.Orderby = count++;
                }
                await db.SaveChangesAsync();
                return Json(new { success = "Xoá thành công" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Có lỗi khi xoá" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}