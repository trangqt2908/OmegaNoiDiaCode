using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class NavigationController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Navigation/
        public ActionResult Index(string key)
        {

            var temp = db.ModuleNavigations.AsNoTracking().Where(
               x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                     (x.WebModule.Culture == null ||
                            (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                            || (ApplicationService.Culture == null)

               ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return View(webmodules);
        }

        [ChildActionOnly]
        //  [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Main(string key, int? moduleID)
        {
            var listParentID = new List<int>();
            if (moduleID != null)
            {
                listParentID = GetListParentID(moduleID ?? 0).ToList();
                listParentID.Add(moduleID ?? 0);
            }
            ViewBag.ListModuleActive = listParentID;
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);

        }
        public IEnumerable<int> GetListParentID(int? id)
        {
            using (var context = new WebContext())
            {
                List<int> listParentID = new List<int>();
                var WebModule = context.WebModules.Where(x => x.ID == id).FirstOrDefault();
                while (WebModule.ParentID != null)
                {
                    listParentID.Add(WebModule.ParentID.Value);
                    WebModule = context.WebModules.Where(x => x.ID == WebModule.ParentID).FirstOrDefault();
                }

                return listParentID;
            }
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Menu_Footer(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }

        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxTourType(string key, int currentId)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;


            ViewBag.currentModule = db.WebModules.Where(x => x.UID == "list-tour" && x.Culture == ApplicationService.Culture).FirstOrDefault();

            ViewBag.currentModuleId = currentId;

            return PartialView(webmodules);
        }

        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxGalleryType()
        {
            var webmodules = db.WebModules.Where(x => x.Culture.Equals(ApplicationService.Culture) && x.UID == "gallery").FirstOrDefault();

            var lstWebmodule = db.WebModules.Where(x => x.ParentID == webmodules.ID).OrderBy(x => x.Order);

            return PartialView(lstWebmodule);
        }

        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxTourTypeList(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);

            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;
            return PartialView(webmodules);
        }
        


        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _SlideCat(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [AllowAnonymous]
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _ViewListDestination(string lstDestination)
        {
            try
            {
                List<WebContent> webmodules = new List<WebContent>();

                if (!string.IsNullOrWhiteSpace(lstDestination))
                {
                    var result = lstDestination.Split(new char[] { ',' }).ToList();


                    foreach (var item in result)
                    {
                        int id = 0;

                        int.TryParse(item, out id);

                        var obj = db.WebContents.Where(x => x.ID == id).FirstOrDefault();

                        if (obj != null)
                        {
                            webmodules.Add(obj);
                        }

                    }
                }
                return PartialView(webmodules);
            }
            catch (Exception)
            {

                throw;
            }

        }
        public static WebContent GetTopNews(int webmoduleid)
        {
            WebContext db = new WebContext();
            var model = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).OrderByDescending(x => x.CreatedDate);
            if (model.Count() > 0)
            {
                return model.FirstOrDefault();
            }
            else
            {
                return null;
            }

        }
        public static List<WebContent> GetListTopNews(int webmoduleid, int take)
        {
            WebContext db = new WebContext();
            var model = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).OrderByDescending(x => x.CreatedDate);
            return model.Take(take).ToList();
        }
        public static List<WebContent> GetNewsByWebmoduleID(int webmoduleid, int take)
        {
            WebContext db = new WebContext();
            var contents = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).OrderByDescending(x => x.CreatedDate).Take(take);
            return contents.Take(take).ToList();

        }
        public static List<WebContent> GetAllContent(int webmoduleid, int take)
        {
            WebContext db = new WebContext();
            var contents = new List<WebContent>();
            contents = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).ToList();
            var webmodule = db.WebModules.Where(x => x.ParentID == webmoduleid && x.Status == (int)Status.Public);
            if (webmodule.Any())
            {
                foreach (var item in webmodule)
                {
                    var lstnews = db.WebContents.Where(x => x.WebModuleID == item.ID && x.Status == (int)Status.Public);
                    if (lstnews.Any())
                    {
                        contents.AddRange(lstnews);
                    }
                }
            }
            return contents.OrderByDescending(x => x.CreatedDate).Take(take).ToList();
        }



        public static WebModule GetDestinationTitle(int destinationId)
        {
            WebContext db = new WebContext();

            var module = db.WebModules.Where(x => x.ID == destinationId).FirstOrDefault();

            return module;
        }


        public static List<WebModule> GetCityByCountry(int id)
        {
            WebContext db = new WebContext();
            List<WebModule> module = new List<WebModule>();
            module = db.WebModules.Where(x => x.ParentID == id).ToList();

            return module;
        }


        public static List<WebModule> GetAllModuleByParenID(int webmoduleid)
        {
            WebContext db = new WebContext();
            var modeul = new List<WebModule>();
            modeul = db.WebModules.Where(x => x.ParentID == webmoduleid && x.Status == (int)Status.Public).ToList();

            return modeul.OrderByDescending(x => x.Order).ToList();
        }


        public static string SubString(int lenght, string str)
        {
            if (str.Length > lenght)
            {
                str = str.Substring(0, lenght) + "...";
                return str;
            }
            else
            {
                return str;
            }

        }
        public static string GetTotalTour(int destinationId)
        {
            WebContext db = new WebContext();
            string id = destinationId.ToString();
            decimal countTour = db.WebContents.Where(x => x.ProductInfo.Destination.Contains(id)).Count();

            countTour = Math.Round(countTour % 2);

            return countTour.ToString();
        }


        public static List<WebModule> GetAllModule(int webmoduleid)
        {
            WebContext db = new WebContext();
            var model = new List<WebModule>();
            model = db.WebModules.Where(x => x.ParentID == webmoduleid && x.Status == (int)Status.Public).ToList();

            return model;
        }
        public static List<WebContent> GetContenByListID(string lstID)
        {
            WebContext db = new WebContext();
            List<WebContent> webContent = new List<WebContent>();

            if (!string.IsNullOrWhiteSpace(lstID))
            {
                var result = lstID.Split(new char[] { ',' }).ToList();

                foreach (var item in result)
                {
                    int id = 0;

                    int.TryParse(item, out id);

                    var obj = db.WebContents.Where(x => x.ID == id).FirstOrDefault();

                    if (obj != null)
                    {
                        webContent.Add(obj);
                    }
                }
            }
            return webContent;
        }
    }
}