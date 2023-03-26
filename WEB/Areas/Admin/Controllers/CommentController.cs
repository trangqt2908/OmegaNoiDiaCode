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
using System.Configuration;

namespace WEB.Areas.Admin.Controllers
{
    public partial class CommentController : BaseController
    {
        WebContext db = new WebContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WebContent_Read([DataSourceRequest] DataSourceRequest request)
        {
            var contents = db.Comments
                .Select(x => new { x.ID, x.Title, x.Order });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }
            return Json(contents.ToDataSourceResult(request));
        }

        public ActionResult Add(int id)
        {
            ViewBag.ID = id;
            var model = new Comment();
            ViewBag.Avatar = model.Image == null ? "/Images/no_avatar_60x60.jpg" : model.Image;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(Comment model, HttpPostedFileBase imagefile)
        {
            if (ModelState.IsValid)
            {
                if (imagefile != null)
                {
                    var name = imagefile.FileName;
                    string extension = Path.GetExtension(name);
                    var newName = model.Image + extension;
                    var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uploads/avatars/"));
                    if (!dir.Exists) dir.Create();
                    var uri = "/content/uploads/avatars/" + newName;
                    imagefile.SaveAs(HttpContext.Server.MapPath(uri));
                    try
                    {
                        if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                        {
                            var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 240, 240, true, 80);
                            model.Image = uri;
                        }
                        else
                        {
                            Utility.DeleteFile(uri);
                        }
                    }
                    catch (Exception)
                    { }
                }

                db.Set<Comment>().Add(model);

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
            var model = db.Set<Comment>().Find(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.Avatar = model.Image == null ? "/Images/no_avatar_60x60.jpg" : model.Image;

            return View("Edit", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Comment model, HttpPostedFileBase imagefile)
        {
            if (ModelState.IsValid)
            {
                if (imagefile != null)
                {
                    var name = imagefile.FileName;
                    string extension = Path.GetExtension(name);
                    var newName = model.Title.Replace(" ", "_") + extension;
                    var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uploads/avatars/"));
                    if (!dir.Exists) dir.Create();
                    var uri = "/content/uploads/avatars/" + newName;
                    imagefile.SaveAs(HttpContext.Server.MapPath(uri));
                    try
                    {
                        if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                        {
                            var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 240, 240, true, 80);
                            model.Image = uri;
                        }
                        else
                        {
                            Utility.DeleteFile(uri);
                        }
                    }
                    catch (Exception)
                    { }
                }

                try
                {
                    db.Comments.Attach(model);

                    db.Entry(model).Property(a => a.Title).IsModified = true;
                    db.Entry(model).Property(a => a.Body).IsModified = true;
                    db.Entry(model).Property(a => a.Order).IsModified = true;
                    db.Entry(model).Property(a => a.Image).IsModified = true;
                    db.Entry(model).Property(a => a.DateTime).IsModified = true;
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
        public ActionResult Delete(Comment model)
        {
            try
            {
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

        //[ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndex(int id)
        {
            IList<Comment> comment = this.db.Comments.OrderBy(m => m.Order).ToList();

            return PartialView(comment);
        }
    }
}
