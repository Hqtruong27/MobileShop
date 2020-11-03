using Models;
using Models.Models.DataModels;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.Areas.Admin.Controllers
{
    public class NewsController : BaseController
    {
        MobileShopContext db = new MobileShopContext();
        // GET: Admin/News
        #region Newss
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAllNews()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var news = (from n in db.News.Where(n => n.Status == 1 || n.Status == 0)
                        join u in db.Users.Where(u => u.Status == 1 || u.Status == 0)
                        on n.UserId equals u.UserId
                        select new NewsJoinAdmin()
                        {
                            NewsId = n.NewsId,
                            FullName = u.FullName,
                            NewsTitle = n.NewsTitle,
                            FeatureImage = n.FeatureImage,
                            ShortDescription = n.ShortDescription,
                            Description = n.Description,
                            CountView = n.CountView,
                            Created = n.Created,
                            Status = n.Status

                        }).AsEnumerable();

            return Json(new { data = news }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create News
        //GET: Admin/Create News
        public ActionResult Create()
        {
            return View();
        }

        //POST: Admin/Create News
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(News news)
        {
            var user = (User)HttpContext.Session["User"];
            if (user == null)
            {
                return View("Unauthorized");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    news.CountView = 0;
                    news.Created = DateTime.Now;
                    news.UserId = user.UserId;
                    db.News.Add(news);
                    db.SaveChanges();
                    setAlert("Success !", "Thêm mới thành công !!", "top-right", "success", 4000);
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    setAlert("Error !", "Có gì đó không đúng, vui lòng thử lại sau !!", "top-right", "error", 4000);
                    return View(news);
                }
            }
            return View(news);
        }
        #endregion

        #region Update News
        //GET: Admin/Edit News
        public ActionResult Edit(int? id)
        {
            var user = (User)HttpContext.Session["User"];
            if (user == null || id == null)
            {
                return View("Unauthorized");
            }
            News news = db.News.Find(id);
            if (news == null)
            {
                return View("Unauthorized");
            }
            return View(news);
        }
        //POST: Admin/Edit News
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News news)
        {
            var user = (User)HttpContext.Session["User"];
            if (news == null)
            {
                return View("Unauthorized");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var result = db.News.Where(n => n.NewsId == news.NewsId).FirstOrDefault();
                    if (result != null)
                    {
                        result.NewsTitle = news.NewsTitle;
                        result.FeatureImage = news.FeatureImage;
                        result.ShortDescription = news.ShortDescription;
                        result.Description = news.Description;
                        result.Status = news.Status;
                        db.SaveChanges();
                        setAlert("Success !", "Chỉnh sửa thành công !!", "top-right", "success", 4000);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        setAlert("Error !", "Không tìm thấy bài viết !!", "top-right", "error", 4000);
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception)
                {
                    setAlert("Error !", "Có gì đó không đúng, vui lòng thử lại sau !!", "top-right", "error", 4000);
                    return View(news);
                }
            }
            return View(news);
        }
        #endregion

        #region Delete News
        //JSON: Admin/Delete News
        [HttpPost]
        public JsonResult Delete(int? id)
        {
            var user = (User)HttpContext.Session["User"];
            if (user == null)
            {
                return Json(new { nulluser = "" }, JsonRequestBehavior.AllowGet);
            }
            if (id == null)
            {
                return Json(new { error = "Không tìm thấy bài viết" }, JsonRequestBehavior.AllowGet);
            }
            News news = db.News.Find(id);
            if (news != null)
            {
                news.Status = 10;//delete with status = 10
                db.SaveChanges();
                return Json(new { success = "Xoá thành công" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Không tìm thấy bài viết" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}