using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using Common;
using WebModels;
using System.Data.Entity;
using WebMatrix.WebData;

namespace WEB.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AccessWebModuleController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/AccessLog/

        public ActionResult _Index(int id)
        {
            
            return PartialView();
        }
    }
}
