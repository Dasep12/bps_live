using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class SalesOrderListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class crudSalesOrderDetailListModel
    {
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> OrderLastQty { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> Workday { get; set; }
        public Nullable<double> DailyQty { get; set; }
        public Nullable<double> DailyLastQty { get; set; }
        public Nullable<double> N1 { get; set; }
        public Nullable<double> N2 { get; set; }
        public Nullable<double> N3 { get; set; }
        public Nullable<double> FluctuationQty { get; set; }
        public Nullable<double> FluctuationPercent { get; set; }
        public string formAction { get; set; }
    }

    public class PostSalesOrderModel
    {
        public Tbl_TRS_SalesOrder SalesOrder { get; set; }
        public List<crud_SalesOrderDetail> SalesOrderDetail { get; set; }
        public List<crud_ScheduleDelivery> ScheduleDelivery { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_SalesOrderDetail
    {
        public string RowStatus { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> OrderN1 { get; set; }
        public Nullable<double> OrderN2 { get; set; }
        public Nullable<double> OrderN3 { get; set; }
        public Nullable<double> DeliveryPerDay { get; set; }
    }
    public class crud_ScheduleDelivery
    {
        public string SONumber { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public System.DateTime DeliveryDate { get; set; }
        public Nullable<double> DeliveryQty { get; set; }
    }
    public class ImportSalesOrderListModel
    {
        public List<ImportSalesOrderModel> ImportSalesOrder { get; set; }
        public List<ImportSalesOrderDetailModel> ImportSalesOrderDetail { get; set; }
    }
    public class ImportSalesOrderModel
    {
        public string CustomerId { get; set; }
        public string PONumber { get; set; }
        public Nullable<System.DateTime> PODate { get; set; }
        public Nullable<System.DateTime> ReceiveDate { get; set; }
        public string DeliveryMonth { get; set; }
        public string DeliveryYear { get; set; }
        public Nullable<bool> PassThrough { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
    }
    public class ImportSalesOrderDetailModel
    {
        public string RowStatus { get; set; }
        public string CustomerId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public Nullable<double> QtyByKanban { get; set; }
        public string Unit { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> OrderN1 { get; set; }
        public Nullable<double> OrderN2 { get; set; }
        public Nullable<double> OrderN3 { get; set; }
        public Nullable<double> DeliveryPerDay { get; set; }

    }
    public class DeliveryScheduleListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
}