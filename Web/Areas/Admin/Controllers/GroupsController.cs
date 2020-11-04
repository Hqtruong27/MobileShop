﻿using Models;
using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Web.Areas.Admin.Models;

namespace Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class GroupsController : Controller
    {
        MobileShopContext db = new MobileShopContext();
        #region Group Roles
        // GET: Admin/Groups
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            var user = HttpContext.Session["User"] as User;
            if (user.IsAdmin != true) return View("Unauthorized");

            ViewBag.business = await db.Businesses.Where(x => x.Status == x.Status && x.Status != 3).ToListAsync();
            ViewBag.groups = await db.Groups.Where(x => x.GroupId == x.GroupId && x.GroupId != "0").ToListAsync();
            return View(await db.Roles.ToListAsync());
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
        public async Task<ActionResult> GrandRole(GroupRole gr)
        {
            string mes = "";

            //Kiểm tra quyền đã có hay chưa.
            var data = await db.GroupRoles.AnyAsync(x => x.GroupId == gr.GroupId && x.BusinessId == gr.BusinessId && x.RoleId == gr.RoleId);
            //Lấy ra quyền cần huỷ
            if (data)
            {
                //huỷ quyền
                var grouprole = await db.GroupRoles.FirstOrDefaultAsync(x => x.GroupId == gr.GroupId && x.BusinessId == gr.BusinessId && x.RoleId == gr.RoleId);
                db.GroupRoles.Remove(grouprole);
                await db.SaveChangesAsync();
                mes = "Huỷ quyền thành công";
            }
            else
            {
                //gán quyền
                db.GroupRoles.Add(gr);
                await db.SaveChangesAsync();
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
        public async Task<ActionResult> CreateRole(Group g)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var checkNameRole = await db.Groups.AnyAsync(x => x.GroupName == g.GroupName);
                    if (!checkNameRole)
                    {
                        db.Groups.Add(g);
                        await db.SaveChangesAsync();
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
        public async Task<JsonResult> EditRole(Group group)
        {
            var result = await db.Groups.Where(x => (x.Status == 1 || x.Status == 0) && x.GroupId == group.GroupId).SingleOrDefaultAsync();
            if (result != null)
            {
                result.GroupName = group.GroupName;
                result.Background = group.Background;
                result.Status = group.Status;
                await db.SaveChangesAsync();
                return Json(new { success = "Chỉnh sửa thành công !" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Không thìm thấy Id role!" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}