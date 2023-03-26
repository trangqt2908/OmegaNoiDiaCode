using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using WEB.Models;
using Kendo.Mvc;

namespace WEB.Areas.Admin.Controllers
{
    public class SupportController : BaseController
    {
        WebContext db = new WebContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LiveSupport_Read([DataSourceRequest] DataSourceRequest request)
        {
            var c = (from x in db.SupportOnlines select new { x.ID, x.Title, x.Yahoo, x.Skype,x.Phone,x.Culture,x.Email,x.Order });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("Order", System.ComponentModel.ListSortDirection.Descending));
            }

            return Json(c.ToDataSourceResult(request));
        }

        

        public ActionResult Add()
        {
            var support = new SupportOnline();
            support.Culture = ApplicationService.Culture;
            return View(support);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(SupportOnline model)
        {
         
            if (ModelState.IsValid)
            {
                db.SupportOnlines.Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }

            return View(model);
        }
        
        public ActionResult Edit(int id = 0)
        {
            SupportOnline model = db.SupportOnlines.Find(id);
            
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(SupportOnline model)
        {
            
            if (ModelState.IsValid)
            {
                db.SupportOnlines.Attach(model);
                db.Entry(model).Property(a => a.Title).IsModified = true;    
                db.Entry(model).Property(a => a.Order).IsModified = true;
                db.Entry(model).Property(a => a.Yahoo).IsModified = true;
                db.Entry(model).Property(a => a.Skype).IsModified = true;
                db.Entry(model).Property(a => a.Phone).IsModified = true;

                db.Entry(model).Property(a => a.Email).IsModified = true;
                db.Entry(model).Property(a => a.Status).IsModified = true;
                db.Entry(model).Property(a => a.Culture).IsModified = true;
                db.SaveChanges();
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var s = db.Set<SupportOnline>().Find(id);
            if (s == null)
            {
                return HttpNotFound();
            }
            return View("Delete", s);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(SupportOnline model)
        {
            try
            {
                var s = db.SupportOnlines.Attach(model);
                db.Set<SupportOnline>().Remove(s);
                db.SaveChanges();
                ViewBag.StartupScript = "delete_success();";
                return View();
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

        }

        public ActionResult Deletes(string id)
        {

            var objects = id.Split(',');
            var lstSiteID = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstSiteID.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }

            var temp = from p in db.Set<SupportOnline>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<SupportOnline> model)
        {

            var temp = new List<SupportOnline>();
            foreach (var item in model)
            {
                try
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();

                }
                catch (Exception)
                {
                    db.Entry(item).State = EntityState.Unchanged;
                    temp.Add(item);
                }
            }

            if (temp.Count == 0)
            {
                ViewBag.StartupScript = "deletes_success();";
                return View(temp);
            }
            else if (temp.Count > 0)
            {
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read();  ";
                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
                return View(temp);
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
