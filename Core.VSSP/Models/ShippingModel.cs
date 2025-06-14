using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class PostDeliveryImportModel
    {
        public List<ImportDeliveryOrderModel> deliveryOrder { get; set; }
        public string replace { get; set; }
        public string formAction { get; set; }
    }
    public class ImportDeliveryListModel
    {
        public List<ImportDeliveryOrderModel> ImportDeliveryOrder { get; set; }
    }
    public class ImportDeliveryOrderModel
    {
        public string DONumber { get; set; }
        public DateTime DODate { get; set; }
        public string RefNumber { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public double Qty { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    
    public class PostDeliveryOrderImportModel
    {
        public Tbl_TRS_DeliveryOrderImport DeliveryOrderImport { get; set; }
        public string formAction { get; set; }
    }

    public class DeliveryOrderListModel
    {
        public string ExportOption { get; set; }
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class DeliveryOrderOutstandingStockModel
    {
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<Double> StockKanban { get; set; }
        public Nullable<Double> StockQty { get; set; }
        public Nullable<Double> OutstandingKanban { get; set; }
        public Nullable<Double> OutstandingQty { get; set; }
    }
    public class PostDeliveryOrderModel
    {
        public Tbl_TRS_DeliveryOrder DeliveryOrder { get; set; }
        public List<crud_DeliveryOrderDetail> DeliveryOrderDetail { get; set; }
        public string transid { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public class crud_DeliveryOrderDetail
    {
        public string RowStatus { get; set; }
        public string DONumber { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> DeliveryQty { get; set; }
        public Nullable<double> DeliveryUnitQty { get; set; }
    }
    public class KanbanOrderListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class crud_DeliveryOrderKanban
    {
        public string DONumber { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public string KanbanNumber { get; set; }
        public string RefNumber { get; set; }
        public Nullable<double> DeliveryQty { get; set; }
        public string KanbanData { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> ScanTime { get; set; }
    }
    public class KanbanDataModel
    {
        public string CustomerId { get; set; }
        public string OrderNumber { get; set; }
        public string RefNumber { get; set; }
        public Nullable<DateTime> OrderDate { get; set; }
        public string KanbanNumber { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public double OrderQty { get; set; }
        public string ErrMessages { get; set; }
    }
}