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

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ArticleController
    {

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndexGrid(int id)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);

            ViewBag.WebModule = webmodule;
            var contents = webmodule.WebContents.Where(x => x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).OrderByDescending(x => x.ID); //db.WebContents.Where(x => x.WebModuleID == id).OrderByDescending(x => x.CreatedDate).ToList();
            return PartialView(contents);
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
            //List<WebContent> contents = GetListContents(id, contentslst).Where(x => x.Status.HasValue && x.WebModule.Culture == WEB.Models.ApplicationService.Culture && x.Status.Value.Equals((int)Status.Public)).ToList();
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
            contents = contents.OrderByDescending(x => x.CreatedDate).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * 12).Take(12).ToList());
        }
        private List<WebContent> GetListContents(int webModuleId, List<WebContent> results)
        {
            var webContents = db.WebContents.Where(x => x.WebModuleID == webModuleId);
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
        public ActionResult _PubDetail(string metatitle)
        {
            var content = db.Set<WebContent>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault();

            content.Views = content.Views + 1;

            db.WebContents.Attach(content);
            db.Entry(content).Property(a => a.Views).IsModified = true;
            db.SaveChanges();

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
        public ActionResult _OtherNews(int id, bool? isNew)
        {
            ViewBag.IsNews = isNew;
            var item = db.WebContents.Single(x => x.ID == id);
            ViewBag.WebModule = item.WebModule;

            if (isNew.HasValue && isNew == true)
            {
                var module = db.WebModules.Where(x => x.UID.Equals("blogs") && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

                var contents = db.WebContents.Where(x => x.WebModuleID == module.ID 
                    && x.Status == (int)Status.Public).OrderByDescending(x => x.CreatedDate).Take(10).ToList();
                return PartialView(contents);
            }
            else
            {
                var contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID < id && x.WebModuleID == item.WebModuleID && x.WebModule.Culture.Equals(ApplicationService.Culture)).Take(3).ToList();
                var contents2 = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID > id && x.WebModuleID == item.WebModuleID && x.WebModule.Culture.Equals(ApplicationService.Culture)).Take(3).ToList();
                contents.AddRange(contents2);
                return PartialView(contents.OrderByDescending(x => x.ID));
            }

        }


        [AllowAnonymous]
        public ActionResult _Category()
        {
            var contents = db.WebModules.Where(x => x.Parent.UID.Equals("list-video") && x.Status == (int)Status.Public && x.Culture.Equals(ApplicationService.Culture)).ToList();
            return PartialView(contents.OrderByDescending(x => x.Order));
        }
        [AllowAnonymous]
        public static string GetBGColor(string color)
        {
            var rbga = System.Drawing.ColorTranslator.FromHtml(color);
            var strColor = String.Format("rgba({0}, {1}, {2}, 0.06)", rbga.R, rbga.G, rbga.B);
            return strColor;
            //rgba(249, 233, 241, 0.4)
        }

        [AllowAnonymous]
        public ActionResult _OtherTourByTag(string tag)
        {
            List<WebContent> contents = new List<WebContent>();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var result = tag.Split(new char[] { ',' }).ToList();

                string key = result[0];

                contents = db.WebContents.Where(x => x.Status == (int)Status.Public
                    && x.Title.Contains(key) && x.WebModule.Culture.Equals(ApplicationService.Culture)).Take(6).OrderByDescending(x => x.ID).ToList();
            }

            return PartialView(contents);
        }

        [AllowAnonymous]
        public ActionResult _GetDestinations(int id)
        {
            List<WebModule> webContent = new List<WebModule>();

            if (id == 0)
            {
                var module = db.WebModules.Where(x => x.UID.Equals("destination") && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(module.TopDestination))
                {
                    var result = module.TopDestination.Split(new char[] { ',' }).ToList();

                    foreach (var item in result)
                    {
                        int idItem = 0;
                        int.TryParse(item, out idItem);

                        var obj = db.WebModules.Where(x => x.ID == idItem).FirstOrDefault();

                        if (obj != null)
                        {
                            webContent.Add(obj);
                        }
                    }
                }
            }
            else
            {
                var module = db.WebModules.Where(x => x.ID == id && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(module.TopDestination))
                {
                    var result = module.TopDestination.Split(new char[] { ',' }).ToList();

                    foreach (var item in result)
                    {
                        int idItem = 0;
                        int.TryParse(item, out idItem);

                        var obj = db.WebModules.Where(x => x.ID == idItem).FirstOrDefault();

                        if (obj != null)
                        {
                            webContent.Add(obj);
                        }
                    }
                }
            }
            return PartialView(webContent);
        }
    }
}
