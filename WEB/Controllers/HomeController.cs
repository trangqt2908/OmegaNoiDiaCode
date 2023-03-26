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
            else if(!string.IsNullOrWhiteSpace(metatitle))
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

            var des = Request.QueryString["country"];
            var leng = Request.QueryString["leng"];
            var type = Request.QueryString["type"];
            //var keyword = Request.QueryString["keyword"];
            var tag = Request.QueryString["tag"];

            //var sKeyword = "";
            //if (keyword != null) sKeyword = keyword.ToString().ToLower();

            var sTag = "";
            if (tag != null) sTag = tag.ToString().ToLower();

            var sDes = 0;
            int.TryParse(des, out sDes);

            int sLeng = 0;
            int.TryParse(leng, out sLeng);

            int sType = 0;
            int.TryParse(type, out sType);

            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = "Home",
                action = "Search",
                area = "",
                keyword = sDes
            });

            var module = db.WebModules.FirstOrDefault(x => x.UID.Equals("all-tour")
                && x.Culture.Equals(ApplicationService.Culture)
                && x.Status == (int)Status.Public);

            List<WebContent> contentsearch = new List<WebContent>();

            if (sDes > 0)
            {
                List<WebContent> contentTour = new List<WebContent>();
                GetListContents(module.ID, contentTour);

                var countryId = sDes;
                var cityIdsOfCountry = db.WebModules.Where(x => x.ParentID == countryId).Select(y => y.ID).ToList();
                foreach (var tour in contentTour)
                {
                    var cityIdsOfTour = tour.ProductInfo.Destination.Split(',').Select(x => int.Parse(x));
                    if (cityIdsOfTour.Intersect(cityIdsOfCountry).Count() == 0)
                    {
                        continue;
                    }

                    contentsearch.Add(tour);
                }
            }
            else
            {
                GetListContents(module.ID, contentsearch);
            }

            if (sType > 0)
            {
                contentsearch = contentsearch.Where(x => x.WebModuleID == sType).ToList();
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


            if (!string.IsNullOrWhiteSpace(sTag))
            {
                contentsearch = contentsearch.Where(x => x.Title.Contains(sTag) || (x.Tag != null && x.Tag.Contains(sTag))).ToList();
            }

            ViewBag.Type = sTag;

            ViewBag.des = des;
            ViewBag.leng = leng;
            ViewBag.type1 = type;


            ViewBag.CountryList = GetAllModule();
            var typeOfTour = db.WebModules.Where(x => x.Parent.UID.Equals("all-tour") && x.Parent.Culture == ApplicationService.Culture).ToList();
            ViewBag.TypeOfTour = typeOfTour;

            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contentsearch.Count();
            ViewBag.CurrentPage = ipage;
            return View(contentsearch.Skip((ipage - 1) * 12).Take(12).OrderByDescending(x => x.CreatedDate).ToList());
        }
        private List<WebModule> GetAllModule()
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
        public ActionResult _SiteMap(int id = 0, int m_id = 0, int idnew = 0)
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
