using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using WebMatrix.WebData;
using System.Data.Entity;

namespace WEB.Areas.Admin.Controllers
{
    public class AdvertisementController : BaseController
    {
        private WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Advertisement_Read([DataSourceRequest] DataSourceRequest request)
        {
            var contents = db.Advertisements.Select(x => new { x.ID, x.Title, x.Media,x.Culture, x.Link,x.Target, Position=x.AdvertisementPosition.Title }).ToList(); ;
            return Json(contents.ToDataSourceResult(request));
        }


        //
        // GET: /Admin/Advertisement/Create

        public ActionResult Add()
        {
            ViewBag.Language = Language;
            var advPositions = db.AdvertisementPositions.Select(x => new {x.ID,x.Title,x.Image }).ToList();
            ViewBag.AdvPositions = advPositions;
            return View();
        }

        //
        // POST: /Admin/Advertisement/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(Advertisement model )
        {
            ViewBag.Language = Language;
            var advPositions = db.AdvertisementPositions.Select(x => new { x.ID, x.Title, x.Image }).ToList();
            ViewBag.AdvPositions = advPositions;
            if (ModelState.IsValid)
            {
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now; 
                db.Advertisements.Add(model);
                db.SaveChanges(); 
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }

            return View(model);
        }

        //
        // GET: /Admin/Advertisement/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ViewBag.Language = Language;
            var advPositions = db.AdvertisementPositions.Select(x => new { x.ID, x.Title, x.Image }).ToList();
            ViewBag.AdvPositions = advPositions;
            Advertisement model = db.Advertisements.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
             
            return View(model);
        }

        //
        // POST: /Admin/Advertisement/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Advertisement model, HttpPostedFileBase image)
        {
            ViewBag.Language = Language;
            var advPositions = db.AdvertisementPositions.Select(x => new { x.ID, x.Title, x.Image }).ToList();
            ViewBag.AdvPositions = advPositions;
            if (ModelState.IsValid)
            {
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;
                 
                db.Advertisements.Attach(model);
                db.Entry(model).Property(a => a.Title).IsModified = true;
                db.Entry(model).Property(a => a.Description).IsModified = true; 
                db.Entry(model).Property(a => a.Target).IsModified = true;
                db.Entry(model).Property(a => a.Media).IsModified = true;
                db.Entry(model).Property(a => a.Link).IsModified = true;
                db.Entry(model).Property(a => a.AdvertisementPositionID).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                db.Entry(model).Property(a => a.Culture).IsModified = true;
                db.SaveChanges();

               
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }



        public ActionResult Delete(int id)
        {

            var role = db.Set<Advertisement>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Advertisement model)
        {



            try
            {
                var role = db.Advertisements.Attach(model);
                db.Set<Advertisement>().Remove(role);
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

            var temp = from p in db.Set<Advertisement>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<Advertisement> model)
        {

            var temp = new List<Advertisement>();
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
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read(); top.treeview.dataSource.read();";
                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
                return View(temp);
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View();
            }

        }


 

        
    }
}