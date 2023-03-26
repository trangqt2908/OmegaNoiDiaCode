using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class WebModuleController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View();
        }
        [ChildActionOnly]
        [AllowAnonymous]
        [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Navigation(string uId)
        {
            var webmodules = from e in db.WebModules
                             where (e.ParentID == null)
                             orderby e.Order
                             select e;
            //WebModule webmodules = this.db.WebModules.Where(m => m.ParentID == id && m.Culture == ApplicationService.Culture).FirstOrDefault();
            return PartialView(webmodules);
        }
        [AllowAnonymous]
        [ChildActionOnly]
        public PartialViewResult _ColLeft(int id = 0, int m_id = 0)
        {
            id = m_id > 0 ? m_id : id;

            if (id == 0)
            {
                WebModule module1 = this.db.WebModules.Where(x => x.UID == "project").FirstOrDefault();
                return PartialView(module1);
            }

            WebModule module = this.db.WebModules.Find(id);
            if (module.SubWebModules.Count == 0)
            {
                module = this.db.WebModules.Where(x => x.UID == "project").FirstOrDefault();
            }

            while (module.Parent != null)
            {
                module = module.Parent;
            }

            return PartialView(module);
        }
        //public PartialViewResult _ColLeft(string uId)
        //{
        //    WebModule module = this.db.WebModules.Where(m => m.UID == uId && m.Culture == ApplicationService.Culture).FirstOrDefault();
        //    return PartialView(module);
        //}
    }
}
