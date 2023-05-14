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
using System.Data.Entity.SqlServer;
namespace WEB.Controllers
{
    public class HomeController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Index(int? id, string uid, string lang, string metatitle, int? page)
        {
            var home = new List<string> { "home", "index", "home.html", "index.html", "trangchu", "trang-chu", "trangchu.html", "trang-chu.html" };


            if (id.HasValue)
            {
                var module = db.Set<WebModule>().Find(id);
                TempData["WebModule"] = module;
                ViewBag.Page = page;
                return View(module);

            }
            else if (!string.IsNullOrWhiteSpace(metatitle))
            {
                var module = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault();
                TempData["WebModule"] = module;
                ViewBag.Page = page;
                return View(module);
            }
            else if (!string.IsNullOrEmpty(uid))
            {

                var module = (from x in db.WebModules
                              where
                                  (x.UID.ToLower().Equals(uid.ToLower()))
                                                              &&
                                  ((x.Culture == null ||
                                  (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                                  || (ApplicationService.Culture == null))

                              select x).AsNoTracking().FirstOrDefault();
                return View(module);
            }
            else
            {

                var module = (from x in db.WebModules
                              where
                                  (x.UID == null || home.Contains(x.ContentType.ID.ToLower()))
                                                                      &&
                                  ((x.Culture == null ||
                                  (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                                  || (ApplicationService.Culture == null))

                              select x).AsNoTracking().FirstOrDefault();
                return View(module);
            }

        }
        public ActionResult Detail(string metatitle, string m_metatitle)
        {
            //ViewBag.ID = id;
            var module = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(m_metatitle)).FirstOrDefault();
            return View(module);
        }
        [OutputCache(Duration = 120, VaryByCustom = "culture")]
        public ActionResult SiteMap()
        {
            ViewBag.Language = Language;
            var webmodules = from e in db.WebModules
                             where (e.ParentID == null)
                             orderby e.Order
                             select e;
            return View(webmodules);
        }
        public ActionResult Search(int? page)
        {
            WebContext db = new WebContext();

            //var des = Request.QueryString["country"];
            var leng = Request.QueryString["leng"];
            var type = Request.QueryString["type"];
            var keyword = Request.QueryString["keyword"];

            var sKeyword = "";
            if (keyword != null) sKeyword = keyword.ToString().ToLower().Trim();

            int sLeng = 0;
            int.TryParse(leng, out sLeng);

            int sDes = 0;
            int.TryParse(type, out sDes);

            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = "Home",
                action = "Search",
                area = "",
                keyword = keyword
            });

            List<int> tourTrongNuoc = new List<int>();
            List<int> tourNuocNgoai = new List<int>();
            List<int> comboTour = new List<int>();

            var modules = db.WebModules.Where(x => (x.UID.Equals("tour-trong-nuoc")
            || x.UID.Equals("tour-nuoc-ngoai") || x.UID.Equals("combo"))
            && x.Status == (int)Status.Public).ToList();

            foreach (var item in modules)
            {
                List<WebModule> allModules = new List<WebModule>();
                if (item.UID == "tour-trong-nuoc")
                {
                    GetListModule(item.ID, allModules);
                    tourTrongNuoc = allModules.Select(x => x.ID).Distinct().ToList();
                }
                else if (item.UID == "tour-nuoc-ngoai")
                {
                    GetListModule(item.ID, allModules);
                    tourNuocNgoai = allModules.Select(x => x.ID).Distinct().ToList();
                }
                else
                {
                    GetListModule(item.ID, allModules);
                    comboTour = allModules.Select(x => x.ID).Distinct().ToList();
                }
            }

            var contentsearch = new List<WebContent>();          

            if (sDes > 0)
            {
                var contentTour = db.WebContents.Where(x => x.WebModule.ContentTypeID == "Tour");

                foreach (var tour in contentTour)
                {
                    var cityIdsOfTour = tour.ProductInfo.Destination.Split(',').Select(x => int.Parse(x));
                    if (cityIdsOfTour.Any(x=>x == sDes))
                    {
                        contentsearch.Add(tour);
                    }
                }
            }
            else
            {
                contentsearch = db.WebContents.Where(x => x.WebModule.ContentTypeID == "Tour").ToList();
            }


            if (sLeng > 0)
            {
                if (sLeng == 1)
                {
                    contentsearch = contentsearch.Where(x => x.ProductInfo.Duration == sLeng).ToList();
                }
                else if (sLeng == 5)
                {
                    contentsearch = contentsearch.Where(x => x.ProductInfo.Duration >= 2 && x.ProductInfo.Duration <= 5).ToList();
                }
                else if (sLeng == 10)
                {
                    contentsearch = contentsearch.Where(x => x.ProductInfo.Duration >= 6 && x.ProductInfo.Duration <= 10).ToList();
                }
                else if (sLeng == 11)
                {
                    contentsearch = contentsearch.Where(x => x.ProductInfo.Duration >= 11).ToList();
                }
                else
                {
                    contentsearch = contentsearch.Where(x => x.ProductInfo.Duration == sLeng).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(sKeyword))
            {
                contentsearch = contentsearch.Where(x => x.Title.ToLower().Contains(sKeyword)).ToList();
            }

            ViewBag.lstTourTrongNuocsIds = tourTrongNuoc;
            ViewBag.lstTourNuocNgoaiIds = tourNuocNgoai;
            ViewBag.lstComboTourIds = comboTour;

            ViewBag.isViewTourTrongNuoc = contentsearch.Any(x => tourTrongNuoc.Any(d => d == x.WebModuleID));
            ViewBag.isViewTourNuocNgoai = contentsearch.Any(x => tourNuocNgoai.Any(d => d == x.WebModuleID));
            ViewBag.isViewComboTour = contentsearch.Any(x => comboTour.Any(d => d == x.WebModuleID));

            return View(contentsearch.ToList());
        }
        private List<WebModule> GetListModule(int webModuleId, List<WebModule> results)
        {
            var webModule = db.WebModules.Where(x => x.ID == webModuleId).FirstOrDefault();
            results.Add(webModule);

            var webModules = db.WebModules.Where(x => x.ParentID == webModuleId);
            results.AddRange(webModules);

            foreach (var childWebModule in webModules)
            {
                GetListModule(childWebModule.ID, results);
            }

            return results;
        }
        private List<WebModule> GetAllModuleDestinations()
        {
            var webmodule = db.WebModules.Where(x => x.UID.Equals("destinations")
               && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

            List<WebModule> contents = new List<WebModule>();

            contents = db.WebModules.Where(x => x.ParentID == webmodule.ID && x.Status == (int)Status.Public).ToList();

            return contents;
        }
        //-----------------------------------------
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
        public ActionResult _Language()
        {
            return PartialView(this.Language);
        }

        [ChildActionOnly]
        public ActionResult _SiteMap(string metatitle, string m_metatitle, int idnew = 0)
        {
            ViewBag.title = "";

            metatitle = m_metatitle ?? metatitle;

            if (string.IsNullOrWhiteSpace(metatitle))
            {
                return PartialView(null);
            }

            WebModule module = new WebModule();

            var modulemetatitle = this.db.WebModules.Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault();
            if (module != null)
            {
                module = modulemetatitle;
            }

            Stack<WebModule> modules = new Stack<WebModule>();

            do
            {
                modules.Push(module);
                module = module.Parent;
            }
            while (module != null);

            if (idnew != 0)
            {
                var content = db.WebContents.Find(idnew);
                ViewBag.title = "/ " + content.Title;
            }

            return PartialView(modules);
        }
        [ChildActionOnly]
        public ActionResult _SiteMapDetail(int id = 0, int m_id = 0, int idnew = 0)
        {
            ViewBag.title = "";
            id = m_id > 0 ? m_id : id;

            if (id == 0)
            {
                return PartialView(null);
            }

            WebModule module = this.db.WebModules.Find(id);
            Stack<WebModule> modules = new Stack<WebModule>();

            do
            {
                modules.Push(module);

                module = module.Parent;
            }
            while (module != null);

            if (idnew != 0)
            {
                var content = db.WebContents.Find(idnew);
                ViewBag.title = "/ " + content.Title;
            }

            return PartialView(modules);
        }

        [ChildActionOnly]
        public ActionResult _GetTagByContentID(int id)
        {
            var content = db.WebContents.Find(id);

            List<string> words = new List<string>();

            if (!string.IsNullOrWhiteSpace(content.Tag))
            {
                words = content.Tag.Split(',').ToList();
            }

            return PartialView(words);
        }

        [ChildActionOnly]
        public static string getconfig(string key)
        {
            WebContext db = new WebContext();
            string value = "";

            var temp = db.WebConfigs.Where(x => x.Key == key).FirstOrDefault();
            if (temp != null)
            {
                value = temp.Value;
            }

            return value;
        }
    }
}
