using Models;
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
            return result == null ? RedirectToAction("Index", "Home") : (ActionResult)View();
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
            return Json(new { error = "Cập nhập thành công !" }, JsonRequestBehavior.AllowGet);
        }

        // JSON: Admin/Business/update controller
        public async Task<ActionResult> Update()
        {
            // Lấy tất cả controller trong admin
            var ctl = Reflection.GetAllController("Web.Areas.Admin.Controllers");
            //Thêm vào db
            foreach (Type item in ctl)
            {
                Business bus = new Business();
                bus.BusinessId = item.Name.Replace("Controller", "");
                bus.BusinessName = "Đang cập nhập...";
                if (!await db.Businesses.AnyAsync(x => x.BusinessId == bus.BusinessId))
                {
                    //Nếu chưa có thỳ thêm mới
                    db.Businesses.Add(bus);
                    var getBusiness = await db.Businesses.FirstOrDefaultAsync(x => x.BusinessId == "Business");
                    var getGroups = await db.Businesses.FirstOrDefaultAsync(x => x.BusinessId == "Groups");
                    if (getBusiness != null)
                    {
                        getBusiness.Status = 3;
                    }
                    if (getGroups != null)
                    {
                        getGroups.Status = 3;
                    }
                    await db.SaveChangesAsync();
                }
            }
            return RedirectToAction("index", "Business");
        }
        #endregion
    }
}