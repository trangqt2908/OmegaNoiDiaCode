using Common;
using System;
using System.Collections.Generic;
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
using System.Data.Entity;
namespace WEB.Areas.Admin.Controllers
{
    [AdminAuthorize] 
    public class RoleController : BaseController 
    {
        


        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }                 
        public ActionResult Index()
        {
            
            return View();
        }
        public ActionResult Roles_Read([DataSourceRequest] DataSourceRequest request)
        {
            var role = db.WebRoles.AsNoTracking().Select(x => new { x.RoleId,x.RoleName,x.Title,x.Description});
             return Json(role.ToDataSourceResult(request));
        }

        public JsonResult GetRoles(string text)
        {

            var roles = from x in db.WebRoles.AsNoTracking() select x;
                if (!string.IsNullOrEmpty(text))
                {
                    roles = roles.Where(p => p.RoleName.Contains(text));
                }

                return Json(roles, JsonRequestBehavior.AllowGet);
            
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "RoleId")]WebRole model)
        {
            if (ModelState.IsValid)
            {


                var temp = (from p in db.Set<WebRole>().AsNoTracking()
                                where p.RoleName.Equals(model.RoleName,StringComparison.OrdinalIgnoreCase)
                                select p).FirstOrDefault();
                    if (temp != null)
                    {
                        ModelState.AddModelError("",  AccountResources.RoleExists);
                        return View(model);
                    }
                    else
                    {
                        db.Set<WebRole>().Add(model);
                        db.SaveChanges();

                        (new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Add.ToString(),WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName)).Insert();

                        ViewBag.StartupScript = "create_success();";
                        return View(model);
                    }
               
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult Edit(int id)
        {

            var role = db.Set<WebRole>().Find(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                ViewBag.cRoleName = role.RoleName;
                return View("Edit", role);
             
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebRole model, string cRoleName)
        {
            ViewBag.cRoleName = cRoleName;
            if (ModelState.IsValid)
            {
                
                    try
                    {
                        var temp = (from p in db.Set<WebRole>().AsNoTracking()
                                    where p.RoleName.Equals(model.RoleName, StringComparison.OrdinalIgnoreCase) && !p.RoleName.Equals(cRoleName, StringComparison.OrdinalIgnoreCase)
                                    select p).FirstOrDefault();
                        if (temp != null)
                        {
                            ModelState.AddModelError("", AccountResources.RoleExists);
                            return View(model);
                        }
                        else
                        {
                            var roleupdate = new WebRole { RoleId = model.RoleId };
                            db.WebRoles.Attach(model);
                            db.Entry(model).Property(a => a.RoleId).IsModified = true;
                            db.Entry(model).Property(a => a.Description).IsModified = true;
                            db.Entry(model).Property(a => a.RoleName).IsModified = true;
                            db.Entry(model).Property(a => a.Title).IsModified = true;
                            db.SaveChanges();

                            (new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Edit.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName)).Insert();

                            ViewBag.StartupScript = "edit_success();";
                            return View(model);
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

            var role = db.Set<WebRole>().Find(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", role);
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebRole model)
        {
           
            
                 
                try
                {
                    var log = (new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Delete.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName));
                    var role =db.WebRoles.Attach(model);
                    db.Set<WebRole>().Remove(role);
                    db.SaveChanges();
                    log.Insert();
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
            var lstRoleId = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstRoleId.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }

            var temp = from p in db.Set<WebRole>().AsNoTracking()
                           where lstRoleId.Contains(p.RoleId)
                           select p;

                return View(temp.ToList());
             
        }
        [HttpPost] 
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebRole> model)
        {
            
                var temp = new List<WebRole>();
                foreach (var item in model)
                {
                    try
                    { 
                        db.Entry(item).State = EntityState.Deleted; 
                        db.SaveChanges();
                        var log = new WebModels.AccessLog("Entity: Role, Item: " + item.RoleId + ":" + item.RoleName, WebModels.AccessLogActions.Delete.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName);
                         
                    }
                    catch (Exception)
                    {
                        db.Entry(item).State = EntityState.Unchanged; 
                        temp.Add(item); }
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
                    return View(model);
                }
            
        }
    }
}
