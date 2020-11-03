using Models;
using Models.Models.DataModels;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Web.Areas.Admin.Models;
using Web.Controllers;
using WebMatrix.WebData;

namespace Web.Areas.Admin.Controllers
{
    public class UsersController : BaseController
    {
        MobileShopContext db = new MobileShopContext();

        #region Login User
        // GET: Admin/Users for Users
        public ActionResult Login()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var checkLogin = db.Users.SingleOrDefault(x => x.Email == user.Email);
                    if (checkLogin != null)
                    {
                        bool checkpwd = BCrypt.Net.BCrypt.Verify(user.Password, checkLogin.Password);
                        if (checkpwd == true && checkLogin.Status == 1 && checkLogin.mStatus != 10)
                        {
                            if (HttpRuntime.Cache["LoggedInUsers"] != null) //if the list exists, add this user to it
                            {
                                //get the list of logged in users from the cache
                                List<User> loggedInUsers = (List<User>)HttpRuntime.Cache["LoggedInUsers"];
                                //add this user to the list
                                foreach (var item in loggedInUsers)
                                {
                                    if (user.Email == item.Email)
                                    {
                                        loggedInUsers.Remove(item);
                                        break;
                                    }
                                }
                                loggedInUsers.Add(checkLogin);
                                //add the list back into the cache
                                HttpRuntime.Cache["LoggedInUsers"] = loggedInUsers;
                            }
                            else //the list does not exist so create it
                            {
                                //create a new list
                                List<User> loggedInUsers = new List<User>();
                                //add this user to the list
                                loggedInUsers.Add(checkLogin);
                                //add the list into the cache
                                HttpRuntime.Cache["LoggedInUsers"] = loggedInUsers;
                            }
                            //Lưu thông tin User
                            Session["User"] = checkLogin;
                            setAlert("Thông báo", "Xin chào: " + checkLogin.FullName + "", "top-right", "success", 4000);
                            return RedirectToAction("Index", "Home");
                        }
                        else if (checkpwd == true && checkLogin.Status == 0 && checkLogin.mStatus != 10)
                        {
                            ModelState.AddModelError("CheckLogin", "Tài khoản của bạn tạm thời đã bị khoá, liên hệ admin để được hỗ trợ");
                        }
                        else
                        {
                            ModelState.AddModelError("CheckLogin", "Email hoặc khẩu không chính xác !");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("CheckLogin", "Email hoặc khẩu không chính xác !");
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("CheckLogin", "Không thể đăng nhập vào lúc này, vui lòng thử lại sau !");
                    return View(user);
                }
            }
            return View(user);
        }
        #endregion

        #region Logout User
        public ActionResult Logout()
        {
            if (HttpContext.Session["User"] != null)
            {
                var users = (User)HttpContext.Session["User"];
                if (HttpRuntime.Cache["LoggedInUsers"] != null)//check if the list has been created
                {
                    //the list is not null so we retrieve it from the cache
                    List<User> loggedInUsers = (List<User>)HttpRuntime.Cache["LoggedInUsers"];
                    foreach (var user in loggedInUsers)
                    {
                        if (user.Email.Contains(users.Email))//if the user is in the list
                        {
                            //then remove them
                            loggedInUsers.Remove(user);
                            break;
                        }
                    }
                    // else do nothing
                }
                FormsAuthentication.SignOut();
                Session.Remove("User");
                return RedirectToAction("Login", "Users");
            }
            return RedirectToAction("Logout", "Users");
        }
        #endregion

        #region Send Email
        //send email (non Action)
        [NonAction]
        public void VerifyLinkEmail(string email, string activeCode, string emailFor)
        {
            var verifyUrl = "/Admin/Users/" + emailFor + "/" + activeCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("c1709h@gmail.com", "ResetPassWord no-reply");
            var toEmail = new MailAddress(email);
            string subject = "";
            string body = "";
            if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "<br/><br/>Đã có một yêu cầu đặt đặt lại mật khẩu web Mobile Shop" +
            "nếu đó là bạn, vui lòng click vào link bên dưới" +
            " <br/><br/><a href='" + link + "'>" + link + "</a>";
            }
            var smpt = new SmtpClient();
            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            smpt.Send(message);

        }
        #endregion

        #region ForgotPassword
        // POST: Admin/Users/forgot pwd
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            string success = "";
            string error = "";
            var emailUser = db.Users.Where(x => x.Email == email).SingleOrDefault();
            if (emailUser != null)
            {
                ///Send email for reser pwd
                string resetCode = Guid.NewGuid().ToString();
                VerifyLinkEmail(emailUser.Email, resetCode, "ResetPassword");
                emailUser.ResetPasswordCode = resetCode;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                success = "Một email đã được gửi đến hộp thư của bạn, vui lòng kiểm tra hộp thư đến trong Email";
            }
            else
            {
                error = "Email chưa được đăng ký";
            }
            ViewBag.success = success;
            ViewBag.error = error;
            return View();
        }
        #endregion

        #region Reset Password
        // POST: Admin/Users/reset pwd
        [AllowAnonymous]
        public ActionResult ResetPassword(string id)
        {
            if (id == null)
            {
                return View("Unauthorized");
            }
            //verify the reset password link
            var result = db.Users.Where(x => x.ResetPasswordCode == id).FirstOrDefault();
            if (result != null)
            {
                ResetPasswordModel m = new ResetPasswordModel();
                m.ResetCode = id;
                return View(m);
            }
            else
            {
                return View("Unauthorized");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel m)
        {
            var success = "";
            if (ModelState.IsValid)
            {
                var result = db.Users.Where(x => x.ResetPasswordCode == m.ResetCode).FirstOrDefault();
                if (result != null)
                {
                    result.Password = BCrypt.Net.BCrypt.HashPassword(m.NewPassword);
                    result.ResetPasswordCode = "";
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                    success = "Mật khẩu mới đã được thay đổi hay thử !";
                }
            }
            ViewBag.success = success;
            return View(m);
        }
        #endregion

        #region Change Password
        [HttpPost]
        [CustomAuth]
        public ActionResult ChangePassword(ChangePassword changepwd)
        {
            var curentUser = (User)HttpContext.Session["User"]; // sesin current user
            if (curentUser == null)
            {
                return Json(new { error = "Vui lòng đăng nhập lại" }, JsonRequestBehavior.AllowGet);
            }
            var user = db.Users.FirstOrDefault(x => x.UserId == curentUser.UserId);
            if (user != null)
            {
                var encodingPwd = BCrypt.Net.BCrypt.Verify(changepwd.Password,user.Password);
                if (!encodingPwd)
                {
                    return Json(new { error = "Mật khẩu cũ không đúng" }, JsonRequestBehavior.AllowGet);
                }
                if (changepwd.NewPassword != changepwd.ConfirmPassword)
                {
                    return Json(new { error = "Mật khẩu mới không khớp" }, JsonRequestBehavior.AllowGet);
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(changepwd.NewPassword);
                db.SaveChanges();
                return Json(new { success = "Thay đổi mật khẩu thành công !!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = "Vui lòng đăng nhập lại" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Info User, Change Info User
        // POST: Admin/Users/ Information current user
        [CustomAuth]
        public ActionResult Index(string userName)
        {
            var InfoUser = (User)HttpContext.Session["User"];
            if (InfoUser == null)
            {
                return View("Unauthorized");
            }
            return View(db.Users.Find(InfoUser.UserId));
        }
        [HttpPost]
        [CustomAuth]
        public JsonResult ChangeInfoUser(User u, HttpPostedFileBase imageUpload)
        {
            if (Request.Files.Count > 0)
            {
                imageUpload = Request.Files[0];
            }
            List<User> loggedInUsers = (List<User>)HttpRuntime.Cache["LoggedInUsers"];
            var curentUser = (User)HttpContext.Session["User"]; // sesin current user
            if (HttpRuntime.Cache["LoggedInUsers"] != null)//check if the list has been created
            {
                //the list is not null so we retrieve it from the cache
                foreach (var user in loggedInUsers)
                {
                    if (user.Email == curentUser.Email)//if the user is in the list
                    {
                        //then remove them
                        loggedInUsers.Remove(user);
                        break;
                    }
                    // else do nothing
                }
            }
            if (ModelState.IsValid)
            {
                User session = Session["User"] as User;
                var result = db.Users.Where(x => x.Email != session.Email).Any(x => x.Email == u.Email);
                if (result)
                {
                    return Json(new { error = "Email đã tồn tại !!", JsonRequestBehavior.AllowGet });
                }
                var current = db.Users.Find(session.UserId);
                current.UserName = u.UserName;
                current.FullName = u.FullName;
                current.Phone = u.Phone;
                if (imageUpload != null)
                {
                    string filename = Path.GetFileNameWithoutExtension(imageUpload.FileName);
                    string extention = Path.GetExtension(imageUpload.FileName);
                    filename = filename + extention;
                    string FullPath = Path.Combine(Server.MapPath("~/Images/"), filename);
                    imageUpload.SaveAs(FullPath);
                    current.Avatar = filename;
                    u.Avatar = current.Avatar;
                    db.SaveChanges();
                    loggedInUsers.Add(current);
                    Session["User"] = current;
                    ModelState.Clear();
                    return Json(new { success = "Cập nhập thành công !!", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    db.SaveChanges();
                    loggedInUsers.Add(current);
                    Session["User"] = current;
                    ModelState.Clear();
                    return Json(new { success = "Cập nhập thành công !!", JsonRequestBehavior.AllowGet });
                }
            }
            loggedInUsers.Add(curentUser);
            return Json(new { error = "Không thể chỉnh sửa thông tin !!", JsonRequestBehavior.AllowGet });
        }
        #endregion

        #region List Users
        //ManagerAdmin
        //JSON: Admin/Users/getall User
        [CustomAuth]
        public ActionResult UserManager()
        {
            ViewBag.GroupId = new SelectList(db.Groups.Where(x => x.Status == x.Status && x.Status != 5 && x.isAdmin == false), "GroupId", "GroupName");
            return View();
        }
        [CustomAuth]
        public JsonResult GetdataUser()
        {
            var getuser = HttpContext.Session["User"] as User;
            db.Configuration.ProxyCreationEnabled = false;
            var result = (from u in db.Users
                          join g in db.Groups
                          on u.GroupId equals g.GroupId
                          where u.IsAdmin == false && u.UserId != getuser.UserId && u.mStatus != 10
                          select new UsersGroup()
                          {
                              FullName = u.FullName,
                              UserId = u.UserId,
                              Email = u.Email,
                              Avatar = u.Avatar,
                              Phone = u.Phone,
                              GroupName = g.GroupName,
                              Status = u.Status,
                              Background = g.Background,
                              GroupId = u.GroupId
                          }).AsEnumerable();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create User
        // POST: Admin/Users/Create users
        public ActionResult Create()
        {
            var countGroupId = db.Groups.Where(x => x.isAdmin == false && x.Status == 1).Count();
            ViewBag.GroupId = new SelectList(db.Groups.Where(x => x.isAdmin == false && x.Status == 1), "GroupId", "GroupName");
            if (countGroupId == 0)
            {
                setAlert("Error !", "vui lòng thêm mới vai trò trước khi thêmm mới nhân viên !!", "top-right", "error", 7000);
                return RedirectToAction("Index", "Groups");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUser c)
        {
            var countGroupId = db.Groups.Where(x => x.isAdmin == false && x.Status == 1).Count();
            if (countGroupId == 0)
            {
                setAlert("Error !", "Vui lòng thêm mới vai trò trước khi thêmm mới nhân viên !!", "top-right", "error", 7000);
                return RedirectToAction("Index", "Groups");
            }
            ViewBag.GroupId = new SelectList(db.Groups.Where(x => x.isAdmin == false), "GroupId", "GroupName");
            if (ModelState.IsValid)
            {
                try
                {
                    User u = new User();
                    u.FullName = c.FullName;
                    u.Email = c.Email;
                    u.Phone = c.Phone;
                    u.Password = BCrypt.Net.BCrypt.HashPassword(c.Password);
                    u.IsAdmin = false;
                    u.Status = 1;
                    u.GroupId = c.GroupId;
                    db.Users.Add(u);
                    db.SaveChanges();
                    return RedirectToAction("UserManager", "Users");
                }
                catch (Exception)
                {
                    setAlert("Error !", "Đã xảy ra lỗi khi thêm mới nhân viên, vui lòng thử lại !!", "top-right", "error", 5000);
                    return View(c);
                }
            }
            return View(c);
        }
        #endregion

        #region Change Status, Delete Users
        // POST: Do not use
        //public ActionResult Edit(int id)
        //{
        //    return View(db.Users.Find(id));
        //}
        //[HttpPost]
        //public ActionResult Edit(User u)
        //{
        //    var result = db.Users.Where(x => x.UserId == u.UserId).SingleOrDefault();
        //    if (result != null)
        //    {
        //        result.UserName
        //    }
        //    return View();
        //}

        /// <summary>
        /// Action Post [Change(edit) Users Status and GroupId(Roles) use Json]
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeStatus(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var result = db.Users.SingleOrDefault(x => x.UserId == id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChaneStatusSuccess(User u)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var data = db.Users.SingleOrDefault(x => x.UserId == u.UserId);
            if (data != null)
            {
                data.Status = u.Status;
                data.GroupId = u.GroupId;
                db.SaveChanges();
            }
            return Json(new { data, success = "Cập nhập thành công !" }, JsonRequestBehavior.AllowGet);
        }

        // JSON: Admin/Users/delete user
        [HttpPost]
        public JsonResult DeleteUsers(int id)
        {
            var result = db.Users.SingleOrDefault(x => x.UserId == id);
            if (result != null)
            {
                result.mStatus = 10;
                db.SaveChanges();
            }
            return Json(new { success = "Xoá thành công !" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}