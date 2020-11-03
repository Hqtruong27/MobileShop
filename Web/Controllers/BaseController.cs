using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class BaseController : Controller
    {
        #region Notication
        // GET: Base
        protected void setAlert(string notication, string messenger, string position, string type,int hideAfter)
        {
            TempData["notication"] = notication;
            TempData["alertMessenger"] = messenger;
            TempData["position"] = position;
            TempData["hideAfter"] = hideAfter;
            if (type == "success")
            {
                TempData["TypeAlert"] = "success";
            }
            if (type == "warning")
            {
                TempData["TypeAlert"] = "warning";
            }
            if (type == "error")
            {
                TempData["TypeAlert"] = "error";
            }
        }
        protected void notify(string notify, string type)
        {
            TempData["notify"] = notify;
            if (type == "success")
            {
                TempData["typeAlert"] = "success";
            }
            if (type == "error")
            {
                TempData["typeAlert"] = "error";
            }
        }
        #endregion
    }
}