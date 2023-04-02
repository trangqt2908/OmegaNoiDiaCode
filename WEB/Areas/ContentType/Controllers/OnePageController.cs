using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WEB.Models;
using WebModels;

namespace WEB.Areas.ContentType.Controllers
{
    public class OnePageController : BaseController
    {
        private WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {
            var model = db.Set<WebModule>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return PartialView(model);
        }

        public ActionResult EditModule(int id)
        {

            var model = db.Set<WebModule>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View("EditModule", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditModule(WebModule model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    db.WebModules.Attach(model);
                    db.Entry(model).Property(a => a.Body).IsModified = true;
                    db.SaveChanges();

                    ViewBag.StartupScript = "edit_success();";
                    return View(model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

            }
            else
            {
                return View(model);
            }
        }




        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndex(string metatitle)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault(); 
            if (webmodule == null)
            {
                return HttpNotFound();
            }

            return PartialView(webmodule);
        }

        [AllowAnonymous]
        public ActionResult _OtherAbout(int id)
        {
            //about-us-2

            List<WebModule> lstModule = new List<WebModule>();

            var isCheckItem = db.WebModules.Where(x => x.ID == id).FirstOrDefault();
            if (isCheckItem.ParentID == null || isCheckItem.ParentID == 0)
            {
                lstModule = db.WebModules.Where(x => x.ParentID == id).ToList();
            }
            else
            {
                var module = db.WebModules.Where(x => x.ParentID == isCheckItem.ParentID || x.ID == isCheckItem.ParentID);
                lstModule = module.Where(x => x.ID != id).ToList();

            }
            return PartialView(lstModule.Take(10));
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndexComment(int id, string metatitle, int? page)
        {
            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = ControllerContext.ParentActionViewContext.RouteData.Values["controller"],
                action = ControllerContext.ParentActionViewContext.RouteData.Values["action"],
                area = ControllerContext.ParentActionViewContext.RouteData.Values["area"]
            });
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            var contents = new List<Comment>();
            ViewBag.WebModule = webmodule;

            contents = db.Comments.OrderByDescending(x => x.Order).ToList();

            contents = contents.ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * 10).Take(10).ToList());
        }
        public ActionResult _TourService()
        {
            var model = db.WebModules.Where(x => x.UID == "blog").FirstOrDefault();
            if(model!= null)
            {
                var tourItem = db.WebContents.Where(x => x.WebModuleID == model.ID).ToList();
                return View(tourItem);
            }
            return View();
        }
        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _AboutUs(string metatitle)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault();
            if (webmodule == null)
            {
                return HttpNotFound();
            }

            return PartialView(webmodule);
        }

    }
}
