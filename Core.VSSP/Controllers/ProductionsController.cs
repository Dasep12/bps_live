using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Dapper;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace Core.VSSP.Controllers
{
    public class ProductionsController : Controller
    {
        // GET: Line
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        ProductionsService _ProductionsService = new ProductionsService();
        vssp_entity vssp_db = new vssp_entity();
        QRCodeGeneratorController QRCode = new QRCodeGeneratorController();

        public ActionResult GroupLineListJson(string searchFilter)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Line = (from a in vssp_db.Tbl_MST_LineGroup
                        where (a.GroupId.ToString().Contains(searchFilter) || a.GroupName.Contains(searchFilter))
                        select new { a.GroupId, a.GroupName, a.UserId, a.EditDate }).ToList();

            return Json(Line, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudGroupLineList(string groupId, string groupName, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    switch (formAction.ToLower())
                    {
                        case "create":

                            Tbl_MST_LineGroup LineGroup = new Tbl_MST_LineGroup();
                            LineGroup.GroupName = groupName;
                            LineGroup.UserId = uid;
                            LineGroup.EditDate = DateTime.Now;
                            vssp_db.Tbl_MST_LineGroup.Add(LineGroup);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_LineGroup.First(a => a.GroupId.ToString() == groupId);

                            ListUpdate.GroupName = groupName;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* remove existing LineGroup */
                            var ListDelete = vssp_db.Tbl_MST_LineGroup.First(a => a.GroupId.ToString() == groupId);
                            vssp_db.Tbl_MST_LineGroup.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        if (formAction == "Delete") vssp_db.SP_SYS_ResetIdentity("Tbl_MST_LineGroup", "Id");

                        return Json(groupName, JsonRequestBehavior.AllowGet);
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
        public ActionResult ColorListJson(string searchFilter)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Line = (from a in vssp_db.Tbl_MST_Color
                        where a.ColorName.ToString().Contains(searchFilter)
                        select new { a.ColorName, a.HexNumber, a.UserId, a.EditDate }).ToList();

            return Json(Line, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudColorList(string colorName, string hexNumber, string formAction)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    switch (formAction.ToLower())
                    {
                        case "create":

                            Tbl_MST_Color Color = new Tbl_MST_Color();
                            Color.ColorName = colorName;
                            Color.HexNumber = hexNumber;
                            Color.UserId = uid;
                            Color.EditDate = DateTime.Now;
                            vssp_db.Tbl_MST_Color.Add(Color);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_Color.First(a => a.ColorName.ToString() == colorName);

                            ListUpdate.HexNumber = hexNumber;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* remove existing Color */
                            var ListDelete = vssp_db.Tbl_MST_Color.First(a => a.ColorName.ToString() == colorName);
                            vssp_db.Tbl_MST_Color.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();

                        return Json(colorName, JsonRequestBehavior.AllowGet);
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
        public ActionResult Line()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "Line");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Productions " + _SystemService.Vf(acccessPreviliege.MenuName);
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

        public ActionResult LineListJson(string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            searchFilter = searchFilter.ToLower() == "all" ? "" : searchFilter;
            var Line = (from a in vssp_db.Tbl_MST_Line
                        join b in vssp_db.Tbl_MST_LineGroup on a.GroupId equals b.GroupId into groupline
                        from b in groupline.DefaultIfEmpty()
                        join c in vssp_db.Tbl_MST_Color on a.KanbanColor equals c.ColorName into color
                        from c in color.DefaultIfEmpty()
                        where (a.LineId.Contains(searchFilter) || a.LineName.Contains(searchFilter)) && a.Actived == isActive
                        orderby b.GroupId, a.LineId
                        select new { b.GroupId, b.GroupName, a.LineId, a.LineName, a.AreaId, a.LocationId, a.KanbanColor, c.HexNumber, a.UserId, a.EditDate, a.Actived }).ToList();

            return Json(Line, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LineKanbanCalculationJson(string lineid)
        {
            var Line = (from a in vssp_db.Tbl_MST_KanbanCalculation
                        where a.LineId == lineid
                        orderby a.LineId
                        select new { a.LineId, a.StartDate, a.EndDate, a.InProcess, a.Stock, a.PrepareHeijunka, a.WIP, a.PrepareDelivery }).ToList();

            return Json(Line, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LineGateJson(string lineid, string gateid)
        {
            gateid = _SystemService.Vf(gateid);
            var Line = (from a in vssp_db.Tbl_MST_LineGate
                        where a.LineId == lineid && a.GateId.Contains(gateid)
                        orderby a.LineId
                        select new { a.LineId, a.GateId, a.GateName }).ToList();

            return Json(Line, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudLineList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostLineModel postLine = JsonConvert.DeserializeObject<PostLineModel>(jsonData);
                    List<crud_KanbanCalculation> kanbanCalculations = postLine.KanbanCalculation;
                    List<crud_LineGate> lineGates = postLine.LineGate;
                    Tbl_MST_Line Line = postLine.Line;

                    string formAction = postLine.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":

                            Tbl_MST_Line ListLine = new Tbl_MST_Line();
                            ListLine.GroupId = Line.GroupId;
                            ListLine.LineId = Line.LineId;
                            ListLine.LineName = Line.LineName;
                            ListLine.AreaId = Line.AreaId;
                            ListLine.LocationId = Line.LocationId;
                            ListLine.KanbanColor = Line.KanbanColor;
                            ListLine.Actived = Line.Actived;
                            ListLine.UserId = uid;
                            ListLine.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_Line.Add(ListLine);

                            /* crud Kanban Calculation */
                            crudKanbanCalculation(kanbanCalculations, Line.LineId);
                            /* crud Line Gate */
                            crudLineGate(lineGates, Line.LineId);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_Line.First(a => a.LineId == Line.LineId);

                            ListUpdate.LineName = Line.LineName;
                            ListUpdate.AreaId = Line.AreaId;
                            ListUpdate.LocationId = Line.LocationId;
                            ListUpdate.KanbanColor = Line.KanbanColor;
                            ListUpdate.Actived = Line.Actived;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Kanban Calculation */
                            crudKanbanCalculation(kanbanCalculations, Line.LineId);
                            /* crud Line Gate */
                            crudLineGate(lineGates, Line.LineId);

                            break;

                        case "delete":

                            /* remove existing Kanban Calculation */
                            var deleteKanbanCalculation = from a in vssp_db.Tbl_MST_KanbanCalculation
                                                          where a.LineId == Line.LineId
                                                          select a;

                            deleteKanbanCalculation.ForEach(kanban =>
                            {
                                vssp_db.Tbl_MST_KanbanCalculation.Remove(kanban);
                            });
                            /* remove existing Line Gate */
                            var deleteLineGate = from a in vssp_db.Tbl_MST_LineGate
                                                          where a.LineId == Line.LineId
                                                          select a;

                            deleteLineGate.ForEach(line =>
                            {
                                vssp_db.Tbl_MST_LineGate.Remove(line);
                            });

                            /* remove existing Line */
                            var ListDelete = vssp_db.Tbl_MST_Line.First(a => a.LineId == Line.LineId);
                            vssp_db.Tbl_MST_Line.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(Line, JsonRequestBehavior.AllowGet);
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
        public void crudLineGate(List<crud_LineGate> LineGates, string LineId)
        {

            foreach (var Gates in LineGates)
            {
                if (Gates.RowStatus != null)
                {
                    switch (Gates.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Gates */
                            Tbl_MST_LineGate ListGate = new Tbl_MST_LineGate();
                            ListGate.LineId = LineId;
                            ListGate.GateId = Gates.GateId;
                            ListGate.GateName = Gates.GateName;

                            vssp_db.Tbl_MST_LineGate.Add(ListGate);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_LineGate.First(a => a.LineId == LineId && a.GateId == Gates.GateId);

                            ListUpdate.GateName = Gates.GateName;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_MST_LineGate.First(a => a.LineId == LineId && a.GateId == Gates.GateId);

                            vssp_db.Tbl_MST_LineGate.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        public void crudKanbanCalculation(List<crud_KanbanCalculation> KanbanCalculations, string LineId)
        {

            foreach (var Kanbans in KanbanCalculations)
            {
                if (Kanbans.RowStatus != null)
                {
                    switch (Kanbans.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Kanbans */
                            Tbl_MST_KanbanCalculation ListKanban = new Tbl_MST_KanbanCalculation();
                            ListKanban.LineId = LineId;
                            ListKanban.StartDate = Kanbans.StartDate;
                            ListKanban.EndDate = Kanbans.EndDate;
                            ListKanban.InProcess = Kanbans.InProcess;
                            ListKanban.Stock = Kanbans.Stock;
                            ListKanban.PrepareHeijunka = Kanbans.PrepareHeijunka;
                            ListKanban.WIP = Kanbans.WIP;
                            ListKanban.PrepareDelivery = Kanbans.PrepareDelivery;

                            vssp_db.Tbl_MST_KanbanCalculation.Add(ListKanban);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_KanbanCalculation.First(a => a.LineId == LineId && a.StartDate == Kanbans.StartDate);

                            ListUpdate.EndDate = Kanbans.EndDate;
                            ListUpdate.InProcess = Kanbans.InProcess;
                            ListUpdate.Stock = Kanbans.Stock;
                            ListUpdate.PrepareHeijunka = Kanbans.PrepareHeijunka;
                            ListUpdate.WIP = Kanbans.WIP;
                            ListUpdate.PrepareDelivery = Kanbans.PrepareDelivery;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_MST_KanbanCalculation.First(a => a.LineId == LineId && a.StartDate == Kanbans.StartDate);

                            vssp_db.Tbl_MST_KanbanCalculation.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        public ActionResult ProductionMaterials()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "ProductionMaterials");

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

        public ActionResult ProductionMaterialsListJson(string searchFilter, string FormAction = "", string LineId = "",
                                        string PartNumber = "", string UniqueNumber = "", string UniqueNotInclude = null,
                                        string PartNotInclude = null, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            if (searchFilter != "validator")
            {

                var ProductionMaterials = from a in vssp_db.Tbl_MST_PartProductionMaterials
                                          where a.Actived == isActive && (a.LineId.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter))
                                          orderby a.LineId, a.PartNumber
                                          select new { ProductionMaterialKey = (a.LineId + a.PartNumber), a.LineId, a.PartNumber,
                                              a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.PackingId, a.AreaId, a.LocationId, a.UnitLevel1,
                                              a.UnitLevel2, a.UnitQty, a.SafetyHours, a.SubProcess, a.Actived, a.UserId, a.EditDate };

                if (LineId != "")
                {
                    ProductionMaterials = ProductionMaterials.Where(a => a.LineId == LineId);
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
                    ProductionMaterials = ProductionMaterials.Where(a => !exceptionList.Contains(a.UniqueNumber));
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
                    ProductionMaterials = ProductionMaterials.Where(a => !exceptionList.Contains(a.PartNumber));
                }

                return Json(ProductionMaterials, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var ProductionMaterials = new object();

                switch (FormAction)
                {
                    case "Create":

                        ProductionMaterials = from a in vssp_db.Tbl_MST_PartProductionMaterials
                                              where a.LineId == LineId && (a.PartNumber == PartNumber || a.UniqueNumber == UniqueNumber)
                                              orderby a.LineId, a.PartNumber
                                              select new { ProductionMaterialKey = (a.LineId + a.PartNumber), a.LineId, a.PartNumber, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Actived, a.UserId, a.EditDate };
                        break;

                    case "Update":

                        ProductionMaterials = from a in vssp_db.Tbl_MST_PartProductionMaterials
                                              where a.PartNumber != PartNumber && a.LineId == LineId && a.UniqueNumber == UniqueNumber
                                              orderby a.LineId, a.PartNumber
                                              select new { ProductionMaterialKey = (a.LineId + a.PartNumber), a.LineId, a.PartNumber, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Actived, a.UserId, a.EditDate };
                        break;

                    default:

                        ProductionMaterials = from a in vssp_db.Tbl_MST_PartProductionMaterials
                                              where a.LineId == "*"
                                              orderby a.LineId, a.PartNumber
                                              select new { ProductionMaterialKey = (a.LineId + a.PartNumber), a.LineId, a.PartNumber, a.UniqueNumber, a.PartName, a.PartModel, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Actived, a.UserId, a.EditDate };

                        break;
                }
                return Json(ProductionMaterials, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ProductionMaterialsPriceListJson(string key)
        {
            key = _SystemService.Vf(key);
            var ProductionMaterialsPrice = from a in vssp_db.Tbl_MST_PartProductionMaterialsPrice
                                           where (a.LineId + a.PartNumber) == key
                                           orderby a.LineId, a.PartNumber, a.StartDate descending
                                           select new { ProductionMaterialKey = (a.LineId + a.PartNumber), PriceId = a.StartDate, a.LineId, a.PartNumber, a.StartDate, a.EndDate, a.Price, a.UserId, a.EditDate };
            return Json(ProductionMaterialsPrice, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ImportJson(string formaction, string canConfidential)
        {
            try {
                if (formaction == "Line")
                {
                    ImportProductionsListModel List = new ImportProductionsListModel();
                    return Json(List, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "Line-validation")
                {
                    HttpFileCollectionBase files = Request.Files;
                    var ListUpload = _ProductionsService.uploadLineExcel(files[0]);
                    return Json(ListUpload, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "ProductionMaterial")
                {
                    ImportProductionMaterialModel ProductionMaterials = new ImportProductionMaterialModel();
                    return Json(ProductionMaterials, JsonRequestBehavior.AllowGet);
                }
                else
                if (formaction == "ProductionMaterial-validation")
                {
                    HttpFileCollectionBase files = Request.Files;
                    var ProductionMaterialsUpload = _ProductionsService.uploadProductionMaterialExcel(files[0], canConfidential);
                    return Json(ProductionMaterialsUpload, JsonRequestBehavior.AllowGet);
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
                try {
                    if (formaction == "Line")
                    {
                        string userId = Session["UserID"].ToString();
                        HttpFileCollectionBase files = Request.Files;
                        var ListUpload = _ProductionsService.crudImportLineExcel(replace, userId, files[0]);
                        return Json(ListUpload, JsonRequestBehavior.AllowGet);
                    }
                    else
                    if (formaction == "ProductionMaterial")
                    {
                        string userId = Session["UserID"].ToString();
                        HttpFileCollectionBase files = Request.Files;
                        var ListUpload = _ProductionsService.crudImportProductionMaterialExcel(replace, userId, files[0], canConfidential);
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

        public ActionResult crudProductionMaterialList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostProductionMaterialModel postProductionMaterial = JsonConvert.DeserializeObject<PostProductionMaterialModel>(jsonData);
                    Tbl_MST_PartProductionMaterials ProductionMaterial = postProductionMaterial.ProductionMaterial;
                    List<crud_PartProductionMaterialsPrice> ProductionMaterialPrice = postProductionMaterial.ProductionMaterialPrice;
                    string formAction = postProductionMaterial.formAction.ToLower();

                    Tbl_MST_PartProductionMaterials ListProductionMaterial = new Tbl_MST_PartProductionMaterials();
                    ListProductionMaterial.LineId = ProductionMaterial.LineId;
                    ListProductionMaterial.PartNumber = ProductionMaterial.PartNumber;
                    ListProductionMaterial.UniqueNumber = ProductionMaterial.UniqueNumber;
                    ListProductionMaterial.PartName = ProductionMaterial.PartName;
                    ListProductionMaterial.PartModel = ProductionMaterial.PartModel;
                    ListProductionMaterial.CategoryId = ProductionMaterial.CategoryId;
                    ListProductionMaterial.PackingId = ProductionMaterial.PackingId;
                    ListProductionMaterial.AreaId = ProductionMaterial.AreaId;
                    ListProductionMaterial.LocationId = ProductionMaterial.LocationId;
                    ListProductionMaterial.UnitLevel1 = ProductionMaterial.UnitLevel1;
                    ListProductionMaterial.UnitLevel2 = ProductionMaterial.UnitLevel2;
                    ListProductionMaterial.UnitQty = ProductionMaterial.UnitQty;
                    ListProductionMaterial.SafetyHours = ProductionMaterial.SafetyHours;
                    ListProductionMaterial.SubProcess = ProductionMaterial.SubProcess;
                    ListProductionMaterial.Actived = ProductionMaterial.Actived;
                    ListProductionMaterial.UserId = uid;
                    ListProductionMaterial.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_PartProductionMaterials.Add(ListProductionMaterial);

                            /* crud Prices */
                            crudProductionMaterialPrice(ProductionMaterialPrice, ProductionMaterial.LineId, ProductionMaterial.PartNumber, uid);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_PartProductionMaterials.First(a => a.LineId == ProductionMaterial.LineId && a.PartNumber == ProductionMaterial.PartNumber);

                            ListUpdate.LineId = ProductionMaterial.LineId;
                            ListUpdate.PartNumber = ProductionMaterial.PartNumber;
                            ListUpdate.UniqueNumber = ProductionMaterial.UniqueNumber;
                            ListUpdate.PartName = ProductionMaterial.PartName;
                            ListUpdate.PartModel = ProductionMaterial.PartModel;
                            ListUpdate.CategoryId = ProductionMaterial.CategoryId;
                            ListUpdate.PackingId = ProductionMaterial.PackingId;
                            ListUpdate.AreaId = ProductionMaterial.AreaId;
                            ListUpdate.LocationId = ProductionMaterial.LocationId;
                            ListUpdate.UnitLevel1 = ProductionMaterial.UnitLevel1;
                            ListUpdate.UnitLevel2 = ProductionMaterial.UnitLevel2;
                            ListUpdate.UnitQty = ProductionMaterial.UnitQty;
                            ListUpdate.SafetyHours = ProductionMaterial.SafetyHours;
                            ListUpdate.SubProcess = ProductionMaterial.SubProcess;
                            ListUpdate.Actived = ProductionMaterial.Actived;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Prices */
                            crudProductionMaterialPrice(ProductionMaterialPrice, ProductionMaterial.LineId, ProductionMaterial.PartNumber, uid);

                            break;

                        case "delete":

                            /* remove existing Prices */
                            var deletePrice = from a in vssp_db.Tbl_MST_PartProductionMaterialsPrice
                                              where a.LineId == ProductionMaterial.LineId && a.PartNumber == ProductionMaterial.PartNumber
                                              select a;

                            deletePrice.ForEach(Prices =>
                            {
                                vssp_db.Tbl_MST_PartProductionMaterialsPrice.Remove(Prices);
                            });

                            /* remove existing Part Finish Good */
                            var ListDelete = vssp_db.Tbl_MST_PartProductionMaterials.First(a => a.LineId == ProductionMaterial.LineId && a.PartNumber == ProductionMaterial.PartNumber);

                            vssp_db.Tbl_MST_PartProductionMaterials.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(ProductionMaterial, JsonRequestBehavior.AllowGet);
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

        public void crudProductionMaterialPrice(List<crud_PartProductionMaterialsPrice> ProductionMaterialPrices, string LineId, string PartNumber, string uid)
        {

            foreach (var Prices in ProductionMaterialPrices)
            {
                if (Prices.RowStatus != null)
                {
                    switch (Prices.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Prices */
                            Tbl_MST_PartProductionMaterialsPrice ListPrice = new Tbl_MST_PartProductionMaterialsPrice();
                            ListPrice.LineId = LineId;
                            ListPrice.PartNumber = PartNumber;
                            ListPrice.StartDate = Prices.StartDate;
                            ListPrice.EndDate = Prices.EndDate;
                            ListPrice.Price = Prices.Price;
                            ListPrice.UserId = uid;
                            ListPrice.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_PartProductionMaterialsPrice.Add(ListPrice);

                            break;

                        case "update":

                            /* update Prices */
                            var ListUpdate = vssp_db.Tbl_MST_PartProductionMaterialsPrice.First(a => a.LineId == LineId && a.PartNumber == PartNumber && a.StartDate == Prices.StartDate);

                            ListUpdate.EndDate = Prices.EndDate;
                            ListUpdate.Price = Prices.Price;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* delete Prices */
                            var ListDelete = vssp_db.Tbl_MST_PartProductionMaterialsPrice.First(a => a.LineId == LineId && a.PartNumber == PartNumber && a.StartDate == Prices.StartDate);

                            vssp_db.Tbl_MST_PartProductionMaterialsPrice.Remove(ListDelete);

                            break;
                    }
                }
            }
        }
        public ActionResult BillOfMaterials()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "BillOfMaterials");

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
        public ActionResult BillOfMaterialsJson(string searchFilter, string lineid, string partRawMaterial, bool isActive = true, bool passthrough = false)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var bom = (from a in vssp_db.Vw_MST_BillOfMaterials
                       where (a.CustomerId.Contains(searchFilter) || a.LineId.Contains(searchFilter) || a.LineName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter)) && a.Actived == isActive && a.PassThrough == passthrough
                       orderby a.LineId
                       select new { a.CustomerId, a.CustomerName, a.UniqueNumber, a.PartNumber, a.PartName, a.LineId, a.LineName, a.Revision, a.TotalItem, a.TotalCost, a.Remarks, a.PassThrough, a.Actived, a.UserId, a.EditDate }).ToList();

            if(_SystemService.Vf(lineid) != "")
            {
                bom = bom.Where(a => a.LineId == lineid).ToList();
            }
            if(_SystemService.Vf(partRawMaterial) != "")
            {
                var bomraw = (from a in vssp_db.Tbl_MST_PartBillOfMaterialsDetails
                              where a.PartNumber == partRawMaterial
                              select new { a.PartNumberParent }).ToList();

                var acceptionList = new List<string>();
                foreach (var raw in bomraw)
                {
                    acceptionList.Add(raw.PartNumberParent);
                }
                bom = bom.Where(a => acceptionList.Contains(a.PartNumber)).ToList();
            }
            return Json(bom, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BillOfMaterialsImages(string partnumber)
        {
            try {
                var bom = (from a in vssp_db.Tbl_MST_PartBillOfMaterials
                           where a.PartNumber == partnumber
                           select new { a.ImagesLocation }).FirstOrDefault();

                if (bom != null)
                {
                    var jsonResult = Json(bom, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult BillOfMaterialsDetailsJson(string lineid, string customerid, string parentid)
        {
            var boms = (from a in vssp_db.Vw_MST_BillOfMaterialsDetails
                        where a.LineId == lineid && a.CustomerId == customerid && a.BOMNumber == parentid
                        orderby a.BOMLevel, a.PartNumber
                        select a).ToList();

            return Json(boms, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PartMaterialsJson(int partLevel, string includepart, bool passthrough = false)
        {

            includepart = _SystemService.Vf(includepart);
            var partMaterials = (from a in vssp_db.Vw_MST_PartMaterials
                                 where a.PartType == partLevel && a.Passthrough == passthrough
                                 orderby a.UniqueNumber, a.PartNumber
                                 select a).ToList();

            if (partLevel == 0)
            {
                var exceptionList = new List<string>();
                var existBOM = (from a in vssp_db.Tbl_MST_PartBillOfMaterials
                                where a.Actived == true
                                select new { a.PartNumber }).ToList();
                foreach (var bom in existBOM)
                {
                    if (includepart != "")
                    {
                        if (bom.PartNumber != includepart) exceptionList.Add(bom.PartNumber);
                    } else
                    {
                        exceptionList.Add(bom.PartNumber);
                    }

                }

                partMaterials = partMaterials.Where(a => !exceptionList.Contains(a.PartNumber)).ToList();
            }
            return Json(partMaterials, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PartTypeJson(int exludeLevel = 99)
        {
            var partTypes = (from a in vssp_db.Tbl_MST_PartType
                             orderby a.Id
                             select new { a.Id, a.Name }).ToList();

            if (exludeLevel != 99)
            {
                partTypes = partTypes.Where(a => a.Id != exludeLevel && a.Id != 4).ToList();
            }

            return Json(partTypes, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBOMDetailsJson(string PartNumberParent, int BOMLevel, string PartNumber, string Revision, float Qty, string Remarks)
        {
            var parts = (from a in vssp_db.Vw_MST_PartMaterials
                         where a.PartType == BOMLevel && a.PartNumber == PartNumber
                         select a).FirstOrDefault();
            var bomdetails = new Vw_MST_BillOfMaterialsDetails();
            if (parts != null)
            {
                bomdetails.BOMNumber = PartNumberParent;
                bomdetails.BOMLevel = parts.PartType;
                bomdetails.Level01 = parts.PartType == 1 ? true : false;
                bomdetails.Level02 = parts.PartType == 2 ? true : false;
                bomdetails.Level03 = parts.PartType == 3 ? true : false;
                bomdetails.Level04 = parts.PartType == 4 ? true : false;
                bomdetails.PartNumber = parts.PartNumber;
                bomdetails.PartName = parts.PartName;
                bomdetails.Revision = Revision;
                bomdetails.LevelName = parts.PartTypeName;
                bomdetails.CostUnit = parts.Price;
                bomdetails.Qty = Qty;
                bomdetails.Unit = parts.UnitLevel2;
                bomdetails.Cost = (parts.Price * Qty);
                bomdetails.Remarks = Remarks;
                bomdetails.Passthrough = parts.Passthrough;
            }

            return Json(bomdetails, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudBillOfMaterials(string jsonData)
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

                    PostBillOfMaterialsModel postBillOfMaterials = JsonConvert.DeserializeObject<PostBillOfMaterialsModel>(jsonData);
                    Tbl_MST_PartBillOfMaterials BillOfMaterials = postBillOfMaterials.PartBillOfMaterials;
                    List<Vw_MST_BillOfMaterialsDetails> BillOfMaterialsDetails = postBillOfMaterials.BillOfMaterialsDetails;
                    string formAction = postBillOfMaterials.formAction.ToLower();

                    if (file != null)
                    {
                        //BillOfMaterials.Images = _SystemService.ConvertToBytes(file);
                        string path = "";
                        string filename = "";
                        string extention = "";
                        string savefile = "";
                        string imagelocation = "";

                        extention = Path.GetExtension(file.FileName);
                        filename = BillOfMaterials.PartNumber + extention;
                        path = Server.MapPath("~/_VSSPAssets/Images/apps/BillOfmaterials");
                        savefile = Path.Combine(path, filename);
                        imagelocation = "../_VSSPAssets/Images/apps/BillOfmaterials/" + filename;

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        if (System.IO.File.Exists(savefile))
                        {
                            System.IO.File.Delete(savefile);
                        }
                        if (System.IO.File.Exists(BillOfMaterials.ImagesLocation))
                        {
                            System.IO.File.Delete(BillOfMaterials.ImagesLocation);
                        }

                        file.SaveAs(savefile);
                        BillOfMaterials.ImagesLocation = imagelocation;
                    }

                    switch (formAction)
                    {
                        case "create":

                            Tbl_MST_PartBillOfMaterials ListBillOfMaterials = new Tbl_MST_PartBillOfMaterials();
                            ListBillOfMaterials.CustomerId = BillOfMaterials.CustomerId;
                            ListBillOfMaterials.LineId = BillOfMaterials.LineId;
                            ListBillOfMaterials.PartNumber = BillOfMaterials.PartNumber;
                            ListBillOfMaterials.Revision = BillOfMaterials.Revision;
                            ListBillOfMaterials.Remarks = BillOfMaterials.Remarks;
                            ListBillOfMaterials.Actived = BillOfMaterials.Actived;
                            ListBillOfMaterials.UserId = uid;
                            ListBillOfMaterials.EditDate = DateTime.Now;
                            ListBillOfMaterials.ImagesLocation = BillOfMaterials.ImagesLocation;

                            vssp_db.Tbl_MST_PartBillOfMaterials.Add(ListBillOfMaterials);

                            /* crud Details */
                            crudBillOfMaterialsDetails(BillOfMaterialsDetails, BillOfMaterials, uid, formAction);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_PartBillOfMaterials.First(a => a.LineId == BillOfMaterials.LineId && a.CustomerId == BillOfMaterials.CustomerId && a.PartNumber == BillOfMaterials.PartNumber);

                            ListUpdate.Revision = BillOfMaterials.Revision;
                            ListUpdate.Remarks = BillOfMaterials.Remarks;
                            ListUpdate.ImagesLocation = BillOfMaterials.ImagesLocation != null ? BillOfMaterials.ImagesLocation : ListUpdate.ImagesLocation;
                            ListUpdate.Actived = BillOfMaterials.Actived;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Details */
                            crudBillOfMaterialsDetails(BillOfMaterialsDetails, BillOfMaterials, uid, formAction);

                            break;

                        case "delete":


                            /* crud Details */
                            crudBillOfMaterialsDetails(BillOfMaterialsDetails, BillOfMaterials, uid, formAction);

                            /* remove existing Part Finish Good */
                            var ListDelete = vssp_db.Tbl_MST_PartBillOfMaterials.First(a => a.LineId == BillOfMaterials.LineId && a.CustomerId == BillOfMaterials.CustomerId && a.PartNumber == BillOfMaterials.PartNumber);

                            vssp_db.Tbl_MST_PartBillOfMaterials.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        var jsonResult = Json(BillOfMaterials, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue;

                        return jsonResult;
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

        public void crudBillOfMaterialsDetails(List<Vw_MST_BillOfMaterialsDetails> BillOfMaterialsDetails, Tbl_MST_PartBillOfMaterials BillOfMaterials, string uid, string formAction)
        {

            var ListDelete = vssp_db.Tbl_MST_PartBillOfMaterialsDetails.Where(a => a.LineId == BillOfMaterials.LineId && a.CustomerId == BillOfMaterials.CustomerId && a.PartNumberParent == BillOfMaterials.PartNumber).ToList();
            foreach (var delete in ListDelete)
            {
                vssp_db.Tbl_MST_PartBillOfMaterialsDetails.Remove(delete);
            }

            foreach (var Details in BillOfMaterialsDetails)
            {
                switch (formAction.ToLower())
                {
                    case "create":

                        /* create Details */
                        Tbl_MST_PartBillOfMaterialsDetails ListAdd = new Tbl_MST_PartBillOfMaterialsDetails();
                        ListAdd.LineId = BillOfMaterials.LineId;
                        ListAdd.CustomerId = BillOfMaterials.CustomerId;
                        ListAdd.PartNumberParent = BillOfMaterials.PartNumber;
                        ListAdd.PartNumber = Details.PartNumber;
                        ListAdd.PartType = Details.BOMLevel;
                        ListAdd.Revision = Details.Revision;
                        ListAdd.Qty = Details.Qty;
                        ListAdd.Remarks = Details.Remarks;
                        ListAdd.Passthrough = Details.Passthrough;

                        vssp_db.Tbl_MST_PartBillOfMaterialsDetails.Add(ListAdd);

                        break;

                    case "update":

                        /* update Details */
                        Tbl_MST_PartBillOfMaterialsDetails ListUpdate = new Tbl_MST_PartBillOfMaterialsDetails();
                        ListUpdate.LineId = BillOfMaterials.LineId;
                        ListUpdate.CustomerId = BillOfMaterials.CustomerId;
                        ListUpdate.PartNumberParent = BillOfMaterials.PartNumber;
                        ListUpdate.PartNumber = Details.PartNumber;
                        ListUpdate.PartType = Details.BOMLevel;
                        ListUpdate.Revision = Details.Revision;
                        ListUpdate.Qty = Details.Qty;
                        ListUpdate.Remarks = Details.Remarks;
                        ListUpdate.Passthrough = Details.Passthrough;

                        vssp_db.Tbl_MST_PartBillOfMaterialsDetails.Add(ListUpdate);

                        break;

                    case "delete":

                        //nothing

                        break;
                }
            }
        }
        public ActionResult ControlPlanning()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "ControlPlanning");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Production Control & Planning";
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;
                    ViewBag.DateTime = DateTime.Now;

                    ControlPlanningListModel ControlPlanning = new ControlPlanningListModel();
                    ControlPlanning.ExportList = _SystemService.ComboExport().ToList();
                    ControlPlanning.StatusList = (from a in vssp_db.Tbl_TRS_Status
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

                        return View(ControlPlanning);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ControlPlanningListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_ControlPlanning> ControlPlanning = (from a in vssp_db.Vw_TRS_ControlPlanning
                                                            where a.OrderNumber.Contains(searchFilter) || a.LineId.Contains(searchFilter)
                                                            orderby a.OrderYear descending, a.OrderMonth descending, a.OrderDate descending, a.EditDate descending, a.OrderNumber
                                                            select a).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                ControlPlanning = ControlPlanning.Where(a => a.OrderDate >= startdate && a.OrderDate <= enddate).ToList();
            }
            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string ordermonth = arrs[0];
                string orderyears = arrs[1];
                ControlPlanning = ControlPlanning.Where(a => a.OrderMonth == ordermonth && a.OrderYear == orderyears).ToList();
            }
            if (status != 99)
            {
                ControlPlanning = ControlPlanning.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in ControlPlanning
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                ControlPlanning = ControlPlanning.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            return Json(ControlPlanning, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ProductionScheduleJson(string ordernumber, string searchfilter, Nullable<DateTime> startdate = null, Nullable<DateTime> enddate = null)
        {
            searchfilter = _SystemService.Vf(searchfilter);
            DateTime now = DateTime.Now;
            if (startdate == null)
            {
                startdate = new DateTime(now.Year, now.Month, 1);
            }
            if (enddate == null)
            {
                enddate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
            }

            string DBMaster = ConfigurationManager.ConnectionStrings["DBMaster"].ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(DBMaster);

            string databaseName = vssp_db.Database.Connection.Database;

            var connetionString = "Data Source=" + builder.DataSource + ";Initial Catalog=" + databaseName + ";User ID=" + builder.UserID + ";Password=" + builder.Password + "";

            IEnumerable<IDictionary<string, object>> ShiftSchedule;

            using (var cnn = new SqlConnection(connetionString))
            {
                cnn.Open();

                var p = new DynamicParameters();
                p.Add("@StartDate", startdate, DbType.DateTime);
                p.Add("@EndDate", enddate, DbType.DateTime);
                p.Add("@OrderNumber", ordernumber, DbType.String);
                p.Add("@SearchFilter", searchfilter, DbType.String);

                ShiftSchedule = (IEnumerable<IDictionary<string, object>>)
                            cnn.Query(sql: "SP_TRS_ScheduleProduction",
                                      param: p,
                                      commandType: CommandType.StoredProcedure);
            }

            return Json(ShiftSchedule, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ControlPlanningDetailListJson(string ordernumber, string month, string lineid, int shift = 0, string formAction = "")
        {
            try
            {

                var ControlPlanningDetail = new List<SP_TRS_ControlPlanningDetailLast_Result>();

                if (_SystemService.Vf(lineid) != "" && _SystemService.Vf(month) != "" && _SystemService.Vn(shift.ToString()) != 0)
                {

                    string[] arrs = _SystemService.Vf(month).Split('/');
                    string ordermonth = "";
                    string orderyears = "";

                    if (_SystemService.Vf(month) != "")
                    {
                        ordermonth = arrs[0];
                        orderyears = arrs[1];
                    }

                    switch (_SystemService.Vf(formAction).ToLower())
                    {
                        case "create":

                            ControlPlanningDetail = vssp_db.SP_TRS_ControlPlanningDetailLast(lineid, ordermonth, orderyears, shift, ordernumber).ToList();
                            break;

                        case "regenerate":

                            var ControlPlanningDetailAdd = vssp_db.SP_TRS_ControlPlanningDetailAdditional(lineid, ordermonth, orderyears, shift, ordernumber).ToList();
                            foreach(var additional in ControlPlanningDetailAdd)
                            {
                                SP_TRS_ControlPlanningDetailLast_Result result = new SP_TRS_ControlPlanningDetailLast_Result();
                                result.OrderNumber = additional.OrderNumber;
                                result.LineId = additional.LineId;
                                result.CustomerId = additional.CustomerId;
                                result.PartNumber = additional.PartNumber;
                                result.UniqueNumber = additional.UniqueNumber;
                                result.PartName = additional.PartName;
                                result.Model = additional.Model;
                                result.QtyByKanban = additional.QtyByKanban;
                                result.Units = additional.Units;
                                result.Unit = additional.Unit;
                                result.PackingId = additional.PackingId;
                                result.OrderQty = additional.OrderQty;
                                result.N1 = additional.N1;
                                result.N2 = additional.N2;
                                result.N3 = additional.N3;
                                result.DailyQty = additional.DailyQty;
                                result.ShiftQty = additional.ShiftQty;
                                result.SONumber = additional.SONumber;

                                ControlPlanningDetail.Add(result);
                            }
                            break;

                        default:

                            ControlPlanningDetail = vssp_db.SP_TRS_ControlPlanningDetailLast(lineid, ordermonth, orderyears, shift, ordernumber).ToList();
                            break;
                    }

                }

                if (ControlPlanningDetail != null)
                {
                    foreach (var planning in ControlPlanningDetail)
                    {
                        planning.ShiftQty = planning.DailyQty / shift;
                    }
                }

                return Json(ControlPlanningDetail, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult checkPlanningActived(string lineid, string month)
        {
            string[] arrs = _SystemService.Vf(month).Split('/');
            string ordermonth = "";
            string orderyears = "";

            if (_SystemService.Vf(month) != "")
            {
                ordermonth = arrs[0];
                orderyears = arrs[1];
            }

            var checkForecast = (from a in vssp_db.Vw_TRS_ControlPlanning
                                 where a.LineId == lineid && a.OrderYear == orderyears && a.OrderMonth == ordermonth &&
                                 (a.Status.ToString().Contains("0") || a.Status.ToString().Contains("1") || a.Status.ToString().Contains("2") || a.Status.ToString().Contains("3"))
                                 select a).ToList();

            return Json(checkForecast, JsonRequestBehavior.AllowGet);

        }

        public ActionResult FinishGoodsLineListJson(
                            string searchFilter, string LineId = "",
                            string PartNotInclude = null, string PassThrough = null, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            var FinishGoods = from a in vssp_db.Tbl_MST_PartFinishGoods
                              join b in vssp_db.Tbl_MST_PartBillOfMaterials on a.PartNumber equals b.PartNumber into bom
                              from b in bom.DefaultIfEmpty()
                              where a.Actived == isActive && b.LineId != null && (b.LineId.Contains(searchFilter) || a.CustomerId.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter))
                              orderby b.LineId, a.PartNumber
                              select new { a.CustomerId, b.LineId, a.CustomerUnitModel, a.PartNumber, a.PartNumberCustomer, a.UniqueNumber, a.PartName, a.CategoryId, a.PackingId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.PassThrough, a.Actived, a.UserId, a.EditDate };

            if (_SystemService.Vf(LineId) != "")
            {
                FinishGoods = FinishGoods.Where(a => a.LineId == LineId);
            }
            if (_SystemService.Vf(PassThrough) != "")
            {
                var _passThrough = _SystemService.Vb(PassThrough);
                FinishGoods = FinishGoods.Where(a => a.PassThrough == _passThrough);
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
                FinishGoods = FinishGoods.Where(a => !exceptionList.Contains(a.PartNumber));
            }

            return Json(FinishGoods, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ControlPlanningApprovalListJson(string ordernumber, Nullable<bool> approved)
        {
            try
            {

                var ControlPlanningApproval = from a in vssp_db.Tbl_TRS_ControlPlanningApproval
                                              where a.OrderNumber.Contains(ordernumber)
                                              orderby a.ApprovalLevel
                                              select new { a.OrderNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

                if (approved != null)
                {
                    ControlPlanningApproval = ControlPlanningApproval.Where(a => a.Approved == approved);
                }

                return Json(ControlPlanningApproval, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudControlPlanningList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostControlPlanningModel postControlPlanning = JsonConvert.DeserializeObject<PostControlPlanningModel>(jsonData);
                    Tbl_TRS_ControlPlanning ControlPlanning = postControlPlanning.ControlPlanning;
                    List<crud_ControlPlanningDetail> ControlPlanningDetail = postControlPlanning.ControlPlanningDetail;
                    List<crud_ScheduleProduction> ScheduleProduction = postControlPlanning.ScheduleProduction;

                    string formAction = postControlPlanning.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":


                            /* Get New Order Number */
                            string CompId = Session["CompID"].ToString();
                            var OrderNumber = vssp_db.SP_GET_ControlPlanningNumber(ControlPlanning.OrderMonth, ControlPlanning.OrderYear, CompId);
                            foreach (SP_GET_ControlPlanningNumber_Result number in OrderNumber)
                            {
                                ControlPlanning.OrderNumber = number.OrderNumber;
                            }

                            Tbl_TRS_ControlPlanning ListControlPlanning = new Tbl_TRS_ControlPlanning();
                            ListControlPlanning.OrderNumber = ControlPlanning.OrderNumber;
                            ListControlPlanning.OrderDate = ControlPlanning.OrderDate;
                            ListControlPlanning.OrderYear = ControlPlanning.OrderYear;
                            ListControlPlanning.OrderMonth = ControlPlanning.OrderMonth;
                            ListControlPlanning.Shift = ControlPlanning.Shift;
                            ListControlPlanning.LineId = ControlPlanning.LineId;
                            ListControlPlanning.StartDateProduction = ControlPlanning.StartDateProduction;
                            ListControlPlanning.EndDateProduction = ControlPlanning.EndDateProduction;
                            ListControlPlanning.Remarks = ControlPlanning.Remarks;
                            ListControlPlanning.Status = 0;
                            ListControlPlanning.UserId = uid;
                            ListControlPlanning.EditDate = DateTime.Now;

                            vssp_db.Tbl_TRS_ControlPlanning.Add(ListControlPlanning);

                            /* crud Details */
                            crudControlPlanningDetail(ControlPlanningDetail, ControlPlanning.OrderNumber, ControlPlanning.LineId, formAction);
                            /* crud Production Schedule */
                            crudScheduleProduction(ScheduleProduction, ControlPlanning.OrderNumber, ControlPlanning.LineId, formAction);
                            /* crud Approval */
                            crudControlPlanningApproval(postControlPlanning.ApprovalId, ControlPlanning.OrderNumber, uid, formAction);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_ControlPlanning.First(a => a.OrderNumber == ControlPlanning.OrderNumber);

                            ListUpdate.OrderDate = ControlPlanning.OrderDate;
                            ListUpdate.OrderYear = ControlPlanning.OrderYear;
                            ListUpdate.OrderMonth = ControlPlanning.OrderMonth;
                            ListUpdate.Shift = ControlPlanning.Shift;
                            ListUpdate.LineId = ControlPlanning.LineId;
                            ListUpdate.StartDateProduction = ControlPlanning.StartDateProduction;
                            ListUpdate.EndDateProduction = ControlPlanning.EndDateProduction;
                            ListUpdate.Remarks = ControlPlanning.Remarks;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Details */
                            crudControlPlanningDetail(ControlPlanningDetail, ControlPlanning.OrderNumber, ControlPlanning.LineId, formAction);
                            /* crud Production Schedule */
                            crudScheduleProduction(ScheduleProduction, ControlPlanning.OrderNumber, ControlPlanning.LineId, formAction);
                            /* crud Approval */
                            crudControlPlanningApproval(postControlPlanning.ApprovalId, ControlPlanning.OrderNumber, uid, formAction);

                            break;
                        case "closed":

                            var ListClosed = vssp_db.Tbl_TRS_ControlPlanning.First(a => a.OrderNumber == ControlPlanning.OrderNumber);

                            ListClosed.Status = 3;

                            break;

                        case "canceled":

                            var ListCanceled = vssp_db.Tbl_TRS_ControlPlanning.First(a => a.OrderNumber == ControlPlanning.OrderNumber);

                            ListCanceled.Status = 4;

                            break;

                        case "delete":

                            /* remove existing ControlPlanning */
                            var ListDelete = vssp_db.Tbl_TRS_ControlPlanning.First(a => a.OrderNumber == ControlPlanning.OrderNumber);

                            ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                            //vssp_db.Tbl_TRS_ControlPlanning.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();

                        if (formAction == "create" || formAction == "update")
                        {
                            //crudMasterListKanbanDetail(ControlPlanning.OrderNumber); <-- MLOK Produksi
                            crudMasterListKanbanProduction(ControlPlanning.OrderNumber);
                            crudMasterListKanbanStockFG(ControlPlanning.OrderNumber);
                        }

                        /* generate kanban production */
                        var kanban = vssp_db.SP_CRUD_KanbanProductions(ControlPlanning.OrderNumber, formAction.ToLower());

                        return Json(ControlPlanning, JsonRequestBehavior.AllowGet);
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

        public void crudControlPlanningDetail(List<crud_ControlPlanningDetail> ControlPlanningDetails, string OrderNumber, string LineId, string formAction)
        {

            foreach (var Details in ControlPlanningDetails)
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
                            Tbl_TRS_ControlPlanningDetail ListDetail = new Tbl_TRS_ControlPlanningDetail();
                            ListDetail.OrderNumber = OrderNumber;
                            ListDetail.LineId = LineId;
                            ListDetail.CustomerId = Details.CustomerId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.OrderQty = Details.OrderQty;
                            ListDetail.N1 = Details.N1;
                            ListDetail.N2 = Details.N2;
                            ListDetail.N3 = Details.N3;
                            ListDetail.DailyQty = Details.DailyQty;
                            ListDetail.ShiftQty = Details.ShiftQty;
                            ListDetail.SONumber = Details.SONumber;

                            vssp_db.Tbl_TRS_ControlPlanningDetail.Add(ListDetail);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_ControlPlanningDetail.First(a => a.OrderNumber == OrderNumber && a.LineId == LineId && a.CustomerId == Details.CustomerId && a.PartNumber == Details.PartNumber);

                            ListUpdate.OrderQty = Details.OrderQty;
                            ListUpdate.N1 = Details.N1;
                            ListUpdate.N2 = Details.N2;
                            ListUpdate.N3 = Details.N3;
                            ListUpdate.DailyQty = Details.DailyQty;
                            ListUpdate.ShiftQty = Details.ShiftQty;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_ControlPlanningDetail.First(a => a.OrderNumber == OrderNumber && a.LineId == LineId && a.CustomerId == Details.CustomerId && a.PartNumber == Details.PartNumber);

                            vssp_db.Tbl_TRS_ControlPlanningDetail.Remove(ListDelete);

                            break;
                    }
                }
            }
        }
        public void crudScheduleProduction(List<crud_ScheduleProduction> scheduleProduction, string OrderNumber, string LineId, string FormAction)
        {

            /* clear schedule */
            var ListDelete = vssp_db.Tbl_TRS_ScheduleProduction.Where(a => a.OrderNumber == OrderNumber).ToList();
            foreach (var delete in ListDelete)
            {
                vssp_db.Tbl_TRS_ScheduleProduction.Remove(delete);
            }

            foreach (var Details in scheduleProduction)
            {

                switch (FormAction.ToLower())
                {
                    case "create":

                        /* create Details */
                        Tbl_TRS_ScheduleProduction ListDetail = new Tbl_TRS_ScheduleProduction();
                        ListDetail.OrderNumber = OrderNumber;
                        ListDetail.LineId = LineId;
                        ListDetail.CustomerId = Details.CustomerId;
                        ListDetail.PartNumber = Details.PartNumber;
                        ListDetail.ShiftId = Details.ShiftId;
                        ListDetail.ProductionDate = Details.ProductionDate;
                        ListDetail.ProductionQty = Details.ProductionQty;

                        vssp_db.Tbl_TRS_ScheduleProduction.Add(ListDetail);

                        break;

                    case "update":

                        /* create Details */
                        Tbl_TRS_ScheduleProduction ListUpdate = new Tbl_TRS_ScheduleProduction();
                        ListUpdate.OrderNumber = OrderNumber;
                        ListUpdate.LineId = LineId;
                        ListUpdate.CustomerId = Details.CustomerId;
                        ListUpdate.PartNumber = Details.PartNumber;
                        ListUpdate.ShiftId = Details.ShiftId;
                        ListUpdate.ProductionDate = Details.ProductionDate;
                        ListUpdate.ProductionQty = Details.ProductionQty;

                        vssp_db.Tbl_TRS_ScheduleProduction.Add(ListUpdate);


                        break;


                }
            }

        }
        public void crudControlPlanningApproval(string ApprovalId, string OrderNumber, string UserId, string action)
        {
            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    List<UserApprovalListModel> userApprovalLists = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalLists)
                    {
                        Tbl_TRS_ControlPlanningApproval ListApproval = new Tbl_TRS_ControlPlanningApproval();
                        ListApproval.OrderNumber = OrderNumber;
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

                        vssp_db.Tbl_TRS_ControlPlanningApproval.Add(ListApproval);
                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_TRS_ControlPlanningApproval> UserApproval = vssp_db.Tbl_TRS_ControlPlanningApproval.Where(a => a.OrderNumber == OrderNumber).ToList();
                    foreach (var user in UserApproval)
                    {
                        UserApprovalListModel ApprovalLists = _AccountService.UserApprovalType(user.UserId, ApprovalId).FirstOrDefault(a => a.UserID == user.UserId);
                    }

                    /* create approval */
                    List<UserApprovalListModel> userApprovalListsUpdate = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalListsUpdate)
                    {
                        Tbl_TRS_ControlPlanningApproval existUser = (from a in vssp_db.Tbl_TRS_ControlPlanningApproval
                                                                     where a.OrderNumber == OrderNumber && a.UserId == users.UserID
                                                                     select a).FirstOrDefault();
                        if (existUser == null)
                        {
                            Tbl_TRS_ControlPlanningApproval ListApproval = new Tbl_TRS_ControlPlanningApproval();
                            ListApproval.OrderNumber = OrderNumber;
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

                            vssp_db.Tbl_TRS_ControlPlanningApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "sent":

                    var ListSent = vssp_db.Tbl_TRS_ControlPlanningApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == ApprovalId);

                    ListSent.SentEmail = true;
                    ListSent.SentEmailDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "approved":

                    var ListUpdate = vssp_db.Tbl_TRS_ControlPlanningApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == ApprovalId);

                    ListUpdate.Approved = true;
                    ListUpdate.ApprovedDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_TRS_ControlPlanningApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == ApprovalId);

                    vssp_db.Tbl_TRS_ControlPlanningApproval.Remove(ListDelete);

                    break;
            }

        }
        public void crudMasterListKanbanProduction(string OrderNumber)
        {
            vssp_db.Database.CommandTimeout = 0;
            var MLOK = vssp_db.SP_CRUD_MasterKanbanListProduction(OrderNumber);
        }
        public void crudMasterListKanbanStockFG(string OrderNumber)
        {
            var Stock = vssp_db.SP_CRUD_MasterKanbanListStockFG(OrderNumber);
        }
        public ActionResult ControlPlanningApproval(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Production Planning Approval";

            try
            {

                if (ordernumber == null || uid == null)
                {
                    ordernumber = Session["ordernumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["ordernumber"] = ordernumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_TRS_ControlPlanning ControlPlanning = vssp_db.Vw_TRS_ControlPlanning.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_TRS_ControlPlanningApproval approval = vssp_db.Tbl_TRS_ControlPlanningApproval.Where(a => a.OrderNumber == ordernumber && a.UserId == user.UserID).FirstOrDefault();

                    if (ControlPlanning != null && user != null && approval != null)
                    {

                        string orderdate = new DateTime(int.Parse(ControlPlanning.OrderYear), int.Parse(ControlPlanning.OrderMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.OrderTitle = "Production Planning";
                        ViewBag.OrderNumber = ControlPlanning.OrderNumber;
                        ViewBag.OrderDate = orderdate;
                        ViewBag.LineName = ControlPlanning.LineName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        if (approval.Approved == false)
                        {
                            return View();
                        }
                        else
                        {
                            ViewBag.ApprovedDate = _SystemService.Vd(approval.ApprovedDate.ToString(), "dd MMMM, yyyy");
                            return View("ControlPlanningApproved");
                        }

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Part Requirement List";
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

        public ActionResult ControlPlanningApproved(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Production Planning Approval";

            try
            {

                if (ordernumber == null || uid == null)
                {
                    ordernumber = Session["ordernumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["ordernumber"] = ordernumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_TRS_ControlPlanning ControlPlanning = vssp_db.Vw_TRS_ControlPlanning.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();

                    if (ControlPlanning != null && user != null)
                    {

                        string orderdate = new DateTime(int.Parse(ControlPlanning.OrderYear), int.Parse(ControlPlanning.OrderMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.OrderTitle = "Production Planning";
                        ViewBag.OrderNumber = ControlPlanning.OrderNumber;
                        ViewBag.OrderDate = orderdate;
                        ViewBag.LineName = ControlPlanning.LineName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        crudControlPlanningApproval(user.UserID, ControlPlanning.OrderNumber, user.UserID, "Approved");
                        return RedirectToAction("ContinuePage", "System", new { cmessage = "Successfuly Approved " + ViewBag.OrderTitle + " \n " + ordernumber, caction = "Dashboard", ccontroller = "Home", capps = "Home" });

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Production Planning";
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
        public ActionResult MonthlySchedule()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "MonthlySchedule");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Production " + acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
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
        public ActionResult DailyPlanning()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "DailyPlanning");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Production " + acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now.ToString("yyyy-MM-dd");

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
        public ActionResult DailyPlanningListJson(
                                    string searchFilter,
                                    Nullable<DateTime> productiondate = null
                                    )
        {
            searchFilter = _SystemService.Vf(searchFilter);

            if (productiondate == null)
            {
                productiondate = DateTime.Parse(_SystemService.Vd(DateTime.Now.ToString(), "yyyy-MM-dd"));
            }

            var ScheduleProduction = (from a in vssp_db.Vw_TRS_ScheduleProduction
                                      where a.Status != 4 && a.Status != 5 && a.ProductionDate == productiondate && (a.OrderNumber.Contains(searchFilter) || a.LineId.Contains(searchFilter)
                                      || a.LineName.Contains(searchFilter) || a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter)
                                      || a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                                      orderby a.ProductionDate descending, a.CustomerId, a.LineId, a.OrderNumber
                                      select new { a.OrderNumber, a.CustomerId, a.CustomerName, a.LineId, a.LineName, a.UniqueNumber, a.PartNumber, a.PartName, a.ProductionDate, a.ShiftId, a.ProductionQty }).ToList();

            return Json(ScheduleProduction, JsonRequestBehavior.AllowGet);

        }

        public ActionResult MLOKCustomerList(string ordernumber, string lineid)
        {

            ordernumber = _SystemService.Vf(ordernumber);
            lineid = _SystemService.Vf(lineid);

            var mlokcustomer = (from a in vssp_db.Tbl_TRS_ControlPlanningDetail
                                join b in vssp_db.Tbl_MST_Customer on a.CustomerId equals b.CustomerId into customer
                                from b in customer.DefaultIfEmpty()
                                where a.OrderNumber.Contains(ordernumber) || a.LineId.Contains(lineid)
                                orderby a.CustomerId
                                select new { a.CustomerId, b.CustomerName }).DistinctBy(a => new { a.CustomerId, a.CustomerName }).ToList();

            return Json(mlokcustomer, JsonRequestBehavior.AllowGet);

        }
        public ActionResult MLOKPartList(string ordernumber, string customerid)
        {
            customerid = _SystemService.Vf(customerid);
            var mlokpart = (from a in vssp_db.Tbl_TRS_ControlPlanningDetail
                            join b in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber } into part
                            from b in part.DefaultIfEmpty()
                            where a.OrderNumber == ordernumber && a.CustomerId.Contains(customerid)
                            orderby b.UniqueNumber, b.PartNumber
                            select new { b.UniqueNumber, a.PartNumber, b.PartName }).DistinctBy(a => new { a.UniqueNumber, a.PartNumber, a.PartName }).ToList();

            return Json(mlokpart, JsonRequestBehavior.AllowGet);

        }
        public ActionResult MLOKShiftList(string ordernumber)
        {
            var mlokshift = (from a in vssp_db.Tbl_TRS_ScheduleProduction
                             join b in vssp_db.Tbl_TMS_WorkingShiftMaster on a.ShiftId equals b.ShiftId into shift
                             from b in shift.DefaultIfEmpty()
                             where a.OrderNumber == ordernumber
                             orderby a.ShiftId
                             select new { a.ShiftId, b.ShiftName }).DistinctBy(a => new { a.ShiftId, a.ShiftName }).ToList();

            return Json(mlokshift, JsonRequestBehavior.AllowGet);

        }
        public ActionResult crudDailyPlanningList(string OrderNumber, string LineId, string CustomerId,
            string PartNumber, DateTime ProductionDate, string ShiftId, float ProductionQty, string formAction)
        {
            try {
                switch (formAction.ToLower())
                {
                    case "create":

                        /* create planning */
                        Tbl_TRS_ScheduleProduction ListDetail = new Tbl_TRS_ScheduleProduction();
                        ListDetail.OrderNumber = OrderNumber;
                        ListDetail.LineId = LineId;
                        ListDetail.CustomerId = CustomerId;
                        ListDetail.PartNumber = PartNumber;
                        ListDetail.ShiftId = ShiftId;
                        ListDetail.ProductionDate = ProductionDate;
                        ListDetail.ProductionQty = ProductionQty;

                        vssp_db.Tbl_TRS_ScheduleProduction.Add(ListDetail);

                        break;

                    case "update":

                        /* update planning */
                        Tbl_TRS_ScheduleProduction ListUpdate = vssp_db.Tbl_TRS_ScheduleProduction.FirstOrDefault(a => a.OrderNumber == OrderNumber &&
                                                                                                                  a.LineId == LineId && a.CustomerId == CustomerId && a.PartNumber == PartNumber &&
                                                                                                                  a.ProductionDate == ProductionDate && a.ShiftId == ShiftId);                     
                        ListUpdate.ProductionQty = ProductionQty;

                        break;

                    case "delete":

                        /* update planning */
                        Tbl_TRS_ScheduleProduction ListDelete = vssp_db.Tbl_TRS_ScheduleProduction.FirstOrDefault(a => a.OrderNumber == OrderNumber &&
                                                                                                                  a.LineId == LineId && a.CustomerId == CustomerId && a.PartNumber == PartNumber &&
                                                                                                                  a.ProductionDate == ProductionDate && a.ShiftId == ShiftId);
                        vssp_db.Tbl_TRS_ScheduleProduction.Remove(ListDelete);

                        break;

                }
                try
                {
                    vssp_db.SaveChanges();
                    return Json("<br/>" + PartNumber + "<br/>" + ProductionDate.ToString("dd-MMM-yyyy") + "<br/>" + ShiftId + "<br/>", JsonRequestBehavior.AllowGet);
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
        public ActionResult KanbanControl()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "KanbanControl");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Production " + acccessPreviliege.MenuName;
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now.ToString("yyyy-MM-dd");

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
        public ActionResult KanbanControlListJson(
                                    string searchFilter,
                                    string kanbankey,
                                    string customerid,
                                    string partnumber
                                    )
        {
            searchFilter = _SystemService.Vf(searchFilter);

            var kanbanControlListJson = (from a in vssp_db.Tbl_MST_KanbanProduction
                                         join b in vssp_db.Tbl_MST_Line on a.LineId equals b.LineId into line
                                         from b in line.DefaultIfEmpty()
                                         join c in vssp_db.Tbl_MST_Customer on a.CustomerId equals c.CustomerId into customer
                                         from c in customer.DefaultIfEmpty()
                                         join d in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { d.CustomerId, d.PartNumber } into part
                                         from d in part.DefaultIfEmpty()
                                         where (a.KanbanKey.Contains(searchFilter) || a.LineId.Contains(searchFilter)
                                         || b.LineName.Contains(searchFilter) || a.CustomerId.Contains(searchFilter) || c.CustomerName.Contains(searchFilter)
                                         || d.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || d.PartName.Contains(searchFilter))
                                         group new { a.KanbanKey, a.Storage, a.Actived }
                                         by new { a.LineId, b.LineName, a.CustomerId, c.CustomerName, d.UniqueNumber, a.PartNumber, d.PartName, d.UnitQty, d.UnitLevel2 } into kanban
                                         orderby kanban.Key.LineId, kanban.Key.CustomerId, kanban.Key.UniqueNumber, kanban.Key.PartNumber
                                         select new
                                         {
                                             kanban.Key.LineId,
                                             kanban.Key.LineName,
                                             kanban.Key.CustomerId,
                                             kanban.Key.CustomerName,
                                             kanban.Key.UniqueNumber,
                                             kanban.Key.PartNumber,
                                             kanban.Key.PartName,
                                             kanban.Key.UnitQty,
                                             kanban.Key.UnitLevel2,
                                             TotalKanban = kanban.Sum(x => x.KanbanKey != "" ? 1 : 0),
                                             ActivedKanban = kanban.Sum(x => x.Actived == true ? 1 : 0),
                                             StorageKanban = kanban.Sum(x => x.Storage == true ? 1 : 0),
                                             SuspendKanban = kanban.Sum(x => x.Actived == false ? 1 : 0)
                                         }).ToList();

            //return Json(kanbanControlListJson, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(kanbanControlListJson, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;


        }
        public ActionResult KanbanProductionListJson(string customerid, string partnumber, string kanbankey = "")
        {
            customerid = _SystemService.Vf(customerid);
            partnumber = _SystemService.Vf(partnumber);

            var kanbanProductionListJson = (from a in vssp_db.Vw_MST_KanbanProductionList
                                            where a.CustomerId.Contains(customerid) && a.PartNumber.Contains(partnumber)
                                            orderby a.KanbanRun
                                            select a).ToList();

            if(kanbankey != "")
            {
                string[] key = kanbankey.Split(';');
                kanbanProductionListJson = kanbanProductionListJson.Where(a=> a.KanbanKey == key[0]).ToList();
            }
            //return Json(kanbanProductionListJson, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(kanbanProductionListJson, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult crudKanbanControlList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();
                    //PostKanbanControlModel postKanbanControlModel = JsonConvert.DeserializeObject<PostKanbanControlModel>(jsonData);
                    List<Tbl_MST_KanbanProduction> kanbanProduction = JsonConvert.DeserializeObject<List<Tbl_MST_KanbanProduction>>(jsonData);
                    
                    foreach(var kanban in kanbanProduction)
                    {
                        var ListUpdate = vssp_db.Tbl_MST_KanbanProduction.First(a => a.KanbanKey == kanban.KanbanKey);

                        ListUpdate.Storage = false;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(kanbanProduction, JsonRequestBehavior.AllowGet);
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
        public ActionResult LineProcess()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();
                string area = Session["Area"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "LineProcess");

                var usrarea = vssp_db.Vw_MST_AreaLine.Where(a => a.AreaID == area).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Production " + acccessPreviliege.MenuName;

                    if (usrarea != null)
                    {
                        //ViewBag.AreaId = usrarea.AreaID;
                        //ViewBag.AreaName = usrarea.AreaName;
                        ViewBag.Title = ViewBag.Title + " [" + usrarea.AreaName + "]";
                    } else
                    {
                        //if (area.ToLower() == "all")
                        //{
                        //    ViewBag.AreaId = area;
                        //    ViewBag.AreaName = area;
                        //} else
                        //{
                        //    ViewBag.AreaId = "";
                        //    ViewBag.AreaName = "";
                        //}
                    }
                    ViewBag.AreaId = "";
                    ViewBag.AreaName = "";

                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;

                    DateTime pdate = DateTime.Now;
                    if (pdate.Hour>= 0 && pdate.Hour<=7)
                    {
                        pdate = pdate.AddDays(-1);
                    }

                    ViewBag.DateTime = pdate;

                    LineProcessListModel LineProcess = new LineProcessListModel();
                    LineProcess.ExportList = _SystemService.ComboExport().ToList();
                    LineProcess.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                              orderby a.Id
                                              select a).ToList();
                    LineProcess.ProductionDate = DateTime.Now;
                    LineProcess.LineId = ViewBag.AreaId;
                    LineProcess.LineName = ViewBag.AreaName;

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

                        return View(LineProcess);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult LineProcessQC()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();
                string area = Session["Area"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "LineProcessQC");

                var usrarea = vssp_db.Vw_MST_AreaLine.Where(a => a.AreaID == area).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Production " + acccessPreviliege.MenuName;

                    if (usrarea != null)
                    {
                        //ViewBag.AreaId = usrarea.AreaID;
                        //ViewBag.AreaName = usrarea.AreaName;
                        ViewBag.Title = ViewBag.Title + " [" + usrarea.AreaName + "]";
                    }
                    else
                    {
                        //if (area.ToLower() == "all")
                        //{
                        //    ViewBag.AreaId = area;
                        //    ViewBag.AreaName = area;
                        //}
                        //else
                        //{
                        //    ViewBag.AreaId = "";
                        //    ViewBag.AreaName = "";
                        //}
                    }
                    ViewBag.AreaId = "";
                    ViewBag.AreaName = "";

                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;

                    DateTime pdate = DateTime.Now;
                    if (pdate.Hour >= 0 && pdate.Hour <= 7)
                    {
                        pdate = pdate.AddDays(-1);
                    }

                    ViewBag.DateTime = pdate;

                    LineProcessListModel LineProcess = new LineProcessListModel();
                    LineProcess.ExportList = _SystemService.ComboExport().ToList();
                    LineProcess.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                              orderby a.Id
                                              select a).ToList();
                    LineProcess.ProductionDate = DateTime.Now;
                    LineProcess.LineId = ViewBag.AreaId;
                    LineProcess.LineName = ViewBag.AreaName;

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

                        return View(LineProcess);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult LineProcessQCVer2()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();
                string area = Session["Area"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Productions", "LineProcessQCVer2");

                var usrarea = vssp_db.Vw_MST_AreaLine.Where(a => a.AreaID == area).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Production " + acccessPreviliege.MenuName;

                    if (usrarea != null)
                    {
                        ViewBag.AreaId = usrarea.AreaID;
                        ViewBag.AreaName = usrarea.AreaName;
                        ViewBag.Title = ViewBag.Title + " [" + usrarea.AreaName + "]";
                    }
                    else
                    {
                        if (area.ToLower() == "all")
                        {
                            ViewBag.AreaId = area;
                            ViewBag.AreaName = area;
                        }
                        else
                        {
                            ViewBag.AreaId = "";
                            ViewBag.AreaName = "";
                        }
                    }

                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;

                    DateTime pdate = DateTime.Now;
                    if (pdate.Hour >= 0 && pdate.Hour <= 7)
                    {
                        pdate = pdate.AddDays(-1);
                    }

                    ViewBag.DateTime = pdate;

                    LineProcessListModel LineProcess = new LineProcessListModel();
                    LineProcess.ExportList = _SystemService.ComboExport().ToList();
                    LineProcess.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                              orderby a.Id
                                              select a).ToList();
                    LineProcess.LineList = (from a in vssp_db.Tbl_MST_Line
                                            where a.Actived == true
                                            orderby a.LineId
                                            select a).ToList(); 
                    LineProcess.ProductionDate = DateTime.Now;
                    LineProcess.LineId = ViewBag.AreaId;
                    LineProcess.LineName = ViewBag.AreaName;

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

                        return View(LineProcess);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult LineProcessJson(
                                    string searchFilter,
                                    Nullable<DateTime> productionDate,
                                    string gateId,
                                    string lineId,
                                    string userId)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            lineId = _SystemService.Vf(lineId);
            gateId = _SystemService.Vf(gateId);
            userId = _SystemService.Vf(userId);
            if (productionDate != null)
            {
                var LineProcess = (from a in vssp_db.Vw_TRS_ProductionLineProcess
                                   where (a.ProductionDate == productionDate && a.LineId.Contains(lineId) && a.GateId.Contains(gateId)) &&
                                        (a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                                   orderby a.ProductionDate descending, a.LineId, a.CustomerId, a.ShiftId
                                   select new { a.ProductionYear, a.ProductionMonth, a.ProductionDate, a.ShiftId, a.LineId, a.LineName, a.GateId, a.GateName, a.CustomerId, a.CustomerName, a.UniqueNumber, a.PartNumber, a.PartName, a.PackingId, a.TotalKanban, a.TotalQtyOK, a.TotalQtyNG, a.UnitLevel2, a.OrderNumber, a.UserId }).ToList();

                return Json(LineProcess, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var LineProcess = (from a in vssp_db.Vw_TRS_ProductionLineProcess
                                   where a.LineId.Contains(lineId) && a.GateId.Contains(gateId) &&
                                        (a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                                   orderby a.ProductionDate descending, a.LineId, a.CustomerId, a.ShiftId
                                   select new { a.ProductionYear, a.ProductionMonth, a.ProductionDate, a.ShiftId, a.LineId, a.LineName, a.GateId, a.GateName, a.CustomerId, a.CustomerName, a.UniqueNumber, a.PartNumber, a.PartName, a.PackingId, a.TotalKanban, a.TotalQtyOK, a.TotalQtyNG, a.UnitLevel2, a.OrderNumber, a.UserId }).ToList();

                return Json(LineProcess, JsonRequestBehavior.AllowGet);
            }
            /* change use view */
            //if (productionDate != null) {
            //    var LineProcess = (from a in vssp_db.Tbl_TRS_ProductionLineProcess
            //                       join b in vssp_db.Tbl_MST_Line on a.LineId equals b.LineId into line
            //                       from b in line.DefaultIfEmpty()
            //                       join c in vssp_db.Tbl_MST_Customer on a.CustomerId equals c.CustomerId into customer
            //                       from c in customer.DefaultIfEmpty()
            //                       join d in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { d.CustomerId, d.PartNumber } into part
            //                       from d in part.DefaultIfEmpty()
            //                       where (a.ProductionDate==productionDate && a.LineId.Contains(lineId) && a.GateId.Contains(gateId)) && 
            //                            (a.CustomerId.Contains(searchFilter) || c.CustomerName.Contains(searchFilter) || d.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || d.PartName.Contains(searchFilter))
            //                       orderby a.ProductionDate descending, a.LineId, a.CustomerId, a.ShiftId
            //                       group new { a.KanbanKey, a.Qty_OK }
            //                       by new { a.ProductionDate, a.LineId, b.LineName, a.GateId, a.CustomerId, c.CustomerName, a.ShiftId, d.UniqueNumber, a.PartNumber, d.PartName, d.UnitLevel2, a.UserId } into prod
            //                       select new
            //                       {
            //                           prod.Key.ProductionDate,
            //                           prod.Key.LineId,
            //                           prod.Key.GateId,
            //                           prod.Key.LineName,
            //                           prod.Key.CustomerId,
            //                           prod.Key.CustomerName,
            //                           prod.Key.ShiftId,
            //                           prod.Key.UniqueNumber,
            //                           prod.Key.PartNumber,
            //                           prod.Key.PartName,
            //                           TotalKanban = prod.Sum(x => x.KanbanKey != "" ? 1 : 0),
            //                           TotalQtyOK = prod.Sum(x => x.Qty_OK),
            //                           prod.Key.UnitLevel2,
            //                           prod.Key.UserId
            //                       }).ToList();

            //    return Json(LineProcess, JsonRequestBehavior.AllowGet);

            //}
            //else
            //{
            //    var LineProcess = (from a in vssp_db.Tbl_TRS_ProductionLineProcess
            //                       join b in vssp_db.Tbl_MST_Line on a.LineId equals b.LineId into line
            //                       from b in line.DefaultIfEmpty()
            //                       join c in vssp_db.Tbl_MST_Customer on a.CustomerId equals c.CustomerId into customer
            //                       from c in customer.DefaultIfEmpty()
            //                       join d in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { d.CustomerId, d.PartNumber } into part
            //                       from d in part.DefaultIfEmpty()
            //                       where a.LineId.Contains(lineId) && a.GateId.Contains(gateId) &&
            //                            (a.CustomerId.Contains(searchFilter) || c.CustomerName.Contains(searchFilter) || d.UniqueNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || d.PartName.Contains(searchFilter))
            //                       orderby a.ProductionDate descending, a.LineId, a.CustomerId, a.ShiftId
            //                       group new { a.KanbanKey, a.Qty_OK }
            //                       by new { a.ProductionDate, a.LineId, b.LineName, a.GateId, a.CustomerId, c.CustomerName, a.ShiftId, d.UniqueNumber, a.PartNumber, d.PartName, d.UnitLevel2, a.UserId } into prod
            //                       select new
            //                       {
            //                           prod.Key.ProductionDate,
            //                           prod.Key.LineId,
            //                           prod.Key.GateId,
            //                           prod.Key.LineName,
            //                           prod.Key.CustomerId,
            //                           prod.Key.CustomerName,
            //                           prod.Key.ShiftId,
            //                           prod.Key.UniqueNumber,
            //                           prod.Key.PartNumber,
            //                           prod.Key.PartName,
            //                           TotalKanban = prod.Sum(x => x.KanbanKey != "" ? 1 : 0),
            //                           TotalQtyOK = prod.Sum(x => x.Qty_OK),
            //                           prod.Key.UnitLevel2,
            //                           prod.Key.UserId
            //                       }).ToList();

            //    return Json(LineProcess, JsonRequestBehavior.AllowGet);
            //}
        }
        public ActionResult LineProcessListJson(
                            string productionNumber,
                            DateTime productionDate,
                            string shiftId,
                            string lineId,
                            string gateId,
                            string customerId,
                            string partNumber,
                            string userId)
        {
            List<Vw_TRS_ProductionLineProcessList> LineProcess = (from a in vssp_db.Vw_TRS_ProductionLineProcessList
                                                                  where a.ProductionDate == productionDate && a.ShiftId == shiftId
                                                                    && a.LineId == lineId && a.CustomerId == customerId
                                                                    && a.PartNumber == partNumber && a.UserId == userId
                                                                  orderby a.EditDate descending
                                                                  select a).ToList();

            if (_SystemService.Vf(productionNumber) != "")
            {
                LineProcess = LineProcess.Where(a => a.ProductionNumber == productionNumber).ToList();
            }
            if (_SystemService.Vf(gateId) != "")
            {
                LineProcess = LineProcess.Where(a => a.GateId == gateId).ToList();
            }

            return Json(LineProcess, JsonRequestBehavior.AllowGet);

        }
        public ActionResult NGProcessListJson(
                            string productionNumber,
                            DateTime productionDate,
                            string shiftId,
                            string lineId,
                            string gateId,
                            string customerId,
                            string partNumber,
                            string userId)
        {
            List<Vw_TRS_ProductionNGProcessList> LineProcess = (from a in vssp_db.Vw_TRS_ProductionNGProcessList
                                                                where a.ProductionDate == productionDate && a.ShiftId == shiftId
                                                                  && a.LineId == lineId && a.CustomerId == customerId
                                                                  && a.PartNumber == partNumber && a.UserId == userId
                                                                orderby a.EditDate descending
                                                                select a).ToList();

            if (_SystemService.Vf(productionNumber) != "")
            {
                LineProcess = LineProcess.Where(a => a.ProductionNumber == productionNumber).ToList();
            }
            if (_SystemService.Vf(gateId) != "")
            {
                LineProcess = LineProcess.Where(a => a.GateId == gateId).ToList();
            }

            return Json(LineProcess, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetProductionShift(DateTime proddate)
        {

            var prodshift = (from a in vssp_db.Tbl_TRS_ScheduleProduction
                             join b in vssp_db.Tbl_TMS_WorkingShiftMaster on a.ShiftId equals b.ShiftId into shift
                             from b in shift.DefaultIfEmpty()
                             join c in vssp_db.Tbl_TRS_ControlPlanning on a.OrderNumber equals c.OrderNumber into planning
                             from c in planning.DefaultIfEmpty()
                             where a.ProductionDate == proddate && c.Status!= 4 && c.Status != 5
                             select new { a.ShiftId, b.ShiftName }).DistinctBy(a => new { a.ShiftId, a.ShiftName }).ToList();

            return Json(prodshift, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetProductionDailySchedule(string lineid, string shiftid, DateTime proddate)
        {

            var prodschedule = vssp_db.SP_GET_ProductionDailySchedule(lineid, shiftid, proddate).ToList();

            return Json(prodschedule, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetProductionDailySummary(string lineid, string gateid, string shiftid, DateTime proddate)
        {

            gateid = _SystemService.Vf(gateid);
            var prodsummary = vssp_db.SP_GET_ProductionDailyShiftSummary(lineid, gateid, shiftid, proddate).ToList();

            return Json(prodsummary, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetLatestOutput(string lineid, string gateid, string shiftid, DateTime proddate, string userid)
        {

            try
            {
                gateid = _SystemService.Vf(gateid);
                SP_GET_ProductionDailyLatestSummary_Result prodoutput = vssp_db.SP_GET_ProductionDailyLatestSummary(lineid, gateid, shiftid, proddate, userid).FirstOrDefault();
                List<ProductionDailyLatestOutputModel> latestOutputModels = new List<ProductionDailyLatestOutputModel>();

                if (prodoutput != null)
                {
                    if (prodoutput.KanbanKey != null)
                    {
                        QRCode.generateKanbanProductionQrcode(prodoutput.CustomerId, prodoutput.PartNumber, prodoutput.KanbanKey);
                    }

                    var output = prodoutput;
                    var qrcode = (from a in vssp_db.Tbl_TRS_QrCodePath
                                  where a.DocId == output.KanbanKey
                                  select new { a.QrcodePath }).FirstOrDefault();

                    var pi_images = (from a in vssp_db.Tbl_MST_PartIdentification
                                     where a.CustomerId == prodoutput.CustomerId && a.PartNumber == prodoutput.PartNumber
                                     select a.PI_Images).FirstOrDefault();

                    ProductionDailyLatestOutputModel latestOutputModel = new ProductionDailyLatestOutputModel();
                    latestOutputModel.OrderNumber = output.OrderNumber;
                    latestOutputModel.KanbanKey = output.KanbanKey;
                    latestOutputModel.LineId = output.LineId;
                    latestOutputModel.GateId = output.GateId;
                    latestOutputModel.CustomerId = output.CustomerId;
                    latestOutputModel.UniqueNumber = output.UniqueNumber;
                    latestOutputModel.PartNumber = output.PartNumber;
                    latestOutputModel.PartName = output.PartName;
                    latestOutputModel.Qty_OK = output.Qty_OK;
                    latestOutputModel.UnitLevel2 = output.UnitLevel2;
                    latestOutputModel.SumKanban = output.SumKanban;
                    latestOutputModel.SumQty = output.SumQty;
                    latestOutputModel.SumNG = output.SumNG;
                    latestOutputModel.QrcodePath = qrcode != null ? qrcode.QrcodePath : "";
                    latestOutputModel.PI_Images = (pi_images == null ? "../_VSSPAssets/Images/noimage.png" : pi_images);

                    latestOutputModels.Add(latestOutputModel);

                }

                return Json(latestOutputModels, JsonRequestBehavior.AllowGet);

            }
            catch ( Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult ClearKanbanQrImages(string CustomerId, string PartNumber, string KanbanKey)
        {

            QRCode.cleanKanbanProductionQrcode(CustomerId, PartNumber, KanbanKey);

            return Json("successed", JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetStartTime()
        {

            int dow = (int)DateTime.Now.DayOfWeek;

            var shift = vssp_db.SP_TMS_GetStartWorkingTime(dow.ToString()).FirstOrDefault();

            return Json(shift, JsonRequestBehavior.AllowGet);

        }
        public Tbl_TRS_ProductionLineProcess crudKanbanProcess(string kanbanqkey, string prodnumber, DateTime proddate, string shiftid, string lineid, string gateid, string customerid, string partnumber, double? qty, string uid, string formAction)
        {
            Tbl_TRS_ProductionLineProcess _ProductionLineProcess = new Tbl_TRS_ProductionLineProcess();
            gateid = _SystemService.Vf(gateid);

            switch (formAction.ToLower())
            {
                case "create":

                    /* create production */
                    SP_GET_ProductionNumber_Result productionnumber = vssp_db.SP_GET_ProductionNumber(lineid, proddate).FirstOrDefault();

                    Tbl_TRS_ProductionLineProcess ListDetail = new Tbl_TRS_ProductionLineProcess();
                    ListDetail.ProductionNumber = productionnumber.ProdNumber;
                    ListDetail.ProductionDate = proddate;
                    ListDetail.KanbanKey = kanbanqkey;
                    ListDetail.ShiftId = shiftid;
                    ListDetail.LineId = lineid;
                    ListDetail.GateId = gateid;
                    ListDetail.CustomerId = customerid;
                    ListDetail.PartNumber = partnumber;
                    ListDetail.Qty_OK = qty;
                    ListDetail.Qty_NG = 0;
                    ListDetail.OrderNumber = "";
                    ListDetail.UserId = uid;
                    ListDetail.EditDate = DateTime.Now;

                    vssp_db.Tbl_TRS_ProductionLineProcess.Add(ListDetail);

                    /* update kanban status */
                    var kanbantrue = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.KanbanKey == kanbanqkey).FirstOrDefault();
                    kanbantrue.Storage = true;

                    _ProductionLineProcess = ListDetail;
                    break;

                case "update":

                    /* update production */
                    Tbl_TRS_ProductionLineProcess ListUpdate = vssp_db.Tbl_TRS_ProductionLineProcess.Where(a => a.ProductionNumber == prodnumber).FirstOrDefault();
                    ListUpdate.Qty_OK = qty;
                    ListUpdate.Qty_NG = 0;
                    ListUpdate.UserId = uid;
                    ListUpdate.EditDate = DateTime.Now;

                    vssp_db.Tbl_TRS_ProductionLineProcess.Add(ListUpdate);

                    _ProductionLineProcess = ListUpdate;
                    break;

                case "delete":

                    /* delete production */
                    Tbl_TRS_ProductionLineProcess ListDelete = vssp_db.Tbl_TRS_ProductionLineProcess.Where(a => a.ProductionNumber == prodnumber).FirstOrDefault();
                    vssp_db.Tbl_TRS_ProductionLineProcess.Remove(ListDelete);

                    /* update kanban status */
                    var kanbanfalse = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.KanbanKey == kanbanqkey).FirstOrDefault();
                    kanbanfalse.Storage = false;

                    _ProductionLineProcess = ListDelete;
                    break;

            }

            return _ProductionLineProcess;

        }
        public ActionResult deleteKanbanProcessJson(string prodnumber)
        {
            Tbl_TRS_ProductionLineProcess kanbanProcess = vssp_db.Tbl_TRS_ProductionLineProcess.Where(a => a.ProductionNumber == prodnumber).FirstOrDefault();
            try
            {
                crudKanbanProcess(kanbanProcess.KanbanKey, kanbanProcess.ProductionNumber, kanbanProcess.ProductionDate, kanbanProcess.ShiftId, kanbanProcess.LineId, kanbanProcess.GateId, kanbanProcess.CustomerId, kanbanProcess.PartNumber, kanbanProcess.Qty_OK, kanbanProcess.UserId, "Delete");
                try
                {
                    vssp_db.SaveChanges();
                    return Json(kanbanProcess, JsonRequestBehavior.AllowGet);

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
        public ActionResult deletePartNGProcessJson(string prodnumber)
        {
            Tbl_TRS_ProductionNGProcess ngProcess = vssp_db.Tbl_TRS_ProductionNGProcess.Where(a => a.ProductionNumber == prodnumber).FirstOrDefault();
            try
            {
                vssp_db.Tbl_TRS_ProductionNGProcess.Remove(ngProcess);
                try
                {
                    vssp_db.SaveChanges();
                    return Json(ngProcess, JsonRequestBehavior.AllowGet);

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
        public ActionResult kanbanReader(string ShiftId, string LineId, string GateId, DateTime ProdDate, string KanbanData, string UserId)
        {

            GateId = _SystemService.Vf(GateId);

            string sparator = ";";
            string[] kanbandata = KanbanData.Split(new[] { sparator }, StringSplitOptions.None);

            string kanbankey = kanbandata[0];
            KanbanScanStatusModel kanbanScanStatus = new KanbanScanStatusModel();

            var kanbanValidation = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.KanbanKey == kanbankey).FirstOrDefault();
            if (kanbanValidation != null)
            {
                var kanbanActivedValidation = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.Actived == true && a.KanbanKey == kanbankey).FirstOrDefault();
                if (kanbanActivedValidation != null)
                {
                    var kanbanInStorageValidation = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.Actived == true && a.Storage == false && a.KanbanKey == kanbankey).FirstOrDefault();
                    if (kanbanInStorageValidation != null)
                    {
                        var kanbanLineValidation = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.Actived == true && a.Storage == false && a.LineId == LineId && a.KanbanKey == kanbankey).FirstOrDefault();
                        if (kanbanLineValidation != null)
                        {

                            var partValidation = vssp_db.Tbl_MST_PartFinishGoods.Where(a => (a.CustomerId == kanbanLineValidation.CustomerId && (a.PartNumber == kanbanLineValidation.PartNumber))).FirstOrDefault();
                            if (partValidation != null)
                            {
                                try
                                {
                                    var kanbanresult = crudKanbanProcess(kanbankey, "", ProdDate, ShiftId, LineId, GateId, partValidation.CustomerId, partValidation.PartNumber, partValidation.UnitQty, UserId, "Create");
                                    try
                                    {
                                        vssp_db.SaveChanges();
                                        kanbanScanStatus.ProductionNumber = kanbanresult.ProductionNumber;
                                        kanbanScanStatus.KanbanStatus = "Success";
                                        kanbanScanStatus.ErrMessages = "SCAN COMPLETED";
                                    }
                                    catch (DbEntityValidationException e)
                                    {
                                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                        var errinfo = _SystemService.GetExceptionDetails(e);
                                        kanbanScanStatus.KanbanStatus = "Error";
                                        kanbanScanStatus.ErrMessages = errinfo;
                                    }

                                }
                                catch (Exception e)
                                {
                                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    var errinfo = _SystemService.GetExceptionDetails(e);
                                    kanbanScanStatus.KanbanStatus = "Error";
                                    kanbanScanStatus.ErrMessages = errinfo;
                                }

                            }
                            else
                            {
                                kanbanScanStatus.KanbanStatus = "Error";
                                kanbanScanStatus.ErrMessages = "Wrong Part Number! Please check your kanban card.";
                            }
                        }
                        else
                        {
                            kanbanScanStatus.KanbanStatus = "Error";
                            kanbanScanStatus.ErrMessages = "Wrong kanban line! Please check your kanban card.";
                        }
                    }

                    else
                    {
                        kanbanScanStatus.KanbanStatus = "Error";
                        kanbanScanStatus.ErrMessages = "Kanban already scanned or in storage! Please set kanban out storage in kanban control.";
                    }
                }

                else
                {
                    kanbanScanStatus.KanbanStatus = "Error";
                    kanbanScanStatus.ErrMessages = "Kanban not actived! Please check your kanban card.";
                }
            }
            else
            {
                kanbanScanStatus.KanbanStatus = "Error";
                kanbanScanStatus.ErrMessages = "Kanban not found! Please check your kanban card.";
            }
            return Json(kanbanScanStatus, JsonRequestBehavior.AllowGet);

        }
        public ActionResult crudPartNG(
            string prodnumber, DateTime proddate, string shiftid, string lineid, string customerid, string partnumber, 
            double? ngqty, string defectid, bool repair, bool scrap,
            string uid, string formAction)
        {
            try
            {
                switch (formAction.ToLower())
                {
                    case "create":

                        /* create production */
                        SP_GET_ProductionNGNumber_Result productionnumber = vssp_db.SP_GET_ProductionNGNumber(lineid, proddate).FirstOrDefault();

                        Tbl_TRS_ProductionNGProcess ListDetail = new Tbl_TRS_ProductionNGProcess();
                        ListDetail.ProductionNumber = productionnumber.ProdNumber;
                        ListDetail.ProductionDate = proddate;
                        ListDetail.ShiftId = shiftid;
                        ListDetail.LineId = lineid;
                        ListDetail.CustomerId = customerid;
                        ListDetail.PartNumber = partnumber;
                        ListDetail.Qty_NG = ngqty;
                        ListDetail.DefectId = defectid;
                        ListDetail.Repair = repair;
                        ListDetail.Scrap = scrap;
                        ListDetail.UserId = uid;
                        ListDetail.EditDate = DateTime.Now;

                        vssp_db.Tbl_TRS_ProductionNGProcess.Add(ListDetail);

                        break;

                    case "update":

                        /* update production */
                        Tbl_TRS_ProductionNGProcess ListUpdate = vssp_db.Tbl_TRS_ProductionNGProcess.Where(a => a.ProductionNumber == prodnumber).FirstOrDefault();
                        ListUpdate.Qty_NG = ngqty;
                        ListUpdate.DefectId = defectid;
                        ListUpdate.Repair = repair;
                        ListUpdate.Scrap = scrap;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        //vssp_db.Tbl_TRS_ProductionNGProcess.Add(ListUpdate);

                        break;

                    case "delete":

                        /* delete production */
                        Tbl_TRS_ProductionNGProcess ListDelete = vssp_db.Tbl_TRS_ProductionNGProcess.Where(a => a.ProductionNumber == prodnumber).FirstOrDefault();
                        vssp_db.Tbl_TRS_ProductionNGProcess.Remove(ListDelete);

                        break;
                }
                try
                {
                    vssp_db.SaveChanges();
                    return Json(partnumber, JsonRequestBehavior.AllowGet);

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
    }

}