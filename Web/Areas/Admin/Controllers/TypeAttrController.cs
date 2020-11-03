using Models;
using Models.Models.DataModels;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Attribute = Models.Models.DataModels.Attribute;

namespace Web.Areas.Admin.Controllers
{
    public class TypeAttrController : Controller
    {
        MobileShopContext db = new MobileShopContext();
        #region TypeAttrs, detail
        // GET: Admin/TypeAttr
        public ActionResult Index()
        {
            return View();
        }
        // Json: Admin/Getdata TypeAttr
        public ActionResult Getdata()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.TypeAttrs.Where(x => x.Status == x.Status && x.Status != 10 && x.OrderBy >= 1);
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        // JSON: Admin/Edit TypeAttr JSON
        public ActionResult GetId(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.TypeAttrs.Find(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create
        // Json: Admin/Create TypeAttr
        [HttpPost]
        public JsonResult Create(TypeAttr t)
        {
            if (ModelState.IsValid)
            {
                var countOrderby = db.TypeAttrs.OrderBy(x => x.OrderBy).Where(x => x.Status != 10).Count();
                t.OrderBy = countOrderby + 1;
                db.TypeAttrs.Add(t);
                db.SaveChanges();
                return Json(new { success = "Thêm mới thành công !" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Có lỗi khi thêm mới !" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update
        public ActionResult Edit(TypeAttr t)
        {
            var typeattr = db.TypeAttrs.Where(x => x.TypeId == t.TypeId).SingleOrDefault();
            if (typeattr != null)
            {
                var coutOrderby = db.TypeAttrs.OrderBy(x => x.OrderBy).Where(y => y.TypeId != t.TypeId && y.Status != 10 && y.OrderBy >= t.OrderBy).ToList();
                typeattr.TypeName = t.TypeName;

                if (t.OrderBy > 0)
                {
                    if (typeattr.OrderBy != t.OrderBy)
                    {
                        var orderby = t.OrderBy;
                        foreach (var item in coutOrderby)
                        {
                            item.OrderBy = ++orderby;
                        }
                    }
                    typeattr.OrderBy = t.OrderBy;
                    typeattr.Status = t.Status;
                    db.SaveChanges();
                    return Json(new { success = "Chỉnh sửa thành công !" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = "Số thứ tự không được nhỏ hơn 1 !" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { error = "Có lỗi khi chỉnh sửa !" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete
        // JSON: Admin/Delete TypeAttr JSON
        [HttpPost]
        public JsonResult Delete(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.TypeAttrs.Where(x => x.TypeId == id).SingleOrDefault();
            if (result != null)
            {
                var listOrderby = db.TypeAttrs.OrderBy(x => x.OrderBy).Where(x => x.TypeId != id && x.Status != 10);
                var count = 0;
                foreach (var item in listOrderby)
                {
                    item.OrderBy = ++count;
                }
                result.OrderBy = -1;
                result.Status = 10; //delete with change status = 10
                db.SaveChanges();
                return Json(new { success = "Xoá thành công !!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "có lỗi khi xoá !!" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Atributes, detail
        // GET: Admin/Attribute
        public ActionResult Attribute()
        {
            ViewBag.TypeId = new SelectList(db.TypeAttrs.Where(x => x.Status == 1), "TypeId", "TypeName");
            return View();
        }
        public JsonResult GetdataAttr()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = (from t in db.TypeAttrs
                          join a in db.Attributes on t.TypeId
                          equals a.TypeId
                          where a.Status == 1 || a.Status == 0
                          select new AttributeJoinTypeAttr()
                          {
                              AttrId = a.AttrId,
                              TypeName = t.TypeName,
                              AttrName = a.AttrName,
                              Value = a.Value,
                              Status = a.Status
                          }).AsEnumerable();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        //JSON: Admin/Attribute/EditAttr use JSON
        [HttpPost]
        public JsonResult GetidAttr(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var attrid = db.Attributes.Find(id);
            return Json(attrid, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create Atribute
        // JSON: Admin/Attribute/CreateAttr use JSON
        [HttpPost]
        public JsonResult CreateAttr(Attribute a)
        {
            if (ModelState.IsValid)
            {
                db.Attributes.Add(a);
                db.SaveChanges();
                return Json(new { success = "Thêm mới thành công !!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có lỗi khi thêm mới !!" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Update Atribute
        [HttpPost]
        public JsonResult EditAttr(Attribute a)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid)
            {
                var attr = db.Attributes.Where(x => x.AttrId == a.AttrId).FirstOrDefault();
                if (attr != null)
                {
                    attr.AttrName = a.AttrName;
                    attr.TypeId = a.TypeId;
                    attr.Value = a.Value;
                    attr.Status = a.Status;
                    db.SaveChanges();
                    return Json(new {success = "Chỉnh sửa thành công !!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = "Không thể chỉnh sửa !!" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { error = "Không thể chỉnh sửa !!" }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion

        #region Delete Atribute
        //JSON: Admin/Attribute/DeleteAttr use JSON
        public JsonResult DeleteAttr(int id)
        {
            var attrid = db.Attributes.Where(x => x.AttrId == id).FirstOrDefault();
            if (attrid != null)
            {
                attrid.Status = 10; //delete with change status = 10;
                db.SaveChanges();
                return Json(new { success = "Xoá thành công !!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng!!" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}