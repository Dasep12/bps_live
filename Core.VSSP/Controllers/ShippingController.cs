using System;
using System.Collections.Generic;
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
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Core.VSSP.Controllers
{
    public class ShippingController : Controller
    {
        // GET: Order
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        DeliveryService _DeliveryService = new DeliveryService();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult DeliveryImport()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Shipping", "DeliveryImport");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    DeliveryOrderListModel DeliveryOrder = new DeliveryOrderListModel();
                    DeliveryOrder.ExportList = _SystemService.ComboExport().ToList();
                    DeliveryOrder.StatusList = (from a in vssp_db.Tbl_TRS_Status
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

                        return View(DeliveryOrder);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult DeliveryOrder()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string guid = Guid.NewGuid().ToString().Replace("-", "");
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Shipping", "DeliveryOrder");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;
                    ViewBag.TransId = guid;
                    DeliveryOrderListModel DeliveryOrder = new DeliveryOrderListModel();
                    DeliveryOrder.ExportList = _SystemService.ComboExport().ToList();
                    DeliveryOrder.StatusList = (from a in vssp_db.Tbl_TRS_Status
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

                        return View(DeliveryOrder);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult DeliveryOrderListJson(string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99,
                                    bool imported = false,
                                    bool kanbanorder = false)

        {
            searchFilter = _SystemService.Vf(searchFilter);
            List<Vw_TRS_DeliveryOrder> Order = new List<Vw_TRS_DeliveryOrder>();

            if (_SystemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string ordermonth = arrs[0];
                string orderyears = arrs[1];
                Order = (from a in vssp_db.Vw_TRS_DeliveryOrder
                         where a.DOYear == orderyears && a.DOMonth == ordermonth && a.KanbanOrder == kanbanorder && (a.DONumber.Contains(searchFilter) || a.PONumber.Contains(searchFilter) || a.RefNumber.Contains(searchFilter) || a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter))
                         orderby a.DODate descending, a.DONumber descending
                         select a).ToList();
                //Order = Order.Where(a => Convert.ToDateTime(a.DODate).ToString("MM") == ordermonth && Convert.ToDateTime(a.DODate).ToString("yyyy") == orderyears).ToList();
            }
            else
            {
                Order = (from a in vssp_db.Vw_TRS_DeliveryOrder
                         where a.KanbanOrder == kanbanorder && (a.DONumber.Contains(searchFilter) || a.PONumber.Contains(searchFilter) || a.RefNumber.Contains(searchFilter) || a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter))
                         orderby a.DODate descending, a.DONumber descending
                         select a).ToList();

            }

            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                Order = Order.Where(a => a.DODate >= startdate && a.DODate <= enddate).ToList();
            }
            if (status != 99)
            {
                Order = Order.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in Order
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                Order = Order.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }
            if (imported == true)
            {
                Order = Order.Where(a => a.Status.ToString() == "6").ToList();
            }
            else
            {
                Order = Order.Where(a => a.Status.ToString() != "6").ToList();
            }

            //return Json(Order, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(Order, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult DeliveryOrderDetailListJson(string DONumber, bool import = false)
        {
            DONumber = _SystemService.Vf(DONumber);
            var orderdetails = (from a in vssp_db.Vw_TRS_DeliveryOrderDetail
                                where a.DONumber == DONumber
                                orderby a.PartNumber
                                select new
                                {
                                    a.DONumber,
                                    a.CustomerId,
                                    a.CustomerName,
                                    a.SONumber,
                                    a.UniqueNumber,
                                    a.PartNumber,
                                    a.PartNumberCustomer,
                                    a.PartName,
                                    a.PassThrough,
                                    a.CustomerUnitModel,
                                    a.UnitQty,
                                    a.UnitLevel1,
                                    a.UnitLevel2,
                                    a.StockKanban,
                                    a.StockQty,
                                    a.DeliveryQty,
                                    a.DeliveryUnitQty,
                                    a.OutstandingQty,
                                    a.OutstandingUnitQty,
                                    a.Status
                                }).ToList();

            if (import == true)
            {
                orderdetails = orderdetails.Where(a => a.Status == 6).ToList();
            }
            else
            {
                orderdetails = orderdetails.Where(a => a.Status != 6).ToList();
            }

            //return Json(orderdetails, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(orderdetails, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult GetOutstandingStock(string salesnumber, string customerid, string partnumber)
        {
            if (_SystemService.Vf(salesnumber) == "")
            {
                salesnumber = "*";
            }

            DeliveryOrderOutstandingStockModel oustandingStockModel = new DeliveryOrderOutstandingStockModel();

            var outstanding = vssp_db.Vw_TRS_SalesOrderOutstanding.Where(a => a.SONumber == salesnumber && a.CustomerId == customerid && a.PartNumber == partnumber).FirstOrDefault();
            var stock = vssp_db.Vw_TRS_StockFG.Where(a => a.CustomerId == customerid && a.PartNumber == partnumber).FirstOrDefault();

            oustandingStockModel.CustomerId = customerid;
            oustandingStockModel.PartNumber = partnumber;
            oustandingStockModel.StockKanban = stock == null ? 0 : stock.StockKanban;
            oustandingStockModel.StockQty = stock == null ? 0 : stock.StockQty;
            oustandingStockModel.OutstandingKanban = outstanding == null ? 0 : outstanding.OutstandingKanban;
            oustandingStockModel.OutstandingQty = outstanding == null ? 0 : outstanding.OutstandingQty;

            //return Json(oustandingStockModel, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(oustandingStockModel, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        //  ========= CRUD DELIVERY ORDER ================
        public ActionResult crudDeliveryOrderList(string jsonData, bool kanbanorder = false)
        {

            try
            {

                vssp_db.Database.CommandTimeout = 0;

                PostDeliveryOrderModel postDeliveryOrder = JsonConvert.DeserializeObject<PostDeliveryOrderModel>(jsonData);
                Tbl_TRS_DeliveryOrder DeliveryOrder = postDeliveryOrder.DeliveryOrder;
                List<crud_DeliveryOrderDetail> DeliveryOrderDetail = postDeliveryOrder.DeliveryOrderDetail;

                string uid = postDeliveryOrder.uid;
                string transid = postDeliveryOrder.transid;
                string returnCodes = "";
                string formAction = postDeliveryOrder.formAction.ToLower();

                switch (formAction)
                {
                    case "create":

                        /* Get New Order Number */
                        string CompId = Session["CompID"].ToString();
                        var OrderNumber = vssp_db.SP_GET_DeliveryOrderNumber(DeliveryOrder.CustomerId, DeliveryOrder.DODate, CompId);
                        foreach (SP_GET_DeliveryOrderNumber_Result number in OrderNumber)
                        {
                            DeliveryOrder.DONumber = number.OrderNumber;
                        }

                        //Tbl_TRS_DeliveryOrder ListDeliveryOrder = new Tbl_TRS_DeliveryOrder();
                        //ListDeliveryOrder.DONumber = DeliveryOrder.DONumber;
                        //ListDeliveryOrder.DODate = DeliveryOrder.DODate;
                        //ListDeliveryOrder.CustomerId = DeliveryOrder.CustomerId;
                        //ListDeliveryOrder.SONumber = DeliveryOrder.SONumber;
                        //ListDeliveryOrder.RefNumber = DeliveryOrder.RefNumber;
                        //ListDeliveryOrder.Remarks = DeliveryOrder.Remarks;
                        //ListDeliveryOrder.KanbanOrder = DeliveryOrder.KanbanOrder;
                        //ListDeliveryOrder.KanbanUsage = DeliveryOrder.KanbanUsage;
                        //ListDeliveryOrder.Status = 0;
                        //ListDeliveryOrder.UserId = uid;
                        //ListDeliveryOrder.EditDate = DateTime.Now;

                        //vssp_db.Tbl_TRS_DeliveryOrder.Add(ListDeliveryOrder);

                        /* crud Details */
                        //crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.CustomerId, formAction);
                        returnCodes = crudDeliveryOrderDetailNew(DeliveryOrderDetail, DeliveryOrder, uid, formAction);

                        if (kanbanorder == true)
                        {
                            /* crud Kanban */
                            crudKanbanOrder(DeliveryOrder.DONumber, transid, uid, DeliveryOrder.KanbanUsage, formAction);
                        }

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                        ListUpdate.DODate = DeliveryOrder.DODate;
                        ListUpdate.SONumber = DeliveryOrder.SONumber;
                        ListUpdate.KanbanUsage = DeliveryOrder.KanbanUsage;
                        ListUpdate.RefNumber = DeliveryOrder.RefNumber;
                        ListUpdate.Remarks = DeliveryOrder.Remarks;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Details */
                        //crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.CustomerId, formAction);
                        returnCodes = crudDeliveryOrderDetailNew(DeliveryOrderDetail, DeliveryOrder, uid, formAction);
                        if (kanbanorder == true)
                        {
                            /* crud Kanban */
                            crudKanbanOrder(DeliveryOrder.DONumber, transid, uid, DeliveryOrder.KanbanUsage, formAction);
                        }

                        break;
                    case "closed":

                        var ListClosed = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                        ListCanceled.Status = 4;

                        /* crud detail */
                        //crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.DONumber, "delete");
                        returnCodes = crudDeliveryOrderDetailNew(DeliveryOrderDetail, DeliveryOrder, uid, "delete");
                        break;

                    case "delete":

                        /* remove existing DeliveryOrder */
                        var ListDelete = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        /* crud detail */
                        //crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.CustomerId, formAction);
                        returnCodes = crudDeliveryOrderDetailNew(DeliveryOrderDetail, DeliveryOrder, uid, formAction);
                        //vssp_db.Tbl_TRS_DeliveryOrder.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                        break;
                }

                try
                {
                    //vssp_db.SaveChanges();
                    //updateStatusSalesOrder(DeliveryOrder.SONumber, formAction);
                    //if (formAction != "closed")
                    //{
                    //    vssp_db.SP_CRUD_StockTransactionDeliveryOrder(DeliveryOrder.DONumber, uid, formAction);
                    //}

                    //return Json(DeliveryOrder, JsonRequestBehavior.AllowGet);
                    var resp = new Dictionary<string, string>
                        {
                            { "err" , returnCodes } ,
                            { "data",DeliveryOrder.DONumber }
                        };

                    if (returnCodes == "00")
                    {
                        vssp_db.SaveChanges();
                        updateStatusSalesOrder(DeliveryOrder.SONumber, formAction);
                        if (formAction != "closed")
                        {
                            vssp_db.SP_CRUD_StockTransactionDeliveryOrder(DeliveryOrder.DONumber, uid, formAction);
                        }
                        return Json(resp, JsonRequestBehavior.AllowGet);
                    }

                    return Json(resp, JsonRequestBehavior.AllowGet);
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

        public void crudDeliveryOrderDetail(List<crud_DeliveryOrderDetail> DeliveryOrderDetails, string DONumber, string CustomerId, string formAction)
        {

            /* check details if deleted */
            var exceptionList = new List<string>();
            foreach (var part in DeliveryOrderDetails)
            {
                exceptionList.Add(part.PartNumber);
            }

            var deletedItems = (from a in vssp_db.Tbl_TRS_DeliveryOrderDetail
                                where a.DONumber == DONumber
                                select a).ToList();

            deletedItems = deletedItems.Where(x => !exceptionList.Contains(x.PartNumber)).ToList();
            vssp_db.Tbl_TRS_DeliveryOrderDetail.RemoveRange(deletedItems);
            /* end deleted */

            foreach (var Details in DeliveryOrderDetails)
            {

                if (Details.RowStatus == null)
                {
                    Details.RowStatus = formAction;
                    var doexist = vssp_db.Tbl_TRS_DeliveryOrderDetail.Where(a => a.DONumber == DONumber && a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber).FirstOrDefault();
                    if(formAction.ToLower() == "update")
                    {
                        if (doexist == null) Details.RowStatus = "Create";
                    }
                }
                if (Details.RowStatus != null)
                {
                    switch (Details.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Details */
                            Tbl_TRS_DeliveryOrderDetail ListDetail = new Tbl_TRS_DeliveryOrderDetail();
                            ListDetail.DONumber = DONumber;
                            ListDetail.CustomerId = CustomerId;
                            ListDetail.PartNumber = Details.PartNumber;
                            ListDetail.DeliveryQty = Details.DeliveryQty;
                            ListDetail.DeliveryUnitQty = Details.DeliveryUnitQty;

                            vssp_db.Tbl_TRS_DeliveryOrderDetail.Add(ListDetail);

                            //var partadd = vssp_db.Tbl_MST_PartFinishGoods.Where(a => a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber).FirstOrDefault();
                            //if (partadd.PassThrough == true)
                            //{
                            //    crudStockRAW(CustomerId, Details.PartNumber, Details.DeliveryQty, Details.DeliveryUnitQty, "Add", false);
                            //} else
                            //{
                            //    crudStockFG(CustomerId, Details.PartNumber, Details.DeliveryQty, Details.DeliveryUnitQty, "Add", false);
                            //}

                            break;

                        case "update":

                            //var partupdate = vssp_db.Tbl_MST_PartFinishGoods.Where(a => a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber).FirstOrDefault();
                            var ListUpdate = vssp_db.Tbl_TRS_DeliveryOrderDetail.Where(a => a.DONumber == DONumber && a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber).FirstOrDefault();
                            //if (partupdate.PassThrough == true)
                            //{
                            //    if (ListUpdate != null) crudStockRAW(CustomerId, Details.PartNumber, ListUpdate.DeliveryQty, ListUpdate.DeliveryUnitQty, "Delete", false);
                            //}
                            //else
                            //{
                            //    if (ListUpdate != null) crudStockFG(CustomerId, Details.PartNumber, ListUpdate.DeliveryQty, ListUpdate.DeliveryUnitQty, "Delete", false);
                            //}

                            if (ListUpdate != null)
                            {
                                ListUpdate.DeliveryQty = Details.DeliveryQty;
                                ListUpdate.DeliveryUnitQty = Details.DeliveryUnitQty;
                            }

                            //if (partupdate.PassThrough == true)
                            //{
                            //    crudStockRAW(CustomerId, Details.PartNumber, Details.DeliveryQty, Details.DeliveryUnitQty, "Add", false);
                            //}
                            //else
                            //{
                            //    crudStockFG(CustomerId, Details.PartNumber, Details.DeliveryQty, Details.DeliveryUnitQty, "Add", false);
                            //}
                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_TRS_DeliveryOrderDetail.First(a => a.DONumber == DONumber && a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber);
                            var partdelete = vssp_db.Tbl_MST_PartFinishGoods.Where(a => a.CustomerId == CustomerId && a.PartNumber == Details.PartNumber).FirstOrDefault();
                            vssp_db.Tbl_TRS_DeliveryOrderDetail.Remove(ListDelete);
                            //if (partdelete.PassThrough == true)
                            //{
                            //    crudStockRAW(CustomerId, Details.PartNumber, Details.DeliveryQty, Details.DeliveryUnitQty, "Delete", false);
                            //}
                            //else
                            //{
                            //    crudStockFG(CustomerId, Details.PartNumber, Details.DeliveryQty, Details.DeliveryUnitQty, "Delete", false);
                            //}
                            break;
                    }
                }
            }

        }

        public string crudDeliveryOrderDetailNew(List<crud_DeliveryOrderDetail> DeliveryOrderDetails, Tbl_TRS_DeliveryOrder ListDeliveryOrder, string uid, string formAction)
        {

            /* check details if deleted */
            var exceptionList = new List<string>();
            foreach (var part in DeliveryOrderDetails)
            {
                exceptionList.Add(part.PartNumber);
            }

            var deletedItems = (from a in vssp_db.Tbl_TRS_DeliveryOrderDetail
                                where a.DONumber == ListDeliveryOrder.DONumber
                                select a).ToList();

            deletedItems = deletedItems.Where(x => !exceptionList.Contains(x.PartNumber)).ToList();
            vssp_db.Tbl_TRS_DeliveryOrderDetail.RemoveRange(deletedItems);
            /* end deleted */
            using (var db = new vssp_entity())
            {

                // Konversi List ke DataTable
                DataTable dt = new DataTable();
                dt.Columns.Add("DONumber", typeof(string));
                dt.Columns.Add("CustomerId", typeof(string));
                dt.Columns.Add("PartNumber", typeof(string));
                dt.Columns.Add("DeliveryQty", typeof(double));
                dt.Columns.Add("DeliveryUnitQty", typeof(double));
                dt.Columns.Add("RowStatus", typeof(string));

                foreach (var detail in DeliveryOrderDetails)
                {
                    if (detail.RowStatus == null)
                    {
                        detail.RowStatus = formAction;
                    }
                    dt.Rows.Add(ListDeliveryOrder.DONumber, ListDeliveryOrder.CustomerId, detail.PartNumber,
                                detail.DeliveryQty, detail.DeliveryUnitQty, detail.RowStatus?.ToLower());
                }

                // Buat parameter untuk DataTable
                var orderDetailsParam = new SqlParameter("@OrderDetails", SqlDbType.Structured)
                {
                    TypeName = "DeliveryOrderDetailType",
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
                db.Database.ExecuteSqlCommand("EXEC SP_CrudDeliveryOrderDetailNew @OrderDetails, @ReturnCode OUTPUT,@ErrorMessage OUTPUT , @DONumber,@DODate,@CustomerId,@SONumber,@RefNumber,@Remarks,@KanbanOrder,@KanbanUsage,@UserId",
                                       orderDetailsParam,
                                       returnCodeParam,
                                       errorMessageParam,
                                       new SqlParameter("@DONumber", ListDeliveryOrder.DONumber),
                                       new SqlParameter("@DODate", ListDeliveryOrder.DODate),
                                       new SqlParameter("@CustomerId", ListDeliveryOrder.CustomerId),
                                       new SqlParameter("@SONumber", ListDeliveryOrder.SONumber),
                                       new SqlParameter("@RefNumber", ListDeliveryOrder.RefNumber),
                                       new SqlParameter("@Remarks", ListDeliveryOrder.Remarks),
                                       new SqlParameter("@KanbanOrder", ListDeliveryOrder.KanbanOrder),
                                       new SqlParameter("@KanbanUsage", (object)ListDeliveryOrder.KanbanUsage ?? DBNull.Value),
                                       new SqlParameter("@UserId", uid)
                                       );

                // Ambil return code dari SP
                if (returnCodeParam.Value.ToString() != "00") return errorMessageParam.Value.ToString();

                return returnCodeParam.Value.ToString();


            }

        }


        public ActionResult updateReffNumber(string DONumber, string RefNumber)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    var deliveryOrder = vssp_db.Tbl_TRS_DeliveryOrder.FirstOrDefault(a => a.DONumber == DONumber);
                    if (deliveryOrder != null)
                    {
                        deliveryOrder.RefNumber = _SystemService.Vf(RefNumber);
                        vssp_db.SaveChanges();
                        return Json(new { ErrMessages = "", Success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { ErrMessages = "Error! Delivery Order Not Found", Success = false }, JsonRequestBehavior.AllowGet);
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
        //  ========= CRUD DELIVERY ORDER END ================

        //public void crudStockFG(string customerid, string partnumber, Nullable<Double> kanbanqty, Nullable<Double> unitqty, string formaction, bool save = false)
        //{
        //    Tbl_TRS_StockFG stock = (from a in vssp_db.Tbl_TRS_StockFG
        //                             where a.CustomerId == customerid && a.PartNumber == partnumber
        //                             select a).FirstOrDefault();

        //    if (stock != null)
        //    {
        //        switch (formaction.ToLower())
        //        {
        //            case "add":
        //                stock.StockKanban -= kanbanqty;
        //                stock.StockQty -= unitqty;
        //                stock.LastUpdate = DateTime.Now;

        //                break;
        //            case "delete":
        //                stock.StockKanban += kanbanqty;
        //                stock.StockQty += unitqty;
        //                stock.LastUpdate = DateTime.Now;


        //                break;
        //        }

        //        if (save == true)
        //        {
        //            vssp_db.SaveChanges();
        //        }

        //    }
        //}
        //public void crudStockRAW(string customerid, string partnumber, Nullable<Double> kanbanqty, Nullable<Double> unitqty, string formaction, bool save = false)
        //{
        //    DateTime date = DateTime.Now;

        //    //Vw_MST_PartFinishGoodsRelation part = (from a in vssp_db.Vw_MST_PartFinishGoodsRelation
        //    //                                       where a.CustomerId == customerid && a.PartNumber == partnumber && a.StartDate <= date && (a.EndDate == null ? date : a.EndDate) >= date
        //    //                                           && ((a.SSP == "" ? true : a.DeliveryRawMaterial) == true || (a.SSP == null ? true : a.DeliveryRawMaterial) == true)
        //    //                                       select a).FirstOrDefault();

        //    var part = (from a in vssp_db.Vw_TRS_StockFG
        //                join b in vssp_db.Vw_MST_PartFinishGoodsRelation on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber } into relation
        //                from b in relation.DefaultIfEmpty()
        //                where a.CustomerId == customerid && a.PartNumber == partnumber && b.StartDate <= date && (b.EndDate == null ? date : b.EndDate) >= date
        //                select new { b.SupplierId, b.PartNumberRawMaterial }).FirstOrDefault();

        //    Tbl_TRS_Stock stock = (from a in vssp_db.Tbl_TRS_Stock
        //                           where a.SupplierId == part.SupplierId && a.PartNumber == part.PartNumberRawMaterial
        //                           select a).FirstOrDefault();

        //    if (stock != null)
        //    {
        //        switch (formaction.ToLower())
        //        {
        //            case "add":
        //                stock.StockKanban -= kanbanqty;
        //                stock.StockQty -= unitqty;
        //                stock.LastUpdate = DateTime.Now;

        //                break;
        //            case "delete":
        //                stock.StockKanban += kanbanqty;
        //                stock.StockQty += unitqty;
        //                stock.LastUpdate = DateTime.Now;


        //                break;
        //        }

        //        if (save == true)
        //        {
        //            vssp_db.SaveChanges();
        //        }

        //    }
        //}
        public ActionResult ImportJson(string formaction, string canConfidential)
        {
            if (formaction == "Order")
            {
                ImportDeliveryListModel List = new ImportDeliveryListModel();
                return Json(List, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "Order-validation")
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    var ListUpload = _DeliveryService.uploadOrderExcel(files[0]);
                    return Json(ListUpload, JsonRequestBehavior.AllowGet);
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
                return Json("Error! No Valid Action", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult crudImportJson(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                PostDeliveryImportModel postDelivery = JsonConvert.DeserializeObject<PostDeliveryImportModel>(jsonData);
                List<ImportDeliveryOrderModel> deliveryOrderImport = postDelivery.deliveryOrder;
                bool replace = _SystemService.Vb(postDelivery.replace.ToString());
                string formaction = postDelivery.formAction;

                try
                {
                    if (formaction == "Order")
                    {
                        string userId = Session["UserID"].ToString();
                        HttpFileCollectionBase files = Request.Files;
                        var ListUpload = _DeliveryService.crudImportOrderExcel(deliveryOrderImport, replace, userId);
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
        [HttpPost]
        public ActionResult deleteImportJson(string CustomerId, string DONumber, DateTime DODate)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    //DONumber = _SystemService.Vf(DONumber, true);

                    var deliveryOrderImport = (from a in vssp_db.Vw_TRS_DeliveryOrderDetail
                                               where a.CustomerId == CustomerId && a.DONumber == DONumber
                                               select a).ToList();

                    foreach (var DO in deliveryOrderImport)
                    {
                        var relationPart = (from a in vssp_db.Vw_TRS_DeliveryOrderRelation
                                            where a.CustomerId == DO.CustomerId && a.DONumber == DO.DONumber && a.PartNumber == DO.PartNumber
                                            select a).ToList();

                        foreach (var raw in relationPart)
                        {

                            var stock = vssp_db.Tbl_TRS_Stock.FirstOrDefault(a => a.SupplierId == raw.SupplierId && a.PartNumber == raw.PartNumber);

                            if (stock != null) {
                                stock.StockQty += raw.DeliveryQty;
                                stock.StockKanban += raw.KanbanQty;
                                stock.LastUpdate = DateTime.Now;
                            }

                        }

                        var ListDelete = vssp_db.Tbl_TRS_DeliveryOrderImport.First(a => a.CustomerId == DO.CustomerId && a.DONumber == DO.DONumber && a.PartNumber == DO.PartNumber);
                        vssp_db.Tbl_TRS_DeliveryOrderImport.Remove(ListDelete);

                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json("<br/>Customer : " + CustomerId + "<br/>DO Number : " + DONumber + "<br/>", JsonRequestBehavior.AllowGet);
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
        public ActionResult KanbanOrder()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string guid = Guid.NewGuid().ToString().Replace("-","");
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Shipping", "KanbanOrder");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess;
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;
                    ViewBag.TransId = guid;

                    KanbanOrderListModel KanbanOrder = new KanbanOrderListModel();
                    KanbanOrder.ExportList = _SystemService.ComboExport().ToList();
                    KanbanOrder.StatusList = (from a in vssp_db.Tbl_TRS_Status
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

                        return View(KanbanOrder);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult KanbanOrderDetailSumJson(string DONumber)
        {
            DONumber = _SystemService.Vf(DONumber);
            var orderdetails = (from a in vssp_db.Vw_TRS_DeliveryOrderKanbanSumTemp
                                where a.DONumber == DONumber
                                orderby a.PartNumber
                                select new
                                {
                                    a.DONumber,
                                    a.CustomerId,
                                    a.CustomerName,
                                    a.SONumber,
                                    a.UniqueNumber,
                                    a.PartNumber,
                                    a.PartNumberCustomer,
                                    a.PartName,
                                    a.PassThrough,
                                    a.CustomerUnitModel,
                                    a.UnitQty,
                                    a.UnitLevel1,
                                    a.UnitLevel2,
                                    a.DeliveryQty,
                                    a.DeliveryUnitQty,
                                }).ToList();
            //return Json(orderdetails, JsonRequestBehavior.AllowGet);

            var jsonResult = Json(orderdetails, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult KanbanOrderDetailJson(string DONumber, string PartNumber)
        {
            try
            {

                var KanbanOrderDetail = from a in vssp_db.Tbl_TRS_DeliveryOrderKanban
                                        join b in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber } into part
                                        from b in part.DefaultIfEmpty()
                                        where a.DONumber == DONumber
                                        select new
                                        {
                                            a.DONumber,
                                            a.CustomerId,
                                            a.PartNumber,
                                            b.PartName,
                                            b.UnitQty,
                                            b.UnitLevel2,
                                            a.RefNumber,
                                            a.DeliveryQty,
                                            a.KanbanData,
                                            a.UserId,
                                            a.ScanTime
                                        };

                if (_SystemService.Vf(PartNumber) != "")
                {
                    KanbanOrderDetail = KanbanOrderDetail.Where(a => a.PartNumber == PartNumber);
                }

                //return Json(KanbanOrderDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(KanbanOrderDetail, JsonRequestBehavior.AllowGet);
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
        public ActionResult KanbanOrderDetailTempJson(string DONumber, string PartNumber)
        {
            try
            {

                if (_SystemService.Vf(DONumber) == "")
                {
                    DONumber = Session["UserId"].ToString();
                }

                var KanbanOrderDetail = from a in vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp
                                        join b in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber } into part
                                        from b in part.DefaultIfEmpty()
                                        where a.DONumber == DONumber
                                        select new
                                        {
                                            a.DONumber,
                                            a.CustomerId,
                                            a.KanbanNumber,
                                            b.UniqueNumber,
                                            a.PartNumber,
                                            b.PartName,
                                            a.DeliveryQty,
                                            b.UnitLevel2,
                                            a.RefNumber,
                                            a.KanbanData,
                                            a.UserId,
                                            a.ScanTime
                                        };

                if (_SystemService.Vf(PartNumber) != "")
                {
                    KanbanOrderDetail = KanbanOrderDetail.Where(a => a.PartNumber == PartNumber);
                }

                //return Json(KanbanOrderDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(KanbanOrderDetail, JsonRequestBehavior.AllowGet);
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
        public ActionResult kanbanProductionReader(string DONumber, string CustomerId, string OrderNumber, Nullable<DateTime> OrderDate, string RefNumber, string KanbanData)
        {
            string[] kanbankey = KanbanData.Split(';');
            string key = kanbankey[0];
            var kanbanData = vssp_db.Vw_MST_KanbanProductionList.Where(a => a.KanbanKey == key).FirstOrDefault();
            KanbanDataModel kanbanDataModel = new KanbanDataModel();
            if (kanbanData != null)
            {
                kanbanDataModel.OrderNumber = OrderNumber;
                kanbanDataModel.OrderDate = OrderDate;
                kanbanDataModel.CustomerId = kanbanData.CustomerId;
                kanbanDataModel.OrderQty = float.Parse(kanbanData.UnitQty.ToString());
                kanbanDataModel.UniqueNumber = kanbanData.UniqueNumber;
                kanbanDataModel.PartNumber = kanbanData.PartNumber;
                kanbanDataModel.KanbanNumber = kanbanData.KanbanKey;
                kanbanDataModel.RefNumber = "";

                if (kanbanData.Actived == true)
                {
                    if (kanbanData.Storage == true)
                    {
                        var customerValidation = vssp_db.Tbl_MST_Customer.Where(a => a.CustomerId == kanbanData.CustomerId || a.CustomerCode == kanbanData.CustomerId).FirstOrDefault();
                        if (customerValidation != null)
                        {
                            var partValidation = vssp_db.Tbl_MST_PartFinishGoods.Where(a => (a.CustomerId == customerValidation.CustomerId && (a.PartNumber == kanbanDataModel.PartNumber || a.PartNumberCustomer == kanbanDataModel.PartNumber))).FirstOrDefault();
                            if (partValidation != null)
                            {
                                var alreadyScanKanban = vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Where(a => a.DONumber == DONumber && a.KanbanData == KanbanData).FirstOrDefault();
                                if (alreadyScanKanban == null)
                                {
                                    var salesorder = vssp_db.Tbl_TRS_SalesOrderDetail.Where(a => a.SONumber == OrderNumber && a.CustomerId == customerValidation.CustomerId && a.PartNumber == kanbanDataModel.PartNumber).FirstOrDefault();
                                    if (salesorder != null)
                                    {
                                        crudDeliveryOrderKanbanTemp(DONumber, CustomerId, kanbanDataModel.PartNumber, kanbanData.KanbanKey, "", float.Parse(kanbanData.UnitQty.ToString()), KanbanData, "Create");
                                        if(kanbanDataModel.OrderQty != partValidation.UnitQty)
                                        {
                                            kanbanDataModel.ErrMessages = "<b>" + kanbanDataModel.PartNumber + " :<br />has been added successfuly with note!<br />Kanban qty [" + kanbanDataModel.OrderQty + "] not same with master part qty [" + partValidation.UnitQty + "]";
                                        }
                                        else
                                        {
                                            kanbanDataModel.ErrMessages = "<b>" + kanbanDataModel.PartNumber + " :<br />has been added successfuly.";
                                        }
                                    }
                                    else
                                    {
                                        kanbanDataModel.ErrMessages = "<b>Error :<br />  No Sales Order for this Part Number of kanban!</b><br />Part Info : " + kanbanDataModel.UniqueNumber + " - " + kanbanDataModel.PartNumber + "<br /> Please check your kanban card.";
                                    }
                                }
                                else
                                {
                                    kanbanDataModel.ErrMessages = "<b>Error :<br />  Kanban already scan on this order !</b><br />Part Info : " + kanbanDataModel.UniqueNumber + " - " + kanbanDataModel.PartNumber + "<br /> Please check your kanban card.";
                                }
                            }
                            else
                            {
                                kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong customer part number!</b><br /> Please check your kanban card.";
                            }
                        }
                        else
                        {
                            kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong customer kanban!</b><br /> Please check your kanban card.";
                        }
                    }
                    else
                    {
                        kanbanDataModel.ErrMessages = "<b>Error :<br />  Invalid kanban!</b><br />Kanban in Production Process. Please check your kanban card.";
                    }
                }
                else
                {
                    kanbanDataModel.ErrMessages = "<b>Error :<br />  Invalid kanban!</b><br />Kanban suspend. Please check your kanban card.";
                }

                return Json(kanbanDataModel, JsonRequestBehavior.AllowGet);

            }
            else
            {
                kanbanDataModel.OrderNumber = OrderNumber;
                kanbanDataModel.OrderDate = OrderDate;
                kanbanDataModel.CustomerId = "";
                kanbanDataModel.OrderQty = 0;
                kanbanDataModel.UniqueNumber = "";
                kanbanDataModel.PartNumber = "";
                kanbanDataModel.KanbanNumber = key;
                kanbanDataModel.RefNumber = "";

                kanbanDataModel.ErrMessages = "<b>Error :<br /> Invalid kanban not found!</b><br /> Please check your kanban card.";
                return Json(kanbanDataModel, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult referenceOrderReader(string CustomerId, string RefNumber)
        {
            var existDelivery = (from a in vssp_db.Tbl_TRS_DeliveryOrder
                                 where a.CustomerId == CustomerId && a.RefNumber.Replace(".", "").Replace("-", "").Trim() == RefNumber.Replace(".", "").Replace("-", "").Trim()
                                 select new { a.DONumber, a.DODate, a.RefNumber }).ToList();
            return Json(existDelivery, JsonRequestBehavior.AllowGet);
        }
        public ActionResult kanbanReader(string DONumber,string CustomerId,string OrderNumber, Nullable<DateTime> OrderDate, string RefNumber, string KanbanData)
        {
            var setting = vssp_db.Tbl_MST_KanbanSetting.Where(a => a.CustomerId == CustomerId).FirstOrDefault();

            if (setting != null)
            {
                string sparator = setting.DataSparator;
                string[] kanbandata = KanbanData.Split(new[] { sparator }, StringSplitOptions.None);

                var jsondata = new Dictionary<string, string> { };
                int seq = 0;
                foreach (var kanban in kanbandata)
                {
                    seq += 1;
                    var kanbanseq = vssp_db.Tbl_MST_KanbanSettingSequence.Where(a => a.Active == true && a.CustomerId == CustomerId && a.SequenceNumber == seq).FirstOrDefault();

                    if (kanbanseq != null)
                    {
                        jsondata.Add(kanbanseq.FieldName, kanban);
                    }
                }
                var jsSerializer = new JavaScriptSerializer();
                var serialized = jsSerializer.Serialize(jsondata);

                KanbanDataModel kanbanDataModel = JsonConvert.DeserializeObject<KanbanDataModel>(serialized);

                if (kanbanDataModel.CustomerId == null) kanbanDataModel.CustomerId = CustomerId;
                if (kanbanDataModel.OrderNumber == null) kanbanDataModel.OrderNumber = OrderNumber;
                if (kanbanDataModel.OrderDate == null) kanbanDataModel.OrderDate = OrderDate;
                if (kanbanDataModel.RefNumber == null) kanbanDataModel.RefNumber = RefNumber;

                var customerValidation = vssp_db.Tbl_MST_Customer.Where(a => a.CustomerId == kanbanDataModel.CustomerId || a.CustomerCode == kanbanDataModel.CustomerId).FirstOrDefault();
                if (customerValidation != null)
                {
                    var partValidation = vssp_db.Tbl_MST_PartFinishGoods.Where(a => (a.CustomerId == customerValidation.CustomerId && (a.PartNumber == kanbanDataModel.PartNumber || a.PartNumberCustomer == kanbanDataModel.PartNumber))).FirstOrDefault();
                    if (partValidation != null)
                    {
                        if (partValidation.PassThrough == true)
                        {
                            var rawRelation = vssp_db.Tbl_MST_PartFinishGoodsRelation.Where(a=> a.CustomerId==partValidation.CustomerId && a.PartNumber==partValidation.PartNumber).FirstOrDefault(); 
                            if(rawRelation == null)
                            {
                                KanbanDataModel kanbanDataModel2 = new KanbanDataModel();
                                kanbanDataModel2.ErrMessages = "<b>Error :<br />  Part Passthrough Relation Empty!</b><br />Part Info : " + kanbanDataModel.UniqueNumber + " - " + kanbanDataModel.PartNumber + "<br /> Please check your Finish Goods Part.";
                                return Json(kanbanDataModel2, JsonRequestBehavior.AllowGet);
                            }
                        }

                        var alreadyScanKanban = vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Where(a => a.DONumber==DONumber && a.KanbanData == KanbanData).FirstOrDefault();
                        if (alreadyScanKanban == null)
                        {
                            if (kanbanDataModel.OrderNumber == OrderNumber)
                            {
                                var salesorder = vssp_db.Tbl_TRS_SalesOrderDetail.Where(a => a.SONumber == kanbanDataModel.OrderNumber && a.CustomerId == customerValidation.CustomerId && a.PartNumber == kanbanDataModel.PartNumber).FirstOrDefault();
                                if (salesorder != null)
                                {
                                    if (kanbanDataModel.RefNumber == RefNumber)
                                    {
                                        crudDeliveryOrderKanbanTemp(DONumber, CustomerId, kanbanDataModel.PartNumber, kanbanDataModel.KanbanNumber, kanbanDataModel.RefNumber, kanbanDataModel.OrderQty, KanbanData, "Create");
                                        kanbanDataModel.ErrMessages = "<b>" + kanbanDataModel.PartNumber + " :<br />has been added successfuly.";
                                    }
                                    else
                                    {
                                        kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong Reference Number of kanban!</b><br />Part Info : " + kanbanDataModel.UniqueNumber + " - " + kanbanDataModel.PartNumber + "<br /> Please check your kanban card.";
                                    }
                                } else
                                {
                                    kanbanDataModel.ErrMessages = "<b>Error :<br />  No Sales Order for this Part Number of kanban!</b><br />Part Info : " + kanbanDataModel.UniqueNumber + " - " + kanbanDataModel.PartNumber + "<br /> Please check your kanban card.";
                                }

                            }
                            else
                            {
                                kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong PO Number of kanban!</b><br />Part Info : " + kanbanDataModel.UniqueNumber + " - " + kanbanDataModel.PartNumber + "<br /> Please check your kanban card.";
                            }
                        } else
                        {
                            kanbanDataModel.ErrMessages = "<b>Error :<br />  Kanban already scan on "+ alreadyScanKanban.DONumber.Replace("/",".") + "!</b><br />Part Info : " + kanbanDataModel.UniqueNumber + " - " + kanbanDataModel.PartNumber + "<br /> Please check your kanban card.";
                        }
                    }
                    else
                    {
                        kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong customer part number!</b><br /> Please check your kanban card.";
                    }
                }
                else
                {
                    kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong customer kanban!</b><br /> Please check your kanban card.";
                }
                return Json(kanbanDataModel, JsonRequestBehavior.AllowGet);

            } else
            {
                KanbanDataModel kanbanDataModel = new KanbanDataModel();
                kanbanDataModel.ErrMessages = "<b>Error :<br />  Customer kanban setting not found!</b><br /> Please check customer kanban setting.";
                return Json(kanbanDataModel, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult clearKanbanOrderJson(string donumber, string formAction, string uid)
        {
            try
            {
                //string uid = Session["UserID"].ToString();

                var deleteDeliveryOrderKanban = vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Where(a => a.DONumber == donumber || a.UserId == uid).ToList();
                foreach (var delete in deleteDeliveryOrderKanban)
                {
                    vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Remove(delete);
                }

                switch (formAction)
                {
                    case "Create":

                        //nothing

                        break;
                    default:

                        var kanbanorderdetail = vssp_db.Tbl_TRS_DeliveryOrderKanban.Where(a => a.DONumber == donumber).ToList();
                        foreach (var kanban in kanbanorderdetail)
                        {
                            Tbl_TRS_DeliveryOrderKanbanTemp _TRS_DeliveryOrderKanbanTemp = new Tbl_TRS_DeliveryOrderKanbanTemp();
                            _TRS_DeliveryOrderKanbanTemp.DONumber = kanban.DONumber;
                            _TRS_DeliveryOrderKanbanTemp.CustomerId = kanban.CustomerId;
                            _TRS_DeliveryOrderKanbanTemp.PartNumber = kanban.PartNumber;
                            _TRS_DeliveryOrderKanbanTemp.KanbanNumber = kanban.KanbanNumber;
                            _TRS_DeliveryOrderKanbanTemp.RefNumber = kanban.RefNumber;
                            _TRS_DeliveryOrderKanbanTemp.DeliveryQty = kanban.DeliveryQty;
                            _TRS_DeliveryOrderKanbanTemp.KanbanData = kanban.KanbanData;
                            _TRS_DeliveryOrderKanbanTemp.UserId = kanban.UserId;
                            _TRS_DeliveryOrderKanbanTemp.ScanTime = kanban.ScanTime;

                            vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Add(_TRS_DeliveryOrderKanbanTemp);
                        }

                        break;
                }

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
        public void crudKanbanOrder(string DONumber, string TransId, string Uid, string KanbanUsage, string formAction)
        {
            /* clear kanban scan */
            string donumberTemp = DONumber;
            if (formAction.ToLower() == "create") donumberTemp = TransId;

            var kanbanscan = vssp_db.Tbl_TRS_DeliveryOrderKanban.Where(a => a.DONumber == DONumber).ToList();
            foreach (var kanban in kanbanscan)
            {
                vssp_db.Tbl_TRS_DeliveryOrderKanban.Remove(kanban);
                if (KanbanUsage.ToLower() == "production")
                {
                    Tbl_MST_KanbanProduction kanbanProd = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.KanbanKey == kanban.KanbanNumber).FirstOrDefault();
                    kanbanProd.Storage = true;
                }
            }

            if (formAction.ToLower() != "delete") { 
                var kanbanscantemp = vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Where(a => a.DONumber == donumberTemp).ToList();
                foreach (var kanban in kanbanscantemp)
                {
                    /* insert kanban scan */
                    Tbl_TRS_DeliveryOrderKanban deliveryOrderKanban = new Tbl_TRS_DeliveryOrderKanban();

                    deliveryOrderKanban.DONumber = DONumber;
                    deliveryOrderKanban.CustomerId = kanban.CustomerId;
                    deliveryOrderKanban.PartNumber = kanban.PartNumber;
                    deliveryOrderKanban.KanbanNumber = kanban.KanbanNumber;
                    deliveryOrderKanban.RefNumber = kanban.RefNumber;
                    deliveryOrderKanban.DeliveryQty = kanban.DeliveryQty;
                    deliveryOrderKanban.KanbanData = kanban.KanbanData;
                    deliveryOrderKanban.UserId = kanban.UserId;
                    deliveryOrderKanban.ScanTime = kanban.ScanTime;

                    vssp_db.Tbl_TRS_DeliveryOrderKanban.Add(deliveryOrderKanban);
                    vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Remove(kanban);

                    if (KanbanUsage.ToLower() == "production")
                    {
                        Tbl_MST_KanbanProduction kanbanProd = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.KanbanKey == kanban.KanbanNumber).FirstOrDefault();
                        kanbanProd.Storage = false;
                    }
                }
            }
        }
        public ActionResult crudDeliveryOrderKanbanTemp(string DONumber, string CustomerId, string PartNumber, string KanbanNumber, string RefNumber, double DeliveryQty, string KanbanData, string formAction)
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                
                switch (formAction.ToLower())
                {
                    case "create":

                        /* create Kanbans */
                        Tbl_TRS_DeliveryOrderKanbanTemp ListKanban = new Tbl_TRS_DeliveryOrderKanbanTemp();
                        ListKanban.DONumber = DONumber;
                        ListKanban.CustomerId = CustomerId;
                        ListKanban.PartNumber = PartNumber;
                        ListKanban.KanbanNumber = KanbanNumber;
                        ListKanban.RefNumber = RefNumber;
                        ListKanban.DeliveryQty = DeliveryQty;
                        ListKanban.KanbanData = KanbanData;
                        ListKanban.UserId = uid;
                        ListKanban.ScanTime = DateTime.Now;

                        vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Add(ListKanban);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.First(a => a.DONumber == DONumber && a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.KanbanNumber == KanbanNumber);

                        ListUpdate.DeliveryQty = DeliveryQty;
                        ListUpdate.KanbanData = KanbanData;
                        ListUpdate.UserId = uid;
                        ListUpdate.ScanTime = DateTime.Now;

                        break;

                    case "delete":

                        var ListDelete = vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.First(a => a.DONumber == DONumber && a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.KanbanNumber == KanbanNumber);
                        vssp_db.Tbl_TRS_DeliveryOrderKanbanTemp.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json("<br/>Part Number : " + PartNumber + "<br/>Kanban Number : " + KanbanNumber + "<br/>", JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
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
        public ActionResult crudDeliveryOrderKanban(string jsonData)
        {

            try
            {
                vssp_db.Database.CommandTimeout = 0;

                PostDeliveryOrderModel postDeliveryOrder = JsonConvert.DeserializeObject<PostDeliveryOrderModel>(jsonData);
                Tbl_TRS_DeliveryOrder DeliveryOrder = postDeliveryOrder.DeliveryOrder;

                string uid = postDeliveryOrder.uid;
                string formAction = postDeliveryOrder.formAction.ToLower();
                string donumber = "";

                if (DeliveryOrder.KanbanUsage.ToLower() == "customer")
                {

                    var kanbansetting = vssp_db.Tbl_MST_KanbanSetting.Where(a => a.CustomerId == DeliveryOrder.CustomerId).FirstOrDefault();

                    if (kanbansetting.DeliveryOrder == true)
                    {
                        //return RedirectToAction("crudDeliveryOrderList", new { jsonData = jsonData, kanbanorder = true });
                        /* crud Delivery Order Kanban List */
                        donumber = crudDeliveryOrderKanbanList(postDeliveryOrder);
                    }
                    else
                    {
                        return Json("Error. Kanban setting not found", JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    //return RedirectToAction("crudDeliveryOrderList", new { jsonData = jsonData, kanbanorder = true });
                    /* crud Delivery Order Kanban List */
                    donumber = crudDeliveryOrderKanbanList(postDeliveryOrder);
                }

                try
                {
                    vssp_db.SaveChanges();
                    updateStatusSalesOrder(DeliveryOrder.SONumber, formAction);
                    if (postDeliveryOrder.formAction != "closed")
                    {
                        vssp_db.SP_CRUD_StockTransactionDeliveryOrder(donumber, uid, formAction);
                    }

                    return Json(DeliveryOrder, JsonRequestBehavior.AllowGet);
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

        public string crudDeliveryOrderKanbanList(PostDeliveryOrderModel postDeliveryOrder)
        {


            Tbl_TRS_DeliveryOrder DeliveryOrder = postDeliveryOrder.DeliveryOrder;
            List<crud_DeliveryOrderDetail> DeliveryOrderDetail = postDeliveryOrder.DeliveryOrderDetail;

            string transid = postDeliveryOrder.transid;
            string uid = postDeliveryOrder.uid;
            string formAction = postDeliveryOrder.formAction.ToLower();
            string donumber = "";

            switch (formAction)
            {
                case "create":

                    /* Get New Order Number */
                    string CompId = Session["CompID"].ToString();
                    var OrderNumber = vssp_db.SP_GET_DeliveryOrderNumber(DeliveryOrder.CustomerId, DeliveryOrder.DODate, CompId);
                    foreach (SP_GET_DeliveryOrderNumber_Result number in OrderNumber)
                    {
                        DeliveryOrder.DONumber = number.OrderNumber;
                    }

                    Tbl_TRS_DeliveryOrder ListDeliveryOrder = new Tbl_TRS_DeliveryOrder();
                    ListDeliveryOrder.DONumber = DeliveryOrder.DONumber;
                    ListDeliveryOrder.DODate = DeliveryOrder.DODate;
                    ListDeliveryOrder.CustomerId = DeliveryOrder.CustomerId;
                    ListDeliveryOrder.SONumber = DeliveryOrder.SONumber;
                    ListDeliveryOrder.RefNumber = DeliveryOrder.RefNumber;
                    ListDeliveryOrder.Remarks = DeliveryOrder.Remarks;
                    ListDeliveryOrder.KanbanOrder = DeliveryOrder.KanbanOrder;
                    ListDeliveryOrder.KanbanUsage = DeliveryOrder.KanbanUsage;
                    ListDeliveryOrder.Status = 0;
                    ListDeliveryOrder.UserId = uid;
                    ListDeliveryOrder.EditDate = DateTime.Now;

                    vssp_db.Tbl_TRS_DeliveryOrder.Add(ListDeliveryOrder);

                    /* crud Details */
                    crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.CustomerId, formAction);

                    /* crud Kanban */
                    crudKanbanOrder(DeliveryOrder.DONumber, transid, uid, DeliveryOrder.KanbanUsage, formAction);

                    break;

                case "update":

                    var ListUpdate = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                    ListUpdate.DODate = DeliveryOrder.DODate;
                    ListUpdate.SONumber = DeliveryOrder.SONumber;
                    ListUpdate.KanbanUsage = DeliveryOrder.KanbanUsage;
                    ListUpdate.RefNumber = DeliveryOrder.RefNumber;
                    ListUpdate.Remarks = DeliveryOrder.Remarks;
                    ListUpdate.UserId = uid;
                    ListUpdate.EditDate = DateTime.Now;

                    /* crud Details */
                    crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.CustomerId, formAction);

                    /* crud Kanban */
                    crudKanbanOrder(DeliveryOrder.DONumber, transid, uid, DeliveryOrder.KanbanUsage, formAction);

                    break;

                case "closed":

                    var ListClosed = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                    ListClosed.Status = 3;

                    break;

                case "canceled":

                    var ListCanceled = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                    ListCanceled.Status = 4;

                    /* crud Kanban */
                    crudKanbanOrder(DeliveryOrder.DONumber, transid, uid, DeliveryOrder.KanbanUsage, "delete");

                    /* crud stock details */
                    crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.DONumber, "delete");

                    break;

                case "delete":

                    /* remove existing DeliveryOrder */
                    var ListDelete = vssp_db.Tbl_TRS_DeliveryOrder.First(a => a.DONumber == DeliveryOrder.DONumber);

                    ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                    /* crud Kanban */
                    crudKanbanOrder(DeliveryOrder.DONumber, transid, uid, DeliveryOrder.KanbanUsage, formAction);

                    /* crud stock details */
                    crudDeliveryOrderDetail(DeliveryOrderDetail, DeliveryOrder.DONumber, DeliveryOrder.CustomerId, formAction);

                    //vssp_db.Tbl_TRS_DeliveryOrder.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                    break;
            }

            donumber = DeliveryOrder.DONumber;
            return donumber;

        }

        public void updateStatusSalesOrder(string sonumber, string formaction)
        {
            var status = vssp_db.Vw_TRS_SalesDeliveryStatus.FirstOrDefault(a => a.SONumber == sonumber);
            if (status != null)
            {
                var sales = vssp_db.Tbl_TRS_SalesOrder.First(a => a.SONumber == sonumber);

                if(sales != null)
                {
                    sales.Status = status.Status;
                    vssp_db.SaveChanges();
                }
            } else
            {
                var sales = vssp_db.Tbl_TRS_SalesOrder.First(a => a.SONumber == sonumber);

                if (sales != null)
                {
                    sales.Status = 0;
                    vssp_db.SaveChanges();
                }

            }

        }


        // NEW MODULE EXTRACT BARCODE KANBAN TBINA REGULAR => DEV BY DDE
        [HttpPost]
        public JsonResult UploadPdf()
        {
            try
            {
                if (Request.Files.Count == 0)
                    return Json(new { success = false, message = "No file uploaded" });

                HttpPostedFileBase file = Request.Files[0];

                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, message = "File is empty" });

                string filePath = System.IO.Path.Combine(Server.MapPath("~/Uploads"), System.IO.Path.GetFileName(file.FileName));
                file.SaveAs(filePath);
                var services = new PdfExtractorService();
                List<string> barcodes = services.ExtractBarcodesFromKanbanPDF(filePath);
                System.IO.File.Delete(filePath);
                return Json(new { success = true, barcodes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // NEW MODULE EXTRACT DATA FROM DN TBINA ADM => DEV BY DDE
        [HttpPost]
        public ActionResult ExtractDataFromDN(HttpPostedFileBase file, int page)
        {
            if (file == null || file.ContentLength == 0)
            {
                return Json(new { success = false, message = "File tidak ditemukan." });
            }

            try
            {
                // Simpan file sementara di server
                string filePath = System.IO.Path.Combine(Server.MapPath("~/Uploads"), file.FileName);
                file.SaveAs(filePath);

                // Panggil service untuk ekstraksi barcode dan data tabel
                var service = new PdfExtractorService();
                var (barcode, tableData) = service.ExtractData(filePath, page);

                // Hapus file setelah diproses untuk menghemat ruang
                System.IO.File.Delete(filePath);

                return Json(new
                {
                    success = true,
                    Barcode = barcode,
                    TableData = tableData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}