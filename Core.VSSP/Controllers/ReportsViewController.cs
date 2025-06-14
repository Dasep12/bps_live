using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.WorkEntity;
using Core.VSSP.Services;
using Core.VSSP.Models;
using Core.VSSP.Controllers;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Data.Linq;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Net;
using System.Collections;

namespace Core.VSSP.Controllers
{
    public class ReportsViewController : Controller
    {
        // GET: ReportsView
        vssp_entity           vssp                = new vssp_entity();
        AccountService        accountService      = new AccountService();
        SystemService         systemService       = new SystemService();
        CryptoLibService      cryptoLibService    = new CryptoLibService();
        DynamicLinqController dynamicLinq         = new DynamicLinqController();
        
        public ActionResult Index(string group, string category)
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string utype = Session["UserType"].ToString();
                string action = "Index?group=" + group + "&category=" + category + "";
                var acccessPreviliege = accountService.AccessPreviliege(uid, "ReportsView", action);
                var menu = systemService.SidebarEditList(acccessPreviliege.MenuID==null? "": acccessPreviliege.MenuID).FirstOrDefault();
                var mainmenu = systemService.SidebarEditList(menu==null? "": menu.ParrentID).FirstOrDefault();
                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                } else 
                if (menu.Confidential==true && acccessPreviliege.ConfidentialAccess == false)
                {
                    return RedirectToAction("ConfidentialAccess", "System");
                }
                else
                {
                    ViewBag.MenuId = systemService.Vf(acccessPreviliege.MenuID);
                    ViewBag.Title = "Report " + systemService.Vf(mainmenu.MenuName) + " " + systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.Group = group;
                    ViewBag.Category = category;
                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = systemService.ComboExport(true).ToList();

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
        public ActionResult ReportSettings()
        {
            if (Session["UserID"] != null)
            {

                ViewBag.Title = "Report Settings";
                return View(systemService.SidebarList("", "MN-0005").ToList()); // redirecting to all Employees List

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public DbContext GetTable(string TableName)
        {
            var table = vssp.GetType().GetProperty(TableName).GetValue(vssp) as DbContext;
            return table;
        }
        [ValidateInput(false)]
        public ActionResult PrintReport(string jsonData)
        {
            PostPrintModel postPrint = JsonConvert.DeserializeObject<PostPrintModel>(jsonData);
            PrintOptionModel printOption = postPrint.PrintOption;
            List<FilterDataModel> filterData = postPrint.FilterData;
            MenuReportListModel menuReportList = systemService.MenuReportList(printOption.MenuId).FirstOrDefault();
            vssp.Database.CommandTimeout = 0;

            if(filterData != null || menuReportList != null) { 
                try { 
                    string sqlQuery = dynamicLinq.queryBuilder(printOption, filterData, menuReportList);

                    //var table = vssp.GetType().GetProperty(menuReportList.SchemaName).GetValue(vssp);
                    var table = new object();
                    string database = vssp.Database.Connection.ConnectionString;
                    string[] builders = database.Split(';');
                    string[] datasource = builders[0].Split('=');
                    string[] catalog = builders[1].Split('=');
                    string[] userid = builders[3].Split('=');
                    string[] password = builders[4].Split('=');

                    var connetionString = "Data Source=" + datasource[1] + ";Initial Catalog=" + catalog[1] + ";User ID=" + userid[1] + ";Password=" + password[1] + "";
                    List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();
                    var CompanyProfile = systemService.GetLicenseInfo();
                    if (CompanyProfile != null)
                    {
                        ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                        _ReportCompanyProfile.CompId = CompanyProfile.ID;
                        _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
                        _ReportCompanyProfile.CompCity = CompanyProfile.City;
                        _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                        reportCompanyProfile.Add(_ReportCompanyProfile);
                    }
                    switch (printOption.GroupReport)
                    {
                        case "marketing":
                            switch (printOption.CategoryReport.ToLower())
                            {
                                case "salescategory":
                                    table = SalesCategoryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "salesorder":
                                    table = SalesOrderBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "salesdelivery":
                                    table = SalesDeliveryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "sumsalesorder":
                                    table = SalesOrderSummaryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "sumsalesdelivery":
                                    table = SalesDeliverySummaryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "yearlysalesorder":
                                    table = SalesOrderYearlyBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "yearlysalesdelivery":
                                    table = SalesDeliveryYearlyBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "achievement":
                                    table = SalesAchievementBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "finishgoodsprice":
                                    table = FinishGoodsPriceBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                default:
                                    table = null;
                                    break;
                            }
                            break;
                        case "logistic":
                            switch (printOption.CategoryReport.ToLower())
                            {
                                case "materialinout":
                                    table = MaterialInOutBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "shortage":
                                    table = MaterialShortageBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "orderdaily":
                                    table = SupplierOrderRecapBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "orderyearly":
                                    table = SupplierOrderYearlyBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "receivingdaily":
                                    table = ReceiveOrderRecapBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "receivingyearly":
                                    table = ReceiveOrderYearlyBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "forecastachievement":
                                    table = ForecastAchievementBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "forecastachievementsummary":
                                    table = ForecastAchievementSummaryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "supplierperformance":
                                    table = SupplierPerformanceBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "stockcontrolrawmaterial":
                                    table = StockControlRawMaterialBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                default:
                                    table = null;
                                    break;
                            }
                            break;
                        case "delivery":
                            switch (printOption.CategoryReport.ToLower())
                            {
                                case "dailyrecap":
                                    table = DeliveryOrderRecapBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "yearlyrecap":
                                    table = SalesDeliveryYearlyBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "achievementrecap":
                                    table = DeliveryAchievementBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "achievementsummary":
                                    table = DeliveryAchievementSummaryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "outstanding":
                                    table = DeliveryOutstandingBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "stocklevelfg":
                                    table = StockLevelFGBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                default:
                                    table = null;
                                    break;
                            }
                            break;
                        case "finacc":
                            switch (printOption.CategoryReport.ToLower())
                            {
                                case "supplierinvoicerecap":
                                    table = InvoiceReceivingRecapBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "suppliersummaryinvoice":
                                    table = InvoiceReceivingSummaryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "customerinvoicerecap":
                                    table = InvoiceDeliveryRecapBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "customersummaryinvoice":
                                    table = InvoiceDeliverySummaryBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "rawmaterialsprice":
                                    table = RawMaterialsPriceBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "rawmaterialspricemonthly":
                                    table = RawMaterialsPriceMonthlyBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "financestockreport":
                                    table = FinanceStockBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;

                                default:
                                    table = null;
                                    break;
                            }
                            break;
                        case "productions":
                            switch (printOption.CategoryReport.ToLower())
                            {
                                case "dailyrecap":
                                    table = ProductionProcessRecapBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "yearlyrecap":
                                    table = ProductionYearlyBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "achievementrecap":
                                    table = ProductionAchievementBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                default:
                                    table = null;
                                    break;
                            }
                            break;
                        case "qualityincoming":
                            switch (printOption.CategoryReport.ToLower())
                            {
                                case "defectbygate":
                                    table = QualityDefectByGateBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "paretodefectpart":
                                    table = QualityParetoDefectPartBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "efficiencyinspector":
                                    table = QualityEfficiencyInspectorBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "top10defect":
                                    sqlQuery = sqlQuery.Replace("*", "Top 10 *");
                                    table = QualityTop10DefectBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "top10worstsupplier":
                                    sqlQuery = sqlQuery.Replace("*", "Top 10 *");
                                    table = QualityTop10WorstSupplierIncomingBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                default:
                                    table = null;
                                    break;
                            }
                            break;
                        case "qualityoutgoing":
                            switch (printOption.CategoryReport.ToLower())
                            {
                                case "defectbygate":
                                    table = QualityDefectByGateBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "paretodefectpart":
                                    table = QualityParetoDefectPartBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "efficiencyinspector":
                                    table = QualityEfficiencyInspectorBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "top10defect":
                                    sqlQuery = sqlQuery.Replace("*", "Top 10 *");
                                    table = QualityTop10DefectBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                case "top10worstsupplier":
                                    sqlQuery = sqlQuery.Replace("*", "Top 10 *");
                                    table = QualityTop10WorstSupplierBuilder(connetionString, sqlQuery, menuReportList.SchemaName);
                                    break;
                                default:
                                    table = null;
                                    break;
                            }
                            break;
                        default:
                            table = null;
                            break;

                    }

                    ReportDocument rd = new ReportDocument();

                    if (table == null)
                    {
                        rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
                    }
                    else
                    {
                        var property = typeof(ICollection).GetProperty("Count");
                        int tablecount = (int)property.GetValue(table, null);

                        if (tablecount == 0)
                        {
                            rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
                        }
                        else { 
                            rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), menuReportList.FileName));
                            rd.Database.Tables[0].SetDataSource(reportCompanyProfile);
                            rd.Database.Tables[1].SetDataSource(table);
                        }
                    }

                    if (printOption.ToPrinter == false)
                    {
                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();
                        if (printOption.FileFormat.ToLower() == "pdf")
                        {
                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            rd.Close();
                            rd.Dispose();
                            GC.Collect();

                            stream.Seek(0, SeekOrigin.Begin);

                            return File(stream, "application/pdf");
                        }
                        else
                        {
                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.ExcelRecord);
                            rd.Close();
                            rd.Dispose();
                            GC.Collect();

                            stream.Seek(0, SeekOrigin.Begin);
                            return File(stream, "application/vnd.ms-excel");
                        }
                    }
                    else
                    {
                        //rd.PrintOptions.PrinterName = "\\ ";
                        rd.PrintToPrinter(1, true, 1, 1);
                        rd.Close();
                        rd.Dispose();
                        GC.Collect();
                        return new HttpStatusCodeResult(204); ;
                    }
                    //var jsonResult = Json(table, JsonRequestBehavior.AllowGet);
                    //jsonResult.MaxJsonLength = int.MaxValue;
                    //return jsonResult;

                } catch(Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);

                    if (errinfo.Contains("Error in File"))
                    {
                        ReportDocument rd = new ReportDocument();
                        rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
                        TextObject txt;
                        txt = (TextObject)rd.ReportDefinition.ReportObjects["TextError"];
                        txt.Text = errinfo.Replace("vssp","") + ". Please contact your administrators";
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        rd.Close();
                        rd.Dispose();
                        GC.Collect();

                        stream.Seek(0, SeekOrigin.Begin);

                        return File(stream, "application/pdf");

                    }
                    else
                    {
                        return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = HttpUtility.HtmlEncode(errinfo.ToString())});
                    }
                }
            } else
            {
                return RedirectToAction("ErrorPage", "System", new { errnumber = "100", errmessage = "No report configuration found" });
            }
        }

        public List<ReportSalesDelivery> SalesCategoryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesDelivery> salesCategoryBuilder = new List<ReportSalesDelivery>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesCategoryBuilder = CommonMethod.ConvertToList<ReportSalesDelivery>(dataTable);
            }

            return salesCategoryBuilder;

        }

        public List<ReportSalesOrder> SalesOrderBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesOrder> salesOrderBuilder = new List<ReportSalesOrder>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesOrderBuilder = CommonMethod.ConvertToList<ReportSalesOrder>(dataTable);
            }

            return salesOrderBuilder;

        }
        public List<ReportSalesDelivery> SalesDeliveryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesDelivery> salesDeliveryBuilder = new List<ReportSalesDelivery>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesDeliveryBuilder = CommonMethod.ConvertToList<ReportSalesDelivery>(dataTable);
            }

            return salesDeliveryBuilder;

        }
        public List<ReportSalesOrderSummary> SalesOrderSummaryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesOrderSummary> salesOrderSummary = new List<ReportSalesOrderSummary>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesOrderSummary = CommonMethod.ConvertToList<ReportSalesOrderSummary>(dataTable);
            }

            return salesOrderSummary;

        }
        public List<ReportSalesDeliverySummary> SalesDeliverySummaryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesDeliverySummary> salesDeliverySummaryBuilder = new List<ReportSalesDeliverySummary>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesDeliverySummaryBuilder = CommonMethod.ConvertToList<ReportSalesDeliverySummary>(dataTable);
            }

            return salesDeliverySummaryBuilder;

        }
        public List<ReportSalesOrderYearly> SalesOrderYearlyBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesOrderYearly> salesOrderYearly = new List<ReportSalesOrderYearly>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesOrderYearly = CommonMethod.ConvertToList<ReportSalesOrderYearly>(dataTable);
            }

            return salesOrderYearly;

        }
        public List<ReportSalesDeliveryYearly> SalesDeliveryYearlyBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesDeliveryYearly> salesDeliveryYearly = new List<ReportSalesDeliveryYearly>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesDeliveryYearly = CommonMethod.ConvertToList<ReportSalesDeliveryYearly>(dataTable);
            }

            return salesDeliveryYearly;

        }
        public List<ReportSalesAchievement> SalesAchievementBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesAchievement> salesAchievementBuilder = new List<ReportSalesAchievement>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesAchievementBuilder = CommonMethod.ConvertToList<ReportSalesAchievement>(dataTable);
            }

            return salesAchievementBuilder;

        }
        
        public List<ReportPartFinishGoodsPrice> FinishGoodsPriceBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportPartFinishGoodsPrice> finishGoodsPriceBuilder = new List<ReportPartFinishGoodsPrice>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                finishGoodsPriceBuilder = CommonMethod.ConvertToList<ReportPartFinishGoodsPrice>(dataTable);
            }

            return finishGoodsPriceBuilder;

        }
        public List<SP_TRS_RawMaterialTransaction_Result> MaterialInOutBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<SP_TRS_RawMaterialTransaction_Result> materialInOutBuilder = new List<SP_TRS_RawMaterialTransaction_Result>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                materialInOutBuilder = CommonMethod.ConvertToList<SP_TRS_RawMaterialTransaction_Result>(dataTable);
            }

            return materialInOutBuilder;

        }
        public List<ReportSupplierOrderShortage> MaterialShortageBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSupplierOrderShortage> materialShortageBuilder = new List<ReportSupplierOrderShortage>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                materialShortageBuilder = CommonMethod.ConvertToList<ReportSupplierOrderShortage>(dataTable);
            }

            return materialShortageBuilder;

        }
        public List<ReportSupplierOrderRecap> SupplierOrderRecapBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSupplierOrderRecap> SupplierOrderRecap = new List<ReportSupplierOrderRecap>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                SupplierOrderRecap = CommonMethod.ConvertToList<ReportSupplierOrderRecap>(dataTable);
            }

            return SupplierOrderRecap;

        }
        public List<ReportSupplierOrderYearly> SupplierOrderYearlyBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSupplierOrderYearly> SupplierOrderYearly = new List<ReportSupplierOrderYearly>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                SupplierOrderYearly = CommonMethod.ConvertToList<ReportSupplierOrderYearly>(dataTable);
            }

            return SupplierOrderYearly;

        }
        public List<ReportReceiveOrderRecap> ReceiveOrderRecapBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportReceiveOrderRecap> ReceiveOrderRecap = new List<ReportReceiveOrderRecap>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ReceiveOrderRecap = CommonMethod.ConvertToList<ReportReceiveOrderRecap>(dataTable);
            }

            return ReceiveOrderRecap;

        }
        public List<ReportReceiveOrderYearly> ReceiveOrderYearlyBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportReceiveOrderYearly> ReceiveOrderYearly = new List<ReportReceiveOrderYearly>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ReceiveOrderYearly = CommonMethod.ConvertToList<ReportReceiveOrderYearly>(dataTable);
            }

            return ReceiveOrderYearly;

        }
        public List<ReportAchievementForecast> ForecastAchievementBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportAchievementForecast> ForecastAchievement = new List<ReportAchievementForecast>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ForecastAchievement = CommonMethod.ConvertToList<ReportAchievementForecast>(dataTable);
            }

            return ForecastAchievement;

        }
        public List<ReportForecastAchievementModel> ForecastAchievementSummaryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportForecastAchievementModel> ForecastAchievement = new List<ReportForecastAchievementModel>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ForecastAchievement = CommonMethod.ConvertToList<ReportForecastAchievementModel>(dataTable);
            }

            return ForecastAchievement;

        }
        public List<SupplierOrderPerformance> SupplierPerformanceBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<SupplierOrderPerformance> supplierPerformanceBuilder = new List<SupplierOrderPerformance>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                supplierPerformanceBuilder = CommonMethod.ConvertToList<SupplierOrderPerformance>(dataTable);
            }

            return supplierPerformanceBuilder;

        }
        public List<ReportStockControlModel> StockControlRawMaterialBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportStockControlModel> StockControl = new List<ReportStockControlModel>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                StockControl = CommonMethod.ConvertToList<ReportStockControlModel>(dataTable);
            }

            return StockControl;

        }

        public List<ReportDeliveryOrderRecap> DeliveryOrderRecapBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportDeliveryOrderRecap> deliveryOrderRecap = new List<ReportDeliveryOrderRecap>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                deliveryOrderRecap = CommonMethod.ConvertToList<ReportDeliveryOrderRecap>(dataTable);
            }

            return deliveryOrderRecap;

        }
        public List<ReportAchievementDelivery> DeliveryAchievementBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportAchievementDelivery> deliveryAchievement = new List<ReportAchievementDelivery>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                deliveryAchievement = CommonMethod.ConvertToList<ReportAchievementDelivery>(dataTable);
            }

            return deliveryAchievement;

        }
        public List<ReportSalesForecastAchiementModel> DeliveryAchievementSummaryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSalesForecastAchiementModel> salesAchievementBuilder = new List<ReportSalesForecastAchiementModel>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                salesAchievementBuilder = CommonMethod.ConvertToList<ReportSalesForecastAchiementModel>(dataTable);
            }

            return salesAchievementBuilder;

        }
        public List<ReportOutstandingDelivery> DeliveryOutstandingBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportOutstandingDelivery> deliveryOutstanding = new List<ReportOutstandingDelivery>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                deliveryOutstanding = CommonMethod.ConvertToList<ReportOutstandingDelivery>(dataTable);
            }

            return deliveryOutstanding;

        }
        public List<ReportStockLevelFGModel> StockLevelFGBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportStockLevelFGModel> StockLevelFG = new List<ReportStockLevelFGModel>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                StockLevelFG = CommonMethod.ConvertToList<ReportStockLevelFGModel>(dataTable);
            }

            return StockLevelFG;

        }

        public List<ReportSupplierInvoiceReceivingRecap> InvoiceReceivingRecapBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSupplierInvoiceReceivingRecap> invoiceReceivingRecap = new List<ReportSupplierInvoiceReceivingRecap>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                invoiceReceivingRecap = CommonMethod.ConvertToList<ReportSupplierInvoiceReceivingRecap>(dataTable);
            }

            return invoiceReceivingRecap;

        }
        public List<ReportSupplierInvoiceReceivingSummary> InvoiceReceivingSummaryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportSupplierInvoiceReceivingSummary> invoiceReceivingSummary = new List<ReportSupplierInvoiceReceivingSummary>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                invoiceReceivingSummary = CommonMethod.ConvertToList<ReportSupplierInvoiceReceivingSummary>(dataTable);
            }

            return invoiceReceivingSummary;

        }
        public List<ReportCustomerInvoiceDeliveryRecap> InvoiceDeliveryRecapBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportCustomerInvoiceDeliveryRecap> invoiceDeliveryRecap = new List<ReportCustomerInvoiceDeliveryRecap>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                invoiceDeliveryRecap = CommonMethod.ConvertToList<ReportCustomerInvoiceDeliveryRecap>(dataTable);
            }

            return invoiceDeliveryRecap;

        }
        public List<ReportCustomerInvoiceDeliverySummary> InvoiceDeliverySummaryBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportCustomerInvoiceDeliverySummary> invoiceDeliverySummary = new List<ReportCustomerInvoiceDeliverySummary>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                invoiceDeliverySummary = CommonMethod.ConvertToList<ReportCustomerInvoiceDeliverySummary>(dataTable);
            }

            return invoiceDeliverySummary;

        }
        public List<ReportPartRawMaterialsPrice> RawMaterialsPriceBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportPartRawMaterialsPrice> rawMaterialsPriceBuilder = new List<ReportPartRawMaterialsPrice>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                rawMaterialsPriceBuilder = CommonMethod.ConvertToList<ReportPartRawMaterialsPrice>(dataTable);
            }

            return rawMaterialsPriceBuilder;

        }
        public List<ReportPriceListRawMaterialsModel> RawMaterialsPriceMonthlyBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportPriceListRawMaterialsModel> ReportPriceListRawMaterials = new List<ReportPriceListRawMaterialsModel>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ReportPriceListRawMaterials = CommonMethod.ConvertToList<ReportPriceListRawMaterialsModel>(dataTable);
            }

            return ReportPriceListRawMaterials;

        }
        public List<ReportFinanceStockModel> FinanceStockBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportFinanceStockModel> ReportFinanceStock = new List<ReportFinanceStockModel>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ReportFinanceStock = CommonMethod.ConvertToList<ReportFinanceStockModel>(dataTable);
            }

            return ReportFinanceStock;

        }
        public List<ReportProductionRecap> ProductionProcessRecapBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportProductionRecap> ProductionOrderRecap = new List<ReportProductionRecap>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ProductionOrderRecap = CommonMethod.ConvertToList<ReportProductionRecap>(dataTable);
            }

            return ProductionOrderRecap;

        }
        public List<ReportAchievementProduction> ProductionAchievementBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportAchievementProduction> ProductionAchievement = new List<ReportAchievementProduction>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                ProductionAchievement = CommonMethod.ConvertToList<ReportAchievementProduction>(dataTable);
            }

            return ProductionAchievement;

        }
        public List<ReportProductionYearly> ProductionYearlyBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportProductionYearly> prodYearly = new List<ReportProductionYearly>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                var Yearly = CommonMethod.ConvertToList<ReportProductionYearly>(dataTable);
                prodYearly = Yearly;
            }


            return prodYearly;

        }

        public List<ReportDefectInGate> QualityDefectByGateBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportDefectInGate> qualityDefectByGate = new List<ReportDefectInGate>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                qualityDefectByGate = CommonMethod.ConvertToList<ReportDefectInGate>(dataTable);
            }

            return qualityDefectByGate;

        }
        public List<ReportParetoDefectPart> QualityParetoDefectPartBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportParetoDefectPart> qualityParetoDefectPart = new List<ReportParetoDefectPart>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                qualityParetoDefectPart = CommonMethod.ConvertToList<ReportParetoDefectPart>(dataTable);
            }

            return qualityParetoDefectPart;

        }
        public List<ReportEfficiencyInspector> QualityEfficiencyInspectorBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportEfficiencyInspector> qualityEfficiencyInspector = new List<ReportEfficiencyInspector>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                qualityEfficiencyInspector = CommonMethod.ConvertToList<ReportEfficiencyInspector>(dataTable);
            }

            return qualityEfficiencyInspector;

        }

        public List<ReportTopDefect> QualityTop10DefectBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportTopDefect> QualityTop10Defect = new List<ReportTopDefect>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                QualityTop10Defect = CommonMethod.ConvertToList<ReportTopDefect>(dataTable);
            }

            return QualityTop10Defect;

        }
        public List<ReportTopWorstSupplierIncoming> QualityTop10WorstSupplierIncomingBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportTopWorstSupplierIncoming> QualityTop10WorstSupplier = new List<ReportTopWorstSupplierIncoming>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                QualityTop10WorstSupplier = CommonMethod.ConvertToList<ReportTopWorstSupplierIncoming>(dataTable);
            }

            return QualityTop10WorstSupplier;

        }
        public List<ReportTopWorstSupplier> QualityTop10WorstSupplierBuilder(string constr, string sqlQuery, string schemaName)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            List<ReportTopWorstSupplier> QualityTop10WorstSupplier = new List<ReportTopWorstSupplier>();

            using (var cnn = new SqlConnection(constr))
            {
                cnn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, cnn);
                sda.SelectCommand.CommandTimeout = 0;
                sda.SelectCommand.CommandType = CommandType.Text;
                sda.Fill(dataSet, schemaName);

                var data = dataSet.Tables[0];
                dataTable = data; // Call BusinessLogic to fill DataTable, Here your ResultDT will get the result in which you will be having single or multiple rows with columns "StudentId,RoleNumber and Name"  
                QualityTop10WorstSupplier = CommonMethod.ConvertToList<ReportTopWorstSupplier>(dataTable);
            }

            return QualityTop10WorstSupplier;

        }
        public ActionResult RawMaterialMovement()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = accountService.AccessPreviliege(uid, "Suppliers", "RawMaterials");

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
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = systemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    var stockTakingEvent = systemService.GetStockTakingEvent();

                    if (stockTakingEvent != null && stockTakingEvent.InventoryStatus.Contains("in progress"))
                    {
                        ViewBag.Messages = stockTakingEvent.InventoryStatus;
                    }
                    else
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";

                    }

                    return View(exportOption);

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult RawMaterialMovementListJson(string SupplierId,string PartNumber, Nullable<DateTime> StartDate, Nullable<DateTime> EndDate)
        {

            if (StartDate == null)
            {
                StartDate = DateTime.Now;
            }
            if (EndDate == null)
            {
                EndDate = DateTime.Now;
            }
            var RawMaterials = vssp.SP_TRS_RawMaterialTransaction(SupplierId, PartNumber, StartDate, EndDate).ToList();
            return Json(RawMaterials, JsonRequestBehavior.AllowGet);

            
        }
    }
}