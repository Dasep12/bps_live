using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    /* MENU */
    public class postMenuListModel
    {
        public MenuListModel MenuList { get; set; }
        public MenuReportListModel MenuReportList { get; set; }
        public List<MenuReportFilterListModel> MenuReportFilterLists { get; set; }

    }
    public class MenuListModel
    {
        public string MenuID { get; set; }
        public string MenuName { get; set; }
        public string MenuLevel { get; set; }
        public string ParrentID { get; set; }
        public string IconClass { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool NeedApproval { get; set; }
        public bool Confidential { get; set; }
        public bool Active { get; set; }
        public string FormAction { get; set; }
    }
    public class MenuReportListModel
    {
        public string MenuID { get; set; }
        public string FileName { get; set; }
        public string SchemaType { get; set; }
        public string SchemaName { get; set; }
        public string CustomFilter { get; set; }
        public string SortOrder { get; set; }
    }
    public class MenuReportFilterListModel
    {
        public string MenuID { get; set; }
        public string SchemaName { get; set; }
        public string Field { get; set; }
        public string Caption { get; set; }
        public string FilterName { get; set; }
        public string FilterType { get; set; }
        public string FilterValues { get; set; }
        public bool Active { get; set; }
        public int Sort { get; set; }
    }
    public class SidebarListModel
    {
        public double No { get; set; }
        public string MenuID { get; set; }
        public string MenuName { get; set; }
        public string MenuLevel { get; set; }
        public string ParrentID { get; set; }
        public string IconClass { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool NeedApproval { get; set; }
        public bool Confidential { get; set; }
        public bool Active { get; set; }
    }

    public class SidebarAddModel
    {
        [Required]
        [Display(Name = "ID")]
        public string MenuID { get; set; }
        [Required]
        [Display(Name = "Menu Name")]
        public string MenuName { get; set; }
        [Required]
        [Display(Name = "Level")]
        public string MenuLevel { get; set; }
        [Required]
        [Display(Name = "Parrent ID")]
        public string ParrentID { get; set; }
        [Required]
        [Display(Name = "Class Icon")]
        public string IconClass { get; set; }
        [Display(Name = "Controller")]
        public string ControllerName { get; set; }
        [Display(Name = "Need Approval")]
        public bool NeedApproval { get; set; }
        [Display(Name = "Action")]
        public string ActionName { get; set; }

        public IEnumerable<MenuLevelModel> MenuLevelList { get; set; }
        public IEnumerable<MenuParentModel> ParentList { get; set; }
        public IEnumerable<ClassIconModel> ClassIconList { get; set; }

    }
    public class SidebarEditModel
    {
        [Required]
        [Display(Name = "ID")]
        public string MenuID { get; set; }
        [Required]
        [Display(Name = "Menu Name")]
        public string MenuName { get; set; }
        [Required]
        [Display(Name = "Level")]
        public string MenuLevel { get; set; }
        [Required]
        [Display(Name = "Parrent ID")]
        public string ParrentID { get; set; }
        [Required]
        [Display(Name = "Class Icon")]
        public string IconClass { get; set; }
        [Display(Name = "Controller")]
        public string ControllerName { get; set; }
        [Display(Name = "Action")]
        public string ActionName { get; set; }
        [Display(Name = "Need Approval")]
        public bool NeedApproval { get; set; }
        [Display(Name = "Confidential")]
        public bool Confidential { get; set; }
        [Display(Name = "Active")]
        public bool Active { get; set; }

        public IEnumerable<MenuLevelModel> MenuLevelList { get; set; }
        public IEnumerable<MenuParentModel> ParentList { get; set; }
        public IEnumerable<ClassIconModel> ClassIconList { get; set; }

    }

    public class MenuLevelModel
    {
        public string LevelID { get; set; }
        public string LevelName { get; set; }
    }

    public class MenuParentModel
    {
        public string MenuID { get; set; }
        public string MenuName { get; set; }
        public string MenuLevel { get; set; }
    }

    public class ClassIconModel
    {
        public string IconID { get; set; }
        public string IconName { get; set; }
        public string IconSortName { get; set; }
    }

    public class MenuModel
    {
        public double No { get; set; }
        public string MenuID { get; set; }
        public string MenuName { get; set; }
        public string MenuLevel { get; set; }
        public string IconClass { get; set; }
        public bool NeedApproval { get; set; }
        public bool IsActive { get; set; }
        public bool hasChildren { get; set; }
        public bool ConfidentialAccess { get; set; }
        public bool CanSee { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
    }

    public class UserApprovalListModel
    {
        public string MenuID { get; set; }
        public string MenuName { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
        public byte[] Sign { get; set; }
    }

    public class ExportOptionModel
    {
        public string ExportOption { get; set; }
        public List<ExportListModel> ExportList { get; set; }
    }

    public class ExportListModel
    {
        public string ExportID { get; set; }
        public string ExportName { get; set; }
    }


    public class ProcessListModel
    {
        public string ProcessID { get; set; }
        public string ProcessName { get; set; }
        public string Remark { get; set; }
        public int Sort { get; set; }
    }
    public class TahunListModel
    {
        public string TahunID { get; set; }
        public string TahunName { get; set; }
    }

    public class BulanListModel
    {
        public string MonthID { get; set; }
        public string MonthName { get; set; }
    }

    public class JobTitleListModel
    {
        public string JobTitleID { get; set; }
        public string JobTitleName { get; set; }
    }
    public class GraduateListModel
    {
        public string GraduateID { get; set; }
        public string GraduateName { get; set; }
        public string Remark { get; set; }
        public bool Active { get; set; }
        public int Sort { get; set; }

    }
    public class EmailConfigurationModel
    {
        [Required]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [Required]
        [Display(Name = "Sender Name")]
        public string SenderName { get; set; }
        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [Required]
        [Display(Name = "User Id")]
        public string EmailUserID { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string EmailPassword { get; set; }
        [Required]
        [Display(Name = "SMTP Server")]
        public string OutgoingServer { get; set; }
        [Required]
        [Display(Name = "SMTP Ports")]
        public int OutgoingPort { get; set; }
        [Required]
        [Display(Name = "Enable SSL")]
        public bool EnableSSL { get; set; }
    }    
    public class CompanyLicenseListModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string TaxId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Provience { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string Websites { get; set; }
        public byte[] Logo { get; set; }
        public byte[] LogoSmall { get; set; }
        public string FormAction { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseStart { get; set; }
        public string LicenseEnd { get; set; }
        public string LicenseStatus { get; set; }
        public string LicenseDay { get; set; }
        public string RemainDay { get; set; }
        public string LicenseCertificate { get; set; }

    }
    public class CompanyLicenseModel
    {
        [Required]
        public string ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string TaxId { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Provience { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string ZipCode { get; set; }
        [Required]
        public string Phone1 { get; set; }
        [Required]
        public string Phone2 { get; set; }
        [Required]
        public string Fax { get; set; }
        [Required]
        public string Email1 { get; set; }
        [Required]
        public string Email2 { get; set; }
        [Required]
        public string Websites { get; set; }
        [Required]
        public byte[] Logo { get; set; }
        [Required]
        public byte[] LogoSmall { get; set; }
        [Required]
        public string FormAction { get; set; }
    }

    public class ErrorPageModel
    {
        public string ErrNumber { get; set; }
        public string ErrMessage { get; set; }
        public string BackController { get; set; }
        public string BackAction { get; set; }

    }

    public class AppVersionModel
    {
        public string AppID { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public string AppRevision { get; set; }
        public string AppBuild { get; set; }
        public string AppCompany { get; set; }
        public string AppWebsite { get; set; }
        public string AppDescription { get; set; }
        public byte[] AppLogo { get; set; }
    }
    public class NotificationTotalModel
    {
        public string UserID { get; set; }
        public double CountCategory { get; set; }
        public List<NotificationSubTotalModel> NotificationSubTotalList { get; set; }

    }
    public class NotificationSubTotalModel
    {
        public string UserID { get; set; }
        public string Category { get; set; }
        public double CountCategory { get; set; }
        public string Icon { get; set; }
    }
    public class NotificationModel
    {
        public string RecordNumber { get; set; }
        public string RecordDate { get; set; }
        public string UserID { get; set; }
        public string Category { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string MessageHtml { get; set; }
        public string UrlLink { get; set; }
        public bool? Sent { get; set; }
        public bool? Readed { get; set; }
        public bool? Favorites { get; set; }
        public bool? Deleted { get; set; }
        public string Status { get; set; }
    }
    public class LogActivitiesModel
    {
        public string LogDate { get; set; }
        public string UserID { get; set; }
        public string IpAddress { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Module { get; set; }
        public string Activity { get; set; }
        public string Remarks { get; set; }
    }
    public class SystemUtilizeModel
    {
        public string ProcessorName { get; set; }
        public double LogicalCpu { get; set; }
        public double PhysicalMemory { get; set; }
        public List<DatabaseUtilizeModel> DatabaseUtilize { get; set; }
        public List<DriveUtilizeModel> DriveUtilize { get; set; }
    }
    public class DatabaseUtilizeModel
    {
        public string InstanceName { get; set; }
        public string Edition { get; set; }
        public string ProductVersion { get; set; }
        public string ProductLevel { get; set; }
        public string PhysicalName { get; set; }
        public double DatabaseSize { get; set; }
        public string Units { get; set; }

    }
    public class DriveUtilizeModel
    {
        public string DriveLetter { get; set; }
        public string Label { get; set; }
        public double FreeSpace { get; set; }
        public double UsedSpace { get; set; }
        public double TotalSpace { get; set; }
        public double PercentageFreeSpace { get; set; }
        public double PercentageUsedSpace { get; set; }
        public string PercentColour { get; set; }
    }
    public class DatabaseBackupModel
    {
        public string BackupFile { get; set; }
        public string BackupDate { get; set; }
        public bool Restored { get; set; }
        public string OperateBy { get; set; }
        public string OperateDate { get; set; }
    }


    public class PopulatingJQGridModel
    {
        List<jqgridcolModel> colModels { get; set; }
        List<jqgridcolDataModel> colData { get; set; }
    }

    public class jqgridcolModel
    {
        public string label { get; set; }
        public string name { get; set; }
        public int width { get; set; }
        public string align { get; set; }
        public string formatter { get; set; }

    }
    public class jqgridcolDataModel
    {
        public string name { get; set; }
        public string value { get; set; }

    }
    public class GeoLocationModel
    {

    }

    public class OrderApproval
    {
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Nullable<int> ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
        public string ApprovalEmail { get; set; }
        public Nullable<bool> SentEmail { get; set; }
        public Nullable<System.DateTime> SentEmailDate { get; set; }
        public Nullable<bool> Approved { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public string Messages { get; set; }
    }
    public class ManualBookModel
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
    public class FileConvertBase64
    {
        public string src { get; set; }
        public string dst { get; set; }
        public string contenttype { get; set; }
    }
}