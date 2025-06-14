using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.Models;
using Core.VSSP.Controllers;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using System.Net;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.IO;
using System.Security.Cryptography;
using CrystalDecisions.CrystalReports.Engine;

namespace Core.VSSP.Controllers
{
    public class InventoryController : Controller
    {
        // GET: Inventory
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult StockRawMaterial()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Inventory", "StockRawMaterial");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    StockRawMaterialListModel StockRawMaterial = new StockRawMaterialListModel();
                    StockRawMaterial.ExportList = _SystemService.ComboExport().ToList();
                    StockRawMaterial.SupplierList = vssp_db.Tbl_MST_Supplier.Where(a => a.Actived==true).OrderBy(a => a.SupplierId).ToList();
                    StockRawMaterial.AreaList = vssp_db.Tbl_MST_WarehouseArea.OrderBy(a => a.AreaID).ToList();
                    StockRawMaterial.LocationList = vssp_db.Tbl_MST_WarehouseLocation.OrderBy(a => a.LocationId).ToList();

                    Session["Layout"] = "portal";
                    return View(StockRawMaterial);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult StockRawMaterialJson(
                                    string searchFilter,
                                    string supplierid,
                                    string areaid,
                                    string locationid,
                                    bool abnormal
                                    )
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_Stock> StockRawMaterial = (from a in vssp_db.Vw_TRS_Stock
                                            where a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter)
                                            select a).ToList();
            
            if (_SystemService.Vf(supplierid) != "")
            {
                StockRawMaterial = StockRawMaterial.Where(a => a.SupplierId == supplierid).ToList();
            }
            if (_SystemService.Vf(areaid) != "")
            {
                StockRawMaterial = StockRawMaterial.Where(a => a.AreaId == areaid).ToList();
            }
            if (_SystemService.Vf(locationid) != "")
            {
                StockRawMaterial = StockRawMaterial.Where(a => a.LocationId == locationid).ToList();
            }
            if (_SystemService.Vb(abnormal.ToString()) != false)
            {
                StockRawMaterial = StockRawMaterial.Where(a => a.TotalStockKanban < a.MinStock || a.TotalStockKanban > a.MaxStock).ToList();
            }
            StockRawMaterial.OrderBy(a => new { a.SupplierId, a.AreaId, a.LocationId, a.UniqueNumber });

            return Json(StockRawMaterial, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockRawReturn()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Inventory", "StockRawReturn");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    StockRawReturnListModel StockRawReturn = new StockRawReturnListModel();
                    StockRawReturn.ExportList = _SystemService.ComboExport().ToList();
                    StockRawReturn.SupplierList = vssp_db.Tbl_MST_Supplier.Where(a => a.Actived == true).OrderBy(a => a.SupplierId).ToList();
                    StockRawReturn.AreaList = vssp_db.Tbl_MST_WarehouseArea.OrderBy(a => a.AreaID).ToList();
                    StockRawReturn.LocationList = vssp_db.Tbl_MST_WarehouseLocation.OrderBy(a => a.LocationId).ToList();

                    Session["Layout"] = "portal";
                    return View(StockRawReturn);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult StockRawReturnJson(
                                    string searchFilter,
                                    string supplierid,
                                    string areaid,
                                    string locationid,
                                    string PartNotInclude,
                                    bool abnormal,
                                    bool exludeZero = false
                                    )
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_StockRawReturn> StockRawReturn = (from a in vssp_db.Vw_TRS_StockRawReturn
                                                          where a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter)
                                                   select a).ToList();

            if (_SystemService.Vf(supplierid) != "")
            {
                StockRawReturn = StockRawReturn.Where(a => a.SupplierId == supplierid).ToList();
            }
            if (_SystemService.Vf(areaid) != "")
            {
                StockRawReturn = StockRawReturn.Where(a => a.AreaId == areaid).ToList();
            }
            if (_SystemService.Vf(locationid) != "")
            {
                StockRawReturn = StockRawReturn.Where(a => a.LocationId == locationid).ToList();
            }
            if (_SystemService.Vb(abnormal.ToString()) != false)
            {
                StockRawReturn = StockRawReturn.Where(a => a.TotalStockQty < a.MinStock || a.TotalStockQty > a.MaxStock).ToList();
            }
            if (exludeZero)
            {
                StockRawReturn = StockRawReturn.Where(a => a.StockQty > 0).ToList();
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
                StockRawReturn = StockRawReturn.Where(a => !exceptionList.Contains(a.PartNumber)).ToList();
            }
            StockRawReturn.OrderBy(a => new { a.SupplierId, a.AreaId, a.LocationId, a.UniqueNumber });

            return Json(StockRawReturn, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockFinishGoods()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Inventory", "StockFinishGoods");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    StockFinishGoodsListModel StockFinishGoods = new StockFinishGoodsListModel();
                    StockFinishGoods.ExportList = _SystemService.ComboExport().ToList();
                    StockFinishGoods.CustomerList = vssp_db.Tbl_MST_Customer.Where(a => a.Actived == true).OrderBy(a => a.CustomerId).ToList();
                    StockFinishGoods.AreaList = vssp_db.Tbl_MST_WarehouseArea.OrderBy(a => a.AreaID).ToList();
                    StockFinishGoods.LocationList = vssp_db.Tbl_MST_WarehouseLocation.OrderBy(a => a.LocationId).ToList();

                    Session["Layout"] = "portal";
                    return View(StockFinishGoods);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult StockFinishGoodsJson(
                                    string searchFilter,
                                    string Customerid,
                                    string areaid,
                                    string locationid,
                                    bool abnormal
                                    )
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_StockFG> StockFinishGoods = (from a in vssp_db.Vw_TRS_StockFG
                                                   where a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter)
                                                   select a).ToList();

            if (_SystemService.Vf(Customerid) != "")
            {
                StockFinishGoods = StockFinishGoods.Where(a => a.CustomerId == Customerid).ToList();
            }
            if (_SystemService.Vf(areaid) != "")
            {
                StockFinishGoods = StockFinishGoods.Where(a => a.AreaId == areaid).ToList();
            }
            if (_SystemService.Vf(locationid) != "")
            {
                StockFinishGoods = StockFinishGoods.Where(a => a.LocationId == locationid).ToList();
            }
            if (_SystemService.Vb(abnormal.ToString()) != false)
            {
                StockFinishGoods = StockFinishGoods.Where(a => a.StockKanban < a.MinStock || a.StockKanban > a.MaxStock).ToList();
            }
            StockFinishGoods.OrderBy(a => new { a.CustomerId, a.AreaId, a.LocationId, a.UniqueNumber });

            return Json(StockFinishGoods, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockWaitInProcess()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Inventory", "StockWaitInProcess");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    StockWaitInProcessListModel StockWaitInProcess = new StockWaitInProcessListModel();
                    StockWaitInProcess.ExportList = _SystemService.ComboExport().ToList();
                    StockWaitInProcess.SupplierList = vssp_db.Tbl_MST_Supplier.Where(a => a.Actived == true).OrderBy(a => a.SupplierId).ToList();
                    StockWaitInProcess.AreaList = vssp_db.Tbl_MST_WarehouseArea.OrderBy(a => a.AreaID).ToList();
                    StockWaitInProcess.LocationList = vssp_db.Tbl_MST_WarehouseLocation.OrderBy(a => a.LocationId).ToList();

                    Session["Layout"] = "portal";
                    return View(StockWaitInProcess);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult StockWaitInProcessJson(
                                    string searchFilter,
                                    string Supplierid,
                                    string areaid,
                                    string locationid,
                                    bool abnormal
                                    )
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_StockWIP> StockWaitInProcess = (from a in vssp_db.Vw_TRS_StockWIP
                                                     where a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter)
                                                     select a).ToList();

            if (_SystemService.Vf(Supplierid) != "")
            {
                StockWaitInProcess = StockWaitInProcess.Where(a => a.SupplierId == Supplierid).ToList();
            }
            if (_SystemService.Vf(areaid) != "")
            {
                StockWaitInProcess = StockWaitInProcess.Where(a => a.AreaId == areaid).ToList();
            }
            if (_SystemService.Vf(locationid) != "")
            {
                StockWaitInProcess = StockWaitInProcess.Where(a => a.LocationId == locationid).ToList();
            }
            if (_SystemService.Vb(abnormal.ToString()) != false)
            {
                StockWaitInProcess = StockWaitInProcess.Where(a => a.StockKanban < a.MinStock || a.StockKanban > a.MaxStock).ToList();
            }
            StockWaitInProcess.OrderBy(a => new { a.SupplierId, a.AreaId, a.LocationId, a.UniqueNumber });

            return Json(StockWaitInProcess, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockTaking()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Inventory", "StockTaking");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    StockTakingListModel StockTaking = new StockTakingListModel();
                    StockTaking.ExportList = _SystemService.ComboExport().ToList();
                    StockTaking.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                                orderby a.Id
                                                select a).ToList();

                    Session["Layout"] = "portal";
                    return View(StockTaking);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult StockTakingListJson(
                                    string searchFilter,
                                    string month = null,
                                    Nullable<DateTime> inventorydate = null,
                                    int status = 99)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            string ordermonth = "";
            string orderyears = "";

            List<Vw_TRS_StockTaking> StockTaking = new List<Vw_TRS_StockTaking>();
            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                ordermonth = arrs[0];
                orderyears = arrs[1];

                StockTaking = (from a in vssp_db.Vw_TRS_StockTaking
                               where a.InventoryYear == orderyears && a.InventoryMonth == ordermonth && a.InventoryNumber.Contains(searchFilter)
                               orderby a.InventoryDate descending, a.EditDate descending
                               select a).ToList();

            }
            else
            {

                StockTaking = (from a in vssp_db.Vw_TRS_StockTaking
                               where a.InventoryNumber.Contains(searchFilter)
                               orderby a.InventoryDate descending, a.EditDate descending
                               select a).ToList();

            }

            if (inventorydate != null)
                {
                    StockTaking = StockTaking.Where(a => a.InventoryDate >= inventorydate).ToList();
                }
                if (status != 99)
                {
                    StockTaking = StockTaking.Where(a => a.Status.ToString() == status.ToString()).ToList();
                }
                else
                {
                    var notinStatus = from a in StockTaking
                                      where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                      select a.Status;
                    StockTaking = StockTaking.Where(a => !notinStatus.Contains(a.Status)).ToList();
                }

            //return Json(StockTaking, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(StockTaking, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;

        }
        public ActionResult StockTypeList()
        {

            vssp_db.Database.CommandTimeout = 0;
            var stocktype = (from a in vssp_db.Vw_TRS_StockList
                             orderby a.StockType
                             select new { a.StockType }).Distinct().ToList();

            return Json(stocktype, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockOwnerList(string stocktype)
        {
            switch (stocktype.ToLower())
            {
                case "raw":
                    var stockowner1 = (from a in vssp_db.Vw_TRS_StockList
                                      join b in vssp_db.Tbl_MST_Supplier on a.SupplierId equals b.SupplierId into supplier
                                      from b in supplier.DefaultIfEmpty()
                                      where a.StockType==stocktype
                                      orderby a.SupplierId
                                      select new { OwnerId = a.SupplierId, OwnerName = b.SupplierName }).Distinct().ToList();
                    return Json(stockowner1, JsonRequestBehavior.AllowGet);
                case "wip-ssp":
                    var stockowner2 = (from a in vssp_db.Vw_TRS_StockList
                                      join b in vssp_db.Tbl_MST_Supplier on a.SupplierId equals b.SupplierId into supplier
                                      from b in supplier.DefaultIfEmpty()
                                      where a.StockType == stocktype
                                      orderby a.SupplierId
                                      select new { OwnerId = a.SupplierId, OwnerName = b.SupplierName }).Distinct().ToList();
                    return Json(stockowner2, JsonRequestBehavior.AllowGet);
                case "wip-prod":
                    var stockowner3 = (from a in vssp_db.Vw_TRS_StockList
                                       join b in vssp_db.Tbl_MST_Line on a.LineId equals b.LineId into line
                                       from b in line.DefaultIfEmpty()
                                       where a.StockType == stocktype
                                       orderby a.LineId
                                       select new { OwnerId = a.LineId, OwnerName = b.LineName }).Distinct().ToList();
                    return Json(stockowner3, JsonRequestBehavior.AllowGet);
                case "passthrough":
                    var stockowner4 = (from a in vssp_db.Vw_TRS_StockList
                                       join b in vssp_db.Tbl_MST_Supplier on a.SupplierId equals b.SupplierId into supplier
                                       from b in supplier.DefaultIfEmpty()
                                       where a.StockType == stocktype
                                       orderby a.SupplierId
                                       select new { OwnerId = a.SupplierId, OwnerName = b.SupplierName }).Distinct().ToList();
                    return Json(stockowner4, JsonRequestBehavior.AllowGet);
                case "finishgoods":
                    var stockowner5 = (from a in vssp_db.Vw_TRS_StockList
                                       join b in vssp_db.Tbl_MST_Customer on a.CustomerId equals b.CustomerId into customer
                                       from b in customer.DefaultIfEmpty()
                                       where a.StockType == stocktype
                                       orderby a.CustomerId
                                       select new { OwnerId = a.CustomerId, OwnerName = b.CustomerName }).Distinct().ToList();
                    return Json(stockowner5, JsonRequestBehavior.AllowGet);
                default:
                    return Json("ALL", JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult StockTakingDetailListJson(string inventorynumber, string stocktype, string owner, string areaid, string locationid, string formAction, string UniqueNotInclude, string PartNotInclude)
        {
            try
            {
                if (_SystemService.Vf(PartNotInclude) == "")
                {
                    PartNotInclude = "[]";
                }
                List<crud_StockTakingDetail> partNotIn = JsonConvert.DeserializeObject<List<crud_StockTakingDetail>>(PartNotInclude);
                stocktype = _SystemService.Vf(stocktype);
                formAction = _SystemService.Vf(formAction);
                switch (formAction.ToLower())
                {
                    case "create":
                        owner = _SystemService.Vf(owner);
                        var Stock = vssp_db.Vw_TRS_StockList.Where(a=> a.StockType.Contains(stocktype) && (a.CustomerId.Contains(owner) || a.LineId.Contains(owner) || a.SupplierId.Contains(owner))).OrderBy(a => a.StockType).ThenBy(a => a.UniqueNumber).ToList();

                        return Json(Stock, JsonRequestBehavior.AllowGet);
                    default:
                        var StockTakingDetail = (from a in vssp_db.Vw_TRS_StockTakingDetail
                                                 join b in vssp_db.Vw_TRS_StockList on new { a.CustomerId, a.LineId, a.SupplierId, a.PartNumber } equals new { b.CustomerId, b.LineId, b.SupplierId, b.PartNumber } into stock
                                                 from b in stock.DefaultIfEmpty()
                                                 where a.InventoryNumber == inventorynumber || a.InventoryNumber.Replace("/", "") == inventorynumber
                                                 select new { a.InventoryNumber, a.SupplierId, a.LineId, a.CustomerId, b.StockType, a.UniqueNumber, a.PartNumber, a.PartName, a.AreaId, a.LocationId, a.CategoryId, a.PartModel, a.UnitQty, a.UnitLevel2, a.PackingId, a.StockKanban, a.StockQty, a.ActualQty, a.BalanceQty }).ToList();

                        if (_SystemService.Vf(areaid) != "")
                        {
                            StockTakingDetail = StockTakingDetail.Where(a => a.AreaId == areaid).ToList(); // .OrderBy(a => new { a.LocationId, a.UniqueNumber }).ToList();
                        }
                        if (_SystemService.Vf(locationid) != "")
                        {
                            StockTakingDetail = StockTakingDetail.Where(a => a.LocationId == locationid).ToList(); //.OrderBy(a => new { a.LocationId, a.UniqueNumber }).ToList();
                        }
                        //if (_SystemService.Vf(UniqueNotInclude) != "")
                        //{
                        //    var exceptionList = new List<string>();
                        //    JsonTextReader reader = new JsonTextReader(new StringReader(UniqueNotInclude));
                        //    while (reader.Read())
                        //    {
                        //        if (reader.Value != null)
                        //        {
                        //            if (reader.TokenType.ToString() == "String")
                        //            {
                        //                exceptionList.Add(reader.Value.ToString());
                        //            }
                        //        }
                        //    }
                        //    StockTakingDetail = StockTakingDetail.Where(a => !exceptionList.Contains(a.UniqueNumber)).ToList();
                        //}
                        //if (_SystemService.Vf(PartNotInclude) != "")
                        //{
                        //    var exceptionList = new List<string>();
                        //    JsonTextReader reader = new JsonTextReader(new StringReader(PartNotInclude));
                        //    while (reader.Read())
                        //    {
                        //        if (reader.Value != null)
                        //        {
                        //            if (reader.TokenType.ToString() == "String")
                        //            {
                        //                exceptionList.Add(reader.Value.ToString());
                        //            }
                        //        }
                        //    }
                        //    StockTakingDetail = StockTakingDetail.Where(a => !exceptionList.Contains(a.PartNumber)).ToList();
                        //}

                        if(partNotIn.Count() > 0)
                        {
                            StockTakingDetail = StockTakingDetail.Where(a => !partNotIn.Any(b => b.CustomerId == a.CustomerId && b.SupplierId == a.SupplierId && b.PartNumber == a.PartNumber)).ToList();
                        }

                        return Json(StockTakingDetail, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }        
        public ActionResult StockTakingApprovalListJson(string inventorynumber, Nullable<bool> approved)
        {
            try
            {

                var StockTakingApproval = from a in vssp_db.Tbl_TRS_StockTakingApproval
                                            where a.InventoryNumber.Contains(inventorynumber)
                                            orderby a.ApprovalLevel
                                            select new { a.InventoryNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

                if (approved != null)
                {
                    StockTakingApproval = StockTakingApproval.Where(a => a.Approved == approved);
                }

                return Json(StockTakingApproval, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudStockTakingList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostStockTakingModel postStockTaking = JsonConvert.DeserializeObject<PostStockTakingModel>(jsonData);
                    Tbl_TRS_StockTaking StockTaking = postStockTaking.StockTaking;
                    List<crud_StockTakingDetail> StockTakingDetail = postStockTaking.StockTakingDetail;

                    string formAction = postStockTaking.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":


                            /* Get New Order Number */
                            string CompId = Session["CompID"].ToString();
                            var InventoryNumber = vssp_db.SP_GET_StockTakingNumber(StockTaking.InventoryDate, CompId);
                            foreach (SP_GET_StockTakingNumber_Result number in InventoryNumber)
                            {
                                StockTaking.InventoryNumber = number.InventoryNumber;
                            }

                            Tbl_TRS_StockTaking ListStockTaking = new Tbl_TRS_StockTaking();
                            ListStockTaking.InventoryNumber = StockTaking.InventoryNumber;
                            ListStockTaking.InventoryDate = StockTaking.InventoryDate;
                            ListStockTaking.InventoryStartTime = StockTaking.InventoryStartTime;
                            ListStockTaking.InventoryEndTime = StockTaking.InventoryEndTime;
                            ListStockTaking.StockType = StockTaking.StockType;
                            ListStockTaking.Remarks = StockTaking.Remarks;
                            ListStockTaking.Updated = false;
                            ListStockTaking.Status = 0;
                            ListStockTaking.UserId = uid;
                            ListStockTaking.EditDate = DateTime.Now;

                            vssp_db.Tbl_TRS_StockTaking.Add(ListStockTaking);

                            /* crud Details */
                            crudStockTakingDetail(StockTakingDetail, StockTaking.InventoryNumber, formAction);

                            /* crud Approval */
                            crudStockTakingApproval(postStockTaking.ApprovalId, StockTaking.InventoryNumber, uid, formAction);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_StockTaking.First(a => a.InventoryNumber == StockTaking.InventoryNumber);

                            ListUpdate.InventoryDate = StockTaking.InventoryDate;
                            ListUpdate.InventoryStartTime = StockTaking.InventoryStartTime;
                            ListUpdate.InventoryEndTime = StockTaking.InventoryEndTime;
                            ListUpdate.StockType = StockTaking.StockType;
                            ListUpdate.Remarks = StockTaking.Remarks;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Details */
                            crudStockTakingDetail(StockTakingDetail, StockTaking.InventoryNumber, formAction);

                            /* crud Approval */
                            crudStockTakingApproval(postStockTaking.ApprovalId, StockTaking.InventoryNumber, uid, formAction);

                            break;

                        case "actualstock":

                            /* crud Details */
                            crudStockTakingDetail(StockTakingDetail, StockTaking.InventoryNumber, "Update");

                            break;

                        case "closed":

                            var ListClosed = vssp_db.Tbl_TRS_StockTaking.First(a => a.InventoryNumber == StockTaking.InventoryNumber);

                            ListClosed.Status = 3;

                            break;

                        case "canceled":

                            var ListCanceled = vssp_db.Tbl_TRS_StockTaking.First(a => a.InventoryNumber == StockTaking.InventoryNumber);

                            ListCanceled.Status = 4;

                            break;

                        case "delete":

                            /* remove existing StockTaking */
                            var ListDelete = vssp_db.Tbl_TRS_StockTaking.First(a => a.InventoryNumber == StockTaking.InventoryNumber);

                            ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                            //vssp_db.Tbl_TRS_StockTaking.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();

                        return Json(StockTaking, JsonRequestBehavior.AllowGet);
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

        public void crudStockTakingDetail(List<crud_StockTakingDetail> StockTakingDetails, string InventoryNumber, string formAction)
        {

            if (formAction.ToLower() == "update")
            {
                var acceptList = (from a in StockTakingDetails
                                  select a.CustomerId + a.LineId + a.SupplierId + a.PartNumber).ToList();

                var cleanList = (from a in vssp_db.Tbl_TRS_StockTakingDetail
                                 where a.InventoryNumber == InventoryNumber && !acceptList.Contains(a.CustomerId + a.LineId + a.SupplierId + a.PartNumber)
                                 select a).ToList();

                vssp_db.Tbl_TRS_StockTakingDetail.RemoveRange(cleanList);
            }

            foreach (var Details in StockTakingDetails)
            {
                if (Details.RowStatus == null && formAction == "create")
                {
                    Details.RowStatus = formAction;
                }
                if (Details.RowStatus != null)
                {
                    switch (Details.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_StockTakingDetail ListDetail = new Tbl_TRS_StockTakingDetail();
                            ListDetail.InventoryNumber = InventoryNumber;
                            ListDetail.CustomerId = Details.CustomerId;
                            ListDetail.LineId = Details.LineId;
                            ListDetail.SupplierId = Details.SupplierId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.StockKanban = Details.StockKanban;
                            ListDetail.StockQty = Details.StockQty;
                            ListDetail.ActualQty = 0;
                            ListDetail.BalanceQty = 0 - Details.StockQty;

                            vssp_db.Tbl_TRS_StockTakingDetail.Add(ListDetail);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_StockTakingDetail.First(a => a.InventoryNumber == InventoryNumber && (a.SupplierId == Details.SupplierId && a.LineId == Details.LineId && a.CustomerId == Details.CustomerId ) && a.PartNumber == Details.PartNumber);

                            ListUpdate.StockKanban = Details.StockKanban;
                            ListUpdate.StockQty = Details.StockQty;
                            ListUpdate.ActualQty = Details.ActualQty;
                            ListUpdate.BalanceQty = Details.BalanceQty;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_StockTakingDetail.First(a => a.InventoryNumber == InventoryNumber && (a.SupplierId == Details.SupplierId && a.LineId == Details.LineId && a.CustomerId == Details.CustomerId) && a.PartNumber == Details.PartNumber);

                            vssp_db.Tbl_TRS_StockTakingDetail.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        public ActionResult crudLatestStock(string InventoryNumber)
        {
            if (Session["UserID"] != null) { 
                
                try { 

                    //var ListDelete = vssp_db.Tbl_TRS_StockTakingDetail.Where(a => a.InventoryNumber == InventoryNumber).ToList();
                    //foreach(var delete in ListDelete)
                    //{
                    //    vssp_db.Tbl_TRS_StockTakingDetail.Remove(delete);
                    //}

                    var StockTaking = (from a in vssp_db.Vw_TRS_StockTakingDetail
                                     where a.InventoryNumber == InventoryNumber
                                     select new { a.InventoryNumber, a.SupplierId, a.LineId, a.CustomerId, a.PartNumber, a.StockKanban, a.StockQty }
                                     ).OrderBy(a => new { a.SupplierId, a.PartNumber }).ToList();

                    foreach (var stock in StockTaking)
                    {
                        var stockTakingDetail = vssp_db.Tbl_TRS_StockTakingDetail.Where(a => a.InventoryNumber==stock.InventoryNumber && a.SupplierId == stock.SupplierId && a.LineId == stock.LineId && a.CustomerId == stock.CustomerId && a.PartNumber == stock.PartNumber).FirstOrDefault();
                        
                        //stockTakingDetail.InventoryNumber = InventoryNumber;
                        //stockTakingDetail.SupplierId = stock.SupplierId;
                        //stockTakingDetail.PartNumber = stock.PartNumber;
                        stockTakingDetail.StockKanban = stock.StockKanban;
                        stockTakingDetail.StockQty = stock.StockQty;
                        stockTakingDetail.ActualQty = 0;
                        stockTakingDetail.BalanceQty = 0 - stock.StockQty;

                        //vssp_db.Tbl_TRS_StockTakingDetail.Add(stockTakingDetail);
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        string uid = Session["UserID"].ToString();
                        Tbl_TRS_StockTaking stockTaking = vssp_db.Tbl_TRS_StockTaking.Where(a => a.InventoryNumber == InventoryNumber).FirstOrDefault();
                        stockTaking.Updated = true;
                        stockTaking.UserId = uid;
                        stockTaking.EditDate = DateTime.Now;

                        vssp_db.SaveChanges();

                        return Json("Success", JsonRequestBehavior.AllowGet);
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

        public void crudStockTakingApproval(string ApprovalId, string InventoryNumber, string UserId, string action)
        {
            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    List<UserApprovalListModel> userApprovalLists = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalLists)
                    {
                        Tbl_TRS_StockTakingApproval ListApproval = new Tbl_TRS_StockTakingApproval();
                        ListApproval.InventoryNumber = InventoryNumber;
                        ListApproval.UserId = users.UserID;
                        ListApproval.UserName = users.UserName;
                        ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                        ListApproval.ApprovalName = users.ApprovalName;
                        ListApproval.ApprovalEmail = users.Email;
                        ListApproval.SentEmail = false;
                        ListApproval.SentEmailDate = null;
                        if (users.UserID == UserId)
                        {
                            ListApproval.Approved = true;
                            ListApproval.ApprovedDate = DateTime.Now;
                        }
                        else
                        {
                            ListApproval.Approved = false;
                            ListApproval.ApprovedDate = null;
                        }

                        vssp_db.Tbl_TRS_StockTakingApproval.Add(ListApproval);
                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_TRS_StockTakingApproval> UserApproval = vssp_db.Tbl_TRS_StockTakingApproval.Where(a => a.InventoryNumber == InventoryNumber).ToList();
                    foreach (var user in UserApproval)
                    {
                        UserApprovalListModel ApprovalLists = _AccountService.UserApprovalType(user.UserId, ApprovalId).First(a => a.UserID == user.UserId);
                    }

                    /* create approval */
                    List<UserApprovalListModel> userApprovalListsUpdate = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalListsUpdate)
                    {
                        Tbl_TRS_StockTakingApproval existUser = (from a in vssp_db.Tbl_TRS_StockTakingApproval
                                                                     where a.InventoryNumber == InventoryNumber && a.UserId == users.UserID
                                                                     select a).FirstOrDefault();
                        if (existUser == null)
                        {
                            Tbl_TRS_StockTakingApproval ListApproval = new Tbl_TRS_StockTakingApproval();
                            ListApproval.InventoryNumber = InventoryNumber;
                            ListApproval.UserId = users.UserID;
                            ListApproval.UserName = users.UserName;
                            ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                            ListApproval.ApprovalName = users.ApprovalName;
                            ListApproval.ApprovalEmail = users.Email;
                            ListApproval.SentEmail = false;
                            ListApproval.SentEmailDate = null;
                            if (users.UserID == UserId)
                            {
                                ListApproval.Approved = true;
                                ListApproval.ApprovedDate = DateTime.Now;
                            }
                            else
                            {
                                ListApproval.Approved = false;
                                ListApproval.ApprovedDate = null;
                            }

                            vssp_db.Tbl_TRS_StockTakingApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "sent":

                    var ListSent = vssp_db.Tbl_TRS_StockTakingApproval.First(a => a.InventoryNumber == InventoryNumber && a.UserId == ApprovalId);

                    ListSent.SentEmail = true;
                    ListSent.SentEmailDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "approved":

                    Tbl_TRS_StockTaking stockTaking = vssp_db.Tbl_TRS_StockTaking.Where(a => a.InventoryNumber == InventoryNumber).FirstOrDefault();

                    stockTaking.Status = 2;

                    var ListUpdate = vssp_db.Tbl_TRS_StockTakingApproval.First(a => a.InventoryNumber == InventoryNumber && a.UserId == ApprovalId);

                    ListUpdate.Approved = true;
                    ListUpdate.ApprovedDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_TRS_StockTakingApproval.First(a => a.InventoryNumber == InventoryNumber && a.UserId == ApprovalId);

                    vssp_db.Tbl_TRS_StockTakingApproval.Remove(ListDelete);

                    break;
            }
        }
        public ActionResult StockTakingApproval(string inventorynumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Inventory Stock Taking Approval";

            try
            {

                if (inventorynumber == null || uid == null)
                {
                    inventorynumber = Session["InventoryNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["InventoryNumber"] = inventorynumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_TRS_StockTaking StockTaking = vssp_db.Vw_TRS_StockTaking.Where(a => a.InventoryNumber == inventorynumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_TRS_StockTakingApproval approval = vssp_db.Tbl_TRS_StockTakingApproval.Where(a => a.InventoryNumber == inventorynumber && a.UserId == user.UserID).FirstOrDefault();

                    if (StockTaking != null && user != null && approval != null)
                    {

                        string inventorydate = _SystemService.Vd(StockTaking.InventoryDate.ToString(),"dd MMMM, yyyy");
                        string starttime = _SystemService.Vd(StockTaking.InventoryStartTime.ToString(),"HH:mm");
                        string endtime = _SystemService.Vd(StockTaking.InventoryEndTime.ToString(),"HH:mm");

                        ViewBag.OrderTitle = "Inventory Stock Taking";
                        ViewBag.InventoryNumber = StockTaking.InventoryNumber;
                        ViewBag.InventoryStatus = StockTaking.StatusName;
                        ViewBag.Status = StockTaking.Status;
                        ViewBag.InventoryApproval = StockTaking.Approval.Replace(":"," by");
                        ViewBag.InventoryDate = inventorydate;
                        ViewBag.StartTime = starttime;
                        ViewBag.EndTime = endtime;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        if (approval.Approved == false)
                        {
                            return View();
                        }
                        else
                        {
                            ViewBag.ApprovedDate = _SystemService.Vd(approval.ApprovedDate.ToString(), "dd MMMM, yyyy");
                            return View("StockTakingApproved");
                        }

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Inventory Stock Taking";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }

        public ActionResult StockTakingApproved(string inventorynumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Inventory Stock Taking Approval";

            try
            {

                if (inventorynumber == null || uid == null)
                {
                    inventorynumber = Session["InventoryNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["InventoryNumber"] = inventorynumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_TRS_StockTaking StockTaking = vssp_db.Vw_TRS_StockTaking.Where(a => a.InventoryNumber == inventorynumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();

                    if (StockTaking != null && user != null)
                    {

                        string inventorydate = _SystemService.Vd(StockTaking.InventoryDate.ToString(), "dd MMMM, yyyy");
                        string starttime = _SystemService.Vd(StockTaking.InventoryStartTime.ToString(), "HH:mm");
                        string endtime = _SystemService.Vd(StockTaking.InventoryEndTime.ToString(), "HH:mm");

                        ViewBag.OrderTitle = "Inventory Stock Taking";
                        ViewBag.InventoryNumber = StockTaking.InventoryNumber;
                        ViewBag.InventoryDate = inventorydate;
                        ViewBag.StartTime = starttime;
                        ViewBag.EndTime = endtime;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        crudStockTakingApproval(user.UserID, StockTaking.InventoryNumber, user.UserID, "Approved");
                        return RedirectToAction("ContinuePage", "System", new { cmessage = "Successfuly Approved " + ViewBag.OrderTitle + " \n " + inventorynumber, caction = "Dashboard", ccontroller = "Home", capps = "Home" });

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Inventory Stock Taking";
                        ViewBag.UserName = null;

                        return RedirectToAction("ErrorPage", "System", new { errnumber = "500", errmessage = "Order or User not valid.", backaction = "MainIndex", backcontroller = "Index" });

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }
        public ActionResult StockAdjustment()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Inventory", "StockAdjustment");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    StockAdjustmentListModel StockAdjustment = new StockAdjustmentListModel();
                    StockAdjustment.ExportList = _SystemService.ComboExport().ToList();
                    StockAdjustment.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                              orderby a.Id
                                              select a).ToList();

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

                        return View(StockAdjustment);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult StockAdjustmentListJson(
                                    string searchFilter,
                                    string month = null,
                                    Nullable<DateTime> Adjustmentdate = null,
                                    int status = 99)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List <Vw_TRS_StockAdjustment> StockAdjustment = new List<Vw_TRS_StockAdjustment>();


            string ordermonth = "";
            string orderyears = "";

            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                ordermonth = arrs[0];
                orderyears = arrs[1];

                StockAdjustment = (from a in vssp_db.Vw_TRS_StockAdjustment
                                   where a.AdjustmentYear == orderyears && a.AdjustmentMonth == ordermonth && a.AdjustmentNumber.Contains(searchFilter)
                                   orderby a.AdjustmentDate descending, a.EditDate descending
                                   select a).ToList();
            }
            else
            {
                StockAdjustment = (from a in vssp_db.Vw_TRS_StockAdjustment
                                   where a.AdjustmentNumber.Contains(searchFilter)
                                   orderby a.AdjustmentDate descending, a.EditDate descending
                                   select a).ToList();
            }

            if (Adjustmentdate != null)
            {
                StockAdjustment = StockAdjustment.Where(a => a.AdjustmentDate >= Adjustmentdate).ToList();
            }
            if (status != 99)
            {
                StockAdjustment = StockAdjustment.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in StockAdjustment
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                StockAdjustment = StockAdjustment.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            return Json(StockAdjustment, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockAdjustmentDetailListJson(string Adjustmentnumber, string areaid, string locationid, string formAction)
        {
            try
            {

                formAction = _SystemService.Vf(formAction);
                switch (formAction.ToLower())
                {
                    case "create":
                        var Stock = vssp_db.Vw_TRS_StockList.OrderBy(a => a.StockType).ThenBy(a => a.UniqueNumber);
                        return Json(Stock, JsonRequestBehavior.AllowGet);
                    default:
                        var StockAdjustmentDetail = (from a in vssp_db.Vw_TRS_StockAdjustmentDetail
                                                     join b in vssp_db.Vw_TRS_StockList on new { a.CustomerId, a.LineId, a.SupplierId, a.PartNumber } equals new { b.CustomerId, b.LineId, b.SupplierId, b.PartNumber } into stock
                                                     from b in stock.DefaultIfEmpty()
                                                     where a.AdjustmentNumber == Adjustmentnumber || a.AdjustmentNumber.Replace("/", "") == Adjustmentnumber
                                                     select new { a.AdjustmentNumber, a.CustomerId, a.LineId, a.SupplierId, b.StockType, a.UniqueNumber, a.PartNumber, a.PartName, a.AreaId, a.LocationId, a.CategoryId, a.PartModel, a.UnitQty, a.UnitLevel2, a.PackingId, a.StockKanban, a.StockQty, a.ActualQty, a.BalanceQty, a.AdjustmentQty }).ToList();

                        if (_SystemService.Vf(areaid) != "")
                        {
                            StockAdjustmentDetail = StockAdjustmentDetail.Where(a => a.AreaId == areaid).OrderBy(a => new { a.LocationId, a.UniqueNumber }).ToList();
                        }
                        if (_SystemService.Vf(locationid) != "")
                        {
                            StockAdjustmentDetail = StockAdjustmentDetail.Where(a => a.LocationId == locationid).OrderBy(a => new { a.LocationId, a.UniqueNumber }).ToList();
                        }
                        return Json(StockAdjustmentDetail, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult StockAdjustmentApprovalListJson(string Adjustmentnumber, Nullable<bool> approved)
        {
            try
            {

                var StockAdjustmentApproval = from a in vssp_db.Tbl_TRS_StockAdjustmentApproval
                                          where a.AdjustmentNumber.Contains(Adjustmentnumber)
                                          orderby a.ApprovalLevel
                                          select new { a.AdjustmentNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

                if (approved != null)
                {
                    StockAdjustmentApproval = StockAdjustmentApproval.Where(a => a.Approved == approved);
                }

                return Json(StockAdjustmentApproval, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }           
        }
        public ActionResult GetInventoryNumberJson(string InventoryNumber)
        {
            DateTime fdate = DateTime.Now;
            fdate = fdate.AddDays(-30);

            var inventorynumber = (from a in vssp_db.Vw_TRS_StockTaking
                                where (a.InventoryDate >= fdate || a.InventoryNumber == InventoryNumber) && a.Status == 3 && a.Updated == true && (a.ApprovalLevel == 3 || a.ApprovalLevel == 4)
                                orderby a.InventoryDate descending,a.InventoryNumber descending
                                select new { a.InventoryNumber,a.InventoryDate }).ToList();

            return Json(inventorynumber, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudStockAdjustmentList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostStockAdjustmentModel postStockAdjustment = JsonConvert.DeserializeObject<PostStockAdjustmentModel>(jsonData);
                    Tbl_TRS_StockAdjustment StockAdjustment = postStockAdjustment.StockAdjustment;
                    List<crud_StockAdjustmentDetail> StockAdjustmentDetail = postStockAdjustment.StockAdjustmentDetail;

                    string formAction = postStockAdjustment.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":


                            /* Get New Order Number */
                            string CompId = Session["CompID"].ToString();
                            var AdjustmentNumber = vssp_db.SP_GET_StockAdjustmentNumber(StockAdjustment.AdjustmentDate, CompId);
                            foreach (SP_GET_StockAdjustmentNumber_Result number in AdjustmentNumber)
                            {
                                StockAdjustment.AdjustmentNumber = number.AdjustmentNumber;
                            }

                            Tbl_TRS_StockAdjustment ListStockAdjustment = new Tbl_TRS_StockAdjustment();
                            ListStockAdjustment.AdjustmentNumber = StockAdjustment.AdjustmentNumber;
                            ListStockAdjustment.AdjustmentDate = StockAdjustment.AdjustmentDate;
                            ListStockAdjustment.InventoryNumber = StockAdjustment.InventoryNumber;
                            ListStockAdjustment.AreaId = StockAdjustment.AreaId;
                            ListStockAdjustment.LocationId = StockAdjustment.LocationId;
                            ListStockAdjustment.Remarks = StockAdjustment.Remarks;
                            ListStockAdjustment.Status = 0;
                            ListStockAdjustment.UserId = uid;
                            ListStockAdjustment.EditDate = DateTime.Now;

                            vssp_db.Tbl_TRS_StockAdjustment.Add(ListStockAdjustment);

                            /* crud Details */
                            crudStockAdjustmentDetail(StockAdjustmentDetail, StockAdjustment.AdjustmentNumber, formAction);

                            /* crud Approval */
                            crudStockAdjustmentApproval(postStockAdjustment.ApprovalId, StockAdjustment.AdjustmentNumber, uid, formAction);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_StockAdjustment.First(a => a.AdjustmentNumber == StockAdjustment.AdjustmentNumber);

                            ListUpdate.AdjustmentDate = StockAdjustment.AdjustmentDate;
                            ListUpdate.InventoryNumber = StockAdjustment.InventoryNumber;
                            ListUpdate.AreaId = StockAdjustment.AreaId;
                            ListUpdate.LocationId = StockAdjustment.LocationId;
                            ListUpdate.Remarks = StockAdjustment.Remarks;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Details */
                            crudStockAdjustmentDetail(StockAdjustmentDetail, StockAdjustment.AdjustmentNumber, formAction);

                            /* crud Approval */
                            crudStockAdjustmentApproval(postStockAdjustment.ApprovalId, StockAdjustment.AdjustmentNumber, uid, formAction);

                            break;

                        case "actualstock":

                            /* crud Details */
                            crudStockAdjustmentDetail(StockAdjustmentDetail, StockAdjustment.AdjustmentNumber, "Update");

                            break;

                        case "closed":

                            var ListClosed = vssp_db.Tbl_TRS_StockAdjustment.First(a => a.AdjustmentNumber == StockAdjustment.AdjustmentNumber);

                            ListClosed.Status = 3;

                            break;

                        case "canceled":

                            var ListCanceled = vssp_db.Tbl_TRS_StockAdjustment.First(a => a.AdjustmentNumber == StockAdjustment.AdjustmentNumber);

                            ListCanceled.Status = 4;

                            break;

                        case "delete":

                            /* remove existing StockAdjustment */
                            var ListDelete = vssp_db.Tbl_TRS_StockAdjustment.First(a => a.AdjustmentNumber == StockAdjustment.AdjustmentNumber);

                            ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                            //vssp_db.Tbl_TRS_StockAdjustment.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();

                        return Json(StockAdjustment, JsonRequestBehavior.AllowGet);
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

        public void crudStockAdjustmentDetail(List<crud_StockAdjustmentDetail> StockAdjustmentDetails, string AdjustmentNumber, string formAction)
        {

            foreach (var Details in StockAdjustmentDetails)
            {
                if (Details.RowStatus == null && formAction == "create")
                {
                    Details.RowStatus = formAction;
                }
                if (Details.RowStatus != null)
                {
                    switch (Details.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_StockAdjustmentDetail ListDetail = new Tbl_TRS_StockAdjustmentDetail();
                            ListDetail.AdjustmentNumber = AdjustmentNumber;
                            ListDetail.CustomerId = Details.CustomerId;
                            ListDetail.LineId = Details.LineId;
                            ListDetail.SupplierId = Details.SupplierId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.StockKanban = Details.StockKanban;
                            ListDetail.StockQty = Details.StockQty;
                            ListDetail.ActualQty = Details.ActualQty;
                            ListDetail.BalanceQty = Details.BalanceQty;
                            ListDetail.AdjustmentQty = Details.AdjustmentQty;

                            vssp_db.Tbl_TRS_StockAdjustmentDetail.Add(ListDetail);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_StockAdjustmentDetail.First(a => a.AdjustmentNumber == AdjustmentNumber && a.CustomerId == Details.CustomerId && a.LineId == Details.LineId && a.SupplierId == Details.SupplierId && a.PartNumber == Details.PartNumber);

                            ListUpdate.StockKanban = Details.StockKanban;
                            ListUpdate.StockQty = Details.StockQty;
                            ListUpdate.ActualQty = Details.ActualQty;
                            ListUpdate.BalanceQty = Details.BalanceQty;
                            ListUpdate.AdjustmentQty = Details.AdjustmentQty;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_StockAdjustmentDetail.First(a => a.AdjustmentNumber == AdjustmentNumber && a.CustomerId == Details.CustomerId && a.LineId == Details.LineId && a.SupplierId == Details.SupplierId && a.PartNumber == Details.PartNumber);

                            vssp_db.Tbl_TRS_StockAdjustmentDetail.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        
        public void crudStockAdjustmentApproval(string ApprovalId, string AdjustmentNumber, string UserId, string action)
        {
            vssp_db.Database.CommandTimeout = 0;
            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    List<UserApprovalListModel> userApprovalLists = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalLists)
                    {
                        Tbl_TRS_StockAdjustmentApproval ListApproval = new Tbl_TRS_StockAdjustmentApproval();
                        ListApproval.AdjustmentNumber = AdjustmentNumber;
                        ListApproval.UserId = users.UserID;
                        ListApproval.UserName = users.UserName;
                        ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                        ListApproval.ApprovalName = users.ApprovalName;
                        ListApproval.ApprovalEmail = users.Email;
                        ListApproval.SentEmail = false;
                        ListApproval.SentEmailDate = null;
                        if (users.UserID == UserId)
                        {
                            ListApproval.Approved = true;
                            ListApproval.ApprovedDate = DateTime.Now;
                        }
                        else
                        {
                            ListApproval.Approved = false;
                            ListApproval.ApprovedDate = null;
                        }

                        vssp_db.Tbl_TRS_StockAdjustmentApproval.Add(ListApproval);
                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_TRS_StockAdjustmentApproval> UserApproval = vssp_db.Tbl_TRS_StockAdjustmentApproval.Where(a => a.AdjustmentNumber == AdjustmentNumber).ToList();
                    foreach (var user in UserApproval)
                    {
                        UserApprovalListModel ApprovalLists = _AccountService.UserApprovalType(user.UserId, ApprovalId).First(a => a.UserID == user.UserId);
                    }

                    /* create approval */
                    List<UserApprovalListModel> userApprovalListsUpdate = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalListsUpdate)
                    {
                        Tbl_TRS_StockAdjustmentApproval existUser = (from a in vssp_db.Tbl_TRS_StockAdjustmentApproval
                                                                 where a.AdjustmentNumber == AdjustmentNumber && a.UserId == users.UserID
                                                                 select a).FirstOrDefault();
                        if (existUser == null)
                        {
                            Tbl_TRS_StockAdjustmentApproval ListApproval = new Tbl_TRS_StockAdjustmentApproval();
                            ListApproval.AdjustmentNumber = AdjustmentNumber;
                            ListApproval.UserId = users.UserID;
                            ListApproval.UserName = users.UserName;
                            ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                            ListApproval.ApprovalName = users.ApprovalName;
                            ListApproval.ApprovalEmail = users.Email;
                            ListApproval.SentEmail = false;
                            ListApproval.SentEmailDate = null;
                            if (users.UserID == UserId)
                            {
                                ListApproval.Approved = true;
                                ListApproval.ApprovedDate = DateTime.Now;
                            }
                            else
                            {
                                ListApproval.Approved = false;
                                ListApproval.ApprovedDate = null;
                            }

                            vssp_db.Tbl_TRS_StockAdjustmentApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "sent":

                    var ListSent = vssp_db.Tbl_TRS_StockAdjustmentApproval.First(a => a.AdjustmentNumber == AdjustmentNumber && a.UserId == ApprovalId);

                    ListSent.SentEmail = true;
                    ListSent.SentEmailDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "approved":

                    Tbl_TRS_StockAdjustment stockAdjustment = vssp_db.Tbl_TRS_StockAdjustment.Where(a => a.AdjustmentNumber == AdjustmentNumber).FirstOrDefault();

                    stockAdjustment.Status = 2;

                    var ListUpdate = vssp_db.Tbl_TRS_StockAdjustmentApproval.First(a => a.AdjustmentNumber == AdjustmentNumber && a.UserId == ApprovalId);

                    ListUpdate.Approved = true;
                    ListUpdate.ApprovedDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    if (ListUpdate.ApprovalLevel >= 3){

                        //var StockAdjustmentDetail = (from a in vssp_db.Vw_TRS_StockAdjustmentDetail
                        //                             join b in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                        //                             from b in part.DefaultIfEmpty()
                        //                             join c in vssp_db.Tbl_MST_SpecialSupplyPart on b.SSP equals c.Id into ssp
                        //                             from c in ssp.DefaultIfEmpty()
                        //                             where a.AdjustmentNumber == AdjustmentNumber || a.AdjustmentNumber.Replace("/", "") == AdjustmentNumber
                        //                             select new { a.AdjustmentNumber, a.CustomerId, a.LineId, a.SupplierId, b.UniqueNumber, a.PartNumber, b.PartName, b.SSP, c.SSPStock, b.AreaId, b.LocationId, b.CategoryId, b.PartModel, b.UnitQty, b.UnitLevel2, b.PackingId, a.StockKanban, a.StockQty, a.ActualQty, a.BalanceQty, a.AdjustmentQty }).ToList();

                        var StockAdjustmentDetail = (from a in vssp_db.Vw_TRS_StockAdjustmentDetail
                                                     join b in vssp_db.Vw_TRS_StockList on new { a.CustomerId, a.LineId, a.SupplierId, a.PartNumber } equals new { b.CustomerId, b.LineId, b.SupplierId, b.PartNumber }
                                                     where a.AdjustmentNumber == AdjustmentNumber || a.AdjustmentNumber.Replace("/", "") == AdjustmentNumber
                                                     select new { a.AdjustmentNumber, a.CustomerId, a.LineId, a.SupplierId, b.UniqueNumber, a.PartNumber, b.PartName, b.StockType, b.AreaId, b.LocationId, b.CategoryId, b.PartModel, b.UnitQty, b.UnitLevel2, b.PackingId, a.StockKanban, a.StockQty, a.ActualQty, a.BalanceQty, a.AdjustmentQty }).ToList();


                        foreach (var adjustment in StockAdjustmentDetail) {

                            //double kanbanqty = _SystemService.Vn(adjustment.AdjustmentQty.ToString()) / _SystemService.Vn(adjustment.UnitQty.ToString());

                            //PurchaseController purchaseController = new PurchaseController();
                            //ShippingController shippingController = new ShippingController();

                            /* update 2023 12 10 */
                            switch (adjustment.StockType.ToLower())
                            {
                                case "raw":
                                    //purchaseController.crudStock(adjustment.SupplierId, adjustment.PartNumber, _SystemService.Vn(adjustment.UnitQty.ToString()), _SystemService.Vn(adjustment.AdjustmentQty.ToString()), "add", "", true);
                                    vssp_db.SP_CRUD_StockTransaction("", DateTime.Now, adjustment.StockType.ToUpper(), adjustment.AdjustmentNumber, adjustment.SupplierId, adjustment.PartNumber, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), 0, stockAdjustment.UserId, "create");
                                    break;
                                case "wip-ssp":
                                    //purchaseController.crudStockSSP(adjustment.SupplierId, adjustment.PartNumber, kanbanqty, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), "add", "", false, true);
                                    vssp_db.SP_CRUD_StockTransaction("", DateTime.Now, adjustment.StockType.ToUpper(), adjustment.AdjustmentNumber, adjustment.SupplierId, adjustment.PartNumber, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), 0, stockAdjustment.UserId, "create");
                                    break;
                                case "wip-prod":
                                    //purchaseController.crudStockWIP(adjustment.LineId, adjustment.SupplierId, adjustment.PartNumber, kanbanqty, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), "add", "", true);
                                    vssp_db.SP_CRUD_StockTransaction("", DateTime.Now, adjustment.StockType.ToUpper(), adjustment.AdjustmentNumber, adjustment.SupplierId, adjustment.PartNumber, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), 0, stockAdjustment.UserId, "create");
                                    break;
                                case "passthrough":
                                    //purchaseController.crudStock(adjustment.SupplierId, adjustment.PartNumber, _SystemService.Vn(adjustment.UnitQty.ToString()), _SystemService.Vn(adjustment.AdjustmentQty.ToString()), "add", "", true);
                                    vssp_db.SP_CRUD_StockTransaction("", DateTime.Now, adjustment.StockType.ToUpper(), adjustment.AdjustmentNumber, adjustment.SupplierId, adjustment.PartNumber, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), 0, stockAdjustment.UserId, "create");
                                    break;
                                case "finishgoods":
                                    //shippingController.crudStockFG(adjustment.CustomerId, adjustment.PartNumber, kanbanqty, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), "delete", true);
                                    vssp_db.SP_CRUD_StockTransaction("", DateTime.Now, adjustment.StockType.ToUpper(), adjustment.AdjustmentNumber, adjustment.CustomerId, adjustment.PartNumber, _SystemService.Vn(adjustment.AdjustmentQty.ToString()), 0, stockAdjustment.UserId, "create");
                                    break;
                            }

                        }

                    }

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_TRS_StockAdjustmentApproval.First(a => a.AdjustmentNumber == AdjustmentNumber && a.UserId == ApprovalId);

                    vssp_db.Tbl_TRS_StockAdjustmentApproval.Remove(ListDelete);

                    break;
            }
        }

        //public void crudStockSupplierOrder(string ordernumber, string supplierid, string partnumber, float kanbanqty)
        //{
        //    Tbl_TRS_SupplierOrderDetail order = (from a in vssp_db.Tbl_TRS_SupplierOrderDetail
        //                                         where a.OrderNumber==ordernumber && a.SupplierId == supplierid && a.PartNumber == partnumber
        //                                        select a).FirstOrDefault();

        //    if (order != null)
        //    {
        //        order.StockKanban += kanbanqty;

        //        vssp_db.SaveChanges();

        //    }
        //}
        public ActionResult StockAdjustmentApproval(string Adjustmentnumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Inventory Stock Adjustment Approval";

            try
            {

                if (Adjustmentnumber == null || uid == null)
                {
                    Adjustmentnumber = Session["AdjustmentNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["AdjustmentNumber"] = Adjustmentnumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_TRS_StockAdjustment StockAdjustment = vssp_db.Vw_TRS_StockAdjustment.Where(a => a.AdjustmentNumber == Adjustmentnumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_TRS_StockAdjustmentApproval approval = vssp_db.Tbl_TRS_StockAdjustmentApproval.Where(a => a.AdjustmentNumber == Adjustmentnumber && a.UserId == user.UserID).FirstOrDefault();

                    if (StockAdjustment != null && user != null && approval != null)
                    {

                        string Adjustmentdate = _SystemService.Vd(StockAdjustment.AdjustmentDate.ToString(), "dd MMMM, yyyy");

                        ViewBag.OrderTitle = "Inventory Stock Adjustment";
                        ViewBag.AdjustmentNumber = StockAdjustment.AdjustmentNumber;
                        ViewBag.AdjustmentDate = Adjustmentdate;
                        ViewBag.InventoryNumber = StockAdjustment.InventoryNumber;
                        ViewBag.AreaId = StockAdjustment.AreaId;
                        ViewBag.LocationId = StockAdjustment.LocationId;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        if (approval.Approved == false)
                        {
                            return View();
                        }
                        else
                        {
                            ViewBag.ApprovedDate = _SystemService.Vd(approval.ApprovedDate.ToString(), "dd MMMM, yyyy");
                            return View("StockAdjustmentApproved");
                        }

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Inventory Stock Adjustment";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }

        public ActionResult StockAdjustmentApproved(string Adjustmentnumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Inventory Stock Adjustment Approval";

            try
            {

                if (Adjustmentnumber == null || uid == null)
                {
                    Adjustmentnumber = Session["AdjustmentNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["AdjustmentNumber"] = Adjustmentnumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_TRS_StockAdjustment StockAdjustment = vssp_db.Vw_TRS_StockAdjustment.Where(a => a.AdjustmentNumber == Adjustmentnumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();

                    if (StockAdjustment != null && user != null)
                    {
                        uid = user.UserID;
                        string Adjustmentdate = _SystemService.Vd(StockAdjustment.AdjustmentDate.ToString(), "dd MMMM, yyyy");

                        ViewBag.OrderTitle = "Inventory Stock Adjustment";
                        ViewBag.AdjustmentNumber = StockAdjustment.AdjustmentNumber;
                        ViewBag.AdjustmentDate = Adjustmentdate;
                        ViewBag.InventoryNumber = StockAdjustment.InventoryNumber;
                        ViewBag.AreaId = StockAdjustment.AreaId;
                        ViewBag.LocationId = StockAdjustment.LocationId;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        crudStockAdjustmentApproval(user.UserID, StockAdjustment.AdjustmentNumber, user.UserID, "Approved");
                        return RedirectToAction("ContinuePage", "System", new { cmessage = "Successfuly Approved " + ViewBag.OrderTitle + " \n " + Adjustmentnumber, caction = "Dashboard", ccontroller = "Home", capps = "Home" });

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Inventory Stock Adjustment";
                        ViewBag.UserName = null;

                        return RedirectToAction("ErrorPage", "System", new { errnumber = "500", errmessage = "Order or User not valid.", backaction = "MainIndex", backcontroller = "Index" });

                    }

                }
            }
            catch (Exception e)
            {

                var restoreApproval = vssp_db.Tbl_TRS_StockAdjustmentApproval.FirstOrDefault(a => a.AdjustmentNumber == Adjustmentnumber && a.UserId == uid);

                if (restoreApproval != null)
                {
                    restoreApproval.Approved = false;
                    restoreApproval.ApprovedDate = DateTime.Now;

                    vssp_db.SaveChanges();
                }

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }
        public ActionResult GetStockTransactionJson(string year, string month, string stocktype, string ownerid, string partnumber)
        {
            var StockTransaction = vssp_db.SP_GET_StockTransaction(year, month, stocktype, ownerid, partnumber).OrderByDescending(a => a.StockDate).ToList();

            var jsonResult = Json(StockTransaction, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;

        }
        public ActionResult RegenerateStockEarly()
        {
            try
            {
                string uid = Session["UserID"].ToString();
                vssp_db.Database.CommandTimeout = 0;
                vssp_db.SP_CRUD_StockTransactionGenerateEarly(uid);

                return Json("Regenerate stock successed", JsonRequestBehavior.AllowGet);

            }catch(Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);

                return Json(errinfo, JsonRequestBehavior.AllowGet);

            }
        }
    }
}