using Models;
using Models.Models.DataModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
            var orders = from o in db.Orders select o;
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
            return Json(orders, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return View("Unauthorized");
            }
            var order = await db.Orders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.OrderId == id);
            if (order == null)
            {
                return View("Unauthorized");
            }
            return View(order);
        }
        [HttpPost]
        public async Task<JsonResult> GetId(int id)
        {
            var order = await db.Orders.Where(x => x.OrderId == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return Json(new { error = "Not found Order" }, JsonRequestBehavior.AllowGet);
            }
            return Json(order, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Change Status Order
        [HttpPost]
        public async Task<JsonResult> ChangeStatusOrder(Order order)
        {
            var dbOrder = await db.Orders.Where(x => x.OrderId == order.OrderId).FirstOrDefaultAsync();
            if (dbOrder != null)
            {
                dbOrder.Status = order.Status;
                await db.SaveChangesAsync();
                return Json(new { success = "Cập nhập trạng thái thành công !" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Không tìm thấy Order !" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}