using Models;
using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Areas.Admin.Models
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        MobileShopContext db = new MobileShopContext();
        //Check login, quyền
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //check Login
            if (HttpContext.Current.Session["User"] == null)
            {
                return false;
            }

            //Lấy thông tin của User đã login.
            var getuser = (User)HttpContext.Current.Session["User"];
            //Lấy tên Controller hiện tại
            var getcontrollers = HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("Controller");
            var ConverController = System.Globalization.CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(getcontrollers);

            //Lấy các quyền hiện tại của User.
            var RoleUser = db.GroupRoles.Where(x => x.GroupId == getuser.GroupId).ToList();

            //Kiểm tra quyền
            //Kiểm tra xem có yêu cầu quyền hay không.
            if (String.IsNullOrEmpty(this.Roles))
            {
                return true;
            }

            //Nếu có yêu cầu thỳ kiểm tra quyền (RoleUser) hiện tại có quyền này hay không 
            if (!RoleUser.Any(x => x.BusinessId == ConverController && x.RoleId == this.Roles))
            {
                //Highest admin
                if (getuser.IsAdmin == true)
                {
                    return true;
                }
                //Nếu không có.
                return false;
            }
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new ViewResult()
            {
                ViewName = "Unauthorized"
            };
        }

    }
    //public class TrackLoginsFilter : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //        Dictionary<string, DateTime> loggedInUsers = SecurityHelper.GetLoggedInUsers();

    //        if (HttpContext.Current.User.Identity.IsAuthenticated)
    //        {
    //            if (loggedInUsers.ContainsKey(HttpContext.Current.User.Identity.Name))
    //            {
    //                loggedInUsers[HttpContext.Current.User.Identity.Name] = System.DateTime.Now;
    //            }
    //            else
    //            {
    //                loggedInUsers.Add(HttpContext.Current.User.Identity.Name, System.DateTime.Now);
    //            }

    //        }

    //        // remove users where time exceeds session timeout
    //        var keys = loggedInUsers.Where(u => DateTime.Now.Subtract(u.Value).Minutes >
    //                   HttpContext.Current.Session.Timeout).Select(u => u.Key);
    //        foreach (var key in keys)
    //        {
    //            loggedInUsers.Remove(key);
    //        }

    //    }
    //}
    //public static class SecurityHelper
    //{
    //    public static Dictionary<string, DateTime> GetLoggedInUsers()
    //    {
    //        Dictionary<string, DateTime> loggedInUsers = new Dictionary<string, DateTime>();

    //        if (HttpContext.Current != null)
    //        {
    //            loggedInUsers = (Dictionary<string, DateTime>)HttpContext.Current.Application["loggedinusers"];
    //            if (loggedInUsers == null)
    //            {
    //                loggedInUsers = new Dictionary<string, DateTime>();
    //                HttpContext.Current.Application["loggedinusers"] = loggedInUsers;
    //            }
    //        }
    //        return loggedInUsers;

    //    }
    //}
}