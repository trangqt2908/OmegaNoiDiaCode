using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Common;
using WebMatrix.WebData;
using System.Data.Entity;
using WEB.Models;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ContactController
    {

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndex(string metatitle)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault(); ;
            ViewBag.WebModule = webmodule;

            ViewBag.ID = webmodule.ID;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult _PubIndex(WebContact model)
        {
            if (ModelState.IsValid)
            {
               
                model.CreatedDate = DateTime.Now;
                db.Set<WebContact>().Add(model);
                db.SaveChanges();
                ApplicationService.SendMail(model, "Liên hệ từ www.omegatous.com");
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }

        [AllowAnonymous]
        public ActionResult _PubContact(int id)
        {
            ViewBag.ContentID = id;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult _PubContact(WebContact model)
        {
            if (ModelState.IsValid)
            {
                var module = db.WebModules.Where(x => x.UID.Equals("contact")).FirstOrDefault();
                model.WebModuleID = module.ID;               
                model.CreatedDate = DateTime.Now;
                ApplicationService.SendMail(model, "Liên hệ booking từ www.omegatous.com");
                db.Set<WebContact>().Add(model);
                db.SaveChanges();

                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }
 
 
    }
}
