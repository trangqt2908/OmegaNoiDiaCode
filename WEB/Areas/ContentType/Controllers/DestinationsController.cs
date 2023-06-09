﻿using System.Linq;
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
using System.Data.Entity.Validation;
using System.Configuration;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class DestinationsController : BaseController
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

            var contents = db.WebContents.Where(x => x.WebModuleID == id).Select(x => new { x.ID, x.Title, x.Description, x.Image, x.ModifiedBy, x.ModifiedDate });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }
            return Json(contents.ToDataSourceResult(request));
        }

        public JsonResult GetWebContents(string text, int id)
        {

            {
                var contents = from x in db.WebContents where x.WebModuleID == id select x;
                if (!string.IsNullOrEmpty(text))
                {
                    contents = contents.Where(p => p.Title.Contains(text));
                }

                return Json(contents, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Add(int id)
        {
            ViewBag.UID = UniqueKeyGenerator.RNGTicks(10);
            ViewBag.ID = id;
            var model = new WebContent();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(WebContent model, HttpPostedFileBase image)
        {
            ViewBag.ID = model.WebModuleID;
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;
                model.Views = 1;
                if (image != null)
                {
                    model.Image = image.ImageNoResizeSave("/uploads/" + (now.Month.ToString("00") + now.Year));
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = image.FileName,
                        MetaTitle = image.FileName.UnsignNormalize(),
                        FullPath = model.Image,
                        UID = model.UID,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
                if (string.IsNullOrEmpty(model.MetaTitle))
                {
                    model.MetaTitle = model.Title.UnsignNormalize();
                }
                db.Set<WebContent>().Add(model);
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


            var model = db.Set<WebContent>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.UID = model.UID;
            return View("Edit", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebContent model, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                {
                    try
                    {
                        var now = DateTime.Now;
                        model.ModifiedBy = WebSecurity.CurrentUserName;
                        model.ModifiedDate = DateTime.Now;
                        if (image != null)
                        {
                            model.Image = image.ImageNoResizeSave("/uploads/" + (now.Month.ToString("00") + now.Year));

                        }

                        var temp = db.ProductInfos.Where(x => x.ID == model.ID);
                        if (temp.Count() == 0)
                        {
                            db.Set<ProductInfo>().Add(new ProductInfo() { ID = model.ID });
                            db.SaveChanges();
                        }
                        else
                        {
                            model.ProductInfo.ID = model.ID;
                        }

                        db.WebContents.Attach(model);

                        db.Entry(model).Property(a => a.Title).IsModified = true;
                        db.Entry(model).Property(a => a.Description).IsModified = true;
                        db.Entry(model).Property(a => a.MetaDescription).IsModified = true;
                        db.Entry(model).Property(a => a.MetaKeywords).IsModified = true;
                        db.Entry(model).Property(a => a.Image).IsModified = true;
                        db.Entry(model).Property(a => a.MetaTitle).IsModified = true;
                        db.Entry(model).Property(a => a.Status).IsModified = true;
                        db.Entry(model).Property(a => a.Body).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                        db.Entry(model).Property(a => a.SeoTitle).IsModified = true;
                        db.Entry(model).Property(a => a.Order).IsModified = true;
                        db.Entry(model).Property(a => a.Event).IsModified = true;
                        db.Entry(model).Property(a => a.LinkVideo).IsModified = true;
                        db.Entry(model).Property(a => a.Link).IsModified = true;

                        db.Entry(model.ProductInfo).Property(a => a.Price).IsModified = true;
                        db.Entry(model.ProductInfo).Property(a => a.Code).IsModified = true;
                        db.Entry(model.ProductInfo).Property(a => a.Duration).IsModified = true;
                        db.Entry(model.ProductInfo).Property(a => a.DepartureTime).IsModified = true;
                        db.Entry(model.ProductInfo).Property(a => a.Destination).IsModified = true;
                        db.Entry(model.ProductInfo).Property(a => a.TourTime).IsModified = true;
                        db.Entry(model.ProductInfo).Property(a => a.Discount).IsModified = true;
                        db.Entry(model.ProductInfo).Property(a => a.Transportation).IsModified = true;

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
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Delete(int id)
        {
            {
                var model = db.Set<WebContent>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebContent model)
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
                var temp = from p in db.Set<WebContent>()
                           where lstObjId.Contains(p.ID)
                           select p;

                return View(temp.ToList());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebContent> model)
        {

            {
                var temp = new List<WebContent>();
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
            else webmodule = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault();
            var contents = new List<WebContent>();
            ViewBag.WebModule = webmodule;
            contents = db.WebContents.Where(x => x.WebModuleID == webmodule.ID && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).ToList();
            var sub = webmodule.SubWebModules.Where(x => x.Status == (int)Status.Public);
            if (sub.Any())
            {
                foreach (var item in sub)
                {
                    var lstContentSub = db.WebContents.Where(x => x.WebModuleID == item.ID && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).ToList();
                    if (lstContentSub.Any())
                    {
                        contents.AddRange(lstContentSub);
                    }
                }
            }
            contents = contents.OrderBy(x => x.Order).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * 12).Take(12).ToList());
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubDetail(string metatitle)
        {
            var content = db.Set<WebContent>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault();
            return PartialView(content);
        }

        [AllowAnonymous]
        public ActionResult _TopDestinations(int webContentId)
        {
            var webmodule = db.WebModules.Where(x => x.UID.Equals("destinations")
         && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
            var result = new List<WebContent>();

            var desIds = db.ProductInfos.Where(x => x.ID == webContentId).FirstOrDefault();
            if (desIds != null)
            {
                var data = desIds.Destination.Split(',').ToList().ConvertStringToInt().ToList();

                foreach (var item in data)
                {
                    var obj = db.WebContents.Where(x => x.ID == item && x.WebModule.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                    if (obj != null)
                    {
                        result.Add(obj);
                    }
                }
            }


            return PartialView(result);
        }

        [AllowAnonymous]
        public ActionResult _OtherDestination(int webModuleId, int webContentId)
        {
            var contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID < webContentId && x.WebModuleID == webModuleId).Take(5).ToList();
            var contents2 = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID > webContentId && x.WebModuleID == webModuleId).Take(5).ToList();
            contents.AddRange(contents2);
            return PartialView(contents);
        }


    }
}
