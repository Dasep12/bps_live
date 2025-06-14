using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;

namespace Core.VSSP.Controllers
{
    public class MeasurementsController : Controller
    {
        // GET: Measurements
        CryptoLibService        _CryptoLibService       = new CryptoLibService();
        AccountService          _AccountService         = new AccountService();
        SystemService           _SystemService          = new SystemService();
        MeasurementsService     _MeasurementsService    = new MeasurementsService();
        vssp_entity             vssp_db                 = new vssp_entity();

        public ActionResult Categories()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Measurements", "Categories");

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

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    var stockTakingEvent = _SystemService.GetStockTakingEvent();

                    if (stockTakingEvent != null && stockTakingEvent.InventoryStatus.Contains("in progress"))
                    {
                        ViewBag.Messages = stockTakingEvent.InventoryStatus;
                        return View("../System/SystemLocked");
                    }
                    else
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";

                        return View(exportOption);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult CategoriesListJson(string searchFilter)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Categories = from a in vssp_db.Tbl_MST_MeasurementsCategories
                             where a.CategoryId.Contains(searchFilter) || a.CategoryName.Contains(searchFilter)
                             orderby a.CategoryId
                             select new { a.CategoryId, a.CategoryName, a.Remarks, a.UserID, a.EditDate };
            return Json(Categories, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Units()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Measurements", "Units");

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

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    var stockTakingEvent = _SystemService.GetStockTakingEvent();

                    if (stockTakingEvent != null && stockTakingEvent.InventoryStatus.Contains("in progress"))
                    {
                        ViewBag.Messages = stockTakingEvent.InventoryStatus;
                        return View("../System/SystemLocked");
                    }
                    else
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";

                        return View(exportOption);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult UnitsListJson(string searchFilter)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Units = from a in vssp_db.Tbl_MST_MeasurementsUnits
                        where a.UnitLevel == 1 && (a.UnitId.Contains(searchFilter) || a.UnitName.Contains(searchFilter))
                        orderby a.UnitLevel, a.ParentId
                        select new { a.UnitId, a.UnitName, a.ParentId, a.UnitLevel, a.Remarks, a.UserID, a.EditDate };
            return Json(Units, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UnitsListLevel2Json(string unitid)
        {
            unitid = _SystemService.Vf(unitid);
            var Units = from a in vssp_db.Tbl_MST_MeasurementsUnits
                        where a.UnitLevel == 2 && a.ParentId == unitid
                        orderby a.UnitLevel, a.ParentId
                        select new { a.UnitId, a.UnitName, a.ParentId, a.UnitLevel, a.Remarks, a.UserID, a.EditDate };
            return Json(Units, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Packing()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Measurements", "Packing");

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

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    var stockTakingEvent = _SystemService.GetStockTakingEvent();

                    if (stockTakingEvent != null && stockTakingEvent.InventoryStatus.Contains("in progress"))
                    {
                        ViewBag.Messages = stockTakingEvent.InventoryStatus;
                        return View("../System/SystemLocked");
                    }
                    else
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";

                        return View(exportOption);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult PackingListJson(string searchFilter)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Packing = from a in vssp_db.Tbl_MST_MeasurementsPacking
                             where a.PackingId.Contains(searchFilter) || a.PackingName.Contains(searchFilter)
                             orderby a.PackingId
                             select new { a.PackingId, a.PackingName, a.Remarks, a.UserID, a.EditDate };
            return Json(Packing, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ImportJson(string formaction)
        {
            if (formaction == "Categories")
            {
                ImportCategoriesModel Categories = new ImportCategoriesModel();
                return Json(Categories, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "Categories-validation")
            {
                HttpFileCollectionBase files = Request.Files;
                var CategoriesUpload = _MeasurementsService.uploadCategoriesExcel(files[0]);
                return Json(CategoriesUpload, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "Units")
            {
                ImportUnitsModel Units = new ImportUnitsModel();
                return Json(Units, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "Units-validation")
            {
                HttpFileCollectionBase files = Request.Files;
                var UnitsUpload = _MeasurementsService.uploadUnitsExcel(files[0]);
                return Json(UnitsUpload, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "Packing")
            {
                ImportPackingModel Packing = new ImportPackingModel();
                return Json(Packing, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "Packing-validation")
            {
                HttpFileCollectionBase files = Request.Files;
                var PackingUpload = _MeasurementsService.uploadPackingExcel(files[0]);
                return Json(PackingUpload, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error! No Valid Action", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult crudImportJson(Boolean replace, string formaction)
        {
            if (Session["UserID"] != null)
            {

                if (formaction == "Categories")
                {
                    string userId = Session["UserID"].ToString();
                    HttpFileCollectionBase files = Request.Files;
                    var CategoriesUpload = _MeasurementsService.crudImportCategoriesExcel(replace,userId,files[0]);

                    var logs = _SystemService.LogActivitiesCRUD(userId, Session["IpAddress"].ToString(),
                                Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                "Measurements/Import", formaction, "Success");

                    return Json(CategoriesUpload, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "Units")
                {
                    string userId = Session["UserID"].ToString();
                    HttpFileCollectionBase files = Request.Files;
                    var CategoriesUpload = _MeasurementsService.crudImportUnitsExcel(replace, userId, files[0]);

                    var logs = _SystemService.LogActivitiesCRUD(userId, Session["IpAddress"].ToString(),
                                Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                "Measurements/Import", formaction, "Success");

                    return Json(CategoriesUpload, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "Packing")
                {
                    string userId = Session["UserID"].ToString();
                    HttpFileCollectionBase files = Request.Files;
                    var PackingUpload = _MeasurementsService.crudImportPackingExcel(replace, userId, files[0]);

                    var logs = _SystemService.LogActivitiesCRUD(userId, Session["IpAddress"].ToString(),
                                Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                "Measurements/Import", formaction, "Success");

                    return Json(PackingUpload, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error! No Valid Action", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }

        }
        public ActionResult crudCategories(string CategoryId, string CategoryName, string remarks, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    Tbl_MST_MeasurementsCategories Categories = new Tbl_MST_MeasurementsCategories();
                    Categories.CategoryId = CategoryId;
                    Categories.CategoryName = CategoryName;
                    Categories.Remarks = remarks;
                    Categories.UserID = uid;
                    Categories.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_MeasurementsCategories.Add(Categories);

                            break;
                        case "update":

                            var CategoriesUpdate = vssp_db.Tbl_MST_MeasurementsCategories.First(a => a.CategoryId == Categories.CategoryId);

                            CategoriesUpdate.CategoryName = Categories.CategoryName;
                            CategoriesUpdate.Remarks = Categories.Remarks;
                            CategoriesUpdate.UserID = uid;
                            CategoriesUpdate.EditDate = DateTime.Now;
                            

                            break;
                        case "delete":

                            var CategoriesDelete = vssp_db.Tbl_MST_MeasurementsCategories.First(a => a.CategoryId == Categories.CategoryId);

                            vssp_db.Tbl_MST_MeasurementsCategories.Remove(CategoriesDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        var logs = _SystemService.LogActivitiesCRUD(uid, Session["IpAddress"].ToString(),
                                    Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                    "Measurements/Categories", formAction + " " + Categories.CategoryId, "Success");

                        return Json(Categories, JsonRequestBehavior.AllowGet);
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
        public ActionResult crudUnits(string CategoryId, string UnitId, string UnitName, string ParentId, int UnitLevel, string remarks, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    Tbl_MST_MeasurementsUnits Units = new Tbl_MST_MeasurementsUnits();
                    Units.UnitId = UnitId;
                    Units.UnitName = UnitName;
                    Units.ParentId = ParentId;
                    Units.UnitLevel = UnitLevel;
                    Units.Remarks = remarks;
                    Units.UserID = uid;
                    Units.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_MeasurementsUnits.Add(Units);

                            break;
                        case "update":

                            var UnitsUpdate = vssp_db.Tbl_MST_MeasurementsUnits.First(a => a.UnitId == Units.UnitId);

                            UnitsUpdate.UnitName = Units.UnitName;
                            UnitsUpdate.ParentId = Units.ParentId;
                            UnitsUpdate.UnitLevel = Units.UnitLevel;
                            UnitsUpdate.Remarks = Units.Remarks;
                            UnitsUpdate.UserID = uid;
                            UnitsUpdate.EditDate = DateTime.Now;

                            break;
                        case "delete":

                            var UnitsDelete = vssp_db.Tbl_MST_MeasurementsUnits.First(a => a.UnitId == Units.UnitId);

                            vssp_db.Tbl_MST_MeasurementsUnits.Remove(UnitsDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        var logs = _SystemService.LogActivitiesCRUD(uid, Session["IpAddress"].ToString(),
                                    Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                    "Measurements/Units", formAction + " " + Units.UnitId, "Success");

                        return Json(Units, JsonRequestBehavior.AllowGet);
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
        public ActionResult crudPacking(string PackingId, string PackingName, string remarks, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    Tbl_MST_MeasurementsPacking Packing = new Tbl_MST_MeasurementsPacking();
                    Packing.PackingId = PackingId;
                    Packing.PackingName = PackingName;
                    Packing.Remarks = remarks;
                    Packing.UserID = uid;
                    Packing.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_MeasurementsPacking.Add(Packing);

                            break;
                        case "update":

                            var PackingUpdate = vssp_db.Tbl_MST_MeasurementsPacking.First(a => a.PackingId == Packing.PackingId);

                            PackingUpdate.PackingName = Packing.PackingName;
                            PackingUpdate.Remarks = Packing.Remarks;
                            PackingUpdate.UserID = uid;
                            PackingUpdate.EditDate = DateTime.Now;

                            break;
                        case "delete":

                            var PackingDelete = vssp_db.Tbl_MST_MeasurementsPacking.First(a => a.PackingId == Packing.PackingId);

                            vssp_db.Tbl_MST_MeasurementsPacking.Remove(PackingDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        var logs = _SystemService.LogActivitiesCRUD(uid, Session["IpAddress"].ToString(),
                                Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                "Measurements/Packing", formAction + " " + Packing.PackingId, "Success");

                        return Json(Packing, JsonRequestBehavior.AllowGet);
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
    }
}