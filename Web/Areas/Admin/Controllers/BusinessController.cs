using Models;
using Models.Common;
using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.Areas.Admin.Models;

namespace Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BusinessController : Controller
    {
        MobileShopContext db = new MobileShopContext();
        #region Bussiness
        // GET: Admin/Business
        public async Task<ActionResult> Index()
        {
            User result = await db.Users.SingleOrDefaultAsync(x => x.IsAdmin == true);
            return result == null ? RedirectToAction("Index", nameof(HomeController)) : (ActionResult)View();
        }
        // Json: Admin/Business/getall
        public ActionResult Getdata()
        {
            //stop load child object
            db.Configuration.ProxyCreationEnabled = false;
            var bus = db.Businesses.Where(x => x.Status == x.Status && x.Status != 3);
            return Json(new { data = bus }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update
        // JSON: Admin/Business/Edit
        [HttpPost]
        public async Task<ActionResult> Edit(string id)
        {
            //stop load child object
            db.Configuration.ProxyCreationEnabled = false;
            var result = await db.Businesses.FindAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        // JSON: Admin/Business/update controller
        [HttpPost]
        public async Task<ActionResult> EditSuccess(Business b)
        {
            //stop load child object
            db.Configuration.ProxyCreationEnabled = false;
            var result = await db.Businesses.Where(x => x.BusinessId == b.BusinessId).SingleOrDefaultAsync();
            if (result != null)
            {
                result.BusinessName = b.BusinessName;
                await db.SaveChangesAsync();
                return Json(new { success = "Cập nhập thành công !" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Not found !!" }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Update()
        {
            // get all controller trong admin
            var getController = Reflection.GetAllController("Web.Areas.Admin.Controllers");
            //add to database
            foreach (Type item in getController)
            {
                Business bus = new Business();
                bus.BusinessId = item.Name.Replace("Controller", "");
                if (!await db.Businesses.AnyAsync(x => x.BusinessId == bus.BusinessId))
                {
                    if (bus.BusinessId == "Business" || bus.BusinessId == "Groups")
                        bus.Status = 3;
                    else
                    //if no not have, add new
                    if(bus.BusinessId == "Home") bus.BusinessName = "Quản lý Feedback";
                    if(bus.BusinessId == "Cart") bus.BusinessName = "Quản lý giỏ hàng";
                    if(bus.BusinessId == "Categories") bus.BusinessName = "Quản lý danh mục sản phẩm";
                    if(bus.BusinessId == "News") bus.BusinessName = "Quản lý tin tức";
                    if(bus.BusinessId == "Products") bus.BusinessName = "Quản lý sản phẩm";
                    if(bus.BusinessId == "TypeAttr") bus.BusinessName = "Quản lý thuộc tính sản phẩm";
                    if(bus.BusinessId == "Users") bus.BusinessName = "Quản lý nhân viên";
                    db.Businesses.Add(bus);
                    await db.SaveChangesAsync();
                }
            }
            return RedirectToAction("index", "Business");
        }
        #endregion
    }
}