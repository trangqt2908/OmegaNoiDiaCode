using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class AdvertisementController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
 
        [ChildActionOnly]
        [AllowAnonymous]
         public ActionResult _Adv(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.AdvertisementPosition.UID.ToLower().Equals(position) &&
                         ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))
                );
            ViewBag.Position = position;
            return PartialView(adv);
        }

        public JsonResult JIndex(string position)
        {
            var adv = db.Advertisements.Where(x =>
                x.AdvertisementPosition.UID.ToLower().Equals(position) &&

                         ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))


                ).Select(x => new { x.ID, x.Title, x.Description, x.Link, x.Media, x.Target });
            return Json(adv, JsonRequestBehavior.AllowGet);
        }
    }
}