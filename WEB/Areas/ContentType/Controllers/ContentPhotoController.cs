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
using System.Configuration;
using System.Transactions;
using System.Web.Routing;
using System.IO;

namespace WEB.Areas.ContentType.Controllers
{
    public class ContentPhotoController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {
            //ViewBag.ID = id;
            //ViewBag.View = false;
            //ViewBag.Add = false;
            //ViewBag.Edit = false;
            //ViewBag.Delete = false;
            //var access = db.AccessWebModules.Where(x => x.UserId == WebSecurity.CurrentUserId && x.WebModuleID == id);
            //if (access.Count() > 0)
            //{
            //    var item = access.FirstOrDefault();
            //    ViewBag.View = item.View;
            //    ViewBag.Add = item.Add;
            //    ViewBag.Edit = item.Edit;
            //    ViewBag.Delete = item.Delete;
            //}
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

        public ActionResult Add(int id)
        {
            ViewBag.UID = UniqueKeyGenerator.RNGTicks(10);
            ViewBag.ID = id;
            var model = new WebContent();
            ViewBag.ListCategory = new SelectList(GetCategories(), "ID", "Title");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(WebContent model, int submit, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;

                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
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

        public ActionResult Edit(int id)
        {
            var model = db.Set<WebContent>().Find(id);
            if (model.ProductInfo == null) model.ProductInfo = new ProductInfo();
            if (model == null)
                return HttpNotFound();

            ViewBag.UID = model.UID;
            return View("Edit", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebContent model, int submit, HttpPostedFileBase image)
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
            try
            {
                db.Entry(model).State = EntityState.Deleted;
                db.SaveChanges();
                ViewBag.StartupScript = "delete_success();";
                return View(model);
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

        private List<WebCategory> GetCategories()
        {
            int ctype = (int)CTypeCategories.Product;
            var root = new WebCategory() { ID = 0, Title = Resources.Common.Choose + " " + Resources.Common.Category };
            var lstCategory = new List<WebCategory>();
            lstCategory.Add(root);
            var allItems = db.WebCategories.AsNoTracking().Where(x => x.CType == ctype).AsEnumerable();
            var roots = allItems.Where(x => x.ParentID == null).OrderBy(x => x.Order).AsEnumerable();
            int level = 0;
            foreach (var item in roots)
            {
                lstCategory.Add(item);
                var subs = allItems.Where(x => x.ParentID == item.ID);
                if (subs.Count() > 0)
                    SubCategories(ref allItems, subs, level + 1, ref lstCategory);
            }
            return lstCategory;
        }

        private void SubCategories(ref IEnumerable<WebCategory> allItems, IEnumerable<WebCategory> subCategories, int level, ref List<WebCategory> lstCategory)
        {
            string levelText = "";
            for (int i = 0; i < level; i++)
                levelText += "- - ";
            foreach (var item in subCategories)
            {
                item.Title = levelText + item.Title;
                lstCategory.Add(item);
                var subSubs = allItems.Where(x => x.ParentID == item.ID);
                if (subSubs.Count() > 0)
                    SubCategories(ref allItems, subSubs, level + 1, ref lstCategory);
            }
        }

        public ActionResult AllProductCategories_Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(ProductCategoriesTree().ToDataSourceResult(request));
        }

        private List<WebCategoryNode> ProductCategoriesTree()
        {
            int ctype = (int)CTypeCategories.Product;
            List<WebCategoryNode> tree = new List<WebCategoryNode>();
            var roots = db.WebCategories.Where(x => x.ParentID == null && x.CType == ctype).OrderBy(x => x.Order).AsEnumerable();
            foreach (var item in roots)
            {
                WebCategoryNode node = new WebCategoryNode();
                node.ID = item.ID;
                node.Title = item.Title;
                node.Description = item.Description;
                node.MetaTitle = item.MetaTitle;
                node.Order = item.Order;
                tree.Add(node);
                var subs = item.SubWebCategories.OrderBy(x => x.Order).AsEnumerable();
                if (subs.Count() > 0)
                    TreeNodes(ref subs, node, ref tree);
            }
            return tree;
        }

        private void TreeNodes(ref IEnumerable<WebCategory> subs, WebCategoryNode parentNode, ref List<WebCategoryNode> tree)
        {
            foreach (var item in subs)
            {
                WebCategoryNode node = new WebCategoryNode();
                node.ID = item.ID;
                node.Title = parentNode.Title + " >> " + item.Title;
                node.Description = item.Description;
                node.MetaTitle = item.MetaTitle;
                node.Order = item.Order;
                tree.Add(node);
                var subSubs = item.SubWebCategories.OrderBy(x => x.Order).AsEnumerable();
                if (subs.Count() > 0)
                    TreeNodes(ref subSubs, node, ref tree);
            }
        }

        public ActionResult ProductCategories_Read([DataSourceRequest] DataSourceRequest request, int id)
        {
            return Json(ProductsCategoriesTree(id).ToDataSourceResult(request));
        }

        private List<WebCategoryNode> ProductsCategoriesTree(int id)
        {

            List<WebCategoryNode> tree = new List<WebCategoryNode>();
            var roots = db.WebContentCategories.Where(x => x.WebContentID == id).OrderBy(x => x.Order).Select(x => x.WebCategory).AsEnumerable();
            foreach (var item in roots)
            {
                WebCategoryNode node = new WebCategoryNode();
                node.ID = item.ID;
                node.Description = item.Description;
                node.MetaTitle = item.MetaTitle;
                node.Order = item.Order;
                string title = item.Title;
                var cat = item;
                if (cat.ParentID != null)
                    GetTitle(ref cat, ref title);
                node.Title = title;
                tree.Add(node);
            }
            return tree;
        }

        private void GetTitle(ref WebCategory cat, ref string title)
        {
            var parentCat = cat.ParentWebCategory;
            title = parentCat.Title + " >> " + title;
            if (parentCat.ParentID != null)
                GetTitle(ref parentCat, ref title);
        }

        [HttpPost]
        public ActionResult ProductCategories_Delete(int categoryID, int productID)
        {
            try
            {
                WebContentCategory item = new WebContentCategory() { WebCategoryID = categoryID, WebContentID = productID };
                db.Entry(item).State = EntityState.Deleted;
                db.SaveChanges();
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public ActionResult ProductCategories_Update(int categoryID, int productID, int order)
        {
            try
            {
                var items = db.WebContentCategories.Where(x => x.WebContentID == productID).OrderBy(x => x.Order).ToList();
                if (items.Count == 0)
                {
                    db.WebContentCategories.Add(new WebContentCategory() { WebCategoryID = categoryID, WebContentID = productID, Order = order });
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                var item = db.WebContentCategories.Where(x => x.WebCategoryID == categoryID && x.WebContentID == productID).SingleOrDefault();
                if (order <= 0 || items.Count == 0)
                    order = 1;
                if (order > items.Count)
                {
                    if (item != null)
                        order = items.Count;
                    else
                        order = items.Count + 1;
                }
                if (item == null)
                {
                    var temp = new WebContentCategory() { WebCategoryID = categoryID, WebContentID = productID, Order = order };
                    items.Insert(order - 1, temp);
                }
                else
                {
                    items.Remove(item);
                    item.Order = order;
                    items.Insert(order - 1, item);
                }
                for (int i = 0; i < order - 1; i++)
                    items[i].Order = i + 1;
                for (int i = order; i < items.Count; i++)
                    items[i].Order = i + 1;
                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var deletes = db.WebContentCategories.Where(x => x.WebContentID == productID).AsEnumerable();
                    db.WebContentCategories.RemoveMany(deletes);
                    db.SaveChanges();
                    db.WebContentCategories.AddRange(items);
                    db.SaveChanges();
                    ts.Complete();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        public ActionResult CategoryMapping(int id)
        {
            var model = db.Set<WebContent>().Find(id);
            if (model == null)
                return HttpNotFound();
            ViewBag.UID = model.UID;
            return View(model);
        }

        public ActionResult ContentImages_Read([DataSourceRequest] DataSourceRequest request, int id)
        {
            return Json(db.ContentImages.Where(x => x.WebContentID == id).OrderBy(x => x.Order).Select(x => new { ID = x.ID, Title = x.Title, Image = x.Image, Order = x.Order, WebContentID = x.WebContentID }).ToList().ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult UploadProductImage(int productID)
        {
            int parse;
            int order = int.TryParse(Request.QueryString["order"], out parse) ? int.Parse(Request.QueryString["order"]) : 1;
            HttpPostedFileBase productImage = Request.Files["file0"];
            if (productImage != null)
            {
                var name = productImage.FileName;
                var newName = Utility.GeneratorFileName(name);
                var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/content/uploads/contentphoto/auto/"));
                if (!dir.Exists) dir.Create();
                var fullpath = "/content/uploads/contentphoto/auto/" + newName;
                productImage.SaveAs(System.Web.HttpContext.Current.Server.MapPath(fullpath));
                try
                {
                    if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(fullpath)))
                    {
                        //var result = ImageTools.ResizeImage(System.Web.HttpContext.Current.Server.MapPath(fullpath), System.Web.HttpContext.Current.Server.MapPath(fullpath), 500, 500, true, 80);
                        db.ContentImages.Add(new ContentImage() { Title = name, Image = fullpath, Order = order, WebContentID = productID });
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        Utility.DeleteFile(fullpath);
                        return Json(new { success = 1, error = "Invalid image format." });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = 2, error = ex.Message });
                }
            }
            return Json(new { success = 3 });
        }
        [HttpPost]
        public ActionResult ContentImage_Update(int contentImageID, int order)
        {
            var item = db.ContentImages.Find(contentImageID);
            if (item == null)
                return Json(new { success = false });
            if (order <= 0)
                order = 1;
            item.Order = order;
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ContentImage_Delete(int contentImageID)
        {
            var item = db.ContentImages.Find(contentImageID);
            if (item == null)
                return Json(new { success = false });
            db.ContentImages.Remove(item);
            db.SaveChanges();
            return Json(new { success = true });
        }
        public ActionResult UpdatePictures(int id)
        {
            var item = db.WebContents.Find(id);
            if (item == null)
                return HttpNotFound();
            ViewBag.UID = item.UID;
            return View(item);
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
            ViewBag.WebModule = webmodule;

            var contents = new List<WebContent>();

            var items = GetListContents(webmodule.ID, contents)
               .Select(x => new WebContent
               {
                   ID = x.ID,
                   Title = x.Title,
                   Image = x.Image,
                   Order = x.Order,
                   WebModuleID = x.WebModuleID

               }).OrderBy(x => x.Order);

            return PartialView(items.ToList());
        }
        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubDetail(string metatitle)
        {
            var content = db.Set<WebContent>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault();

            return PartialView(content);
        }
        [AllowAnonymous]
        public static List<WebModule> GetWebModuleByParentID(int webmoduleid)
        {
            WebContext db = new WebContext();
            var contents = db.WebModules.Where(x => x.ParentID == webmoduleid && x.Status == (int)Status.Public).ToList();
            return contents;
        }
        [AllowAnonymous]
        public ActionResult _OtherNews(int id)
        {
            var item = db.WebContents.Single(x => x.ID == id);
            ViewBag.WebModule = item.WebModule;
            var contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID > id && x.WebModuleID == item.WebModuleID).Take(10).ToList();
            var contents2 = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID < id && x.WebModuleID == item.WebModuleID).OrderByDescending(x => x.CreatedDate).Take(15).ToList();
            contents.AddRange(contents2);
            return PartialView(contents.OrderByDescending(x => x.ID));
        }
        [AllowAnonymous]
        public ActionResult _SlideContent(int id)
        {
            var contentImages = db.ContentImages.Where(x => x.WebContentID == id).ToList();
            return PartialView(contentImages);
        }
        [AllowAnonymous]
        public ActionResult _BigSlide(int id)
        {
            var item = db.WebContents.Find(id);
            ViewBag.Content = item;
            var lstPhoto = db.ContentImages.Where(x => x.WebContentID == id).ToList();
            return PartialView(lstPhoto);
        }

        [AllowAnonymous]
        public ActionResult _PubGallery(int take)
        {
            var webmodules = db.WebModules.Where(x => x.Culture.Equals(ApplicationService.Culture) && x.UID == "gallery").FirstOrDefault();


            var webContents = new List<WebContent>();

            var items = GetListContents(webmodules.ID, webContents)
                .Select(x => new WebContent
                {
                    ID = x.ID,
                    Title = x.Title,
                    Image = x.Image,
                    Order = x.Order,
                    WebModuleID = x.WebModuleID

                }).OrderBy(x => x.Order).Take(take);

            return PartialView(items);
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

    }
}
