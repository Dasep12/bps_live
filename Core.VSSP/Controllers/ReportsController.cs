using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Controllers
{
    public class ReportsController : Controller
    {
        // GET: Reports
        vssp_entity vssp = new vssp_entity();
        SystemService systemService = new SystemService();
        AccountService accountService = new AccountService();
        PurchaseController purchaseController = new PurchaseController();
        QRCodeGeneratorController QRCode = new QRCodeGeneratorController();

        public ActionResult DeliveryOrders(string donumber, string toprinter = "false", bool import = false)
        {

            List<Vw_TRS_DeliveryOrder> DeliveryOrder = (from a in vssp.Vw_TRS_DeliveryOrder
                                                        where a.DONumber.Replace("/", "") == donumber || a.DONumber == donumber
                                                        select a).ToList();

            List<Vw_TRS_DeliveryOrderDetail> DeliveryOrderDetail = (from a in vssp.Vw_TRS_DeliveryOrderDetail
                                                                     where a.DONumber.Replace("/", "") == donumber || a.DONumber == donumber
                                                                     orderby a.PartNumber
                                                                    select a).ToList();

            if (import == true)
            {
                DeliveryOrder = DeliveryOrder.Where(a => a.Status == 6).ToList();
                DeliveryOrderDetail = DeliveryOrderDetail.Where(a => a.Status == 6).ToList();
            }
            else
            {
                DeliveryOrder = DeliveryOrder.Where(a => a.Status != 6).ToList();
                DeliveryOrderDetail = DeliveryOrderDetail.Where(a => a.Status != 6).ToList();
            }

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportDeliveryOrderModel> reportDeliveryOrder = new List<ReportDeliveryOrderModel>();
            List<ReportDeliveryOrderDetailsModel> reportDeliveryOrderDetails = new List<ReportDeliveryOrderDetailsModel>();
            List<ReportDeliveryOrderAttentionModel> reportDeliveryOrderAttentions = new List<ReportDeliveryOrderAttentionModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            foreach(var orders in DeliveryOrder)
            {
                ReportDeliveryOrderModel deliveryOrder = new ReportDeliveryOrderModel();
                deliveryOrder.DONumber = orders.DONumber;
                deliveryOrder.DODate = Convert.ToDateTime(systemService.Vd(orders.DODate.ToString()));
                deliveryOrder.CustomerId = orders.CustomerId;
                deliveryOrder.CustomerName = orders.CustomerName;
                deliveryOrder.DeliveryAddress = orders.DeliveryAddress;
                deliveryOrder.PONumber = orders.PONumber;
                deliveryOrder.RefNumber = orders.RefNumber;
                deliveryOrder.TotalItem =  systemService.Vn(orders.TotalItem.ToString());
                deliveryOrder.TotalDelivery = systemService.Vn(orders.TotalDelivery.ToString());

                reportDeliveryOrder.Add(deliveryOrder);
            }
            foreach (var ordersetails in DeliveryOrderDetail)
            {
                ReportDeliveryOrderDetailsModel deliveryOrderdetail = new ReportDeliveryOrderDetailsModel();
                deliveryOrderdetail.DONumber = ordersetails.DONumber;
                deliveryOrderdetail.PartNumber = ordersetails.PartNumber;
                deliveryOrderdetail.PartNumberCustomer = ordersetails.PartNumberCustomer;
                deliveryOrderdetail.UniqueNumber = ordersetails.UniqueNumber;
                deliveryOrderdetail.PartName = ordersetails.PartName;
                deliveryOrderdetail.UnitsQty = systemService.Vn(ordersetails.DeliveryQty.ToString());
                deliveryOrderdetail.UnitLevel1 = ordersetails.UnitLevel1;
                deliveryOrderdetail.UnitQty = systemService.Vn(ordersetails.DeliveryUnitQty.ToString());
                deliveryOrderdetail.UnitLevel2 = ordersetails.UnitLevel2;
                deliveryOrderdetail.PriceUnit = 0; // systemService.Vn(ordersetails.PriceUnit.ToString());
                deliveryOrderdetail.PriceTotal = 0;// systemService.Vn(ordersetails.PriceTotal.ToString());
                deliveryOrderdetail.UserId = DeliveryOrder[0].UserId;
                deliveryOrderdetail.ImportDate = Convert.ToDateTime(systemService.Vd(DeliveryOrder[0].EditDate.ToString()));

                reportDeliveryOrderDetails.Add(deliveryOrderdetail);
            }
            if (DeliveryOrder.Count != 0)
            {
                var order = (from a in DeliveryOrder
                             select a).FirstOrDefault();

                var contacts = (from a in vssp.Tbl_MST_CustomerContact
                               where a.CustomerId == order.CustomerId && a.ReceiveOrder == true
                               select a).ToList();

                string attname = "";
                foreach(var contact in contacts)
                {
                    if (attname != "") attname += ", ";
                    attname = contact.ContactName;
                }

                ReportDeliveryOrderAttentionModel attentionModel = new ReportDeliveryOrderAttentionModel();
                attentionModel.AttentionName=attname;

                reportDeliveryOrderAttentions.Add(attentionModel);
            }
            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }
            ReportDocument rd = new ReportDocument();

            if (DeliveryOrder.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_007.rpt"));
                rd.Database.Tables[0].SetDataSource(reportDeliveryOrder);
                rd.Database.Tables[1].SetDataSource(reportDeliveryOrderDetails);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
                rd.Database.Tables[3].SetDataSource(reportDeliveryOrderAttentions);
            }

            if (toprinter == "false") { 
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                stream.Seek(0, SeekOrigin.Begin);

                return File(stream, "application/pdf");
            } else
            {
                //rd.PrintOptions.PrinterName = "\\ ";
                rd.PrintToPrinter(1, true, 1, 1);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                return new HttpStatusCodeResult(204); ;
            }
        }
        public ActionResult SalesOrders(string sonumber, string FileFormat)
        {

            List<Vw_TRS_SalesOrder> SalesOrder = (from a in vssp.Vw_TRS_SalesOrder
                                                        where a.SONumber == sonumber
                                                        select a).ToList();

            var SalesOrderDetail = from a in vssp.Tbl_TRS_SalesOrderDetail
                                   join b in vssp.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber }
                                   where a.SONumber == sonumber
                                   select new { a.SONumber, a.CustomerId, b.UniqueNumber, a.PartNumber, b.PartName, Model = b.CustomerUnitModel, QtyByKanban = b.UnitQty, Unit = b.UnitLevel2, a.OrderQty, a.OrderN1, a.OrderN2, a.OrderN3, a.DeliveryPerDay };

            List<ReportSalesOrderModel> reportSalesOrder = new List<ReportSalesOrderModel>();
            List<ReportSalesOrderDetailsModel> reportSalesOrderDetails = new List<ReportSalesOrderDetailsModel>();

            foreach (var orders in SalesOrder)
            {
                DateTimeFormatInfo mfi = new DateTimeFormatInfo();
                string strMonthName = mfi.GetMonthName(Convert.ToInt32(orders.DeliveryMonth)).ToString();

                ReportSalesOrderModel _ReportSalesOrder = new ReportSalesOrderModel();
                _ReportSalesOrder.SONumber = orders.SONumber;
                _ReportSalesOrder.SODate = Convert.ToDateTime(systemService.Vd(orders.SODate.ToString()));
                _ReportSalesOrder.CustomerId = orders.CustomerId;
                _ReportSalesOrder.CustomerName = orders.CustomerName;
                _ReportSalesOrder.PONumber = orders.PONumber;
                _ReportSalesOrder.PODate = Convert.ToDateTime(systemService.Vd(orders.PODate.ToString()));
                _ReportSalesOrder.ReceiveDate = Convert.ToDateTime(systemService.Vd(orders.ReceiveDate.ToString()));
                _ReportSalesOrder.PassThrough = systemService.Vb(orders.PassThrough.ToString());
                _ReportSalesOrder.DeliveryMonth = strMonthName;
                _ReportSalesOrder.DeliveryYear = orders.DeliveryYear;
                _ReportSalesOrder.Remarks = orders.Remarks;
                _ReportSalesOrder.Status = orders.StatusName;
                _ReportSalesOrder.UserId = orders.UserId;
                _ReportSalesOrder.EditDate = Convert.ToDateTime(systemService.Vd(orders.EditDate.ToString()));

                reportSalesOrder.Add(_ReportSalesOrder);
            }
            foreach (var orderdetails in SalesOrderDetail)
            {
                ReportSalesOrderDetailsModel _ReportSalesOrderdetail = new ReportSalesOrderDetailsModel();
                _ReportSalesOrderdetail.SONumber = orderdetails.SONumber;
                _ReportSalesOrderdetail.CustomerId = orderdetails.CustomerId;
                _ReportSalesOrderdetail.UniqueNumber = orderdetails.UniqueNumber;
                _ReportSalesOrderdetail.PartNumber = orderdetails.PartNumber;
                _ReportSalesOrderdetail.PartName = orderdetails.PartName;
                _ReportSalesOrderdetail.PartModel = orderdetails.Model;
                _ReportSalesOrderdetail.UnitQty = systemService.Vn(orderdetails.QtyByKanban.ToString());
                _ReportSalesOrderdetail.UnitLevel2 = orderdetails.Unit;
                _ReportSalesOrderdetail.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
                _ReportSalesOrderdetail.OrderN1 = systemService.Vn(orderdetails.OrderN1.ToString());
                _ReportSalesOrderdetail.OrderN2 = systemService.Vn(orderdetails.OrderN2.ToString());
                _ReportSalesOrderdetail.OrderN3 = systemService.Vn(orderdetails.OrderN3.ToString());
                _ReportSalesOrderdetail.DeliveryPerDay = systemService.Vn(orderdetails.DeliveryPerDay.ToString());

                reportSalesOrderDetails.Add(_ReportSalesOrderdetail);
            }
            ReportDocument rd = new ReportDocument();

            if (SalesOrder.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_001.rpt"));
                rd.Database.Tables[0].SetDataSource(reportSalesOrder);
                rd.Database.Tables[1].SetDataSource(reportSalesOrderDetails);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (FileFormat == "Pdf")
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
        public ActionResult ForecastOrders(string OrderNumber)
        {

            Vw_TRS_ForecastOrder ForecastOrder = (from a in vssp.Vw_TRS_ForecastOrder
                                                  where a.OrderNumber == OrderNumber
                                                  select a).First();

            var ForecastOrderDetail = vssp.SP_TRS_ForecastOrderDetailLast("x", "01", "2020", OrderNumber);

            var ForecastOrderApproval = from a in vssp.Tbl_TRS_ForecastOrderApproval
                                        where a.OrderNumber.Contains(OrderNumber)
                                        orderby a.ApprovalLevel
                                        select new { a.OrderNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

            var ForecastOrderRevision = from a in vssp.Tbl_TRS_ForecastOrderRevision
                                        where a.OrderNumber == OrderNumber
                                        orderby a.RevisionNumber
                                        select new { a.OrderNumber, a.RevisionNumber, a.Description, a.RevisionDate, a.RevisionBy };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportForecastOrderModel> reportForecastOrder = new List<ReportForecastOrderModel>();
            List<ReportForecastOrderDetailModel> reportForecastOrderDetails = new List<ReportForecastOrderDetailModel>();
            List<ReportForecastOrderApprovalModel> reportForecastOrderApproval = new List<ReportForecastOrderApprovalModel>();
            List<ReportForecastOrderRevisionModel> reportForecastOrderRevision = new List<ReportForecastOrderRevisionModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            DateTimeFormatInfo mfi = new DateTimeFormatInfo();
            string strMonthName = mfi.GetMonthName(Convert.ToInt32(ForecastOrder.OrderMonth)).ToString();

            ReportForecastOrderModel _reportForecastOrder = new ReportForecastOrderModel();
            _reportForecastOrder.OrderNumber = ForecastOrder.OrderNumber;
            _reportForecastOrder.OrderDate = Convert.ToDateTime(systemService.Vd(ForecastOrder.OrderDate.ToString()));
            _reportForecastOrder.OrderMonth = strMonthName;
            _reportForecastOrder.OrderYear = ForecastOrder.OrderYear;
            _reportForecastOrder.Shift = systemService.Vn(ForecastOrder.Shift.ToString());
            _reportForecastOrder.SupplierId = ForecastOrder.SupplierId;
            _reportForecastOrder.SupplierName = ForecastOrder.SupplierName;
            _reportForecastOrder.TotalPart = systemService.Vn(ForecastOrder.TotalPart.ToString());
            _reportForecastOrder.TotalOrder = systemService.Vn(ForecastOrder.TotalOrder.ToString());
            _reportForecastOrder.Status = systemService.Vn(ForecastOrder.Status.ToString());
            _reportForecastOrder.StatusName = ForecastOrder.StatusName;
            _reportForecastOrder.Approval = ForecastOrder.Approval;
            _reportForecastOrder.Revision = systemService.Vn(ForecastOrder.Revision.ToString());
            _reportForecastOrder.KanbanCycle = ForecastOrder.KanbanCycle;
            _reportForecastOrder.KanbanTime = ForecastOrder.KanbanTime;
            _reportForecastOrder.ApprovalLevel = systemService.Vn(ForecastOrder.ApprovalLevel.ToString());
            _reportForecastOrder.ApprovalName = ForecastOrder.ApprovalName;
            _reportForecastOrder.Remarks = ForecastOrder.Remarks;
            _reportForecastOrder.UserId = ForecastOrder.UserId;
            _reportForecastOrder.EditDate = Convert.ToDateTime(systemService.Vd(ForecastOrder.EditDate.ToString()));

            reportForecastOrder.Add(_reportForecastOrder);


            foreach (var orderdetails in ForecastOrderDetail)
            {
                ReportForecastOrderDetailModel _ReportForecastOrderdetail = new ReportForecastOrderDetailModel();
                _ReportForecastOrderdetail.OrderNumber = orderdetails.OrderNumber;
                _ReportForecastOrderdetail.SupplierId = orderdetails.SupplierId;
                _ReportForecastOrderdetail.UniqueNumber = orderdetails.UniqueNumber;
                _ReportForecastOrderdetail.PartNumber = orderdetails.PartNumber;
                _ReportForecastOrderdetail.PartName = orderdetails.PartName;
                _ReportForecastOrderdetail.Model = orderdetails.Model;
                _ReportForecastOrderdetail.QtyByKanban = systemService.Vn(orderdetails.QtyByKanban.ToString());
                _ReportForecastOrderdetail.Unit = orderdetails.Unit;
                _ReportForecastOrderdetail.DailyLastQty = systemService.Vn(orderdetails.DailyLastQty.ToString());
                _ReportForecastOrderdetail.DailyQty = systemService.Vn(orderdetails.DailyQty.ToString());
                _ReportForecastOrderdetail.OrderLastQty = systemService.Vn(orderdetails.OrderLastQty.ToString());
                _ReportForecastOrderdetail.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
                _ReportForecastOrderdetail.N1 = systemService.Vn(orderdetails.N1.ToString());
                _ReportForecastOrderdetail.N2 = systemService.Vn(orderdetails.N2.ToString());
                _ReportForecastOrderdetail.N3 = systemService.Vn(orderdetails.N3.ToString());
                _ReportForecastOrderdetail.FluctuationQty = systemService.Vn(orderdetails.FluctuationQty.ToString());
                _ReportForecastOrderdetail.FluctuationPercent = systemService.Vn(orderdetails.FluctuationPercent.ToString());

                reportForecastOrderDetails.Add(_ReportForecastOrderdetail);
            }

            /* Header Workday */
            string strmonth = ForecastOrder.OrderMonth + "/" + ForecastOrder.OrderYear;
            var workdayresult = purchaseController.ForecastWorkDayJson(strmonth);
            System.Reflection.PropertyInfo finalresult = workdayresult.GetType().GetProperty("Data");
            var workday = finalresult.GetValue(workdayresult, null);

            /* Approval */
            int checkuser = 1;
            ReportForecastOrderApprovalModel _reportForecastOrderApproval = new ReportForecastOrderApprovalModel();
            foreach (var orderapprovals in ForecastOrderApproval)
            {
                _reportForecastOrderApproval.OrderNumber = orderapprovals.OrderNumber;
                if (orderapprovals.ApprovedDate != null)
                {
                    _reportForecastOrderApproval.ApprovedDate = systemService.Vd(orderapprovals.ApprovedDate.ToString(),"dd-MMMM-yyyy");
                }
                switch (orderapprovals.ApprovalLevel.ToString())
                {
                    case "1":
                        _reportForecastOrderApproval.UserName1 = orderapprovals.UserName;
                        UserEditModel user1 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user1 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign1 = user1.Sign;
                            }
                        }
                        break;
                    case "2":
                        if (checkuser == 1)
                        {
                            _reportForecastOrderApproval.UserName2 = orderapprovals.UserName;
                            UserEditModel user2 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user2 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                _reportForecastOrderApproval.Sign2 = user2.Sign;
                                }
                            }
                            checkuser += 1;
                        } else
                        {
                            _reportForecastOrderApproval.UserName3 = orderapprovals.UserName;
                            UserEditModel user3 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user3 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportForecastOrderApproval.Sign3 = user3.Sign;
                                }
                            }
                        }

                        break;
                    case "3":
                        _reportForecastOrderApproval.UserName4 = orderapprovals.UserName;
                        UserEditModel user4 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user4 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign4 = user4.Sign;
                            }
                        }
                        break;
                    case "4":
                        _reportForecastOrderApproval.UserName5 = orderapprovals.UserName;
                        UserEditModel user5 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user5 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign5 = user5.Sign;
                            }
                        }
                        break;
                }

            }
            reportForecastOrderApproval.Add(_reportForecastOrderApproval);

            foreach (var orderRevisions in ForecastOrderRevision)
            {
                ReportForecastOrderRevisionModel _ReportForecastOrderRevision = new ReportForecastOrderRevisionModel();
                _ReportForecastOrderRevision.OrderNumber = orderRevisions.OrderNumber;
                _ReportForecastOrderRevision.RevisionNumber = systemService.Vn(orderRevisions.RevisionNumber.ToString());
                _ReportForecastOrderRevision.Description = orderRevisions.Description;
                _ReportForecastOrderRevision.RevisionDate = systemService.Vd(orderRevisions.RevisionDate.ToString());
                _ReportForecastOrderRevision.RevisionBy = orderRevisions.RevisionBy;

                reportForecastOrderRevision.Add(_ReportForecastOrderRevision);
            }

            int revision = 5;
            if (reportForecastOrderRevision.Count() < revision)
            {
                int loop = revision - reportForecastOrderRevision.Count();
                int x = 0;
                while (x < loop){
                    x += 1;
                    ReportForecastOrderRevisionModel _ReportForecastOrderRevision = new ReportForecastOrderRevisionModel();
                    _ReportForecastOrderRevision.OrderNumber = OrderNumber;
                    _ReportForecastOrderRevision.RevisionNumber = x;
                    reportForecastOrderRevision.Add(_ReportForecastOrderRevision);
                }
            }

            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (reportForecastOrder == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_002.rpt"));

                rd.Database.Tables[0].SetDataSource(reportForecastOrder);
                rd.Database.Tables[1].SetDataSource(reportForecastOrderDetails);
                rd.Database.Tables[2].SetDataSource(reportForecastOrderApproval);
                rd.Subreports[0].Database.Tables[0].SetDataSource(reportForecastOrderRevision);
                rd.Subreports[1].Database.Tables[0].SetDataSource(reportCompanyProfile);

                ForecastWorkdayModel workdayModel = (ForecastWorkdayModel)workday;

                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(workdayModel))
                {
                    string label = "Txt" + descriptor.Name;             // Name
                    object value = descriptor.GetValue(workdayModel);   // Value
                    try
                    {
                        TextObject rHeader = rd.ReportDefinition.ReportObjects[label] as TextObject;
                        rHeader.Text = (value != null ? value.ToString() : "");
                    } catch(Exception e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = systemService.GetExceptionDetails(e);
                        System.Diagnostics.Debug.WriteLine(label + " : " + errinfo);
                    }

                }

            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/pdf");

        }
        public ActionResult MasterListKanban(string OrderNumber)
        {

            Vw_TRS_ForecastOrder ForecastOrder = (from a in vssp.Vw_TRS_ForecastOrder
                                                  where a.OrderNumber == OrderNumber
                                                  select a).FirstOrDefault();

            var MLOK                    = from a in vssp.Tbl_TRS_MasterListKanbanSupplier
                                          join b in vssp.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber} equals new { b.SupplierId, b.PartNumber}
                                          where a.OrderNumber == OrderNumber
                                          select new { a.OrderNumber, a.SupplierId, a.PartNumber, b.UniqueNumber, b.PartName, b.PartModel, b.UnitLevel1, b.UnitLevel2, b.PackingId, b.LocationId, 
                                                        a.QtyByKanban, a.QtyByDay, a.KanbanN10, a.KanbanN00, a.KanbanN01, a.KanbanReg, a.SafetyHour, a.SafetyKanban, a.SafetyParts, a.WorkHour, a.Cycle1, a.Cycle2, a.Cycle3, a.MinStock, a.MaxStock };

            var ForecastOrderApproval   = from a in vssp.Tbl_TRS_ForecastOrderApproval
                                          where a.OrderNumber.Contains(OrderNumber)
                                          orderby a.ApprovalLevel
                                          select new { a.OrderNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportForecastOrderModel> reportForecastOrder = new List<ReportForecastOrderModel>();
            List<ReportMasterListKanbanModel> reportMasterListKanban = new List<ReportMasterListKanbanModel>();
            List<ReportForecastOrderApprovalModel> reportForecastOrderApproval = new List<ReportForecastOrderApprovalModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            DateTimeFormatInfo mfi = new DateTimeFormatInfo();
            string strMonthName = mfi.GetMonthName(Convert.ToInt32(ForecastOrder.OrderMonth)).ToString();

            ReportForecastOrderModel _reportForecastOrder = new ReportForecastOrderModel();
            _reportForecastOrder.OrderNumber = ForecastOrder.OrderNumber;
            _reportForecastOrder.OrderDate = Convert.ToDateTime(systemService.Vd(ForecastOrder.OrderDate.ToString()));
            _reportForecastOrder.OrderMonth = strMonthName;
            _reportForecastOrder.OrderYear = ForecastOrder.OrderYear;
            _reportForecastOrder.Shift = systemService.Vn(ForecastOrder.Shift.ToString());
            _reportForecastOrder.SupplierId = ForecastOrder.SupplierId;
            _reportForecastOrder.SupplierName = ForecastOrder.SupplierName;
            _reportForecastOrder.TotalPart = systemService.Vn(ForecastOrder.TotalPart.ToString());
            _reportForecastOrder.TotalOrder = systemService.Vn(ForecastOrder.TotalOrder.ToString());
            _reportForecastOrder.Status = systemService.Vn(ForecastOrder.Status.ToString());
            _reportForecastOrder.StatusName = ForecastOrder.StatusName;
            _reportForecastOrder.Approval = ForecastOrder.Approval;
            _reportForecastOrder.Revision = systemService.Vn(ForecastOrder.Revision.ToString());
            _reportForecastOrder.KanbanCycle = ForecastOrder.KanbanCycle;
            _reportForecastOrder.KanbanTime = ForecastOrder.KanbanTime;
            _reportForecastOrder.ApprovalLevel = systemService.Vn(ForecastOrder.ApprovalLevel.ToString());
            _reportForecastOrder.ApprovalName = ForecastOrder.ApprovalName;
            _reportForecastOrder.Remarks = ForecastOrder.Remarks;
            _reportForecastOrder.UserId = ForecastOrder.UserId;
            _reportForecastOrder.EditDate = Convert.ToDateTime(systemService.Vd(ForecastOrder.EditDate.ToString()));

            reportForecastOrder.Add(_reportForecastOrder);


            foreach (var orderdetails in MLOK)
            {
                ReportMasterListKanbanModel _ReportMasterListKanban = new ReportMasterListKanbanModel();
                _ReportMasterListKanban.OrderNumber = orderdetails.OrderNumber;
                _ReportMasterListKanban.SupplierId = orderdetails.SupplierId;
                _ReportMasterListKanban.UniqueNumber = orderdetails.UniqueNumber;
                _ReportMasterListKanban.PartNumber = orderdetails.PartNumber;
                _ReportMasterListKanban.PartName = orderdetails.PartName;
                _ReportMasterListKanban.Model = orderdetails.PartModel;
                _ReportMasterListKanban.Unit = orderdetails.UnitLevel1;
                _ReportMasterListKanban.Units = orderdetails.UnitLevel2;
                _ReportMasterListKanban.PackingId = orderdetails.PackingId;
                _ReportMasterListKanban.LocationId = orderdetails.LocationId;
                _ReportMasterListKanban.QtyByKanban = systemService.Vn(orderdetails.QtyByKanban.ToString());
                _ReportMasterListKanban.QtyByDay = systemService.Vn(orderdetails.QtyByDay.ToString());
                _ReportMasterListKanban.KanbanN10 = systemService.Vn(orderdetails.KanbanN10.ToString());
                _ReportMasterListKanban.KanbanN00 = systemService.Vn(orderdetails.KanbanN00.ToString());
                _ReportMasterListKanban.KanbanN01 = systemService.Vn(orderdetails.KanbanN01.ToString());
                _ReportMasterListKanban.KanbanReg = systemService.Vn(orderdetails.KanbanReg.ToString());
                _ReportMasterListKanban.SafetyHour = systemService.Vn(orderdetails.SafetyHour.ToString());
                _ReportMasterListKanban.SafetyKanban = systemService.Vn(orderdetails.SafetyKanban.ToString());
                _ReportMasterListKanban.SafetyParts = systemService.Vn(orderdetails.SafetyParts.ToString());
                _ReportMasterListKanban.WorkHour = systemService.Vn(orderdetails.WorkHour.ToString());
                _ReportMasterListKanban.Cycle1 = systemService.Vn(orderdetails.Cycle1.ToString());
                _ReportMasterListKanban.Cycle2 = systemService.Vn(orderdetails.Cycle2.ToString());
                _ReportMasterListKanban.Cycle3 = systemService.Vn(orderdetails.Cycle3.ToString());
                _ReportMasterListKanban.MinStock = systemService.Vn(orderdetails.MinStock.ToString());
                _ReportMasterListKanban.MaxStock = systemService.Vn(orderdetails.MaxStock.ToString());

                reportMasterListKanban.Add(_ReportMasterListKanban);
            }

            /* Header Workday */
            string strmonth = ForecastOrder.OrderMonth + "/" + ForecastOrder.OrderYear;
            var workdayresult = purchaseController.ForecastWorkDayJson(strmonth);
            System.Reflection.PropertyInfo finalresult = workdayresult.GetType().GetProperty("Data");
            var workday = finalresult.GetValue(workdayresult, null);

            /* Approval */
            int checkuser = 1;
            ReportForecastOrderApprovalModel _reportForecastOrderApproval = new ReportForecastOrderApprovalModel();
            foreach (var orderapprovals in ForecastOrderApproval)
            {
                _reportForecastOrderApproval.OrderNumber = orderapprovals.OrderNumber;
                if (orderapprovals.ApprovedDate != null)
                {
                    _reportForecastOrderApproval.ApprovedDate = systemService.Vd(orderapprovals.ApprovedDate.ToString(), "dd-MMMM-yyyy");
                }
                switch (orderapprovals.ApprovalLevel.ToString())
                {
                    case "1":
                        _reportForecastOrderApproval.UserName1 = orderapprovals.UserName;
                        UserEditModel user1 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user1 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign1 = user1.Sign;
                            }
                        }
                        break;
                    case "2":
                        if (checkuser == 1)
                        {
                            _reportForecastOrderApproval.UserName2 = orderapprovals.UserName;
                            UserEditModel user2 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user2 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportForecastOrderApproval.Sign2 = user2.Sign;
                                }
                            }
                            checkuser += 1;
                        }
                        else
                        {
                            _reportForecastOrderApproval.UserName3 = orderapprovals.UserName;
                            UserEditModel user3 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user3 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportForecastOrderApproval.Sign3 = user3.Sign;
                                }
                            }
                        }

                        break;
                    case "3":
                        _reportForecastOrderApproval.UserName4 = orderapprovals.UserName;
                        UserEditModel user4 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user4 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign4 = user4.Sign;
                            }
                        }
                        break;
                    case "4":
                        _reportForecastOrderApproval.UserName5 = orderapprovals.UserName;
                        UserEditModel user5 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user5 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign5 = user5.Sign;
                            }
                        }
                        break;
                }

            }
            reportForecastOrderApproval.Add(_reportForecastOrderApproval);

            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (reportForecastOrder == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_003.rpt"));

                rd.Database.Tables[0].SetDataSource(reportForecastOrder);
                rd.Database.Tables[1].SetDataSource(reportForecastOrderApproval);
                rd.Database.Tables[2].SetDataSource(reportMasterListKanban);
                rd.Subreports[0].Database.Tables[0].SetDataSource(reportCompanyProfile);

                //ForecastWorkdayModel workdayModel = (ForecastWorkdayModel)workday;

                //foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(workdayModel))
                //{
                //    string label = "Txt" + descriptor.Name;             // Name
                //    object value = descriptor.GetValue(workdayModel);   // Value
                //    try
                //    {
                //        TextObject rHeader = rd.ReportDefinition.ReportObjects[label] as TextObject;
                //        rHeader.Text = value.ToString();
                //    }
                //    catch (Exception e)
                //    {
                //        System.Diagnostics.Debug.WriteLine(label + " : " + e.Message);
                //    }

                //}

                //Tbl_MST_KanbanCalculation kanbanCalculation = vssp.Tbl_MST_KanbanCalculation.Where(a => a.StartDate <= ForecastOrder.OrderDate && (a.EndDate ?? ForecastOrder.OrderDate) >= ForecastOrder.OrderDate).FirstOrDefault();
                //if (kanbanCalculation != null)
                //{
                //    TextObject TxtHour = rd.ReportDefinition.ReportObjects["TxtHour"] as TextObject;
                //    TxtHour.Text = kanbanCalculation.InProcess.ToString();
                //    TextObject TxtCalc1 = rd.ReportDefinition.ReportObjects["TxtCalc1"] as TextObject;
                //    TxtCalc1.Text = "[ " + kanbanCalculation.InProcess.ToString() + " ]";
                //    TextObject TxtCalc2 = rd.ReportDefinition.ReportObjects["TxtCalc2"] as TextObject;
                //    TxtCalc2.Text = "[ " + kanbanCalculation.Stock.ToString() + " ]";
                //    TextObject TxtCalc3 = rd.ReportDefinition.ReportObjects["TxtCalc3"] as TextObject;
                //    TxtCalc3.Text = "[ " + kanbanCalculation.PrepareHeijunka.ToString() + " ]";
                //    TextObject TxtCalc4 = rd.ReportDefinition.ReportObjects["TxtCalc4"] as TextObject;
                //    TxtCalc4.Text = "[ " + kanbanCalculation.WIP.ToString() + " ]";
                //    TextObject TxtCalc5 = rd.ReportDefinition.ReportObjects["TxtCalc5"] as TextObject;
                //    TxtCalc5.Text = "[ " + kanbanCalculation.PrepareDelivery.ToString() + " ]";
                //}

            }

           

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/pdf");

        }
        public ActionResult MasterListKanbanProduction(string OrderNumber)
        {

            Vw_TRS_ControlPlanning ControlPlanning = (from a in vssp.Vw_TRS_ControlPlanning
                                                  where a.OrderNumber == OrderNumber
                                                  select a).First();

            var MLOK = from a in vssp.Tbl_TRS_MasterListKanbanProduction
                       join b in vssp.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber }
                       where a.OrderNumber == OrderNumber
                       select new
                       {
                           a.OrderNumber,
                           a.CustomerId,
                           a.PartNumber,
                           b.UniqueNumber,
                           b.PartName,
                           b.CustomerUnitModel,
                           b.UnitLevel1,
                           b.UnitLevel2,
                           b.PackingId,
                           a.OrderQty,
                           a.N1,
                           a.N2,
                           a.N3,
                           a.QtyByDay,
                           a.QtyByShift,
                           b.UnitQty,
                           a.QtyByHour,
                           a.KanbanCalc1,
                           a.KanbanCalc2,
                           a.KanbanCalc3,
                           a.KanbanCalc4,
                           a.KanbanCalc5,
                           a.KanbanN10,
                           a.KanbanN00,
                           a.KanbanN01,
                           a.MinStock,
                           a.MaxStock
                       };

            var ForecastOrderApproval = from a in vssp.Tbl_TRS_ControlPlanningApproval
                                        where a.OrderNumber.Contains(OrderNumber)
                                        orderby a.ApprovalLevel
                                        select new { a.OrderNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportControlPlanningModel> reportForecastOrder = new List<ReportControlPlanningModel>();
            List<ReportMasterListKanbanProductionModel> reportMasterListKanban = new List<ReportMasterListKanbanProductionModel>();
            List<ReportForecastOrderApprovalModel> reportForecastOrderApproval = new List<ReportForecastOrderApprovalModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            DateTimeFormatInfo mfi = new DateTimeFormatInfo();
            string strMonthName = mfi.GetMonthName(Convert.ToInt32(ControlPlanning.OrderMonth)).ToString();

            ReportControlPlanningModel _reportForecastOrder = new ReportControlPlanningModel();
            _reportForecastOrder.OrderNumber = ControlPlanning.OrderNumber;
            _reportForecastOrder.OrderDate = Convert.ToDateTime(systemService.Vd(ControlPlanning.OrderDate.ToString()));
            _reportForecastOrder.OrderMonth = strMonthName;
            _reportForecastOrder.OrderYear = ControlPlanning.OrderYear;
            _reportForecastOrder.Shift = systemService.Vn(ControlPlanning.Shift.ToString());
            _reportForecastOrder.LineId = ControlPlanning.LineId;
            _reportForecastOrder.LineName = ControlPlanning.LineName;
            _reportForecastOrder.TotalPart = systemService.Vn(ControlPlanning.TotalPart.ToString());
            _reportForecastOrder.TotalOrder = systemService.Vn(ControlPlanning.TotalOrder.ToString());
            _reportForecastOrder.Status = systemService.Vn(ControlPlanning.Status.ToString());
            _reportForecastOrder.StatusName = ControlPlanning.StatusName;
            _reportForecastOrder.Approval = ControlPlanning.Approval;
            _reportForecastOrder.Revision = systemService.Vn(ControlPlanning.Revision.ToString());
            _reportForecastOrder.ApprovalLevel = systemService.Vn(ControlPlanning.ApprovalLevel.ToString());
            _reportForecastOrder.ApprovalName = ControlPlanning.ApprovalName;
            _reportForecastOrder.Remarks = ControlPlanning.Remarks;
            _reportForecastOrder.UserId = ControlPlanning.UserId;
            _reportForecastOrder.EditDate = Convert.ToDateTime(systemService.Vd(ControlPlanning.EditDate.ToString()));

            reportForecastOrder.Add(_reportForecastOrder);
            double @shift = _reportForecastOrder.Shift;
            double @workhour = systemService.Vn("8");

            foreach (var orderdetails in MLOK)
            {
                ReportMasterListKanbanProductionModel _ReportMasterListKanban = new ReportMasterListKanbanProductionModel();
                _ReportMasterListKanban.OrderNumber = orderdetails.OrderNumber;
                _ReportMasterListKanban.CustomerId = orderdetails.CustomerId;
                _ReportMasterListKanban.UniqueNumber = orderdetails.UniqueNumber;
                _ReportMasterListKanban.PartNumber = orderdetails.PartNumber;
                _ReportMasterListKanban.PartName = orderdetails.PartName;
                _ReportMasterListKanban.Model = orderdetails.CustomerUnitModel;
                _ReportMasterListKanban.Unit = orderdetails.UnitLevel1;
                _ReportMasterListKanban.Units = orderdetails.UnitLevel2;
                _ReportMasterListKanban.PackingId = orderdetails.PackingId;
                _ReportMasterListKanban.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
                _ReportMasterListKanban.N1 = systemService.Vn(orderdetails.N1.ToString());
                _ReportMasterListKanban.N2 = systemService.Vn(orderdetails.N2.ToString());
                _ReportMasterListKanban.N3 = systemService.Vn(orderdetails.N3.ToString());
                _ReportMasterListKanban.QtyByDay = systemService.Vn(orderdetails.QtyByDay.ToString());
                _ReportMasterListKanban.QtyByShift = systemService.Vn(orderdetails.QtyByShift.ToString());
                _ReportMasterListKanban.QtyByKanban = systemService.Vn(orderdetails.UnitQty.ToString());
                _ReportMasterListKanban.QtyByHour = systemService.Vn(orderdetails.QtyByHour.ToString());
                _ReportMasterListKanban.KanbanCalc1 = systemService.Vn(orderdetails.KanbanCalc1.ToString());
                _ReportMasterListKanban.KanbanCalc2 = systemService.Vn(orderdetails.KanbanCalc2.ToString());
                _ReportMasterListKanban.KanbanCalc3 = systemService.Vn(orderdetails.KanbanCalc3.ToString());
                _ReportMasterListKanban.KanbanCalc4 = systemService.Vn(orderdetails.KanbanCalc4.ToString());
                _ReportMasterListKanban.KanbanCalc5 = systemService.Vn(orderdetails.KanbanCalc5.ToString());
                _ReportMasterListKanban.KanbanN10 = systemService.Vn(orderdetails.KanbanN10.ToString());
                _ReportMasterListKanban.KanbanN00 = systemService.Vn(orderdetails.KanbanN00.ToString());
                _ReportMasterListKanban.KanbanN01 = systemService.Vn(orderdetails.KanbanN01.ToString());
                _ReportMasterListKanban.MinStock = systemService.Vn(orderdetails.MinStock.ToString());
                _ReportMasterListKanban.MaxStock = systemService.Vn(orderdetails.MaxStock.ToString());

                reportMasterListKanban.Add(_ReportMasterListKanban);
            }

            /* Header Workday */
            string strmonth = ControlPlanning.OrderMonth + "/" + ControlPlanning.OrderYear;
            var workdayresult = purchaseController.ForecastWorkDayJson(strmonth);
            System.Reflection.PropertyInfo finalresult = workdayresult.GetType().GetProperty("Data");
            var workday = finalresult.GetValue(workdayresult, null);

            /* Approval */
            int checkuser = 1;
            ReportForecastOrderApprovalModel _reportForecastOrderApproval = new ReportForecastOrderApprovalModel();
            foreach (var orderapprovals in ForecastOrderApproval)
            {
                _reportForecastOrderApproval.OrderNumber = orderapprovals.OrderNumber;
                if (orderapprovals.ApprovedDate != null)
                {
                    _reportForecastOrderApproval.ApprovedDate = systemService.Vd(orderapprovals.ApprovedDate.ToString(), "dd-MMMM-yyyy");
                }
                switch (orderapprovals.ApprovalLevel.ToString())
                {
                    case "1":
                        _reportForecastOrderApproval.UserName1 = orderapprovals.UserName;
                        UserEditModel user1 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user1 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign1 = user1.Sign;
                            }
                        }
                        break;
                    case "2":
                        if (checkuser == 1)
                        {
                            _reportForecastOrderApproval.UserName2 = orderapprovals.UserName;
                            UserEditModel user2 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user2 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportForecastOrderApproval.Sign2 = user2.Sign;
                                }
                            }
                            checkuser += 1;
                        }
                        else
                        {
                            _reportForecastOrderApproval.UserName3 = orderapprovals.UserName;
                            UserEditModel user3 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user3 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportForecastOrderApproval.Sign3 = user3.Sign;
                                }
                            }
                        }

                        break;
                    case "4":
                        _reportForecastOrderApproval.UserName4 = orderapprovals.UserName;
                        UserEditModel user4 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user4 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportForecastOrderApproval.Sign4 = user4.Sign;
                            }
                        }
                        break;
                }

            }
            reportForecastOrderApproval.Add(_reportForecastOrderApproval);

            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (reportForecastOrder == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_015.rpt"));

                rd.Database.Tables[0].SetDataSource(reportForecastOrder);
                rd.Database.Tables[1].SetDataSource(reportForecastOrderApproval);
                rd.Database.Tables[2].SetDataSource(reportMasterListKanban);
                rd.Subreports[0].Database.Tables[0].SetDataSource(reportCompanyProfile);

                ForecastWorkdayModel workdayModel = (ForecastWorkdayModel)workday;

                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(workdayModel))
                {
                    string label = "Txt" + descriptor.Name;             // Name
                    object value = descriptor.GetValue(workdayModel);   // Value
                    try
                    {
                        TextObject rHeader = rd.ReportDefinition.ReportObjects[label] as TextObject;
                        if (value != null)
                        {
                            rHeader.Text = value.ToString();
                        } else
                        {
                            rHeader.Text = "0";
                        }
                    }
                    catch (Exception e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = systemService.GetExceptionDetails(e);
                        System.Diagnostics.Debug.WriteLine(label + " : " + errinfo);
                    }

                }

                Tbl_MST_KanbanCalculation kanbanCalculation = vssp.Tbl_MST_KanbanCalculation.Where(a => a.StartDate <= ControlPlanning.OrderDate && (a.EndDate ?? ControlPlanning.OrderDate) >= ControlPlanning.OrderDate).FirstOrDefault();
                if (kanbanCalculation != null)
                {
                    TextObject TxtHour = rd.ReportDefinition.ReportObjects["TxtHour"] as TextObject;
                    //TxtHour.Text = kanbanCalculation.InProcess.ToString();
                    TxtHour.Text = (workhour * shift).ToString();
                    TextObject TxtCalc1 = rd.ReportDefinition.ReportObjects["TxtCalc1"] as TextObject;
                    TxtCalc1.Text = "[ " + kanbanCalculation.InProcess.ToString() + " ]";
                    TextObject TxtCalc2 = rd.ReportDefinition.ReportObjects["TxtCalc2"] as TextObject;
                    TxtCalc2.Text = "[ " + kanbanCalculation.Stock.ToString() + " ]";
                    TextObject TxtCalc3 = rd.ReportDefinition.ReportObjects["TxtCalc3"] as TextObject;
                    TxtCalc3.Text = "[ " + kanbanCalculation.PrepareHeijunka.ToString() + " ]";
                    TextObject TxtCalc4 = rd.ReportDefinition.ReportObjects["TxtCalc4"] as TextObject;
                    TxtCalc4.Text = "[ " + kanbanCalculation.WIP.ToString() + " ]";
                    TextObject TxtCalc5 = rd.ReportDefinition.ReportObjects["TxtCalc5"] as TextObject;
                    TxtCalc5.Text = "[ " + kanbanCalculation.PrepareDelivery.ToString() + " ]";
                }

            }



            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/pdf");

        }
        public ActionResult DeliveryNote(string ordernumber)
        {

            //Vw_TRS_SupplierOrder SupplierOrder = (from a in vssp.Vw_TRS_SupplierOrder
            //                                      where a.OrderNumber == ordernumber
            //                                            select a).FirstOrDefault();

            QRCode.generateOrderQr(ordernumber, "SupplierOrder");

            var SupplierOrder = (from a in vssp.Vw_TRS_SupplierOrder
                                 join b in vssp.Tbl_TRS_QrCodePath on a.OrderNumber equals b.DocId into qrcode
                                 from b in qrcode.DefaultIfEmpty()
                                 where a.OrderNumber == ordernumber
                                 select new
                                 {
                                     a.OrderNumber,
                                     a.OrderDate,
                                     a.SupplierId,
                                     a.SupplierName,
                                     a.SSP,
                                     a.ProcessName,
                                     a.KanbanCycle,
                                     a.IncomingDate,
                                     a.IncomingTime,
                                     a.Shift,
                                     a.TotalPart,
                                     a.TotalOrder,
                                     a.Status,
                                     a.StatusName,
                                     a.Remarks,
                                     b.QrcodePath,
                                     b.BarcodePath,
                                     a.UserId,
                                     a.EditDate
                                 }).FirstOrDefault();
            
            var SupplierOrderDetail = from a in vssp.Tbl_TRS_SupplierOrderDetail
                                        join b in vssp.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber}
                                        where a.OrderNumber == ordernumber
                                        select new { a.OrderNumber,a.SupplierId, b.UniqueNumber, a.PartNumber, b.PartName, b.UnitQty, 
                                            b.UnitLevel1, b.UnitLevel2, b.PackingId, b.PartModel, a.OrderQty, a.OrderUnitQty, a.ReceiveQty };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportSupplierOrderModel> reportSupplierOrder = new List<ReportSupplierOrderModel>();
            List<ReportSupplierOrderDetailModel> reportSupplierOrderDetail = new List<ReportSupplierOrderDetailModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportSupplierOrderModel _ReportSupplierOrder = new ReportSupplierOrderModel();
            _ReportSupplierOrder.OrderNumber = SupplierOrder.OrderNumber;
            _ReportSupplierOrder.OrderDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.OrderDate.ToString()));
            _ReportSupplierOrder.SupplierId = SupplierOrder.SupplierId;
            _ReportSupplierOrder.SupplierName = SupplierOrder.SupplierName;
            _ReportSupplierOrder.SSP = SupplierOrder.SSP;
            _ReportSupplierOrder.ProcessName = SupplierOrder.ProcessName;
            _ReportSupplierOrder.KanbanCycle = SupplierOrder.KanbanCycle;
            _ReportSupplierOrder.IncomingDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingDate.ToString()));
            _ReportSupplierOrder.IncomingTime = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingTime.ToString(),"HH:mm"));
            _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
            _ReportSupplierOrder.TotalPart = systemService.Vn(SupplierOrder.TotalPart.ToString());
            _ReportSupplierOrder.TotalOrder = systemService.Vn(SupplierOrder.TotalOrder.ToString());
            _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
            _ReportSupplierOrder.Remarks = SupplierOrder.Remarks;
            _ReportSupplierOrder.Status = systemService.Vn(SupplierOrder.Status.ToString());
            _ReportSupplierOrder.StatusName = SupplierOrder.StatusName;
            _ReportSupplierOrder.UserId = SupplierOrder.UserId;
            _ReportSupplierOrder.EditDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.EditDate.ToString()));
            if (systemService.Vf(SupplierOrder.QrcodePath) != "")
            {
                _ReportSupplierOrder.QrCode = Server.MapPath(systemService.Vf(SupplierOrder.QrcodePath));
            } else
            {
                _ReportSupplierOrder.QrCode = "";
            }
            //_ReportSupplierOrder.QrCode = "http://chart.googleapis.com/chart?chs=200x200&cht=qr&chl="+ SupplierOrder.OrderNumber +"&choe=UTF-8";

            reportSupplierOrder.Add(_ReportSupplierOrder);

            foreach (var orderdetails in SupplierOrderDetail)
            {
                ReportSupplierOrderDetailModel _ReportSupplierOrderdetail = new ReportSupplierOrderDetailModel();
                _ReportSupplierOrderdetail.OrderNumber = orderdetails.OrderNumber;
                _ReportSupplierOrderdetail.SupplierId = orderdetails.SupplierId;
                _ReportSupplierOrderdetail.UniqueNumber = orderdetails.UniqueNumber;
                _ReportSupplierOrderdetail.PartNumber = orderdetails.PartNumber;
                _ReportSupplierOrderdetail.PartName = orderdetails.PartName;
                _ReportSupplierOrderdetail.PartModel = orderdetails.PartModel;
                _ReportSupplierOrderdetail.PackingId = orderdetails.PackingId;
                _ReportSupplierOrderdetail.UnitLevel1 = orderdetails.UnitLevel1;
                _ReportSupplierOrderdetail.UnitLevel2 = orderdetails.UnitLevel2;
                _ReportSupplierOrderdetail.UnitQty = systemService.Vn(orderdetails.UnitQty.ToString());
                _ReportSupplierOrderdetail.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
                _ReportSupplierOrderdetail.OrderUnitQty = systemService.Vn(orderdetails.OrderUnitQty.ToString());
                _ReportSupplierOrderdetail.ReceiveQty = systemService.Vn(orderdetails.ReceiveQty.ToString());
                _ReportSupplierOrderdetail.QrCode = "";
                //_ReportSupplierOrderdetail.QrCode = "http://chart.googleapis.com/chart?chs=150x150&cht=qr&chl=" + orderdetails.OrderNumber + ";" + orderdetails.SupplierId + ";" + orderdetails.UniqueNumber + ";" + orderdetails.PartNumber + ";" + orderdetails.PartName + ";" + orderdetails.OrderQty + ";" + orderdetails.OrderUnitQty + "&choe=UTF-8";

                reportSupplierOrderDetail.Add(_ReportSupplierOrderdetail);
            }

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name;
            _ReportCompanyProfile.CompAddress = CompanyProfile.ID + " - " + CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (SupplierOrder.OrderNumber == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_004.rpt"));
                rd.Database.Tables[0].SetDataSource(reportSupplierOrder);
                rd.Database.Tables[1].SetDataSource(reportSupplierOrderDetail);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            QRCode.cleanOrderQr(ordernumber);

            return File(stream, "application/pdf");

        }
        public ActionResult DeliveryReturn(string ReturnNumber)
        {

            //Vw_TRS_ReturnPart ReturnPart = (from a in vssp.Vw_TRS_ReturnPart
            //                                      where a.ReturnNumber == ReturnNumber
            //                                            select a).FirstOrDefault();

            QRCode.generateOrderQr(ReturnNumber, "ReturnPart");

            var ReturnPart = (from a in vssp.Vw_TRS_ReturnPart
                                 join b in vssp.Tbl_TRS_QrCodePath on a.ReturnNumber equals b.DocId into qrcode
                                 from b in qrcode.DefaultIfEmpty()
                                 where a.ReturnNumber == ReturnNumber
                                 select new
                                 {
                                     a.ReturnNumber,
                                     a.ReturnDate,
                                     a.SupplierId,
                                     a.SupplierName,
                                     a.IncomingDate,
                                     a.IncomingTime,
                                     a.TotalPart,
                                     a.TotalUnitReturn,
                                     a.Status,
                                     a.StatusName,
                                     a.Remarks,
                                     b.QrcodePath,
                                     b.BarcodePath,
                                     a.UserId,
                                     a.EditDate
                                 }).FirstOrDefault();

            var ReturnPartDetail = from a in vssp.Tbl_TRS_ReturnPartDetail
                                      join b in vssp.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber }
                                      where a.ReturnNumber == ReturnNumber
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
                                          a.ReturnUnitQty,
                                          a.ReceiveQty
                                      };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportReturnPartModel> reportReturnPart = new List<ReportReturnPartModel>();
            List<ReportReturnPartDetailModel> reportReturnPartDetail = new List<ReportReturnPartDetailModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportReturnPartModel _ReportReturnPart = new ReportReturnPartModel();
            _ReportReturnPart.ReturnNumber = ReturnPart.ReturnNumber;
            _ReportReturnPart.ReturnDate = Convert.ToDateTime(systemService.Vd(ReturnPart.ReturnDate.ToString()));
            _ReportReturnPart.SupplierId = ReturnPart.SupplierId;
            _ReportReturnPart.SupplierName = ReturnPart.SupplierName;
            _ReportReturnPart.IncomingDate = Convert.ToDateTime(systemService.Vd(ReturnPart.IncomingDate.ToString()));
            _ReportReturnPart.IncomingTime = Convert.ToDateTime(systemService.Vd(ReturnPart.IncomingTime.ToString(), "HH:mm"));
            _ReportReturnPart.TotalPart = systemService.Vn(ReturnPart.TotalPart.ToString());
            _ReportReturnPart.TotalUnitReturn = systemService.Vn(ReturnPart.TotalUnitReturn.ToString());
            _ReportReturnPart.Remarks = ReturnPart.Remarks;
            _ReportReturnPart.Status = systemService.Vn(ReturnPart.Status.ToString());
            _ReportReturnPart.StatusName = ReturnPart.StatusName;
            _ReportReturnPart.UserId = ReturnPart.UserId;
            _ReportReturnPart.EditDate = Convert.ToDateTime(systemService.Vd(ReturnPart.EditDate.ToString()));
            if (systemService.Vf(ReturnPart.QrcodePath) != "")
            {
                _ReportReturnPart.QrCode = Server.MapPath(systemService.Vf(ReturnPart.QrcodePath));
            }
            else
            {
                _ReportReturnPart.QrCode = "";
            }
            //_ReportReturnPart.QrCode = "http://chart.googleapis.com/chart?chs=200x200&cht=qr&chl="+ ReturnPart.ReturnNumber +"&choe=UTF-8";

            reportReturnPart.Add(_ReportReturnPart);

            foreach (var Returndetails in ReturnPartDetail)
            {
                ReportReturnPartDetailModel _ReportReturnPartdetail = new ReportReturnPartDetailModel();
                _ReportReturnPartdetail.ReturnNumber = Returndetails.ReturnNumber;
                _ReportReturnPartdetail.SupplierId = Returndetails.SupplierId;
                _ReportReturnPartdetail.UniqueNumber = Returndetails.UniqueNumber;
                _ReportReturnPartdetail.PartNumber = Returndetails.PartNumber;
                _ReportReturnPartdetail.PartName = Returndetails.PartName;
                _ReportReturnPartdetail.PartModel = Returndetails.PartModel;
                _ReportReturnPartdetail.PackingId = Returndetails.PackingId;
                _ReportReturnPartdetail.UnitLevel1 = Returndetails.UnitLevel1;
                _ReportReturnPartdetail.UnitLevel2 = Returndetails.UnitLevel2;
                _ReportReturnPartdetail.UnitQty = systemService.Vn(Returndetails.UnitQty.ToString());
                _ReportReturnPartdetail.ReturnUnitQty = systemService.Vn(Returndetails.ReturnUnitQty.ToString());
                _ReportReturnPartdetail.ReceiveQty = systemService.Vn(Returndetails.ReceiveQty.ToString());
                _ReportReturnPartdetail.QrCode = "";
                //_ReportReturnPartdetail.QrCode = "http://chart.googleapis.com/chart?chs=150x150&cht=qr&chl=" + Returndetails.ReturnNumber + ";" + Returndetails.SupplierId + ";" + Returndetails.UniqueNumber + ";" + Returndetails.PartNumber + ";" + Returndetails.PartName + ";" + Returndetails.ReturnQty + ";" + Returndetails.ReturnUnitQty + "&choe=UTF-8";

                reportReturnPartDetail.Add(_ReportReturnPartdetail);
            }

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name;
            _ReportCompanyProfile.CompAddress = CompanyProfile.ID + " - " + CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (ReturnPart.ReturnNumber == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_019.rpt"));
                rd.Database.Tables[0].SetDataSource(reportReturnPart);
                rd.Database.Tables[1].SetDataSource(reportReturnPartDetail);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            QRCode.cleanOrderQr(ReturnNumber);

            return File(stream, "application/pdf");

        }
        public ActionResult SSPDeliveryOrder(string ordernumber)
        {

            //Vw_TRS_SupplierOrder SupplierOrder = (from a in vssp.Vw_TRS_SupplierOrder
            //                                      where a.OrderNumber == ordernumber
            //                                            select a).FirstOrDefault();

            //QRCode.generateOrderQr(ordernumber, "SupplierOrder");

            var SupplierOrder = (from a in vssp.Vw_TRS_SupplierOrder
                                 join b in vssp.Tbl_TRS_QrCodePath on a.OrderNumber equals b.DocId into qrcode
                                 join c in vssp.Tbl_MST_Supplier on a.SupplierId equals c.SupplierId
                                 from b in qrcode.DefaultIfEmpty()
                                 where a.OrderNumber == ordernumber
                                 select new
                                 {
                                     a.OrderNumber,
                                     a.OrderDate,
                                     a.SupplierId,
                                     a.SupplierName,
                                     c.Address,
                                     c.City,
                                     a.SSP,
                                     a.ProcessName,
                                     a.KanbanCycle,
                                     a.IncomingDate,
                                     a.IncomingTime,
                                     a.Shift,
                                     a.TotalPart,
                                     a.TotalOrder,
                                     a.Status,
                                     a.StatusName,
                                     a.Remarks,
                                     b.QrcodePath,
                                     b.BarcodePath,
                                     a.UserId,
                                     a.EditDate
                                 }).FirstOrDefault();

            var SupplierOrderDetail = from a in vssp.Tbl_TRS_SupplierOrderDetail
                                      join b in vssp.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber }
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
                                          a.OrderQty,
                                          a.OrderUnitQty,
                                          a.ReceiveQty
                                      };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportSupplierOrderModel> reportSupplierOrder = new List<ReportSupplierOrderModel>();
            List<ReportSupplierOrderDetailModel> reportSupplierOrderDetail = new List<ReportSupplierOrderDetailModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportSupplierOrderModel _ReportSupplierOrder = new ReportSupplierOrderModel();
            _ReportSupplierOrder.OrderNumber = SupplierOrder.OrderNumber;
            _ReportSupplierOrder.OrderDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.OrderDate.ToString()));
            _ReportSupplierOrder.SupplierId = SupplierOrder.SupplierId;
            _ReportSupplierOrder.SupplierName = SupplierOrder.SupplierName;
            _ReportSupplierOrder.SupplierAddress = SupplierOrder.Address + ", " + SupplierOrder.City;
            _ReportSupplierOrder.SSP = SupplierOrder.SSP;
            _ReportSupplierOrder.ProcessName = SupplierOrder.ProcessName;
            _ReportSupplierOrder.KanbanCycle = SupplierOrder.KanbanCycle;
            _ReportSupplierOrder.IncomingDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingDate.ToString()));
            _ReportSupplierOrder.IncomingTime = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingTime.ToString(), "HH:mm"));
            _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
            _ReportSupplierOrder.TotalPart = systemService.Vn(SupplierOrder.TotalPart.ToString());
            _ReportSupplierOrder.TotalOrder = systemService.Vn(SupplierOrder.TotalOrder.ToString());
            _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
            _ReportSupplierOrder.Remarks = SupplierOrder.Remarks;
            _ReportSupplierOrder.Status = systemService.Vn(SupplierOrder.Status.ToString());
            _ReportSupplierOrder.StatusName = SupplierOrder.StatusName;
            _ReportSupplierOrder.UserId = SupplierOrder.UserId;
            _ReportSupplierOrder.EditDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.EditDate.ToString()));
            _ReportSupplierOrder.QrCode = "";

            reportSupplierOrder.Add(_ReportSupplierOrder);

            foreach (var orderdetails in SupplierOrderDetail)
            {
                ReportSupplierOrderDetailModel _ReportSupplierOrderdetail = new ReportSupplierOrderDetailModel();
                _ReportSupplierOrderdetail.OrderNumber = orderdetails.OrderNumber;
                _ReportSupplierOrderdetail.SupplierId = orderdetails.SupplierId;
                _ReportSupplierOrderdetail.UniqueNumber = orderdetails.UniqueNumber;
                _ReportSupplierOrderdetail.PartNumber = orderdetails.PartNumber;
                _ReportSupplierOrderdetail.PartName = orderdetails.PartName;
                _ReportSupplierOrderdetail.PartModel = orderdetails.PartModel;
                _ReportSupplierOrderdetail.PackingId = orderdetails.PackingId;
                _ReportSupplierOrderdetail.UnitLevel1 = orderdetails.UnitLevel1;
                _ReportSupplierOrderdetail.UnitLevel2 = orderdetails.UnitLevel2;
                _ReportSupplierOrderdetail.UnitQty = systemService.Vn(orderdetails.UnitQty.ToString());
                _ReportSupplierOrderdetail.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
                _ReportSupplierOrderdetail.OrderUnitQty = systemService.Vn(orderdetails.OrderUnitQty.ToString());
                _ReportSupplierOrderdetail.ReceiveQty = systemService.Vn(orderdetails.ReceiveQty.ToString());
                _ReportSupplierOrderdetail.QrCode = "";
                //_ReportSupplierOrderdetail.QrCode = "http://chart.googleapis.com/chart?chs=150x150&cht=qr&chl=" + orderdetails.OrderNumber + ";" + orderdetails.SupplierId + ";" + orderdetails.UniqueNumber + ";" + orderdetails.PartNumber + ";" + orderdetails.PartName + ";" + orderdetails.OrderQty + ";" + orderdetails.OrderUnitQty + "&choe=UTF-8";

                reportSupplierOrderDetail.Add(_ReportSupplierOrderdetail);
            }

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name;
            _ReportCompanyProfile.CompAddress = CompanyProfile.ID + " - " + CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (SupplierOrder.OrderNumber == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_011.rpt"));
                rd.Database.Tables[0].SetDataSource(reportSupplierOrder);
                rd.Database.Tables[1].SetDataSource(reportSupplierOrderDetail);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            QRCode.cleanOrderQr(ordernumber);

            return File(stream, "application/pdf");

        }
        public ActionResult KanbanOrder(string ordernumber)
        {

            QRCode.generateKanbanCardQrcode(ordernumber);

            var SupplierOrder = (from a in vssp.Vw_TRS_SupplierOrder
                                 join b in vssp.Tbl_TRS_QrCodePath on a.OrderNumber equals b.DocId into qrcode
                                 from b in qrcode.DefaultIfEmpty()
                                 where a.OrderNumber == ordernumber
                                 select new
                                 {
                                     a.OrderNumber,
                                     a.OrderDate,
                                     a.SupplierId,
                                     a.SupplierName,
                                     a.KanbanCycle,
                                     a.IncomingDate,
                                     a.IncomingTime,
                                     a.Shift,
                                     a.TotalPart,
                                     a.TotalOrder,
                                     a.Status,
                                     a.StatusName,
                                     a.Remarks,
                                     b.QrcodePath,
                                     b.BarcodePath,
                                     a.UserId,
                                     a.EditDate
                                 }).FirstOrDefault();

            var KanbanOrderDetail = (from a in vssp.Tbl_TRS_SupplierOrderKanban
                                     join b in vssp.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                     from b in part.DefaultIfEmpty()
                                     join c in vssp.Tbl_TRS_SupplierOrderDetail on new { a.OrderNumber, a.SupplierId, a.PartNumber } equals new { c.OrderNumber, c.SupplierId, c.PartNumber } into partorder
                                     from c in partorder.DefaultIfEmpty()
                                     join d in vssp.Tbl_TRS_QrCodePath on a.KanbanKey equals d.DocId into qrcode
                                     from d in qrcode.DefaultIfEmpty()
                                     where a.OrderNumber == ordernumber
                                     orderby a.KanbanKey
                                     select new {
                                         a.KanbanKey,
                                         a.KanbanRun,
                                         c.OrderQty,
                                         a.OrderNumber,
                                         a.ReceiveNumber,
                                         a.SupplierId,
                                         b.UniqueNumber,
                                         a.PartNumber,
                                         b.PartName,
                                         b.PartModel,
                                         b.CategoryId,
                                         b.PackingId,
                                         b.AreaId,
                                         b.LocationId,
                                         a.CostId,
                                         b.UnitQty,
                                         b.UnitLevel2,
                                         a.KanbanCycle,
                                         a.IncomingDate,
                                         a.IncomingTime,
                                         a.Received,
                                         d.QrcodePath,
                                         d.BarcodePath
                                     }).ToList();

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportSupplierOrderModel> reportSupplierOrder = new List<ReportSupplierOrderModel>();
            List<ReportSupplierOrderKanbanModel> reportSupplierOrderKanban = new List<ReportSupplierOrderKanbanModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportSupplierOrderModel _ReportSupplierOrder = new ReportSupplierOrderModel();
            _ReportSupplierOrder.OrderNumber = SupplierOrder.OrderNumber;
            _ReportSupplierOrder.OrderDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.OrderDate.ToString()));
            _ReportSupplierOrder.SupplierId = SupplierOrder.SupplierId;
            _ReportSupplierOrder.SupplierName = SupplierOrder.SupplierName;
            _ReportSupplierOrder.KanbanCycle = SupplierOrder.KanbanCycle;
            _ReportSupplierOrder.IncomingDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingDate.ToString()));
            _ReportSupplierOrder.IncomingTime = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingTime.ToString()));
            _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
            _ReportSupplierOrder.TotalPart = systemService.Vn(SupplierOrder.TotalPart.ToString());
            _ReportSupplierOrder.TotalOrder = systemService.Vn(SupplierOrder.TotalOrder.ToString());
            _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
            _ReportSupplierOrder.Remarks = SupplierOrder.Remarks;
            _ReportSupplierOrder.Status = systemService.Vn(SupplierOrder.Status.ToString());
            _ReportSupplierOrder.StatusName = SupplierOrder.StatusName;
            _ReportSupplierOrder.UserId = SupplierOrder.UserId;
            _ReportSupplierOrder.EditDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.EditDate.ToString()));
            if (systemService.Vf(SupplierOrder.QrcodePath) != "")
            {
                _ReportSupplierOrder.QrCode = Server.MapPath(systemService.Vf(SupplierOrder.QrcodePath));
            } else
            {
                _ReportSupplierOrder.QrCode = "";
            }
            //_ReportSupplierOrder.QrCode = "http://chart.googleapis.com/chart?chs=150x150&cht=qr&chl=" + SupplierOrder.OrderNumber + "&choe=UTF-8";

            reportSupplierOrder.Add(_ReportSupplierOrder);

            foreach (var kanban in KanbanOrderDetail)
            {
                ReportSupplierOrderKanbanModel _ReportSupplierOrderKanban = new ReportSupplierOrderKanbanModel();
                _ReportSupplierOrderKanban.KanbanKey = kanban.KanbanKey;
                _ReportSupplierOrderKanban.KanbanRun = systemService.Vn(kanban.KanbanRun.ToString());
                _ReportSupplierOrderKanban.KanbanTotal= systemService.Vn(kanban.OrderQty.ToString());
                _ReportSupplierOrderKanban.OrderNumber = kanban.OrderNumber;
                _ReportSupplierOrderKanban.ReceiveNumber = kanban.ReceiveNumber;
                _ReportSupplierOrderKanban.SupplierId = kanban.SupplierId;
                _ReportSupplierOrderKanban.UniqueNumber = kanban.UniqueNumber;
                _ReportSupplierOrderKanban.PartNumber = kanban.PartNumber;
                _ReportSupplierOrderKanban.PartName = kanban.PartName;
                _ReportSupplierOrderKanban.Category = kanban.CategoryId;
                _ReportSupplierOrderKanban.PartModel = kanban.PartModel;
                _ReportSupplierOrderKanban.Packing = kanban.PackingId;
                _ReportSupplierOrderKanban.Area = kanban.AreaId;
                _ReportSupplierOrderKanban.Location = kanban.LocationId;
                _ReportSupplierOrderKanban.CostId = kanban.CostId;
                _ReportSupplierOrderKanban.Unit = kanban.UnitLevel2;
                _ReportSupplierOrderKanban.UnitQty = systemService.Vn(kanban.UnitQty.ToString());
                _ReportSupplierOrderKanban.KanbanCycle = kanban.KanbanCycle;
                _ReportSupplierOrderKanban.IncomingDate = Convert.ToDateTime(systemService.Vd(kanban.IncomingDate.ToString()));
                _ReportSupplierOrderKanban.IncomingTime = Convert.ToDateTime(systemService.Vd(kanban.IncomingTime.ToString(),"HH:mm"));
                _ReportSupplierOrderKanban.Received = systemService.Vb(kanban.Received.ToString());
                if (systemService.Vf(kanban.QrcodePath) != "")
                {

                    _ReportSupplierOrderKanban.QrCode = Server.MapPath(systemService.Vf(kanban.QrcodePath));
                    _ReportSupplierOrderKanban.Barcode = Server.MapPath(systemService.Vf(kanban.BarcodePath));
                } else
                {
                    _ReportSupplierOrderKanban.QrCode = "";
                    _ReportSupplierOrderKanban.Barcode = "";
                }
                
                Debug.Print(_ReportSupplierOrderKanban.KanbanRun.ToString());
                Debug.Print(_ReportSupplierOrderKanban.KanbanTotal.ToString());

                //_ReportSupplierOrderKanban.QrCode = "http://chart.googleapis.com/chart?chs=200x200&cht=qr&chl=" + kanban.KanbanKey + ";" + kanban.OrderNumber + ";" + 
                //                                    kanban.SupplierId + ";" + kanban.UniqueNumber + ";" + kanban.PartNumber + ";" + kanban.PartName + ";" + 
                //                                    systemService.Vn(kanban.UnitQty.ToString()) + ";" + kanban.UnitLevel2 + ";" + kanban.KanbanRun + ";" + kanban.OrderQty + ";" + kanban.KanbanCycle + ";" +
                //                                    systemService.Vd(kanban.IncomingDate.ToString()) + ";" + systemService.Vd(kanban.IncomingTime.ToString(), "HH:mm") + "&choe=UTF-8";
                //_ReportSupplierOrderKanban.Barcode = "http://barcodes4.me/barcode/c128a/" + kanban.KanbanKey + ".png";
                reportSupplierOrderKanban.Add(_ReportSupplierOrderKanban);
            }

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name;
            _ReportCompanyProfile.CompAddress = CompanyProfile.ID + " - " + CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (SupplierOrder.OrderNumber == null || reportSupplierOrderKanban.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_005.rpt"));
                rd.Database.Tables[0].SetDataSource(reportSupplierOrder);
                rd.Database.Tables[1].SetDataSource(reportSupplierOrderKanban);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            QRCode.cleanKanbanCardQrcode(ordernumber);

            return File(stream, "application/pdf");

        }

        public ActionResult ReceiveNote(string receivenumber)
        {

            QRCode.generateOrderQr(receivenumber,"ReceiveOrder");

            var ReceivingOrder = (from a in vssp.Vw_TRS_ReceivingOrder
                                  join b in vssp.Tbl_TRS_QrCodePath on a.ReceiveNumber equals b.DocId into qrcode
                                  from b in qrcode.DefaultIfEmpty()
                                  where a.ReceiveNumber == receivenumber
                                  select new
                                  {
                                      a.ReceiveNumber,
                                      a.ReceiveDate,
                                      a.OrderNumber,
                                      a.SupplierId,
                                      a.SupplierName,
                                      a.KanbanCycle,
                                      a.IncomingDate,
                                      a.IncomingTime,
                                      a.TotalPart,
                                      a.TotalReceive,
                                      a.ReturnPart,
                                      a.Remarks,
                                      a.Status,
                                      a.StatusName,
                                      a.UserId,
                                      a.EditDate,
                                      b.QrcodePath
                                  }).FirstOrDefault();

            var ReceivingOrderDetail = from a in vssp.Vw_TRS_ReceivingOrderDetail
                                       where a.ReceiveNumber == receivenumber
                                       select new
                                       {
                                           a.ReceiveNumber,
                                           a.SupplierId,
                                           a.UniqueNumber,
                                           a.PartNumber,
                                           a.PartName,
                                           a.UnitQty,
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

            var CompanyProfile = systemService.GetLicenseInfo();
            var Users = accountService.UserEditList(ReceivingOrder.UserId).FirstOrDefault();

            List<ReportReceivingOrderModel> reportReceivingOrder = new List<ReportReceivingOrderModel>();
            List<ReportReceivingOrderDetailModel> reportReceivingOrderDetail = new List<ReportReceivingOrderDetailModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

             ReportReceivingOrderModel _ReportReceivingOrder = new ReportReceivingOrderModel();
            _ReportReceivingOrder.ReceiveNumber = ReceivingOrder.ReceiveNumber;
            _ReportReceivingOrder.ReceiveDate = Convert.ToDateTime(systemService.Vd(ReceivingOrder.ReceiveDate.ToString(),"MMMM dd, yyyy HH:mm"));
            _ReportReceivingOrder.OrderNumber = ReceivingOrder.OrderNumber;
            _ReportReceivingOrder.SupplierId = ReceivingOrder.SupplierId;
            _ReportReceivingOrder.SupplierName = ReceivingOrder.SupplierName;
            _ReportReceivingOrder.KanbanCycle = ReceivingOrder.KanbanCycle;
            _ReportReceivingOrder.IncomingDate = Convert.ToDateTime(systemService.Vd(ReceivingOrder.IncomingDate.ToString(), "MMMM dd, yyyy HH:mm")); ;
            _ReportReceivingOrder.IncomingTime = Convert.ToDateTime(systemService.Vd(ReceivingOrder.IncomingTime.ToString(), "MMMM dd, yyyy HH:mm")); ;
            _ReportReceivingOrder.TotalPart = systemService.Vn(ReceivingOrder.TotalPart.ToString());
            _ReportReceivingOrder.TotalReceive = systemService.Vn(ReceivingOrder.TotalReceive.ToString());
            _ReportReceivingOrder.ReturnPart = systemService.Vb(ReceivingOrder.ReturnPart.ToString());
            _ReportReceivingOrder.Remarks = ReceivingOrder.Remarks;
            _ReportReceivingOrder.Status = systemService.Vn(ReceivingOrder.Status.ToString());
            _ReportReceivingOrder.StatusName = ReceivingOrder.StatusName;
            _ReportReceivingOrder.UserId = ReceivingOrder.UserId;
            if (Users != null)
            {
                _ReportReceivingOrder.UserName = Users.UserName;
            }
            _ReportReceivingOrder.EditDate = Convert.ToDateTime(systemService.Vd(ReceivingOrder.EditDate.ToString()));
            if (systemService.Vf(ReceivingOrder.QrcodePath) != "")
            {
                _ReportReceivingOrder.QrCode = Server.MapPath(systemService.Vf(ReceivingOrder.QrcodePath));
            } else
            {
                _ReportReceivingOrder.QrCode = "";
            }
                //_ReportReceivingOrder.QrCode = "http://chart.googleapis.com/chart?chs=200x200&cht=qr&chl=" + ReceivingOrder.ReceiveNumber + "&choe=UTF-8";
            reportReceivingOrder.Add(_ReportReceivingOrder);

            foreach (var orderdetails in ReceivingOrderDetail)
            {
                ReportReceivingOrderDetailModel _ReportReceivingOrderdetail = new ReportReceivingOrderDetailModel();
                _ReportReceivingOrderdetail.ReceiveNumber = orderdetails.ReceiveNumber;
                _ReportReceivingOrderdetail.SupplierId = orderdetails.SupplierId;
                _ReportReceivingOrderdetail.UniqueNumber = orderdetails.UniqueNumber;
                _ReportReceivingOrderdetail.PartNumber = orderdetails.PartNumber;
                _ReportReceivingOrderdetail.PartName = orderdetails.PartName;
                _ReportReceivingOrderdetail.PartModel = orderdetails.PartModel;
                _ReportReceivingOrderdetail.PackingId = orderdetails.PackingId;
                _ReportReceivingOrderdetail.Unit = orderdetails.UnitLevel2;
                _ReportReceivingOrderdetail.UnitQty = systemService.Vn(orderdetails.UnitQty.ToString());
                _ReportReceivingOrderdetail.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
                _ReportReceivingOrderdetail.OrderUnitQty = systemService.Vn(orderdetails.OrderUnitQty.ToString());
                _ReportReceivingOrderdetail.ReceiveQty = systemService.Vn(orderdetails.ReceiveQty.ToString());
                _ReportReceivingOrderdetail.ReceiveUnitQty = systemService.Vn(orderdetails.ReceiveUnitQty.ToString());
                _ReportReceivingOrderdetail.OutstandingQty = systemService.Vn(orderdetails.OutstandingQty.ToString());
                _ReportReceivingOrderdetail.OutstandingUnitQty = systemService.Vn(orderdetails.OutstandingUnitQty.ToString());

                reportReceivingOrderDetail.Add(_ReportReceivingOrderdetail);
            }

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name;
            _ReportCompanyProfile.CompAddress = CompanyProfile.ID + " - " + CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (ReceivingOrder.ReceiveNumber == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_006.rpt"));
                rd.Database.Tables[0].SetDataSource(reportReceivingOrder);
                rd.Database.Tables[1].SetDataSource(reportReceivingOrderDetail);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            QRCode.cleanOrderQr(receivenumber);

            return File(stream, "application/pdf");

        }

        public ActionResult StockList(string InventoryNumber)
        {

            Vw_TRS_StockTaking StockTaking = (from a in vssp.Vw_TRS_StockTaking
                                                  where a.InventoryNumber == InventoryNumber
                                                  select a).First();

            var StockTakingDetail = (from a in vssp.Vw_TRS_StockTakingDetail
                                     join b in vssp.Vw_TRS_StockList on new { a.CustomerId, a.LineId, a.SupplierId, a.PartNumber } equals new { b.CustomerId, b.LineId, b.SupplierId, b.PartNumber } into stock
                                     from b in stock.DefaultIfEmpty()
                                     where a.InventoryNumber == InventoryNumber
                                     orderby b.StockType, a.AreaId, a.LocationId, a.CustomerId, a.LineId, a.SupplierId, a.UniqueNumber, a.PartNumber
                                     select new { a.InventoryNumber, b.StockType, a.CustomerId, a.LineId, a.SupplierId, a.UniqueNumber, a.PartNumber, a.PartName, a.AreaId, a.LocationId, a.CategoryId, a.PartModel, a.UnitQty, a.UnitLevel2, a.PackingId, a.StockKanban, a.StockQty, a.ActualQty, a.BalanceQty }).ToList();


            var StockTakingApproval = from a in vssp.Tbl_TRS_StockTakingApproval
                                        where a.InventoryNumber.Contains(InventoryNumber)
                                        orderby a.ApprovalLevel
                                        select new { a.InventoryNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportStockTakingModel> reportStockTaking = new List<ReportStockTakingModel>();
            List<ReportStockTakingDetailModel> reportStockTakingDetails = new List<ReportStockTakingDetailModel>();
            List<ReportStockTakingApprovalModel> reportStockTakingApproval = new List<ReportStockTakingApprovalModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportStockTakingModel _reportStockTaking = new ReportStockTakingModel();
            _reportStockTaking.InventoryNumber = StockTaking.InventoryNumber;
            _reportStockTaking.InventoryDate = Convert.ToDateTime(systemService.Vd(StockTaking.InventoryDate.ToString()));
            _reportStockTaking.InventoryStartTime = Convert.ToDateTime(systemService.Vd(StockTaking.InventoryStartTime.ToString(),"HH:mm"));
            _reportStockTaking.InventoryEndTime = Convert.ToDateTime(systemService.Vd(StockTaking.InventoryEndTime.ToString(), "HH:mm"));
            _reportStockTaking.TotalPart = systemService.Vn(StockTaking.TotalPart.ToString());
            _reportStockTaking.TotalOrder = systemService.Vn(StockTaking.TotalOrder.ToString());
            _reportStockTaking.Status = systemService.Vn(StockTaking.Status.ToString());
            _reportStockTaking.StatusName = StockTaking.StatusName;
            _reportStockTaking.Approval = StockTaking.Approval;
            _reportStockTaking.ApprovalLevel = systemService.Vn(StockTaking.ApprovalLevel.ToString());
            _reportStockTaking.ApprovalName = StockTaking.ApprovalName;
            _reportStockTaking.Remarks = StockTaking.Remarks;
            _reportStockTaking.UserId = StockTaking.UserId;
            _reportStockTaking.EditDate = Convert.ToDateTime(systemService.Vd(StockTaking.EditDate.ToString()));

            reportStockTaking.Add(_reportStockTaking);


            foreach (var stocklist in StockTakingDetail)
            {
                ReportStockTakingDetailModel _ReportStockTakingdetail = new ReportStockTakingDetailModel();
                _ReportStockTakingdetail.InventoryNumber = stocklist.InventoryNumber;
                _ReportStockTakingdetail.StockType = stocklist.StockType;
                _ReportStockTakingdetail.CustomerId = stocklist.CustomerId;
                _ReportStockTakingdetail.LineId = stocklist.CustomerId;
                _ReportStockTakingdetail.SupplierId = stocklist.SupplierId;
                _ReportStockTakingdetail.UniqueNumber = stocklist.UniqueNumber;
                _ReportStockTakingdetail.PartNumber = stocklist.PartNumber;
                _ReportStockTakingdetail.PartName = stocklist.PartName;
                _ReportStockTakingdetail.AreaId = stocklist.AreaId;
                _ReportStockTakingdetail.LocationId = stocklist.LocationId;
                _ReportStockTakingdetail.CategoryId = stocklist.CategoryId;
                _ReportStockTakingdetail.PartModel = stocklist.PartModel;
                _ReportStockTakingdetail.UnitQty = systemService.Vn(stocklist.UnitQty.ToString());
                _ReportStockTakingdetail.UnitLevel2 = stocklist.UnitLevel2;
                _ReportStockTakingdetail.PackingId = stocklist.PackingId;
                _ReportStockTakingdetail.StockKanban = systemService.Vn(stocklist.StockQty.ToString());
                _ReportStockTakingdetail.StockQty = systemService.Vn(stocklist.StockQty.ToString());
                _ReportStockTakingdetail.ActualQty = systemService.Vn(stocklist.ActualQty.ToString());
                _ReportStockTakingdetail.BalanceQty = systemService.Vn(stocklist.BalanceQty.ToString());

                reportStockTakingDetails.Add(_ReportStockTakingdetail);
            }

            /* Approval */
            int checkuser = 1;
            ReportStockTakingApprovalModel _reportStockTakingApproval = new ReportStockTakingApprovalModel();
            foreach (var orderapprovals in StockTakingApproval)
            {
                _reportStockTakingApproval.InventoryNumber = orderapprovals.InventoryNumber;
                if (orderapprovals.ApprovedDate != null)
                {
                    _reportStockTakingApproval.ApprovedDate = systemService.Vd(orderapprovals.ApprovedDate.ToString(), "dd-MMMM-yyyy");
                }
                switch (orderapprovals.ApprovalLevel.ToString())
                {
                    case "1":
                        _reportStockTakingApproval.UserName1 = orderapprovals.UserName;
                        UserEditModel user1 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user1 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportStockTakingApproval.Sign1 = user1.Sign;
                            }
                        }
                        break;
                    case "2":
                        if (checkuser == 1)
                        {
                            _reportStockTakingApproval.UserName2 = orderapprovals.UserName;
                            UserEditModel user2 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user2 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportStockTakingApproval.Sign2 = user2.Sign;
                                }
                            }
                            checkuser += 1;
                        }
                        else
                        {
                            _reportStockTakingApproval.UserName3 = orderapprovals.UserName;
                            UserEditModel user3 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user3 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportStockTakingApproval.Sign3 = user3.Sign;
                                }
                            }
                        }

                        break;
                    case "4":
                        _reportStockTakingApproval.UserName4 = orderapprovals.UserName;
                        UserEditModel user4 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user4 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportStockTakingApproval.Sign4 = user4.Sign;
                            }
                        }
                        break;
                }

            }
            reportStockTakingApproval.Add(_reportStockTakingApproval);

            

            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
                _ReportCompanyProfile.CompCity = CompanyProfile.City;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (reportStockTaking == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_008.rpt"));

                rd.Database.Tables[0].SetDataSource(reportStockTaking);
                rd.Database.Tables[1].SetDataSource(reportStockTakingDetails);
                rd.Database.Tables[2].SetDataSource(reportStockTakingApproval);
                rd.Database.Tables[3].SetDataSource(reportCompanyProfile);

            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/pdf");

        }
        public ActionResult StockCard(string InventoryNumber)
        {

            Vw_TRS_StockTaking StockTaking = (from a in vssp.Vw_TRS_StockTaking
                                              where a.InventoryNumber == InventoryNumber
                                              select a).First();

            var StockTakingDetail = (from a in vssp.Vw_TRS_StockTakingDetail
                                     join b in vssp.Vw_TRS_StockList on new { a.CustomerId, a.LineId, a.SupplierId, a.PartNumber } equals new { b.CustomerId, b.LineId, b.SupplierId, b.PartNumber } into stock
                                     from b in stock.DefaultIfEmpty()
                                     where a.InventoryNumber == InventoryNumber
                                     orderby b.StockType, a.AreaId, a.LocationId, a.CustomerId, a.LineId, a.SupplierId, a.UniqueNumber, a.PartNumber
                                     select new { a.InventoryNumber, b.StockType, a.CustomerId, a.LineId, a.SupplierId, a.UniqueNumber, a.PartNumber, a.PartName, a.AreaId, a.LocationId, a.CategoryId, a.PartModel, a.UnitQty, a.UnitLevel2, a.PackingId, a.StockKanban, a.StockQty, a.ActualQty, a.BalanceQty }).ToList();


            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportStockTakingModel> reportStockTaking = new List<ReportStockTakingModel>();
            List<ReportStockTakingDetailModel> reportStockTakingDetails = new List<ReportStockTakingDetailModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportStockTakingModel _reportStockTaking = new ReportStockTakingModel();
            _reportStockTaking.InventoryNumber = StockTaking.InventoryNumber;
            _reportStockTaking.InventoryDate = Convert.ToDateTime(systemService.Vd(StockTaking.InventoryDate.ToString()));
            _reportStockTaking.InventoryStartTime = Convert.ToDateTime(systemService.Vd(StockTaking.InventoryStartTime.ToString(), "HH:mm"));
            _reportStockTaking.InventoryEndTime = Convert.ToDateTime(systemService.Vd(StockTaking.InventoryEndTime.ToString(), "HH:mm"));
            _reportStockTaking.TotalPart = systemService.Vn(StockTaking.TotalPart.ToString());
            _reportStockTaking.TotalOrder = systemService.Vn(StockTaking.TotalOrder.ToString());
            _reportStockTaking.Status = systemService.Vn(StockTaking.Status.ToString());
            _reportStockTaking.StatusName = StockTaking.StatusName;
            _reportStockTaking.Approval = StockTaking.Approval;
            _reportStockTaking.ApprovalLevel = systemService.Vn(StockTaking.ApprovalLevel.ToString());
            _reportStockTaking.ApprovalName = StockTaking.ApprovalName;
            _reportStockTaking.Remarks = StockTaking.Remarks;
            _reportStockTaking.UserId = StockTaking.UserId;
            _reportStockTaking.EditDate = Convert.ToDateTime(systemService.Vd(StockTaking.EditDate.ToString()));

            reportStockTaking.Add(_reportStockTaking);


            foreach (var stocklist in StockTakingDetail)
            {
                int loop = 0;
                while(loop<3) {
                    
                    loop += 1;

                    ReportStockTakingDetailModel _ReportStockTakingdetail = new ReportStockTakingDetailModel();
                    _ReportStockTakingdetail.InventoryNumber = stocklist.InventoryNumber;
                    _ReportStockTakingdetail.StockType = stocklist.StockType;
                    _ReportStockTakingdetail.CustomerId = stocklist.CustomerId;
                    _ReportStockTakingdetail.LineId = stocklist.LineId;
                    _ReportStockTakingdetail.SupplierId = stocklist.SupplierId;
                    _ReportStockTakingdetail.UniqueNumber = stocklist.UniqueNumber;
                    _ReportStockTakingdetail.PartNumber = stocklist.PartNumber;
                    _ReportStockTakingdetail.PartName = stocklist.PartName;
                    _ReportStockTakingdetail.AreaId = stocklist.AreaId;
                    _ReportStockTakingdetail.LocationId = stocklist.LocationId;
                    _ReportStockTakingdetail.CategoryId = stocklist.CategoryId;
                    _ReportStockTakingdetail.PartModel = stocklist.PartModel;
                    _ReportStockTakingdetail.UnitQty = systemService.Vn(stocklist.UnitQty.ToString());
                    _ReportStockTakingdetail.UnitLevel2 = stocklist.UnitLevel2;
                    _ReportStockTakingdetail.PackingId = stocklist.PackingId;
                    _ReportStockTakingdetail.StockKanban = systemService.Vn(stocklist.StockQty.ToString());
                    _ReportStockTakingdetail.StockQty = systemService.Vn(stocklist.StockQty.ToString());
                    _ReportStockTakingdetail.ActualQty = systemService.Vn(stocklist.ActualQty.ToString());
                    _ReportStockTakingdetail.BalanceQty = systemService.Vn(stocklist.BalanceQty.ToString());

                    reportStockTakingDetails.Add(_ReportStockTakingdetail);
                }
            }

            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name;
                _ReportCompanyProfile.CompCity = CompanyProfile.City;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (reportStockTaking == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_009.rpt"));

                rd.Database.Tables[0].SetDataSource(reportStockTaking);
                rd.Database.Tables[1].SetDataSource(reportStockTakingDetails);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);

            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/pdf");

        }
        public ActionResult StockAdjustment(string AdjustmentNumber)
        {

            Vw_TRS_StockAdjustment StockAdjustment = (from a in vssp.Vw_TRS_StockAdjustment
                                              where a.AdjustmentNumber == AdjustmentNumber
                                              select a).First();

            var StockAdjustmentDetail = (from a in vssp.Vw_TRS_StockAdjustmentDetail
                                         join b in vssp.Vw_TRS_StockList on new { a.CustomerId, a.LineId, a.SupplierId, a.PartNumber } equals new { b.CustomerId, b.LineId, b.SupplierId, b.PartNumber } into stock
                                         from b in stock.DefaultIfEmpty()
                                         where a.AdjustmentNumber == AdjustmentNumber || a.AdjustmentNumber.Replace("/", "") == AdjustmentNumber
                                         orderby b.StockType, a.AreaId, a.LocationId, a.CustomerId, a.LineId, a.SupplierId, a.UniqueNumber, a.PartNumber
                                         select new { a.AdjustmentNumber, a.CustomerId, a.LineId, a.SupplierId, b.StockType, a.UniqueNumber, a.PartNumber, a.PartName, a.AreaId, a.LocationId, a.CategoryId, a.PartModel, a.UnitQty, a.UnitLevel2, a.PackingId, a.StockKanban, a.StockQty, a.ActualQty, a.BalanceQty, a.AdjustmentQty }).ToList();



            var StockAdjustmentApproval = from a in vssp.Tbl_TRS_StockAdjustmentApproval
                                      where a.AdjustmentNumber.Contains(AdjustmentNumber)
                                      orderby a.ApprovalLevel
                                      select new { a.AdjustmentNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportStockAdjustmentModel> reportStockAdjustment = new List<ReportStockAdjustmentModel>();
            List<ReportStockAdjustmentDetailModel> reportStockAdjustmentDetails = new List<ReportStockAdjustmentDetailModel>();
            List<ReportStockAdjustmentApprovalModel> reportStockAdjustmentApproval = new List<ReportStockAdjustmentApprovalModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportStockAdjustmentModel _reportStockAdjustment = new ReportStockAdjustmentModel();
            _reportStockAdjustment.AdjustmentNumber = StockAdjustment.AdjustmentNumber;
            _reportStockAdjustment.AdjustmentDate = Convert.ToDateTime(systemService.Vd(StockAdjustment.AdjustmentDate.ToString()));
            _reportStockAdjustment.InventoryNumber = StockAdjustment.InventoryNumber;
            _reportStockAdjustment.AreaId = StockAdjustment.AreaId;
            _reportStockAdjustment.LocationId = StockAdjustment.LocationId;
            _reportStockAdjustment.TotalPart = systemService.Vn(StockAdjustment.TotalPart.ToString());
            _reportStockAdjustment.TotalOrder = systemService.Vn(StockAdjustment.TotalOrder.ToString());
            _reportStockAdjustment.Status = systemService.Vn(StockAdjustment.Status.ToString());
            _reportStockAdjustment.StatusName = StockAdjustment.StatusName;
            _reportStockAdjustment.Approval = StockAdjustment.Approval;
            _reportStockAdjustment.ApprovalLevel = systemService.Vn(StockAdjustment.ApprovalLevel.ToString());
            _reportStockAdjustment.ApprovalName = StockAdjustment.ApprovalName;
            _reportStockAdjustment.Remarks = StockAdjustment.Remarks;
            _reportStockAdjustment.UserId = StockAdjustment.UserId;
            _reportStockAdjustment.EditDate = Convert.ToDateTime(systemService.Vd(StockAdjustment.EditDate.ToString()));

            reportStockAdjustment.Add(_reportStockAdjustment);


            foreach (var stocklist in StockAdjustmentDetail)
            {
                ReportStockAdjustmentDetailModel _ReportStockAdjustmentdetail = new ReportStockAdjustmentDetailModel();
                _ReportStockAdjustmentdetail.AdjustmentNumber = stocklist.AdjustmentNumber;
                _ReportStockAdjustmentdetail.CustomerId = stocklist.CustomerId;
                _ReportStockAdjustmentdetail.LineId = stocklist.LineId;
                _ReportStockAdjustmentdetail.SupplierId = stocklist.SupplierId;
                _ReportStockAdjustmentdetail.StockType = stocklist.StockType;
                _ReportStockAdjustmentdetail.UniqueNumber = stocklist.UniqueNumber;
                _ReportStockAdjustmentdetail.PartNumber = stocklist.PartNumber;
                _ReportStockAdjustmentdetail.PartName = stocklist.PartName;
                _ReportStockAdjustmentdetail.AreaId = stocklist.AreaId;
                _ReportStockAdjustmentdetail.LocationId = stocklist.LocationId;
                _ReportStockAdjustmentdetail.CategoryId = stocklist.CategoryId;
                _ReportStockAdjustmentdetail.PartModel = stocklist.PartModel;
                _ReportStockAdjustmentdetail.UnitQty = systemService.Vn(stocklist.UnitQty.ToString());
                _ReportStockAdjustmentdetail.UnitLevel2 = stocklist.UnitLevel2;
                _ReportStockAdjustmentdetail.PackingId = stocklist.PackingId;
                _ReportStockAdjustmentdetail.StockKanban = systemService.Vn(stocklist.StockQty.ToString());
                _ReportStockAdjustmentdetail.StockQty = systemService.Vn(stocklist.StockQty.ToString());
                _ReportStockAdjustmentdetail.ActualQty = systemService.Vn(stocklist.ActualQty.ToString());
                _ReportStockAdjustmentdetail.BalanceQty = systemService.Vn(stocklist.BalanceQty.ToString());
                _ReportStockAdjustmentdetail.AdjustmentQty = systemService.Vn(stocklist.AdjustmentQty.ToString());

                reportStockAdjustmentDetails.Add(_ReportStockAdjustmentdetail);
            }

            /* Approval */
            int checkuser = 1;
            ReportStockAdjustmentApprovalModel _reportStockAdjustmentApproval = new ReportStockAdjustmentApprovalModel();
            foreach (var orderapprovals in StockAdjustmentApproval)
            {
                _reportStockAdjustmentApproval.AdjustmentNumber = orderapprovals.AdjustmentNumber;
                if (orderapprovals.ApprovedDate != null)
                {
                    _reportStockAdjustmentApproval.ApprovedDate = systemService.Vd(orderapprovals.ApprovedDate.ToString(), "dd-MMMM-yyyy");
                }
                switch (orderapprovals.ApprovalLevel.ToString())
                {
                    case "1":
                        _reportStockAdjustmentApproval.UserName1 = orderapprovals.UserName;
                        UserEditModel user1 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user1 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportStockAdjustmentApproval.Sign1 = user1.Sign;
                            }
                        }
                        break;
                    case "2":
                        if (checkuser == 1)
                        {
                            _reportStockAdjustmentApproval.UserName2 = orderapprovals.UserName;
                            UserEditModel user2 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user2 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportStockAdjustmentApproval.Sign2 = user2.Sign;
                                }
                            }
                            checkuser += 1;
                        }
                        else
                        {
                            _reportStockAdjustmentApproval.UserName3 = orderapprovals.UserName;
                            UserEditModel user3 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                            if (user3 != null)
                            {
                                if (orderapprovals.Approved == true)
                                {
                                    _reportStockAdjustmentApproval.Sign3 = user3.Sign;
                                }
                            }
                        }

                        break;
                    case "4":
                        _reportStockAdjustmentApproval.UserName4 = orderapprovals.UserName;
                        UserEditModel user4 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user4 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportStockAdjustmentApproval.Sign4 = user4.Sign;
                            }
                        }
                        break;
                }

            }
            reportStockAdjustmentApproval.Add(_reportStockAdjustmentApproval);



            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
                _ReportCompanyProfile.CompCity = CompanyProfile.City;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (reportStockAdjustment == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_010.rpt"));

                rd.Database.Tables[0].SetDataSource(reportStockAdjustment);
                rd.Database.Tables[1].SetDataSource(reportStockAdjustmentDetails);
                rd.Database.Tables[2].SetDataSource(reportStockAdjustmentApproval);
                rd.Database.Tables[3].SetDataSource(reportCompanyProfile);

            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/pdf");

        }
        public ActionResult InvoiceRecaps(string RecapNumber, string FileFormat)
        {

            List<Vw_ACC_InvoiceRecap> InvoiceRecap = (from a in vssp.Vw_ACC_InvoiceRecap
                                                        where a.RecapNumber == RecapNumber
                                                        select a).ToList();

            List<Vw_ACC_InvoiceRecapDetail> InvoiceRecapDetail = (from a in vssp.Vw_ACC_InvoiceRecapDetail
                                                                     where a.RecapNumber == RecapNumber
                                                                     select a).ToList();

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportInvoiceRecapModel> reportInvoiceRecap = new List<ReportInvoiceRecapModel>();
            List<ReportInvoiceRecapDetailsModel> reportInvoiceRecapDetails = new List<ReportInvoiceRecapDetailsModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            foreach (var orders in InvoiceRecap)
            {
                ReportInvoiceRecapModel invoiceRecap = new ReportInvoiceRecapModel();
                invoiceRecap.RecapNumber = orders.RecapNumber;
                invoiceRecap.RecapDate = Convert.ToDateTime(systemService.Vd(orders.RecapDate.ToString()));
                invoiceRecap.RecapYear = orders.RecapYear;
                invoiceRecap.RecapMonth = orders.RecapMonth;
                invoiceRecap.InvoiceNumber = orders.InvoiceNumber;
                invoiceRecap.ReceiveNote = orders.ReceiveNote;
                invoiceRecap.TaxInvoice = orders.TaxInvoice;
                invoiceRecap.SupplierId = orders.SupplierId;
                invoiceRecap.SupplierName = orders.SupplierName;
                invoiceRecap.TotalPrice = systemService.Vn(orders.TotalPrice.ToString());
                invoiceRecap.PPN = systemService.Vn(orders.PPN.ToString());
                invoiceRecap.PPH23 = systemService.Vn(orders.PPH23.ToString());
                invoiceRecap.DebitNote = systemService.Vn(orders.DebitNote.ToString());
                invoiceRecap.Payment = systemService.Vn(orders.Payment.ToString());
                invoiceRecap.TotalPart = systemService.Vn(orders.TotalPart.ToString());
                invoiceRecap.TotalRecap = systemService.Vn(orders.TotalRecap.ToString());
                invoiceRecap.Status = systemService.Vn(orders.Status.ToString());
                invoiceRecap.StatusName = orders.StatusName;
                invoiceRecap.Approval = orders.Approval;
                invoiceRecap.ApprovalLevel = systemService.Vn(orders.ApprovalLevel.ToString());
                invoiceRecap.ApprovalName = orders.ApprovalName;
                invoiceRecap.Paid = systemService.Vb(orders.Paid.ToString());
                invoiceRecap.Remarks = orders.Remarks;
                invoiceRecap.UserId = orders.UserId;
                invoiceRecap.EditDate = Convert.ToDateTime(systemService.Vd(orders.EditDate.ToString()));

                reportInvoiceRecap.Add(invoiceRecap);
            }
            foreach (var ordersetails in InvoiceRecapDetail)
            {
                ReportInvoiceRecapDetailsModel invoiceRecapDetaildetail = new ReportInvoiceRecapDetailsModel();
                invoiceRecapDetaildetail.RecapNumber = ordersetails.RecapNumber;
                invoiceRecapDetaildetail.OrderNumber = ordersetails.OrderNumber;
                invoiceRecapDetaildetail.ReceiveNumber = ordersetails.ReceiveNumber;
                invoiceRecapDetaildetail.ReceiveDate = Convert.ToDateTime(systemService.Vd(ordersetails.ReceiveDate.ToString()));
                invoiceRecapDetaildetail.SupplierId = ordersetails.SupplierId;
                invoiceRecapDetaildetail.UniqueNumber = ordersetails.UniqueNumber;
                invoiceRecapDetaildetail.PartNumber = ordersetails.PartNumber;
                invoiceRecapDetaildetail.PartName = ordersetails.PartName;
                invoiceRecapDetaildetail.PartModel = ordersetails.PartModel;
                invoiceRecapDetaildetail.ClassificationId = ordersetails.ClassificationId;
                invoiceRecapDetaildetail.PaymentId = ordersetails.PaymentId;
                invoiceRecapDetaildetail.CategoryId = ordersetails.CategoryId;
                invoiceRecapDetaildetail.RecapQty = systemService.Vn(ordersetails.RecapQty.ToString());
                invoiceRecapDetaildetail.PriceQty = systemService.Vn(ordersetails.PriceQty.ToString());
                invoiceRecapDetaildetail.TotalPrice = systemService.Vn(ordersetails.TotalPrice.ToString());
                invoiceRecapDetaildetail.PPN = systemService.Vn(ordersetails.PPN.ToString());
                invoiceRecapDetaildetail.PPH23 = systemService.Vn(ordersetails.PPH23.ToString());
                invoiceRecapDetaildetail.Payment = systemService.Vn(ordersetails.Payment.ToString());

                reportInvoiceRecapDetails.Add(invoiceRecapDetaildetail);
            }
            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name;
                _ReportCompanyProfile.CompCity = CompanyProfile.City;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (InvoiceRecap.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_ACC_001.rpt"));
                rd.Database.Tables[0].SetDataSource(reportInvoiceRecap);
                rd.Database.Tables[1].SetDataSource(reportInvoiceRecapDetails);
                rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (FileFormat == "Pdf")
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                stream.Seek(0, SeekOrigin.Begin);

                return File(stream, "application/pdf");
            } else
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.ExcelRecord);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/vnd.ms-excel");
            }

        }

        public ActionResult RawMaterialTransaction(string SupplierId, string PartNumber, Nullable<DateTime> StartDate, Nullable<DateTime> EndDate, string FileFormat)
        {
            if (StartDate == null)
            {
                StartDate = DateTime.Now;
            }
            if (EndDate == null)
            {
                EndDate = DateTime.Now;
            }

            List<SP_TRS_RawMaterialTransaction_Result> RawMaterialTransaction = vssp.SP_TRS_RawMaterialTransaction(SupplierId, PartNumber, StartDate, EndDate).ToList();

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportRawMaterialTransactionModel> reportRawMaterialTransaction = new List<ReportRawMaterialTransactionModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            foreach (var transaction in RawMaterialTransaction)
            {
                ReportRawMaterialTransactionModel rawMaterialTransaction = new ReportRawMaterialTransactionModel();
                rawMaterialTransaction.SupplierId = transaction.SupplierId;
                rawMaterialTransaction.SupplierName = transaction.SupplierName;
                rawMaterialTransaction.Date_Process = Convert.ToDateTime(systemService.Vd(transaction.Date_Process.ToString()));
                rawMaterialTransaction.UniqueNumber = transaction.UniqueNumber;
                rawMaterialTransaction.PartNumber = transaction.PartNumber;
                rawMaterialTransaction.PartName = transaction.PartName;
                rawMaterialTransaction.PartModel = transaction.PartModel;
                rawMaterialTransaction.ReceiveKanban = systemService.Vn(transaction.ReceiveKanban.ToString());
                rawMaterialTransaction.ReceiveQty = systemService.Vn(transaction.ReceiveQty.ToString());
                rawMaterialTransaction.DeliveryKanban = systemService.Vn(transaction.DeliveryKanban.ToString());
                rawMaterialTransaction.DeliveryQty = systemService.Vn(transaction.DeliveryQty.ToString());

                reportRawMaterialTransaction.Add(rawMaterialTransaction);
            }
            
            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name;
                _ReportCompanyProfile.CompCity = CompanyProfile.City;
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }

            ReportDocument rd = new ReportDocument();

            if (reportRawMaterialTransaction.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_011.rpt"));
                rd.Database.Tables[0].SetDataSource(reportRawMaterialTransaction);
                rd.Database.Tables[1].SetDataSource(reportCompanyProfile);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (FileFormat.ToLower() == "pdf")
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

        //public ActionResult SSPDeliveryOrder(string ordernumber)
        //{

        //    //Vw_TRS_SupplierOrder SupplierOrder = (from a in vssp.Vw_TRS_SupplierOrder
        //    //                                      where a.OrderNumber == ordernumber
        //    //                                            select a).FirstOrDefault();

        //    //QRCode.generateOrderQr(ordernumber, "SupplierOrder");

        //    var SupplierOrder = (from a in vssp.Vw_TRS_SupplierOrder
        //                         join b in vssp.Tbl_TRS_QrCodePath on a.OrderNumber equals b.DocId into qrcode
        //                         join c in vssp.Tbl_MST_Supplier on a.SupplierId equals c.SupplierId
        //                         from b in qrcode.DefaultIfEmpty()
        //                         where a.OrderNumber == ordernumber
        //                         select new
        //                         {
        //                             a.OrderNumber,
        //                             a.OrderDate,
        //                             a.SupplierId,
        //                             a.SupplierName,
        //                             c.Address,
        //                             c.City,
        //                             a.SSP,
        //                             a.ProcessName,
        //                             a.KanbanCycle,
        //                             a.IncomingDate,
        //                             a.IncomingTime,
        //                             a.Shift,
        //                             a.TotalPart,
        //                             a.TotalOrder,
        //                             a.Status,
        //                             a.StatusName,
        //                             a.Remarks,
        //                             b.QrcodePath,
        //                             b.BarcodePath,
        //                             a.UserId,
        //                             a.EditDate
        //                         }).FirstOrDefault();

        //    var SupplierOrderDetail = from a in vssp.Tbl_TRS_SupplierOrderDetail
        //                              join b in vssp.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber }
        //                              where a.OrderNumber == ordernumber
        //                              select new
        //                              {
        //                                  a.OrderNumber,
        //                                  a.SupplierId,
        //                                  b.UniqueNumber,
        //                                  a.PartNumber,
        //                                  b.PartName,
        //                                  b.UnitQty,
        //                                  b.UnitLevel1,
        //                                  b.UnitLevel2,
        //                                  b.PackingId,
        //                                  b.PartModel,
        //                                  a.OrderQty,
        //                                  a.OrderUnitQty,
        //                                  a.ReceiveQty
        //                              };

        //    var CompanyProfile = systemService.GetLicenseInfo();

        //    List<ReportSupplierOrderModel> reportSupplierOrder = new List<ReportSupplierOrderModel>();
        //    List<ReportSupplierOrderDetailModel> reportSupplierOrderDetail = new List<ReportSupplierOrderDetailModel>();
        //    List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

        //    ReportSupplierOrderModel _ReportSupplierOrder = new ReportSupplierOrderModel();
        //    _ReportSupplierOrder.OrderNumber = SupplierOrder.OrderNumber;
        //    _ReportSupplierOrder.OrderDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.OrderDate.ToString()));
        //    _ReportSupplierOrder.SupplierId = SupplierOrder.SupplierId;
        //    _ReportSupplierOrder.SupplierName = SupplierOrder.SupplierName;
        //    _ReportSupplierOrder.SupplierAddress = SupplierOrder.Address + ", " + SupplierOrder.City;
        //    _ReportSupplierOrder.SSP = SupplierOrder.SSP;
        //    _ReportSupplierOrder.ProcessName = SupplierOrder.ProcessName;
        //    _ReportSupplierOrder.KanbanCycle = SupplierOrder.KanbanCycle;
        //    _ReportSupplierOrder.IncomingDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingDate.ToString()));
        //    _ReportSupplierOrder.IncomingTime = Convert.ToDateTime(systemService.Vd(SupplierOrder.IncomingTime.ToString(), "HH:mm"));
        //    _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
        //    _ReportSupplierOrder.TotalPart = systemService.Vn(SupplierOrder.TotalPart.ToString());
        //    _ReportSupplierOrder.TotalOrder = systemService.Vn(SupplierOrder.TotalOrder.ToString());
        //    _ReportSupplierOrder.Shift = systemService.Vn(SupplierOrder.Shift.ToString());
        //    _ReportSupplierOrder.Remarks = SupplierOrder.Remarks;
        //    _ReportSupplierOrder.Status = systemService.Vn(SupplierOrder.Status.ToString());
        //    _ReportSupplierOrder.StatusName = SupplierOrder.StatusName;
        //    _ReportSupplierOrder.UserId = SupplierOrder.UserId;
        //    _ReportSupplierOrder.EditDate = Convert.ToDateTime(systemService.Vd(SupplierOrder.EditDate.ToString()));
        //    _ReportSupplierOrder.QrCode = "";

        //    reportSupplierOrder.Add(_ReportSupplierOrder);

        //    foreach (var orderdetails in SupplierOrderDetail)
        //    {
        //        ReportSupplierOrderDetailModel _ReportSupplierOrderdetail = new ReportSupplierOrderDetailModel();
        //        _ReportSupplierOrderdetail.OrderNumber = orderdetails.OrderNumber;
        //        _ReportSupplierOrderdetail.SupplierId = orderdetails.SupplierId;
        //        _ReportSupplierOrderdetail.UniqueNumber = orderdetails.UniqueNumber;
        //        _ReportSupplierOrderdetail.PartNumber = orderdetails.PartNumber;
        //        _ReportSupplierOrderdetail.PartName = orderdetails.PartName;
        //        _ReportSupplierOrderdetail.PartModel = orderdetails.PartModel;
        //        _ReportSupplierOrderdetail.PackingId = orderdetails.PackingId;
        //        _ReportSupplierOrderdetail.UnitLevel1 = orderdetails.UnitLevel1;
        //        _ReportSupplierOrderdetail.UnitLevel2 = orderdetails.UnitLevel2;
        //        _ReportSupplierOrderdetail.UnitQty = systemService.Vn(orderdetails.UnitQty.ToString());
        //        _ReportSupplierOrderdetail.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
        //        _ReportSupplierOrderdetail.OrderUnitQty = systemService.Vn(orderdetails.OrderUnitQty.ToString());
        //        _ReportSupplierOrderdetail.ReceiveQty = systemService.Vn(orderdetails.ReceiveQty.ToString());
        //        _ReportSupplierOrderdetail.QrCode = "";
        //        //_ReportSupplierOrderdetail.QrCode = "http://chart.googleapis.com/chart?chs=150x150&cht=qr&chl=" + orderdetails.OrderNumber + ";" + orderdetails.SupplierId + ";" + orderdetails.UniqueNumber + ";" + orderdetails.PartNumber + ";" + orderdetails.PartName + ";" + orderdetails.OrderQty + ";" + orderdetails.OrderUnitQty + "&choe=UTF-8";

        //        reportSupplierOrderDetail.Add(_ReportSupplierOrderdetail);
        //    }

        //    ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
        //    _ReportCompanyProfile.CompId = CompanyProfile.ID;
        //    _ReportCompanyProfile.CompName = CompanyProfile.Name;
        //    _ReportCompanyProfile.CompAddress = CompanyProfile.ID + " - " + CompanyProfile.Address;
        //    _ReportCompanyProfile.CompCity = CompanyProfile.City;
        //    _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
        //    _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

        //    reportCompanyProfile.Add(_ReportCompanyProfile);

        //    ReportDocument rd = new ReportDocument();

        //    if (SupplierOrder.OrderNumber == null)
        //    {
        //        rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
        //    }
        //    else
        //    {
        //        rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_011.rpt"));
        //        rd.Database.Tables[0].SetDataSource(reportSupplierOrder);
        //        rd.Database.Tables[1].SetDataSource(reportSupplierOrderDetail);
        //        rd.Database.Tables[2].SetDataSource(reportCompanyProfile);
        //    }

        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //    stream.Seek(0, SeekOrigin.Begin);

        //    QRCode.cleanOrderQr(ordernumber);

        //    return File(stream, "application/pdf");

        //}


        public ActionResult ApprovedCustomListJson()
        {
            var data = (from a in vssp.Tbl_MST_ApprovedCustom select new { a.NameApproved, a.Jabatan }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InvoiceNotaFaktur(string invoicenumber, string FileFormat, string ApprovedName = "")
        {

            var customerInvoice = (from a in vssp.Vw_ACC_CustomerInvoice
                                   join b in vssp.Tbl_MST_Customer on a.CustomerId equals b.CustomerId into customer
                                   from b in customer.DefaultIfEmpty()
                                   where a.InvoiceNumber == invoicenumber
                                   select new
                                   {
                                       a.InvoiceNumber,
                                       a.InvoiceDate,
                                       a.CustomerId,
                                       a.CustomerName,
                                       b.Address,
                                       b.City,
                                       b.Provience,
                                       b.Country,
                                       b.PostalCode,
                                       a.Terms,
                                       a.Remarks,
                                       a.SubTotal,
                                       a.PPN,
                                       a.PPH23,
                                       a.GrandTotal,
                                       a.Status,
                                       a.StatusName,
                                       a.ApprovalName
                                   }).FirstOrDefault();

            var customerInvoiceDetails = (from a in vssp.Vw_ACC_CustomerInvoiceDetail
                                          where a.InvoiceNumber == invoicenumber
                                          select a).ToList();

            var customerInvoiceApproval = (from a in vssp.Tbl_ACC_CustomerInvoiceApproval
                                           where a.InvoiceNumber == invoicenumber && a.ApprovalLevel == 3
                                           select new { a.UserId, a.UserName }).FirstOrDefault();

            var bankAccount = (from a in vssp.Tbl_MST_AccountingBankAccount
                               join b in vssp.Tbl_MST_Bank on a.BankId equals b.BankId
                               where a.StartDate <= customerInvoice.InvoiceDate && (a.EndDate == null ? customerInvoice.InvoiceDate : a.EndDate) >= customerInvoice.InvoiceDate
                               orderby a.StartDate descending
                               select new { a.BankId, b.BankName, a.Branch, a.AccountName, a.AccountNumber }).FirstOrDefault();
            var dataApproved = vssp.Tbl_MST_ApprovedCustom.Where(x => x.NameApproved.Equals(ApprovedName)).FirstOrDefault();
            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportCustomerInvoiceModel> reportCustomerInvoices = new List<ReportCustomerInvoiceModel>();
            List<ReportCustomerInvoiceDetailModel> reportCustomerInvoiceDetails = new List<ReportCustomerInvoiceDetailModel>();
            List<ReportAccountingBankAccountModel> reportAccountingBankAccounts = new List<ReportAccountingBankAccountModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportCustomerInvoiceModel _ReportCustomerInvoice = new ReportCustomerInvoiceModel();
            _ReportCustomerInvoice.InvoiceNumber = customerInvoice.InvoiceNumber;
            _ReportCustomerInvoice.InvoiceDate = Convert.ToDateTime(systemService.Vd(customerInvoice.InvoiceDate.ToString()));
            _ReportCustomerInvoice.CustomerId = customerInvoice.CustomerId;
            _ReportCustomerInvoice.CustomerName = customerInvoice.CustomerName;
            _ReportCustomerInvoice.CustomerAddress1 = customerInvoice.Address;
            _ReportCustomerInvoice.CustomerAddress2 = customerInvoice.City + ", " + customerInvoice.Provience + ", " + customerInvoice.Country + " " + customerInvoice.PostalCode;
            _ReportCustomerInvoice.SubTotal = systemService.Vn(customerInvoice.SubTotal.ToString());
            _ReportCustomerInvoice.PPN = systemService.Vn(customerInvoice.PPN.ToString());
            _ReportCustomerInvoice.PPH23 = systemService.Vn(customerInvoice.PPH23.ToString());
            _ReportCustomerInvoice.GrandTotal = systemService.Vn(customerInvoice.GrandTotal.ToString());
            _ReportCustomerInvoice.Terms = customerInvoice.Terms;
            _ReportCustomerInvoice.Remarks = customerInvoice.Remarks;
            _ReportCustomerInvoice.Status = int.Parse(customerInvoice.Status.ToString());
            _ReportCustomerInvoice.StatusName = customerInvoice.StatusName;
            _ReportCustomerInvoice.ApprovalName = customerInvoiceApproval == null ? "" : ApprovedName;
            //_ReportCustomerInvoice.ApprovalName = dataApproved.NameApproved;
            reportCustomerInvoices.Add(_ReportCustomerInvoice);

            foreach (var detail in customerInvoiceDetails)
            {
                ReportCustomerInvoiceDetailModel _ReportCustomerInvoiceDetail = new ReportCustomerInvoiceDetailModel();
                _ReportCustomerInvoiceDetail.InvoiceNumber = detail.InvoiceNumber;
                _ReportCustomerInvoiceDetail.CustomerId = detail.CustomerId;
                _ReportCustomerInvoiceDetail.UniqueNumber = detail.UniqueNumber;
                _ReportCustomerInvoiceDetail.PartNumber = detail.PartNumber;
                _ReportCustomerInvoiceDetail.PartName = detail.PartName;
                _ReportCustomerInvoiceDetail.UnitLevel2 = detail.UnitLevel2;
                _ReportCustomerInvoiceDetail.PriceUnit = systemService.Vn(detail.PriceUnit.ToString());
                _ReportCustomerInvoiceDetail.InvoiceQty = systemService.Vn(detail.InvoiceQty.ToString());
                _ReportCustomerInvoiceDetail.Amount = systemService.Vn(detail.Amount.ToString());

                reportCustomerInvoiceDetails.Add(_ReportCustomerInvoiceDetail);
            }

            ReportAccountingBankAccountModel _ReportAccountingBankAccount = new ReportAccountingBankAccountModel();
            _ReportAccountingBankAccount.BankId = bankAccount != null ? bankAccount.BankId : "";
            _ReportAccountingBankAccount.BankName = bankAccount != null ? bankAccount.BankName : ""; ;
            _ReportAccountingBankAccount.Branch = bankAccount != null ? bankAccount.Branch : ""; ;
            _ReportAccountingBankAccount.AccountName = bankAccount != null ? bankAccount.AccountName : ""; ;
            _ReportAccountingBankAccount.AccountNumber = bankAccount != null ? bankAccount.AccountNumber : ""; 

            reportAccountingBankAccounts.Add(_ReportAccountingBankAccount);

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
            _ReportCompanyProfile.CompAddress = CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.Phone1 = CompanyProfile.Phone1;
            _ReportCompanyProfile.Phone2 = CompanyProfile.Phone2;
            _ReportCompanyProfile.Fax = CompanyProfile.Fax;
            _ReportCompanyProfile.CompLogo = systemService.ByteToImage(CompanyProfile.Logo);

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (customerInvoice.InvoiceNumber == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_012.rpt"));
                rd.Database.Tables[0].SetDataSource(reportCompanyProfile);
                rd.Database.Tables[1].SetDataSource(reportCustomerInvoices);
                rd.Database.Tables[2].SetDataSource(reportCustomerInvoiceDetails);
                rd.Database.Tables[3].SetDataSource(reportAccountingBankAccounts);
            }
            rd.SetParameterValue("DepartmentName", dataApproved == null ? "" : dataApproved.Jabatan);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (FileFormat.ToLower() == "pdf")
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
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/vnd.ms-excel");
            }            

        }
        public ActionResult InvoiceRecapDelivery(string invoicenumber, string FileFormat)
        {

            var customerInvoice = (from a in vssp.Vw_ACC_CustomerInvoice
                                   join b in vssp.Tbl_MST_Customer on a.CustomerId equals b.CustomerId into customer
                                   from b in customer.DefaultIfEmpty()
                                   where a.InvoiceNumber == invoicenumber
                                   select new
                                   {
                                       a.InvoiceNumber,
                                       a.InvoiceDate,
                                       a.CustomerId,
                                       a.CustomerName,
                                       b.Address,
                                       b.City,
                                       b.Provience,
                                       b.Country,
                                       b.PostalCode,
                                       a.Terms,
                                       a.Remarks,
                                       a.SubTotal,
                                       a.PPN,
                                       a.GrandTotal,
                                       a.Status,
                                       a.StatusName,
                                       a.ApprovalName
                                   }).FirstOrDefault();

            //var customerInvoiceDetails = (from a in vssp.Vw_ACC_CustomerInvoiceDeliveryRecap
            //                              where a.InvoiceNumber == invoicenumber
            //                              orderby a.DODate, a.DONumber, a.PartNumber
            //                              select a).ToList();

            vssp.Database.CommandTimeout = 0;
            var customerInvoiceDetails = vssp.Database.SqlQuery<Vw_ACC_CustomerInvoiceDeliveryRecap>("EXEC sp_GetCustomerInvoiceDeliveryRecap @InvoiceNumber", new SqlParameter("@InvoiceNumber", invoicenumber)).ToList();

            //return Json(customerInvoiceDetails, JsonRequestBehavior.AllowGet);
            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportCustomerInvoiceModel> reportCustomerInvoices = new List<ReportCustomerInvoiceModel>();
            //List<ReportCustomerInvoiceDeliveryRecapModel> reportCustomerInvoiceDetails = new List<ReportCustomerInvoiceDeliveryRecapModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportCustomerInvoiceModel _ReportCustomerInvoice = new ReportCustomerInvoiceModel();
            _ReportCustomerInvoice.InvoiceNumber = customerInvoice.InvoiceNumber;
            _ReportCustomerInvoice.InvoiceDate = Convert.ToDateTime(systemService.Vd(customerInvoice.InvoiceDate.ToString()));
            _ReportCustomerInvoice.CustomerId = customerInvoice.CustomerId;
            _ReportCustomerInvoice.CustomerName = customerInvoice.CustomerName;
            _ReportCustomerInvoice.CustomerAddress1 = customerInvoice.Address;
            _ReportCustomerInvoice.CustomerAddress2 = customerInvoice.City + ", " + customerInvoice.Provience + ", " + customerInvoice.Country + " " + customerInvoice.PostalCode;
            _ReportCustomerInvoice.SubTotal = systemService.Vn(customerInvoice.SubTotal.ToString());
            _ReportCustomerInvoice.PPN = systemService.Vn(customerInvoice.PPN.ToString());
            _ReportCustomerInvoice.GrandTotal = systemService.Vn(customerInvoice.GrandTotal.ToString());
            _ReportCustomerInvoice.Terms = customerInvoice.Terms;
            _ReportCustomerInvoice.Remarks = customerInvoice.Remarks;
            _ReportCustomerInvoice.Status = int.Parse(customerInvoice.Status.ToString());
            _ReportCustomerInvoice.StatusName = customerInvoice.StatusName;
            _ReportCustomerInvoice.ApprovalName = "";

            reportCustomerInvoices.Add(_ReportCustomerInvoice);

            List<ReportCustomerInvoiceDeliveryRecapModel> reportCustomerInvoiceDetails = customerInvoiceDetails
            .Select(detail => new ReportCustomerInvoiceDeliveryRecapModel
            {
                CustomerId = detail.CustomerId,
                CustomerName = detail.CustomerName,
                DONumber = detail.DONumber,
                DODate = Convert.ToDateTime(systemService.Vd(detail.DODate.ToString())),
                SONumber = detail.SONumber,
                PONumber = detail.PONumber,
                PODate = Convert.ToDateTime(systemService.Vd(detail.PODate.ToString())),
                RefNumber = detail.RefNumber,
                InvoiceNumber = detail.InvoiceNumber,
                UniqueNumber = detail.UniqueNumber,
                PartNumber = detail.PartNumber,
                PartName = detail.PartName,
                UnitLevel2 = detail.UnitLevel2,
                Price = systemService.Vn(detail.Price.ToString()),
                DeliveryUnitQty = systemService.Vn(detail.DeliveryUnitQty.ToString()),
                Amount = systemService.Vn(detail.Amount.ToString()),
                Invoiced = systemService.Vb(detail.Invoiced.ToString())
            })
            .ToList();

            //foreach (var detail in customerInvoiceDetails)
            //{
            //    ReportCustomerInvoiceDeliveryRecapModel _ReportCustomerInvoiceDetail = new ReportCustomerInvoiceDeliveryRecapModel();
            //    _ReportCustomerInvoiceDetail.CustomerId = detail.CustomerId;
            //    _ReportCustomerInvoiceDetail.CustomerName = detail.CustomerName;
            //    _ReportCustomerInvoiceDetail.DONumber = detail.DONumber;
            //    _ReportCustomerInvoiceDetail.DODate = Convert.ToDateTime(systemService.Vd(detail.DODate.ToString()));
            //    _ReportCustomerInvoiceDetail.SONumber = detail.SONumber;
            //    _ReportCustomerInvoiceDetail.PONumber = detail.PONumber;
            //    _ReportCustomerInvoiceDetail.PODate = Convert.ToDateTime(systemService.Vd(detail.PODate.ToString()));
            //    _ReportCustomerInvoiceDetail.RefNumber = detail.RefNumber;
            //    _ReportCustomerInvoiceDetail.InvoiceNumber = detail.InvoiceNumber;
            //    _ReportCustomerInvoiceDetail.UniqueNumber = detail.UniqueNumber;
            //    _ReportCustomerInvoiceDetail.PartNumber = detail.PartNumber;
            //    _ReportCustomerInvoiceDetail.PartName = detail.PartName;
            //    _ReportCustomerInvoiceDetail.UnitLevel2 = detail.UnitLevel2;
            //    _ReportCustomerInvoiceDetail.Price = systemService.Vn(detail.Price.ToString());
            //    _ReportCustomerInvoiceDetail.DeliveryUnitQty = systemService.Vn(detail.DeliveryUnitQty.ToString());
            //    _ReportCustomerInvoiceDetail.Amount = systemService.Vn(detail.Amount.ToString());
            //    _ReportCustomerInvoiceDetail.Invoiced = systemService.Vb(detail.Invoiced.ToString());

            //    reportCustomerInvoiceDetails.Add(_ReportCustomerInvoiceDetail);
            //}

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
            _ReportCompanyProfile.CompAddress = CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.Phone1 = CompanyProfile.Phone1;
            _ReportCompanyProfile.Phone2 = CompanyProfile.Phone2;
            _ReportCompanyProfile.Fax = CompanyProfile.Fax;
            _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (customerInvoice.InvoiceNumber == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_013.rpt"));
                rd.Database.Tables[0].SetDataSource(reportCompanyProfile);
                rd.Database.Tables[1].SetDataSource(reportCustomerInvoices);
                rd.Database.Tables[2].SetDataSource(reportCustomerInvoiceDetails);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (FileFormat.ToLower() == "pdf")
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
        public ActionResult InvoiceRecapPartNumber(string invoicenumber, string FileFormat)
        {

            var customerInvoice = (from a in vssp.Vw_ACC_CustomerInvoice
                                   join b in vssp.Tbl_MST_Customer on a.CustomerId equals b.CustomerId into customer
                                   from b in customer.DefaultIfEmpty()
                                   where a.InvoiceNumber == invoicenumber
                                   select new
                                   {
                                       a.InvoiceNumber,
                                       a.InvoiceDate,
                                       a.CustomerId,
                                       a.CustomerName,
                                       b.Address,
                                       b.City,
                                       b.Provience,
                                       b.Country,
                                       b.PostalCode,
                                       a.Terms,
                                       a.Remarks,
                                       a.SubTotal,
                                       a.PPN,
                                       a.PPH23,
                                       a.GrandTotal,
                                       a.Status,
                                       a.StatusName,
                                       a.ApprovalName
                                   }).FirstOrDefault();

            var customerInvoiceDetails = (from a in vssp.Vw_ACC_CustomerInvoiceDetail
                                          where a.InvoiceNumber == invoicenumber
                                          orderby a.PartNumber
                                          select a).ToList();

            var customerInvoiceApprovals = (from a in vssp.Tbl_ACC_CustomerInvoiceApproval
                                           where a.InvoiceNumber == invoicenumber
                                           select new { a.InvoiceNumber, a.UserId, a.UserName, a.ApprovalLevel,a.ApprovalName,a.ApprovalEmail,a.SentEmail,a.SentEmailDate,a.Approved,a.ApprovedDate }).ToList();

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportCustomerInvoiceModel> reportCustomerInvoices = new List<ReportCustomerInvoiceModel>();
            List<ReportCustomerInvoiceDetailModel> reportCustomerInvoiceDetails = new List<ReportCustomerInvoiceDetailModel>();
            List<ReportCustomerInvoiceApprovalModel> reportCustomerInvoiceApprovals = new List<ReportCustomerInvoiceApprovalModel>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            ReportCustomerInvoiceModel _ReportCustomerInvoice = new ReportCustomerInvoiceModel();
            _ReportCustomerInvoice.InvoiceNumber = customerInvoice.InvoiceNumber;
            _ReportCustomerInvoice.InvoiceDate = Convert.ToDateTime(systemService.Vd(customerInvoice.InvoiceDate.ToString()));
            _ReportCustomerInvoice.CustomerId = customerInvoice.CustomerId;
            _ReportCustomerInvoice.CustomerName = customerInvoice.CustomerName;
            _ReportCustomerInvoice.CustomerAddress1 = customerInvoice.Address;
            _ReportCustomerInvoice.CustomerAddress2 = customerInvoice.City + ", " + customerInvoice.Provience + ", " + customerInvoice.Country + " " + customerInvoice.PostalCode;
            _ReportCustomerInvoice.SubTotal = systemService.Vn(customerInvoice.SubTotal.ToString());
            _ReportCustomerInvoice.PPN = systemService.Vn(customerInvoice.PPN.ToString());
            _ReportCustomerInvoice.PPH23 = systemService.Vn(customerInvoice.PPH23.ToString());
            _ReportCustomerInvoice.GrandTotal = systemService.Vn(customerInvoice.GrandTotal.ToString());
            _ReportCustomerInvoice.Terms = customerInvoice.Terms;
            _ReportCustomerInvoice.Remarks = customerInvoice.Remarks;
            _ReportCustomerInvoice.Status = int.Parse(customerInvoice.Status.ToString());
            _ReportCustomerInvoice.StatusName = customerInvoice.StatusName;
            _ReportCustomerInvoice.ApprovalName = "";

            reportCustomerInvoices.Add(_ReportCustomerInvoice);

            foreach (var detail in customerInvoiceDetails)
            {
                ReportCustomerInvoiceDetailModel _ReportCustomerInvoiceDetail = new ReportCustomerInvoiceDetailModel();
                _ReportCustomerInvoiceDetail.InvoiceNumber = detail.InvoiceNumber;
                _ReportCustomerInvoiceDetail.CustomerId = detail.CustomerId;
                _ReportCustomerInvoiceDetail.UniqueNumber = detail.UniqueNumber;
                _ReportCustomerInvoiceDetail.PartNumber = detail.PartNumber;
                _ReportCustomerInvoiceDetail.PartName = detail.PartName;
                _ReportCustomerInvoiceDetail.UnitLevel2 = detail.UnitLevel2;
                _ReportCustomerInvoiceDetail.PriceUnit = systemService.Vn(detail.PriceUnit.ToString());
                _ReportCustomerInvoiceDetail.InvoiceQty = systemService.Vn(detail.InvoiceQty.ToString());
                _ReportCustomerInvoiceDetail.Amount = systemService.Vn(detail.Amount.ToString());

                reportCustomerInvoiceDetails.Add(_ReportCustomerInvoiceDetail);
            }

            /* Approval */
            int checkuser = 1;
            ReportCustomerInvoiceApprovalModel _reportCustomerInvoiceApproval = new ReportCustomerInvoiceApprovalModel();
            foreach (var orderapprovals in customerInvoiceApprovals)
            {
                _reportCustomerInvoiceApproval.InvoiceNumber = orderapprovals.InvoiceNumber;
                if (orderapprovals.ApprovedDate != null)
                {
                    _reportCustomerInvoiceApproval.ApprovedDate = systemService.Vd(orderapprovals.ApprovedDate.ToString(), "dd-MMMM-yyyy");
                }
                switch (orderapprovals.ApprovalLevel.ToString())
                {
                    case "1":
                        _reportCustomerInvoiceApproval.UserName1 = orderapprovals.UserName.ToUpper();
                        UserEditModel user1 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user1 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportCustomerInvoiceApproval.Sign1 = user1.Sign;
                            }
                        }
                        break;
                    case "2":
                        _reportCustomerInvoiceApproval.UserName2 = orderapprovals.UserName.ToUpper();
                        UserEditModel user2 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user2 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportCustomerInvoiceApproval.Sign2 = user2.Sign;
                            }
                        }

                        break;
                    case "3":
                        _reportCustomerInvoiceApproval.UserName3 = orderapprovals.UserName.ToUpper();
                        UserEditModel user3 = accountService.UserEditList(orderapprovals.UserId).FirstOrDefault();
                        if (user3 != null)
                        {
                            if (orderapprovals.Approved == true)
                            {
                                _reportCustomerInvoiceApproval.Sign3= user3.Sign;
                            }
                        }
                        break;

                }

            }
            reportCustomerInvoiceApprovals.Add(_reportCustomerInvoiceApproval);

            ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
            _ReportCompanyProfile.CompId = CompanyProfile.ID;
            _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
            _ReportCompanyProfile.CompAddress = CompanyProfile.Address;
            _ReportCompanyProfile.CompCity = CompanyProfile.City;
            _ReportCompanyProfile.CompCountry = CompanyProfile.Provience + ", " + CompanyProfile.Country + ", " + CompanyProfile.ZipCode;
            _ReportCompanyProfile.Phone1 = CompanyProfile.Phone1;
            _ReportCompanyProfile.Phone2 = CompanyProfile.Phone2;
            _ReportCompanyProfile.Fax = CompanyProfile.Fax;
            _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

            reportCompanyProfile.Add(_ReportCompanyProfile);

            ReportDocument rd = new ReportDocument();

            if (customerInvoice.InvoiceNumber == null)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_014.rpt"));
                rd.Database.Tables[0].SetDataSource(reportCompanyProfile);
                rd.Database.Tables[1].SetDataSource(reportCustomerInvoices);
                rd.Database.Tables[2].SetDataSource(reportCustomerInvoiceDetails);
                rd.Database.Tables[3].SetDataSource(reportCustomerInvoiceApprovals);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (FileFormat.ToLower() == "pdf")
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
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/vnd.ms-excel");
            }

        }
        public ActionResult KanbanProduction(string CustomerId, string PartNumber, string KanbanKey, string formAction)
        {
            KanbanKey = systemService.Vf(KanbanKey);

            QRCode.generateKanbanProductionQrcode(CustomerId, PartNumber, KanbanKey);

            var KanbanCard = (from a in vssp.Vw_MST_KanbanProductionList
                              join b in vssp.Tbl_TRS_QrCodePath on a.KanbanKey equals b.DocId into qrcode
                              from b in qrcode.DefaultIfEmpty()
                              where a.Actived == true && a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.KanbanKey.Contains(KanbanKey)
                              orderby a.KanbanRun
                              select new { a.KanbanKey, a.KanbanCode, a.LineId, a.LineName, a.CustomerId, a.CustomerName, a.UniqueNumber, a.PartNumber, a.PartName, a.CustomerUnitModel, a.UnitQty, a.UnitLevel2, a.PackingId, a.LocationId, a.KanbanRun, a.Actived, b.QrcodePath, b.BarcodePath }).ToList();

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportKanbanProductionList> reportKanbanProductionList = new List<ReportKanbanProductionList>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            foreach (var kanban in KanbanCard)
            {

                ReportKanbanProductionList kanbancard = new ReportKanbanProductionList();
                kanbancard.KanbanKey = kanban.KanbanKey;
                kanbancard.KanbanCode = kanban.KanbanCode;
                kanbancard.LineId = kanban.LineId;
                kanbancard.LineName = kanban.LineName;
                kanbancard.CustomerId = kanban.CustomerId;
                kanbancard.CustomerName = kanban.CustomerName;
                kanbancard.UniqueNumber = kanban.UniqueNumber;
                kanbancard.PartNumber = kanban.PartNumber;
                kanbancard.PartName = kanban.PartName;
                kanbancard.Model = kanban.CustomerUnitModel;
                kanbancard.UnitQty = systemService.Vn(kanban.UnitQty.ToString());
                kanbancard.UnitLevel2 = kanban.UnitLevel2;
                kanbancard.PackingId = kanban.PackingId;
                kanbancard.LocationId = kanban.LocationId;
                kanbancard.KanbanRun = systemService.Vn(kanban.KanbanRun.ToString());
                kanbancard.Actived = systemService.Vb(kanban.Actived.ToString());
                if (systemService.Vf(kanban.QrcodePath) != "")
                {
                    kanbancard.QrCode = Server.MapPath(systemService.Vf(kanban.QrcodePath));
                }
                else
                {
                    kanbancard.QrCode = "";
                }
                if (systemService.Vf(kanban.BarcodePath) != "")
                {
                    kanbancard.BarCode = Server.MapPath(systemService.Vf(kanban.BarcodePath));
                }
                else
                {
                    kanbancard.BarCode = "";
                }
                reportKanbanProductionList.Add(kanbancard);
            }
            
            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }
            ReportDocument rd = new ReportDocument();

            if (KanbanCard.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                switch (formAction.ToLower())
                {
                    case "kanbancard":
                        rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_016.rpt"));
                        break;
                    case "kanbanqrcode":
                        rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_017.rpt"));
                        break;
                }
                rd.Database.Tables[0].SetDataSource(reportCompanyProfile);
                rd.Database.Tables[1].SetDataSource(reportKanbanProductionList);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            QRCode.cleanKanbanProductionQrcode(CustomerId, PartNumber, KanbanKey);

            return File(stream, "application/pdf");

        }
        public ActionResult LabelProduction(string ProductionNumber, string toprinter)
        {

            QRCode.generateLabelProductionQrcode(ProductionNumber);

            var KanbanCard = (from a in vssp.Vw_TRS_ProductionLineProcessList
                              join b in vssp.Tbl_TRS_QrCodePath on a.ProductionNumber equals b.DocId into qrcode
                              from b in qrcode.DefaultIfEmpty()
                              join c in vssp.Tbl_MST_PartIdentification on new { a.CustomerId,a.PartNumber} equals new {c.CustomerId, c.PartNumber} into partidentification
                              from c in partidentification.DefaultIfEmpty()
                              where a.ProductionNumber == ProductionNumber
                              orderby a.KanbanRun
                              select new { a.ProductionNumber, a.ProductionDate, a.KanbanKey, a.LineId, a.LineName, a.GateId, a.GateName, a.CustomerId, a.CustomerName, a.UniqueNumber, a.PartNumber, a.PartName, a.CustomerUnitModel, c.PartIdentity, a.Qty_OK, a.UnitLevel1, a.PackingId, a.KanbanRun, b.QrcodePath, a.EditDate, a.UserId }).ToList();

            var CompanyProfile = systemService.GetLicenseInfo();

            List<ReportLabelProductionList> reportKanbanProductionList = new List<ReportLabelProductionList>();
            List<ReportCompanyProfile> reportCompanyProfile = new List<ReportCompanyProfile>();

            foreach (var kanban in KanbanCard)
            {

                ReportLabelProductionList kanbancard = new ReportLabelProductionList();
                kanbancard.KanbanKey = kanban.KanbanKey;
                kanbancard.ProductionNumber = kanban.ProductionNumber;
                kanbancard.ProductionDate = Convert.ToDateTime(systemService.Vd(kanban.ProductionDate.ToString(),"dd-MMM-yyyy HH:mm"));
                kanbancard.LineId = kanban.LineId;
                kanbancard.LineName = kanban.LineName;
                kanbancard.GateId = kanban.GateId;
                kanbancard.GateName = kanban.GateName;
                kanbancard.CustomerId = kanban.CustomerId;
                kanbancard.CustomerName = kanban.CustomerName;
                kanbancard.UniqueNumber = kanban.UniqueNumber;
                kanbancard.PartNumber = kanban.PartNumber;
                kanbancard.PartName = kanban.PartName;
                kanbancard.Model = kanban.CustomerUnitModel;
                kanbancard.PartIdentity = kanban.PartIdentity;
                kanbancard.UnitQty = systemService.Vn(kanban.Qty_OK.ToString());
                kanbancard.UnitLevel2 = kanban.UnitLevel1;
                kanbancard.PackingId = kanban.PackingId;
                kanbancard.KanbanRun = systemService.Vn(kanban.KanbanRun.ToString());
                kanbancard.InspectionDate = Convert.ToDateTime(systemService.Vd(kanban.EditDate.ToString(), "dd-MMM-yyyy HH:mm"));
                UserEditModel user = accountService.UserEditList(kanban.UserId).FirstOrDefault();
                if (user != null)
                {
                    kanbancard.InspectorName = user.UserName;
                }
                else
                {
                    kanbancard.InspectorName = kanban.UserId;
                }

                if (systemService.Vf(kanban.QrcodePath) != "")
                {
                    kanbancard.QrCode = Server.MapPath(systemService.Vf(kanban.QrcodePath));
                }
                else
                {
                    kanbancard.QrCode = "";
                }
                reportKanbanProductionList.Add(kanbancard);
            }

            if (CompanyProfile != null)
            {
                ReportCompanyProfile _ReportCompanyProfile = new ReportCompanyProfile();
                _ReportCompanyProfile.CompId = CompanyProfile.ID;
                _ReportCompanyProfile.CompName = CompanyProfile.Name.ToUpper();
                _ReportCompanyProfile.CompLogo = CompanyProfile.LogoSmall;

                reportCompanyProfile.Add(_ReportCompanyProfile);
            }
            ReportDocument rd = new ReportDocument();

            if (KanbanCard.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_020.rpt"));
                rd.Database.Tables[0].SetDataSource(reportCompanyProfile);
                rd.Database.Tables[1].SetDataSource(reportKanbanProductionList);
            }

            if (toprinter == "false")
            {
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                stream.Seek(0, SeekOrigin.Begin);

                QRCode.cleanOrderQr(ProductionNumber);

                return File(stream, "application/pdf");
            }
            else
            {
                //rd.PrintOptions.PrinterName = "\\ ";
                //rd.PrintToPrinter(1, true, 0, 0);
                //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, "crReport");
                
                PdfPathModel pdfPathModel = new PdfPathModel();
                Guid imageGuid = Guid.NewGuid();
                string pdfPath = System.Web.Hosting.HostingEnvironment.MapPath("~/_VSSPAssets/Document/LabelProduction/");
                string reportName = "labelproduction";
                string pdfName = String.Format(@"{0}{1}{2}.pdf", pdfPath, reportName, imageGuid);
                string hostingUrl = String.Format(@"{0}{1}{2}.pdf", Request.Url.GetLeftPart(UriPartial.Authority) + "/_VSSPAssets/Document/LabelProduction/", reportName, imageGuid);
                pdfPathModel.pdfPath = pdfName;
                pdfPathModel.pdfUrl = hostingUrl;
                if (!Directory.Exists(pdfPath))
                {
                    Directory.CreateDirectory(pdfPath);
                }
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, pdfName);
                rd.Close();
                rd.Dispose();
                GC.Collect();

                QRCode.cleanOrderQr(ProductionNumber);

                return Json(pdfPathModel, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult cleanLabelProduction(string filename)
        {
            if(System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }
            return new HttpStatusCodeResult(204);
        }
        public ActionResult RequestOrderParts(string ordernumber)
        {

            List<Vw_TRS_RequestOrderParts> RequestOrderParts = (from a in vssp.Vw_TRS_RequestOrderParts
                                                  where a.OrderNumber == ordernumber
                                                  select a).ToList();

            var RequestOrderPartsDetail = from a in vssp.Tbl_TRS_RequestOrderPartsDetail
                                   join b in vssp.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber }
                                   where a.OrderNumber == ordernumber
                                   select new { a.OrderNumber, a.LineId, a.SupplierId, b.UniqueNumber, a.PartNumber, b.PartName, Model = b.PartModel, b.UnitQty, Unit = b.UnitLevel2, a.OrderQty, a.OrderUnitQty, a.ReceiveQty };

            List<ReportRequestOrderPartsModel> reportRequestOrderParts = new List<ReportRequestOrderPartsModel>();
            List<ReportRequestOrderPartsDetailsModel> reportRequestOrderPartsDetails = new List<ReportRequestOrderPartsDetailsModel>();

            foreach (var orders in RequestOrderParts)
            {
                ReportRequestOrderPartsModel _ReportRequestOrderParts = new ReportRequestOrderPartsModel();
                _ReportRequestOrderParts.OrderNumber = orders.OrderNumber;
                _ReportRequestOrderParts.OrderDate = Convert.ToDateTime(systemService.Vd(orders.OrderDate.ToString()));
                _ReportRequestOrderParts.DeliveryDate = Convert.ToDateTime(systemService.Vd(orders.DeliveryDate.ToString()));
                _ReportRequestOrderParts.LineId = orders.LineId;
                _ReportRequestOrderParts.LineName = orders.LineName;
                _ReportRequestOrderParts.ShiftId = orders.ShiftId;
                _ReportRequestOrderParts.Remarks = orders.Remarks;
                _ReportRequestOrderParts.StatusName = orders.StatusName;
                _ReportRequestOrderParts.UserId = orders.UserId;
                _ReportRequestOrderParts.EditDate = Convert.ToDateTime(systemService.Vd(orders.EditDate.ToString()));

                reportRequestOrderParts.Add(_ReportRequestOrderParts);
            }
            foreach (var orderdetails in RequestOrderPartsDetail)
            {
                ReportRequestOrderPartsDetailsModel _ReportRequestOrderPartsdetail = new ReportRequestOrderPartsDetailsModel();
                _ReportRequestOrderPartsdetail.OrderNumber = orderdetails.OrderNumber;
                _ReportRequestOrderPartsdetail.LineId = orderdetails.LineId;
                _ReportRequestOrderPartsdetail.SupplierId = orderdetails.SupplierId;
                _ReportRequestOrderPartsdetail.UniqueNumber = orderdetails.UniqueNumber;
                _ReportRequestOrderPartsdetail.PartNumber = orderdetails.PartNumber;
                _ReportRequestOrderPartsdetail.PartName = orderdetails.PartName;
                _ReportRequestOrderPartsdetail.PartModel = orderdetails.Model;
                _ReportRequestOrderPartsdetail.UnitQty = systemService.Vn(orderdetails.UnitQty.ToString());
                _ReportRequestOrderPartsdetail.Unit = orderdetails.Unit;
                _ReportRequestOrderPartsdetail.OrderQty = systemService.Vn(orderdetails.OrderQty.ToString());
                _ReportRequestOrderPartsdetail.OrderUnitQty = systemService.Vn(orderdetails.OrderUnitQty.ToString());
                _ReportRequestOrderPartsdetail.ReceiveQty = systemService.Vn(orderdetails.ReceiveQty.ToString());

                reportRequestOrderPartsDetails.Add(_ReportRequestOrderPartsdetail);
            }
            ReportDocument rd = new ReportDocument();

            if (RequestOrderParts.Count == 0)
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports/addon"), "_blank.rpt"));
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Views/Reports"), "RPT_TRS_018.rpt"));
                rd.Database.Tables[0].SetDataSource(reportRequestOrderParts);
                rd.Database.Tables[1].SetDataSource(reportRequestOrderPartsDetails);
            }

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            GC.Collect();

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/pdf");

        }
    }
    
}