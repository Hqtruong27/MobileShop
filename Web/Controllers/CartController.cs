using Models;
using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Areas.Admin.Models;

namespace Web.Controllers
{
    [CustomersAutherize]
    public class CartController : Controller
    {
        MobileShopContext db = new MobileShopContext();

        #region Carts
        // GET: Cart
        public ActionResult Index()
        {
            //Lấy thông tin người dùng hiện tại 
            HttpCookie cookie = Request.Cookies["InfoCustomer"];
            var userId = cookie["id"];
            var parseIntUser = int.Parse(userId);
            var cart = db.AddToCarts.Where(x => x.CustomerId == parseIntUser).FirstOrDefault();
            if (cart == null)
            {
                ViewBag.cartnull = "Chưa có sản phẩm trong giỏ hàng";
            }
            return View();
        }
        public PartialViewResult GetallCart()
        {
            //Lấy thông tin người dùng hiện tại 
            HttpCookie cookie = Request.Cookies["InfoCustomer"];
            var userId = cookie["id"];
            var parseIntUser = int.Parse(userId);
            var cart = db.AddToCarts.Where(x => x.CustomerId == parseIntUser).FirstOrDefault();
            if (cart == null)
            {
                ViewBag.cartnull = "Chưa có sản phẩm trong giỏ hàng";
            }
            var list = db.AddToCarts.Where(x => x.CustomerId == parseIntUser).ToList();
            return PartialView("_GetallCart", list);
        }
        #endregion

        #region Update Quantity
        //POST: Delete cart
        [HttpPost]
        public ActionResult UpdateQuantity(int? id, int quantity)
        {
            if (id == null)
            {
                return Json(new { error = "Sản phẩm không tồn tại !!" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var result = db.AddToCarts.Where(x => x.CartId == id).FirstOrDefault();
                if (result != null)
                {
                    if (quantity >= 0)
                    {
                        if (quantity == 0)
                        {
                            db.AddToCarts.Remove(result);
                            db.SaveChanges();
                            return Json(new { success = "Đã xoá sản phẩm" }, JsonRequestBehavior.AllowGet);
                        }
                        result.Quantity = quantity;
                        db.SaveChanges();
                        return Json(new { success = "Cập nhập thành công" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { error = "Vui lòng chọn số lượng !!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { error = "Có gì đó không đúng !!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Có gì đó không đúng !!" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete product Carts
        //POST: Delete cart
        [HttpPost]
        public JsonResult Delete(int id)
        {
            AddToCart addToCart = db.AddToCarts.Where(x => x.CartId == id).FirstOrDefault();
            if (addToCart != null)
            {
                db.AddToCarts.Remove(addToCart);
                db.SaveChanges();
                return Json(new { success = "Đã xoá sản phẩm !!" });
            }
            else
            {
                return Json(new { error = "Có gì đó không đúng !!" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Payment
        //POST: Payment
        public ActionResult Payment()
        {
            //get id current User
            HttpCookie cookie = Request.Cookies["InfoCustomer"];
            var id = cookie["id"];
            var userid = int.Parse(id);
            //get info current User
            var currentUser = db.Customers.Where(x => x.CustomerId == userid).SingleOrDefault();
            if (currentUser == null)
            {
                return HttpNotFound();
            }
            //check cart is empty
            var cart = db.AddToCarts.Where(x => x.CustomerId == userid).FirstOrDefault();
            if (cart == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.listCart = db.AddToCarts.Where(x => x.CustomerId == userid).ToList();
            return View(currentUser);
        }

        [HttpPost]
        public ActionResult Payment(Order order)
        {
            order.Created = DateTime.Now;
            order.TimeExpires = DateTime.Now.AddMinutes(10);
            order.Status = 0; // pending
            Random random = new Random(DateTime.Now.Ticks.GetHashCode());
            var getYear = DateTime.Now.Year.ToString();
            getYear = getYear.Substring(2);
            var randomCode = random.Next(100000, 999999).ToString() + getYear;
            var codeOrder = db.Orders.Any(x => x.CodeOrder.Equals(randomCode));
            while (codeOrder)
            {
                randomCode = random.Next(100000, 999999).ToString() + getYear;
            }
            order.CodeOrder = randomCode;
            //get all Address in one columns
            order.Address = order.City + " - " + order.District + " - " + order.Commune + " - " + order.HouseNumber;
            //get oder current custommer
            var orderCurrentUSer = db.AddToCarts.Where(x => x.CustomerId == order.CustomerId).ToList();
            //create List save Order detail
            List<OrderDetail> list = new List<OrderDetail>();
            double price = 0;
            foreach (var item in orderCurrentUSer)
            {
                var priceQty = item.Price * item.Quantity;
                price = price + priceQty;
                //increase buy more 1
                var SaleQuantity = db.Products.Where(x => x.ProductId == item.product.ProductId);
                foreach (var saleQty in SaleQuantity)
                {
                    saleQty.ProductSaleQuantity++;
                }
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.CategoryName = item.product.Categories.CategoryName;
                orderDetail.ProviderName = item.product.Providers.ProviderName;
                orderDetail.AttrName = item.AttrName;
                orderDetail.ProductId = item.product.ProductId;
                orderDetail.ProductName = item.product.ProductName;
                orderDetail.Discount = item.product.Discount;
                orderDetail.FeatureImage = item.product.FeatureImage;
                orderDetail.ProductId = item.product.ProductId;
                orderDetail.Quantity = item.Quantity;
                orderDetail.PriceOut = item.product.PriceOut;
                orderDetail.Price = item.Price;
                list.Add(orderDetail);
            }
            order.totalPrice = price;
            //Save orderdetail as database
            order.OrderDetails = list;
            //Save order as database
            try
            {
                db.Orders.Add(order);
                db.SaveChanges();
                //remove all cart after order succes
                db.AddToCarts.RemoveRange(orderCurrentUSer);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ViewBag.error = "Đã xảy ra lỗi khi đặt hàng, vui lòng cập nhập lại giỏ hàng và thử lại";
                return RedirectToAction("Payment");
            }
        }
        #endregion
    }
}