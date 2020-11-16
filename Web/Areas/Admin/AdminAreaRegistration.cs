using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Login Users",
                "admin/dang-nhap",
                new { controller = "Users", action = "Login", id = UrlParameter.Optional },
                new[] { "Web.Areas.Admin.Controllers" }
            );
           // context.MapRoute(
           //    "Users_default",
           //    "Admin/{userName}",
           //    new { controller = "Users", action = "Index", id = UrlParameter.Optional },
           //    new[] { "Web.Areas.Admin.Controllers" }
           //);
            //context.MapRoute(
            //    "Infor Users",
            //    "admin/Users/{User}",
            //    new { controller = "Users", action = "Index" },
            //    new { User = new GuidConstraint(), guid2 = new GuidConstraint() },
            //    new[] { "Web.Areas.Admin.Controllers" }
            //    );
            ///
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "Web.Areas.Admin.Controllers" }
            );
        }
        public class RootRouteConstraint<T> : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                var rootMethodNames = typeof(T).GetMethods().Select(x => x.Name.ToLower());
                return rootMethodNames.Contains(values["action"].ToString().ToLower());
            }
        }
    }
}