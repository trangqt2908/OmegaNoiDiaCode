using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEB.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        //
        // GET: /Admin/Home/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Loading()
        {
            return View();
        }
       
    }
}
