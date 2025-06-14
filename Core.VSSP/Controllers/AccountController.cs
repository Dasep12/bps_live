using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Web.Security;
using Core.VSSP.Models;
using Core.VSSP.Services;
using System.IO;
using System.Net.Mail;
using System.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.Validation;

namespace Core.VSSP.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        HomeController _HomeController = new HomeController();
        CoreUIController _CoreUIController = new CoreUIController();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        SystemController _SystemController = new SystemController();
        CryptoLibService _CryptoLibService = new CryptoLibService();

        public ActionResult Login()
        {
            if (Session["CompID"] == null)
            {
                return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
            }
            else
            {
                if (Session["UserID"] != null)
                {
                    if (Session["UserType"].ToString() == "MBR")
                    {
                        ViewBag.Title = "Home";
                        return View(_CoreUIController.Index("Dashboard"));
                    }
                    else
                    {
                        ViewBag.Title = "Dashboard";
                        return View(_HomeController.Index());
                    }
                }
                else
                {
                    ViewBag.Title = "Login";
                    return View();
                }
            }
        }
         public ActionResult WebLoginJson(string userid,string password)
        {
            LoginModel loginModel = new LoginModel();
            loginModel.UserID = userid;
            loginModel.Password = password;
            LoginResultModel loginResult = _AccountService.UserAuthenticate(loginModel);
            return Json(loginResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult WebLogin(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                model.LogIpAddress      = _SystemService.GetIP();
                var loginResult         = _AccountService.UserAuthenticate(model);
                var stockTakingEvent    = _SystemService.GetStockTakingEvent();

                if (loginResult.UserID != null)
                {

                    Session["UserID"]           = loginResult.UserID.ToString();
                    Session["UserName"]         = loginResult.UserName.ToString();
                    Session["UserNameDisplay"]  = loginResult.UserName.ToString();
                    Session["UserType"]         = loginResult.UserType.ToString();
                    Session["Phone"]            = loginResult.Phone.ToString();
                    Session["Organization"]     = loginResult.Organization.ToString();
                    Session["Location"]         = loginResult.Location.ToString();
                    Session["Email"]            = loginResult.Email.ToString();
                    Session["Area"]             = loginResult.Area.ToString();
                    Session["DateCreate"]       = loginResult.DateCreated.ToString();
                    Session["Images"]           = loginResult.Image;
                    Session["Sign"]             = loginResult.Sign;
                    Session["UserAction"]       = "";
                    Session["IpAddress"]        = loginResult.LogIpAddress;
                    Session["Country"]          = loginResult.LogCountry;
                    Session["City"]             = loginResult.LogCity;
                    Session["ServerName"]       = loginResult.ServerName.ToString();
                    Session["DatabaseName"]     = loginResult.DatabaseName.ToString();
                    Session["Timeout"]          = 360;
                    Session.Timeout             = 360;
                    HttpContext.Session.Timeout = 360;
                    Session["BaseUrl"]          = GetBaseUrl();

                    if (loginResult.UserName.ToString().Length > 18)
                    {
                        Session["UserNameDisplay"] = loginResult.UserName.ToString().Substring(0, 14) + "...";
                    }

                    model.Password = string.Empty;
                    DateTime tdate = DateTime.Now;

                    if (stockTakingEvent != null)
                    {
                        Session["InventoryNumber"] = stockTakingEvent.InventoryNumber;
                        Session["InventoryDate"] = stockTakingEvent.InventoryDate;
                        Session["InventoryStartTime"] = stockTakingEvent.InventoryStartTime;
                        Session["InventoryEndTime"] = stockTakingEvent.InventoryEndTime;
                        Session["InventoryStatus"] = stockTakingEvent.InventoryStatus;
                        if (stockTakingEvent.InventoryStatus.Contains("in progress"))
                        {
                            Session["InventoryCountTime"] = stockTakingEvent.InventoryEndTime;
                        } else
                        {
                            Session["InventoryCountTime"] = stockTakingEvent.InventoryStartTime;
                        }
                    } else
                    {
                        Session["InventoryNumber"] = "";
                        Session["InventoryDate"] = "";
                        Session["InventoryStartTime"] = "";
                        Session["InventoryEndTime"] = "";
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";
                    }

                    if (Session["History"] != null)
                    {
                        string historyurl = Session["History"].ToString();
                        int len = historyurl.Length;
                        var viewurl = historyurl.Substring(1, len - 1);

                        if (historyurl== "/System/NotificationItem")
                        {
                            return RedirectToAction("Dashboard", "Home");
                        } else
                        {
                            return Redirect(historyurl);
                        }

                    } else
                    {
                        return RedirectToAction("Dashboard", "Home");
                    }

                }
                else
                {
                    TempData["Alert"] = "Failed";
                    TempData["Message"] = "Wrong user name or password. please type a correct user name and password";
                    return RedirectToAction("Login");
                }

            }
            return RedirectToAction("Login");
        }
        public ActionResult RequestEdit()
        {
            if (Session["UserID"] != null)
            {
                Session["UserAction"] = "Edit";
                return RedirectToAction("MainIndex", "Index");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult Register()
        {
            ViewBag.Title = "Register";
            return View();
        }
        public ActionResult ForgotPassword()
        {
            ViewBag.Title = "Forgot Password";
            return View();
        }
        public ActionResult ChangePassword()
        {
            ViewBag.Title = "Change Password";
            return View();
        }
        [HttpPost]
        public ActionResult WebLoginRequestReset(string emailaddress)
        {
            Guid gToken = Guid.NewGuid();
            string token = gToken.ToString();//_SystemService.EncryptPass(emailaddress + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string urlapi = GetBaseUrl() + "/account/resetrequest?token=" + token;
            bool crudreset = _AccountService.crudResetPassword(token, emailaddress, null, 12, false, "request");
            bool emailresult = false;

            if (crudreset == true)
            {
                emailresult = this.SentEmailReset(emailaddress, emailaddress, urlapi);

                var logs = _SystemService.LogActivitiesCRUD(emailaddress, Session["IpAddress"].ToString(),
                                                    Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                                    "User", "Request reset password", "Success");


            }

            return RedirectToAction("ResetPasswordResult", "Index", new { resetresult=emailresult, formaction = "request" });
        }
        public ActionResult resetrequest(string token)
        {
            List<UserResetModel> userResetModel = new List<UserResetModel>().ToList();
            userResetModel = _AccountService.GetUserResetRequest(token).ToList();
            var user = (from u in userResetModel
                        where u.Token == token
                        select u).FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("ResetPasswordResult", "Index", new { resetresult = false, formaction = "expired" });
            } else
            {
                if (user.Expire == true)
                {
                    return RedirectToAction("ResetPasswordResult", "Index", new { resetresult = false, formaction = "expired" });
                }
                else
                {
                    return View("LoginReset", user);
                }
            }
            
        }
        [HttpPost]
        public ActionResult WebLoginCrudReset(string token, string emailaddress, string password)
        {
            bool crudreset = _AccountService.crudResetPassword(token, emailaddress, password, 12, true, "taken");
            var logs = _SystemService.LogActivitiesCRUD(emailaddress, Session["IpAddress"].ToString(),
                                                Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                                "User", "Reset password", "Success");


            return RedirectToAction("ResetPasswordResult", "Index", new { resetresult = crudreset, formaction = "reset" });
        }
        public ActionResult CekUser(string userid, string oldpassword)
        {
            var cekUser = _AccountService.CekUser(userid, oldpassword);
            return Json(cekUser, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SignOut()
        {

            var logs = _SystemService.LogActivitiesCRUD(Session["UserID"].ToString(), Session["IpAddress"].ToString(), 
                                                Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                                "User", "Sign out", "Success");

            FormsAuthentication.SignOut();
            Session.Abandon(); // it will clear the session at the end of request
            return RedirectToAction("MainIndex","Index");

            //Session["UserID"] = null;
            //Session["UserName"] = null;
            //Session["UserNameDisplay"] = null;
            //Session["UserType"] = null;

            //return RedirectToAction("Login", "Account");
        }

        public ActionResult UserMenuListJson(string userCategoryID, string userId, Nullable<Boolean> needApproval = false)
        {

            if (Session["UserID"] != null)
            {
                string utype = Session["UserType"].ToString();
                var usermenu = _AccountService.UserMenuList(userCategoryID, utype, userId, null, needApproval).ToList();
                return Json(usermenu, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult CreateAccount(
                string UserID,
                string Password,
                string UserName,
                string UserRole,
                string Email,
                bool CreateNew)
        {


            var createaccount = _AccountService.CreateAccount(UserID, Password, UserName, UserRole, Email, CreateNew);
            return Json(createaccount);

        }

        [HttpPost]
        public ActionResult SaveUserRole(string UserID, string MenuID, string IsShow, string IsAdd, string IsEdit, string IsDelete)
        {

            if (Session["UserID"] != null)
            {

                var usermenu = _AccountService.SaveUserRole(UserID, MenuID, IsShow, IsAdd, IsEdit, IsDelete);
                return Json(usermenu);
                //return RedirectToAction("CreateUser", "Transaction");

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        /* --- User Management ---*/
        public ActionResult UserManagement()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Account", "UserManagement");

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
        public ActionResult UserManagementJson(string searchFilter, string userId, bool quality = false, bool isActive = true)
        {
            if (Session["UserID"] != null)
            {

                string utype = Session["UserType"].ToString();

                searchFilter = _SystemService.Vf(searchFilter).ToLower();
                var userManagement = _AccountService.UserList(utype).ToList();
                userManagement = userManagement.Where(a => a.IsActive == isActive && (a.UserID.ToLower().Contains(searchFilter) || a.UserName.ToLower().Contains(searchFilter) || a.Email.ToLower().Contains(searchFilter))).ToList();

                if (_SystemService.Vf(userId) != "")
                {
                    userManagement = userManagement.Where(a => a.UserID == userId).ToList();
                }

                if (quality)
                {
                    userManagement = userManagement.Where(a => a.UserTypeID.Contains("PCD") || a.UserTypeID.Contains("PRD") || a.UserTypeID.Contains("DEL") || a.UserTypeID.Contains("DEL") || a.UserTypeID.Contains("ADM") || a.UserTypeID.Contains("DEV") || a.UserTypeID.Contains("QC")).ToList();
                }
                userManagement = userManagement.OrderBy(a=> a.UserTypeID).ThenBy(a => a.UserID).ToList();

                //return Json(userManagement, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(userManagement, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }

        }
        public ActionResult crudUserManagement(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    //Db Connection string
                    string DBCon = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
                    string uid = Session["UserID"].ToString();

                    postUserManagementModel postUserManagement = JsonConvert.DeserializeObject<postUserManagementModel>(jsonData);
                    UserManagementModel userManagement = postUserManagement.UserManagement;

                    HttpPostedFileBase file = Request.Files["ImageData"];
                    HttpPostedFileBase sign = Request.Files["ImageSign"];
                    if (file != null)
                    {
                        userManagement.Image = _SystemService.ConvertToBytes(file);
                    }
                    if (sign != null)
                    {
                        userManagement.Sign = _SystemService.ConvertToBytes(sign);
                    }

                    string MSQL = "";
                    SqlConnection conn = new SqlConnection(DBCon);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                    switch (userManagement.FormAction.ToLower())
                    {
                        case "create":

                            MSQL = "Insert into Tbl_SYS_Users Select ";
                            MSQL += "'" + userManagement.UserID + "',";
                            MSQL += "'" + _SystemService.EncryptPass(userManagement.Password) + "',";
                            MSQL += "'" + userManagement.UserName + "',";
                            MSQL += "'" + userManagement.Email + "',";
                            MSQL += "'" + userManagement.UserTypeID + "',";
                            MSQL += "'" + userManagement.Phone + "',";
                            MSQL += "'" + userManagement.Area + "',";
                            MSQL += "'" + _SystemService.Vbn(userManagement.ConfidentialAccess.ToString()) + "',";
                            MSQL += "'" + userManagement.Image + "',";
                            MSQL += "'" + userManagement.Sign + "',";
                            MSQL += "'" + _SystemService.Vbn(userManagement.IsActive.ToString()) + "',";
                            MSQL += "'" + _SystemService.Vbn(userManagement.IsArchived.ToString()) + "',";
                            MSQL += "'" + DateTime.Now.ToString("yyyy-MM-dd") + "'";

                            break;

                        case "update":

                            MSQL = "Update Tbl_SYS_UserType Set ";
                            MSQL += "Password           = '" + _SystemService.EncryptPass(userManagement.Password) + "',";
                            MSQL += "UserName           = '" + userManagement.UserName + "',";
                            MSQL += "Email              = '" + userManagement.Email + "',";
                            MSQL += "UserTypeID         = '" + userManagement.UserTypeID + "',";
                            MSQL += "Phone              = '" + userManagement.Phone + "',";
                            MSQL += "Area               = '" + userManagement.Area + "',";
                            MSQL += "ConfidentialAccess =  " + _SystemService.Vbn(userManagement.ConfidentialAccess.ToString()) + ",";
                            if (userManagement.Image != null)
                            {
                                MSQL += "Image          = '" + userManagement.Image + "',";
                            }
                            if (userManagement.Sign != null)
                            {
                                MSQL += "Sign           = '" + userManagement.Sign + "',";
                            }
                            MSQL += "IsActive           =  " + _SystemService.Vbn(userManagement.IsActive.ToString()) + ",";
                            MSQL += "IsArchived         =  " + _SystemService.Vbn(userManagement.IsArchived.ToString()) + "";
                            MSQL += "Where UserID       = '" + userManagement.UserID + "'";

                            break;

                        case "archive":

                            MSQL = "exec [UserDelete] '" + userManagement.UserID + "','" + userManagement.FormAction + "'";
                            
                            break;

                        case "delete":

                            MSQL = "exec [UserDelete] '" + userManagement.UserID + "','" + userManagement.FormAction + "'";

                            break;
                    }

                    try
                    {
                        cmd.CommandText = MSQL;
                        cmd.ExecuteNonQuery();

                        return Json(userManagement, JsonRequestBehavior.AllowGet);
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
        public ActionResult crudUserImage(string userid)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    //Db Connection string
                    string DBCon = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
                    string uid = Session["UserID"].ToString();
                    byte[] imguser = null;
                    HttpPostedFileBase file = Request.Files["ImageData"];
                    UserManagementModel user = new UserManagementModel();

                    if (file != null)
                    {
                        imguser = _SystemService.ConvertToBytes(file);
                        user.Image = imguser;
                        string MSQL = "";
                        SqlConnection conn = new SqlConnection(DBCon);
                        conn.Open();

                        MSQL = "Update Tbl_SYS_Users Set ";
                        MSQL += "Image          = @img ";
                        MSQL += "Where UserID   = '" + userid + "'";

                        SqlCommand cmd = new SqlCommand(MSQL, conn);//call Stored Procedure
                        cmd.Parameters.Add(new SqlParameter("@img", user.Image));

                        try
                        {
                            cmd.ExecuteNonQuery();
                            if (uid == userid)
                            {
                                Session["Images"] = imguser;
                            }

                            return Json("Success", JsonRequestBehavior.AllowGet);
                        }
                        catch (DbEntityValidationException e)
                        {
                            Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            var errinfo = _SystemService.GetExceptionDetails(e);
                            return Json(errinfo, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("No Image", JsonRequestBehavior.AllowGet);
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
        /* --- User Roles ---*/
        public ActionResult UserRoles()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Account", "UserRoles");

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
        public ActionResult UserRolesJson(string searchFilter, string roleId, bool isActive = true)
        {
            if (Session["UserID"] != null)
            {

                string utype = Session["UserType"].ToString();

                searchFilter = _SystemService.Vf(searchFilter).ToLower();
                var userroles = _AccountService.UserTypeList().ToList();
                userroles = userroles.Where(a => (a.ID.ToLower().Contains(searchFilter) || a.UserType.ToLower().Contains(searchFilter) || a.Remark.ToLower().Contains(searchFilter))).ToList();

                if (_SystemService.Vf(roleId) != "")
                {
                    userroles = userroles.Where(a => a.ID == roleId).ToList();
                }
                if (utype != "DEV")
                {
                    userroles = userroles.Where(a => a.ID != "DEV").ToList();
                }

                userroles = userroles.OrderBy(a => a.ID).ToList();

                return Json(userroles, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }

        }
        public ActionResult crudUserRoles(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    //Db Connection string
                    string DBCon = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
                    string uid = Session["UserID"].ToString();

                    postUserRolesModel postUserRoles = JsonConvert.DeserializeObject<postUserRolesModel>(jsonData);
                    UserRolesModel userRoles = postUserRoles.UserRoles;
                    string menulist = "";
                    if (userRoles.MenuList != null && userRoles.MenuList.Length >= 5)
                    {
                        menulist = userRoles.MenuList.Substring(2, (userRoles.MenuList.Length - 4));
                        menulist = menulist.Replace("\",\"", "','");
                        menulist = "'" + menulist + "'";
                    }

                    string MSQL = "";
                    SqlConnection conn = new SqlConnection(DBCon);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                    switch (userRoles.FormAction.ToLower())
                    {
                        case "create":

                            //UserTypeAddModel userTypeAdd = new UserTypeAddModel();
                            //userTypeAdd.CategoryID = userRoles.RoleID;
                            //userTypeAdd.UserType = userRoles.RoleName;
                            //userTypeAdd.Remark = userRoles.Remarks;
                            //userTypeAdd.MenuList = userRoles.MenuList;

                            //var create = _AccountService.UpdateUserType(userTypeAdd, true, "");

                            MSQL = "Insert into Tbl_SYS_UserType Select '" + userRoles.RoleID + "','" + userRoles.RoleName + "','" + userRoles.Remarks + "'";

                            break;

                        case "update":

                            //UserTypeAddModel userTypeUpdate = new UserTypeAddModel();
                            //userTypeUpdate.CategoryID = userRoles.RoleID;
                            //userTypeUpdate.UserType = userRoles.RoleName;
                            //userTypeUpdate.Remark = userRoles.Remarks;
                            //userTypeUpdate.MenuList = userRoles.MenuList;

                            //var update = _AccountService.UpdateUserType(userTypeUpdate, true, "");

                            MSQL = "Update Tbl_SYS_UserType Set UserType='" + userRoles.RoleName + "',Remark='" + userRoles.Remarks + "' ";
                            MSQL += "Where ID='" + userRoles.RoleID + "'";

                            break;

                        case "delete":

                            //UserTypeListModel userTypeDelete = new UserTypeListModel();
                            //userTypeDelete.ID = userRoles.RoleID;
                            //userTypeDelete.UserType = userRoles.RoleName;
                            //userTypeDelete.Remark = userRoles.Remarks;
                            //userTypeDelete.MenuList = userRoles.MenuList;

                            //var delete = _AccountService.UserTypeDelete(userTypeDelete);

                            MSQL = "Delete From Tbl_SYS_UserType ";
                            MSQL += "Where ID='" + userRoles.RoleID + "'";

                            break;
                    }

                    try
                    {
                        cmd.CommandText = MSQL;
                        cmd.ExecuteNonQuery();

                        MSQL = "Delete From Tbl_SYS_MenuPreviliege	Where UserTypeID= '" + userRoles.RoleID + "'";
                        cmd.CommandText = MSQL;
                        cmd.ExecuteNonQuery();

                        if (userRoles.FormAction.ToLower() != "delete")
                        {
                            MSQL = "Insert into Tbl_SYS_MenuPreviliege ";
                            MSQL += "Select '" + userRoles.RoleID + "',a.MenuID,isnull(b.Active,0),GETDATE() ";
                            MSQL += "from Tbl_SYS_Menu a ";
                            MSQL += "left join (select MenuID,Active=1 from Tbl_SYS_Menu) b	on a.MenuID=b.MenuID and b.MenuID in ('*'," + menulist + ")";
                            cmd.CommandText = MSQL;
                            cmd.ExecuteNonQuery();
                        }

                        return Json(userRoles, JsonRequestBehavior.AllowGet);
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

        /* --- User Type ---*/
        public ActionResult UserTypeList()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.Title = "User Type List";
                Session["Layout"] = "portal";
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return View(_AccountService.UserTypeList().ToList()); // redirecting to all List
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult UserTypeAdd()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.ActiveMenu = "Add User Type";
                Session["Layout"] = "portal";
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                UserTypeAddModel _UserTypeAddModel = new UserTypeAddModel();

                return View(_UserTypeAddModel);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult UpdateUserType(UserTypeAddModel model)
        {

            if (ModelState.IsValid)
            {

                try
                {
                    _AccountService.UpdateUserType(model, true, Session["UserID"].ToString());
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "Update user type has been saved successfully";
                    return RedirectToAction("UserTypeList");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "UserTypeList", backcontroller = "Account" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "UserTypeList", backcontroller = "Account" });

            }
        }

        [HttpGet]
        public ActionResult UserTypeEdit(string id)
        {
            if (Session["UserID"] != null)
            {
                if (id != null)
                {
                    id = id.ToUpper();
                    ViewBag.ActiveMenu = "Edit User Type";
                    Session["Layout"] = "portal";
                    Session["History"] = HttpContext.Request.Url.AbsolutePath;
                    UserTypeEditModel _UserTypeEditModel = _AccountService.UserTypeEditList(id).Find(tid => tid.CategoryID == id);
                    return View(_UserTypeEditModel);
                }
                else
                {
                    return View("UserTypeList", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult UserTypeDelete(UserTypeListModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    _AccountService.UserTypeDelete(model);
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "User type has been deleted successfully";
                    return RedirectToAction("UserTypeList");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "UserTypeList", backcontroller = "Account" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "UserTypeList", backcontroller = "Account" });
            }
        }

        /* --- User ---*/
        public ActionResult UserList()
        {
            if (Session["UserID"] != null)
            {
 
                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Account", "UserList");

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

                    Session["Layout"] = "portal";
                    Session["History"] = HttpContext.Request.Url.AbsolutePath;
                    string utype = Session["UserType"].ToString();
                    var userList = _AccountService.UserList(utype).ToList();

                    return View(userList); // redirecting to all List
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult ApprovalTypeListJson()
        {

            var ApprovalType = _AccountService.ApprovalType();
            return Json(ApprovalType, JsonRequestBehavior.AllowGet);

        }
        public ActionResult UserApprovalTypeListJson(string userid, string menuid)
        {

            var UserApprovalType = _AccountService.UserApprovalType(userid,menuid);
            return Json(UserApprovalType, JsonRequestBehavior.AllowGet);

        }
        public ActionResult UserAdd()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.ActiveMenu = "Add User";
                Session["Layout"] = "portal";
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                string utype = Session["UserType"].ToString();

                UserAddModel _UserAddModel = new UserAddModel();
                _UserAddModel.UserTypeList = _AccountService.ComboUserType(utype);
                _UserAddModel.AreaList = _AccountService.ComboArea();
                _UserAddModel.ApprovalTypeList = _AccountService.ApprovalType();
                return View(_UserAddModel);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public ActionResult UserEdit(String id)
        {
            if (Session["UserID"] != null)
            {
                Session["Layout"] = "portal";
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                if (id != null)
                {
                    string utype = Session["UserType"].ToString();
                    ViewBag.ActiveMenu = "Edit User";
                    UserEditModel _UserEditModel = _AccountService.UserEditList(id).Find(uid => uid.UserID == id);
                    _UserEditModel.ReturnAction = "UserManagement";
                    _UserEditModel.UserTypeList = _AccountService.ComboUserType(utype);
                    _UserEditModel.AreaList = _AccountService.ComboArea();
                    _UserEditModel.ApprovalTypeList = _AccountService.ApprovalType();
                    return View(_UserEditModel);
                }
                else
                {
                    return View("UserList", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public ActionResult UserProfile(String id)
        {
            if (Session["UserID"] != null)
            {
                Session["Layout"] = "portal";
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                if (id != null)
                {
                    string utype = Session["UserType"].ToString();
                    ViewBag.Title = "User Profile";
                    UserEditModel _UserEditModel = _AccountService.UserEditList(id).Find(uid => uid.UserID == id);
                    _UserEditModel.ReturnAction = "UserProfile";
                    _UserEditModel.UserTypeList = _AccountService.ComboUserType(utype);
                    _UserEditModel.AreaList = _AccountService.ComboArea();
                    return View(_UserEditModel);
                }
                else
                {
                    return View("Dashboard", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [HttpGet]
        public ActionResult GetUserJson(String id)
        {
            UserEditModel _UserModel = _AccountService.UserEditList(id).Find(uid => uid.UserID == id || uid.Email == id);
            return Json(_UserModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateUser(UserAddModel model, string UserAccess, string UserApproval)
        {

            if (ModelState.IsValid)
            {

                try
                {
                    if(Session["UserID"] != null)
                    {
                        string uid = Session["UserID"].ToString();
                        HttpPostedFileBase file = Request.Files["ImageData"];
                        HttpPostedFileBase sign = Request.Files["ImageSign"];
                        if (file != null)
                        {
                            model.Image = _SystemService.ConvertToBytes(file);
                        }
                        if (sign != null)
                        {
                            model.Sign = _SystemService.ConvertToBytes(sign);
                        }
                        _AccountService.CreateUser(model, true, uid, UserAccess, UserApproval);
                        TempData["Alert"] = "Success";
                        TempData["Message"] = "Create user has been saved successfully";
                        return RedirectToAction("UserManagement");
                    } else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "UserList", backcontroller = "Account" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "UserList", backcontroller = "Account" });
            }

        }

        [HttpPost]
        public ActionResult UpdateUser(UserEditModel model, string UserAccess, string UserApproval)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    if (Session["UserID"]!= null)
                    {
                        string uid = Session["UserID"].ToString();
                        HttpPostedFileBase file = Request.Files["ImageData"];
                        HttpPostedFileBase sign = Request.Files["ImageSign"];
                        if (file != null)
                        {
                            model.Image = _SystemService.ConvertToBytes(file);
                        }
                        if (sign != null)
                        {
                            model.Sign = _SystemService.ConvertToBytes(sign);
                        }
                        _AccountService.UpdateUser(model, false, uid, UserAccess, UserApproval);
                        TempData["Alert"] = "Success";
                        TempData["Message"] = "Update user has been saved successfully";
                        if (model.ReturnAction == "UserProfile")
                        {
                            return RedirectToAction(model.ReturnAction, "Account", new { id = model.UserID });
                        }
                        else
                        {
                            return RedirectToAction(model.ReturnAction, "Account");
                        }

                    } else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    TempData["Alert"] = "Failed";
                    return RedirectToAction("UserList");
                }
            }
            else
            {
                TempData["ModelsError"] = "Error";
                return RedirectToAction("UserList");
            }

        }

        [HttpPost]
        public ActionResult UserActivate(UserListModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    _AccountService.UserActivate(model);
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "Activate user has been saved successfully";
                    return RedirectToAction("UserManagement");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "UserList", backcontroller = "Account" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "UserList", backcontroller = "Account" });
            }
        }

        [HttpPost]
        public ActionResult UserArchive(UserListModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    _AccountService.UserArchive(model);
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "User has been archived successfully";
                    return RedirectToAction("UserManagement");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "UserList", backcontroller = "Account" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "UserList", backcontroller = "Account" });
            }
        }
        [HttpPost]
        public ActionResult UserDelete(UserListModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    _AccountService.UserDelete(model);
                    TempData["Alert"] = "Success";
                    TempData["Message"] = "User has been deleted successfully";
                    return RedirectToAction("UserManagement");
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "UserList", backcontroller = "Account" });
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = "Data model description does not match, please contact the system administrator.", backaction = "UserList", backcontroller = "Account" });
            }
        }
        public string GetBaseUrl()
        {
            try
            {
                var urlschm = HttpContext.Request.Url.Scheme;
                var urlhost = HttpContext.Request.Url.Host;
                var urlport = HttpContext.Request.Url.Port;
                var baseurl = urlschm + "://" + urlhost;
                if (urlport.ToString() != "")
                {
                    baseurl += ":" + urlport;
                }

                return baseurl;

            }
            catch
            {
                string defaulturl = "";
                if (Session != null)
                {
                    defaulturl = Session["CompWebsites"].ToString();
                } else
                {
                    defaulturl = "127.0.0.0";
                }
                return defaulturl;
            }
        }

        public bool SentEmailReset(
            string useremail, string username, string urlapi
            )
        {
            bool iresult = false;
            string[] uname = username.Split('@');
            string user = uname[0].Replace("."," ").Replace("_", " ").ToUpper();

            try
            {

                string body = string.Empty;
                DateTime now = DateTime.Now;
                string copyrightyear = now.ToString("yyyy");

                using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Shared/_VSSPLayout/EmailTemplates/Account/ResetPassword.html")))
                {
                    body = reader.ReadToEnd();
                }
                
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                      | SecurityProtocolType.Tls11
                                      | SecurityProtocolType.Tls12;

                body = body.Replace("{username}", user); //replacing the required things  
                body = body.Replace("{urlapi}", urlapi);
                body = body.Replace("{appname}", Session["AppID"].ToString());
                body = body.Replace("{compid}", Session["CompID"].ToString());
                body = body.Replace("{compname}", Session["CompName"].ToString());
                body = body.Replace("{compphone}", Session["CompPhone1"].ToString());
                body = body.Replace("{compaddress}", Session["CompAddress"].ToString());
                body = body.Replace("{compcity}", Session["CompCity"].ToString());
                body = body.Replace("{compcountry}", Session["CompCountry"].ToString());
                body = body.Replace("{compwebsites}", Session["CompWebsites"].ToString());
                body = body.Replace("{copyrightyear}", copyrightyear);
                body = body.Replace("{baseurl}", GetBaseUrl());
                var emailconfig = _SystemService.GetEmailConfiguration(null);

                var senderEmail = new MailAddress(emailconfig.EmailAddress, emailconfig.SenderName);
                var receiverEmail = new MailAddress(useremail, username);
                var userid = emailconfig.EmailUserID;
                var password = emailconfig.EmailPassword;
                var sub = Session["AppID"].ToString() + " Reset Password";
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
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                TempData["Error"] = errinfo;
                iresult = false;
            }
            return iresult;
        }

        public MenuModel acccessPreviliege(string utype, string uid, string menuid)
        {

            MenuModel getAccess = _AccountService.UserMenuList(utype, utype, uid, menuid).Find(a => a.MenuID == menuid);

            return getAccess;

        }

        public void clearStockTakingEvent()
        {

            Session["InventoryNumber"] = "";
            Session["InventoryDate"] = "";
            Session["InventoryStartTime"] = "";
            Session["InventoryEndTime"] = "";
            Session["InventoryStatus"] = "";
            Session["InventoryCountTime"] = "";

        }

    }
}