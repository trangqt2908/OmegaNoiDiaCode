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

namespace WEB.Areas.ContentType.Controllers
{
    public partial class TourController : BaseController
    {
        WebContext db = new WebContext();

        HomeController home = new HomeController();
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

        public JsonResult GetDestinations()
        {
            //var webmodule = db.WebModules.Where(x => x.UID.Equals("destinations")
            //   && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

            //List<WebContent> webContents = new List<WebContent>();
            //var destinations = GetListContents(webmodule.ID, webContents).Select(x => new { x.ID, x.Title }).ToList();

            //return Json(destinations, JsonRequestBehavior.AllowGet);

            var typeOfTour = db.WebModules.Where(x => x.UID.Equals("tour-trong-nuoc") || x.UID.Equals("tour-nuoc-ngoai")).ToList();
            
            List<WebModule> results = new List<WebModule>();
            foreach (var item in typeOfTour)
            {
                GetListModule(item.ID, results);
            }
            return Json(results.Select(x => new { x.ID, x.Title }), JsonRequestBehavior.AllowGet);
        }
        private List<WebModule> GetListModule(int webModuleId, List<WebModule> results)
        {
            var webModule = db.WebModules.Where(x => x.ID == webModuleId).FirstOrDefault();
            results.Add(webModule);

            var webModules = db.WebModules.Where(x => x.ParentID == webModuleId);            
            foreach (var childWebModule in webModules)
            {
                GetListModule(childWebModule.ID, results);
            }

            return results;
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
        public ActionResult Add(WebContent model, string productMultiSelectVal, int submit, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(productMultiSelectVal))
                {
                    string lstDestination = string.Join(",", productMultiSelectVal);

                    model.ProductInfo.Destination = lstDestination;
                }
                else
                {
                    model.ProductInfo.Destination = "0";
                }

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

                if (submit == 0)
                {
                    ViewBag.StartupScript = "create_success();";
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Edit", new { id = model.ID });
                }

            }
            else
            {
                return View(model);
            }
        }
        private string GetParentCountires(string values, int parentId)
        {
            var country = (from c in this.db.WebModules where c.ID == parentId && c.Culture.Equals(ApplicationService.Culture) select new { c.ID, c.ParentID }).FirstOrDefault();

            values += "," + parentId;

            if (country != null && country.ParentID.HasValue)
            {
                int parentID = 0;
                int.TryParse(country.ParentID.ToString(), out parentID);

                var module = db.WebModules.Where(x => x.ID == parentID && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                if (module.UID.Equals("destination"))
                {
                    return values;
                }
                else
                {
                    return this.GetParentCountires(values, country.ParentID.Value);
                }
            }
            else
            {
                return values;
            }
        }
        public ActionResult Edit(int id)
        {
            var model = db.Set<WebContent>().Find(id);
            List<string> result = new List<string>();

            if (model == null)
            {
                return HttpNotFound();
            }

            if (model.ProductInfo == null)
            {
                model.ProductInfo = new ProductInfo();
            }

            if (!string.IsNullOrEmpty(model.ProductInfo.Destination))
            {
                result = model.ProductInfo.Destination.Split(new char[] { ',' }).ToList();
                ViewBag.oldSelect = result;

                ViewBag.oldSelectString = model.ProductInfo.Destination;
            }
            else
            {
                ViewBag.oldSelect = "0";
            }


            ViewBag.UID = model.UID;
            return View("Edit", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebContent model, string productMultiSelectVal, string productMultiSelectOld, int submit, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                try
                {
                    var now = DateTime.Now;
                    model.ModifiedBy = WebSecurity.CurrentUserName;
                    model.ModifiedDate = DateTime.Now;
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

                    if (!string.IsNullOrWhiteSpace(productMultiSelectVal))
                    {
                        string lstDestination = string.Join(",", productMultiSelectVal);

                        model.ProductInfo.Destination = lstDestination;
                    }
                    else if (!string.IsNullOrWhiteSpace(productMultiSelectOld))
                    {
                        model.ProductInfo.Destination = productMultiSelectOld;
                    }
                    else
                    {
                        model.ProductInfo.Destination = "0";
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

                    db.Entry(model.ProductInfo).Property(a => a.Price).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.Code).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.Duration).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.DepartureTime).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.Destination).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.TourTime).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.Discount).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.Transportation).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.Policy).IsModified = true;

                    db.SaveChanges();
                    if (submit == 0)
                    {
                        ViewBag.StartupScript = "edit_success();";
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("Edit", new { id = model.ID });
                    }

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

        public ActionResult UpdatePictures(int id)
        {
            var item = db.WebContents.Find(id);
            if (item == null)
                return HttpNotFound();
            ViewBag.UID = item.UID;
            return View(item);
        }

        [AllowAnonymous]
        public ActionResult _SlideContent(int id)
        {
            var lstPhoto = db.ContentImages.Where(x => x.WebContentID == id).ToList();
            return PartialView(lstPhoto);
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
        private List<WebContent> GetListContents(int webModuleId, List<WebContent> results)
        {
            var webContents = db.WebContents.Where(x => x.WebModuleID == webModuleId && x.Status == (int)Status.Public);
            results.AddRange(webContents);

            var childWebModules = db.WebModules.Where(x => x.ParentID == webModuleId);

            foreach (var childWebModule in childWebModules)
            {
                GetListContents(childWebModule.ID, results);
            }

            return results;
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
            var webContents = new List<WebContent>();
            ViewBag.WebModule = webmodule;

            var contents = GetListContents(webmodule.ID, webContents).OrderByDescending(x=>x.ID);

            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * 12).Take(12).ToList());
        }


        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubEmpty(int id, string metatitle)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);

            ViewBag.WebModule = webmodule;

            var contents = db.WebModules.Where(x => x.ParentID == id && x.Culture.Equals(ApplicationService.Culture)).ToList();

            return PartialView(contents);
        }


        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubDetail(string metatitle)
        {
            var content = db.Set<WebContent>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault(); ;
            return PartialView(content);
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _ListDestination(string lstDestination)
        {
            List<WebModule> webmodules = new List<WebModule>();

            if (!string.IsNullOrWhiteSpace(lstDestination))
            {
                var result = lstDestination.Split(new char[] { ',' }).ToList();

                foreach (var item in result)
                {
                    int id = 0;

                    int.TryParse(item, out id);

                    var obj = db.WebModules.Where(x => x.ID == id).FirstOrDefault();

                    if (obj != null)
                    {
                        webmodules.Add(obj);
                    }
                }
            }
            return PartialView(webmodules);
        }


        [AllowAnonymous]
        public ActionResult _ColLeft(int id)
        {
            var module = db.WebModules.Where(x => x.UID.Equals("list-tour") && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

            var contents = db.WebModules.Where(x => x.ParentID == module.ID).ToList();

            var moduleactive = db.WebModules.Where(x => x.ID == id).FirstOrDefault();

            ViewBag.ModuleActive = moduleactive;

            //var contents = db.WebModules.Where(x => x.Parent.UID.Equals("list-tour") && x.Status == (int)Status.Public && x.Culture.Equals(ApplicationService.Culture)).ToList();

            return PartialView(contents);
        }

        [AllowAnonymous]
        public ActionResult _OtherTour(int webContentID, int type)
        {
            var item = db.WebContents.Single(x => x.ID == webContentID);
            ViewBag.WebModuleName = "";
            ViewBag.WebModule = null;
            if (type == 0)
            {
                ViewBag.WebModule = item.WebModule;
                var contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID < webContentID && x.WebModuleID == item.WebModuleID).Take(4).ToList();
                var contents2 = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID > webContentID && x.WebModuleID == item.WebModuleID).Take(5).ToList();
                contents.AddRange(contents2);
                return PartialView(contents.OrderByDescending(x => x.ID));
            }
            else
            {
                ViewBag.WebModuleName = item.Title;
                string des = item.WebModuleID.ToString();

                var contents = new List<WebContent>();

                var dataFirst = db.WebContents.Where(x => x.ProductInfo.Destination.Contains(des));
                foreach (var obj in dataFirst)
                {
                    if (!string.IsNullOrWhiteSpace(obj.ProductInfo.Destination))
                    {
                        var newObj = obj.ProductInfo.Destination.Split(',').Where(x => x == des).FirstOrDefault();
                        if (newObj != null)
                        {
                            contents.Add(obj);
                        }
                    }
                }

                return PartialView(contents.OrderByDescending(x => x.ID));
            }
        }


        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndeOnePage(int id)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            if (webmodule == null)
            {
                return HttpNotFound();
            }

            return PartialView(webmodule);
        }

        [AllowAnonymous]
        public ActionResult _LatestTour(int take)
        {
            var content = db.WebContents.Where(x => x.WebModule.ContentTypeID == "Tour")
                .OrderByDescending(x => x.ID).Take(take).ToList();
                    
            return PartialView(content);
        }


    }
}
