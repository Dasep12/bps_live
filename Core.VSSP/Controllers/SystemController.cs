using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.SqlClient;
using System.Configuration;
using Core.VSSP.Services;
using Core.VSSP.Models;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using Core.VSSP.WorkEntity;
using System.Text;
using System.IO.Compression;
using Microsoft.Win32;
using System.Data;
using System.Drawing;
using System.Net.NetworkInformation;

namespace Core.VSSP.Controllers
{
    public class SystemController : Controller
    {
        //Db Connection string
        string DBCon = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;

        SystemService _SystemService = new SystemService();
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        vssp_entity vssp = new vssp_entity();

        public ActionResult CompanyLicense()
        {
            ViewBag.Title = "Register Company Profile";
            var _CompanyLicenseModel = _SystemService.GetLicenseInfo();
            return View(_CompanyLicenseModel);
        }

        public ActionResult CompanyLicenseEdit()
        {

            if (Session["UserType"] != null)
            {
                string utype = Session["UserType"].ToString();
                if (utype == "DEV" || utype == "ADM")
                {
                    ViewBag.Title = "Company Profile";
                    CompanyLicenseListModel _CompanyLicenseModel = _SystemService.GetLicenseInfo();
                    return View(_CompanyLicenseModel);
                }
                else
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult CompanyLicenseCRUD(CompanyLicenseModel model)
        {
            try
            {
                HttpPostedFileBase logo = Request.Files["LogoFile"];
                HttpPostedFileBase logosm = Request.Files["LogoSmallFile"];

                if (logo != null)
                {
                    model.Logo = _SystemService.ConvertToBytes(logo);
                }
                if (logosm != null)
                {
                    model.LogoSmall = _SystemService.ConvertToBytes(logosm);
                }
                string uid = "";
                if (Session["UserID"] != null)
                {
                    uid = Session["UserID"].ToString();
                }
                else
                {
                    uid = "Pre-Register";
                }
                _SystemService.CompanyLicenseCRUD(model, uid);
                return RedirectToAction("ContinuePage", null, new { cmessage = "Company registration has been " + model.FormAction + " successfully", caction = "SignOut", ccontroller = "Account", capps = Session["AppID"].ToString() });
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", null, new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "CompanyLicenseCRUD", backcontroller = "System" });
            }
        }

        /* --- Error Page ---*/
        [ValidateInput(false)]
        public ActionResult ErrorPage(string errnumber, string errmessage, string backcontroller, string backaction)
        {

            if (errnumber != null)
            {
                if (errmessage == "" || errmessage == null) errmessage = "The system is under contruction or maintenance, please contact your system administrator.";
                if (Session["Layout"].ToString() == "mainindex")
                {
                    if (backcontroller == "" || backcontroller == null) backcontroller = "Index";
                    if (backaction == "" || backaction == null) backaction = "MainIndex";
                }
                else
                {
                    if (backcontroller == "" || backcontroller == null) backcontroller = "Home";
                    if (backaction == "" || backaction == null) backaction = "Dashboard";
                }

                TempData["ErrNumber"] = errnumber;
                TempData["ErrMessage"] = HttpUtility.HtmlDecode(errmessage);
                TempData["ErrController"] = backcontroller;
                TempData["ErrAction"] = backaction;

                ViewBag.Title = "Error " + TempData["ErrNumber"];

                return View();

            }
            else
            {
                return RedirectToAction("MainIndex", "Index");
            }

        }

        /* --- Continue Page --- */
        public ActionResult ContinuePage(string cmessage, string ccontroller, string caction, string capps)
        {
            if (cmessage != null)
            {
                TempData["ContinueMessage"] = cmessage;
                TempData["ContinueController"] = ccontroller;
                TempData["ContinueAction"] = caction;
                TempData["ContinueApps"] = capps;

                return View();

            }
            else
            {
                return RedirectToAction("MainIndex", "Index");
            }
        }
        public ActionResult UnauthorizedAccess()
        {
            Session["Layout"] = "portal";
            ViewBag.Title = "Unauthorized Access!";
            TempData["ContinueMessage"] = "You dont have access previliege to this page. More info please contact your administrator";
            TempData["ContinueController"] = "Home";
            TempData["ContinueAction"] = "Dashboard";
            TempData["ContinueApps"] = "Home";

            return View();
        }
        public ActionResult ConfidentialAccess()
        {
            Session["Layout"] = "portal";
            ViewBag.Title = "Confidential Access!";
            TempData["ContinueMessage"] = "You dont have confidential access previliege to this page. More info please contact your administrator";
            TempData["ContinueController"] = "Home";
            TempData["ContinueAction"] = "Dashboard";
            TempData["ContinueApps"] = "Home";

            return View();
        }
        /* --- Sidebar ---*/
        public ActionResult SidebarList()
        {
            if (Session["UserID"] != null)
            {

                ViewBag.ActiveMenu = "Sidebar List";
                return View(_SystemService.SidebarList("").ToList()); // redirecting to all Employees List
                //return View("SidebarList");
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult SidebarAdd()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.ActiveMenu = "Add Sidebar";
                SidebarAddModel _SidebarAddModel = new SidebarAddModel();
                _SidebarAddModel.MenuLevelList = _SystemService.ComboMenuLevel();
                _SidebarAddModel.ParentList = _SystemService.ComboMenuParent(null);
                _SidebarAddModel.ClassIconList = _SystemService.ComboClassIcon();


                return View(_SidebarAddModel);
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public ActionResult SidebarEdit(string id)
        {
            if (Session["UserID"] != null)
            {
                if (id != null)
                {
                    id = id.ToUpper();
                    ViewBag.ActiveMenu = "Edit Sidebar";
                    SidebarEditModel _SidebarEditModel = _SystemService.SidebarEditList(id).Find(uid => uid.MenuID == id);
                    _SidebarEditModel.MenuLevelList = _SystemService.ComboMenuLevel();
                    _SidebarEditModel.ParentList = _SystemService.ComboMenuParent(_SidebarEditModel.MenuLevel);
                    _SidebarEditModel.ClassIconList = _SystemService.ComboClassIcon();

                    return View(_SidebarEditModel);

                }
                else
                {
                    return View("SidebarList", "Master");
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult CreateSidebar(SidebarEditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    HttpPostedFileBase file = Request.Files["ImageData"];
                    _SystemService.SaveSidebar(file, model, true, Session["UserID"].ToString());
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "Create sidebar menu has been saved successfully";
                    return RedirectToAction("SidebarList");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "SidebarList", backcontroller = "System" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "SidebarList", backcontroller = "System" });
            }
        }

        [HttpPost]
        public ActionResult UpdateSidebar(SidebarEditModel model, String MenuID)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    HttpPostedFileBase file = Request.Files["ImageData"];
                    _SystemService.SaveSidebar(file, model, false, Session["UserID"].ToString());
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "Update Sidebar has been saved successfull";
                    return RedirectToAction("SidebarList");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "SidebarList", backcontroller = "System" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "SidebarList", backcontroller = "System" });
            }

        }

        [HttpPost]
        public ActionResult SidebarDelete(SidebarListModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    _SystemService.SidebarDelete(model);
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "Sidebar menu has been deleted successfully";
                    return RedirectToAction("SidebarList");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "SidebarList", backcontroller = "System" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "SidebarList", backcontroller = "System" });
            }
        }

        public ActionResult ComboMenuLevel()
        {

            if (Session["UserID"] != null)
            {

                var menuparent = _SystemService.ComboMenuLevel();
                return Json(menuparent, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ComboMenuParent(string MenuLevel)
        {

            if (Session["UserID"] != null)
            {

                var menuparent = _SystemService.ComboMenuParent(MenuLevel);
                return Json(menuparent, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ComboIconClass()
        {

            if (Session["UserID"] != null)
            {

                var iconclass = _SystemService.ComboClassIcon();
                return Json(iconclass, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult Menu()
        {

            if (Session["UserID"] != null)
            {
                string usertype = Session["UserType"].ToString();
                var menu = _SystemService.SidebarList(usertype).ToList();
                return PartialView("_VSSPLayout/_sidebar-nav", menu);

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        /* Email Service Configuration */
        //[HttpPost]
        //public ActionResult SendEmail(string PropNumber, string Receiver, string ReceiverName, string Subject, string Message, string UrlLink)
        //{
        //    string iresult = "";
        //    try
        //    {
        //        string App1 = "Menunggu";
        //        string App2 = "Menunggu";
        //        string App3 = "Menunggu";
        //        string App4 = "Menunggu";
        //        string Note = "";

        //        var emailconfig = _SystemService.GetEmailConfiguration(null);
        //        var appinfo = _SystemService.GetApprovalInfo(PropNumber);

        //        var baseurl = GetBaseUrl();
        //        UrlLink = baseurl + UrlLink;

        //        if (appinfo.Approved1 == true)
        //        {
        //            App1 = "Diproses";
        //        } else
        //        {
        //            if (appinfo.Cancel == true)
        //            {
        //                App1 = "Ditolak";
        //            }
        //        }
        //        if (appinfo.Approved2 == true)
        //        {
        //            App2 = "Disetujui";
        //        }
        //        else
        //        {
        //            if (appinfo.Cancel == true)
        //            {
        //                App2 = "Ditolak";
        //            }
        //        }
        //        if (appinfo.Approved3 == true)
        //        {
        //            App3 = "Disetujui";
        //        }
        //        else
        //        {
        //            if (appinfo.Cancel == true)
        //            {
        //                App3 = "Ditolak";
        //            }
        //        }
        //        if (appinfo.Approved4 == true)
        //        {
        //            App4 = "Disetujui";
        //        }
        //        else
        //        {
        //            if (appinfo.Cancel == true)
        //            {
        //                App4 = "Ditolak";
        //            }
        //        }
        //        if (appinfo.Cancel == true)
        //        {
        //            Note = appinfo.CancelReason;
        //        }

        //        var senderEmail = new MailAddress(emailconfig.EmailAddress, emailconfig.SenderName);
        //        var receiverEmail = new MailAddress(Receiver, ReceiverName);
        //        var password = emailconfig.EmailPassword;
        //        var sub = Subject;
        //        var body = this.createEmailBody(ReceiverName, Subject, Message, UrlLink, App1, App2, App3, App4, Note);
        //        var smtp = new SmtpClient
        //        {
        //            Host = emailconfig.OutgoingServer,
        //            Port = emailconfig.OutgoingPort,
        //            EnableSsl = emailconfig.EnableSSL,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(senderEmail.Address, password)
        //        };
        //        using (var mess = new MailMessage(senderEmail, receiverEmail)
        //        {
        //            IsBodyHtml = true,
        //            Subject = Subject,
        //            Body = body
        //        })
        //        {
        //            smtp.Send(mess);
        //        }
        //        iresult = "success";

        //    }
        //    catch (Exception e)
        //    {
        //        //ViewBag.Error = e.Message;
        //        iresult = e.Message;
        //    }
        //    return Json(iresult, JsonRequestBehavior.AllowGet);
        //}

        private string createEmailBody(
            string applicantname,
            string day, string date, string time, string address, string agenda,
            string urlconfirm, string compid, string compname, string compaddress
            )

        {

            string body = string.Empty;
            //using streamreader for reading my htmltemplate   

            using (StreamReader reader = new StreamReader(Path.Combine(Server.MapPath("~/Views/Shared/vsspLayout/EmailTemplates"), "001.invitation.html")))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{applicantname}", applicantname); //replacing the required things  
            body = body.Replace("{day}", day);
            body = body.Replace("{time}", time);
            body = body.Replace("{address}", address);
            body = body.Replace("{agenda}", agenda);
            body = body.Replace("{urlconfirm}", urlconfirm);
            body = body.Replace("{compid}", compid);
            body = body.Replace("{compname}", compname);
            body = body.Replace("{compaddress}", compaddress);

            return body;

        }

        public bool SendInvitation(
            string processid, string applicantemail, string applicantname,
            string day, string date, string time, string address, string mapurl, string agenda,
            string testnumber, string urlconfirm, string compid, string compname, string compphone,
            string compaddress, string compcity, string compcountry, string compwebsites
            )
        {
            bool iresult = false;
            try
            {

                string body = string.Empty;
                DateTime now = DateTime.Now;
                string copyrightyear = now.ToString("yyyy");

                using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Shared/_VSSPLayout/EmailTemplates/RecruitmentInvitation/" + processid + ".html")))
                {
                    body = reader.ReadToEnd();
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                      | SecurityProtocolType.Tls11
                                      | SecurityProtocolType.Tls12;

                body = body.Replace("{applicantname}", applicantname); //replacing the required things  
                body = body.Replace("{day}", DateTime.Parse(day).ToString("dddd", new System.Globalization.CultureInfo("id-ID")));
                body = body.Replace("{date}", DateTime.Parse(date).ToString("dd MMMM yyyy"));
                body = body.Replace("{time}", DateTime.Parse(time).ToString("HH:mm") + " WIB sd Selesai");
                body = body.Replace("{address}", address.Replace("\n", "<br/>"));
                body = body.Replace("{mapurl}", mapurl);
                body = body.Replace("{agenda}", agenda);
                body = body.Replace("{testnumber}", testnumber);
                body = body.Replace("{urlconfirm}", urlconfirm);
                body = body.Replace("{position}", "Human Resource");
                body = body.Replace("{compid}", compid);
                body = body.Replace("{compname}", compname);
                body = body.Replace("{compphone}", compphone);
                body = body.Replace("{compaddress}", compaddress);
                body = body.Replace("{compcity}", compcity);
                body = body.Replace("{compcountry}", compcountry);
                body = body.Replace("{compwebsites}", compwebsites);
                body = body.Replace("{copyrightyear}", copyrightyear);

                var emailconfig = _SystemService.GetEmailConfiguration(null);

                var senderEmail = new MailAddress(emailconfig.EmailAddress, emailconfig.SenderName);
                var receiverEmail = new MailAddress(applicantemail, applicantname);
                var userid = emailconfig.EmailUserID;
                var password = emailconfig.EmailPassword;
                var sub = "Undangan " + agenda + " " + compname;
                var bodymail = body;
                var smtp = new SmtpClient
                {
                    Host = emailconfig.OutgoingServer,
                    Port = emailconfig.OutgoingPort,
                    EnableSsl = emailconfig.EnableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(userid, password)
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    IsBodyHtml = true,
                    Subject = sub,
                    Body = bodymail
                })
                {
                    smtp.Send(mess);
                }

                iresult = true;

            }
            catch (Exception e)
            {
                //ViewBag.Error = e.Message;
                iresult = false;
            }
            return iresult;
        }

        public ActionResult GetSessionInfo(string urladdress)
        {
            if (urladdress == null || urladdress == "/")
            {
                urladdress = Url.Action("MainIndex", "Index");
            }

            var _AppVersionModel = _SystemService.GetAppVersion();
            if (_AppVersionModel.AppID != null)
            {
                Session["AppID"] = _AppVersionModel.AppID;
                Session["AppName"] = _AppVersionModel.AppName;
                Session["AppVersion"] = _AppVersionModel.AppVersion;
                Session["AppRevision"] = _AppVersionModel.AppRevision;
                Session["AppBuild"] = _AppVersionModel.AppBuild;
                Session["AppCompany"] = _AppVersionModel.AppCompany;
                Session["AppWebsite"] = _AppVersionModel.AppWebsite;
                Session["AppDescription"] = _AppVersionModel.AppDescription;
                Session["AppLogo"] = _AppVersionModel.AppLogo;
            }
            else
            {
                Session["AppID"] = "BC Lab's";
                Session["AppName"] = "vssp CoreUI";
                Session["AppVersion"] = "0";
                Session["AppRevision"] = "0";
                Session["AppBuild"] = "0";
                Session["AppCompany"] = "vssp Compusoft Lab's";
                Session["AppWebsite"] = "http://www.vsspcompusoft.com";
                Session["AppDescription"] = "vssp,Compusoft,Software,HR System,HRMS,Dashboard";
                Session["AppCompanyLogo"] = null;
            }

            var _CompanyLicenseModel = _SystemService.GetLicenseInfo();
            var compid = _CompanyLicenseModel.ID;
            if (compid == null) compid = "BTK";
            if (compid != "BTK")
            {
                Session["CompID"] = _CompanyLicenseModel.ID;
                Session["CompName"] = _CompanyLicenseModel.Name;
                Session["CompTitle"] = _CompanyLicenseModel.Title;
                Session["CompTaxId"] = _CompanyLicenseModel.TaxId;
                Session["CompAddress"] = _CompanyLicenseModel.Address;
                Session["CompCity"] = _CompanyLicenseModel.City;
                Session["CompProvience"] = _CompanyLicenseModel.Provience;
                Session["CompCountry"] = _CompanyLicenseModel.Country;
                Session["CompZipcode"] = _CompanyLicenseModel.ZipCode;
                Session["CompPhone1"] = _CompanyLicenseModel.Phone1;
                Session["CompPhone2"] = _CompanyLicenseModel.Phone2;
                Session["CompFax"] = _CompanyLicenseModel.Fax;
                Session["CompEmail1"] = _CompanyLicenseModel.Email1;
                Session["CompEmail2"] = _CompanyLicenseModel.Email2;
                Session["CompWebsites"] = _CompanyLicenseModel.Websites;
                Session["CompLogo"] = _CompanyLicenseModel.Logo;
                Session["CompLogoSmall"] = _CompanyLicenseModel.LogoSmall;
                Session["LicenseNumber"] = _CompanyLicenseModel.LicenseNumber;
                Session["LicenseStart"] = _CompanyLicenseModel.LicenseStart;
                Session["LicenseEnd"] = _CompanyLicenseModel.LicenseEnd;
                Session["LicenseStatus"] = _CompanyLicenseModel.LicenseStatus;
                Session["LicenseDay"] = _CompanyLicenseModel.LicenseDay;
                Session["RemainDay"] = _CompanyLicenseModel.RemainDay;

                var ipdata = _SystemService.GetIpData();
                if (ipdata != null)
                {
                    Session["IpAddress"] = _SystemService.Vf(ipdata.ip);
                    Session["ClientCountry"] = _SystemService.Vf(ipdata.country_name);
                    Session["ClientCity"] = _SystemService.Vf(ipdata.city);
                }
                else
                {
                    Session["IpAddress"] = _SystemService.GetIP();
                    Session["ClientCountry"] = "";
                    Session["ClientCity"] = "";
                }
                return Redirect(urladdress);
            }
            else
            {
                return RedirectToAction("CompanyLicense", "System");
            }
        }
        public ActionResult AboutApps()
        {

            var _AppVersionModel = _SystemService.GetAppVersion();
            return PartialView(_AppVersionModel);

        }
        public ActionResult UpdateAppsVersion(AppVersionModel model)
        {
            string appname = Session["AppName"].ToString();

            if (ModelState.IsValid)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    HttpPostedFileBase file = Request.Files["AppsLogo"];
                    if (file != null)
                    {
                        model.AppLogo = _SystemService.ConvertToBytes(file);
                    }

                    string rs = _SystemService.UpdateAppVersion(model);
                    if (rs == "Success")
                    {
                        return RedirectToAction("ContinuePage", null, new { cmessage = "Application version has been updated successfully", caction = "SignOut", ccontroller = "Account", capps = appname });
                    }
                    else
                    {
                        return RedirectToAction("ContinuePage", null, new { cmessage = rs, caction = "Dashboard", ccontroller = "Home", capps = appname });
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    TempData["Alert"] = "Failed";
                    return RedirectToAction("ContinuePage", null, new { cmessage = errinfo, caction = "Dashboard", ccontroller = "Home", capps = appname });
                }
            }
            else
            {
                TempData["ModelsError"] = "Error";
                return RedirectToAction("ContinuePage", null, new { cmessage = "Model entity description not match!", caction = "Dashboard", ccontroller = "Home", capps = appname });
            }

        }
        public ActionResult NotificationItem()
        {

            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();

                NotificationTotalModel notificationTotals = _SystemService.NotificationTotal(uid);
                notificationTotals.NotificationSubTotalList = _SystemService.NotificationSubTotal(uid).ToList();

                return PartialView("_VSSPLayout/_user_notification", notificationTotals);
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult Notification(string Category, string Icon)
        {
            if (Session["UserID"] != null)
            {

                if (Category != null)
                {
                    Session["Param1"] = Category;
                    Session["Param2"] = Icon;
                }
                else
                {
                    Category = Session["Param1"].ToString();
                    Icon = Session["Param1"].ToString();
                }
                string uid = Session["UserID"].ToString();
                ViewBag.Title = Category;
                ViewBag.Icon = Icon;
                ViewBag.UserName = Session["UserName"].ToString();
                List<NotificationModel> _NotificationModel = _SystemService.NotificationList(uid, Category, null);
                Session["Layout"] = "portal";
                return View("NotificationList", _NotificationModel);

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }
        [HttpPost]
        public ActionResult NotificationCRUD(string ID, string FormAction)
        {
            if (Session["UserID"] != null)
            {
                string uid = Session["UserID"].ToString();
                var notificationCRUD = _SystemService.NotificationRead(ID, FormAction);
                return Json(notificationCRUD, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult LogActivities(string FormAction)
        {
            if (_SystemService.Vf(FormAction) == "partial")
            {
                ViewBag.Title = "Log Activities";
                string uid = Session["UserID"].ToString();
                IEnumerable<LogActivitiesModel> _LogActivitiesModel = _SystemService.GetLogActivities(uid);
                return PartialView(_LogActivitiesModel);

            }
            else
            {

                if (Session["UserID"] != null)
                {

                    string uid = Session["UserID"].ToString();
                    var acccessPreviliege = _AccountService.AccessPreviliege(uid, "System", "LogActivities");

                    if (acccessPreviliege.CanSee == false)
                    {
                        return RedirectToAction("UnauthorizedAccess", "System");
                    }
                    else
                    {

                        ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                        ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                        ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                        ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                        ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                        ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                        string utype = Session["UserType"].ToString();

                        if (utype == "DEV" || utype == "ADM")
                        {
                            uid = null;
                        }
                        else
                        {
                            uid = Session["UserID"].ToString();
                        }

                        ViewBag.title = "Log Activities";
                        IEnumerable<LogActivitiesModel> _LogActivitiesModel = _SystemService.GetLogActivities(uid);
                        return View(_LogActivitiesModel);
                    }
                }
                else
                {
                    Session["History"] = HttpContext.Request.Url.AbsolutePath;
                    return RedirectToAction("Login", "Account");
                }
            }

        }
        public ActionResult SystemUtilize()
        {
            SystemUtilizeModel systemUtilizeModel = _SystemService.SystemUtilize();
            systemUtilizeModel.DatabaseUtilize = _SystemService.DatabaseUtilize().ToList();
            systemUtilizeModel.DriveUtilize = _SystemService.DriveUtilize().ToList();

            return PartialView("_VSSPLayout/_asidePartial/SystemUtilize", systemUtilizeModel);
        }
        public ActionResult DatabaseBackupList()
        {
            //IEnumerable<DatabaseBackupModel> backupDatabaseModel = _SystemService.DatabaseBackupList("D:\\backups", "web_vssp");
            string appname = "";
            if (Session["AppID"] != null)
            {
                appname = Session["AppID"].ToString();
            }
            //IEnumerable<DatabaseBackupModel> backupDatabaseModel = _SystemService.DatabaseBackupList(appname);

            List<DatabaseBackupModel> backupDatabaseModel = new List<DatabaseBackupModel>();
            string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/_BackupDatabase");

            if (Directory.Exists(folder))
            {
                string[] filePaths = Directory.GetFiles(Server.MapPath("~/_BackupDatabase/"));

                //Copy File names to Model collection.
                foreach (string filePath in filePaths)
                {
                    if (filePath.Contains("bak"))
                    {
                        DatabaseBackupModel databaseBackup = new DatabaseBackupModel();

                        databaseBackup.BackupFile = Path.GetFileName(filePath);

                        backupDatabaseModel.Add(databaseBackup);
                    }
                }

                backupDatabaseModel = backupDatabaseModel.OrderByDescending(a => a.BackupFile).ToList();
            }
            
            return PartialView("_VSSPLayout/_asidePartial/DatabaseBackup", backupDatabaseModel);
        }
        public ActionResult DatabaseBackup()
        {

            string cstr = vssp.Database.Connection.ConnectionString;

            SqlConnection con = new SqlConnection(cstr);
            con.Open();

            try
            {
                string appname = "vssp";
                if (Session["UserID"] != null)
                {
                    appname = Session["AppID"].ToString();
                }

                string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/_BackupDatabase");
                string filebackup = appname.ToLower() + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string cmd = "BACKUP DATABASE [" + vssp.Database.Connection.Database + "] TO DISK='" + folder + "\\" + filebackup + "'";

                using (SqlCommand command = new SqlCommand(cmd, con))
                {
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                }
                //var databaseBackup = _SystemService.DatabaseBackup(appname);
                //return Json(databaseBackup, JsonRequestBehavior.AllowGet);

                con.Close();
                return Json(filebackup, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                if (con.State == ConnectionState.Open) con.Close();

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DatabaseRestore(string BackupFile)
        {

            string cstr = vssp.Database.Connection.ConnectionString;

            SqlConnection con = new SqlConnection(cstr);
            con.Open();

            try
            {
                string appname = "";
                if (Session["UserID"] != null)
                {
                    appname = Session["AppID"].ToString();
                }

                string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/_BackupDatabase");

                string sqlStmt2 = string.Format("ALTER DATABASE [" + vssp.Database.Connection.Database + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                SqlCommand bu2 = new SqlCommand(sqlStmt2, con);
                bu2.CommandTimeout = 0;
                bu2.ExecuteNonQuery();

                string sqlStmt3 = "USE MASTER RESTORE DATABASE [" + vssp.Database.Connection.Database + "] FROM DISK='" + folder + "\\" + BackupFile + "' WITH REPLACE;";
                SqlCommand bu3 = new SqlCommand(sqlStmt3, con);
                bu3.CommandTimeout = 0;
                bu3.ExecuteNonQuery();

                string sqlStmt4 = string.Format("ALTER DATABASE [" + vssp.Database.Connection.Database + "] SET MULTI_USER");
                SqlCommand bu4 = new SqlCommand(sqlStmt4, con);
                bu4.CommandTimeout = 0;
                bu4.ExecuteNonQuery();

                con.Close();
                return Json("success", JsonRequestBehavior.AllowGet);

                //var databaseBackup = _SystemService.DatabaseRestore(BackupFile, appname);
                //return Json(databaseBackup, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                if (con.State == ConnectionState.Open) con.Close();

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DownloadBackupFile(string database)
        {
            try
            {
                //opening the subkey 
                //RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQLServer");

                string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/_BackupDatabase");
                string backupdir = folder;

                string filename = database.Replace(".bak", "");
                string srcfile = backupdir + "\\" + database;
                string srccopy = backupdir + "\\" + database.Replace(".bak","");
                string dstfile = backupdir;


                if (!Directory.Exists(srccopy))
                {
                    Directory.CreateDirectory(srccopy);
                }

                System.IO.File.Copy(srcfile, srccopy + "\\" + database, true);

                if (System.IO.File.Exists(dstfile + "\\" + filename + ".zip"))
                    System.IO.File.Delete(dstfile + "\\" + filename + ".zip");

                ZipFile.CreateFromDirectory(srccopy, dstfile + "\\" + filename + ".zip");

                byte[] finalResult = System.IO.File.ReadAllBytes(dstfile + "\\" + filename + ".zip");

                if (System.IO.File.Exists(dstfile + "\\" + filename + ".zip"))
                    System.IO.File.Delete(dstfile + "\\" + filename + ".zip");

                if (Directory.Exists(srccopy))
                {
                    string[] filePaths = Directory.GetFiles(srccopy);

                    //Copy File names to Model collection.
                    foreach (string filePath in filePaths)
                    {
                       System.IO.File.Delete(filePath);
                    }

                    Directory.Delete(srccopy);
                }

                if (finalResult == null || !finalResult.Any())
                    throw new Exception(String.Format("No backup database files found."));

                return File(finalResult, "application/zip", filename + ".zip");                

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                byte[] finalResult = null;
                if (finalResult == null || !finalResult.Any())
                    throw new Exception(String.Format(errinfo));

                return File(finalResult, "application/zip", "");
            }
            
        }
        public ActionResult EmailSetting()
        {
            EmailConfigurationModel emailConfigurationModel = _SystemService.GetEmailConfiguration("noreply");

            return PartialView("_VSSPLayout/_asidePartial/EmailSetting", emailConfigurationModel);
        }
        public ActionResult EmailConfigurationEdit()
        {

            if (Session["UserType"] != null)
            {
                string utype = Session["UserType"].ToString();
                if (utype == "DEV" || utype == "ADM")
                {
                    ViewBag.Title = "Email Configuration";
                    EmailConfigurationModel _EmailConfigurationModel = _SystemService.GetEmailConfiguration("noreply");
                    return View(_EmailConfigurationModel);
                }
                else
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult EmailConfigurationCRUD(EmailConfigurationModel model)
        {
            try
            {
                string uid = "";
                uid = Session["UserID"].ToString();

                _SystemService.EmailConfigurationCRUD(model, uid);
                return RedirectToAction("ContinuePage", null, new { cmessage = "Email configuration has been saved successfully", caction = "SignOut", ccontroller = "Account", capps = Session["AppID"].ToString() });
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", null, new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "EmailConfigurationCRUD", backcontroller = "System" });
            }
        }
        public ActionResult ManualBook()
        {

            if (Session["UserType"] != null)
            {
                string utype = Session["UserType"].ToString();
                List<ManualBookModel> manualBook = new List<ManualBookModel>();
                FinanceAccountingController accountingController = new FinanceAccountingController();
                //Fetch all files in the Folder (Directory).
                string[] filePaths = Directory.GetFiles(Server.MapPath("~/Document/Manual Book/"));
                string baseurl = Session["BaseUrl"].ToString();

                //Copy File names to Model collection.
                foreach (string filePath in filePaths)
                {
                    ManualBookModel manual = new ManualBookModel();

                    manual.FileName = Path.GetFileName(filePath);
                    manual.FileUrl = baseurl + "/Document/Manual Book/" + Path.GetFileName(filePath);

                    manualBook.Add(manual);
                }

                if (utype != "DEV" && utype != "ADM")
                {
                    var manualadmin = manualBook.Where(a => a.FileName.Contains("Admin")).FirstOrDefault();
                    if (manualadmin != null)
                    {
                        manualBook.Remove(manualadmin);
                    }
                }

                return PartialView("_VSSPLayout/_asidePartial/ManualBook", manualBook);

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult MenuList()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "System", "MenuList");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";

                    return View(exportOption);

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult MenuListJson(string searchFilter, string menuId, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter).ToLower();
            var menulist = _SystemService.SidebarList("").ToList();
            menulist = menulist.Where(a => a.MenuID != "*" && a.Active == isActive && (a.MenuID.ToLower().Contains(searchFilter) || a.MenuName.ToLower().Contains(searchFilter) || a.ParrentID.ToLower().Contains(searchFilter) || a.ControllerName.ToLower().Contains(searchFilter) || a.ActionName.ToLower().Contains(searchFilter))).ToList();

            if (_SystemService.Vf(menuId) != "")
            {
                menulist = menulist.Where(a => a.MenuID == menuId).ToList();
            }

            menulist = menulist.OrderBy(a => a.MenuID).ToList();

            return Json(menulist, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MenuReportListJson(string menuId)
        {
            var menulist = _SystemService.MenuReportList(menuId).ToList();
            return Json(menulist, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MenuReportFilterListJson(string menuId, string schemaName, Nullable<Boolean> isActive, string supplierid)
        {
            List<MenuReportFilterListModel> filterlist = new List<MenuReportFilterListModel>();
            List<MenuReportFilterListModel> filterdata = _SystemService.MenuReportFilterList(menuId).ToList();

            if (schemaName == null && filterdata.Count() != 0) schemaName = filterdata[0].SchemaName;

            var schemaColumnList = (from a in vssp.Vw_SYS_SchemaColumnList
                                    where a.SCHEMA_NAMES == schemaName
                                    orderby a.POSITION
                                    select new { a.SCHEMA_NAMES, a.COLUMNNAME, a.POSITION, a.IS_NULLABLE, a.DATATYPE, a.LENGHT }).ToList();
            foreach (var column in schemaColumnList)
            {
                MenuReportFilterListModel _filter = new MenuReportFilterListModel();

                var _filterdata = filterdata.Where(a => a.SchemaName == column.SCHEMA_NAMES && a.Field == column.COLUMNNAME).FirstOrDefault();

                string caption = "";
                string filtername = "";
                bool active = false;
                supplierid = _SystemService.Vf(supplierid);

                if (_filterdata != null)
                {
                    caption = _filterdata.Caption;
                    filtername = _filterdata.FilterName;
                    active = _filterdata.Active;
                }
                _filter.SchemaName = column.SCHEMA_NAMES;
                _filter.Field = column.COLUMNNAME;
                _filter.Caption = caption;
                _filter.FilterName = filtername;
                _filter.FilterType = column.DATATYPE;
                _filter.FilterValues = (column.COLUMNNAME.ToLower() == "supplierid" ? supplierid : "");
                _filter.Active = active;
                _filter.Sort = int.Parse(_SystemService.Vn(column.POSITION.ToString()).ToString());

                filterlist.Add(_filter);
            }

            if (filterlist.Count() != 0 && isActive != null)
            {
                filterlist = filterlist.Where(a => a.Active == isActive).ToList();
            }

            return Json(filterlist, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudMenuList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();
                    postMenuListModel postMenu = JsonConvert.DeserializeObject<postMenuListModel>(jsonData);
                    MenuListModel menu = postMenu.MenuList;
                    MenuReportListModel menuReport = postMenu.MenuReportList;
                    List<MenuReportFilterListModel> menuReportFilter = postMenu.MenuReportFilterLists;
                    string MSQL = "";
                    SqlConnection conn = new SqlConnection(DBCon);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                    switch (menu.FormAction.ToLower())
                    {
                        case "create":

                            MSQL = "INSERT INTO Tbl_SYS_Menu SELECT ";
                            MSQL += "'" + menu.MenuID + "',";
                            MSQL += "'" + menu.MenuName + "',";
                            MSQL += "'" + menu.MenuLevel + "',";
                            MSQL += "'" + menu.ParrentID + "',";
                            MSQL += "'" + menu.IconClass + "',";
                            MSQL += "'" + menu.ControllerName + "',";
                            MSQL += "'" + menu.ActionName + "',";
                            MSQL += "" + _SystemService.Vbn(menu.NeedApproval.ToString()) + ",";
                            MSQL += "" + _SystemService.Vbn(menu.Confidential.ToString()) + ",";
                            MSQL += "" + _SystemService.Vbn(menu.Active.ToString()) + "";

                            crudMenuReportList(menuReport, menu.MenuID, menu.FormAction);
                            crudMenuReportFilterList(menuReportFilter, menu.MenuID, menu.FormAction);

                            break;

                        case "update":

                            MSQL = "UPDATE Tbl_SYS_Menu SET ";
                            MSQL += "ParrentID          ='" + menu.ParrentID + "',";
                            MSQL += "MenuName           ='" + menu.MenuName + "',";
                            MSQL += "MenuLevel          ='" + menu.MenuLevel + "',";
                            MSQL += "IconClass          ='" + menu.IconClass + "',";
                            MSQL += "ControllerName     ='" + menu.ControllerName + "',";
                            MSQL += "ActionName         ='" + menu.ActionName + "',";
                            MSQL += "NeedApproval       =" + _SystemService.Vbn(menu.NeedApproval.ToString()) + ",";
                            MSQL += "Confidential       =" + _SystemService.Vbn(menu.Confidential.ToString()) + ",";
                            MSQL += "Active             =" + _SystemService.Vbn(menu.Active.ToString()) + " ";
                            MSQL += "Where MenuID       ='" + menu.MenuID + "'";

                            crudMenuReportList(menuReport, menu.MenuID, menu.FormAction);
                            crudMenuReportFilterList(menuReportFilter, menu.MenuID, menu.FormAction);

                            break;

                        case "delete":

                            MSQL = "DELETE FROM Tbl_SYS_Menu ";
                            MSQL += "Where MenuID       ='" + menu.MenuID + "'";

                            crudMenuReportList(menuReport, menu.MenuID, menu.FormAction);
                            crudMenuReportFilterList(menuReportFilter, menu.MenuID, menu.FormAction);

                            break;
                    }

                    try
                    {
                        cmd.CommandText = MSQL;
                        cmd.ExecuteNonQuery();

                        return Json(menu, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public void crudMenuReportList(MenuReportListModel menuReport, string menuid, string formAction)
        {
            string MSQL = "";
            SqlConnection conn = new SqlConnection(DBCon);
            conn.Open();
            SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

            MSQL = "DELETE FROM Tbl_SYS_MenuReport ";
            MSQL += "Where MenuID       ='" + menuid + "'";

            cmd.CommandText = MSQL;
            cmd.ExecuteNonQuery();

            MSQL = "INSERT INTO Tbl_SYS_MenuReport SELECT ";
            MSQL += "'" + menuid + "',";
            MSQL += "'" + menuReport.FileName + "',";
            MSQL += "'" + menuReport.SchemaType + "',";
            MSQL += "'" + menuReport.SchemaName + "',";
            MSQL += "'" + menuReport.CustomFilter + "',";
            MSQL += "'" + menuReport.SortOrder + "'";

            cmd.CommandText = MSQL;
            cmd.ExecuteNonQuery();
        }
        public void crudMenuReportFilterList(List<MenuReportFilterListModel> menuReportFilter, string menuid, string formAction)
        {

            string MSQL = "";
            SqlConnection conn = new SqlConnection(DBCon);
            conn.Open();
            SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

            MSQL = "DELETE FROM Tbl_SYS_MenuReportFilter ";
            MSQL += "Where MenuID   ='" + menuid + "' ";

            cmd.CommandText = MSQL;
            cmd.ExecuteNonQuery();

            foreach (var filter in menuReportFilter)
            {


                MSQL = "INSERT INTO Tbl_SYS_MenuReportFilter SELECT ";
                MSQL += "'" + menuid + "',";
                MSQL += "'" + filter.SchemaName + "',";
                MSQL += "'" + filter.Field + "',";
                MSQL += "'" + filter.Caption + "',";
                MSQL += "'" + filter.FilterName + "',";
                MSQL += "'" + filter.FilterType + "',";
                MSQL += "" + _SystemService.Vbn(filter.Active.ToString()) + ",";
                MSQL += "" + _SystemService.Vn(filter.Sort.ToString()) + "";

                cmd.CommandText = MSQL;
                cmd.ExecuteNonQuery();
            }
        }
        public ActionResult ReportFile()
        {

            List<ManualBookModel> files = new List<ManualBookModel>();
            //Fetch all files in the Folder (Directory).
            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Views/Reports/"));
            string baseurl = Session["BaseUrl"].ToString();

            //Copy File names to Model collection.
            foreach (string filePath in filePaths)
            {
                ManualBookModel file = new ManualBookModel();

                string filename = Path.GetFileName(filePath);

                int len = filename.Length;

                if (filename.Substring(len - 3, 3) == "rpt")
                {
                    file.FileName = Path.GetFileName(filePath);
                    file.FileUrl = baseurl + "/Reports/" + Path.GetFileName(filePath);

                    files.Add(file);
                }
            }

            return Json(files, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SchemaList(string schemaType)
        {

            var schemaList = vssp.Vw_SYS_SchemaList.Where(a => a.SCHEMA_TYPE == schemaType).OrderBy(a => a.SCHEMA_NAMES).ToList();
            return Json(schemaList, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SchemaColumnList(string schemaName)
        {

            var schemaColumnList = (from a in vssp.Vw_SYS_SchemaColumnList
                                    where a.SCHEMA_NAMES == schemaName
                                    orderby a.POSITION
                                    select new { a.SCHEMA_NAMES, a.COLUMNNAME, a.POSITION, a.IS_NULLABLE, a.DATATYPE, a.LENGHT }).ToList();
            return Json(schemaColumnList, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SQLInjection()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "System", "SQLInjection");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";

                    return View(exportOption);

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult RunSQLInjection(string sqlScript)
        {
            try
            {
                sqlScript = System.Text.RegularExpressions.Regex.Unescape(sqlScript);
                //sqlScript = sqlScript.Replace("\n\t", " ").Replace("\t\t", " ").Replace("\t", " ").Replace("\n", " ");
                SP_SYS_SQLInjection_Result sqlinjection = new SP_SYS_SQLInjection_Result();

                if (sqlScript.Substring(0,10).ToLower().Contains("select"))
                {
                    sqlinjection = vssp.SP_SYS_SQLInjection(sqlScript).FirstOrDefault();
                    return Json(sqlinjection, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var res = vssp.Database.ExecuteSqlCommand(sqlScript);
                    sqlinjection.ErrorMessages = res.ToString();

                    return Json(sqlinjection, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult FileToBase64(string path, string format)
        {
            FileConvertBase64 fileConvertBase64 = new FileConvertBase64();
            try
            {
                path = System.Web.Hosting.HostingEnvironment.MapPath(path.Replace("../", "~/"));
                byte[] imageArray = System.IO.File.ReadAllBytes(path);
                string dst = Convert.ToBase64String(imageArray);
                fileConvertBase64.src = path;
                fileConvertBase64.dst = "data:image/jpg;base64," + dst;
                fileConvertBase64.contenttype = format;
            } catch(Exception ex)
            {
                path = System.Web.Hosting.HostingEnvironment.MapPath("~/_VSSPAssets/Images/noimage.png");
                byte[] imageArray = System.IO.File.ReadAllBytes(path);
                string dst = Convert.ToBase64String(imageArray);
                fileConvertBase64.src = path;
                fileConvertBase64.dst = "data:image/jpg;base64," + dst;
                fileConvertBase64.contenttype = format;
            }

            var jsonResult = Json(fileConvertBase64, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult GetGUIDJson()
        {
            Guid guid = new Guid();
            try
            {
                return Json(guid.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(_SystemService.Vd(DateTime.Now.ToString(),"yyyyMMddHHmmss"), JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult checkConnection()
        {
            string baseUri = HttpContext.Request.Url.AbsoluteUri;
            string basePath = HttpContext.Request.Url.AbsolutePath;
            //baseUri = "https://bps.bonecomtricom.com/";
            string baseUrl = baseUri.Replace(basePath, "").Replace("https://", "").Replace("http://", "").Replace(".com/",".com").Replace(".id/",".id");

            return Json("Success", JsonRequestBehavior.AllowGet);

            //try
            //{
            //    if (baseUrl.Contains("localhost"))
            //    {
            //        return Json("Success", JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        var pingStatus = new Ping().Send(baseUrl).Status;
            //        return Json(pingStatus.ToString(), JsonRequestBehavior.AllowGet);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //    var errinfo = _SystemService.GetExceptionDetails(e);
            //    errinfo += " <b>" + baseUrl + "</b>\n"; 
            //    return Json(errinfo, JsonRequestBehavior.AllowGet);
            //}
        }
    }
}