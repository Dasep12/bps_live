using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace Core.VSSP.Controllers
{
    public class SuppliersController : Controller
    {
        // GET: Supplier
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        SuppliersService _SuppliersService = new SuppliersService();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult List()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Suppliers", "List");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Suppliers " + _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
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

        public ActionResult SupplierListJson(string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Supplier = (from a in vssp_db.Vw_MST_Supplier
                            where (a.SupplierId.Contains(searchFilter) || a.SupplierName.Contains(searchFilter)) && a.Actived == isActive
                            orderby a.SupplierId
                            select new
                            {
                                a.SupplierId,
                                a.SupplierName,
                                a.Address,
                                a.City,
                                a.Provience,
                                a.Country,
                                a.PostalCode,
                                a.TaxId,
                                a.Websites,
                                a.ContactName,
                                a.Phone,
                                a.Email,
                                a.BankId,
                                a.BankName,
                                a.AccountName,
                                a.AccountNumber,
                                a.BankAddress,
                                a.Actived,
                                a.Logo,
                                a.UserID,
                                a.EditDate
                            }).ToList();

            var jsonResult = Json(Supplier, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }
        public ActionResult SupplierContactListJson(string Supplierid, Nullable<bool> EmailContact = false)
        {
            Supplierid = _SystemService.Vf(Supplierid);
            var contact = from a in vssp_db.Tbl_MST_SupplierContact
                          where a.SupplierId == Supplierid
                          orderby a.SupplierId
                          select new { ContactId = a.ContactName, a.SupplierId, a.ContactName, a.Organization, a.Position, a.Phone1, a.Phone2, a.Fax, a.Email, a.ReceiveOrder, a.ReceiveInvoice };

            if (EmailContact == true)
            {
                contact = contact.Where(a => a.ReceiveOrder == true);
            }

            return Json(contact, JsonRequestBehavior.AllowGet);
        }
        public ActionResult KanbanCycleListJson(string key, string month)
        {
            key = _SystemService.Vf(key);
            month = _SystemService.Vf(month);
            Nullable<DateTime> dates = null;

            try
            {
                if (month != "")
                {
                    string[] arr = month.Split('/');
                    dates = Convert.ToDateTime(arr[1] + "-" + arr[0] + "-01");
                }

                var RawMaterialsKanban = from a in vssp_db.Tbl_MST_KanbanCycle
                                         where a.SupplierId == key
                                         orderby a.SupplierId, a.StartDate descending
                                         select new { a.SupplierId, a.Cycle1, a.Cycle2, a.Cycle3, a.CycleTime, a.StartDate, a.EndDate, a.UserId, a.EditDate };

                if (_SystemService.Vf(dates.ToString()) != "")
                {
                    RawMaterialsKanban = RawMaterialsKanban.Where(a => a.StartDate <= dates && (a.EndDate ?? dates) >= dates);
                }

                return Json(RawMaterialsKanban, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SupplierCostCenterListJson(string Supplierid)
        {
            Supplierid = _SystemService.Vf(Supplierid);
            var CostCenter = from a in vssp_db.Tbl_MST_SupplierCostCenter
                             where a.SupplierId == Supplierid
                             orderby a.SupplierId
                             select new { CostCenterId = a.CostId, a.SupplierId, a.CostId, a.CostName, a.UserId, a.EditDate };
            return Json(CostCenter, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SupplierBankAccountListJson(string Supplierid)
        {
            Supplierid = _SystemService.Vf(Supplierid);
            var BankAccount = from a in vssp_db.Tbl_MST_SupplierBankAccount
                              join b in vssp_db.Tbl_MST_Bank on a.BankId equals b.BankId into bank
                              from b in bank.DefaultIfEmpty()
                              where a.SupplierId == Supplierid
                              orderby a.SupplierId
                              select new { b.BankName, a.SupplierId, a.BankId, a.BankAddress, a.AccountNumber, a.AccountName, a.StartDate, a.EndDate, a.UserId, a.EditDate };

            return Json(BankAccount, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RawMaterials()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Suppliers", "RawMaterials");

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

        public ActionResult RawMaterialsListJson(string searchFilter, string FormAction = "", string SupplierId = "",
                                        string PartNumber = "", string UniqueNumber = "", string UniqueNotInclude = null,
                                        string PartNotInclude = null, bool noprice = false, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            if (searchFilter != "validator")
            {

                vssp_db.Database.CommandTimeout = 0;

                var RawMaterials = (from a in vssp_db.Vw_MST_PartRawMaterials
                                    where a.Actived == isActive && (a.SupplierId.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter))
                                    orderby a.SupplierId, a.PartNumber
                                    select new
                                    {
                                        a.RawMaterialKey,
                                        a.SupplierId,
                                        a.SupplierName,
                                        a.PartNumber,
                                        a.PartNumberSupplier,
                                        a.UniqueNumber,
                                        a.PartName,
                                        a.PartModel,
                                        a.CategoryId,
                                        a.PackingId,
                                        a.AreaId,
                                        a.LocationId,
                                        a.UnitLevel1,
                                        a.MinStock,
                                        a.MaxStock,
                                        a.StockKanban,
                                        a.StockQty,
                                        a.UnitLevel2,
                                        a.UnitQty,
                                        a.SafetyHours,
                                        a.SSP,
                                        a.ProcessName,
                                        a.Price,
                                        a.EndDate,
                                        a.Expired,
                                        a.Actived,
                                        IsActived = a.Actived,
                                        a.UserId,
                                        a.EditDate
                                    }).Distinct().ToList();

                if (_SystemService.Vf(SupplierId) != "")
                {
                    RawMaterials = RawMaterials.Where(a => a.SupplierId == SupplierId).ToList();
                }

                if (noprice)
                {
                    RawMaterials = RawMaterials.Where(a => a.Price == null).ToList();
                }
                if (UniqueNotInclude != null)
                {
                    var exceptionList = new List<string>();
                    JsonTextReader reader = new JsonTextReader(new StringReader(UniqueNotInclude));
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.TokenType.ToString() == "String")
                            {
                                exceptionList.Add(reader.Value.ToString());
                            }
                        }
                    }
                    RawMaterials = RawMaterials.Where(a => !exceptionList.Contains(a.UniqueNumber)).ToList();
                }

                if (PartNotInclude != null)
                {
                    var exceptionList = new List<string>();
                    JsonTextReader reader = new JsonTextReader(new StringReader(PartNotInclude));
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.TokenType.ToString() == "String")
                            {
                                exceptionList.Add(reader.Value.ToString());
                            }
                        }
                    }
                    RawMaterials = RawMaterials.Where(a => !exceptionList.Contains(a.PartNumber)).ToList();
                }

                return Json(RawMaterials, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var RawMaterials = new object();

                switch (FormAction)
                {
                    case "Create":

                        RawMaterials = from a in vssp_db.Vw_MST_PartRawMaterials
                                       where a.SupplierId == SupplierId && (a.PartNumber == PartNumber || a.UniqueNumber == UniqueNumber)
                                       orderby a.SupplierId, a.PartNumber
                                       select new { a.RawMaterialKey, a.SupplierId, a.SupplierName, a.PartNumber, a.PartNumberSupplier, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.Actived, a.UserId, a.EditDate };
                        break;

                    case "Update":

                        RawMaterials = from a in vssp_db.Vw_MST_PartRawMaterials
                                       where a.PartNumber != PartNumber && a.SupplierId == SupplierId && a.UniqueNumber == UniqueNumber
                                       orderby a.SupplierId, a.PartNumber
                                       select new { a.RawMaterialKey, a.SupplierId, a.SupplierName, a.PartNumber, a.PartNumberSupplier, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.Actived, a.UserId, a.EditDate };
                        break;

                    default:

                        RawMaterials = from a in vssp_db.Vw_MST_PartRawMaterials
                                       where a.SupplierId == "*"
                                       orderby a.SupplierId, a.PartNumber
                                       select new { a.RawMaterialKey, a.SupplierId, a.SupplierName, a.PartNumber, a.PartNumberSupplier, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.Actived, a.UserId, a.EditDate };

                        break;
                }
                return Json(RawMaterials, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RawMaterialsByLineListJson(string searchFilter, string FormAction = "", string SupplierId = "", string LineId = "",
                                        string PartNumber = "", string UniqueNumber = "", string UniqueNotInclude = null,
                                        string PartNotInclude = null, bool noprice = false, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            if (searchFilter != "validator")
            {
                //var RawMaterials = (from a in vssp_db.Tbl_MST_PartRawMaterials
                //                    join b in vssp_db.Vw_TRS_Stock on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                //                    from b in part.DefaultIfEmpty()
                //                    join c in vssp_db.Tbl_MST_SpecialSupplyPart on a.SSP equals c.Id into SSP
                //                    from c in SSP.DefaultIfEmpty()
                //                    join d in vssp_db.Tbl_MST_PartBillOfMaterialsDetails on new { a.PartNumber } equals new { d.PartNumber } into bom
                //                    from d in bom.DefaultIfEmpty()
                //                    join e in vssp_db.Vw_MST_PartRawMaterialsPrice on new { a.SupplierId, a.PartNumber } equals new { e.SupplierId, e.PartNumber } into price
                //                    from e in price.DefaultIfEmpty()
                //                    where a.Actived == isActive && (a.SupplierId.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter))
                //                    orderby a.SupplierId, a.PartNumber


                var RawMaterials = (from a in vssp_db.Vw_MST_PartRawMaterialsByLine
                                    where a.Actived == isActive && (a.SupplierId.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter))
                                    orderby a.SupplierId, a.PartNumber
                                    select new
                                    {
                                        a.RawMaterialKey,
                                        a.SupplierId,
                                        a.SupplierName,
                                        a.LineId,
                                        a.PartNumber,
                                        a.PartNumberSupplier,
                                        a.UniqueNumber,
                                        a.PartName,
                                        a.PartModel,
                                        a.CategoryId,
                                        a.PackingId,
                                        a.AreaId,
                                        a.LocationId,
                                        a.UnitLevel1,
                                        a.MinStock,
                                        a.MaxStock,
                                        a.StockKanban,
                                        a.StockQty,
                                        a.UnitLevel2,
                                        a.UnitQty,
                                        a.SafetyHours,
                                        a.SSP,
                                        a.ProcessName,
                                        a.Price,
                                        a.EndDate,
                                        a.Expired,
                                        a.Actived,
                                        a.UserId,
                                        a.EditDate
                                    }).Distinct().ToList();

                if (_SystemService.Vf(SupplierId) != "")
                {
                    RawMaterials = RawMaterials.Where(a => a.SupplierId == SupplierId).ToList();
                }

                if (_SystemService.Vf(LineId) != "")
                {
                    RawMaterials = RawMaterials.Where(a => a.LineId == LineId).ToList();
                }
                if (noprice)
                {
                    RawMaterials = RawMaterials.Where(a => a.Price == null).ToList();
                }
                if (UniqueNotInclude != null)
                {
                    var exceptionList = new List<string>();
                    JsonTextReader reader = new JsonTextReader(new StringReader(UniqueNotInclude));
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.TokenType.ToString() == "String")
                            {
                                exceptionList.Add(reader.Value.ToString());
                            }
                        }
                    }
                    RawMaterials = RawMaterials.Where(a => !exceptionList.Contains(a.UniqueNumber)).ToList();
                }

                if (PartNotInclude != null)
                {
                    var exceptionList = new List<string>();
                    JsonTextReader reader = new JsonTextReader(new StringReader(PartNotInclude));
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.TokenType.ToString() == "String")
                            {
                                exceptionList.Add(reader.Value.ToString());
                            }
                        }
                    }
                    RawMaterials = RawMaterials.Where(a => !exceptionList.Contains(a.PartNumber)).ToList();
                }

                return Json(RawMaterials, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var RawMaterials = new object();

                switch (FormAction)
                {
                    case "Create":

                        RawMaterials = from a in vssp_db.Vw_MST_PartRawMaterialsByLine
                                       where a.SupplierId == SupplierId && (a.PartNumber == PartNumber || a.UniqueNumber == UniqueNumber)
                                       orderby a.SupplierId, a.PartNumber
                                       select new { a.RawMaterialKey, a.SupplierId, a.SupplierName, a.PartNumber, a.PartNumberSupplier, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.Actived, a.UserId, a.EditDate };
                        break;

                    case "Update":

                        RawMaterials = from a in vssp_db.Vw_MST_PartRawMaterialsByLine
                                       where a.PartNumber != PartNumber && a.SupplierId == SupplierId && a.UniqueNumber == UniqueNumber
                                       orderby a.SupplierId, a.PartNumber
                                       select new { a.RawMaterialKey, a.SupplierId, a.SupplierName, a.PartNumber, a.PartNumberSupplier, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.Actived, a.UserId, a.EditDate };
                        break;

                    default:

                        RawMaterials = from a in vssp_db.Vw_MST_PartRawMaterialsByLine
                                       where a.SupplierId == "*"
                                       orderby a.SupplierId, a.PartNumber
                                       select new { a.RawMaterialKey, a.SupplierId, a.SupplierName, a.PartNumber, a.PartNumberSupplier, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.Actived, a.UserId, a.EditDate };

                        break;
                }
                return Json(RawMaterials, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RawMaterialsPriceListJson(string key)
        {
            key = _SystemService.Vf(key);
            var RawMaterialsPrice = (from a in vssp_db.Tbl_MST_PartRawMaterialsPrice
                                    where (a.SupplierId + a.PartNumber) == key
                                    orderby a.SupplierId, a.PartNumber, a.StartDate descending
                                    select new { RawMaterialKey = (a.SupplierId + a.PartNumber), PriceId = a.StartDate, a.SupplierId, a.PartNumber, a.StartDate, a.EndDate, a.Price, a.UserId, a.EditDate }).ToList();
            return Json(RawMaterialsPrice, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RawMaterialsCostCenterListJson(string key)
        {
            key = _SystemService.Vf(key);
            var RawMaterialsCostCenter = (from a in vssp_db.Tbl_MST_PartRawMaterialsCostCenter
                                         where (a.SupplierId + a.PartNumber) == key
                                         orderby a.SupplierId, a.PartNumber, a.StartDate descending
                                         select new { RawMaterialKey = (a.SupplierId + a.PartNumber), CostCenterId = a.StartDate, a.SupplierId, a.PartNumber, a.StartDate, a.EndDate, a.CostId, a.ClassificationId, a.PaymentId, a.CategoryId, a.UserId, a.EditDate }).ToList();
            return Json(RawMaterialsCostCenter, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ImportJson(string formaction, string canConfidential)
        {
            try
            {
                if (formaction == "Supplier")
                {
                    ImportSuppliersListModel List = new ImportSuppliersListModel();
                    return Json(List, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "Supplier-validation")
                {
                    HttpFileCollectionBase files = Request.Files;
                    var ListUpload = _SuppliersService.uploadSupplierExcel(files[0]);
                    return Json(ListUpload, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "RawMaterial")
                {
                    ImportRawMaterialModel RawMaterials = new ImportRawMaterialModel();
                    return Json(RawMaterials, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "RawMaterial-validation")
                {
                    HttpFileCollectionBase files = Request.Files;
                    var RawMaterialsUpload = _SuppliersService.uploadRawMaterialExcel(files[0], canConfidential);
                    return Json(RawMaterialsUpload, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error! No Valid Action", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult crudImportJson(Boolean replace, string formaction, string canConfidential)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    if (formaction == "Supplier")
                    {
                        string userId = Session["UserID"].ToString();
                        HttpFileCollectionBase files = Request.Files;
                        var ListUpload = _SuppliersService.crudImportSupplierExcel(replace, userId, files[0]);
                        return Json(ListUpload, JsonRequestBehavior.AllowGet);
                    }
                    else
                    if (formaction == "RawMaterial")
                    {
                        string userId = Session["UserID"].ToString();
                        HttpFileCollectionBase files = Request.Files;
                        var ListUpload = _SuppliersService.crudImportRawMaterialExcel(replace, userId, files[0], canConfidential);
                        return Json(ListUpload, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error! No Valid Action", JsonRequestBehavior.AllowGet);
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
        public ActionResult crudSupplierList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();
                    HttpFileCollectionBase files = Request.Files;
                    HttpPostedFileBase file = null;
                    for (int i = 0; i < files.Count; i++)
                    {
                        file = files[i];
                    }

                    PostSupplierModel postSupplier = JsonConvert.DeserializeObject<PostSupplierModel>(jsonData);
                    Tbl_MST_Supplier Supplier = postSupplier.Supplier;
                    List<crud_SupplierContact> SupplierContact = postSupplier.Suppliercontact;
                    List<crud_KanbanCycle> KanbanCycle = postSupplier.KanbanCycle;
                    List<crud_SupplierCostCenter> SupplierCostCenter = postSupplier.SupplierCostCenter;
                    List<crud_SupplierBankAccount> SupplierBankAccount = postSupplier.SupplierBankAccount;

                    string formAction = postSupplier.formAction.ToLower();

                    if (file != null)
                    {
                        Supplier.Logo = _SystemService.ConvertToBytes(file);
                    }

                    Tbl_MST_Supplier ListSupplier = new Tbl_MST_Supplier();
                    ListSupplier.SupplierId = Supplier.SupplierId;
                    ListSupplier.SupplierName = Supplier.SupplierName;
                    ListSupplier.Address = Supplier.Address;
                    ListSupplier.City = Supplier.City;
                    ListSupplier.Provience = Supplier.Provience;
                    ListSupplier.Country = Supplier.Country;
                    ListSupplier.PostalCode = Supplier.PostalCode;
                    ListSupplier.Websites = Supplier.Websites;
                    ListSupplier.TaxId = Supplier.TaxId;
                    ListSupplier.Logo = Supplier.Logo;
                    ListSupplier.Actived = Supplier.Actived;
                    ListSupplier.UserID = uid;
                    ListSupplier.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_Supplier.Add(ListSupplier);

                            /* crud contacts */
                            crudSupplierContact(SupplierContact, Supplier.SupplierId);

                            /* crud Kanbans */
                            crudKanbanCycle(KanbanCycle, Supplier.SupplierId, uid);

                            /* crud CostCenters */
                            crudSupplierCostCenter(SupplierCostCenter, Supplier.SupplierId, uid);

                            /* crud BankAccounts */
                            crudSupplierBankAccount(SupplierBankAccount, Supplier.SupplierId, uid);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_Supplier.First(a => a.SupplierId == Supplier.SupplierId);

                            ListUpdate.SupplierName = Supplier.SupplierName;
                            ListUpdate.Address = Supplier.Address;
                            ListUpdate.City = Supplier.City;
                            ListUpdate.Provience = Supplier.Provience;
                            ListUpdate.Country = Supplier.Country;
                            ListUpdate.PostalCode = Supplier.PostalCode;
                            ListUpdate.Websites = Supplier.Websites;
                            ListUpdate.TaxId = Supplier.TaxId;
                            ListUpdate.Actived = Supplier.Actived;
                            if (Supplier.Logo != null)
                            {
                                ListUpdate.Logo = Supplier.Logo;
                            }
                            ListUpdate.UserID = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud contacts */
                            crudSupplierContact(SupplierContact, Supplier.SupplierId);

                            /* crud Kanbans */
                            crudKanbanCycle(KanbanCycle, Supplier.SupplierId, uid);

                            /* crud CostCenters */
                            crudSupplierCostCenter(SupplierCostCenter, Supplier.SupplierId, uid);

                            /* crud BankAccounts */
                            crudSupplierBankAccount(SupplierBankAccount, Supplier.SupplierId, uid);

                            break;

                        case "delete":

                            /* remove existing Kanbans */
                            var deleteKanban = from a in vssp_db.Tbl_MST_KanbanCycle
                                               where a.SupplierId == Supplier.SupplierId
                                               select a;

                            deleteKanban.ForEach(Kanbans =>
                            {

                                var deleteCycle = from a in vssp_db.Tbl_MST_KanbanCycleTime
                                                  where a.SupplierId == Supplier.SupplierId && a.StartDate == Kanbans.StartDate
                                                  select a;

                                deleteCycle.ForEach(Cycles =>
                                {
                                    vssp_db.Tbl_MST_KanbanCycleTime.Remove(Cycles);
                                });

                                vssp_db.Tbl_MST_KanbanCycle.Remove(Kanbans);

                            });

                            /* remove existing CostCenter */
                            var CostCenterDelete = from a in vssp_db.Tbl_MST_SupplierCostCenter
                                                   where a.SupplierId == Supplier.SupplierId
                                                   select a;

                            foreach (var CostCenter in CostCenterDelete)
                            {
                                vssp_db.Tbl_MST_SupplierCostCenter.Remove(CostCenter);
                            }

                            /* remove existing BankAccount */
                            var BankAccountDelete = from a in vssp_db.Tbl_MST_SupplierBankAccount
                                                    where a.SupplierId == Supplier.SupplierId
                                                    select a;

                            foreach (var BankAccount in BankAccountDelete)
                            {
                                vssp_db.Tbl_MST_SupplierBankAccount.Remove(BankAccount);
                            }

                            /* remove existing contact */
                            var ContactDelete = from a in vssp_db.Tbl_MST_SupplierContact
                                                where a.SupplierId == Supplier.SupplierId
                                                select a;

                            foreach (var contact in ContactDelete)
                            {
                                vssp_db.Tbl_MST_SupplierContact.Remove(contact);
                            }

                            /* remove existing supplier */
                            var ListDelete = vssp_db.Tbl_MST_Supplier.First(a => a.SupplierId == Supplier.SupplierId);
                            vssp_db.Tbl_MST_Supplier.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(Supplier, JsonRequestBehavior.AllowGet);
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

        public void crudSupplierContact(List<crud_SupplierContact> SupplierContacts, string SupplierId)
        {

            foreach (var contacts in SupplierContacts)
            {
                if (contacts.RowStatus != null)
                {
                    switch (contacts.RowStatus.ToLower())
                    {
                        case "create":

                            /* create contacts */
                            Tbl_MST_SupplierContact ListContact = new Tbl_MST_SupplierContact();
                            ListContact.SupplierId = SupplierId;
                            ListContact.ContactName = contacts.ContactName;
                            ListContact.Organization = contacts.Organization;
                            ListContact.Position = contacts.Position;
                            ListContact.Phone1 = contacts.Phone1;
                            ListContact.Phone2 = contacts.Phone2;
                            ListContact.Fax = contacts.Fax;
                            ListContact.Email = contacts.Email;
                            ListContact.ReceiveOrder = contacts.ReceiveOrder;
                            ListContact.ReceiveInvoice = contacts.ReceiveInvoice;

                            vssp_db.Tbl_MST_SupplierContact.Add(ListContact);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_SupplierContact.First(a => a.SupplierId == SupplierId && a.ContactName == contacts.ContactId);

                            ListUpdate.ContactName = contacts.ContactName;
                            ListUpdate.Organization = contacts.Organization;
                            ListUpdate.Position = contacts.Position;
                            ListUpdate.Phone1 = contacts.Phone1;
                            ListUpdate.Phone2 = contacts.Phone2;
                            ListUpdate.Fax = contacts.Fax;
                            ListUpdate.Email = contacts.Email;
                            ListUpdate.ReceiveOrder = contacts.ReceiveOrder;
                            ListUpdate.ReceiveInvoice = contacts.ReceiveInvoice;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_MST_SupplierContact.First(a => a.SupplierId == SupplierId && a.ContactName == contacts.ContactId);

                            vssp_db.Tbl_MST_SupplierContact.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        public void crudSupplierCostCenter(List<crud_SupplierCostCenter> SupplierCostCenters, string SupplierId, string UserId)
        {
            if (SupplierCostCenters != null)
            {
                foreach (var CostCenters in SupplierCostCenters)
                {
                    if (CostCenters.RowStatus != null)
                    {
                        switch (CostCenters.RowStatus.ToLower())
                        {
                            case "create":

                                /* create CostCenters */
                                Tbl_MST_SupplierCostCenter ListCostCenter = new Tbl_MST_SupplierCostCenter();
                                ListCostCenter.SupplierId = SupplierId;
                                ListCostCenter.CostId = CostCenters.CostId;
                                ListCostCenter.CostName = CostCenters.CostName;
                                ListCostCenter.UserId = UserId;
                                ListCostCenter.EditDate = DateTime.Now;

                                vssp_db.Tbl_MST_SupplierCostCenter.Add(ListCostCenter);

                                break;

                            case "update":

                                var ListUpdate = vssp_db.Tbl_MST_SupplierCostCenter.First(a => a.SupplierId == SupplierId && a.CostId == CostCenters.CostId);

                                ListUpdate.CostName = CostCenters.CostName;
                                ListUpdate.UserId = UserId;
                                ListUpdate.EditDate = DateTime.Now;

                                break;

                            case "delete":

                                var ListDelete = vssp_db.Tbl_MST_SupplierCostCenter.First(a => a.SupplierId == SupplierId && a.CostId == CostCenters.CostId);

                                vssp_db.Tbl_MST_SupplierCostCenter.Remove(ListDelete);

                                break;
                        }
                    }
                }
            }
        }

        public void crudSupplierBankAccount(List<crud_SupplierBankAccount> SupplierBankAccounts, string SupplierId, string UserId)
        {
            if (SupplierBankAccounts != null)
            {
                foreach (var BankAccounts in SupplierBankAccounts)
                {
                    if (BankAccounts.RowStatus != null)
                    {
                        switch (BankAccounts.RowStatus.ToLower())
                        {
                            case "create":

                                /* create BankAccounts */
                                Tbl_MST_SupplierBankAccount ListBankAccount = new Tbl_MST_SupplierBankAccount();
                                ListBankAccount.SupplierId = SupplierId;
                                ListBankAccount.BankId = BankAccounts.BankId;
                                ListBankAccount.BankAddress = BankAccounts.BankAddress;
                                ListBankAccount.AccountNumber = BankAccounts.AccountNumber;
                                ListBankAccount.AccountName = BankAccounts.AccountName;
                                ListBankAccount.StartDate = BankAccounts.StartDate;
                                ListBankAccount.EndDate = BankAccounts.EndDate;
                                ListBankAccount.UserId = UserId;
                                ListBankAccount.EditDate = DateTime.Now;

                                vssp_db.Tbl_MST_SupplierBankAccount.Add(ListBankAccount);

                                break;

                            case "update":

                                var ListUpdate = vssp_db.Tbl_MST_SupplierBankAccount.First(a => a.SupplierId == SupplierId && a.BankId == BankAccounts.BankId && a.StartDate == BankAccounts.StartDate);

                                ListUpdate.BankAddress = BankAccounts.BankAddress;
                                ListUpdate.AccountNumber = BankAccounts.AccountNumber;
                                ListUpdate.AccountName = BankAccounts.AccountName;
                                ListUpdate.EndDate = BankAccounts.EndDate;
                                ListUpdate.UserId = UserId;
                                ListUpdate.EditDate = DateTime.Now;

                                break;

                            case "delete":

                                var ListDelete = vssp_db.Tbl_MST_SupplierBankAccount.First(a => a.SupplierId == SupplierId && a.BankId == BankAccounts.BankId && a.StartDate == BankAccounts.StartDate);

                                vssp_db.Tbl_MST_SupplierBankAccount.Remove(ListDelete);

                                break;
                        }
                    }
                }
            }
        }
        public ActionResult crudRawMaterialList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostRawMaterialModel postRawMaterial = JsonConvert.DeserializeObject<PostRawMaterialModel>(jsonData);
                    Tbl_MST_PartRawMaterials RawMaterial = postRawMaterial.RawMaterial;
                    List<crud_PartRawMaterialsCostCenter> RawMaterialCostCenter = postRawMaterial.RawMaterialCostCenter;
                    List<crud_PartRawMaterialsPrice> RawMaterialPrice = postRawMaterial.RawMaterialPrice;
                    string formAction = postRawMaterial.formAction.ToLower();

                    Tbl_MST_PartRawMaterials ListRawMaterial = new Tbl_MST_PartRawMaterials();
                    ListRawMaterial.SupplierId = RawMaterial.SupplierId;
                    ListRawMaterial.PartNumber = RawMaterial.PartNumber;
                    ListRawMaterial.PartNumberSupplier = RawMaterial.PartNumberSupplier;
                    ListRawMaterial.UniqueNumber = RawMaterial.UniqueNumber;
                    ListRawMaterial.PartName = RawMaterial.PartName;
                    ListRawMaterial.PartModel = RawMaterial.PartModel;
                    ListRawMaterial.CategoryId = RawMaterial.CategoryId;
                    ListRawMaterial.PackingId = RawMaterial.PackingId;
                    ListRawMaterial.AreaId = RawMaterial.AreaId;
                    ListRawMaterial.LocationId = RawMaterial.LocationId;
                    ListRawMaterial.UnitLevel1 = RawMaterial.UnitLevel1;
                    ListRawMaterial.UnitLevel2 = RawMaterial.UnitLevel2;
                    ListRawMaterial.UnitQty = RawMaterial.UnitQty;
                    ListRawMaterial.SafetyHours = RawMaterial.SafetyHours;
                    ListRawMaterial.SSP = RawMaterial.SSP;
                    ListRawMaterial.Actived = RawMaterial.Actived;
                    ListRawMaterial.UserId = uid;
                    ListRawMaterial.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            RawMaterial.PartNumber = _SystemService.Vf(RawMaterial.PartNumber);
                            RawMaterial.PartNumberSupplier = _SystemService.Vf(RawMaterial.PartNumberSupplier);
                            RawMaterial.UniqueNumber = _SystemService.Vf(RawMaterial.UniqueNumber);

                            ListRawMaterial.PartNumber = RawMaterial.PartNumber;
                            ListRawMaterial.PartNumberSupplier = RawMaterial.PartNumberSupplier;
                            ListRawMaterial.UniqueNumber = RawMaterial.UniqueNumber;

                            vssp_db.Tbl_MST_PartRawMaterials.Add(ListRawMaterial);

                            /* crud Prices */
                            crudRawMaterialPrice(RawMaterialPrice, RawMaterial.SupplierId, RawMaterial.PartNumber, uid);

                            /* crud CostCenters */
                            crudRawMaterialCostCenter(RawMaterialCostCenter, RawMaterial.SupplierId, RawMaterial.PartNumber, uid);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_PartRawMaterials.First(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);

                            ListUpdate.SupplierId = RawMaterial.SupplierId;
                            ListUpdate.PartNumber = RawMaterial.PartNumber;
                            ListUpdate.PartNumberSupplier = RawMaterial.PartNumberSupplier;
                            ListUpdate.UniqueNumber = RawMaterial.UniqueNumber;
                            ListUpdate.PartName = RawMaterial.PartName;
                            ListUpdate.PartModel = RawMaterial.PartModel;
                            ListUpdate.CategoryId = RawMaterial.CategoryId;
                            ListUpdate.PackingId = RawMaterial.PackingId;
                            ListUpdate.AreaId = RawMaterial.AreaId;
                            ListUpdate.LocationId = RawMaterial.LocationId;
                            ListUpdate.UnitLevel1 = RawMaterial.UnitLevel1;
                            ListUpdate.UnitLevel2 = RawMaterial.UnitLevel2;
                            ListUpdate.UnitQty = RawMaterial.UnitQty;
                            ListUpdate.SafetyHours = RawMaterial.SafetyHours;
                            ListUpdate.SSP = RawMaterial.SSP;
                            ListUpdate.Actived = RawMaterial.Actived;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Prices */
                            crudRawMaterialPrice(RawMaterialPrice, RawMaterial.SupplierId, RawMaterial.PartNumber, uid);

                            /* crud CostCenters */
                            crudRawMaterialCostCenter(RawMaterialCostCenter, RawMaterial.SupplierId, RawMaterial.PartNumber, uid);

                            /* crud moving stock ssp */
                            if (_SystemService.Vf(RawMaterial.SSP) != "")
                            {
                                //ssp type
                                var sspstock = vssp_db.Tbl_MST_SpecialSupplyPart.FirstOrDefault(a => a.Id == RawMaterial.SSP);
                                if(sspstock != null)
                                {
                                    if(sspstock.SSPStock == true && sspstock.DeliveryRawMaterial == false)
                                    {
                                        //moving stock raw to stock ssp
                                        var exisstockraw = vssp_db.Tbl_TRS_Stock.FirstOrDefault(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);
                                        if (exisstockraw != null)
                                        {
                                            var exisstockssp = vssp_db.Tbl_TRS_StockSSP.FirstOrDefault(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);
                                            if (exisstockssp == null)
                                            {
                                                Tbl_TRS_StockSSP _StockSSP = new Tbl_TRS_StockSSP();
                                                _StockSSP.SupplierId = exisstockraw.SupplierId;
                                                _StockSSP.PartNumber = exisstockraw.PartNumber;
                                                _StockSSP.MinStock = exisstockraw.MinStock;
                                                _StockSSP.MaxStock = exisstockraw.MaxStock;
                                                _StockSSP.StockKanban = exisstockraw.StockKanban;
                                                _StockSSP.StockQty = exisstockraw.StockQty;
                                                _StockSSP.LastUpdate = DateTime.Now;

                                                vssp_db.Tbl_TRS_StockSSP.Add(_StockSSP);
                                            }
                                            vssp_db.Tbl_TRS_Stock.Remove(exisstockraw);
                                        }
                                    }
                                    else if (sspstock.SSPStock == false && sspstock.DeliveryRawMaterial == true)
                                    {
                                        //moving stock raw to stock ssp
                                        var exisstockssp = vssp_db.Tbl_TRS_StockSSP.FirstOrDefault(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);
                                        if (exisstockssp != null)
                                        {
                                            var exisstockraw = vssp_db.Tbl_TRS_Stock.FirstOrDefault(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);
                                            if (exisstockraw == null)
                                            {
                                                Tbl_TRS_Stock _StockRAW = new Tbl_TRS_Stock();
                                                _StockRAW.SupplierId = exisstockssp.SupplierId;
                                                _StockRAW.PartNumber = exisstockssp.PartNumber;
                                                _StockRAW.MinStock = exisstockssp.MinStock;
                                                _StockRAW.MaxStock = exisstockssp.MaxStock;
                                                _StockRAW.StockKanban = exisstockssp.StockKanban;
                                                _StockRAW.StockQty = exisstockssp.StockQty;
                                                _StockRAW.LastUpdate = DateTime.Now;

                                                vssp_db.Tbl_TRS_Stock.Add(_StockRAW);
                                            }
                                            vssp_db.Tbl_TRS_StockSSP.Remove(exisstockssp);
                                        }
                                    }

                                }
                            }
                            else
                            {
                                //moving stock ssp to stock raw
                                var exisstockssp = vssp_db.Tbl_TRS_StockSSP.FirstOrDefault(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);
                                if (exisstockssp != null)
                                {
                                    var exisstockraw = vssp_db.Tbl_TRS_Stock.FirstOrDefault(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);
                                    if (exisstockraw == null)
                                    {
                                        Tbl_TRS_Stock _StockRaw = new Tbl_TRS_Stock();
                                        _StockRaw.SupplierId = exisstockssp.SupplierId;
                                        _StockRaw.PartNumber = exisstockssp.PartNumber;
                                        _StockRaw.MinStock = exisstockssp.MinStock;
                                        _StockRaw.MaxStock = exisstockssp.MaxStock;
                                        _StockRaw.StockKanban = exisstockssp.StockKanban;
                                        _StockRaw.StockQty = exisstockssp.StockQty;
                                        _StockRaw.LastUpdate = DateTime.Now;

                                        vssp_db.Tbl_TRS_Stock.Add(_StockRaw);
                                    }
                                    vssp_db.Tbl_TRS_StockSSP.Remove(exisstockssp);
                                }
                            }
                            break;

                        case "delete":

                            /* remove existing Stock */
                            var deleteStock = from a in vssp_db.Tbl_TRS_Stock
                                              where a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber
                                              select a;

                            deleteStock.ForEach(Stock =>
                            {
                                vssp_db.Tbl_TRS_Stock.Remove(Stock);
                            });

                            /* remove existing Stock SSP */
                            var deleteStockSSP = from a in vssp_db.Tbl_TRS_StockSSP
                                                 where a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber
                                                 select a;

                            deleteStockSSP.ForEach(Stock =>
                            {
                                vssp_db.Tbl_TRS_StockSSP.Remove(Stock);
                            });

                            /* remove existing Stock WIP */
                            var deleteStockWIP = from a in vssp_db.Tbl_TRS_StockWIP
                                                 where a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber
                                                 select a;

                            deleteStockWIP.ForEach(Stock =>
                            {
                                vssp_db.Tbl_TRS_StockWIP.Remove(Stock);
                            });

                            /* remove existing CostCenters */
                            var deleteCostCenter = from a in vssp_db.Tbl_MST_PartRawMaterialsCostCenter
                                                   where a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber
                                                   select a;

                            deleteCostCenter.ForEach(CostCenters =>
                            {
                                vssp_db.Tbl_MST_PartRawMaterialsCostCenter.Remove(CostCenters);
                            });

                            /* remove existing Prices */
                            var deletePrice = from a in vssp_db.Tbl_MST_PartRawMaterialsPrice
                                              where a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber
                                              select a;

                            deletePrice.ForEach(Prices =>
                            {
                                vssp_db.Tbl_MST_PartRawMaterialsPrice.Remove(Prices);
                            });

                            /* remove existing Part Finish Good */
                            var ListDelete = vssp_db.Tbl_MST_PartRawMaterials.First(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber);

                            vssp_db.Tbl_MST_PartRawMaterials.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        if (formAction.ToLower() != "delete")
                        {
                            var result = vssp_db.Vw_MST_PartRawMaterials.Where(a => a.SupplierId == RawMaterial.SupplierId && a.PartNumber == RawMaterial.PartNumber).FirstOrDefault();
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(RawMaterial, JsonRequestBehavior.AllowGet);
                        }
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
        public void crudRawMaterialCostCenter(List<crud_PartRawMaterialsCostCenter> RawMaterialCostCenters, string SupplierId, string PartNumber, string uid)
        {

            foreach (var CostCenters in RawMaterialCostCenters)
            {
                if (CostCenters.RowStatus != null)
                {
                    switch (CostCenters.RowStatus.ToLower())
                    {
                        case "create":

                            /* create CostCenters */
                            Tbl_MST_PartRawMaterialsCostCenter ListCostCenter = new Tbl_MST_PartRawMaterialsCostCenter();
                            ListCostCenter.SupplierId = SupplierId;
                            ListCostCenter.PartNumber = PartNumber;
                            ListCostCenter.StartDate = CostCenters.StartDate;
                            ListCostCenter.EndDate = CostCenters.EndDate;
                            ListCostCenter.CostId = CostCenters.CostId;
                            ListCostCenter.ClassificationId = CostCenters.ClassificationId;
                            ListCostCenter.PaymentId = CostCenters.PaymentId;
                            ListCostCenter.CategoryId = CostCenters.CategoryId;
                            ListCostCenter.UserId = uid;
                            ListCostCenter.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_PartRawMaterialsCostCenter.Add(ListCostCenter);

                            break;

                        case "update":

                            /* update CostCenters */
                            var ListUpdate = vssp_db.Tbl_MST_PartRawMaterialsCostCenter.First(a => a.SupplierId == SupplierId && a.PartNumber == PartNumber && a.StartDate == CostCenters.StartDate);

                            ListUpdate.EndDate = CostCenters.EndDate;
                            ListUpdate.CostId = CostCenters.CostId;
                            ListUpdate.ClassificationId = CostCenters.ClassificationId;
                            ListUpdate.PaymentId = CostCenters.PaymentId;
                            ListUpdate.CategoryId = CostCenters.CategoryId;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* delete CostCenters */
                            var ListDelete = vssp_db.Tbl_MST_PartRawMaterialsCostCenter.First(a => a.SupplierId == SupplierId && a.PartNumber == PartNumber && a.StartDate == CostCenters.StartDate);

                            vssp_db.Tbl_MST_PartRawMaterialsCostCenter.Remove(ListDelete);

                            break;
                    }
                }
            }
        }

        public void crudRawMaterialPrice(List<crud_PartRawMaterialsPrice> RawMaterialPrices, string SupplierId, string PartNumber, string uid)
        {

            foreach (var Prices in RawMaterialPrices)
            {
                if (Prices.RowStatus != null)
                {
                    switch (Prices.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Prices */
                            Tbl_MST_PartRawMaterialsPrice ListPrice = new Tbl_MST_PartRawMaterialsPrice();
                            ListPrice.SupplierId = SupplierId;
                            ListPrice.PartNumber = PartNumber;
                            ListPrice.StartDate = Prices.StartDate;
                            ListPrice.EndDate = Prices.EndDate;
                            ListPrice.Price = Prices.Price;
                            ListPrice.UserId = uid;
                            ListPrice.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_PartRawMaterialsPrice.Add(ListPrice);

                            break;

                        case "update":

                            /* update Prices */
                            var ListUpdate = vssp_db.Tbl_MST_PartRawMaterialsPrice.First(a => a.SupplierId == SupplierId && a.PartNumber == PartNumber && a.StartDate == Prices.StartDate);

                            ListUpdate.EndDate = Prices.EndDate;
                            ListUpdate.Price = Prices.Price;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* delete Prices */
                            var ListDelete = vssp_db.Tbl_MST_PartRawMaterialsPrice.First(a => a.SupplierId == SupplierId && a.PartNumber == PartNumber && a.StartDate == Prices.StartDate);

                            vssp_db.Tbl_MST_PartRawMaterialsPrice.Remove(ListDelete);

                            break;
                    }
                }
            }
        }
        public void crudKanbanCycle(List<crud_KanbanCycle> KanbanCycles, string SupplierId, string uid)
        {

            foreach (var Kanbans in KanbanCycles)
            {
                if (Kanbans.RowStatus != null)
                {
                    switch (Kanbans.RowStatus.ToLower())
                    {
                        case "create":


                            /* create Kanbans */
                            Tbl_MST_KanbanCycle ListKanban = new Tbl_MST_KanbanCycle();
                            ListKanban.SupplierId = SupplierId;
                            ListKanban.Cycle1 = Kanbans.Cycle1;
                            ListKanban.Cycle2 = Kanbans.Cycle2;
                            ListKanban.Cycle2 = Kanbans.Cycle2;
                            ListKanban.Cycle3 = Kanbans.Cycle3;
                            ListKanban.CycleTime = Kanbans.CycleTime;
                            ListKanban.StartDate = Kanbans.StartDate;
                            ListKanban.EndDate = Kanbans.EndDate;
                            ListKanban.UserId = uid;
                            ListKanban.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_KanbanCycle.Add(ListKanban);

                            break;

                        case "update":

                            /* update Kanbans */
                            var ListUpdate = vssp_db.Tbl_MST_KanbanCycle.First(a => a.SupplierId == SupplierId && a.StartDate == Kanbans.StartDate);

                            ListUpdate.Cycle1 = Kanbans.Cycle1;
                            ListUpdate.Cycle2 = Kanbans.Cycle2;
                            ListUpdate.Cycle2 = Kanbans.Cycle2;
                            ListUpdate.Cycle3 = Kanbans.Cycle3;
                            ListUpdate.CycleTime = Kanbans.CycleTime;
                            ListUpdate.StartDate = Kanbans.StartDate;
                            ListUpdate.EndDate = Kanbans.EndDate;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* delete Kanbans */
                            var deleteCycle = from a in vssp_db.Tbl_MST_KanbanCycleTime
                                              where a.SupplierId == SupplierId && a.StartDate == Kanbans.StartDate
                                              select a;

                            deleteCycle.ForEach(Cycles =>
                            {
                                vssp_db.Tbl_MST_KanbanCycleTime.Remove(Cycles);
                            });

                            var ListDelete = vssp_db.Tbl_MST_KanbanCycle.First(a => a.SupplierId == SupplierId && a.StartDate == Kanbans.StartDate);
                            vssp_db.Tbl_MST_KanbanCycle.Remove(ListDelete);

                            break;
                    }
                }
            }
        }
        public ActionResult SpecialSupplyPart()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Suppliers", "SpecialSupplyPart");

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

        public ActionResult SpecialSupplyPartListJson(string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            var SpecialSupplyPart = from a in vssp_db.Tbl_MST_SpecialSupplyPart
                                    where a.Actived == isActive && (a.Id.Contains(searchFilter) || a.ProcessName.Contains(searchFilter))
                                    orderby a.Id
                                    select new
                                    {
                                        a.Id,
                                        a.ProcessName,
                                        a.Remarks,
                                        a.SSPStock,
                                        a.DeliveryOrder,
                                        a.DeliveryRawMaterial,
                                        a.Actived,
                                        a.UserId,
                                        a.EditDate
                                    };



            return Json(SpecialSupplyPart, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudSpecialSupplyPartList(string Id, string ProcessName, string Remarks, bool SSPStock, bool DeliveryOrder, bool DeliveryRawMaterial, bool Actived, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();


                    Tbl_MST_SpecialSupplyPart ListSpecialSupplyPart = new Tbl_MST_SpecialSupplyPart();
                    ListSpecialSupplyPart.Id = Id;
                    ListSpecialSupplyPart.ProcessName = ProcessName;
                    ListSpecialSupplyPart.Remarks = Remarks;
                    ListSpecialSupplyPart.SSPStock = SSPStock;
                    ListSpecialSupplyPart.DeliveryOrder = DeliveryOrder;
                    ListSpecialSupplyPart.DeliveryRawMaterial = DeliveryRawMaterial;
                    ListSpecialSupplyPart.Actived = Actived;
                    ListSpecialSupplyPart.UserId = uid;
                    ListSpecialSupplyPart.EditDate = DateTime.Now;

                    switch (formAction.ToLower())
                    {
                        case "create":

                            vssp_db.Tbl_MST_SpecialSupplyPart.Add(ListSpecialSupplyPart);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_SpecialSupplyPart.First(a => a.Id == Id);

                            ListUpdate.ProcessName = ProcessName;
                            ListUpdate.Remarks = Remarks;
                            ListUpdate.SSPStock = SSPStock;
                            ListUpdate.DeliveryOrder = DeliveryOrder;
                            ListUpdate.DeliveryRawMaterial = DeliveryRawMaterial;
                            ListUpdate.Actived = Actived;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* remove existing SSP */
                            var ListDelete = vssp_db.Tbl_MST_SpecialSupplyPart.First(a => a.Id == Id);

                            vssp_db.Tbl_MST_SpecialSupplyPart.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(ListSpecialSupplyPart, JsonRequestBehavior.AllowGet);
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