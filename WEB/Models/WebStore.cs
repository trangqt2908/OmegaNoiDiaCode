using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using System.Web;
using System.Configuration;
using Kendo.Mvc.UI;
using System.Web.Mvc;
using WebModels;
namespace WEB.Models
{
    
    public partial class WebModuleStore
     {
        public static IEnumerable<WebModuleTree> WebModuleInline()
         {
             var items = HttpContext.Current.Cache.Data("ListWebModuleInlineTree" + ApplicationService.Culture, 60, () =>
             {

                 using (var db = new WebContext())
                 {
                                    
                     var webmodules = from e in db.WebModules.AsNoTracking() 
                                      orderby e.Order 
                                      where 
                                      e.ParentID == null   &&
                                                    (e.Culture == null ||
                                     (!string.IsNullOrEmpty(e.Culture) && e.Culture.Equals(ApplicationService.Culture)))
                                     || (ApplicationService.Culture == null)
                                      select e;
                     List<WebModuleTree> inlineDefault = new List<WebModuleTree> { };
                     foreach (var item in webmodules)
                     {
                         var model = new WebModuleTree();
                         model.Title = item.Title; model.ID = item.ID; model.Order = item.Order; model.ParentID = item.ParentID; model.ContentTypeID = item.ContentTypeID;
                         model.MetaTitle = item.MetaTitle; model.Description = item.Description;
                         inlineDefault.Add(model);
                         var subs = item.SubWebModules.Where(x => x.ShowOnAdmin);
                         if (subs.Any())
                         {
                             WebModuleSubInline(ref inlineDefault, subs.OrderBy(x => x.Order), model);
                         }
                     }
                     return inlineDefault;
                 }
             });

             return items;

         }
        private static void WebModuleSubInline(ref  List<WebModuleTree> inlineDefault, IEnumerable<WebModule> data, WebModuleTree parent)
         {

             foreach (var item in data)
             {
                 var model = new WebModuleTree();
                 model.Title = parent.Title + " >> " + item.Title; model.ID = item.ID; model.Order = item.Order; model.ParentID = item.ParentID; model.ContentTypeID = item.ContentTypeID;
                 model.MetaTitle = item.MetaTitle; model.Description = item.Description; 
                 inlineDefault.Add(model);
                 var subs = item.SubWebModules.Where(
                     x => x.ShowOnAdmin  &&
                           (x.Culture == null ||
                                     (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                                     || (ApplicationService.Culture == null)
                     
                     );
                 if (subs.Any())  
                 {
                     WebModuleSubInline(ref inlineDefault, subs.OrderBy(x => x.Order), model);
                 }
             }
         }



        public static List<TreeViewItemModel> WebModuleTreeLeft()
        {
            UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            List<TreeViewItemModel> inlineDefault = new List<TreeViewItemModel>();
            var items = HttpContext.Current.Cache.Data("WebModuleTreeLeft"+ ApplicationService.Culture, 180, () =>
            {
                using (var db = new WebContext())
                {

                    var webmodules = from e in db.WebModules.AsNoTracking() 
                                     orderby e.Order 
                                     where e.ParentID == null && e.ShowOnAdmin  &&

                                      (e.Culture == null ||
                                     (!string.IsNullOrEmpty(e.Culture) && e.Culture.Equals(ApplicationService.Culture)))
                                     || (ApplicationService.Culture == null)

                                     select e;
                    foreach (var item in webmodules)
                    {
                        var model = new TreeViewItemModel() { Text = item.Title, Id = item.ID.ToString(), Expanded = true, Url = helper.Action("Index", "WebContent", new { id = item.ID, area = "admin" }) };
                        var subs = item.SubWebModules.Where(x => x.ShowOnAdmin);
                        if (subs.Any())
                        {
                            WebModuleTreeLeftSub(ref model, subs.OrderBy(x => x.Order));
                        }
                        inlineDefault.Add(model);
                    }
                    return inlineDefault;
                }
            });

            return items;

        }
        private      static void WebModuleTreeLeftSub(ref TreeViewItemModel treeitem, IEnumerable<WebModule> data)
        {
            UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            foreach (var item in data)
            {
                var model = new TreeViewItemModel() { Text = item.Title, Expanded = true, Id = item.ID.ToString(), Url = helper.Action("Index", "WebContent", new { id = item.ID, area = "admin" }) };
                var subs = item.SubWebModules.Where(
                    
                    x => x.ShowOnAdmin     &&

                                   (x.Culture == null ||
                                     (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                                     || (ApplicationService.Culture == null)

                    );
                if (subs.Any())
                {
                    WebModuleTreeLeftSub(ref model, subs.OrderBy(x => x.Order));
                }    
                treeitem.Items.Add(model);
            }
        }

         
     }
}
