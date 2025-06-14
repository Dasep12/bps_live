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
    public class WarehouseController : Controller
    {
        // GET: Warehouse
        AccountService      _AccountService         = new AccountService();
        CryptoLibService    _CryptoLibService       = new CryptoLibService();
        SystemService       _SystemService          = new SystemService();
        WarehouseService    _WarehouseService       = new WarehouseService();
        vssp_entity         vssp_db                 = new vssp_entity();
        public ActionResult Area()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Warehouse","Area");

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

        public ActionResult AreaListJson(string searchFilter)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var area = from a in vssp_db.Tbl_MST_WarehouseArea
                       where a.AreaID.Contains(searchFilter) || a.AreaName.Contains(searchFilter)
                       orderby a.AreaID
                       select new { a.AreaID,a.AreaName,a.Remarks,a.UserID,a.EditDate };
            return Json(area.ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Location()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Warehouse", "Location");

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

        public ActionResult LocationListJson(string searchFilter)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Location = from a in vssp_db.Tbl_MST_WarehouseLocation
                       where a.AreaId.Contains(searchFilter) || a.LocationId.Contains(searchFilter) || a.LocationName.Contains(searchFilter)
                       orderby a.AreaId, a.LocationId
                       select new { keyid=(a.AreaId + a.LocationId), a.AreaId,a.LocationId,a.LocationName,a.Remarks,a.UserId,a.EditDate };
            return Json(Location, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ImportJson(string formaction)
        {
            if (formaction == "area")
            {
                ImportAreaModel area = new ImportAreaModel();
                return Json(area, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "area-validation")
            {
                HttpFileCollectionBase files = Request.Files;
                var areaUpload = _WarehouseService.uploadAreaExcel(files[0]);
                return Json(areaUpload, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "location")
            {
                ImportLocationModel location = new ImportLocationModel();
                return Json(location, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "location-validation")
            {
                HttpFileCollectionBase files = Request.Files;
                var locationUpload = _WarehouseService.uploadLocationExcel(files[0]);
                return Json(locationUpload, JsonRequestBehavior.AllowGet);
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

                if (formaction == "area")
                {
                    string userId = Session["UserID"].ToString();
                    HttpFileCollectionBase files = Request.Files;
                    var areaUpload = _WarehouseService.crudImportAreaExcel(replace,userId,files[0]);

                    var logs = _SystemService.LogActivitiesCRUD(userId, Session["IpAddress"].ToString(),
                        Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                        "Warehouse/Area", "Import " + formaction, "Success");

                    return Json(areaUpload, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "location")
                {
                    string userId = Session["UserID"].ToString();
                    HttpFileCollectionBase files = Request.Files;
                    var areaUpload = _WarehouseService.crudImportLocationExcel(replace, userId, files[0]);

                    var logs = _SystemService.LogActivitiesCRUD(userId, Session["IpAddress"].ToString(),
                        Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                        "Warehouse/Location", "Import " + formaction, "Success");

                    return Json(areaUpload, JsonRequestBehavior.AllowGet);
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
        public ActionResult crudArea(string areaId, string areaName, string remarks, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    Tbl_MST_WarehouseArea area = new Tbl_MST_WarehouseArea();
                    area.AreaID = areaId;
                    area.AreaName = areaName;
                    area.Remarks = remarks;
                    area.UserID = uid;
                    area.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_WarehouseArea.Add(area);

                            break;
                        case "update":

                            var areaUpdate = vssp_db.Tbl_MST_WarehouseArea.First(a => a.AreaID == area.AreaID);

                            areaUpdate.AreaName = area.AreaName;
                            areaUpdate.Remarks = area.Remarks;
                            areaUpdate.UserID = uid;
                            areaUpdate.EditDate = DateTime.Now;
                            
                            break;
                        case "delete":

                            var areaDelete = vssp_db.Tbl_MST_WarehouseArea.First(a => a.AreaID == area.AreaID);

                            vssp_db.Tbl_MST_WarehouseArea.Remove(areaDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        var logs = _SystemService.LogActivitiesCRUD(uid, Session["IpAddress"].ToString(),
                                        Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                        "Warehouse/Area", formAction + " " + area.AreaID, "Success");

                        return Json(area, JsonRequestBehavior.AllowGet);
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
        public ActionResult crudlocation(string areaid, string locationId, string locationName, string remarks, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    Tbl_MST_WarehouseLocation location = new Tbl_MST_WarehouseLocation();
                    location.AreaId = areaid;
                    location.LocationId = locationId;
                    location.LocationName = locationName;
                    location.Remarks = remarks;
                    location.UserId = uid;
                    location.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_WarehouseLocation.Add(location);

                            break;
                        case "update":

                            var locationUpdate = vssp_db.Tbl_MST_WarehouseLocation.First(a => a.AreaId==location.AreaId && a.LocationId == location.LocationId);

                            locationUpdate.LocationName = location.LocationName;
                            locationUpdate.Remarks = location.Remarks;
                            locationUpdate.UserId = uid;
                            locationUpdate.EditDate = DateTime.Now;

                            break;
                        case "delete":

                            var locationDelete = vssp_db.Tbl_MST_WarehouseLocation.First(a => a.AreaId==location.AreaId && a.LocationId == location.LocationId);

                            vssp_db.Tbl_MST_WarehouseLocation.Remove(locationDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        var logs = _SystemService.LogActivitiesCRUD(uid, Session["IpAddress"].ToString(),
                                    Session["ClientCountry"].ToString(), Session["ClientCity"].ToString(),
                                    "Warehouse/Location", formAction + " " + location.LocationId, "Success");

                        return Json(location, JsonRequestBehavior.AllowGet);
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