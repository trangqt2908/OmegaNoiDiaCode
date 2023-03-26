using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Filters;
using WebMatrix.WebData;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using WebModels;
using Newtonsoft.Json.Linq;
using System.Data;
using System;
using System.Collections.Generic;
using Common;
using Kendo.Mvc;
using WEB.Models;
using System.Data.Entity;
using System.Web.Routing;
using System.IO;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class WebContentInformationController : BaseController
    {
        WebContext db = new WebContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        protected override void Initialize(RequestContext requestContext)
        {

            base.Initialize(requestContext);
        }

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {

            ViewBag.ID = id;
            return PartialView();
        }

        public ActionResult WebContent_Read([DataSourceRequest] DataSourceRequest request, int id)
        {

            var contents = db.WebContentInformations.Where(x => x.WebModuleID == id).Select(x => new { x.ID, x.Title, x.Order });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }
            return Json(contents.ToDataSourceResult(request));
        }

        public ActionResult Add(int id)
        {
            ViewBag.UID = UniqueKeyGenerator.RNGTicks(10);
            ViewBag.ID = id;
            var model = new WebContentInformation();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(WebContentInformation model)
        {
            ViewBag.ID = model.WebModuleID;
            if (ModelState.IsValid)
            {
                db.Set<WebContentInformation>().Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult Edit(int id)
        {
            var model = db.Set<WebContentInformation>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View("Edit", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebContentInformation model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.WebContentInformations.Attach(model);
                    db.Entry(model).Property(a => a.Title).IsModified = true;
                    db.Entry(model).Property(a => a.Body).IsModified = true;
                    db.Entry(model).Property(a => a.Order).IsModified = true;
                    db.Entry(model).Property(a => a.Width).IsModified = true;
                    db.Entry(model).Property(a => a.Description).IsModified = true;
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
        public ActionResult Delete(int id)
        {
            {
                var model = db.Set<WebContentInformation>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebContentInformation model)
        {
            {

                try
                {
                    var deletes = db.ContentRelateds.Where(x => x.MainID == model.ID || x.RelatedID == model.ID).ToList();

                    for (int i = deletes.Count - 1; i >= 0; i--)
                    {
                        db.Entry(deletes[i]).State = EntityState.Deleted;
                        db.SaveChanges();
                    }

                    db.Entry(model).State = EntityState.Deleted;
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
        }

        public ActionResult Deletes(string id)
        {

            var objects = id.Split(',');
            var lstObjId = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstObjId.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }

            {
                var temp = from p in db.Set<WebContentInformation>()
                           where lstObjId.Contains(p.ID)
                           select p;

                return View(temp.ToList());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebContentInformation> model)
        {
            var temp = new List<WebContentInformation>();
            foreach (var item in model)
            {
                try
                {
                    var deletes = db.ContentRelateds.Where(x => x.MainID == item.ID || x.RelatedID == item.ID).ToList();
                    for (int i = deletes.Count - 1; i >= 0; i--)
                    {
                        db.Entry(deletes[i]).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
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
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read();";
                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
                return View(temp);
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View();
            }
        }


        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndex(string metatitle, int? page)
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
            else webmodule = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault(); ;
            var contents = new List<WebContent>();

            ViewBag.WebModule = webmodule;
            ViewBag.WebModuleParent = webmodule.Parent;

            string des = webmodule.ID.ToString();

            contents = db.WebContents.Where(x => x.ProductInfo.Destination.Contains(des)).OrderByDescending(x => x.CreatedDate).ToList();

            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * ApplicationService.PageSizeSmall).Take(ApplicationService.PageSizeSmall).ToList());

        }

        [AllowAnonymous]
        public ActionResult _InformationDestination(int webmoduleId)
        {
            var contents = db.WebContentInformations.Where(x => x.WebModuleID == webmoduleId).ToList();

            return PartialView(contents.OrderByDescending(x => x.Order));
        }
        [AllowAnonymous]
        public ActionResult _OtherDestination(int id)
        {
            var item = db.WebModules.Single(x => x.ID == id);
            var contents = db.WebModules.Where(x => x.Status == (int)Status.Public && x.ID < id && x.ParentID == item.ParentID).Take(4).ToList();

            var contents2 = db.WebModules.Where(x => x.Status == (int)Status.Public && x.ID > id && x.ParentID == item.ParentID).Take(4).ToList();
            contents.AddRange(contents2);

       
        
            return PartialView(contents.Take(4));
        }
    }
}
