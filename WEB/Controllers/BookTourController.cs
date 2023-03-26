using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class BookTourController : BaseController
    {
        //
        // GET: /BookTour/
        WebContext db = new WebContext();
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Book(int id)
        {
            var item = db.WebContents.Find(id);

            ViewBag.Content = item;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult Book(BookTour model)
        {
            if (ModelState.IsValid)
            {
                db.Set<BookTour>().Add(model);
                //db.SaveChanges();

                //bool sendmail = ApplicationService.SendMail(model);

                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }

    }
}