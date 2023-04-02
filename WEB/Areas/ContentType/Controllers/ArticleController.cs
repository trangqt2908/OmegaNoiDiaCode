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
using System.Data.Entity.Validation;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ArticleController : BaseController
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
                        var webContent = db.WebContents.FirstOrDefault(x => x.ID == model.ID);
                        webContent.ModifiedBy = WebSecurity.CurrentUserName;
                        webContent.ModifiedDate = DateTime.Now;
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
                        
                        webContent.Title = model.Title;
                        webContent.Description = model.Description;
                        webContent.MetaDescription = model.MetaDescription;
                        webContent.MetaKeywords = model.MetaKeywords;
                        webContent.Image = model.Image;
                        webContent.MetaTitle = model.MetaTitle;
                        webContent.Status = model.Status;
                        webContent.Body = model.Body;
                        webContent.ModifiedBy = WebSecurity.CurrentUserName; 
                        webContent.ModifiedDate = DateTime.Now;
                        webContent.Tag = model.Tag;
                        webContent.SeoTitle = model.SeoTitle;
                        webContent.Order = model.Order;

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
       

    }
}
