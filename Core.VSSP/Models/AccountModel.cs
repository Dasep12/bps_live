using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class LoginModel
    {
        /* loginModel */
        [Required]
        [Display(Name = "UserID")]
        public string UserID { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string LogIpAddress { get; set; }
        public string LogCountry { get; set; }
        public string LogCity { get; set; }

    }

    public class LoginResultModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string Phone { get; set; }
        public string Organization { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
        public string Area { get; set; }
        public bool ConfidentialAccess { get; set; }
        public byte[] Image { get; set; }
        public byte[] Sign { get; set; }
        public string DateCreated { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string LogIpAddress { get; set; }
        public string LogCountry { get; set; }
        public string LogCity { get; set; }
        public IEnumerable<CompanyLicenseModel> CompanyLicenseList { get; set; }

    }

    /* --- User ---*/
    public class UserListModel
    {
        public int No { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserTypeID { get; set; }
        public string Phone { get; set; }
        public string Area { get; set; }
        public bool ConfidentialAccess { get; set; }
        public byte[] Image { get; set; }
        public byte[] Sign { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public string DateCreated { get; set; }

    }

    public class UserAddModel
    {

        [Required]
        [Display(Name = "User ID")]
        public string UserID { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "User Type")]
        public string UserTypeID { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        [Display(Name = "Area")]
        public string Area { get; set; }
        [Display(Name = "Confidential Access")]
        public bool ConfidentialAccess { get; set; }
        [Display(Name = "Avatar")]
        public byte[] Image { get; set; }
        public byte[] Sign { get; set; }
        public IEnumerable<UserTypeListModel> UserTypeList { get; set; }
        public IEnumerable<AreaListModel> AreaList { get; set; }
        public IEnumerable<ApprovalTypeListModel> ApprovalTypeList { get; set; }

    }

    public class UserEditModel
    {

        [Required]
        [Display(Name = "User ID")]
        public string UserID { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "User Type")]
        public string UserTypeID { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        [Display(Name = "Area")]
        public string Area { get; set; }
        [Display(Name = "Confidential Access")]
        public bool ConfidentialAccess { get; set; }

        [Display(Name = "Avatar")]
        public byte[] Image { get; set; }
        public byte[] Sign { get; set; }
        public string ReturnAction { get; set; }

        public IEnumerable<UserTypeListModel> UserTypeList { get; set; }
        public IEnumerable<AreaListModel> AreaList { get; set; }
        public IEnumerable<ApprovalTypeListModel> ApprovalTypeList { get; set; }
    }
    public class UserProfileModel
    {
        public UserEditModel UserEdit { get; set; }
        public UserProfileSummaryModel UserProfileSummary { get; set; }

    }
    public class UserProfileSummaryModel
    {
        public double IncomingMessages { get; set; }
        public double LogActivities { get; set; }
    }
    public class UserTypeListModel
    {
        public int No { get; set; }
        public string ID { get; set; }
        public string UserType { get; set; }
        public string Remark { get; set; }
        public string MenuList { get; set; }
    }
    public class ApprovalTypeListModel
    {
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
    }
    public class UserApprovalTypeListModel
    {
        public int ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
    }
    public class postUserRolesModel
    {
        public UserRolesModel UserRoles { get; set; }

    }
    public class UserRolesModel
    {
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public string Remarks { get; set; }
        public string MenuList { get; set; }
        public string FormAction { get; set; }
    }
    public class postUserManagementModel
    {
        public UserManagementModel UserManagement { get; set; }

    }
    public class UserManagementModel
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserTypeID { get; set; }
        public string Phone { get; set; }
        public string Area { get; set; }
        public bool ConfidentialAccess { get; set; }
        public byte[] Image { get; set; }
        public byte[] Sign { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public string DateCreated { get; set; }
        public string FormAction { get; set; }
    }
    public class UserTypeAddModel
    {
        [Required]
        [Display(Name = "ID")]
        public string CategoryID { get; set; }
        [Required]
        [Display(Name = "Type Name")]
        public string UserType { get; set; }
        [Required]
        [Display(Name = "Remark")]
        public string Remark { get; set; }
        [Required]
        [Display(Name = "Menu List")]
        public string MenuList { get; set; }
    }

    public class UserTypeEditModel
    {
        [Required]
        [Display(Name = "ID")]
        public string CategoryID { get; set; }
        [Required]
        [Display(Name = "User Type")]
        public string UserType { get; set; }
        [Required]
        [Display(Name = "Remark")]
        public string Remark { get; set; }
        [Required]
        [Display(Name = "Menu List")]
        public string MenuList { get; set; }
    }

    public class AreaListModel
    {
        public string AreaID { get; set; }
        public string AreaName { get; set; }
    }

    public class RegisterModel
    {
        public string UserID { get; set; }
    }
    public class ResetModel
    {
        public string UserID { get; set; }
    }
    public class ValidationUserModel
    {
        public double CountAccount { get; set; }
    }
    public class UserResetModel
    {
        public string Token { get; set; }
        public string RequestTime { get; set; }
        public string ExpiredTime { get; set; }
        public string EmailAddress { get; set; }
        public double LifeTime { get; set; }
        public string Taken { get; set; }
        public bool Valid { get; set; }
        public bool Expire { get; set; }
    }
}