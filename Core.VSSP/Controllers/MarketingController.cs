using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Dapper;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace Core.VSSP.Controllers
{
    public class MarketingController : Controller
    {
        // GET: Marketing
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult SalesOrder()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Marketing", "SalesOrder");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
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

                    SalesOrderListModel SalesOrder = new SalesOrderListModel();
                    SalesOrder.ExportList = _SystemService.ComboExport().ToList();
                    SalesOrder.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                             orderby a.Id
                                             select a).ToList();

                    Session["Layout"] = "portal";
                    return View(SalesOrder);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult SalesOrderListJson(
                                    string searchFilter,
                                    string customerid,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99, bool incClosed = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_SalesOrder> SalesOrder = new List<Vw_TRS_SalesOrder>();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                SalesOrder = (from a in vssp_db.Vw_TRS_SalesOrder
                              where a.SODate >= startdate && a.SODate <= enddate && (a.SONumber.Contains(searchFilter) || a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter) || a.PONumber.Contains(searchFilter))
                              orderby a.DeliveryYear descending, a.DeliveryMonth descending, a.SODate descending, a.SONumber descending
                              select a).ToList();
                SalesOrder = SalesOrder.Where(a => a.SODate >= startdate && a.SODate <= enddate).ToList();
            }
            else
            {
                SalesOrder = (from a in vssp_db.Vw_TRS_SalesOrder
                              where a.SONumber.Contains(searchFilter) || a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter) || a.PONumber.Contains(searchFilter)
                              orderby a.DeliveryYear descending, a.DeliveryMonth descending, a.SODate descending, a.SONumber descending
                              select a).ToList();
            }

            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string ordermonth = arrs[0];
                string orderyears = arrs[1];
                SalesOrder = SalesOrder.Where(a => a.DeliveryMonth == ordermonth && a.DeliveryYear == orderyears).ToList();
            }
            if (_SystemService.Vf(customerid) != "")
            {
                SalesOrder = SalesOrder.Where(a => a.CustomerId == customerid).ToList();
            }
            //if (status != 99)
            //{
            //    SalesOrder = SalesOrder.Where(a => a.Status.ToString() == status.ToString()).ToList();
            //}
            if (status != 99)
            {
                SalesOrder = SalesOrder.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in SalesOrder
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                SalesOrder = SalesOrder.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }
            if (!incClosed)
            {
                SalesOrder = SalesOrder.Where(a => a.Status != 3).ToList();
            }

            return Json(SalesOrder, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SalesOrderDetailListJson(string SONumber)
        {
            try
            {

                var SalesOrderDetail = from a in vssp_db.Tbl_TRS_SalesOrderDetail
                                       join b in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber }
                                       where a.SONumber == SONumber
                                       orderby a.CustomerId,b.UniqueNumber,a.PartNumber
                                       select new { a.SONumber, a.CustomerId, b.UniqueNumber, a.PartNumber, b.PartName, Model = b.CustomerUnitModel, QtyByKanban = b.UnitQty, Unit = b.UnitLevel2, a.OrderQty, a.OrderN1, a.OrderN2, a.OrderN3, a.DeliveryPerDay };

                return Json(SalesOrderDetail, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudSalesOrderList(string jsonData, string jsonSchedule)
        {

            try
            {

                vssp_db.Database.CommandTimeout = 0;

                PostSalesOrderModel postSalesOrder = JsonConvert.DeserializeObject<PostSalesOrderModel>(jsonData);
                Tbl_TRS_SalesOrder SalesOrder = postSalesOrder.SalesOrder;
                List<crud_SalesOrderDetail> SalesOrderDetail = postSalesOrder.SalesOrderDetail;
                List<crud_ScheduleDelivery> ScheduleDelivery = postSalesOrder.ScheduleDelivery;
                string uid = postSalesOrder.uid;
                string formAction = postSalesOrder.formAction.ToLower();

                switch (formAction)
                {
                    case "create":

                        /* Get New Order Number */
                        string CompId = Session["CompID"].ToString();
                        var SONumber = vssp_db.SP_GET_SalesNumber(SalesOrder.DeliveryMonth, SalesOrder.DeliveryYear, CompId);
                        foreach (SP_GET_SalesNumber_Result number in SONumber)
                        {
                            SalesOrder.SONumber = number.OrderNumber;
                        }

                        Tbl_TRS_SalesOrder ListSalesOrder = new Tbl_TRS_SalesOrder();
                        ListSalesOrder.SONumber = SalesOrder.SONumber;
                        ListSalesOrder.SODate = SalesOrder.SODate;
                        ListSalesOrder.CustomerId = SalesOrder.CustomerId;
                        ListSalesOrder.PONumber = SalesOrder.PONumber;
                        ListSalesOrder.PODate = SalesOrder.PODate;
                        ListSalesOrder.ReceiveDate = SalesOrder.ReceiveDate;
                        ListSalesOrder.DeliveryMonth = SalesOrder.DeliveryMonth;
                        ListSalesOrder.DeliveryYear = SalesOrder.DeliveryYear;
                        ListSalesOrder.PassThrough = SalesOrder.PassThrough;
                        ListSalesOrder.StartDelivery = SalesOrder.StartDelivery;
                        ListSalesOrder.StartDateDelivery = SalesOrder.StartDateDelivery;
                        ListSalesOrder.EndDateDelivery = SalesOrder.EndDateDelivery;
                        ListSalesOrder.Remarks = SalesOrder.Remarks;
                        ListSalesOrder.Status = 0;
                        ListSalesOrder.UserID = uid;
                        ListSalesOrder.EditDate = DateTime.Now;

                        vssp_db.Tbl_TRS_SalesOrder.Add(ListSalesOrder);

                        /* crud Details */
                        crudSalesOrderDetail(SalesOrderDetail, SalesOrder.SONumber, SalesOrder.CustomerId);
                        /* crud Delivery Schedule */
                        crudScheduleDelivery(ScheduleDelivery, SalesOrder.SONumber, SalesOrder.CustomerId, formAction);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_SalesOrder.First(a => a.SONumber == SalesOrder.SONumber);

                        ListUpdate.SODate = SalesOrder.SODate;
                        ListUpdate.CustomerId = SalesOrder.CustomerId;
                        ListUpdate.PONumber = SalesOrder.PONumber;
                        ListUpdate.PODate = SalesOrder.PODate;
                        ListUpdate.ReceiveDate = SalesOrder.ReceiveDate;
                        ListUpdate.DeliveryMonth = SalesOrder.DeliveryMonth;
                        ListUpdate.DeliveryYear = SalesOrder.DeliveryYear;
                        ListUpdate.PassThrough = SalesOrder.PassThrough;
                        ListUpdate.StartDelivery = SalesOrder.StartDelivery;
                        ListUpdate.StartDateDelivery = SalesOrder.StartDateDelivery;
                        ListUpdate.EndDateDelivery = SalesOrder.EndDateDelivery;
                        ListUpdate.Remarks = SalesOrder.Remarks;
                        ListUpdate.UserID = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Details */
                        crudSalesOrderDetail(SalesOrderDetail, SalesOrder.SONumber, SalesOrder.CustomerId);
                        /* crud Delivery Schedule */
                        crudScheduleDelivery(ScheduleDelivery, SalesOrder.SONumber, SalesOrder.CustomerId, formAction);

                        break;

                    case "closed":

                        var ListClosed = vssp_db.Tbl_TRS_SalesOrder.First(a => a.SONumber == SalesOrder.SONumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_TRS_SalesOrder.First(a => a.SONumber == SalesOrder.SONumber);

                        ListCanceled.Status = 4;

                        break;

                    case "delete":

                        var ListDelete = vssp_db.Tbl_TRS_SalesOrder.First(a => a.SONumber == SalesOrder.SONumber);

                        ListDelete.Status = 5;

                        ///* remove existing Detail */
                        //var DetailDelete = from a in vssp_db.Tbl_TRS_SalesOrderDetail
                        //                    where a.SONumber == SalesOrder.SONumber
                        //                    select a;

                        //foreach (var Detail in DetailDelete)
                        //{
                        //    vssp_db.Tbl_TRS_SalesOrderDetail.Remove(Detail);
                        //}

                        ///* remove existing SalesOrder */
                        //var ListDelete = vssp_db.Tbl_TRS_SalesOrder.First(a => a.SONumber == SalesOrder.SONumber);
                        //vssp_db.Tbl_TRS_SalesOrder.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(SalesOrder, JsonRequestBehavior.AllowGet);
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

        public void crudSalesOrderDetail(List<crud_SalesOrderDetail> SalesOrderDetails, string SONumber, string CustomerId)
        {

            foreach (var Details in SalesOrderDetails)
            {
                if (Details.RowStatus != null)
                {
                    switch (Details.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_SalesOrderDetail ListDetail = new Tbl_TRS_SalesOrderDetail();
                            ListDetail.SONumber = SONumber;
                            ListDetail.CustomerId = CustomerId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.OrderQty = Details.OrderQty;
                            ListDetail.OrderN1 = Details.OrderN1;
                            ListDetail.OrderN2 = Details.OrderN2;
                            ListDetail.OrderN3 = Details.OrderN3;
                            ListDetail.DeliveryPerDay = Details.DeliveryPerDay;

                            vssp_db.Tbl_TRS_SalesOrderDetail.Add(ListDetail);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_SalesOrderDetail.First(a => a.SONumber == SONumber && a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber);

                            ListUpdate.CustomerId = CustomerId;
                            ListUpdate.PartNumber = Details.PartNumber;
                            ListUpdate.OrderQty = Details.OrderQty;
                            ListUpdate.OrderN1 = Details.OrderN1;
                            ListUpdate.OrderN2 = Details.OrderN2;
                            ListUpdate.OrderN3 = Details.OrderN3;
                            ListUpdate.DeliveryPerDay = Details.DeliveryPerDay;

                            break;

                        case "delete":

                            //var ScdlDelete = vssp_db.Tbl_TRS_ScheduleDelivery.First(a => a.SONumber == SONumber && a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber);
                            //vssp_db.Tbl_TRS_ScheduleDelivery.Remove(ScdlDelete);

                            var ListDelete = vssp_db.Tbl_TRS_SalesOrderDetail.First(a => a.SONumber == SONumber && a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber);
                            vssp_db.Tbl_TRS_SalesOrderDetail.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        public void crudScheduleDelivery(List<crud_ScheduleDelivery> scheduleDeliveries, string SONumber, string CustomerId, string FormAction)
        {

            /* clear schedule */
            var ListDelete = vssp_db.Tbl_TRS_ScheduleDelivery.Where(a => a.SONumber == SONumber).ToList();
            foreach(var delete in ListDelete)
            {
                vssp_db.Tbl_TRS_ScheduleDelivery.Remove(delete);
            }

            foreach (var Details in scheduleDeliveries)
            {

                    switch (FormAction.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_ScheduleDelivery ListDetail = new Tbl_TRS_ScheduleDelivery();
                            ListDetail.SONumber = SONumber;
                            ListDetail.CustomerId = CustomerId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.DeliveryDate = Details.DeliveryDate;
                            ListDetail.DeliveryQty = Details.DeliveryQty;

                            vssp_db.Tbl_TRS_ScheduleDelivery.Add(ListDetail);

                            break;

                        case "update":

                            /* create Details */
                            Tbl_TRS_ScheduleDelivery ListUpdate = new Tbl_TRS_ScheduleDelivery();
                            ListUpdate.SONumber = SONumber;
                            ListUpdate.CustomerId = CustomerId;
                            ListUpdate.PartNumber = Details.PartNumber;
                            ListUpdate.DeliveryDate = Details.DeliveryDate;
                            ListUpdate.DeliveryQty = Details.DeliveryQty;

                            vssp_db.Tbl_TRS_ScheduleDelivery.Add(ListUpdate);


                        break;


                }
            }

        }
        
        public ActionResult ImportJson()
        {
            try
            {
                HttpFileCollectionBase files = Request.Files;
                var ListUpload = this.uploadSalesOrderExcel(files[0]);
                return Json(ListUpload, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json("Error! " + errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ImportSalesOrderListModel uploadSalesOrderExcel(HttpPostedFileBase files)
        {
            ImportSalesOrderListModel _ImportListModel = new ImportSalesOrderListModel();
            _ImportListModel.ImportSalesOrder = new List<ImportSalesOrderModel>();
            _ImportListModel.ImportSalesOrderDetail = new List<ImportSalesOrderDetailModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));
                string customerid = "";

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //SalesOrder
                    var workSheet = package.Workbook.Worksheets[1];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var SalesOrder = new ImportSalesOrderModel();

                    bool validTemplate = true;

                    //header validator SalesOrder
                    var SalesOrderTemplate = _SystemService.Vf(workSheet?.Cells[1, 1]?.Value?.ToString());

                    if (SalesOrderTemplate.Replace(" ", "").Replace("*", "").ToLower() != "salesorder")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    stopValidation:

                    //get data excel
                    if (validTemplate == true)
                    {

                        ImportSalesOrderModel ImportList = new ImportSalesOrderModel();

                        ImportList.Status = "";
                        ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[2, 3]?.Value?.ToString());
                        ImportList.PONumber = _SystemService.Vf(workSheet?.Cells[3, 3]?.Value?.ToString());
                        if (workSheet?.Cells[2, 5]?.Value?.ToString() != null)
                        {
                            ImportList.PODate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[2, 5]?.Value?.ToString(), "yyyy-MM-dd"));
                        }
                        else
                        {
                            ImportList.Status += "PO Date Empty </br>";
                        }
                        if (workSheet?.Cells[3, 5]?.Value?.ToString() != null)
                        {
                            ImportList.ReceiveDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[3, 5]?.Value?.ToString(), "yyyy-MM-dd"));
                        }
                        else
                        {
                            ImportList.Status += "Receive Date Empty </br>";
                        }
                        ImportList.DeliveryMonth = _SystemService.Vf(workSheet?.Cells[4, 5]?.Value?.ToString());
                        ImportList.DeliveryYear = _SystemService.Vf(workSheet?.Cells[4, 6]?.Value?.ToString());
                        ImportList.PassThrough = _SystemService.Vb(workSheet?.Cells[4, 3]?.Value?.ToString());
                        ImportList.Remarks = "Import";

                        var exceptList = new List<string> { "4", "5" };
                        var exist = (from a in vssp_db.Tbl_TRS_SalesOrder
                                     where a.CustomerId == ImportList.CustomerId && a.PONumber == ImportList.PONumber && !exceptList.Contains(a.Status.ToString())
                                     select a).FirstOrDefault();

                        if (exist != null)
                        {
                            ImportList.Status += "PO Number already exist with SO Number " + exist.SONumber + "</br>";
                        }

                        customerid = ImportList.CustomerId;
                        _ImportListModel.ImportSalesOrder.Add(ImportList);
                    }

                    //SalesOrder Detail
                    workSheet = package.Workbook.Worksheets[1];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var SalesOrderDetail = new ImportSalesOrderDetailModel();

                    //header validator SalesOrder
                    var UniqueNumber = _SystemService.Vf(workSheet?.Cells[6, 2]?.Value?.ToString());
                    var PartNumber = _SystemService.Vf(workSheet?.Cells[6, 3]?.Value?.ToString());
                    var N = _SystemService.Vf(workSheet?.Cells[6, 4]?.Value?.ToString());
                    var N1 = _SystemService.Vf(workSheet?.Cells[6, 5]?.Value?.ToString());
                    var N2 = _SystemService.Vf(workSheet?.Cells[6, 6]?.Value?.ToString());
                    var N3 = _SystemService.Vf(workSheet?.Cells[6, 7]?.Value?.ToString());
                    var DelQty = _SystemService.Vf(workSheet?.Cells[6, 8]?.Value?.ToString());

                    if (UniqueNumber.Replace(" ", "").Replace("*", "").ToLower() != "uniquenumber")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (N.Replace(" ", "").Replace("*", "").ToLower() != "n")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (N1.Replace(" ", "").Replace("*", "").ToLower() != "n+1")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (N2.Replace(" ", "").Replace("*", "").ToLower() != "n+2")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (N3.Replace(" ", "").Replace("*", "").ToLower() != "n+3")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (DelQty.Replace(" ", "").Replace("*", "").ToLower() != "del/day")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }

                    stopValidation2:

                    //get data excel
                    if (validTemplate == true)
                    {

                        for (int rowIterator = 7; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportSalesOrderDetailModel ImportList = new ImportSalesOrderDetailModel();

                            ImportList.CustomerId = customerid;
                            ImportList.UniqueNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.OrderQty = _SystemService.Vn(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.OrderN1 = _SystemService.Vn(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.OrderN2 = _SystemService.Vn(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.OrderN3 = _SystemService.Vn(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.DeliveryPerDay = _SystemService.Vn(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());

                            var part = (from a in vssp_db.Tbl_MST_PartFinishGoods
                                        where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber
                                        select a).FirstOrDefault();

                            if (part == null)
                            {
                                ImportList.RowStatus = "Invalid";
                            }
                            else
                            {
                                ImportList.UniqueNumber = part.UniqueNumber;
                                ImportList.PartName = part.PartName;
                                ImportList.Model = part.CustomerUnitModel;
                                ImportList.QtyByKanban = part.UnitQty;
                                ImportList.Unit = part.UnitLevel2;
                                ImportList.RowStatus = "Create";
                            }

                            if (ImportList.UniqueNumber == "")
                            {
                                goto stopUpload2;
                            }

                            _ImportListModel.ImportSalesOrderDetail.Add(ImportList);
                        }
                    };

                }
            }

            stopUpload2:

            return _ImportListModel;

        }
        public ActionResult DeliveryScheduleJson(string sonumber, Nullable<DateTime> startdate = null, Nullable<DateTime> enddate = null)
        {
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
                p.Add("@SONumber", sonumber, DbType.String);

                ShiftSchedule = (IEnumerable<IDictionary<string, object>>)
                            cnn.Query(sql: "SP_TRS_ScheduleDelivery",
                                      param: p,
                                      commandType: CommandType.StoredProcedure);
            }

            
            return Json(ShiftSchedule, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeliverySchedule()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Marketing", "DeliverySchedule");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = acccessPreviliege.MenuName;
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
    }
    
}