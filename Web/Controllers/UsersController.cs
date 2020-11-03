using Models;
using Models.Models.DataModels;
using Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Web.Areas.Admin.Models;
using Facebook;
using System.Configuration;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class UsersController : BaseController
    {
        MobileShopContext db = new MobileShopContext();

        #region Info Customer
        // GET: Users/ information current user
        [CustomersAutherize]
        public ActionResult Index()
        {
            int? id = int.Parse(Request.Cookies["InfoCustomer"]["Id"]);
            if (id == null)
            {
                return HttpNotFound();
            }
            return View(db.Customers.Where(u => u.CustomerId == id).SingleOrDefault());
        }
        [HttpPost]
        [CustomersAutherize]
        public async Task<ActionResult> Index(Customer c, HttpPostedFileBase imageUpload)
        {
            HttpCookie cookie = Request.Cookies["InfoCustomer"];
            var id = cookie["id"];
            if (id == null)
            {
                return HttpNotFound("Cannot find user");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var current = await db.Customers.FindAsync(int.Parse(id));
                    current.FullName = c.FullName;
                    current.DateofBirth = c.DateofBirth;
                    current.Gender = c.Gender;
                    if (imageUpload != null)
                    {
                        string filename = Path.GetFileNameWithoutExtension(imageUpload.FileName);
                        string extention = Path.GetExtension(imageUpload.FileName);
                        filename = filename + extention;
                        string extentionLower = extention.ToLower();
                        var extLower = extentionLower.Substring(extentionLower.LastIndexOf('.') + 1);
                        if (extLower == "png" || extLower == "jpg" || extLower == "jpeg")
                        {
                            string fullpath = Path.Combine(Server.MapPath("~/Images/"), filename);
                            current.Avatar = filename;
                            imageUpload.SaveAs(fullpath);
                            await db.SaveChangesAsync();
                            cookie["Name"] = current.FullName;
                            cookie["Avatar"] = current.Avatar;
                            Response.Cookies.Add(cookie);
                            cookie.Expires = DateTime.Now.AddDays(2);
                            ModelState.Clear();
                            setAlert("Success !", "Cập nhập thông tin thành công.", "bottom-left", "success", 5000);
                            return RedirectToAction("Index", "Users");
                        }
                        else
                        {
                            setAlert("Lỗi !", "Hãy chọn đúng định dạng đuôi .png, jpg, .jpeg", "bottom-left", "error", 5000);
                            return RedirectToAction("Index", "Users");
                        }
                    }
                    else
                    {
                        await db.SaveChangesAsync();
                        cookie["Name"] = current.FullName;
                        cookie["Avatar"] = current.Avatar;
                        Response.Cookies.Add(cookie);
                        cookie.Expires = DateTime.Now.AddDays(2);
                        ModelState.Clear();
                        setAlert("Success !", "Cập nhập thông tin thành công.", "bottom-left", "success", 5000);
                        return RedirectToAction("Index", "Users");
                    }
                }
                return View(c);
            }
            catch (Exception)
            {
                setAlert("Lỗi !", "Không thể chỉnh sửa thông tin", "bottom-left", "error", 5000);
                return RedirectToAction("Index", "Users");
            }

        }
        #endregion

        #region Order, detail
        // GET: Users/ ListOrders
        [CustomersAutherize]
        public async Task<ActionResult> Orders()
        {
            if (Request.Cookies["InfoCustomer"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var id = int.Parse(Request.Cookies["InfoCustomer"]["Id"]);
            return View(await db.Orders.Where(o => o.CustomerId == id).Include(x => x.OrderDetails).ToListAsync());
        }
        [CustomersAutherize]
        public async Task<ActionResult> OrderDetail(string id)
        {
            if (Request.Cookies["InfoCustomer"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return RedirectToAction("Orders");
            }
            var userid = int.Parse(Request.Cookies["InfoCustomer"]["Id"]);
            Order orders = await db.Orders.Where(o => o.CodeOrder == id).Include(o => o.OrderDetails).SingleOrDefaultAsync();
            if (orders == null)
            {
                return RedirectToAction("Orders");
            }
            return View(orders);
        }
        //JSON: Users/ canceled order
        [HttpPost]
        public async Task<JsonResult> CaneledOrder(int id)
        {
            var result = await db.Orders.Where(o => o.OrderId == id).SingleOrDefaultAsync();
            if (result == null) return Json(new { error = "Không tìm thấy đơn hàng này" }, JsonRequestBehavior.AllowGet);
            if ((result.Status != 0 || result.Status != 1) && result.TimeExpires <= DateTime.Now)
                return Json(new { error = "Bạn không thể huỷ đơn hàng" }, JsonRequestBehavior.AllowGet);

            result.Status = -2;//CaneledOrder with status = -2;
            await db.SaveChangesAsync();
            return Json(new { success = "Huỷ đơn hàng thành công !" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ReturnUrl
        // POST: Users/ Login - Register - Logout - returnUrl
        private ActionResult RedireactToLocal(string next)
        {
            if (Url.IsLocalUrl(next))
            {
                return Redirect(next);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Login Customer
        [AllowAnonymous]
        public ActionResult Login(string next)
        {
            ViewBag.ReturnUrl = next;
            if (Request.Cookies["InfoCustomer"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginCustomer c, string next)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await db.Customers.SingleOrDefaultAsync(x => x.Email == c.Email);
                    if (user != null)
                    {
                        var checkpwd = BCrypt.Net.BCrypt.Verify(c.Password, user.Password);
                        if (checkpwd)
                        {
                            HttpCookie cookie = new HttpCookie("InfoCustomer");
                            cookie["id"] = user.CustomerId.ToString();
                            cookie["Email"] = user.Email;
                            cookie["Avatar"] = user.Avatar;
                            cookie["CreateDate"] = user.CreateDate.ToString("dd/MM/yyyy HH:mm");
                            cookie.Expires = DateTime.Now.AddDays(2);
                            Response.Cookies.Add(cookie);
                            if (c.RememberMe == true)
                            {
                                Response.Cookies["Email"].Value = c.Email;
                                Response.Cookies["Email"].Expires = DateTime.Now.AddDays(15);
                                ViewBag.Email = Request.Cookies["Email"].Value;
                            }
                            setAlert("", "Đăng nhập thành công !", "top-right", "success", 5000);
                            return RedireactToLocal(next);
                        }
                        else
                        {
                            ModelState.AddModelError("Email", "Email hoặc mật khẩu không chính xác");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Email hoặc mật khẩu không chính xác");
                    }
                }
                catch (Exception)
                {
                    setAlert("Lỗi!", "Không thể đăng nhập vào lúc này, vui lòng thử lại sau.", "bottom-left", "error", 5000);
                    return View(c);
                }
            }
            return View(c);
        }
        #endregion

        #region Login Facebook
        private Uri RediredtUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }
        public ActionResult LoginFacebook()
        {
            var facebook = new FacebookClient();
            var loginUrl = facebook.GetLoginUrl(new
            {
                client_id = "306811353878359",
                client_secret = "2162640f817925fed0799a5047d89a70",
                redirect_uri = RediredtUri.AbsoluteUri,
                response_type = "code",
                scope = "email"
            });
            return Redirect(loginUrl.AbsoluteUri);
        }
        public async Task<ActionResult> FacebookCallback(string code)
        {
            var facebook = new FacebookClient();
            dynamic result = facebook.Post("oauth/access_token", new
            {
                client_id = "306811353878359",
                client_secret = "2162640f817925fed0799a5047d89a70",
                redirect_uri = RediredtUri.AbsoluteUri,
                code = code
            });
            var accessToken = result.access_token;
            if (!String.IsNullOrEmpty(accessToken))
            {
                facebook.AccessToken = accessToken;
                dynamic me = facebook.Get("me?fields=link,first_name,currency,last_name,email,gender,locale,timezone,verified,picture,age_range");
                string email = me.email;
                string userName = me.email;
                string firstname = me.first_name;
                string middleName = me.middle_name;
                string lastName = me.last_name;
                var resultcustomer = await db.Customers.FirstOrDefaultAsync(x => x.Email == email);
                if (resultcustomer == null)
                {
                    Customer customer = new Customer();
                    customer.Email = email;
                    customer.FullName = firstname + " " + middleName + " " + lastName;
                    customer.Avatar = me.picture.data.url;
                    customer.Status = 0;
                    customer.ActiveCode = Guid.NewGuid();
                    customer.DateofBirth = new DateTime(1960, 01, 01);
                    customer.CreateDate = DateTime.Now;
                    db.Customers.Add(customer);
                    await db.SaveChangesAsync();
                    HttpCookie cookie = new HttpCookie("InfoCustomer");
                    cookie["id"] = customer.CustomerId.ToString();
                    cookie["Email"] = customer.Email;
                    cookie["Avatar"] = customer.Avatar;
                    cookie["CreateDate"] = customer.CreateDate.ToString("dd/MM/yyyy HH:mm");
                    cookie.Expires = DateTime.Now.AddDays(2);
                    Response.Cookies.Add(cookie);
                }
                else
                {
                    HttpCookie cookie = new HttpCookie("InfoCustomer");
                    cookie["id"] = resultcustomer.CustomerId.ToString();
                    cookie["Email"] = resultcustomer.Email;
                    cookie["Avatar"] = resultcustomer.Avatar;
                    cookie["CreateDate"] = resultcustomer.CreateDate.ToString("dd/MM/yyyy HH:mm");
                    cookie.Expires = DateTime.Now.AddDays(2);
                    Response.Cookies.Add(cookie);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Register Customer

        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Request.Cookies["InfoCustomer"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(CustomerViewModel c)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Customer cus = new Customer();
                    if (await db.Customers.AnyAsync(x => x.Email == c.Email))
                    {
                        setAlert("Lỗi!", "Email đã được sử dụng.", "bottom-left", "error", 12000);
                        return View(c);
                    }
                    cus.FullName = c.FullName;
                    cus.Email = c.Email;
                    var pwd = BCrypt.Net.BCrypt.HashPassword(c.Password);
                    cus.Password = pwd;
                    cus.CreateDate = DateTime.Now;
                    cus.Status = 1;
                    db.Customers.Add(cus);
                    await db.SaveChangesAsync();
                    if (Request.Cookies["Email"] != null)
                    {
                        Response.Cookies["Email"].Expires = DateTime.Now.AddDays(-1);
                    }
                    setAlert("Success", "Đăng ký tài khoản thành công !", "bottom-left", "success", 5000);
                    return RedirectToAction("Login");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Có lỗi khi đăng ký, vui lòng thử lại sau !");
                    return View(c);
                }
            }
            return View();
        }
        #endregion

        #region Logout
        [CustomersAutherize]
        public ActionResult Logout(string next)
        {
            Response.Cookies["InfoCustomer"].Expires = DateTime.Now.AddDays(-2);
            Response.Cookies["Avatar"].Expires = DateTime.Now.AddDays(-2);
            return RedireactToLocal(next);
        }
        #endregion

        #region Change Password
        [CustomersAutherize]
        public ActionResult ChangePassword()
        {
            int? id = int.Parse(Request.Cookies["InfoCustomer"]["Id"]);
            if (id == null)
            {
                return HttpNotFound("không tìm thấy user");
            }
            return View();
        }
        [HttpPost]
        [CustomersAutherize]
        public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            int? id = int.Parse(Request.Cookies["InfoCustomer"]["Id"]);
            if (id == null)
            {
                return HttpNotFound("không tìm thấy user");
            }
            if (ModelState.IsValid)
            {
                Customer customer = await db.Customers.Where(c => c.CustomerId == id).SingleOrDefaultAsync();
                if (customer != null)
                {
                    var checkpwd = BCrypt.Net.BCrypt.Verify(model.Password, customer.Password);
                    if (checkpwd)
                    {
                        if (model.Password == model.NewPassword)
                        {
                            ModelState.AddModelError("error", "Mật khẩu mới phải khác mật khẩu cũ.");
                            return View(model);
                        }
                        customer.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        await db.SaveChangesAsync();
                        setAlert("Success", "Thay dổi mật khẩu thành công, vui lòng đăng nhập lại !", "bottom-left", "success", 5000);
                        Response.Cookies["InfoCustomer"].Expires = DateTime.Now.AddDays(-2);
                        Response.Cookies["Avatar"].Expires = DateTime.Now.AddDays(-2);
                        return RedirectToAction("Login", "Users");
                    }
                    else
                    {
                        ModelState.AddModelError("error", "Mật khẩu cũ không đúng");
                    }
                }
                else
                {
                    ModelState.AddModelError("null", "Không thể thay đổi mật khẩu vào lúc này, vui lòng thử lại sau !");
                }
            }
            return View(model);
        }
        #endregion

        #region Send Email
        [NonAction]
        public void SendVerifiedLinkEmail(string email, string activeCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/Users/" + emailFor + "/" + activeCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("c1709h@gmail.com", "ResetPassWord no-reply");
            var toEmail = new MailAddress(email);
            string subject = "";
            string body = "";
            if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "<br/><br/>Đã có một yêu cầu đặt đặt lại mật khẩu web Mobile Shop" +
            "nếu đó là bạn, vui lòng click vào link bên dưới, link sẽ tồn tại trong 24h." +
            " <br/><br/><a href='" + link + "'>" + link + "</a>";
            }

            var smtp = new SmtpClient();
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
        #endregion

        #region Forgot Password
        // POST: Users/forgot pwd
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(string Email)
        {
            if (Request.Cookies["InfoCustomer"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            string success = "";
            string error = "";
            var account = await db.Customers.Where(x => x.Email == Email).SingleOrDefaultAsync();
            if (account != null)
            {
                //Send Email for reset pwd
                string resetCode = Guid.NewGuid().ToString();
                SendVerifiedLinkEmail(account.Email, resetCode, "ResetPassword");
                account.ResetPasswordCode = resetCode;
                account.ExpiredTime = DateTime.Now.AddDays(1);
                //
                db.Configuration.ValidateOnSaveEnabled = false;
                await db.SaveChangesAsync();
                success = "Một email đã được gửi đến bạn, vui lòng kiểm tra hộp thư trong Email của bạn";
            }
            else
            {
                error = "Email không hợp lệ hoặc được đăng ký";
            }
            ViewBag.error = error;
            ViewBag.success = success;
            return View();
        }
        #endregion

        #region Reset Password
        // POST: Users/Reset pwd - res_pwd confirm
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string id)
        {
            if (Request.Cookies["InfoCustomer"] != null) return RedirectToAction("Index", "Home");
            if (id == null) return HttpNotFound();

            //verify the reset password link
            var user = await db.Customers.Where(x => x.ResetPasswordCode == id).FirstOrDefaultAsync();
            if (user == null) return RedirectToAction("Index", "Home");
            if (user.ExpiredTime < DateTime.Now)
            {
                user.ResetPasswordCode = "";
                user.ExpiredTime = null;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            ResetPasswordModel model = new ResetPasswordModel();
            model.ResetCode = id;
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (Request.Cookies["InfoCustomer"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            //verify the reset password link
            var success = "";
            if (ModelState.IsValid)
            {
                var cus =  await db.Customers.Where(x => x.ResetPasswordCode == model.ResetCode).FirstOrDefaultAsync();
                if (cus != null)
                {
                    if (cus.ExpiredTime < DateTime.Now)
                    {
                        cus.ResetPasswordCode = "";
                        cus.ExpiredTime = null;
                        db.SaveChanges();
                        return RedirectToAction("Index", "Home");
                    }
                    cus.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    cus.ResetPasswordCode = "";
                    cus.ExpiredTime = null;
                    await db.SaveChangesAsync();
                    success = "Mật khẩu mới đã được thay đổi thành công, hãy thử ";
                    ViewBag.success = success;
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }
        #endregion

        #region Parial Action, Other
        [CustomersAutherize]
        public ActionResult Address()
        {
            return View();
        }


        // View Partial: Users/right header
        public PartialViewResult RightHeader()
        {
            if (Request.Cookies["InfoCustomer"] == null)
            {
                return PartialView("_RightHeader");
            }
            int? _id = int.Parse(Request.Cookies["InfoCustomer"]["Id"]);
            return PartialView("_RightHeader", db.Customers.Find(_id));
        }

        // View Partial: Users/right header
        public PartialViewResult MainleftUser()
        {
            if (Request.Cookies["InfoCustomer"] == null)
            {
                return PartialView("_MainleftUser");
            }
            int? _id = int.Parse(Request.Cookies["InfoCustomer"]["Id"]);
            if (_id == null)
            {
                return PartialView("_MainleftUser");
            }
            return PartialView("_MainleftUser", db.Customers.Find(_id));
        }
        #endregion
    }
}