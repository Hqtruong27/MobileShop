using Models;
using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Web.Areas.Admin.Models;

namespace Web.Areas.Admin.Controllers
{
    [CustomAuth]
    public class GroupsController : Controller
    {
        MobileShopContext db = new MobileShopContext();
        #region Group Roles
        // GET: Admin/Groups
        public ActionResult Index()
        {
            var getuser = HttpContext.Session["User"] as User;
            if (getuser.IsAdmin == true)
            {
                ViewBag.business = db.Businesses.Where(x => x.Status == x.Status && x.Status != 3).ToList();
                ViewBag.groups = db.Groups.Where(x => x.GroupId == x.GroupId && x.GroupId != "0").ToList();
            }
            else
            {
                return View("Unauthorized");
            }
            return View(db.Roles.ToList());
        }
        public ActionResult GrandRoleByGroup(string id)
        {
            var data = db.GroupRoles.Where(x => x.GroupId == id).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Grand Role
        // GET: Admin/Groups/gán và huỷ quyền người dùng
        [HttpPost]
        public ActionResult GrandRole(GroupRole gr)
        {
            string mes = "";

            //Kiểm tra quyền đã có hay chưa.
            var data = db.GroupRoles.Any(x => x.GroupId == gr.GroupId && x.BusinessId == gr.BusinessId && x.RoleId == gr.RoleId);
            //Lấy ra quyền cần huỷ
            if (data)
            {
                //huỷ quyền
                var grouprole = db.GroupRoles.FirstOrDefault(x => x.GroupId == gr.GroupId && x.BusinessId == gr.BusinessId && x.RoleId == gr.RoleId);
                db.GroupRoles.Remove(grouprole);
                db.SaveChanges();
                mes = "Huỷ quyền thành công";
            }
            else
            {
                //gán quyền
                db.GroupRoles.Add(gr);
                db.SaveChanges();
                mes = "Gán quyền thành công";
            }
            return Json(new
            {
                StatusCode = 200,
                Message = mes,
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region List Role Users, detail
        // GET - JSON: Admin/Groups/Listroles/List Roles for Users
        public ActionResult ListRole()
        {
            return View();
        }
        public JsonResult Getdatarole()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var data = db.Groups.Where(x => x.isAdmin == false && x.GroupId == x.GroupId && x.GroupId != "0" && x.Status == x.Status && x.Status != 10);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/Roles/Get id group
        public JsonResult GetId(string id)
        {
            var data = db.Groups.Where(x => (x.Status == 1 || x.Status == 0) && x.GroupId == id).SingleOrDefault();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region New Role for User
        // POST: Admin/Groups/Create New Role for Users(for ViewListRole)
        public ActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRole(Group g)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var checkNameRole = db.Groups.Any(x => x.GroupName == g.GroupName);
                    if (!checkNameRole)
                    {
                        db.Groups.Add(g);
                        db.SaveChanges();
                        return RedirectToAction("ListRole", "Groups");
                    }
                    else
                    {
                        ModelState.AddModelError("GroupName", "Tên Role không được trùng !");
                        return View(g);
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Có lỗi khi thêm mới Roles, vui lòng thử lại sau ! !");
                    return View(g);
                }
            }
            return View(g);
        }
        #endregion

        #region Edit Role
        //Edit/Roles/Json
        [HttpPost]
        public JsonResult EditRole(Group group)
        {
            var result = db.Groups.Where(x => (x.Status == 1 || x.Status == 0) && x.GroupId == group.GroupId).SingleOrDefault();
            if (result != null)
            {
                result.GroupName = group.GroupName;
                result.Background = group.Background;
                result.Status = group.Status;
                db.SaveChanges();
                return Json(new { success = "Chỉnh sửa thành công !" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng!" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}