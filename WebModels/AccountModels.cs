using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace WebModels
{
     

    [Table("webpages_Roles")]
    public class WebRole
    {
        
        private string _roleName = string.Empty;
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }
        [Required( ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRoleName")]
        [Display(ResourceType = typeof(AccountResources), Name = "RoleName")]
        public string RoleName { 
             get { return _roleName.Trim(); }
             set { _roleName = value.Trim(); } 
        }
        [Display(ResourceType = typeof(AccountResources), Name = "RoleDescription")]
        public string Description { get; set; }
        [Display(ResourceType = typeof(AccountResources), Name = "Title")]
        public string Title { get; set; }
    }

    [Table("UserProfile")]
    public partial class UserProfile
    {
        public UserProfile()
        {
            this.AccessAdminSites = new HashSet<AccessAdminSite>();
            this.AccessWebModules = new HashSet<AccessWebModule>();
        }
        private string _userName = string.Empty;
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9]{2,255}$", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "UserNameNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName {
            get { return _userName.Trim(); }
            set { _userName = value.Trim(); } 
        }
        [Display(ResourceType = typeof(AccountResources), Name = "FulllName")]
        public string FullName { get; set; }
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}",ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName="EmailNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "Email")]
        public string Email { get; set; }
        [Display(ResourceType = typeof(AccountResources), Name = "Mobile")]
        public string Mobile { get; set; }
        [Display(ResourceType = typeof(AccountResources), Name = "Avatar")]
        public string Avatar { get; set; }

        public virtual ICollection<AccessAdminSite> AccessAdminSites { get; set; }

        public virtual ICollection<AccessWebModule> AccessWebModules { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessageResourceType=typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPassword")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        private string _userName = string.Empty;
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUserName")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9]{2,255}$", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "UserNameNotValid")]
        [Display(ResourceType = typeof(AccountResources), Name = "UserName")]
        public string UserName
        {
            get { return _userName.Trim(); }
            set { _userName = value.Trim(); }
        }
        
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPassword")]
        [StringLength(100, ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "PasswordLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "ConfirmPassword")]
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredConfirmPassword")]
        [Compare("Password", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "PasswordNotMatch")]
        public string ConfirmPassword { get; set; }
        
        public UserProfile UserProfile { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }


}
