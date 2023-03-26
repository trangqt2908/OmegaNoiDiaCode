
using Common;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Filters;
using WebMatrix.WebData;
using WebModels; 

namespace WEB.Areas.Admin.Controllers
{
    [AdminAuthorize] 
    public class AccountController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/Account/

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.Languages = this.Language;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }
            ViewBag.Languages = this.Language;
            ModelState.AddModelError("", AccountResources.LoginIncorrect);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Denied()
        {
            return View();
        }
        
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult UserProfile()
        {
            var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.Avatar = user.Avatar == null ? "/Images/no_avatar_60x60.jpg" : user.Avatar;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserProfile([Bind(Exclude = "")]UserProfile model, HttpPostedFileBase imagefile)
        {
            if (ModelState.IsValid)
            {
                if (imagefile != null)
                {
                    var name = imagefile.FileName;
                    string extension = Path.GetExtension(name);
                    var newName = model.UserName + extension;
                    var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uploads/avatars/"));
                    if (!dir.Exists) dir.Create();
                    var uri = "/content/uploads/avatars/" + newName;
                    imagefile.SaveAs(HttpContext.Server.MapPath(uri));
                    try
                    {
                        if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                        {
                            var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 240, 240, true, 80);
                            model.Avatar = uri;
                        }
                        else
                        {
                            Utility.DeleteFile(uri);
                        }
                    }
                    catch (Exception)
                    { }
                }

                db.UserProfiles.Attach(model);
                db.Entry(model).Property(a => a.UserName).IsModified = false;
                db.Entry(model).Property(a => a.FullName).IsModified = true;
                db.Entry(model).Property(a => a.Email).IsModified = true;
                db.Entry(model).Property(a => a.Mobile).IsModified = true;
                db.Entry(model).Property(a => a.Avatar).IsModified = true;
                db.SaveChanges();
            }
            ViewBag.Avatar = model.Avatar == null ? "/Images/no_avatar_60x60.jpg" : model.Avatar;
            ViewBag.Alert = "<div class='alert alert-info'>Thông tin cá nhân của bạn đã được thay đổi thành công !</div>";
            return View();
        }

        public ActionResult _Info()
        {
            var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.Displayname = user.FullName == null ? "Noname" : user.FullName;
            ViewBag.Avatar = user.Avatar == null ? "/Images/no_avatar.jpg" : user.Avatar;
            return PartialView();
        }

        public PartialViewResult _ChangePassword()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult _ChangePassword(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));

            if (!hasLocalAccount || !ModelState.IsValid)
            {
                return Json(new { Success = false, Error = "The current password is incorrect or the new password is invalid." });
            }

            bool changePasswordSucceeded;

            try
            {
                changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            return Json(new { Success = changePasswordSucceeded, Error = changePasswordSucceeded ? "" : "The current password is incorrect or the new password is invalid." });
        }

    }
}
