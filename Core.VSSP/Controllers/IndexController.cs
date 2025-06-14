using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.Services;
using Core.VSSP.Models;

namespace Core.VSSP.Controllers
{
    public class IndexController : Controller
    {
        SystemService _SystemService = new SystemService();
        IndexService _IndexService = new IndexService();
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();


        // GET: Index
        public ActionResult MainIndex()
        {
            Session["Layout"] = "mainindex";
            //Session["History"] = HttpContext.Request.Url.AbsolutePath;

            if (Session["CompID"] == null)
            {
                return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
            }
            else
            {
                ViewBag.Title = Session["CompName"].ToString();

                return View();
            }

        }
        public ActionResult IndexHeader()
        {

            return PartialView("IndexHeader");

        }
        public ActionResult IndexFooter()
        {

            return PartialView("IndexFooter");

        }
        public ActionResult Spinner()
        {

            return PartialView("Spinner");

        }

        public ActionResult CMSList(string Category, string FormAction)
        {
            if (Session["UserID"] != null)
            {
                string action = "CMSList?Category=" + Category + "&FormAction=" + FormAction;
                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Index", action);

                if (acccessPreviliege.CanSee == false) {
                    return RedirectToAction("UnauthorizedAccess", "System");
                } 
                else 
                { 
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True","").Replace("False","disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    List<CMSListModel> _CMSListModel = _IndexService.CMSList("List", Category, "", "").ToList();

                    if (FormAction?.ToString() == "FrontEnd")
                    {
                        //Session["Layout"] = Layout;
                        Session["Layout"] = "portal";
                        Session["History"] = HttpContext.Request.Url.AbsolutePath + "?Category=" + Category + "&" + "FormAction=" + FormAction;
                    }
                    else
                    {
                        Session["Layout"] = "portal";
                        Session["History"] = HttpContext.Request.Url.AbsolutePath;
                    }

                    return View(_CMSListModel);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult CMSAdd()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.Title = "CMS List > Add Item";
                CMSCrudModel _CMSCrudModel = new CMSCrudModel();
                _CMSCrudModel.CreateDate = _SystemService.Vd(DateTime.Now.ToString("yyyy-MM-dd"));
                _CMSCrudModel.PublishedDate = _SystemService.Vd(DateTime.Now.ToString("yyyy-MM-dd"));
                _CMSCrudModel.Editor = Session["UserName"].ToString();
                _CMSCrudModel.CMSCategoryList = _IndexService.ComboCMSCategory();

                Session["Layout"] = "portal";
                return View(_CMSCrudModel);

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult CMSEdit(string ID, string startup = "")
        {
            if (Session["UserID"] != null)
            {
                ViewBag.Title = "CMS List > Edit Item";
                if (startup != "") Session["History"] = HttpContext.Request.Url;

                CMSCrudModel _CMSCrudModel = _IndexService.CMSShowData(ID, "");
                if (_CMSCrudModel.PublishedDate == "")
                {
                    _CMSCrudModel.PublishedDate = _SystemService.Vd(DateTime.Now.ToString("yyyy-MM-dd"));
                }
                _CMSCrudModel.CMSCategoryList = _IndexService.ComboCMSCategory();

                Session["Layout"] = "portal";
                return View(_CMSCrudModel);

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult CMSCreate(CMSCrudModel model)
        {

            string historyurl = Session["History"].ToString();
            int len = historyurl.Length;
            string viewurl = historyurl.Substring(1, len - 1);

            if (ModelState.IsValid)
            {

                try
                {
                    HttpPostedFileBase file = Request.Files["ImageData"];

                    if (file != null) model.Image = _SystemService.ConvertToBytes(file);
                    if (model.Publish == false) model.PublishedDate = null;

                    _IndexService.CMSCrud(model, "create");

                    TempData["Alert"] = "Success";
                    TempData["Message"] = "CMS has been create successfully";
                    return Redirect(historyurl);
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("",errinfo);
                    TempData["Alert"] = "Fail";
                    TempData["Message"] = errinfo;
                    return Redirect(historyurl);
                }

            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                ModelState.AddModelError("", "Model Not Valid");
                TempData["Alert"] = "Fail";
                TempData["Message"] = "Model Not Valid";
                return Redirect(historyurl);
            }
        }
        [HttpPost]
        public ActionResult CMSUpdate(CMSCrudModel model)
        {
            string historyurl = Session["History"].ToString();
            int len = historyurl.Length;
            string viewurl = historyurl.Substring(1, len - 1);

            if (ModelState.IsValid)
            {

                try
                {
                    HttpPostedFileBase file = Request.Files["ImageData"];
                    if (file != null) model.Image = _SystemService.ConvertToBytes(file);
                    if (model.Publish == false) model.PublishedDate = null;
                    model.Editor = Session["UserName"].ToString();

                    _IndexService.CMSCrud(model, "update");

                    TempData["Alert"] = "Success";
                    TempData["Message"] = "CMS has been update successfully";
                    return Redirect(historyurl);
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    ModelState.AddModelError("", errinfo);
                    TempData["Alert"] = "Fail";
                    TempData["Message"] = errinfo;
                    return Redirect(historyurl);
                }

            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                ModelState.AddModelError("", "Model Not Valid");
                TempData["Fail"] = "Fail";
                TempData["Message"] = "Model Not Valid";
                return Redirect(historyurl);
            }
        }
        [HttpPost]
        public ActionResult CMSDelete(string ID)
        {

            string historyurl = Session["History"].ToString();
            int len = historyurl.Length;
            string viewurl = historyurl.Substring(1, len - 1);

            try
            {
                _IndexService.CMSDelete(ID, "delete");

                TempData["Success"] = "Success";
                TempData["Message"] = "CMS has been deleted successfully";
                return Redirect(historyurl);
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                TempData["Fail"] = "Fail";
                TempData["Message"] = errinfo;
                return Redirect(historyurl);
            }

        }

        
        public ActionResult MainPartial(string FormAction)
        {
            if (Session["CompID"] == null)
            {
                return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
            }
            else
            {
                if (FormAction != "LatestNews")
                {
                    List<CMSListModel> _CMSListModel = _IndexService.CMSList("", FormAction, "", "").ToList();
                    Session["BackgroundImage"] = _CMSListModel[0].Image;
                    string partialview = "Content/" + FormAction;
                    return PartialView(partialview, _CMSListModel);
                }
                else
                {
                    List<NEWSListModel> _NEWSListModel = _IndexService.NEWSList(FormAction, "", "", "").ToList();
                    string partialview = "Content/" + FormAction;
                    return PartialView(partialview, _NEWSListModel);
                }
            }
        }
        
        public ActionResult ResetPasswordResult(bool resetresult, string formaction)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Reset Password Result";
            ViewBag.Result = resetresult;
            ViewBag.Action = formaction;
            return View("../Account/ResetResult");
        }
        public ActionResult Policy()
        {
            Session["Layout"] = "mainindex";
            Session["History"] = HttpContext.Request.Url.AbsolutePath;

            if (Session["CompID"] == null)
            {
                return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
            }
            else
            {
                ViewBag.Title = "Privacy Policy";

                List<CMSListModel> _CMSListModel = _IndexService.CMSList("List", "Policy", "", "").ToList();
                return View("Content/Policy", _CMSListModel);
            }

        }
        public ActionResult Terms()
        {
            Session["Layout"] = "mainindex";
            Session["History"] = HttpContext.Request.Url.AbsolutePath;

            if (Session["CompID"] == null)
            {
                return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
            }
            else
            {
                ViewBag.Title = "Terms & Condition";

                //List<CMSListModel> _CMSListModel = _IndexService.CMSList("List", "Terms", "", "").ToList();
                return View("Content/Terms");
            }

        }
        public ActionResult Faq()
        {
            Session["Layout"] = "mainindex";
            Session["History"] = HttpContext.Request.Url.AbsolutePath;

            if (Session["CompID"] == null)
            {
                return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
            }
            else
            {
                ViewBag.Title = "Frequently Ask Questions";

                List<CMSListModel> _CMSListModel = _IndexService.CMSList("List", "Faq", "", "").ToList();
                return View("Content/Faq", _CMSListModel);
            }

        }
    }
}