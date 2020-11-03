using Models;
using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Areas.Admin.Models;

namespace Web.Areas.Admin.Controllers
{
    public class BusinessController : Controller
    {
        MobileShopContext db = new MobileShopContext();
        #region Bussiness
        // GET: Admin/Business
        public ActionResult Index()
        {
            User result = db.Users.SingleOrDefault(x => x.IsAdmin == true);
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
        public ActionResult Edit(string id)
        {
            //stop load child object
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.Businesses.Where(x => x.BusinessId == id).SingleOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditSuccess(Business b)
        {
            //stop load child object
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.Businesses.Where(x => x.BusinessId == b.BusinessId).SingleOrDefault();
            if (result != null)
            {
                result.BusinessName = b.BusinessName;
                db.SaveChanges();
            }
            return Json(new { result, success = "Cập nhập thành công !" }, JsonRequestBehavior.AllowGet);
        }

        // JSON: Admin/Business/update controller
        public ActionResult Update()
        {
            // Lấy tất cả controller trong admin
            var ctl = Reflection.GetAllController("Web.Areas.Admin.Controllers");
            //Thêm vào db
            foreach (Type item in ctl)
            {
                Business bus = new Business();
                bus.BusinessId = item.Name.Replace("Controller", "");
                bus.BusinessName = "Đang cập nhập...";
                if (!db.Businesses.Any(x => x.BusinessId == bus.BusinessId))
                {
                    //Nếu chưa có thỳ thêm mới
                    db.Businesses.Add(bus);
                    var getBusiness = db.Businesses.FirstOrDefault(x => x.BusinessId == "Business");
                    var getGroups = db.Businesses.FirstOrDefault(x => x.BusinessId == "Groups");
                    if (getBusiness != null)
                    {
                        getBusiness.Status = 3;
                    }
                    if (getGroups != null)
                    {
                        getGroups.Status = 3;
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("index", "Business");
        }
        #endregion
    }
}