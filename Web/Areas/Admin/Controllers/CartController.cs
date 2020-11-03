using Models;
using Models.Models.DataModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Areas.Admin.Controllers
{
    public class CartController : Controller
    {
        MobileShopContext db = new MobileShopContext();
        // GET: Admin/Get All Cart
        #region List Orders, detail
        public ActionResult Index()
        {
            return View();
        }
        
        public JsonResult Getdata(string isvk)
        {
            var orders = from o in db.Orders
                         select o;
            switch (isvk)
            {
                case "canceled":
                    orders = db.Orders.Where(o => o.Status == -1 || o.Status == -2);
                    break;
                case "pending":
                    orders = db.Orders.Where(o => o.Status == 0);
                    break;
                case "approved":
                    orders = db.Orders.Where(o => o.Status == 1);
                    break;
                case "delivered":
                    orders = db.Orders.Where(o => o.Status == 2);
                    break;
                default:
                    orders = db.Orders;
                    break;
            }
            return Json(orders.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return View("Unauthorized");
            }
            var order = db.Orders.Include(x => x.OrderDetails).FirstOrDefault(x => x.OrderId == id);
            if (order == null)
            {
                return View("Unauthorized");
            }
            return View(order);
        }
        [HttpPost]
        public JsonResult GetId(int id)
        {
            var order = db.Orders.Where(x => x.OrderId == id).FirstOrDefault();
            return Json(order, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Change Status Order
        [HttpPost]
        public JsonResult ChangeStatusOrder(Order order)
        {
            var dbOrder = db.Orders.Where(x => x.OrderId == order.OrderId).FirstOrDefault();
            if (dbOrder != null)
            {
                dbOrder.Status = order.Status;
                db.SaveChanges();
                return Json(new { success = "Cập nhập trạng thái thành công !" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng !" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}