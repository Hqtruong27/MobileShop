using Models;
using Models.Models.DataModels;
using Models.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.Areas.Admin.Controllers
{
    public class CategoriesController : BaseController
    {
        MobileShopContext db = new MobileShopContext();
        // GET: Admin/Categories
        public ActionResult Index()
        {
            return View(db.Categories.Where(x => (x.Status == 1 || x.Status == 0) && x.ParentId == null).OrderBy(x => x.Orderby).ToList());
        }
        public async Task<ActionResult> Getdata()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var categories = await db.Categories.Where(x => x.Status == 1 && x.ParentId == null).OrderBy(x => x.Orderby).ToListAsync();
            return Json(new { data = categories }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Create Categories
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Create()
        {
            var categories = await db.Categories.Where(x => x.Status == 1 && x.ParentId == null).OrderBy(x => x.Orderby).ToListAsync();
            ViewBag.ParentId = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCategories c)
        {
            ViewBag.ParentId = new SelectList(await db.Categories.Where(x => x.Status == 1 && x.ParentId == null).OrderBy(x => x.Orderby).ToListAsync(), "CategoryId", "CategoryName");
            if (ModelState.IsValid)
            {
                var sortOderbyNull = await db.Categories.Where(x => x.ParentId == null && x.Status != 10).OrderBy(x => x.Orderby).CountAsync();
                var sortOderbyNotNull = await db.Categories.Where(x => x.ParentId != null && x.Status != 10).OrderBy(x => x.Orderby).CountAsync();
                Category cate = new Category();
                if (c.ParentId == null)
                {
                    cate.Orderby = sortOderbyNull + 1;
                }
                else
                {
                    cate.Orderby = sortOderbyNotNull + 1;
                }
                cate.Status = 1;
                cate.CategoryName = c.CategoryName;
                cate.ParentId = c.ParentId;
                db.Categories.Add(cate);
                await db.SaveChangesAsync();
                if (cate.ParentId == null)
                {
                    return RedirectToAction("Index", "Categories");
                }
                else
                {
                    return RedirectToAction("ParentCate", "Categories");
                }
            }
            return View(c);
        }

        /// <summary>
        /// Edit Categories
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Unauthorized");
            }
            var category = await db.Categories.Where(x => x.CategoryId == id).SingleOrDefaultAsync();
            if (category == null)
            {
                return View("Unauthorized");
            }
            var findThisHasParentCategory = await db.Categories.Where(x => x.ParentId != null && x.ParentId == category.CategoryId).CountAsync();
            if (findThisHasParentCategory > 0)
            {
                ViewBag.showmsg = "Không thể chọn làm danh mục con vì danh mục này đã chứa danh mục con !";
                return View(category);

            }
            ViewBag.ParentId = new SelectList(await db.Categories.Where(x => x.Status == 1 && x.ParentId == null && x.CategoryId != id).OrderBy(x => x.Orderby).ToListAsync(), "CategoryId", "CategoryName", category.ParentId);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Category c)
        {
            ViewBag.ParentId = new SelectList(await db.Categories.Where(x => x.Status == 1 && x.ParentId == null && x.CategoryId != c.CategoryId).OrderBy(x => x.Orderby).ToListAsync(), "CategoryId", "CategoryName", c.ParentId);
            var editCate = await db.Categories.SingleOrDefaultAsync(x => x.CategoryId == c.CategoryId);
            var listToOrderingParentNull = await db.Categories.Where(x => (x.Status == 0 || x.Status == 1) && x.ParentId == null).ToListAsync();
            var listToOrderingParentNotNull = await db.Categories.Where(x => (x.Status == 0 || x.Status == 1) && x.ParentId != null).ToListAsync();
            var sortParentCategory = 0;
            var sortCategory = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    if (editCate != null)
                    {
                        editCate.CategoryName = c.CategoryName;
                        editCate.Status = c.Status;
                        editCate.ParentId = c.ParentId;
                        editCate.Orderby = c.Orderby;
                        if (c.ParentId == null)
                        {
                            var sortOrderby = c.Orderby;
                            foreach (var item in listToOrderingParentNull)
                            {
                                if (item.CategoryId != c.CategoryId && item.Orderby >= c.Orderby)
                                {
                                    item.Orderby = ++sortOrderby;
                                }
                            }
                            foreach (var item in listToOrderingParentNotNull)
                            {
                                item.Orderby = ++sortParentCategory;
                            }
                        }
                        else
                        {
                            var sortOrderby = c.Orderby;
                            foreach (var item in listToOrderingParentNotNull)
                            {
                                if (item.CategoryId != c.CategoryId && item.Orderby >= c.Orderby)
                                {
                                    item.Orderby = ++sortOrderby;
                                }
                            }
                            foreach (var item in listToOrderingParentNull)
                            {
                                item.Orderby = ++sortCategory;
                            }
                        }
                        await db.SaveChangesAsync();
                        if (editCate.ParentId == null)
                        {
                            return RedirectToAction("Index", "Categories");
                        }
                        return RedirectToAction("ParentCate", "Categories");
                    }
                }
                catch (Exception)
                {
                    return RedirectToAction("Edit", "Categories");
                }
            }
            return View(c);
        }
        [HttpPost]

        //JSON/Categories/delete category
        public async Task<JsonResult> Delete(int id)
        {
            var result = await db.Categories.SingleOrDefaultAsync(x => (x.Status == 0 || x.Status == 1) && x.CategoryId == id);
            var sortOrderby = await db.Categories.Where(x => (x.Status == 0 || x.Status == 1) && x.ParentId == null && x.CategoryId != id).OrderBy(x => x.Orderby).ToListAsync();
            var sortOrderbyParent = await db.Categories.Where(x => (x.Status == 0 || x.Status == 1) && x.ParentId != null && x.CategoryId != id).OrderBy(x => x.Orderby).ToListAsync();
            if (result != null)
            {
                result.Status = 10; //delete with status = 10;
                if (result.ParentId == null)
                {
                    var oderby = 1;
                    foreach (var item in sortOrderby)
                    {
                        item.Orderby = oderby++;
                    }
                }
                else
                {
                    var oderby = 1;
                    foreach (var item in sortOrderbyParent)
                    {
                        item.Orderby = oderby++;
                    }
                }
                await db.SaveChangesAsync();
                return Json(new { success = "Xoá thành công !" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng !" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Categories Parent List and Create new Parnet use JSON
        /// </summary>
        /// <returns></returns>
        public ActionResult ParentCate()
        {
            return View();
        }
        public async Task<ActionResult> GetdataParentCate()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = await db.Categories.Where(x => (x.Status == 1 || x.Status == 0) && x.ParentId != null).OrderBy(x => x.Orderby).ToListAsync();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //View/JSON/Providers/getall
        public ActionResult Providers()
        {
            return View();
        }
        public async Task<JsonResult> GetdataProviders()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var data = await db.Providers.Where(x => x.Status == 1 || x.Status == 0).ToListAsync();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        //JSON/Providers/create new provider
        public async Task<JsonResult> CreateProvider(Provider provider)
        {
            if (ModelState.IsValid)
            {
                var countProvider = await db.Providers.Where(x => x.Status != 10).CountAsync();
                provider.Orderby = countProvider + 1;
                db.Providers.Add(provider);
                await db.SaveChangesAsync();
                return Json(new { success = "Thêm mới thành công !!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng !!" }, JsonRequestBehavior.AllowGet);
            }
        }

        //JSON/Providers/Getid
        public async Task<JsonResult> GetIdProvider(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var provider = await db.Providers.FirstOrDefaultAsync(x => (x.Status == 1 || x.Status == 0) && x.ProviderId == id);
            return Json(provider, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<JsonResult> EditProvider(Provider provider)
        {
            var result = await db.Providers.Where(x => (x.Status == 1 || x.Status == 0) && x.ProviderId == provider.ProviderId).FirstOrDefaultAsync();
            var sortProvider = await db.Providers.OrderBy(x => x.Orderby).Where(x => x.Status != 10 && x.ProviderId != provider.ProviderId && x.Orderby >= provider.Orderby).ToListAsync();
            if (result != null)
            {
                if (result.Orderby != provider.Orderby)
                {
                    var order = provider.Orderby;
                    foreach (var item in sortProvider)
                    {
                        item.Orderby = ++order;
                    }
                }
                result.Orderby = provider.Orderby;
                result.ProviderName = provider.ProviderName;
                result.Status = provider.Status;
                await db.SaveChangesAsync();
                return Json(new { success = "Chỉnh sửa thành công !!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng !!" }, JsonRequestBehavior.AllowGet);
            }
        }

        //JSON/Providers/delete provider
        public async Task<JsonResult> DeleteProvider(int id)
        {
            var provider = await db.Providers.FirstOrDefaultAsync(x => (x.Status == 1 || x.Status == 0) && x.ProviderId == id);
            if (provider != null)
            {
                provider.Status = 10; //delete with status = 10;
                await db.SaveChangesAsync();
                return Json(new { success = "Xoá thành công !!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng!!" }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}