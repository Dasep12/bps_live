using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
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
    public class PurchaseController : Controller
    {
        // GET: ForecastOrder
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult ForecastOrder()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Purchase", "ForecastOrder");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Part Requirement List";
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

                    ForecastOrderListModel forecastOrder = new ForecastOrderListModel();
                    forecastOrder.ExportList = _SystemService.ComboExport().ToList();
                    forecastOrder.StatusList = (from a in vssp_db.Tbl_TRS_Status
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

                        return View(forecastOrder);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ForecastOrderListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_ForecastOrder> ForecastOrder = (from a in vssp_db.Vw_TRS_ForecastOrder
                                                        where a.OrderNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter) || a.SupplierName.Contains(searchFilter)
                                                        orderby a.OrderYear descending, a.OrderMonth descending, a.OrderDate descending, a.EditDate descending, a.OrderNumber
                                                        select a).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                ForecastOrder = ForecastOrder.Where(a => a.OrderDate >= startdate && a.OrderDate <= enddate).ToList();
            }
            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string ordermonth = arrs[0];
                string orderyears = arrs[1];
                ForecastOrder = ForecastOrder.Where(a => a.OrderMonth == ordermonth && a.OrderYear == orderyears).ToList();
            }
            if (status != 99)
            {
                ForecastOrder = ForecastOrder.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in ForecastOrder
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                ForecastOrder = ForecastOrder.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            return Json(ForecastOrder, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ForecastOrderDetailListJson(string ordernumber, string month, string supplierid, string formAction)
        {
            try
            {

                var ForecastOrderDetail = new object();

                if (_SystemService.Vf(supplierid) == "" || _SystemService.Vf(month) == "")
                {

                    ForecastOrderDetail = new SP_TRS_ForecastOrderDetailLast_Result();

                }
                else
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

                            ForecastOrderDetail = vssp_db.SP_TRS_ForecastOrderDetailLast(supplierid, ordermonth, orderyears, ordernumber);
                            break;

                        case "regenerate":

                            //ordernumber = "";
                            ForecastOrderDetail = vssp_db.SP_TRS_ForecastOrderDetailAdditional(supplierid, ordermonth, orderyears, ordernumber);
                            break;

                        default:

                            ForecastOrderDetail = vssp_db.SP_TRS_ForecastOrderDetailLast(supplierid, ordermonth, orderyears, ordernumber);
                            break;
                    }

                }

                return Json(ForecastOrderDetail, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ForecastOrderRevisionListJson(string ordernumber)
        {
            try
            {

                var ForecastOrderRevision = from a in vssp_db.Tbl_TRS_ForecastOrderRevision
                                            where a.OrderNumber.Contains(ordernumber)
                                            orderby a.RevisionNumber
                                            select new { a.OrderNumber, a.RevisionNumber, a.Description, a.RevisionDate, a.RevisionBy };

                return Json(ForecastOrderRevision, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ForecastOrderApprovalListJson(string ordernumber, Nullable<bool> approved)
        {
            try
            {

                var ForecastOrderApproval = from a in vssp_db.Tbl_TRS_ForecastOrderApproval
                                            where a.OrderNumber.Contains(ordernumber)
                                            orderby a.ApprovalLevel
                                            select new { a.OrderNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

                if (approved != null)
                {
                    ForecastOrderApproval = ForecastOrderApproval.Where(a => a.Approved == approved);
                }

                return Json(ForecastOrderApproval, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ForecastWorkDayJson(string month)
        {
            try
            {

                ForecastWorkdayModel forecastWorkday = new ForecastWorkdayModel();

                string[] arrs = _SystemService.Vf(month).Split('/');
                string ordermonth = "";
                string orderyears = "";
                DateTime now = DateTime.Now;
                DateTime startdate = now;
                DateTime enddate = now;

                if (_SystemService.Vf(month) != "")
                {
                    ordermonth = arrs[0];
                    orderyears = arrs[1];
                    startdate = new DateTime(int.Parse(orderyears), int.Parse(ordermonth), 1);
                }
                else
                {
                    startdate = new DateTime(now.Year, now.Month, 1);
                }

                /* N10 */
                startdate = startdate.AddMonths(-1);
                enddate = startdate.AddMonths(1).AddDays(-1);
                var workday10 = vssp_db.SP_GET_WorkDay(startdate, enddate, "N", false);
                foreach (SP_GET_Workday_Result day in workday10)
                {
                    forecastWorkday.M10 = startdate.ToString("MMM").ToUpper();
                    forecastWorkday.N10 = day.TotalDay;
                }
                /* N00 */
                startdate = startdate.AddMonths(1);
                enddate = startdate.AddMonths(1).AddDays(-1);
                var workday00 = vssp_db.SP_GET_WorkDay(startdate, enddate, "N", false);
                foreach (SP_GET_Workday_Result day in workday00)
                {
                    forecastWorkday.M00 = startdate.ToString("MMM").ToUpper();
                    forecastWorkday.N00 = day.TotalDay;
                }
                /* N01 */
                startdate = startdate.AddMonths(1);
                enddate = startdate.AddMonths(1).AddDays(-1);
                var workday01 = vssp_db.SP_GET_WorkDay(startdate, enddate, "N", false);
                foreach (SP_GET_Workday_Result day in workday01)
                {
                    forecastWorkday.M01 = startdate.ToString("MMM").ToUpper();
                    forecastWorkday.N01 = day.TotalDay;
                }
                /* N02 */
                startdate = startdate.AddMonths(1);
                enddate = startdate.AddMonths(1).AddDays(-1);
                var workday02 = vssp_db.SP_GET_WorkDay(startdate, enddate, "N", false);
                foreach (SP_GET_Workday_Result day in workday02)
                {
                    forecastWorkday.M02 = startdate.ToString("MMM").ToUpper();
                    forecastWorkday.N02 = day.TotalDay;
                }

                /* N03 */
                startdate = startdate.AddMonths(1);
                enddate = startdate.AddMonths(1).AddDays(-1);
                var workday03 = vssp_db.SP_GET_WorkDay(startdate, enddate, "N", false);
                foreach (SP_GET_Workday_Result day in workday03)
                {
                    forecastWorkday.M03 = startdate.ToString("MMM").ToUpper();
                    forecastWorkday.N03 = day.TotalDay;
                }
                TempData["forcastWorkday"] = forecastWorkday;
                return Json(forecastWorkday, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult checkForecastActived(string supplierId, string month)
        {
            string[] arrs = _SystemService.Vf(month).Split('/');
            string ordermonth = "";
            string orderyears = "";

            if (_SystemService.Vf(month) != "")
            {
                ordermonth = arrs[0];
                orderyears = arrs[1];
            }

            var checkForecast = (from a in vssp_db.Vw_TRS_ForecastOrder
                                 where a.SupplierId == supplierId && a.OrderYear == orderyears && a.OrderMonth == ordermonth &&
                                 (a.Status.ToString().Contains("0") || a.Status.ToString().Contains("1") || a.Status.ToString().Contains("2") || a.Status.ToString().Contains("3"))
                                 select a).ToList();

            return Json(checkForecast, JsonRequestBehavior.AllowGet);

        }
        public ActionResult crudForecastOrderList(string jsonData)
        {

            try
            {

                PostForecastOrderModel postForecastOrder = JsonConvert.DeserializeObject<PostForecastOrderModel>(jsonData);
                Tbl_TRS_ForecastOrder ForecastOrder = postForecastOrder.ForecastOrder;
                List<crud_ForecastOrderDetail> ForecastOrderDetail = postForecastOrder.ForecastOrderDetail;
                List<crud_ForecastOrderRevision> ForecasrOrderRevision = postForecastOrder.ForecastOrderRevision;

                string uid = postForecastOrder.uid;
                string formAction = postForecastOrder.formAction.ToLower();

                switch (formAction)
                {
                    case "create":


                        /* Get New Order Number */
                        string CompId = Session["CompID"].ToString();
                        var OrderNumber = vssp_db.SP_GET_ForecastNumber(ForecastOrder.OrderMonth, ForecastOrder.OrderYear, CompId);
                        foreach (SP_GET_ForecastNumber_Result number in OrderNumber)
                        {
                            ForecastOrder.OrderNumber = number.OrderNumber;
                        }

                        Tbl_TRS_ForecastOrder ListForecastOrder = new Tbl_TRS_ForecastOrder();
                        ListForecastOrder.OrderNumber = ForecastOrder.OrderNumber;
                        ListForecastOrder.OrderDate = ForecastOrder.OrderDate;
                        ListForecastOrder.OrderYear = ForecastOrder.OrderYear;
                        ListForecastOrder.OrderMonth = ForecastOrder.OrderMonth;
                        ListForecastOrder.SupplierId = ForecastOrder.SupplierId;
                        ListForecastOrder.Remarks = ForecastOrder.Remarks;
                        ListForecastOrder.Shift = ForecastOrder.Shift;
                        ListForecastOrder.Status = 0;
                        ListForecastOrder.UserId = uid;
                        ListForecastOrder.EditDate = DateTime.Now;

                        vssp_db.Tbl_TRS_ForecastOrder.Add(ListForecastOrder);

                        /* crud Details */
                        crudForecastOrderDetail(ForecastOrderDetail, ForecastOrder.OrderNumber, ForecastOrder.SupplierId, formAction);

                        /* crud Approval */
                        crudForecastOrderApproval(postForecastOrder.ApprovalId, ForecastOrder.OrderNumber, uid, formAction);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_ForecastOrder.First(a => a.OrderNumber == ForecastOrder.OrderNumber);

                        ListUpdate.OrderDate = ForecastOrder.OrderDate;
                        ListUpdate.OrderYear = ForecastOrder.OrderYear;
                        ListUpdate.OrderMonth = ForecastOrder.OrderMonth;
                        ListUpdate.SupplierId = ForecastOrder.SupplierId;
                        ListUpdate.Remarks = ForecastOrder.Remarks;
                        ListUpdate.Shift = ForecastOrder.Shift;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Details */
                        crudForecastOrderDetail(ForecastOrderDetail, ForecastOrder.OrderNumber, ForecastOrder.SupplierId, formAction);

                        /* crud Approval */
                        crudForecastOrderApproval(postForecastOrder.ApprovalId, ForecastOrder.OrderNumber, uid, formAction);

                        /* crud Revisions */
                        crudForecastOrderRevision(ForecasrOrderRevision, ForecastOrder.OrderNumber, uid);

                        break;
                    case "closed":

                        var ListClosed = vssp_db.Tbl_TRS_ForecastOrder.First(a => a.OrderNumber == ForecastOrder.OrderNumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_TRS_ForecastOrder.First(a => a.OrderNumber == ForecastOrder.OrderNumber);

                        ListCanceled.Status = 4;

                        break;

                    case "delete":

                        /* remove existing ForecastOrder */
                        var ListDelete = vssp_db.Tbl_TRS_ForecastOrder.First(a => a.OrderNumber == ForecastOrder.OrderNumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        //vssp_db.Tbl_TRS_ForecastOrder.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();

                    if (formAction == "create" || formAction == "update")
                    {
                        //crudMasterListKanbanDetail(ForecastOrder.OrderNumber); <-- MLOK Produksi
                        crudMasterListKanbanSupplier(ForecastOrder.OrderNumber);
                        crudMasterListKanbanStock(ForecastOrder.OrderNumber);
                    }

                    return Json(ForecastOrder, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
                {
                    if (formAction == "create")
                    {
                        var ListErr = vssp_db.Tbl_TRS_ForecastOrder.First(a => a.OrderNumber == ForecastOrder.OrderNumber);
                        vssp_db.Tbl_TRS_ForecastOrder.Remove(ListErr);

                        var ListErrDetail = vssp_db.Tbl_TRS_ForecastOrderDetail.Where(a => a.OrderNumber == ForecastOrder.OrderNumber).ToList();
                        foreach (var detail in ListErrDetail)
                        {
                            vssp_db.Tbl_TRS_ForecastOrderDetail.Remove(detail);
                        }
                    }

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

        public void crudForecastOrderDetail(List<crud_ForecastOrderDetail> ForecastOrderDetails, string OrderNumber, string SupplierId, string formAction)
        {

            foreach (var Details in ForecastOrderDetails)
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
                            Tbl_TRS_ForecastOrderDetail ListDetail = new Tbl_TRS_ForecastOrderDetail();
                            ListDetail.OrderNumber = OrderNumber;
                            ListDetail.SupplierId = SupplierId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.OrderLastQty = Details.OrderLastQty;
                            ListDetail.OrderQty = Details.OrderQty;
                            ListDetail.DailyLastQty = Details.DailyLastQty;
                            ListDetail.DailyQty = Details.DailyQty;
                            ListDetail.N1 = Details.N1;
                            ListDetail.N2 = Details.N2;
                            ListDetail.N3 = Details.N3;
                            ListDetail.FluctuationQty = Details.FluctuationQty;
                            ListDetail.FluctuationPercent = Details.FluctuationPercent;
                            ListDetail.SONumber = Details.SONumber;

                            vssp_db.Tbl_TRS_ForecastOrderDetail.Add(ListDetail);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_ForecastOrderDetail.First(a => a.OrderNumber == OrderNumber && a.SupplierId == SupplierId && a.PartNumber == Details.PartNumber);

                            ListUpdate.OrderLastQty = Details.OrderLastQty;
                            ListUpdate.OrderQty = Details.OrderQty;
                            ListUpdate.DailyLastQty = Details.DailyLastQty;
                            ListUpdate.DailyQty = Details.DailyQty;
                            ListUpdate.N1 = Details.N1;
                            ListUpdate.N2 = Details.N2;
                            ListUpdate.N3 = Details.N3;
                            ListUpdate.FluctuationQty = Details.FluctuationQty;
                            ListUpdate.FluctuationPercent = Details.FluctuationPercent;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_ForecastOrderDetail.First(a => a.OrderNumber == OrderNumber && a.SupplierId == SupplierId && a.PartNumber == Details.PartNumber);

                            vssp_db.Tbl_TRS_ForecastOrderDetail.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        public void crudForecastOrderRevision(List<crud_ForecastOrderRevision> ForecastOrderRevision, string OrderNumber, string UserId)
        {

            if (ForecastOrderRevision != null)
            {
                foreach (var Revisions in ForecastOrderRevision)
                {
                    if (Revisions.RowStatus != null)
                    {
                        switch (Revisions.RowStatus.ToLower())
                        {
                            case "create":

                                /* create Revisions */
                                Tbl_TRS_ForecastOrderRevision ListRevision = new Tbl_TRS_ForecastOrderRevision();
                                ListRevision.OrderNumber = OrderNumber;
                                ListRevision.RevisionNumber = Revisions.RevisionNumber;
                                ListRevision.Description = Revisions.Description;
                                ListRevision.RevisionDate = Revisions.RevisionDate;
                                ListRevision.RevisionBy = UserId;

                                vssp_db.Tbl_TRS_ForecastOrderRevision.Add(ListRevision);

                                break;

                            case "update":

                                var ListUpdate = vssp_db.Tbl_TRS_ForecastOrderRevision.First(a => a.OrderNumber == OrderNumber && a.RevisionNumber == Revisions.RevisionNumber);

                                ListUpdate.Description = Revisions.Description;
                                ListUpdate.RevisionDate = Revisions.RevisionDate;
                                ListUpdate.RevisionBy = UserId;

                                break;

                            case "delete":

                                var ListDelete = vssp_db.Tbl_TRS_ForecastOrderRevision.First(a => a.OrderNumber == OrderNumber && a.RevisionNumber == Revisions.RevisionNumber);

                                vssp_db.Tbl_TRS_ForecastOrderRevision.Remove(ListDelete);

                                break;
                        }
                    }
                }
            }
        }
        public void crudForecastOrderApproval(string ApprovalId, string OrderNumber, string UserId, string action)
        {
            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    List<UserApprovalListModel> userApprovalLists = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalLists)
                    {
                        Tbl_TRS_ForecastOrderApproval ListApproval = new Tbl_TRS_ForecastOrderApproval();
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

                        vssp_db.Tbl_TRS_ForecastOrderApproval.Add(ListApproval);
                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_TRS_ForecastOrderApproval> UserApproval = vssp_db.Tbl_TRS_ForecastOrderApproval.Where(a => a.OrderNumber == OrderNumber).ToList();
                    foreach (var user in UserApproval)
                    {
                        UserApprovalListModel ApprovalLists = _AccountService.UserApprovalType(user.UserId, ApprovalId).First(a => a.UserID == user.UserId);
                    }

                    /* create approval */
                    List<UserApprovalListModel> userApprovalListsUpdate = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalListsUpdate)
                    {
                        Tbl_TRS_ForecastOrderApproval existUser = (from a in vssp_db.Tbl_TRS_ForecastOrderApproval
                                                                   where a.OrderNumber == OrderNumber && a.UserId == users.UserID
                                                                   select a).FirstOrDefault();
                        if (existUser == null)
                        {
                            Tbl_TRS_ForecastOrderApproval ListApproval = new Tbl_TRS_ForecastOrderApproval();
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

                            vssp_db.Tbl_TRS_ForecastOrderApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "sent":

                    var ListSent = vssp_db.Tbl_TRS_ForecastOrderApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == ApprovalId);

                    ListSent.SentEmail = true;
                    ListSent.SentEmailDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "approved":

                    var ListUpdate = vssp_db.Tbl_TRS_ForecastOrderApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == ApprovalId);

                    ListUpdate.Approved = true;
                    ListUpdate.ApprovedDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_TRS_ForecastOrderApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == ApprovalId);

                    vssp_db.Tbl_TRS_ForecastOrderApproval.Remove(ListDelete);

                    break;
            }

        }

        //public void crudMasterListKanbanDetail(string OrderNumber)
        //{
        //    var MLOK = vssp_db.SP_CRUD_MasterKanbanListDetail(OrderNumber);
        //}
        public void crudMasterListKanbanSupplier(string OrderNumber)
        {
            var MLOK = vssp_db.SP_CRUD_MasterKanbanListSupplier(OrderNumber);
        }
        public void crudMasterListKanbanStock(string OrderNumber)
        {
            var Stock = vssp_db.SP_CRUD_MasterKanbanListStock(OrderNumber);
        }

        public ActionResult ForecastApproval(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Part Requirement List Approval";

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
                    Vw_TRS_ForecastOrder ForecastOrder = vssp_db.Vw_TRS_ForecastOrder.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_TRS_ForecastOrderApproval approval = vssp_db.Tbl_TRS_ForecastOrderApproval.Where(a => a.OrderNumber == ordernumber && a.UserId == user.UserID).FirstOrDefault();

                    if (ForecastOrder != null && user != null && approval != null)
                    {

                        string orderdate = new DateTime(int.Parse(ForecastOrder.OrderYear), int.Parse(ForecastOrder.OrderMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.OrderTitle = "Part Requirement List";
                        ViewBag.OrderNumber = ForecastOrder.OrderNumber;
                        ViewBag.OrderDate = orderdate;
                        ViewBag.SupplierName = ForecastOrder.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        if (approval.Approved == false)
                        {
                            return View();
                        }
                        else
                        {
                            ViewBag.ApprovedDate = _SystemService.Vd(approval.ApprovedDate.ToString(), "dd MMMM, yyyy");
                            return View("ForecastApproved");
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

        public ActionResult ForecastSupplier(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Part Requirement List Approval";

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
                    Vw_TRS_ForecastOrder ForecastOrder = vssp_db.Vw_TRS_ForecastOrder.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    //UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_MST_SupplierContact approval = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == ForecastOrder.SupplierId && a.Email == uid).FirstOrDefault();

                    if (ForecastOrder != null && approval != null)
                    {

                        string orderdate = new DateTime(int.Parse(ForecastOrder.OrderYear), int.Parse(ForecastOrder.OrderMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.OrderTitle = "Part Requirement List";
                        ViewBag.OrderNumber = ForecastOrder.OrderNumber;
                        ViewBag.OrderDate = orderdate;
                        ViewBag.SupplierName = ForecastOrder.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = approval.ContactName;

                        return View();

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

        public ActionResult ForecastApproved(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Part Requirement List Approval";

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
                    Vw_TRS_ForecastOrder ForecastOrder = vssp_db.Vw_TRS_ForecastOrder.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();

                    if (ForecastOrder != null && user != null)
                    {

                        string orderdate = new DateTime(int.Parse(ForecastOrder.OrderYear), int.Parse(ForecastOrder.OrderMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.OrderTitle = "Part Requirement List";
                        ViewBag.OrderNumber = ForecastOrder.OrderNumber;
                        ViewBag.OrderDate = orderdate;
                        ViewBag.SupplierName = ForecastOrder.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        crudForecastOrderApproval(user.UserID, ForecastOrder.OrderNumber, user.UserID, "Approved");
                        return RedirectToAction("ContinuePage", "System", new { cmessage = "Successfuly Approved " + ViewBag.OrderTitle + " \n " + ordernumber, caction = "Dashboard", ccontroller = "Home", capps = "Home" });

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Part Requirement List";
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

        public ActionResult SupplierOrder()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();

                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Purchase", "SupplierOrder");

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
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;
                    ViewBag.DateTime = DateTime.Now;

                    SupplierOrderListModel SupplierOrder = new SupplierOrderListModel();
                    SupplierOrder.ExportList = _SystemService.ComboExport().ToList();
                    SupplierOrder.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                                orderby a.Id
                                                select a).ToList();

                    Session["Layout"] = "portal";
                    return View(SupplierOrder);

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult SupplierOrderListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_SupplierOrder> SupplierOrder = (from a in vssp_db.Vw_TRS_SupplierOrder
                                                        where a.OrderNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter)
                                                        orderby a.OrderDate descending, a.EditDate descending, a.OrderNumber
                                                        select a).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                SupplierOrder = SupplierOrder.Where(a => a.OrderDate >= startdate && a.OrderDate <= enddate).ToList();
            }
            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string ordermonth = arrs[0];
                string orderyears = arrs[1];
                SupplierOrder = SupplierOrder.Where(a => Convert.ToDateTime(a.OrderDate).ToString("MM") == ordermonth && Convert.ToDateTime(a.OrderDate).ToString("yyyy") == orderyears).ToList();
            }
            if (status != 99)
            {
                SupplierOrder = SupplierOrder.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in SupplierOrder
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                SupplierOrder = SupplierOrder.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            return Json(SupplierOrder, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SupplierOrderDetailListJson(string ordernumber, string supplierid, string SSP, string formAction)
        {
            try
            {

                SSP = _SystemService.Vf(SSP);

                switch (formAction)
                {
                    case "Create":
                        var SupplierOrderDetailTemp = (from a in vssp_db.Vw_TRS_Stock
                                                       where a.SSP == SSP && a.SupplierId == supplierid && (a.MaxStock - a.TotalStockKanban) > 0
                                                       select new
                                                       {
                                                           OrderNumber = ordernumber,
                                                           a.SupplierId,
                                                           a.UniqueNumber,
                                                           a.PartNumber,
                                                           a.PartName,
                                                           a.SSP,
                                                           a.UnitQty,
                                                           a.UnitLevel1,
                                                           a.UnitLevel2,
                                                           a.PackingId,
                                                           a.PartModel,
                                                           a.MaxStock,
                                                           a.StockKanban,
                                                           a.OutstandingKanban,
                                                           a.OutstandingQty,
                                                           OrderQty = (Math.Ceiling((double)(a.MaxStock - a.TotalStockKanban))),
                                                           OrderUnitQty = (Math.Ceiling((double)(a.MaxStock - a.TotalStockKanban)) * a.UnitQty),
                                                           ReceiveQty = 0
                                                       }).ToList();

                        return Json(SupplierOrderDetailTemp, JsonRequestBehavior.AllowGet);

                    default:
                        var SupplierOrderDetail = (from a in vssp_db.Vw_TRS_SupplierOrderDetail
                                                   join b in vssp_db.Vw_TRS_Stock on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                                   from b in part.DefaultIfEmpty()
                                                   where a.OrderNumber == ordernumber
                                                   select new
                                                   {
                                                       a.OrderNumber,
                                                       a.SupplierId,
                                                       b.UniqueNumber,
                                                       a.PartNumber,
                                                       b.PartName,
                                                       b.UnitQty,
                                                       b.UnitLevel1,
                                                       b.UnitLevel2,
                                                       b.PackingId,
                                                       b.PartModel,
                                                       a.MaxStock,
                                                       a.StockKanban,
                                                       a.OrderQty,
                                                       b.OutstandingKanban,
                                                       b.OutstandingQty,
                                                       a.OrderUnitQty,
                                                       a.ReceiveQty
                                                   }).ToList();

                        if (_SystemService.Vf(supplierid) != "")
                        {
                            SupplierOrderDetail = SupplierOrderDetail.Where(a => a.SupplierId == supplierid).ToList();
                        }

                        return Json(SupplierOrderDetail, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SupplierOrderIncomingDateTimeJson(DateTime orderdate, string supplierid, int shift)
        {
            try
            {
                if (Session["UserID"] != null)
                {

                    string utype = Session["UserType"].ToString();
                    List<SP_GET_IncomingDateTime_Result> IncomingDateTime = vssp_db.SP_GET_IncomingDateTime(orderdate, DateTime.Now, supplierid, shift, false).ToList();

                    if (utype == "DEV" && IncomingDateTime[0].IncomingDate == null)
                    {
                        IncomingDateTime.Clear();

                        SP_GET_IncomingDateTime_Result _IncomingDateTime = new SP_GET_IncomingDateTime_Result();
                        _IncomingDateTime.IncomingDate = DateTime.Now;
                        _IncomingDateTime.IncomingTime = DateTime.Now;
                        _IncomingDateTime.Cycle1 = 0;
                        _IncomingDateTime.Cycle2 = 0;
                        _IncomingDateTime.Cycle3 = 0;

                        IncomingDateTime.Add(_IncomingDateTime);
                    }


                    return Json(IncomingDateTime, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    Session["History"] = HttpContext.Request.Url.AbsolutePath;
                    return RedirectToAction("Login", "Account");
                }


            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudSupplierOrderList(string jsonData)
        {

            try
            {
                string incomingtime = "";
                vssp_db.Database.CommandTimeout = 0;

                PostSupplierOrderModel postSupplierOrder = JsonConvert.DeserializeObject<PostSupplierOrderModel>(jsonData);
                Tbl_TRS_SupplierOrder SupplierOrder = postSupplierOrder.SupplierOrder;
                List<crud_SupplierOrderDetail> SupplierOrderDetail = postSupplierOrder.SupplierOrderDetail;

                string uid = postSupplierOrder.uid;
                string formAction = postSupplierOrder.formAction.ToLower();
                string returnCodes = "";
                incomingtime = _SystemService.Vd(SupplierOrder.IncomingDate.ToString(), "yyyy-MM-dd") + " " + _SystemService.Vd(SupplierOrder.IncomingTime.ToString(), "HH:mm");
                SupplierOrder.IncomingTime = Convert.ToDateTime(incomingtime);

                var sspprocess = vssp_db.Tbl_MST_SpecialSupplyPart.Where(a => a.Id == SupplierOrder.SSP).FirstOrDefault();
                bool SSPDelivery = false;
                if (sspprocess != null)
                {
                    SSPDelivery = _SystemService.Vb(sspprocess.DeliveryOrder.ToString());
                }

                switch (formAction)
                {
                    case "create":

                        /* Get New Order Number */
                        string CompId = Session["CompID"].ToString();
                        var OrderNumber = vssp_db.SP_GET_SupplierOrderNumber(SupplierOrder.SupplierId, SupplierOrder.OrderDate, CompId);
                        foreach (SP_GET_SupplierOrderNumber_Result number in OrderNumber)
                        {
                            SupplierOrder.OrderNumber = number.OrderNumber;
                        }

                        //Tbl_TRS_SupplierOrder ListSupplierOrder = new Tbl_TRS_SupplierOrder();
                        //ListSupplierOrder.OrderNumber = SupplierOrder.OrderNumber;
                        //ListSupplierOrder.OrderDate = SupplierOrder.OrderDate;
                        //ListSupplierOrder.SupplierId = SupplierOrder.SupplierId;
                        //ListSupplierOrder.SSP = SupplierOrder.SSP;
                        //ListSupplierOrder.IncomingDate = SupplierOrder.IncomingDate;
                        //ListSupplierOrder.IncomingTime = SupplierOrder.IncomingTime;
                        //ListSupplierOrder.Shift = SupplierOrder.Shift;
                        //ListSupplierOrder.Remarks = SupplierOrder.Remarks;
                        //ListSupplierOrder.Status = 0;
                        //ListSupplierOrder.UserId = uid;
                        //ListSupplierOrder.EditDate = DateTime.Now;

                        //vssp_db.Tbl_TRS_SupplierOrder.Add(ListSupplierOrder);

                        ///* crud Details */
                        //crudSupplierOrderDetail(SupplierOrderDetail, SupplierOrder.OrderNumber, SupplierOrder.SupplierId, SupplierOrder.SSP, formAction);
                        returnCodes = crudSupplierOrderDetailNew(
                                    SupplierOrderDetail,
                                    SupplierOrder,
                                    uid,
                                    formAction);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_SupplierOrder.First(a => a.OrderNumber == SupplierOrder.OrderNumber);

                        ListUpdate.OrderDate = SupplierOrder.OrderDate;
                        ListUpdate.SSP = SupplierOrder.SSP;
                        ListUpdate.IncomingDate = SupplierOrder.IncomingDate;
                        ListUpdate.IncomingTime = SupplierOrder.IncomingTime;
                        ListUpdate.SupplierId = SupplierOrder.SupplierId;
                        ListUpdate.Remarks = SupplierOrder.Remarks;
                        ListUpdate.Shift = SupplierOrder.Shift;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Details */
                        //crudSupplierOrderDetail(SupplierOrderDetail, SupplierOrder.OrderNumber, SupplierOrder.SupplierId, SupplierOrder.SSP, formAction);
                        returnCodes = crudSupplierOrderDetailNew(
                                   SupplierOrderDetail,
                                   SupplierOrder,
                                   uid,
                                   formAction);

                        break;
                    case "closed":

                        var ListClosed = vssp_db.Tbl_TRS_SupplierOrder.First(a => a.OrderNumber == SupplierOrder.OrderNumber);

                        ListClosed.Status = 3;

                        returnCodes = "00";

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_TRS_SupplierOrder.First(a => a.OrderNumber == SupplierOrder.OrderNumber);

                        ListCanceled.Status = 4;

                        /* crud stock ssp */
                        //crudSupplierOrderDetail(SupplierOrderDetail, SupplierOrder.OrderNumber, SupplierOrder.SupplierId, SupplierOrder.SSP, "delete");
                        returnCodes = crudSupplierOrderDetailNew(
                                    SupplierOrderDetail,
                                    SupplierOrder,
                                    uid,
                                     "delete");

                        break;

                    case "delete":

                        /* remove existing SupplierOrder */
                        var ListDelete = vssp_db.Tbl_TRS_SupplierOrder.First(a => a.OrderNumber == SupplierOrder.OrderNumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        /* crud stock ssp */
                        //crudSupplierOrderDetail(SupplierOrderDetail, SupplierOrder.OrderNumber, SupplierOrder.SupplierId, SupplierOrder.SSP, formAction);

                        returnCodes = crudSupplierOrderDetailNew(
                                   SupplierOrderDetail,
                                   SupplierOrder,
                                   uid,
                                   formAction);

                        //vssp_db.Tbl_TRS_SupplierOrder.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                        break;
                }

                try
                {
                    //vssp_db.SaveChanges();

                    //if (formAction == "create" || formAction == "update")
                    //{
                    //    var Kanban = vssp_db.SP_CRUD_GenerateKanban(SupplierOrder.OrderNumber);
                    //}
                    //if (formAction != "closed")
                    //{
                    //    if (SupplierOrder.SSP != "")
                    //    {
                    //        vssp_db.SP_CRUD_StockTransactionSupplierOrderSSP(SupplierOrder.OrderNumber, uid, formAction);
                    //    }
                    //}

                    //return Json(SupplierOrder, JsonRequestBehavior.AllowGet);
                    if (returnCodes == "00")
                    {
                        vssp_db.SaveChanges();

                        if (formAction == "create" || formAction == "update")
                        {
                            var Kanban = vssp_db.SP_CRUD_GenerateKanban(SupplierOrder.OrderNumber);
                        }
                        if (formAction != "closed")
                        {
                            if (SupplierOrder.SSP != "")
                            {
                                vssp_db.SP_CRUD_StockTransactionSupplierOrderSSP(SupplierOrder.OrderNumber, uid, formAction);
                            }
                        }
                        return Json(new { err = returnCodes , data = SupplierOrder }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { err = returnCodes, data = SupplierOrder }, JsonRequestBehavior.AllowGet);
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

        public void crudSupplierOrderDetail(List<crud_SupplierOrderDetail> SupplierOrderDetails, string OrderNumber, string SupplierId, string SSP, string formAction)
        {

            foreach (var Details in SupplierOrderDetails)
            {

                //var sspprocess = (from a in vssp_db.Vw_TRS_Stock
                //                  where a.SupplierId == SupplierId && a.PartNumber == Details.PartNumber
                //                  select new { a.SupplierId, a.PartNumber, a.SSPStock, a.DeliveryOrder }).FirstOrDefault();

                //string SSPSupplier = "";
                //string SSPPartNumber = "";
                //bool SSPStock = false;
                //bool SSPDelivery = false;
                //double kbnqty = 0;
                //double delqty = 0;
                //double lastkbnqty = 0;
                //double lastdelqty = 0;

                //if (sspprocess != null)
                //{
                //    SSPSupplier = _SystemService.Vf(sspprocess.SupplierId.ToString());
                //    SSPPartNumber = _SystemService.Vf(sspprocess.PartNumber.ToString());
                //    SSPStock = _SystemService.Vb(sspprocess.SSPStock.ToString());
                //    SSPDelivery = _SystemService.Vb(sspprocess.DeliveryOrder.ToString());
                //}

                if (Details.RowStatus == null)
                {
                    Details.RowStatus = formAction;
                }
                if (Details.RowStatus != null)
                {
                    switch (Details.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_SupplierOrderDetail ListDetail = new Tbl_TRS_SupplierOrderDetail();
                            ListDetail.OrderNumber = OrderNumber;
                            ListDetail.SupplierId = SupplierId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.MaxStock = Details.MaxStock;
                            ListDetail.StockKanban = Details.StockKanban;
                            ListDetail.OrderQty = Details.OrderQty;
                            ListDetail.OrderUnitQty = Details.OrderUnitQty;
                            ListDetail.ReceiveQty = 0;

                            vssp_db.Tbl_TRS_SupplierOrderDetail.Add(ListDetail);

                            //if (SSPDelivery == true)
                            //{
                            //    var sspstock = (from a in vssp_db.Vw_TRS_Stock
                            //                    where a.PartNumber == Details.PartNumber && a.SSPStock == true
                            //                    select new { a.SupplierId, a.PartNumber, a.SSPStock, a.DeliveryOrder }).FirstOrDefault();

                            //    kbnqty = 0 - (_SystemService.Vn(ListDetail.OrderQty.ToString()));
                            //    delqty = 0 - (_SystemService.Vn(ListDetail.OrderUnitQty.ToString()));
                            //    if (sspstock != null) crudStockSSP(sspstock.SupplierId, sspstock.PartNumber, kbnqty, delqty, "Add", ListDetail.OrderNumber, false, false);
                            //}
                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_SupplierOrderDetail.First(a => a.OrderNumber == OrderNumber && a.SupplierId == SupplierId && a.PartNumber == Details.PartNumber);

                            //lastkbnqty = _SystemService.Vn(ListUpdate.OrderQty.ToString());
                            //lastdelqty = _SystemService.Vn(ListUpdate.OrderUnitQty.ToString());

                            ListUpdate.MaxStock = Details.MaxStock;
                            ListUpdate.StockKanban = Details.StockKanban;
                            ListUpdate.OrderQty = Details.OrderQty;
                            ListUpdate.OrderUnitQty = Details.OrderUnitQty;

                            //if (SSPDelivery == true)
                            //{
                            //    var sspstock = (from a in vssp_db.Vw_TRS_Stock
                            //                    where a.PartNumber == Details.PartNumber && a.SSPStock == true
                            //                    select new { a.SupplierId, a.PartNumber, a.SSPStock, a.DeliveryOrder }).FirstOrDefault();

                            //    kbnqty = lastkbnqty + (0 - _SystemService.Vn(ListUpdate.OrderQty.ToString()));
                            //    delqty = lastdelqty + (0 - _SystemService.Vn(ListUpdate.OrderUnitQty.ToString()));
                            //    if (sspstock != null) crudStockSSP(sspstock.SupplierId, sspstock.PartNumber, kbnqty, delqty, "Add", ListUpdate.OrderNumber, false, false);
                            //}
                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_SupplierOrderDetail.First(a => a.OrderNumber == OrderNumber && a.SupplierId == SupplierId && a.PartNumber == Details.PartNumber);

                            //if (SSPDelivery == true)
                            //{
                            //    var sspstock = (from a in vssp_db.Vw_TRS_Stock
                            //                    where a.PartNumber == Details.PartNumber && a.SSPStock == true
                            //                    select new { a.SupplierId, a.PartNumber, a.SSPStock, a.DeliveryOrder }).FirstOrDefault();

                            //    kbnqty = _SystemService.Vn(ListDelete.OrderQty.ToString());
                            //    delqty = _SystemService.Vn(ListDelete.OrderUnitQty.ToString());
                            //    if (sspstock != null) crudStockSSP(sspstock.SupplierId, sspstock.PartNumber, kbnqty, delqty, "Add", ListDelete.OrderNumber, false, false);
                            //}

                            if (ListDelete != null)
                            {
                                vssp_db.Tbl_TRS_SupplierOrderDetail.Remove(ListDelete);
                            }
                            break;
                    }
                }
            }

        }

        // UPDATE CRUD DETAIL SUPPLIER ORDER BY DASEP 
        public string crudSupplierOrderDetailNew(List<crud_SupplierOrderDetail> SupplierOrderDetails, Tbl_TRS_SupplierOrder SupplierOrder, string uid, string formAction)
        {
            using (var db = new vssp_entity()) // Ganti dengan nama EDMX Anda
            {
                // Konversi List ke DataTable
                DataTable dt = new DataTable();
                dt.Columns.Add("OrderNumber", typeof(string));
                dt.Columns.Add("SupplierId", typeof(string));
                dt.Columns.Add("PartNumber", typeof(string));
                dt.Columns.Add("MaxStock", typeof(int));
                dt.Columns.Add("StockKanban", typeof(int));
                dt.Columns.Add("OrderQty", typeof(int));
                dt.Columns.Add("OrderUnitQty", typeof(int));
                dt.Columns.Add("RowStatus", typeof(string));

                foreach (var detail in SupplierOrderDetails)
                {
                    if (detail.RowStatus == null)
                    {
                        detail.RowStatus = formAction;
                    }
                    dt.Rows.Add(SupplierOrder.OrderNumber, SupplierOrder.SupplierId, detail.PartNumber,
                                detail.MaxStock, detail.StockKanban, detail.OrderQty,
                                detail.OrderUnitQty, detail.RowStatus?.ToLower());
                }

                // Buat parameter untuk DataTable
                var orderDetailsParam = new SqlParameter("@OrderDetails", SqlDbType.Structured)
                {
                    TypeName = "SupplierOrderDetailType",
                    Value = dt
                };

                // Tambahkan parameter output untuk return code
                var returnCodeParam = new SqlParameter("@ReturnCode", SqlDbType.NVarChar, 2)
                {
                    Direction = ParameterDirection.Output
                };
                var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };

                // Eksekusi Stored Procedure menggunakan EDMX
                db.Database.ExecuteSqlCommand("EXEC SP_CrudSupplierOrderDetailNew @OrderDetails, @ReturnCode OUTPUT,@ErrorMessage OUTPUT , @OrderNumber,@OrderDate,@SupplierId,@SSP,@IncomingDate,@IncomingTime,@Shift,@Remarks,@UserId",
                                       orderDetailsParam,
                                       returnCodeParam,
                                       errorMessageParam,
                                       new SqlParameter("@OrderNumber", SupplierOrder.OrderNumber),
                                       new SqlParameter("@OrderDate", SupplierOrder.OrderDate),
                                       new SqlParameter("@SupplierId", SupplierOrder.SupplierId),
                                       new SqlParameter("@SSP", SupplierOrder.SSP),
                                       new SqlParameter("@IncomingDate", SupplierOrder.IncomingDate),
                                       new SqlParameter("@IncomingTime", SupplierOrder.IncomingTime),
                                       new SqlParameter("@Shift", SupplierOrder.Shift),
                                       new SqlParameter("@Remarks", SupplierOrder.Remarks),
                                       new SqlParameter("@UserId", uid)
                                       );

                // Ambil return code dari SP
                if (returnCodeParam.Value.ToString() != "00") return errorMessageParam.Value.ToString();


                return returnCodeParam.Value.ToString();
                //return SupplierOrder.OrderNumber + "@" + formAction + "@" + SupplierOrder.OrderDate + "@" + SupplierOrder.SupplierId + "@" + SupplierOrder.IncomingDate + "@" + SupplierOrder.IncomingTime;
            }
        }

        // 

        public ActionResult GetOnOrderStock(string ordernumber, string supplierid, string partnumber)
        {
            if (_SystemService.Vf(ordernumber) == "")
            {
                ordernumber = "*";
            }
            var OnOrderStock = vssp_db.SP_GET_OnOrderStock(ordernumber, supplierid, partnumber);

            return Json(OnOrderStock, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SupplierOrderMail(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Delivery Note & Kanban Order";

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
                    Vw_TRS_SupplierOrder SupplierOrder = vssp_db.Vw_TRS_SupplierOrder.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    //UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_MST_SupplierContact approval = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == SupplierOrder.SupplierId && a.Email == uid).FirstOrDefault();

                    if (SupplierOrder != null && approval != null)
                    {

                        string orderdate = _SystemService.Vd(SupplierOrder.OrderDate.ToString(), "MMMM dd, yyyy");

                        ViewBag.OrderTitle = "Delivery Note & Kanban Order";
                        ViewBag.OrderNumber = SupplierOrder.OrderNumber;
                        ViewBag.OrderDate = orderdate;
                        ViewBag.SupplierName = SupplierOrder.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = approval.ContactName;

                        return View();

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Delivery Note & Kanban Order";
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

        public ActionResult ReturnPart()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();

                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Purchase", "ReturnPart");

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
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;
                    ViewBag.DateTime = DateTime.Now;

                    ReturnPartListModel ReturnPart = new ReturnPartListModel();
                    ReturnPart.ExportList = _SystemService.ComboExport().ToList();
                    ReturnPart.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                             orderby a.Id
                                             select a).ToList();

                    Session["Layout"] = "portal";
                    return View(ReturnPart);

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult ReturnPartListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_ReturnPart> ReturnPart = (from a in vssp_db.Vw_TRS_ReturnPart
                                                  where a.ReturnNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter)
                                                  orderby a.ReturnDate descending, a.EditDate descending, a.ReturnNumber
                                                  select a).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                ReturnPart = ReturnPart.Where(a => a.ReturnDate >= startdate && a.ReturnDate <= enddate).ToList();
            }
            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string ordermonth = arrs[0];
                string orderyears = arrs[1];
                ReturnPart = ReturnPart.Where(a => Convert.ToDateTime(a.ReturnDate).ToString("MM") == ordermonth && Convert.ToDateTime(a.ReturnDate).ToString("yyyy") == orderyears).ToList();
            }
            if (status != 99)
            {
                ReturnPart = ReturnPart.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in ReturnPart
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                ReturnPart = ReturnPart.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            return Json(ReturnPart, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ReturnPartDetailListJson(string returnnumber, string supplierid, string formAction)
        {
            try
            {


                switch (formAction)
                {
                    case "Create":
                        var ReturnPartDetailTemp = (from a in vssp_db.Vw_TRS_StockRawReturn
                                                    where a.SupplierId == supplierid && a.StockQty > 0
                                                    select new
                                                    {
                                                        ReturnNumber = returnnumber,
                                                        a.SupplierId,
                                                        a.UniqueNumber,
                                                        a.PartNumber,
                                                        a.PartName,
                                                        a.UnitQty,
                                                        a.UnitLevel1,
                                                        a.UnitLevel2,
                                                        a.PackingId,
                                                        a.PartModel,
                                                        a.StockQty,
                                                        a.OutstandingQty,
                                                        ReturnUnitQty = a.StockQty,
                                                        ReceiveQty = 0
                                                    }).ToList();

                        return Json(ReturnPartDetailTemp, JsonRequestBehavior.AllowGet);

                    default:
                        var ReturnPartDetail = (from a in vssp_db.Vw_TRS_ReturnPartDetail
                                                join b in vssp_db.Vw_TRS_StockRawReturn on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                                from b in part.DefaultIfEmpty()
                                                where a.ReturnNumber == returnnumber
                                                select new
                                                {
                                                    a.ReturnNumber,
                                                    a.SupplierId,
                                                    b.UniqueNumber,
                                                    a.PartNumber,
                                                    b.PartName,
                                                    b.UnitQty,
                                                    b.UnitLevel1,
                                                    b.UnitLevel2,
                                                    b.PackingId,
                                                    b.PartModel,
                                                    b.StockQty,
                                                    b.OutstandingQty,
                                                    a.ReturnUnitQty,
                                                    a.ReceiveQty
                                                }).ToList();

                        if (_SystemService.Vf(supplierid) != "")
                        {
                            ReturnPartDetail = ReturnPartDetail.Where(a => a.SupplierId == supplierid).ToList();
                        }

                        return Json(ReturnPartDetail, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudReturnPartList(string jsonData)
        {

            try
            {
                string incomingtime = "";
                vssp_db.Database.CommandTimeout = 0;

                PostReturnPartModel postReturnPart = JsonConvert.DeserializeObject<PostReturnPartModel>(jsonData);
                Tbl_TRS_ReturnPart ReturnPart = postReturnPart.ReturnPart;
                List<crud_ReturnPartDetail> ReturnPartDetail = postReturnPart.ReturnPartDetail;

                string uid = postReturnPart.uid;
                string formAction = postReturnPart.formAction.ToLower();

                incomingtime = _SystemService.Vd(ReturnPart.IncomingDate.ToString(), "yyyy-MM-dd") + " " + _SystemService.Vd(ReturnPart.IncomingTime.ToString(), "HH:mm");
                ReturnPart.IncomingTime = Convert.ToDateTime(incomingtime);

                switch (formAction)
                {
                    case "create":

                        /* Get New Order Number */
                        string CompId = Session["CompID"].ToString();
                        var ReturnNumber = vssp_db.SP_GET_ReturnPartNumber(ReturnPart.SupplierId, ReturnPart.ReturnDate, CompId);
                        foreach (SP_GET_ReturnPartNumber_Result number in ReturnNumber)
                        {
                            ReturnPart.ReturnNumber = number.ReturnNumber;
                        }

                        Tbl_TRS_ReturnPart ListReturnPart = new Tbl_TRS_ReturnPart();
                        ListReturnPart.ReturnNumber = ReturnPart.ReturnNumber;
                        ListReturnPart.ReturnDate = ReturnPart.ReturnDate;
                        ListReturnPart.SupplierId = ReturnPart.SupplierId;
                        ListReturnPart.IncomingDate = ReturnPart.IncomingDate;
                        ListReturnPart.IncomingTime = ReturnPart.IncomingTime;
                        ListReturnPart.Remarks = ReturnPart.Remarks;
                        ListReturnPart.Status = 0;
                        ListReturnPart.UserId = uid;
                        ListReturnPart.EditDate = DateTime.Now;

                        vssp_db.Tbl_TRS_ReturnPart.Add(ListReturnPart);

                        /* crud Details */
                        crudReturnPartDetail(ReturnPartDetail, ReturnPart.ReturnNumber, ReturnPart.SupplierId, formAction);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_ReturnPart.First(a => a.ReturnNumber == ReturnPart.ReturnNumber);

                        ListUpdate.ReturnDate = ReturnPart.ReturnDate;
                        ListUpdate.IncomingDate = ReturnPart.IncomingDate;
                        ListUpdate.IncomingTime = ReturnPart.IncomingTime;
                        ListUpdate.SupplierId = ReturnPart.SupplierId;
                        ListUpdate.Remarks = ReturnPart.Remarks;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Details */
                        crudReturnPartDetail(ReturnPartDetail, ReturnPart.ReturnNumber, ReturnPart.SupplierId, formAction);

                        break;
                    case "closed":

                        var ListClosed = vssp_db.Tbl_TRS_ReturnPart.First(a => a.ReturnNumber == ReturnPart.ReturnNumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_TRS_ReturnPart.First(a => a.ReturnNumber == ReturnPart.ReturnNumber);

                        ListCanceled.Status = 4;

                        /* crud details */
                        crudReturnPartDetail(ReturnPartDetail, ReturnPart.ReturnNumber, ReturnPart.SupplierId, "delete");

                        break;

                    case "delete":

                        /* remove existing ReturnPart */
                        var ListDelete = vssp_db.Tbl_TRS_ReturnPart.First(a => a.ReturnNumber == ReturnPart.ReturnNumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        /* crud details */
                        crudReturnPartDetail(ReturnPartDetail, ReturnPart.ReturnNumber, ReturnPart.SupplierId, formAction);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();

                    if (formAction == "create" || formAction == "update")
                    {
                        var Kanban = vssp_db.SP_CRUD_GenerateKanban(ReturnPart.ReturnNumber);
                    }

                    return Json(ReturnPart, JsonRequestBehavior.AllowGet);
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

        public void crudReturnPartDetail(List<crud_ReturnPartDetail> ReturnPartDetails, string ReturnNumber, string SupplierId, string formAction)
        {

            foreach (var Details in ReturnPartDetails)
            {

                if (Details.RowStatus == null)
                {
                    Details.RowStatus = formAction;
                }
                if (Details.RowStatus != null)
                {

                    double lastqty = 0;

                    switch (Details.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_ReturnPartDetail ListDetail = new Tbl_TRS_ReturnPartDetail();
                            ListDetail.ReturnNumber = ReturnNumber;
                            ListDetail.SupplierId = SupplierId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.ReturnUnitQty = Details.ReturnUnitQty;
                            ListDetail.ReceiveQty = 0;

                            vssp_db.Tbl_TRS_ReturnPartDetail.Add(ListDetail);

                            /* crud stock return part */
                            crudStockRawReturn(ListDetail.SupplierId, ListDetail.PartNumber, _SystemService.Vn(ListDetail.ReturnUnitQty.ToString()), lastqty, "delete", "", false, false);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_ReturnPartDetail.First(a => a.ReturnNumber == ReturnNumber && a.SupplierId == SupplierId && a.PartNumber == Details.PartNumber);
                            lastqty = _SystemService.Vn(ListUpdate.ReturnUnitQty.ToString());
                            ListUpdate.ReturnUnitQty = Details.ReturnUnitQty;

                            /* crud stock return part */
                            crudStockRawReturn(ListUpdate.SupplierId, ListUpdate.PartNumber, lastqty, _SystemService.Vn(ListUpdate.ReturnUnitQty.ToString()), "update", "", false, false);

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_ReturnPartDetail.First(a => a.ReturnNumber == ReturnNumber && a.SupplierId == SupplierId && a.PartNumber == Details.PartNumber);

                            if (ListDelete != null)
                            {
                                /* crud stock return part */
                                crudStockRawReturn(ListDelete.SupplierId, ListDelete.PartNumber, _SystemService.Vn(ListDelete.ReturnUnitQty.ToString()), lastqty, "create", "", false, false);
                                vssp_db.Tbl_TRS_ReturnPartDetail.Remove(ListDelete);
                            }
                            break;
                    }
                }
            }

        }
        public ActionResult ReturnPartMail(string returnnumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Delivery Return Part Order";

            try
            {

                if (returnnumber == null || uid == null)
                {
                    returnnumber = Session["returnnumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["returnnumber"] = returnnumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_TRS_ReturnPart ReturnPart = vssp_db.Vw_TRS_ReturnPart.Where(a => a.ReturnNumber == returnnumber).FirstOrDefault();
                    //UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_MST_SupplierContact approval = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == ReturnPart.SupplierId && a.Email == uid).FirstOrDefault();

                    if (ReturnPart != null && approval != null)
                    {

                        string orderdate = _SystemService.Vd(ReturnPart.ReturnDate.ToString(), "MMMM dd, yyyy");

                        ViewBag.OrderTitle = "Delivery Return Part Order";
                        ViewBag.ReturnNumber = ReturnPart.ReturnNumber;
                        ViewBag.ReturnDate = orderdate;
                        ViewBag.SupplierName = ReturnPart.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = approval.ContactName;

                        return View();

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Delivery Return Part Order";
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
        public ActionResult ReceivingOrder()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Purchase", "ReceivingOrder");

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
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    ReceivingOrderListModel ReceivingOrder = new ReceivingOrderListModel();
                    ReceivingOrder.ExportList = _SystemService.ComboExport().ToList();
                    ReceivingOrder.StatusList = (from a in vssp_db.Tbl_TRS_Status
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

                        return View(ReceivingOrder);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        //public ActionResult ReceivingOrderListJson(
        //                            string searchFilter,
        //                            Nullable<DateTime> startdate = null,
        //                            Nullable<DateTime> enddate = null,
        //                            string month = null,
        //                            bool returnpart = false,
        //                            int status = 99)
        //{
        //    searchFilter = _SystemService.Vf(searchFilter);
        //    vssp_db.Database.CommandTimeout = 0;
        //    //List<Vw_TRS_ReceivingOrder> ReceivingOrder = (from a in vssp_db.Vw_TRS_ReceivingOrder
        //    //                                            where a.ReceiveNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter)
        //    //                                            orderby a.ReceiveDate descending, a.ReceiveNumber
        //    //                                            select a).ToList();
        //    List<Vw_TRS_ReceivingOrder> ReceivingOrder = new List<Vw_TRS_ReceivingOrder>();

        //    string ordermonth = "";
        //    string orderyears = "";

        //    if (_SystemService.Vf(month) != "")
        //    {
        //        string[] arrs = month.Split('/');
        //        ordermonth = arrs[0];
        //        orderyears = arrs[1];
        //        //ReceivingOrder = ReceivingOrder.Where(a => Convert.ToDateTime(a.ReceiveDate).ToString("MM") == ordermonth && Convert.ToDateTime(a.ReceiveDate).ToString("yyyy") == orderyears).ToList();

        //        ReceivingOrder = (from a in vssp_db.Vw_TRS_ReceivingOrder
        //                          where a.ReceiveYear == orderyears && a.ReceiveMonth == ordermonth &&
        //                          (a.ReceiveNumber.Contains(searchFilter) || a.OrderNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter))
        //                          orderby a.ReceiveDate descending, a.ReceiveNumber
        //                          select a).ToList();
        //    }
        //    else
        //    {
        //        ReceivingOrder = (from a in vssp_db.Vw_TRS_ReceivingOrder
        //                          where (a.ReceiveNumber.Contains(searchFilter) || a.OrderNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter))
        //                          orderby a.ReceiveDate descending, a.ReceiveNumber
        //                          select a).ToList();
        //    }
        //    if (startdate != null)
        //    {
        //        if (enddate == null) enddate = startdate;
        //        ReceivingOrder = ReceivingOrder.Where(a => a.ReceiveDate >= startdate && a.ReceiveDate <= enddate).ToList();
        //    }
        //    //if (_SystemService.Vf(month) != "")
        //    //{
        //    //    string[] arrs = month.Split('/');
        //    //    string ordermonth = arrs[0];
        //    //    string orderyears = arrs[1];
        //    //    ReceivingOrder = ReceivingOrder.Where(a => Convert.ToDateTime(a.ReceiveDate).ToString("MM") == ordermonth && Convert.ToDateTime(a.ReceiveDate).ToString("yyyy") == orderyears).ToList();
        //    //}
        //    if (status != 99)
        //    {
        //        ReceivingOrder = ReceivingOrder.Where(a => a.Status.ToString() == status.ToString()).ToList();
        //    }
        //    else
        //    {
        //        var notinStatus = from a in ReceivingOrder
        //                          where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
        //                          select a.Status;
        //        ReceivingOrder = ReceivingOrder.Where(a => !notinStatus.Contains(a.Status)).ToList();
        //    }

        //    ReceivingOrder = ReceivingOrder.Where(a => a.ReturnPart == returnpart).ToList();

        //    return Json(ReceivingOrder, JsonRequestBehavior.AllowGet);

        //}

        public ActionResult ReceivingOrderListJson(
            string searchFilter,
            Nullable<DateTime> startdate = null,
            Nullable<DateTime> enddate = null,
            string month = null,
            bool returnpart = false,
            int status = 99,
            int page = 1,
            int rows = 20)
        {

            using (var dbConection = new vssp_entity())
            {


                searchFilter = _SystemService.Vf(searchFilter);
                vssp_db.Database.CommandTimeout = 0;
                List<Vw_TRS_ReceivingOrder> ReceivingOrder = new List<Vw_TRS_ReceivingOrder>();

                // Apply filters here (same as before)


                string ordermonth = "";
                string orderyears = "";

                if (_SystemService.Vf(month) != "")
                {
                    string[] arrs = month.Split('/');
                    ordermonth = arrs[0];
                    orderyears = arrs[1];

                    ReceivingOrder = (from a in dbConection.Vw_TRS_ReceivingOrder
                                      where a.ReceiveYear == orderyears && a.ReceiveMonth == ordermonth &&
                                      (a.ReceiveNumber.Contains(searchFilter) || a.OrderNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter))
                                      orderby a.ReceiveDate descending, a.ReceiveNumber
                                      select a).ToList();
                }
                else
                {
                    ReceivingOrder = (from a in dbConection.Vw_TRS_ReceivingOrder
                                      where (a.ReceiveNumber.Contains(searchFilter) || a.OrderNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter))
                                      orderby a.ReceiveDate descending, a.ReceiveNumber
                                      select a).ToList();
                }
                if (startdate != null)
                {
                    if (enddate == null) enddate = startdate;
                    ReceivingOrder = ReceivingOrder.Where(a => a.ReceiveDate >= startdate && a.ReceiveDate <= enddate).ToList();
                }
                if (status != 99)
                {
                    ReceivingOrder = ReceivingOrder.Where(a => a.Status.ToString() == status.ToString()).ToList();
                }
                else
                {
                    var notinStatus = from a in ReceivingOrder
                                      where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                      select a.Status;
                    ReceivingOrder = ReceivingOrder.Where(a => !notinStatus.Contains(a.Status)).ToList();
                }

                ReceivingOrder = ReceivingOrder.Where(a => a.ReturnPart == returnpart).ToList();
                // Pagination
                var totalRecords = ReceivingOrder.Count();
                var totalPages = (int)Math.Ceiling((float)totalRecords / rows);

                var data = ReceivingOrder.Skip((page - 1) * rows).Take(rows).ToList();

                return Json(new
                {
                    page = page,
                    total = totalPages,
                    records = totalRecords,
                    rows = data
                }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ReceivingOrderDetailListJson(string ReceiveNumber, string SupplierId)
        {
            try
            {
                vssp_db.Database.CommandTimeout = 0;

                var ReceivingOrderDetail = from a in vssp_db.Vw_TRS_ReceivingOrderDetail
                                           where a.ReceiveNumber == ReceiveNumber
                                           select new
                                           {
                                               a.ReceiveNumber,
                                               a.SupplierId,
                                               a.UniqueNumber,
                                               a.PartNumber,
                                               a.PartName,
                                               a.UnitQty,
                                               a.UnitLevel1,
                                               a.UnitLevel2,
                                               a.PackingId,
                                               a.PartModel,
                                               a.OrderQty,
                                               a.OrderUnitQty,
                                               a.ReceiveQty,
                                               a.ReceiveUnitQty,
                                               a.OutstandingQty,
                                               a.OutstandingUnitQty
                                           };

                //if (_SystemService.Vf(SupplierId) != "")
                //{
                //    ReceivingOrderDetail = ReceivingOrderDetail.Where(a => a.SupplierId == SupplierId);
                //}

                //return Json(ReceivingOrderDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(ReceivingOrderDetail, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ReceivingOrderDetailTempJson(string ReceiveNumber, string SupplierId)
        {
            try
            {
                vssp_db.Database.CommandTimeout = 0;

                if (_SystemService.Vf(ReceiveNumber) == "")
                {
                    ReceiveNumber = Session["UserId"].ToString();
                }

                var ReceivingOrderDetail = (from a in vssp_db.Vw_TRS_ReceivingOrderDetailTemp
                                            where a.ReceiveNumber == ReceiveNumber
                                            select new
                                            {
                                                a.ReceiveNumber,
                                                a.OrderNumber,
                                                a.SupplierId,
                                                a.UniqueNumber,
                                                a.PartNumber,
                                                a.PartName,
                                                a.UnitQty,
                                                a.Unit,
                                                a.PackingId,
                                                a.PartModel,
                                                a.OrderQty,
                                                a.OrderUnitQty,
                                                a.ReceiveQty,
                                                a.ReceiveUnitQty,
                                                a.OutstandingQty,
                                                a.OutstandingUnitQty
                                            }).ToList();


                var jsonResult = Json(ReceivingOrderDetail, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ReceivingOrderDetailKanbanJson(string ReceiveNumber, string UniqueNumber)
        {
            try
            {
                vssp_db.Database.CommandTimeout = 0;

                var ReceivingOrderDetail = from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanban
                                           where a.ReceiveNumber == ReceiveNumber
                                           select a;

                if (_SystemService.Vf(UniqueNumber) != "")
                {
                    ReceivingOrderDetail = ReceivingOrderDetail.Where(a => a.UniqueNumber == UniqueNumber);
                }

                return Json(ReceivingOrderDetail, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ReceivingOrderDetailTempKanbanJson(string ReceiveNumber, string PartNumber)
        {
            try
            {
                vssp_db.Database.CommandTimeout = 0;

                var ReceivingOrderDetail = from a in vssp_db.Vw_TRS_ReceivingOrderDetailKanbanTemp
                                           where a.ReceiveNumber == ReceiveNumber
                                           select new { a.ReceiveNumber, a.OrderNumber, a.SupplierId, a.KanbanKey, a.UniqueNumber, a.PartNumber, a.PartName, a.UnitQty, a.UnitLevel2, a.ScanTime };

                if (_SystemService.Vf(PartNumber) != "")
                {
                    ReceivingOrderDetail = ReceivingOrderDetail.Where(a => a.PartNumber == PartNumber);
                }

                return Json(ReceivingOrderDetail, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetSupplierOrderJson(string ordertype, string ordernumber)
        {
            vssp_db.Database.CommandTimeout = 0;

            var getSupplierOrderJson = vssp_db.SP_GET_SupplierOrderOpen(ordertype, ordernumber);

            return Json(getSupplierOrderJson, JsonRequestBehavior.AllowGet);

        }
        public ActionResult crudReceivingKanbanOrderJson(string receivenumber, string ordernumber, string KanbanKey)
        {
            try
            {
                vssp_db.Database.CommandTimeout = 0;

                var SupplierOrderKanban = (from a in vssp_db.Tbl_TRS_SupplierOrderKanban
                                           join b in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                           from b in part.DefaultIfEmpty()
                                           where a.OrderNumber == ordernumber && a.KanbanKey == KanbanKey
                                           select new { a.OrderNumber, a.SupplierId, a.PartNumber, b.UniqueNumber, a.KanbanKey, a.UnitQty }).FirstOrDefault();

                Tbl_TRS_ReceivingOrderDetailKanban ReceivingOrderKanban = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanban
                                                                           where a.KanbanKey == KanbanKey
                                                                           select a).FirstOrDefault();

                Tbl_TRS_ReceivingOrderDetailKanbanTemp ReceivingOrderKanbanTemp = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp
                                                                                   where a.KanbanKey == KanbanKey && a.ReceiveNumber == receivenumber
                                                                                   select a).FirstOrDefault();

                var crudMessage = "";
                if (SupplierOrderKanban == null)
                {
                    crudMessage = "Error! " + KanbanKey + "<br/>Kanban Card not found in Supplier Order : " + ordernumber + ".<br/>Please check carefully kanban you will scan.";
                }
                else
                if (ReceivingOrderKanban != null)
                {
                    crudMessage = "Error! " + KanbanKey + "<br/>Kanban already scan in " + ReceivingOrderKanban.ReceiveNumber + ". Please check carefully kanban you will scan.";
                }
                else
                if (ReceivingOrderKanbanTemp != null)
                {
                    crudMessage = "Error! " + KanbanKey + "<br/>Kanban already scan in this receiving. Please check carefully kanban you will scan.";
                }
                else
                {
                    Tbl_TRS_ReceivingOrderDetailKanbanTemp _ReceivingOrderDetailTemp = new Tbl_TRS_ReceivingOrderDetailKanbanTemp();
                    _ReceivingOrderDetailTemp.ReceiveNumber = receivenumber;
                    _ReceivingOrderDetailTemp.OrderNumber = SupplierOrderKanban.OrderNumber;
                    _ReceivingOrderDetailTemp.SupplierId = SupplierOrderKanban.SupplierId;
                    _ReceivingOrderDetailTemp.KanbanKey = SupplierOrderKanban.KanbanKey;
                    _ReceivingOrderDetailTemp.UniqueNumber = SupplierOrderKanban.UniqueNumber;
                    _ReceivingOrderDetailTemp.ReceiveUnitQty = SupplierOrderKanban.UnitQty;
                    _ReceivingOrderDetailTemp.ScanTime = DateTime.Now;

                    vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Add(_ReceivingOrderDetailTemp);

                    try
                    {
                        vssp_db.SaveChanges();
                        crudMessage = "Success! <br/>Kanban already saved successfuly.";
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        crudMessage = "Error!<br/>" + errinfo;
                    }

                }

                return Json(crudMessage, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult clearReceivingKanbanOrderJson(string receivenumber, string ordernumber, string formAction, bool returnpart = false)
        {
            try
            {

                try
                {

                    var deleteSupplierOrderKanban = vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Where(a => a.ReceiveNumber == receivenumber).ToList();
                    foreach (var delete in deleteSupplierOrderKanban)
                    {
                        vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Remove(delete);
                    }

                    switch (formAction)
                    {
                        case "Create":

                            if (_SystemService.Vf(ordernumber) != "")
                            {
                                if (returnpart)
                                {
                                    GetReceiveOrderReturnPartDetail(receivenumber, ordernumber);
                                }
                            }
                            break;
                        default:

                            var supplierorderdetail = vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Where(a => a.ReceiveNumber == receivenumber);
                            foreach (var kanban in supplierorderdetail)
                            {
                                Tbl_TRS_ReceivingOrderDetailKanbanTemp _ReceivingOrderDetailTemp = new Tbl_TRS_ReceivingOrderDetailKanbanTemp();
                                _ReceivingOrderDetailTemp.ReceiveNumber = kanban.ReceiveNumber;
                                _ReceivingOrderDetailTemp.OrderNumber = kanban.OrderNumber;
                                _ReceivingOrderDetailTemp.SupplierId = kanban.SupplierId;
                                _ReceivingOrderDetailTemp.KanbanKey = kanban.KanbanKey;
                                _ReceivingOrderDetailTemp.UniqueNumber = kanban.UniqueNumber;
                                _ReceivingOrderDetailTemp.ReceiveUnitQty = kanban.ReceiveUnitQty;
                                _ReceivingOrderDetailTemp.ScanTime = kanban.ScanTime;

                                vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Add(_ReceivingOrderDetailTemp);
                            }

                            break;
                    }

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
        public ActionResult GetReceiveOrderReturnPartDetail(string receivenumber, string ordernumber)
        {
            var warrantyclaimdetail = (from a in vssp_db.Vw_TRS_ReturnPartDetail
                                       where a.ReturnNumber == ordernumber && a.OutstandingQty > 0
                                       select a).ToList();

            foreach (var detail in warrantyclaimdetail)
            {
                Tbl_TRS_ReceivingOrderDetailKanbanTemp temp = new Tbl_TRS_ReceivingOrderDetailKanbanTemp();
                temp.ReceiveNumber = receivenumber;
                temp.OrderNumber = ordernumber;
                temp.SupplierId = detail.SupplierId;
                temp.KanbanKey = detail.PartNumber;
                temp.UniqueNumber = detail.UniqueNumber;
                temp.ReceiveUnitQty = detail.OutstandingQty;
                temp.ScanTime = DateTime.Now;

                vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Add(temp);

            }

            try
            {
                vssp_db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudReceivingOrderList(string jsonData)
        {

            try
            {
                vssp_db.Database.CommandTimeout = 0;

                PostReceivingOrderModel postReceivingOrder = JsonConvert.DeserializeObject<PostReceivingOrderModel>(jsonData);
                Tbl_TRS_ReceivingOrder ReceivingOrder = postReceivingOrder.ReceivingOrder;

                string uid = postReceivingOrder.uid;
                string formAction = postReceivingOrder.formAction.ToLower();
                //var sspprocess = (from a in vssp_db.Tbl_TRS_SupplierOrder
                //                  join b in vssp_db.Tbl_MST_SpecialSupplyPart on a.SSP equals b.Id into ssp
                //                  from b in ssp.DefaultIfEmpty()
                //                  where a.OrderNumber == ReceivingOrder.OrderNumber
                //                  select new { a.OrderNumber, a.SSP, b.SSPStock, b.DeliveryOrder }).FirstOrDefault();

                //bool SSPStock = false;
                //if (sspprocess != null)
                //{
                //    SSPStock = _SystemService.Vb(sspprocess.SSPStock.ToString());
                //}

                switch (formAction)
                {
                    case "create":


                        /* Get New Order Number */
                        string CompId = Session["CompID"].ToString();
                        var ReceiveNumber = vssp_db.SP_GET_ReceivingOrderNumber(ReceivingOrder.SupplierId, ReceivingOrder.ReceiveDate, CompId);
                        foreach (SP_GET_ReceivingOrderNumber_Result number in ReceiveNumber)
                        {
                            ReceivingOrder.ReceiveNumber = number.OrderNumber;
                        }

                        DateTime ndate = DateTime.Now;
                        string rdate = _SystemService.Vd(ReceivingOrder.ReceiveDate.ToString(), "yyyy-MM-dd") + " " + ndate.ToString("HH:mm:ss");
                        ReceivingOrder.ReceiveDate = Convert.ToDateTime(rdate);

                        Tbl_TRS_ReceivingOrder ListReceivingOrder = new Tbl_TRS_ReceivingOrder();
                        ListReceivingOrder.ReceiveNumber = ReceivingOrder.ReceiveNumber;
                        ListReceivingOrder.ReceiveDate = ReceivingOrder.ReceiveDate;
                        ListReceivingOrder.OrderNumber = ReceivingOrder.OrderNumber;
                        ListReceivingOrder.SupplierId = ReceivingOrder.SupplierId;
                        ListReceivingOrder.ReturnPart = ReceivingOrder.ReturnPart;
                        ListReceivingOrder.Remarks = ReceivingOrder.Remarks;
                        ListReceivingOrder.Status = 0;
                        ListReceivingOrder.UserId = uid;
                        ListReceivingOrder.EditDate = DateTime.Now;

                        vssp_db.Tbl_TRS_ReceivingOrder.Add(ListReceivingOrder);

                        var addSupplierOrderKanbanTemp = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp
                                                          join b in vssp_db.Tbl_TRS_SupplierOrderKanban on new { a.KanbanKey } equals new { b.KanbanKey } into kanban
                                                          from b in kanban.DefaultIfEmpty()
                                                          join c in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, b.PartNumber } equals new { c.SupplierId, c.PartNumber } into part
                                                          from c in part.DefaultIfEmpty()
                                                          where a.ReceiveNumber == uid
                                                          select new { a.ReceiveNumber, a.OrderNumber, a.SupplierId, a.KanbanKey, a.UniqueNumber, c.PartNumber, c.UnitQty, a.ReceiveUnitQty, a.ScanTime }).ToList();

                        foreach (var kanban in addSupplierOrderKanbanTemp)
                        {
                            Tbl_TRS_ReceivingOrderDetailKanban _ReceivingOrderDetail = new Tbl_TRS_ReceivingOrderDetailKanban();
                            _ReceivingOrderDetail.ReceiveNumber = ReceivingOrder.ReceiveNumber;
                            _ReceivingOrderDetail.OrderNumber = kanban.OrderNumber;
                            _ReceivingOrderDetail.SupplierId = kanban.SupplierId;
                            _ReceivingOrderDetail.KanbanKey = kanban.KanbanKey;
                            _ReceivingOrderDetail.UniqueNumber = kanban.UniqueNumber;
                            _ReceivingOrderDetail.ReceiveUnitQty = kanban.ReceiveUnitQty;
                            _ReceivingOrderDetail.ScanTime = kanban.ScanTime;

                            vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Add(_ReceivingOrderDetail);

                            if (ReceivingOrder.ReturnPart == false)
                            {
                                var kanbanorder = vssp_db.Tbl_TRS_SupplierOrderKanban.Where(a => a.OrderNumber == kanban.OrderNumber && a.KanbanKey == kanban.KanbanKey).FirstOrDefault();
                                kanbanorder.Received = true;

                                //if (SSPStock == false)
                                //{
                                //    crudStock(kanban.SupplierId, kanban.PartNumber, _SystemService.Vn(kanban.UnitQty.ToString()), _SystemService.Vn(kanban.ReceiveUnitQty.ToString()), "Add", kanban.OrderNumber);
                                //}
                                //else
                                //{
                                //    crudStockSSP(kanban.SupplierId, kanban.PartNumber, 1, _SystemService.Vn(kanban.ReceiveUnitQty.ToString()), "Add", kanban.OrderNumber);
                                //}
                            }
                            var deleteKanban = vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Where(a => a.ReceiveNumber == kanban.ReceiveNumber && a.KanbanKey == kanban.KanbanKey).FirstOrDefault();
                            vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Remove(deleteKanban);

                        }

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_ReceivingOrder.First(a => a.ReceiveNumber == ReceivingOrder.ReceiveNumber);

                        //ListUpdate.ReceiveDate = ReceivingOrder.ReceiveDate;
                        ListUpdate.SupplierId = ReceivingOrder.SupplierId;
                        ListUpdate.ReturnPart = ReceivingOrder.ReturnPart;
                        ListUpdate.Remarks = ReceivingOrder.Remarks;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Details */
                        //var updateSupplierOrderKanban = vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Where(a => a.ReceiveNumber == ReceivingOrder.ReceiveNumber).ToList();
                        var updateSupplierOrderKanban = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanban
                                                         join b in vssp_db.Tbl_TRS_SupplierOrderKanban on new { a.KanbanKey } equals new { b.KanbanKey } into kanban
                                                         from b in kanban.DefaultIfEmpty()
                                                         join c in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, b.PartNumber } equals new { c.SupplierId, c.PartNumber } into part
                                                         from c in part.DefaultIfEmpty()
                                                         where a.ReceiveNumber == ReceivingOrder.ReceiveNumber
                                                         select new { a.ReceiveNumber, a.OrderNumber, a.SupplierId, a.KanbanKey, a.UniqueNumber, c.PartNumber, c.UnitQty, a.ReceiveUnitQty, a.ScanTime }).ToList();

                        foreach (var delete in updateSupplierOrderKanban)
                        {
                            //vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Remove(delete);
                            var deleteKanban = vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Where(a => a.ReceiveNumber == delete.ReceiveNumber && a.KanbanKey == delete.KanbanKey).FirstOrDefault();
                            vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Remove(deleteKanban);

                            if (ReceivingOrder.ReturnPart == false)
                            {
                                var kanbanorder = vssp_db.Tbl_TRS_SupplierOrderKanban.Where(a => a.OrderNumber == deleteKanban.OrderNumber && a.KanbanKey == deleteKanban.KanbanKey).FirstOrDefault();
                                kanbanorder.Received = false;

                                //if (SSPStock == false)
                                //{
                                //    crudStock(delete.SupplierId, delete.PartNumber, _SystemService.Vn(delete.UnitQty.ToString()), _SystemService.Vn(delete.ReceiveUnitQty.ToString()), "Delete", deleteKanban.OrderNumber);
                                //}
                                //else
                                //{
                                //    crudStockSSP(delete.SupplierId, delete.PartNumber, 1, _SystemService.Vn(delete.ReceiveUnitQty.ToString()), "Delete", deleteKanban.OrderNumber);
                                //}
                            }
                        }

                        var updateSupplierOrderKanbanTemp = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp
                                                             join b in vssp_db.Tbl_TRS_SupplierOrderKanban on new { a.KanbanKey } equals new { b.KanbanKey } into kanban
                                                             from b in kanban.DefaultIfEmpty()
                                                             join c in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, b.PartNumber } equals new { c.SupplierId, c.PartNumber } into part
                                                             from c in part.DefaultIfEmpty()
                                                             where a.ReceiveNumber == ReceivingOrder.ReceiveNumber
                                                             select new { a.ReceiveNumber, a.OrderNumber, a.SupplierId, a.KanbanKey, a.UniqueNumber, c.PartNumber, c.UnitQty, a.ReceiveUnitQty, a.ScanTime }).ToList();

                        foreach (var kanban in updateSupplierOrderKanbanTemp)
                        {
                            Tbl_TRS_ReceivingOrderDetailKanban _ReceivingOrderDetail = new Tbl_TRS_ReceivingOrderDetailKanban();
                            _ReceivingOrderDetail.ReceiveNumber = ReceivingOrder.ReceiveNumber;
                            _ReceivingOrderDetail.OrderNumber = kanban.OrderNumber;
                            _ReceivingOrderDetail.SupplierId = kanban.SupplierId;
                            _ReceivingOrderDetail.KanbanKey = kanban.KanbanKey;
                            _ReceivingOrderDetail.UniqueNumber = kanban.UniqueNumber;
                            _ReceivingOrderDetail.ReceiveUnitQty = kanban.ReceiveUnitQty;
                            _ReceivingOrderDetail.ScanTime = kanban.ScanTime;

                            vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Add(_ReceivingOrderDetail);

                            if (ReceivingOrder.ReturnPart == false)
                            {
                                var kanbanorder = vssp_db.Tbl_TRS_SupplierOrderKanban.Where(a => a.OrderNumber == kanban.OrderNumber && a.KanbanKey == kanban.KanbanKey).FirstOrDefault();
                                kanbanorder.Received = true;

                                //if (SSPStock == false)
                                //{
                                //    crudStock(kanban.SupplierId, kanban.PartNumber, _SystemService.Vn(kanban.UnitQty.ToString()), _SystemService.Vn(kanban.ReceiveUnitQty.ToString()), "Add", kanban.OrderNumber);
                                //}
                                //else
                                //{
                                //    crudStockSSP(kanban.SupplierId, kanban.PartNumber, 1, _SystemService.Vn(kanban.ReceiveUnitQty.ToString()), "Add", kanban.OrderNumber);
                                //}
                            }
                            var deleteKanban = vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Where(a => a.ReceiveNumber == kanban.ReceiveNumber && a.KanbanKey == kanban.KanbanKey).FirstOrDefault();
                            vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Remove(deleteKanban);

                        }

                        break;

                    case "closed":

                        var ListClosed = vssp_db.Tbl_TRS_ReceivingOrder.First(a => a.ReceiveNumber == ReceivingOrder.ReceiveNumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_TRS_ReceivingOrder.First(a => a.ReceiveNumber == ReceivingOrder.ReceiveNumber);

                        ListCanceled.Status = 4;

                        var canceledSupplierOrderKanban = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanban
                                                           join b in vssp_db.Tbl_TRS_SupplierOrderKanban on new { a.KanbanKey } equals new { b.KanbanKey } into kanban
                                                           from b in kanban.DefaultIfEmpty()
                                                           join c in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, b.PartNumber } equals new { c.SupplierId, c.PartNumber } into part
                                                           from c in part.DefaultIfEmpty()
                                                           where a.ReceiveNumber == ReceivingOrder.ReceiveNumber
                                                           select new { a.ReceiveNumber, a.OrderNumber, a.SupplierId, a.KanbanKey, a.UniqueNumber, c.PartNumber, c.UnitQty, a.ReceiveUnitQty, a.ScanTime }).ToList();

                        foreach (var delete in canceledSupplierOrderKanban)
                        {
                            //vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Remove(delete);
                            var deleteKanban = vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Where(a => a.ReceiveNumber == delete.ReceiveNumber && a.KanbanKey == delete.KanbanKey).FirstOrDefault();
                            vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Remove(deleteKanban);

                            if (ReceivingOrder.ReturnPart == false)
                            {
                                var kanbanorder = vssp_db.Tbl_TRS_SupplierOrderKanban.Where(a => a.OrderNumber == deleteKanban.OrderNumber && a.KanbanKey == deleteKanban.KanbanKey).FirstOrDefault();
                                kanbanorder.Received = false;

                                //if (SSPStock == false)
                                //{
                                //    crudStock(delete.SupplierId, delete.PartNumber, _SystemService.Vn(delete.UnitQty.ToString()), _SystemService.Vn(delete.ReceiveUnitQty.ToString()), "Delete", deleteKanban.OrderNumber);
                                //}
                                //else
                                //{
                                //    crudStockSSP(delete.SupplierId, delete.PartNumber, 1, _SystemService.Vn(delete.ReceiveUnitQty.ToString()), "Delete", deleteKanban.OrderNumber);
                                //}
                            }
                        }

                        break;

                    case "delete":

                        /* remove existing ReceivingOrder */
                        var ListDelete = vssp_db.Tbl_TRS_ReceivingOrder.First(a => a.ReceiveNumber == ReceivingOrder.ReceiveNumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        //vssp_db.Tbl_TRS_ReceivingOrder.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                        var deleteSupplierOrderKanban = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanban
                                                         join b in vssp_db.Tbl_TRS_SupplierOrderKanban on new { a.KanbanKey } equals new { b.KanbanKey } into kanban
                                                         from b in kanban.DefaultIfEmpty()
                                                         join c in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, b.PartNumber } equals new { c.SupplierId, c.PartNumber } into part
                                                         from c in part.DefaultIfEmpty()
                                                         where a.ReceiveNumber == ReceivingOrder.ReceiveNumber
                                                         select new { a.ReceiveNumber, a.OrderNumber, a.SupplierId, a.KanbanKey, a.UniqueNumber, b.PartNumber, b.UnitQty, a.ReceiveUnitQty, a.ScanTime }).ToList();

                        foreach (var delete in deleteSupplierOrderKanban)
                        {
                            //vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Remove(delete);
                            var deleteKanban = vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Where(a => a.ReceiveNumber == delete.ReceiveNumber && a.KanbanKey == delete.KanbanKey).FirstOrDefault();
                            vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Remove(deleteKanban);

                            if (ReceivingOrder.ReturnPart == false)
                            {
                                var kanbanorder = vssp_db.Tbl_TRS_SupplierOrderKanban.Where(a => a.OrderNumber == deleteKanban.OrderNumber && a.KanbanKey == deleteKanban.KanbanKey).FirstOrDefault();
                                kanbanorder.Received = false;

                                //if (SSPStock == false)
                                //{
                                //    crudStock(delete.SupplierId, delete.PartNumber, _SystemService.Vn(delete.UnitQty.ToString()), _SystemService.Vn(delete.ReceiveUnitQty.ToString()), "Delete", deleteKanban.OrderNumber);
                                //}
                                //else
                                //{
                                //    crudStockSSP(delete.SupplierId, delete.PartNumber, 1, _SystemService.Vn(delete.ReceiveUnitQty.ToString()), "Delete", deleteKanban.OrderNumber);
                                //}
                            }

                        }

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();

                    var receivingDetail = vssp_db.SP_CRUD_ReceivingOrderDetail(ReceivingOrder.ReceiveNumber, ReceivingOrder.OrderNumber, ReceivingOrder.ReturnPart, formAction);
                    var orderStatus = vssp_db.SP_CRUD_SupplierOrderStatus(ReceivingOrder.OrderNumber, ReceivingOrder.ReturnPart, formAction);

                    if (formAction != "closed")
                    {
                        var receivingStock = vssp_db.SP_CRUD_StockTransactionReceivingOrder(ReceivingOrder.ReceiveNumber, ReceivingOrder.OrderNumber, ReceivingOrder.ReturnPart, uid, formAction);
                        string guid = Guid.NewGuid().ToString();
                        vssp_db.SP_GET_SupplierOrderPerformance(ReceivingOrder.OrderNumber, ReceivingOrder.ReceiveNumber, guid, ReceivingOrder.ReceiveDate, formAction);
                    }

                    return Json(ReceivingOrder, JsonRequestBehavior.AllowGet);
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

        //public void crudReceivingOrderDetail(List<crud_ReceivingOrderDetail> ReceivingOrderDetails, string ReceiveNumber, string SupplierId, string formAction)
        //{

        //    foreach (var Details in ReceivingOrderDetails)
        //    {
        //        if (Details.RowStatus == null && formAction == "create")
        //        {
        //            Details.RowStatus = formAction;
        //        }
        //        if (Details.RowStatus != null)
        //        {
        //            switch (Details.RowStatus.ToLower())
        //            {
        //                case "create":

        //                    /* create Details */
        //                    Tbl_TRS_ReceivingOrderDetailKanban ListDetail = new Tbl_TRS_ReceivingOrderDetailKanban();
        //                    ListDetail.ReceiveNumber = ReceiveNumber;
        //                    ListDetail.OrderNumber = Details.OrderNumber;
        //                    ListDetail.SupplierId = SupplierId;
        //                    ListDetail.KanbanKey = Details.KanbanKey;
        //                    ListDetail.ReceiveUnitQty = Details.ReceiveUnitQty;
        //                    ListDetail.ScanTime = DateTime.Now;

        //                    vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Add(ListDetail);

        //                    break;

        //                case "update":

        //                    var ListUpdate = vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.First(a => a.ReceiveNumber == ReceiveNumber && a.KanbanKey == Details.KanbanKey);

        //                    ListUpdate.OrderNumber = Details.OrderNumber;
        //                    ListUpdate.SupplierId = SupplierId;
        //                    ListUpdate.KanbanKey = Details.KanbanKey;
        //                    ListUpdate.ReceiveUnitQty = Details.ReceiveUnitQty;
        //                    ListUpdate.ScanTime = DateTime.Now;

        //                    break;

        //                case "delete":

        //                    var ListDelete = vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.First(a => a.ReceiveNumber == ReceiveNumber && a.KanbanKey == Details.KanbanKey);

        //                    vssp_db.Tbl_TRS_ReceivingOrderDetailKanban.Remove(ListDelete);

        //                    break;
        //            }
        //        }
        //    }

        //}
        //public void crudReceivingOrderDetailTemp(List<crud_ReceivingOrderDetail> ReceivingOrderDetails, string ReceiveNumber, string SupplierId, string formAction)
        //{

        //    foreach (var Details in ReceivingOrderDetails)
        //    {
        //        if (Details.RowStatus == null && formAction == "create")
        //        {
        //            Details.RowStatus = formAction;
        //        }
        //        if (Details.RowStatus != null)
        //        {
        //            switch (Details.RowStatus.ToLower())
        //            {
        //                case "create":

        //                    /* create Details */
        //                    Tbl_TRS_ReceivingOrderDetailKanbanTemp ListDetail = new Tbl_TRS_ReceivingOrderDetailKanbanTemp();
        //                    ListDetail.ReceiveNumber = ReceiveNumber;
        //                    ListDetail.OrderNumber = Details.OrderNumber;
        //                    ListDetail.SupplierId = SupplierId;
        //                    ListDetail.KanbanKey = Details.KanbanKey;
        //                    ListDetail.ReceiveUnitQty = Details.ReceiveUnitQty;
        //                    ListDetail.ScanTime = DateTime.Now;

        //                    vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Add(ListDetail);

        //                    break;

        //                case "update":

        //                    var ListUpdate = vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.First(a => a.ReceiveNumber == ReceiveNumber && a.KanbanKey == Details.KanbanKey);

        //                    ListUpdate.OrderNumber = Details.OrderNumber;
        //                    ListUpdate.SupplierId = SupplierId;
        //                    ListUpdate.KanbanKey = Details.KanbanKey;
        //                    ListUpdate.ReceiveUnitQty = Details.ReceiveUnitQty;
        //                    ListUpdate.ScanTime = DateTime.Now;

        //                    break;

        //                case "delete":

        //                    var ListDelete = vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.First(a => a.ReceiveNumber == ReceiveNumber && a.KanbanKey == Details.KanbanKey);

        //                    vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Remove(ListDelete);

        //                    break;
        //            }
        //        }
        //    }

        //}

        public ActionResult deleteReceiveOrderKanban(string ReceiveNumber, string KanbanKey)
        {
            try
            {
                if (_SystemService.Vf(ReceiveNumber) == "")
                {
                    ReceiveNumber = Session["UserID"].ToString();
                }
                var ListDelete = vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.First(a => a.ReceiveNumber == ReceiveNumber && a.KanbanKey == KanbanKey);
                vssp_db.Tbl_TRS_ReceivingOrderDetailKanbanTemp.Remove(ListDelete);

                try
                {
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

        //public void crudStock(string supplierid, string partnumber, double kanbanqty, double unitqty, string formaction, string ordernumber, bool save = false)
        //{
        //    Tbl_TRS_Stock stock = (from a in vssp_db.Tbl_TRS_Stock
        //                           where a.SupplierId == supplierid && a.PartNumber == partnumber
        //                           select a).FirstOrDefault();

        //    double stockkanban = 0;

        //    switch (formaction.ToLower())
        //    {
        //        case "add":


        //            if (stock == null)
        //            {
        //                stockkanban = unitqty / kanbanqty;

        //                Tbl_TRS_Stock _Stock = new Tbl_TRS_Stock();
        //                _Stock.SupplierId = supplierid;
        //                _Stock.PartNumber = partnumber;
        //                _Stock.MinStock = 0;
        //                _Stock.MaxStock = 0;
        //                _Stock.StockKanban = stockkanban;
        //                _Stock.StockQty = unitqty;
        //                _Stock.LastUpdate = DateTime.Now;

        //                vssp_db.Tbl_TRS_Stock.Add(_Stock);
        //            }
        //            else
        //            {
        //                stockkanban = (_SystemService.Vn(stock.StockQty.ToString()) + unitqty) / kanbanqty;

        //                stock.StockKanban = stockkanban;
        //                stock.StockQty += unitqty;
        //                stock.LastUpdate = DateTime.Now;
        //            }


        //            if (_SystemService.Vf(ordernumber) != "")
        //            {
        //                var supplierorderdetail = vssp_db.Tbl_TRS_SupplierOrderDetail.Where(a => a.OrderNumber == ordernumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                supplierorderdetail.ReceiveQty += unitqty;
        //            }

        //            break;
        //        case "delete":

        //            stockkanban = (_SystemService.Vn(stock.StockQty.ToString()) - unitqty) / kanbanqty;

        //            stock.StockKanban = stockkanban;
        //            stock.StockQty -= unitqty;
        //            stock.LastUpdate = DateTime.Now;

        //            if (_SystemService.Vf(ordernumber) != "")
        //            {
        //                var supplierorderdetail = vssp_db.Tbl_TRS_SupplierOrderDetail.Where(a => a.OrderNumber == ordernumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                supplierorderdetail.ReceiveQty -= unitqty;
        //            }

        //            break;
        //    }

        //    if (save == true)
        //    {
        //        vssp_db.SaveChanges();
        //    }
        //}
        //public void crudStockSSP(string supplierid, string partnumber, double kanbanqty, double unitqty, string formaction, string ordernumber, bool receive = true, bool save = false)
        //{

        //    Tbl_TRS_StockSSP stockssp = (from a in vssp_db.Tbl_TRS_StockSSP
        //                                 where a.SupplierId == supplierid && a.PartNumber == partnumber
        //                                 select a).FirstOrDefault();

        //    if (stockssp != null)
        //    {
        //        switch (formaction.ToLower())
        //        {
        //            case "add":


        //                if (stockssp == null)
        //                {
        //                    Tbl_TRS_Stock stock = (from a in vssp_db.Tbl_TRS_Stock
        //                                           where a.SupplierId == supplierid && a.PartNumber == partnumber
        //                                           select a).FirstOrDefault();
        //                    if (stock != null)
        //                    {
        //                        Tbl_TRS_StockSSP stocknew = new Tbl_TRS_StockSSP();
        //                        stocknew.SupplierId = stock.SupplierId;
        //                        stocknew.PartNumber = stock.PartNumber;
        //                        stocknew.MinStock = stock.MinStock;
        //                        stocknew.MaxStock = stock.MaxStock;
        //                        stocknew.StockKanban = kanbanqty;
        //                        stocknew.StockQty = unitqty;
        //                        stocknew.LastUpdate = DateTime.Now;

        //                        vssp_db.Tbl_TRS_StockSSP.Add(stocknew);
        //                        stockssp = stocknew;
        //                    }
        //                }
        //                else
        //                {
        //                    stockssp.StockKanban += kanbanqty;
        //                    stockssp.StockQty += unitqty;
        //                    stockssp.LastUpdate = DateTime.Now;
        //                }

        //                if (_SystemService.Vf(ordernumber) != "" && receive == true)
        //                {
        //                    var supplierorderdetail = vssp_db.Tbl_TRS_SupplierOrderDetail.Where(a => a.OrderNumber == ordernumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                    if (supplierorderdetail != null) supplierorderdetail.ReceiveQty += unitqty;
        //                }

        //                break;
        //            case "update":

        //                kanbanqty = _SystemService.Vn(stockssp.StockKanban.ToString()) - kanbanqty;
        //                unitqty = _SystemService.Vn(stockssp.StockQty.ToString()) - unitqty;

        //                stockssp.StockKanban += kanbanqty;
        //                stockssp.StockQty += unitqty;
        //                stockssp.LastUpdate = DateTime.Now;

        //                if (_SystemService.Vf(ordernumber) != "" && receive == true)
        //                {
        //                    var supplierorderdetail = vssp_db.Tbl_TRS_SupplierOrderDetail.Where(a => a.OrderNumber == ordernumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                    if (supplierorderdetail != null) supplierorderdetail.ReceiveQty += unitqty;
        //                }

        //                break;
        //            case "delete":
        //                stockssp.StockKanban -= kanbanqty;
        //                stockssp.StockQty -= unitqty;
        //                stockssp.LastUpdate = DateTime.Now;

        //                if (_SystemService.Vf(ordernumber) != "" && receive == true)
        //                {
        //                    var supplierorderdetail = vssp_db.Tbl_TRS_SupplierOrderDetail.Where(a => a.OrderNumber == ordernumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                    if (supplierorderdetail != null) supplierorderdetail.ReceiveQty -= unitqty;
        //                }

        //                break;
        //        }

        //        if (save == true)
        //        {
        //            vssp_db.SaveChanges();
        //        }

        //    }
        //}
        //public void crudStockWIP(string lineid, string supplierid, string partnumber, double kanbanqty, double unitqty, string formaction, string ordernumber, bool save = false)
        //{
        //    Tbl_TRS_StockWIP stock = (from a in vssp_db.Tbl_TRS_StockWIP
        //                              where a.LineId == lineid && a.SupplierId == supplierid && a.PartNumber == partnumber
        //                              select a).FirstOrDefault();

        //    if (stock != null)
        //    {
        //        switch (formaction.ToLower())
        //        {
        //            case "add":
        //                stock.StockKanban += kanbanqty;
        //                stock.StockQty += unitqty;
        //                stock.LastUpdate = DateTime.Now;

        //                if (_SystemService.Vf(ordernumber) != "")
        //                {
        //                    var RequestOrderPartsdetail = vssp_db.Tbl_TRS_RequestOrderPartsDetail.Where(a => a.OrderNumber == ordernumber && a.LineId == lineid && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                    if (RequestOrderPartsdetail != null) RequestOrderPartsdetail.ReceiveQty += unitqty;
        //                }

        //                break;
        //            case "update":

        //                kanbanqty = _SystemService.Vn(stock.StockKanban.ToString()) - kanbanqty;
        //                unitqty = _SystemService.Vn(stock.StockQty.ToString()) - unitqty;

        //                stock.StockKanban += kanbanqty;
        //                stock.StockQty += unitqty;
        //                stock.LastUpdate = DateTime.Now;

        //                if (_SystemService.Vf(ordernumber) != "")
        //                {
        //                    var RequestOrderPartsdetail = vssp_db.Tbl_TRS_RequestOrderPartsDetail.Where(a => a.OrderNumber == ordernumber && a.LineId == lineid && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                    if (RequestOrderPartsdetail != null) RequestOrderPartsdetail.ReceiveQty += unitqty;
        //                }

        //                break;
        //            case "delete":
        //                stock.StockKanban -= kanbanqty;
        //                stock.StockQty -= unitqty;
        //                stock.LastUpdate = DateTime.Now;

        //                if (_SystemService.Vf(ordernumber) != "")
        //                {
        //                    var RequestOrderPartsdetail = vssp_db.Tbl_TRS_RequestOrderPartsDetail.Where(a => a.OrderNumber == ordernumber && a.LineId == lineid && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
        //                    if (RequestOrderPartsdetail != null) RequestOrderPartsdetail.ReceiveQty -= unitqty;
        //                }

        //                break;
        //        }

        //        if (save == true)
        //        {
        //            vssp_db.SaveChanges();
        //        }

        //    }
        //}
        public void crudStockRawReturn(string supplierid, string partnumber, double unitqty, double lastunitqty, string formaction, string ReturnNumber, bool receive = true, bool save = false)
        {

            Tbl_TRS_StockRawReturn stockRawReturn = (from a in vssp_db.Tbl_TRS_StockRawReturn
                                                     where a.SupplierId == supplierid && a.PartNumber == partnumber
                                                     select a).FirstOrDefault();

            switch (formaction.ToLower())
            {
                case "create":


                    if (stockRawReturn == null)
                    {
                        Tbl_TRS_StockRawReturn stocknew = new Tbl_TRS_StockRawReturn();
                        stocknew.SupplierId = supplierid;
                        stocknew.PartNumber = partnumber;
                        stocknew.MinStock = 0;
                        stocknew.MaxStock = 10;
                        stocknew.StockQty = unitqty;
                        stocknew.LastUpdate = DateTime.Now;

                        vssp_db.Tbl_TRS_StockRawReturn.Add(stocknew);
                        stockRawReturn = stocknew;
                    }
                    else
                    {
                        stockRawReturn.StockQty += unitqty;
                        stockRawReturn.LastUpdate = DateTime.Now;
                    }

                    if (_SystemService.Vf(ReturnNumber) != "" && receive == true)
                    {
                        var supplierorderdetail = vssp_db.Tbl_TRS_ReturnPartDetail.Where(a => a.ReturnNumber == ReturnNumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
                        if (supplierorderdetail != null) supplierorderdetail.ReceiveQty += unitqty;
                    }

                    break;
                case "update":

                    stockRawReturn.StockQty -= lastunitqty;
                    stockRawReturn.StockQty += unitqty;
                    stockRawReturn.LastUpdate = DateTime.Now;

                    if (_SystemService.Vf(ReturnNumber) != "" && receive == true)
                    {
                        var supplierorderdetail = vssp_db.Tbl_TRS_ReturnPartDetail.Where(a => a.ReturnNumber == ReturnNumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
                        if (supplierorderdetail != null) supplierorderdetail.ReceiveQty += unitqty;
                    }

                    break;
                case "delete":
                    stockRawReturn.StockQty -= unitqty;
                    stockRawReturn.LastUpdate = DateTime.Now;

                    if (_SystemService.Vf(ReturnNumber) != "" && receive == true)
                    {
                        var supplierorderdetail = vssp_db.Tbl_TRS_ReturnPartDetail.Where(a => a.ReturnNumber == ReturnNumber && a.SupplierId == supplierid && a.PartNumber == partnumber).FirstOrDefault();
                        if (supplierorderdetail != null) supplierorderdetail.ReceiveQty -= unitqty;
                    }

                    break;
            }

            if (save == true)
            {
                vssp_db.SaveChanges();
            }

        }
        public ActionResult RequestOrderParts(string ordernumber = "")
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();

                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Purchase", "RequestOrderParts");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Request Order Parts";
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.EmailCC = ecc;
                    ViewBag.DateTime = DateTime.Now;
                    ViewBag.OrderNumber = ordernumber;

                    RequestOrderPartsListModel RequestOrderParts = new RequestOrderPartsListModel();
                    RequestOrderParts.ExportList = _SystemService.ComboExport().ToList();
                    RequestOrderParts.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                                    orderby a.Id
                                                    select a).ToList();

                    Session["Layout"] = "portal";
                    return View(RequestOrderParts);

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult RequestOrderPartsListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_RequestOrderParts> RequestOrderParts = (from a in vssp_db.Vw_TRS_RequestOrderParts
                                                                where a.OrderNumber.Contains(searchFilter) || a.LineId.Contains(searchFilter)
                                                                orderby a.OrderDate descending, a.EditDate descending, a.OrderNumber
                                                                select a).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                RequestOrderParts = RequestOrderParts.Where(a => a.OrderDate >= startdate && a.OrderDate <= enddate).ToList();
            }
            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string ordermonth = arrs[0];
                string orderyears = arrs[1];
                RequestOrderParts = RequestOrderParts.Where(a => Convert.ToDateTime(a.OrderDate).ToString("MM") == ordermonth && Convert.ToDateTime(a.OrderDate).ToString("yyyy") == orderyears).ToList();
            }
            if (status != 99)
            {
                RequestOrderParts = RequestOrderParts.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in RequestOrderParts
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                RequestOrderParts = RequestOrderParts.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            return Json(RequestOrderParts, JsonRequestBehavior.AllowGet);

        }
        public ActionResult RequestOrderPartsDetailListJson(string ordernumber, string lineid, string shiftid, Nullable<DateTime> orderdate, string formAction)
        {
            try
            {

                switch (formAction)
                {
                    case "Create":
                        //var RequestOrderPartsDetailTemp = (from a in vssp_db.Vw_TRS_StockWIP
                        //                                  join b in vssp_db.Vw_TRS_Stock on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into rawstock
                        //                                  from b in rawstock.DefaultIfEmpty()
                        //                                  where a.LineId == lineid && (a.MaxStock - a.TotalStockKanban) > 0
                        //                                  select new
                        //                                  {
                        //                                      OrderNumber = ordernumber,
                        //                                      a.SupplierId,
                        //                                      a.UniqueNumber,
                        //                                      a.PartNumber,
                        //                                      a.PartName,
                        //                                      a.UnitQty,
                        //                                      a.UnitLevel1,
                        //                                      a.UnitLevel2,
                        //                                      a.PackingId,
                        //                                      a.PartModel,
                        //                                      a.MaxStock,
                        //                                      StockKanban = (b.StockKanban > a.StockKanban ? b.StockKanban : a.StockKanban),
                        //                                      StockQty = (b.StockQty > a.StockQty ? b.StockQty : a.StockQty),
                        //                                      a.OutstandingKanban,
                        //                                      a.OutstandingQty,
                        //                                      OrderQty = (b.StockKanban > a.StockKanban ? b.StockKanban : (a.MaxStock - a.TotalStockKanban)),
                        //                                      OrderUnitQty = (b.StockQty > a.StockQty ? b.StockQty : ((a.MaxStock - a.TotalStockKanban) * a.UnitQty)),
                        //                                      ReceiveQty = 0
                        //                                  }).ToList();
                        var RequestOrderPartsDetailTemp = (from a in vssp_db.Vw_TRS_RequestOrderPartCreateList
                                                           where a.LineId == lineid && a.ShiftId == shiftid && a.ProductionDate == orderdate
                                                           select new
                                                           {
                                                               OrderNumber = ordernumber,
                                                               a.SupplierId,
                                                               a.UniqueNumber,
                                                               a.PartNumber,
                                                               a.PartName,
                                                               a.UnitQty,
                                                               a.UnitLevel1,
                                                               a.UnitLevel2,
                                                               a.PackingId,
                                                               a.PartModel,
                                                               a.MaxStock,
                                                               a.StockKanban,
                                                               a.StockQty,
                                                               OutstandingKanban = 0,
                                                               OutstandingQty = 0,
                                                               OrderQty = a.OrderKanbanQty,
                                                               OrderUnitQty = a.OrderUnitQty,
                                                               ReceiveQty = 0
                                                           }).ToList();
                        return Json(RequestOrderPartsDetailTemp, JsonRequestBehavior.AllowGet);

                    default:
                        var RequestOrderPartsDetail = (from a in vssp_db.Vw_TRS_RequestOrderPartsDetail
                                                       join b in vssp_db.Vw_TRS_StockWIP on new { a.LineId, a.SupplierId, a.PartNumber } equals new { b.LineId, b.SupplierId, b.PartNumber } into stock
                                                       from b in stock.DefaultIfEmpty()
                                                       where a.OrderNumber == ordernumber
                                                       select new
                                                       {
                                                           a.OrderNumber,
                                                           a.LineId,
                                                           a.SupplierId,
                                                           b.UniqueNumber,
                                                           a.PartNumber,
                                                           b.PartName,
                                                           b.UnitQty,
                                                           b.UnitLevel1,
                                                           b.UnitLevel2,
                                                           b.PackingId,
                                                           b.PartModel,
                                                           b.OutstandingKanban,
                                                           b.OutstandingQty,
                                                           a.StockKanban,
                                                           a.StockQty,
                                                           a.OrderQty,
                                                           a.OrderUnitQty,
                                                           a.ReceiveQty
                                                       }).ToList();

                        return Json(RequestOrderPartsDetail, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudRequestOrderPartsList(string jsonData)
        {

            try
            {

                PostRequestOrderPartsModel postRequestOrderParts = JsonConvert.DeserializeObject<PostRequestOrderPartsModel>(jsonData);
                Tbl_TRS_RequestOrderParts RequestOrderParts = postRequestOrderParts.RequestOrderParts;
                List<crud_RequestOrderPartsDetail> RequestOrderPartsDetail = postRequestOrderParts.RequestOrderPartsDetail;

                string uid = postRequestOrderParts.uid;
                string formAction = postRequestOrderParts.formAction.ToLower();

                switch (formAction)
                {
                    case "create":

                        /* Get New Order Number */
                        string CompId = Session["CompID"].ToString();
                        var OrderNumber = vssp_db.SP_GET_RequestOrderPartsNumber(RequestOrderParts.LineId, RequestOrderParts.OrderDate, CompId);
                        foreach (SP_GET_RequestOrderPartsNumber_Result number in OrderNumber)
                        {
                            RequestOrderParts.OrderNumber = number.OrderNumber;
                        }

                        Tbl_TRS_RequestOrderParts ListRequestOrderParts = new Tbl_TRS_RequestOrderParts();
                        ListRequestOrderParts.OrderNumber = RequestOrderParts.OrderNumber;
                        ListRequestOrderParts.OrderDate = RequestOrderParts.OrderDate;
                        ListRequestOrderParts.DeliveryDate = RequestOrderParts.DeliveryDate;
                        ListRequestOrderParts.LineId = RequestOrderParts.LineId;
                        ListRequestOrderParts.ShiftId = RequestOrderParts.ShiftId;
                        ListRequestOrderParts.Remarks = RequestOrderParts.Remarks;
                        ListRequestOrderParts.Status = 0;
                        ListRequestOrderParts.UserId = uid;
                        ListRequestOrderParts.EditDate = DateTime.Now;

                        vssp_db.Tbl_TRS_RequestOrderParts.Add(ListRequestOrderParts);

                        /* crud Details */
                        crudRequestOrderPartsDetail(RequestOrderPartsDetail, RequestOrderParts.OrderNumber, RequestOrderParts.LineId, formAction);

                        /* crud Approval */
                        crudRequestOrderPartsApproval(postRequestOrderParts.ApprovalId, RequestOrderParts.OrderNumber, uid, formAction);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_RequestOrderParts.First(a => a.OrderNumber == RequestOrderParts.OrderNumber);

                        ListUpdate.OrderDate = RequestOrderParts.OrderDate;
                        ListUpdate.DeliveryDate = RequestOrderParts.DeliveryDate;
                        ListUpdate.LineId = RequestOrderParts.LineId;
                        ListUpdate.ShiftId = RequestOrderParts.ShiftId;
                        ListUpdate.Remarks = RequestOrderParts.Remarks;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Details */
                        crudRequestOrderPartsDetail(RequestOrderPartsDetail, RequestOrderParts.OrderNumber, RequestOrderParts.LineId, formAction);

                        /* crud Approval */
                        crudRequestOrderPartsApproval(postRequestOrderParts.ApprovalId, RequestOrderParts.OrderNumber, uid, formAction);

                        break;

                    case "delivery":

                        var ListDelivery = vssp_db.Tbl_TRS_RequestOrderParts.First(a => a.OrderNumber == RequestOrderParts.OrderNumber);

                        ListDelivery.Remarks = RequestOrderParts.Remarks;
                        if (ListDelivery.Status < 3)
                        {
                            ListDelivery.Status = 3;
                        }

                        /* crud Details */
                        crudRequestOrderPartsDetail(RequestOrderPartsDetail, RequestOrderParts.OrderNumber, RequestOrderParts.LineId, formAction);

                        break;

                    case "closed":

                        var ListClosed = vssp_db.Tbl_TRS_RequestOrderParts.First(a => a.OrderNumber == RequestOrderParts.OrderNumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_TRS_RequestOrderParts.First(a => a.OrderNumber == RequestOrderParts.OrderNumber);

                        ListCanceled.Status = 4;

                        /* crud details */
                        crudRequestOrderPartsDetail(RequestOrderPartsDetail, RequestOrderParts.OrderNumber, RequestOrderParts.LineId, "delete");

                        break;

                    case "delete":

                        /* remove existing RequestOrderParts */
                        var ListDelete = vssp_db.Tbl_TRS_RequestOrderParts.First(a => a.OrderNumber == RequestOrderParts.OrderNumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        /* crud details */
                        crudRequestOrderPartsDetail(RequestOrderPartsDetail, RequestOrderParts.OrderNumber, RequestOrderParts.LineId, formAction);

                        //vssp_db.Tbl_TRS_RequestOrderParts.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();

                    if (formAction == "delivery")
                    {

                        /* crud Approval */
                        crudRequestOrderPartsApproval(postRequestOrderParts.ApprovalId, RequestOrderParts.OrderNumber, uid, "Approved");
                        vssp_db.SP_CRUD_StockTransactionRequestOrderParts(RequestOrderParts.OrderNumber, uid, "create");

                    }

                    return Json(RequestOrderParts, JsonRequestBehavior.AllowGet);
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

        public void crudRequestOrderPartsDetail(List<crud_RequestOrderPartsDetail> RequestOrderPartsDetails, string OrderNumber, string LineId, string formAction)
        {

            foreach (var Details in RequestOrderPartsDetails)
            {

                //var sspprocess = (from a in vssp_db.Tbl_MST_SpecialSupplyPart
                //                  join b in vssp_db.Tbl_MST_PartRawMaterials on a.Id equals b.SSP into raw
                //                  from b in raw.DefaultIfEmpty()
                //                  where a.Id == SSP && b.PartNumber == Details.PartNumber && a.SSPStock == true
                //                  select new { b.LineId, b.PartNumber, a.SSPStock, a.DeliveryOrder }).FirstOrDefault();

                //var sspprocess = (from a in vssp_db.Vw_TRS_StockWIP
                //                  where a.LineId == LineId && a.PartNumber == Details.PartNumber
                //                  select new { a.LineId, a.PartNumber, a.SSPStock, a.DeliveryOrder }).FirstOrDefault();

                //float kbnqty = 0;
                //float delqty = 0;
                //float lastkbnqty = 0;
                //float lastdelqty = 0;

                if (Details.RowStatus == null)
                {
                    Details.RowStatus = formAction;
                }
                if (Details.RowStatus != null)
                {
                    switch (Details.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_RequestOrderPartsDetail ListDetail = new Tbl_TRS_RequestOrderPartsDetail();
                            ListDetail.OrderNumber = OrderNumber;
                            ListDetail.LineId = LineId;
                            ListDetail.SupplierId = Details.SupplierId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.StockKanban = Details.StockKanban;
                            ListDetail.StockQty = Details.StockKanban;
                            ListDetail.OrderQty = Details.OrderQty;
                            ListDetail.OrderUnitQty = Details.OrderUnitQty;
                            ListDetail.ReceiveQty = 0;

                            vssp_db.Tbl_TRS_RequestOrderPartsDetail.Add(ListDetail);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_TRS_RequestOrderPartsDetail.First(a => a.OrderNumber == OrderNumber && a.LineId == LineId && a.SupplierId == Details.SupplierId && a.PartNumber == Details.PartNumber);

                            //lastkbnqty = _SystemService.Vn(ListUpdate.OrderQty.ToString());
                            //lastdelqty = _SystemService.Vn(ListUpdate.OrderUnitQty.ToString());
                            if (formAction != "delivery")
                            {
                                ListUpdate.StockKanban = Details.StockKanban;
                                ListUpdate.StockQty = Details.StockQty;
                                ListUpdate.OrderQty = Details.OrderQty;
                                ListUpdate.OrderUnitQty = Details.OrderUnitQty;
                            }
                            else
                            {
                                ListUpdate.ReceiveQty = Details.ReceiveQty;

                                //var part = vssp_db.Tbl_MST_PartRawMaterials.Where(a => a.SupplierId == ListUpdate.SupplierId && a.PartNumber == ListUpdate.PartNumber).FirstOrDefault();

                                //double? qtykanban = (ListUpdate.ReceiveQty / part.UnitQty);
                                //double? qtyunit = ListUpdate.ReceiveQty;
                                //crudStockWIP(ListUpdate.LineId, ListUpdate.SupplierId, ListUpdate.PartNumber,_SystemService.Vn(qtykanban.ToString()), _SystemService.Vn(qtyunit.ToString()), "update", ListUpdate.OrderNumber, false);

                            }


                            break;

                        case "delivery":

                            var ListDelivery = vssp_db.Tbl_TRS_RequestOrderPartsDetail.First(a => a.OrderNumber == OrderNumber && a.LineId == LineId && a.SupplierId == Details.SupplierId && a.PartNumber == Details.PartNumber);

                            ListDelivery.ReceiveQty = Details.ReceiveQty;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_RequestOrderPartsDetail.First(a => a.OrderNumber == OrderNumber && a.LineId == LineId && a.SupplierId == Details.SupplierId && a.PartNumber == Details.PartNumber);

                            //if (SSPDelivery == true)
                            //{
                            //    var sspstock = (from a in vssp_db.Vw_TRS_Stock
                            //                    where a.PartNumber == Details.PartNumber && a.SSPStock == true
                            //                    select new { a.LineId, a.PartNumber, a.SSPStock, a.DeliveryOrder }).FirstOrDefault();

                            //    kbnqty = _SystemService.Vn(ListDelete.OrderQty.ToString());
                            //    delqty = _SystemService.Vn(ListDelete.OrderUnitQty.ToString());
                            //    if (sspstock != null) crudStockSSP(sspstock.LineId, sspstock.PartNumber, kbnqty, delqty, "Add", ListDelete.OrderNumber, false, false);
                            //}


                            break;
                    }
                }
            }

        }
        public void crudRequestOrderPartsApproval(string ApprovalId, string OrderNumber, string UserId, string action)
        {
            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    List<UserApprovalListModel> userApprovalLists = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalLists)
                    {
                        Tbl_TRS_RequestOrderPartsApproval ListApproval = new Tbl_TRS_RequestOrderPartsApproval();
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

                        vssp_db.Tbl_TRS_RequestOrderPartsApproval.Add(ListApproval);
                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_TRS_RequestOrderPartsApproval> UserApproval = vssp_db.Tbl_TRS_RequestOrderPartsApproval.Where(a => a.OrderNumber == OrderNumber).ToList();
                    foreach (var user in UserApproval)
                    {
                        UserApprovalListModel ApprovalLists = _AccountService.UserApprovalType(user.UserId, ApprovalId).First(a => a.UserID == user.UserId);
                    }

                    /* create approval */
                    List<UserApprovalListModel> userApprovalListsUpdate = _AccountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalListsUpdate)
                    {
                        Tbl_TRS_RequestOrderPartsApproval existUser = (from a in vssp_db.Tbl_TRS_RequestOrderPartsApproval
                                                                       where a.OrderNumber == OrderNumber && a.UserId == users.UserID
                                                                       select a).FirstOrDefault();
                        if (existUser == null)
                        {
                            Tbl_TRS_RequestOrderPartsApproval ListApproval = new Tbl_TRS_RequestOrderPartsApproval();
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

                            vssp_db.Tbl_TRS_RequestOrderPartsApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "sent":

                    var ListSent = vssp_db.Tbl_TRS_RequestOrderPartsApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == UserId);

                    ListSent.SentEmail = true;
                    ListSent.SentEmailDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "approved":

                    var ListUpdate = vssp_db.Tbl_TRS_RequestOrderPartsApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == UserId);

                    ListUpdate.Approved = true;
                    ListUpdate.ApprovedDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_TRS_RequestOrderPartsApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == ApprovalId);

                    vssp_db.Tbl_TRS_RequestOrderPartsApproval.Remove(ListDelete);

                    break;
            }

        }
        public ActionResult RequestOrderPartsApproval(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Production Request Order Parts List";

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
                    Vw_TRS_RequestOrderParts RequestOrderPartsOrder = vssp_db.Vw_TRS_RequestOrderParts.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_TRS_RequestOrderPartsApproval approval = vssp_db.Tbl_TRS_RequestOrderPartsApproval.Where(a => a.OrderNumber == ordernumber && a.UserId == user.UserID).FirstOrDefault();

                    if (RequestOrderPartsOrder != null && user != null && approval != null)
                    {

                        if (approval.Approved == false)
                        {

                            var ListReceive = vssp_db.Tbl_TRS_RequestOrderParts.First(a => a.OrderNumber == RequestOrderPartsOrder.OrderNumber);

                            if (ListReceive.Status < 2)
                            {
                                ListReceive.Status = 2;
                                vssp_db.SaveChanges();
                            }

                            if (RequestOrderPartsOrder != null && user != null)
                            {

                                //ViewBag.OrderTitle = "Production Request Order Parts";
                                //ViewBag.OrderNumber = RequestOrderPartsOrder.OrderNumber;
                                //ViewBag.OrderDate = _SystemService.Vd(RequestOrderPartsOrder.OrderDate.ToString());
                                //ViewBag.LineName = RequestOrderPartsOrder.LineName;
                                //ViewBag.UserID = uid;
                                //ViewBag.UserName = user.UserName;

                                return RedirectToAction("RequestOrderParts", new { ordernumber = RequestOrderPartsOrder.OrderNumber });

                            }
                            else
                            {
                                ViewBag.OrderTitle = "Production Request Order Parts";
                                ViewBag.UserName = null;

                                return RedirectToAction("ErrorPage", "System", new { errnumber = "500", errmessage = "Order or User not valid.", backaction = "MainIndex", backcontroller = "Index" });

                            }

                        }
                        else
                        {
                            ViewBag.OrderTitle = "Production Request Order Parts List";
                            ViewBag.OrderNumber = RequestOrderPartsOrder.OrderNumber;
                            ViewBag.OrderDate = _SystemService.Vd(RequestOrderPartsOrder.OrderDate.ToString(), "dd MMMM, yyyy");
                            ViewBag.LineName = RequestOrderPartsOrder.LineName;
                            ViewBag.UserID = uid;
                            ViewBag.UserName = user.UserName;
                            ViewBag.ApprovedDate = _SystemService.Vd(approval.ApprovedDate.ToString(), "dd MMMM, yyyy");
                            return View("RequestOrderPartsApproved");
                        }

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Production Request Order Parts";
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

        public ActionResult RequestOrderPartsApproved(string ordernumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Production Request Order Parts Approved";

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
                    Vw_TRS_RequestOrderParts RequestOrderPartsOrder = vssp_db.Vw_TRS_RequestOrderParts.Where(a => a.OrderNumber == ordernumber).FirstOrDefault();
                    UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();

                    if (RequestOrderPartsOrder != null && user != null)
                    {

                        ViewBag.OrderTitle = "Production Request Order Parts";
                        ViewBag.OrderNumber = RequestOrderPartsOrder.OrderNumber;
                        ViewBag.OrderDate = _SystemService.Vd(RequestOrderPartsOrder.OrderDate.ToString());
                        ViewBag.LineName = RequestOrderPartsOrder.LineName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        crudRequestOrderPartsApproval(user.UserID, RequestOrderPartsOrder.OrderNumber, user.UserID, "Approved");
                        return RedirectToAction("ContinuePage", "System", new { cmessage = "Successfuly Delivered " + ViewBag.OrderTitle + " \n " + ordernumber, caction = "Dashboard", ccontroller = "Home", capps = "Home" });

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Production Request Order Parts";
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
        public ActionResult RequestOrderPartsApprovalListJson(string ordernumber, Nullable<bool> approved)
        {
            try
            {

                var RequestOrderPartsApproval = from a in vssp_db.Tbl_TRS_RequestOrderPartsApproval
                                                where a.OrderNumber.Contains(ordernumber)
                                                orderby a.ApprovalLevel
                                                select new { a.OrderNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

                if (approved != null)
                {
                    RequestOrderPartsApproval = RequestOrderPartsApproval.Where(a => a.Approved == approved);
                }

                return Json(RequestOrderPartsApproval, JsonRequestBehavior.AllowGet);

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