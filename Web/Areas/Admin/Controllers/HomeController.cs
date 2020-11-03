using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Helpers;
using Models;
using Models.ViewModels;
using Models.Models.DataModels;
using Web.Controllers;
using Web.Areas.Admin.Models;
using System.Web.Configuration;
using System.Web.SessionState;
using WebGrease.Css.Extensions;

namespace Web.Areas.Admin.Controllers
{
    [CustomAuth]
    public class HomeController : BaseController
    {
        MobileShopContext db = new MobileShopContext();
        // GET: Admin/Home
        #region Home
        public ActionResult Index()
        {
            ViewBag.invoice = db.Orders.Where(x => x.Status == 2).Count();
            ViewBag.product = db.Products.Where(x => x.Status == true).Count();
            ViewBag.users = db.Users.Where(x => x.Status == 1 || x.Status == 0).Count();
            ViewBag.customers = db.Customers.Where(x => x.Status == 1 || x.Status == 0).Count();
            return View();
        }
        #endregion

        #region Chart data
        public JsonResult ChartData()
        {
            var orders = db.Orders.Where(x => x.Status == 2).GroupBy(x => new
            {
                Month = x.Created.Month,
                Year = x.Created.Year
            }).Select(c => new ChartData
            {
                Month = c.Key.Month,
                Year = c.Key.Year,
                MonthOfYear = c.Key.Month + "/" + c.Key.Year,
                Total = c.Count()
            }).OrderBy(x => x.Year).AsEnumerable();
            return Json(orders, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Feedback, detail, handle feedback
        public ActionResult Feedback()
        {
            return View();
        }
        public JsonResult GetAllFeedback()
        {
            var feedback = db.Feedbacks.ToList();
            return Json(new { data = feedback }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FeedbackDetail(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Feedback");
            }
            Feedback feedback = db.Feedbacks.Find(id);
            if (feedback == null)
            {
                return RedirectToAction("Feedback");
            }
            return View(feedback);
        }

        [HttpPost]
        public JsonResult HandleFeedback(Feedback f)
        {

            var feedback = db.Feedbacks.Where(x => x.FeedBackId == f.FeedBackId).FirstOrDefault();
            if (feedback == null)
            {
                return Json(new { error = "Not Found !!" }, JsonRequestBehavior.AllowGet);
            }
            try
            {

                if (f.Status > 1 || f.Status < -1)
                {
                    return Json(new { error = "Có lỗi, vui lòng thử lại" }, JsonRequestBehavior.AllowGet);
                }
                feedback.Status = f.Status;//handle feedback with state = 1
                db.SaveChanges();
                return Json(new { success = "Đã xử lý !!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { error = "Có lỗi, vui lòng thử lại" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region AutoSend
        [HttpPost]
        public JsonResult TestAutoSend()
        {
            if (HttpContext.Session["User"] != null)
            {
                if (HttpRuntime.Cache["LoggedInUsers"] != null)//check if the list has been created
                {
                    //the list is not null so we retrieve it from the cache
                    List<User> loggedInUsers = (List<User>)HttpRuntime.Cache["LoggedInUsers"];
                    var curentUser = (User)HttpContext.Session["User"];
                    foreach (var user in loggedInUsers)
                    {
                        if (user.Email.Contains(curentUser.Email))//if the user is in the list
                        {
                            //then remove them
                            loggedInUsers.Remove(user);
                            Session.Remove("User");
                            break;
                        }
                        // else do nothing
                    }
                    return Json(new { success = "thành công", JsonRequestBehavior.AllowGet });
                }
            }
            return Json(new { error = "Không thành công", JsonRequestBehavior.AllowGet });
        }
        #endregion

        #region partialView
        public PartialViewResult MainLeft()
        {
            var getuser = HttpContext.Session["User"] as User;
            ViewBag.isAdmin = getuser.IsAdmin;
            return PartialView("_MainLeft");
        }
        public PartialViewResult InfoUserTopRight()
        {
            return PartialView("_InfoUserTopRight");
        }
        public PartialViewResult Notifications()
        {

            var notiFeedback = db.Feedbacks.Where(x => x.Status == 0).Take(4).ToList();
            ViewBag.coutPenddingFeedback = db.Feedbacks.Where(x => x.Status == 0).Count();
            ViewBag.Orders = db.Orders.Where(x => x.Status == 0).Take(4).OrderByDescending(x => x.Created).ToList();
            ViewBag.CountOrders = db.Orders.Where(x => x.Status == 0).Count();
            return PartialView("_Notifications", notiFeedback);
        }
        #endregion
    }
}