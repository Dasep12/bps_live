using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Core.VSSP.Models;

namespace Core.VSSP.Controllers
{
    public class HomeController : Controller
    {
        SystemService systemService = new SystemService();
        AccountService accountService = new AccountService();
        vssp_entity vssp = new vssp_entity();

        public ActionResult Index()
        {
            if (Session == null)
            {
                return RedirectToAction("SignOut", "Account");
            } else
            {
                if (Session["UserID"] != null)
                {
                    Session["Layout"] = "admin";
                    return View();
                } else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            Session["Layout"] = "admin";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            Session["Layout"] = "admin";
            return View();
        }
        public ActionResult Dashboard(string viewtype)
        {
            if (Session["UserID"] != null)
            {
                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                var acccessPreviliege = accountService.AccessPreviliege(uid, "Home", "Dashboard");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;
                    ViewBag.ViewType = viewtype;

                    Session["Layout"] = "portal";

                    switch (viewtype)
                    {
                        case "General":
                            ViewBag.ViewType = "General";
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "Delivery":
                            ViewBag.ViewType = "Delivery";
                            ViewBag.Title = ViewBag.Title + " Sales & " + ViewBag.ViewType;
                            break;
                        case "Production":
                            ViewBag.ViewType = "Production";
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "ProductionSummary":
                            ViewBag.ViewType = "Production Summary";
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "Receiving":
                            ViewBag.ViewType = "Receiving";
                            ViewBag.Title = ViewBag.Title + " Purchase & " + ViewBag.ViewType;
                            break;
                        case "Sales":
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "Purchases":
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "Stock":
                            ViewBag.ViewType = "Stock";
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "QualityIncoming":
                            ViewBag.ViewType = "Quality Incoming";
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "QualityOutgoing":
                            ViewBag.ViewType = "Quality Outgoing";
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        case "Summary":
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                        default:
                            ViewBag.ViewType = "Home";
                            ViewBag.Title = ViewBag.Title + " " + ViewBag.ViewType;
                            break;
                    }

                    return View();

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult GeneralBrandCardJson(string monthyear)
        {

            List<SP_IDX_SummaryMaster_Result> SummaryMaster = new List<SP_IDX_SummaryMaster_Result>();

            if (systemService.Vf(monthyear) != "")
            {
                string[] monthyearArr = monthyear.Split('/');
                string month = monthyearArr[0];
                string years = monthyearArr[1];

                vssp.Database.CommandTimeout = 0;
                SummaryMaster = vssp.SP_IDX_SummaryMaster(month, years).ToList();

            }

            return Json(SummaryMaster, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GeneralTransactionChartJson(string monthyear)
        {

            List<SP_IDX_TransactionChart_Result> TransactionChart = new List<SP_IDX_TransactionChart_Result>();

            if (systemService.Vf(monthyear) != "")
            {
                string[] monthyearArr = monthyear.Split('/');
                string month = monthyearArr[0];
                string years = monthyearArr[1];

                vssp.Database.CommandTimeout = 0;
                TransactionChart = vssp.SP_IDX_TransactionChart(month, years).ToList();

            }

            return Json(TransactionChart, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GeneralStockInfoJson()
        {

            vssp.Database.CommandTimeout = 0;
            List<Vw_TRS_Stock> stock = vssp.Vw_TRS_Stock.Where(a => a.TotalStockKanban < a.MinStock || a.TotalStockKanban > a.MaxStock).ToList();
            return Json(stock, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GeneralMLOKCapacityJson()
        {

            vssp.Database.CommandTimeout = 0;
            List<SP_IDX_StockKanbanSummary_Result> _StockKanbanSummary = vssp.SP_IDX_StockKanbanSummary().ToList();
            return Json(_StockKanbanSummary, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GeneralUpcomingReceiving(Nullable<DateTime> filterdate)
        {

            if(filterdate == null)
            {
                filterdate = DateTime.Now;
            }

            vssp.Database.CommandTimeout = 0;
            List<SP_IDX_UpcomingReceiving_Result> upcomingReceiving = vssp.SP_IDX_UpcomingReceiving(filterdate).ToList();
            return PartialView("Dashboard/_GeneralPartial/UpcomingReceiving", upcomingReceiving);

        }
        public ActionResult DeliverySummaryJson(DateTime today, string customerFilter, string partnumberFilter)
        {

            if (customerFilter == "") customerFilter = null;
            if (partnumberFilter == "") partnumberFilter = null;
            List<SP_IDX_DeliverySummary_Result> summary_Results = new List<SP_IDX_DeliverySummary_Result>();

            vssp.Database.CommandTimeout = 0;
            summary_Results = vssp.SP_IDX_DeliverySummary(today,customerFilter,partnumberFilter).ToList();

            return Json(summary_Results, JsonRequestBehavior.AllowGet);

        }
        public ActionResult DeliverySummaryDetailJson(DateTime today, string customerFilter, string partnumberFilter)
        {

            if (customerFilter == "") customerFilter = null;
            if (partnumberFilter == "") partnumberFilter = null;
            List<SP_IDX_DeliverySummaryDetail_Result> summaryDetail_Results = new List<SP_IDX_DeliverySummaryDetail_Result>();

            vssp.Database.CommandTimeout = 0;
            summaryDetail_Results = vssp.SP_IDX_DeliverySummaryDetail(today, customerFilter, partnumberFilter).ToList();

            return Json(summaryDetail_Results, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ReceivingSummaryJson(DateTime today, string supplierFilter, string partnumberFilter)
        {
            vssp.Database.CommandTimeout = 0;

            if (supplierFilter == "") supplierFilter = null;
            if (partnumberFilter == "") partnumberFilter = null;
            List<SP_IDX_ReceivingSummary_Result> summary_Results = new List<SP_IDX_ReceivingSummary_Result>();

            summary_Results = vssp.SP_IDX_ReceivingSummary(today, supplierFilter, partnumberFilter).ToList();

            return Json(summary_Results, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ReceivingSummaryDetailJson(DateTime today, string supplierFilter, string partnumberFilter, int statusFilter)
        {
            vssp.Database.CommandTimeout = 0;

            if (supplierFilter == "") supplierFilter = null;
            if (partnumberFilter == "") partnumberFilter = null;
            List<SP_IDX_ReceivingSummaryDetail_Result> summaryDetail_Results = new List<SP_IDX_ReceivingSummaryDetail_Result>();

            summaryDetail_Results = vssp.SP_IDX_ReceivingSummaryDetail(today, supplierFilter, partnumberFilter).ToList();

            switch (statusFilter)
            {
                case 0:
                    //nothing
                    break;
                case 1:
                    summaryDetail_Results = summaryDetail_Results.Where(a => a.OutstandingQty < 0).ToList();
                    break;
                case 2:
                    summaryDetail_Results = summaryDetail_Results.Where(a => a.OutstandingQty > 0).ToList();
                    break;
            }
            return Json(summaryDetail_Results, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ProductionSummaryJson(DateTime today, string customerFilter, string partnumberFilter)
        {

            if (customerFilter == "") customerFilter = null;
            if (partnumberFilter == "") partnumberFilter = null;
            List<SP_IDX_ProductionSummary_Result> summary_Results = new List<SP_IDX_ProductionSummary_Result>();

            vssp.Database.CommandTimeout = 0;
            summary_Results = vssp.SP_IDX_ProductionSummary(today, customerFilter, partnumberFilter).ToList();

            return Json(summary_Results, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ProductionSummaryDetailJson(DateTime today, string customerFilter, string partnumberFilter)
        {

            if (customerFilter == "") customerFilter = null;
            if (partnumberFilter == "") partnumberFilter = null;
            List<SP_IDX_ProductionSummaryDetail_Result> summaryDetail_Results = new List<SP_IDX_ProductionSummaryDetail_Result>();

            vssp.Database.CommandTimeout = 0;
            summaryDetail_Results = vssp.SP_IDX_ProductionSummaryDetail(today, customerFilter, partnumberFilter).ToList();

            return Json(summaryDetail_Results, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockListSummaryJson()
        {
            vssp.Database.CommandTimeout = 0;
            List<Vw_IDX_StockListSummary> stockListSummaries = (from a in vssp.Vw_IDX_StockListSummary
                                                                orderby a.StockType
                                                                select a).ToList();

            return Json(stockListSummaries, JsonRequestBehavior.AllowGet);

        }
        public ActionResult StockListDetailJson(string stocktype, string owner, string part)
        {

            vssp.Database.CommandTimeout = 0;
            stocktype = systemService.Vf(stocktype);
            owner = systemService.Vf(owner);
            part = systemService.Vf(part);
            var stockList = (from a in vssp.Vw_TRS_StockList
                             where a.StockKanban < a.MinStock || a.StockKanban > a.MaxStock
                             orderby a.StockType, a.CustomerId, a.LineId, a.SupplierId
                             select a).ToList();

            if (stocktype != "")
            {
                stockList = stockList.Where(a => a.StockType == stocktype).ToList();
            }
            if (owner != "")
            {
                stockList = stockList.Where(a => (a.CustomerId == owner || a.LineId == owner || a.SupplierId == owner)).ToList();
            }
            if (part != "")
            {
                stockList = stockList.Where(a => (a.UniqueNumber == part || a.PartNumber == part || a.PartName == part)).ToList();
            }

            //stockList = stockList.ToList().Skip((page - 1) * pageSize).Take(pageSize);

            return Json(stockList, JsonRequestBehavior.AllowGet);

        }
        public ActionResult QualityInspectionIncomingBrandCardJson(string monthyear)
        {

            List<SP_IDX_QualityInspectionIncoming_Result> SummaryMaster = new List<SP_IDX_QualityInspectionIncoming_Result>();

            if (systemService.Vf(monthyear) != "")
            {
                string[] monthyearArr = monthyear.Split('/');
                string month = monthyearArr[0];
                string years = monthyearArr[1];

                vssp.Database.CommandTimeout = 0;
                SummaryMaster = vssp.SP_IDX_QualityInspectionIncoming(years, month).ToList();

            }

            return Json(SummaryMaster, JsonRequestBehavior.AllowGet);

        }
        public ActionResult QualityInspectionBrandCardJson(string monthyear)
        {

            List<SP_IDX_QualityInspection_Result> SummaryMaster = new List<SP_IDX_QualityInspection_Result>();

            if (systemService.Vf(monthyear) != "")
            {
                string[] monthyearArr = monthyear.Split('/');
                string month = monthyearArr[0];
                string years = monthyearArr[1];

                vssp.Database.CommandTimeout = 0;
                SummaryMaster = vssp.SP_IDX_QualityInspection(years, month).ToList();

            }

            return Json(SummaryMaster, JsonRequestBehavior.AllowGet);

        }
        public ActionResult top10DefectChartJson(string monthyear, string inspectiontype)
        {

            List<SP_IDX_QualityInspectionDefects_Result> top10Defect = new List<SP_IDX_QualityInspectionDefects_Result>();

            if (systemService.Vf(monthyear) != "")
            {
                string[] monthyearArr = monthyear.Split('/');
                string month = monthyearArr[0];
                string years = monthyearArr[1];

                vssp.Database.CommandTimeout = 0;
                top10Defect = vssp.SP_IDX_QualityInspectionDefects(years, month, inspectiontype).OrderByDescending(a=> a.DefectQty).Take(10).ToList();

            }

            return Json(top10Defect, JsonRequestBehavior.AllowGet);

        }
        public ActionResult defectListJson(string inspectiontype)
        {

            inspectiontype = systemService.Vf(inspectiontype);

            vssp.Database.CommandTimeout = 0;
            DateTime today = DateTime.Today;
            var defectList = (from a in vssp.Vw_QC_InspectionDefects
                              where a.InspectionDate == today && a.InspectionType.Contains(inspectiontype)
                              orderby a.InspectionDate descending, a.StartTime descending
                              select a).ToList();

            return Json(defectList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult problemInformationListJson(string inspectiontype)
        {
            inspectiontype = systemService.Vf(inspectiontype);
            vssp.Database.CommandTimeout = 0;
            DateTime today = DateTime.Today;
            var problemInformationList = (from a in vssp.Tbl_QC_ProblemInformation
                                          join b in vssp.Tbl_TRS_Status on a.Status equals b.Id into status
                                          from b in status.DefaultIfEmpty()
                                          join c in vssp.Tbl_MST_InspectionGate on a.InspectionGate equals c.GateId into gate
                                          from c in gate.DefaultIfEmpty()
                                          where a.ProblemDate == today && c.InspectionType.Contains(inspectiontype)
                                          orderby a.ProblemDate descending, a.ProblemTime descending
                                          select new { a.ProblemNumber, a.ProblemDate, a.ProblemTime, a.InspectionGate, a.ProblemInformation, a.FollowUp, a.Status, StatusName = b.Name }).ToList();

            return Json(problemInformationList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetProductionDailySummary(string lineid, string gateid, string shiftid, DateTime proddate)
        {
            var prodsummary = vssp.SP_GET_ProductionDailyShiftSummary(lineid, gateid, shiftid, proddate).ToList();

            return PartialView("Dashboard/_ProductionSummaryPartial/ViewCards", prodsummary);

        }

    }
}