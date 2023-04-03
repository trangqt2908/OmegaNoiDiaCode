using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using WebModels;
using WEB.Models;
using System.Web.Caching;
using Newtonsoft.Json.Linq;
using Kendo.Mvc;
using WebMatrix.WebData;
using Common;
using System.Text.RegularExpressions;

namespace WEB.Areas.ContentType.Controllers
{
    [AdminAuthorize]
    public class HomeController : BaseController
    {
        WebModels.WebContext db = new WebModels.WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {


            ViewBag.ID = id;
            return PartialView();
        }


        #region GetListContents
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

        #endregion

        #region _Highlight (Popular Tours)
        [ChildActionOnly]
        public ActionResult _Highlight(int take)
        {

            return PartialView();
        }

        public ActionResult EditHighlight()
        {
            return View();
        }

        public ActionResult _Highlight_Read()
        {
            var ids = ConfigurationManager.AppSettings.Get("Home_Highlight");
            var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
            var lstContent = new List<WebContent>();
            var jarray = new JArray();
            foreach (var item in data)
            {
                var obj = db.WebContents.Where(x => x.ID == item && x.Status == (int)Status.Public && x.WebModule.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                if (obj != null)
                {
                    lstContent.Add(obj);
                }
            }

            foreach (var c in lstContent)
            {
                var item = c;//.TranslateTo(ApplicationService.Culture);
                var moduletran = item.WebModule;//.TranslateTo(ApplicationService.Culture);
                var jobjectcontent = new JObject() { new JProperty("Lang", ApplicationService.TwoLetterISOLanguage), new JProperty("ID", item.ID), new JProperty("Title", item.Title), new JProperty("MetaTitle", item.MetaTitle), new JProperty("WebModuleID", item.WebModuleID), new JProperty("WebModule_MetaTitle", moduletran.MetaTitle) };
                jarray.Add(jobjectcontent);
            }

            return Json(new { JsonArray = jarray.ToString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Highlight_Read([DataSourceRequest] DataSourceRequest request)
        {
            var ids = ConfigurationManager.AppSettings.Get("Home_Highlight");
            var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
            var result = new List<WebContent>();
            foreach (var item in data)
            {
                var obj = db.WebContents.Where(x => x.ID == item && x.Status == (int)Status.Public && x.WebModule.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                if (obj != null)
                {
                    result.Add(obj);
                }
            }
            return Json(result.Select(x => new { x.ID, x.Title, x.Description, x.Image, x.ModifiedBy, x.ModifiedDate, x.MetaTitle }).ToDataSourceResult(request));
        }
        public ActionResult AllHighlight_Read([DataSourceRequest] DataSourceRequest request)
        {
            var module = db.WebModules.Where(x => x.UID == "all-tour"
                && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();


            var webContents = new List<WebContent>();

            var items = GetListContents(module.ID, webContents).Where(x => x.WebModule.Culture.Equals(ApplicationService.Culture)).Select(x => new { x.ID, x.Title, x.Description, x.Image, x.ModifiedBy, x.ModifiedDate });
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }

            return Json(items.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult Highlight_Update(int id, int order)
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                var ids = ConfigurationManager.AppSettings.Get("Home_Highlight");
                var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
                data.Remove(id);
                if (order < 1) order = 1;
                else if (order > data.Count + 1) order = data.Count + 1;
                data.Insert(order - 1, id);
                data = data.Distinct().ToList();
                config.AppSettings.Settings.Remove("Home_Highlight");
                config.AppSettings.Settings.Add("Home_Highlight", string.Join(",", data));
                config.Save();
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = true,
                    error = ex.Message
                });
            }
        }
        [HttpPost]
        public ActionResult Highlight_Delete(int id)
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                var ids = ConfigurationManager.AppSettings.Get("Home_Highlight");
                var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
                data.Remove(id);
                data = data.Distinct().ToList();
                config.AppSettings.Settings.Remove("Home_Highlight");
                config.AppSettings.Settings.Add("Home_Highlight", string.Join(",", data));
                config.Save();
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = true,
                    error = ex.Message
                });
            }
        }

        [AllowAnonymous]
        public ActionResult _PubHighlight(int take)
        {
            var ids = ConfigurationManager.AppSettings.Get("Home_Highlight");
            var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
            var result = new List<WebContent>();
            foreach (var item in data)
            {
                var obj = db.WebContents.Where(x => x.ID == item && x.WebModule.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                if (obj != null)
                {
                    result.Add(obj);
                }
            }


            return PartialView(result.Take(take));
        }
        #endregion _Highlight

        #region _SpecialOffers (Popular Destinations)
        [ChildActionOnly]
        public ActionResult _SpecialOffers(int take)
        {
            return PartialView();
        }

        public ActionResult EditSpecialOffers()
        {
            return View();
        }
        public ActionResult _SpecialOffers_Read()
        {
            var ids = ConfigurationManager.AppSettings.Get("Home_SpecialOffers");
            var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
            var lstContent = new List<WebContent>();
            var jarray = new JArray();
            foreach (var item in data)
            {
                var obj = db.WebContents.Where(x => x.ID == item && x.Status == (int)Status.Public && x.WebModule.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                if (obj != null)
                {
                    lstContent.Add(obj);
                }
            }

            foreach (var c in lstContent)
            {
                var item = c;//.TranslateTo(ApplicationService.Culture);
                var moduletran = item.WebModule;//.TranslateTo(ApplicationService.Culture);
                var jobjectcontent = new JObject() { new JProperty("Lang", ApplicationService.TwoLetterISOLanguage), new JProperty("ID", item.ID), new JProperty("Title", item.Title), new JProperty("MetaTitle", item.MetaTitle), new JProperty("WebModuleID", item.WebModuleID), new JProperty("WebModule_MetaTitle", moduletran.MetaTitle) };
                jarray.Add(jobjectcontent);
            }

            return Json(new { JsonArray = jarray.ToString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SpecialOffers_Read([DataSourceRequest] DataSourceRequest request)
        {
            var ids = ConfigurationManager.AppSettings.Get("Home_SpecialOffers");
            var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
            var result = new List<WebContent>();
            foreach (var item in data)
            {
                var obj = db.WebContents.Where(x => x.ID == item && x.Status == (int)Status.Public && x.WebModule.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                if (obj != null)
                {
                    result.Add(obj);
                }
            }
            return Json(result.Select(x => new { x.ID, x.Title, x.Description, x.Image, x.ModifiedBy, x.ModifiedDate, x.MetaTitle }).ToDataSourceResult(request));
        }
        public ActionResult AllSpecialOffers_Read([DataSourceRequest] DataSourceRequest request)
        {
            var module = db.WebModules.Where(x => x.UID == "destinations"
              && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();


            var webContents = new List<WebContent>();

            var items = GetListContents(module.ID, webContents).Where(x => x.WebModule.Culture.Equals(ApplicationService.Culture)).Select(x => new { x.ID, x.Title, x.Description, x.Image, x.ModifiedBy, x.ModifiedDate });
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }

            return Json(items.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult SpecialOffers_Update(int id, int order)
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                var ids = ConfigurationManager.AppSettings.Get("Home_SpecialOffers");
                var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
                data.Remove(id);
                if (order < 1) order = 1;
                else if (order > data.Count + 1) order = data.Count + 1;
                data.Insert(order - 1, id);
                data = data.Distinct().ToList();
                config.AppSettings.Settings.Remove("Home_SpecialOffers");
                config.AppSettings.Settings.Add("Home_SpecialOffers", string.Join(",", data));
                config.Save();
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = true,
                    error = ex.Message
                });
            }
        }
        [HttpPost]
        public ActionResult SpecialOffers_Delete(int id)
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                var ids = ConfigurationManager.AppSettings.Get("Home_SpecialOffers");
                var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
                data.Remove(id);
                data = data.Distinct().ToList();
                config.AppSettings.Settings.Remove("Home_SpecialOffers");
                config.AppSettings.Settings.Add("Home_SpecialOffers", string.Join(",", data));
                config.Save();
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = true,
                    error = ex.Message
                });
            }
        }

        #endregion _SpecialOffers

        #region _Publish
        [AllowAnonymous]
        public JsonResult GetDestinations(int? id)
        {
            if (id == null)
            {
                var countries = db.WebModules.Where(x => x.Parent.UID.Equals("destination") && x.Status == (int)Status.Public
                    && x.Culture == ApplicationService.Culture).Select(x => new { x.ID, x.Title });
                //(from c in this.db.WebModules where c.Parent.UID.Equals("destination") select new { c.ID, c.Title });
                return Json(countries, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var countries = db.WebModules.Where(x => x.ParentID == id && x.Status == (int)Status.Public).Select(x => new { x.ID, x.Title });
                return Json(countries, JsonRequestBehavior.AllowGet);
            }
        }

        private List<WebModule> GetAllModule()
        {
            var webmodule = db.WebModules.Where(x => x.UID.Equals("destinations")
               && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

            List<WebModule> contents = new List<WebModule>();

            contents = db.WebModules.Where(x => x.ParentID == webmodule.ID && x.Status == (int)Status.Public).ToList();

            return contents;
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndex(string metatitle)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Where(x => x.MetaTitle.Equals(metatitle)).FirstOrDefault(); ;
            ViewBag.WebModule = webmodule;

            return PartialView();
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubSlide()
        {

            IList<WebSlide> slides = this.db.WebSlides.Where(m => m.Status == (int)Status.Public
                && m.Culture.Equals(ApplicationService.Culture)).OrderBy(m => m.Order).ToList();

            return PartialView(slides);
        }

        [AllowAnonymous]
        public ActionResult _GetLetter()
        {
            return PartialView();
        }
        

        [AllowAnonymous]
        [HttpPost]
        public JsonResult _GetLetter(string email)
        {
            try
            {
                if (!Regex.IsMatch(email, @"[\w\d._%+-]+@[\w\d.-]+\.\w{2,4}"))
                {
                    throw new Exception(WebResources.EmailNotValid);
                }

                if (!this.db.SubscribeNotices.Where(s => s.Email == email).Any())
                {
                    this.db.SubscribeNotices.Add(new SubscribeNotice()
                    {
                        CreatedBy = WebSecurity.CurrentUserName,
                        ModifiedBy = WebSecurity.CurrentUserName,
                        Email = email
                    });
                }

                this.db.SaveChanges();

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.Message });
            }
        }

        [AllowAnonymous]
        public ActionResult _PubDestinations()
        {
            var webmodule = db.WebModules.Where(x => x.UID.Equals("destinations")
              && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();

            var ids = ConfigurationManager.AppSettings.Get("Home_SpecialOffers");
            var data = ids.Split(',').ToList().ConvertStringToInt().ToList();
            var result = new List<WebContent>();
            foreach (var item in data)
            {
                var obj = db.WebContents.Where(x => x.ID == item && x.WebModule.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
                if (obj != null)
                {
                    result.Add(obj);
                }
            }

            ViewBag.Webmodule = webmodule;
            return PartialView(result.OrderBy(x => x.Order).Take(7));
        }


        [AllowAnonymous]
        public ActionResult _TopBlogTravel()
        {
            List<WebContent> webContent = new List<WebContent>();

            var module = db.WebModules.Where(x => x.UID.Equals("blog") && x.Culture.Equals(ApplicationService.Culture)).FirstOrDefault();
            ViewBag.WebModule = module;
            var listTopBlog = db.WebContents.Where(x => x.WebModuleID == module.ID).ToList();
            if(listTopBlog!= null && listTopBlog.Any())
            {
                return PartialView(listTopBlog.OrderByDescending(x => x.CreatedDate).Take(3));
            }
            if (!string.IsNullOrWhiteSpace(module.PositionActive))
            {
                var result = module.PositionActive.Split(new char[] { ',' }).ToList();

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
            return PartialView(webContent);
        }

        [AllowAnonymous]
        public ActionResult _FrmSearch()
        {
            ViewBag.CountryList = GetAllModule();

            var typeOfTour = db.WebModules.Where(x => x.Parent.UID.Equals("all-tour") && x.Parent.Culture == ApplicationService.Culture).ToList();
            ViewBag.TypeOfTour = typeOfTour;
            
            return PartialView();
        }

        [AllowAnonymous]
        public ActionResult _FrmSearchDetail()
        {
            ViewBag.CountryList = GetAllModule();

            var typeOfTour = db.WebModules.Where(x => x.Parent.UID.Equals("all-tour") && x.Parent.Culture == ApplicationService.Culture).ToList();
            ViewBag.TypeOfTour = typeOfTour;

            return PartialView();
        }

        [AllowAnonymous]
        public ActionResult _LastestBlog()
        {

            return PartialView();
        }
        #endregion _Publish

    }
}