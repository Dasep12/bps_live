using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class ForecastOrderListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }    
    public class crudForecaseOrderDetailListModel
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
    public class ForecastWorkdayModel
    {
        public string M10 { get; set; }
        public Nullable<double> N10 { get; set; }
        public string M00 { get; set; }
        public Nullable<double> N00 { get; set; }
        public string M01 { get; set; }
        public Nullable<double> N01 { get; set; }
        public string M02 { get; set; }
        public Nullable<double> N02 { get; set; }
        public string M03 { get; set; }
        public Nullable<double> N03 { get; set; }
    }
    public class PostForecastOrderModel
    {
        public Tbl_TRS_ForecastOrder ForecastOrder { get; set; }
        public List<crud_ForecastOrderDetail> ForecastOrderDetail { get; set; }
        public List<crud_ForecastOrderRevision> ForecastOrderRevision { get; set; }
        public string ApprovalId { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_ForecastOrderDetail
    {
        public string RowStatus { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> OrderLastQty { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> DailyQty { get; set; }
        public Nullable<double> DailyLastQty { get; set; }
        public Nullable<double> N1 { get; set; }
        public Nullable<double> N2 { get; set; }
        public Nullable<double> N3 { get; set; }
        public Nullable<double> FluctuationQty { get; set; }
        public Nullable<double> FluctuationPercent { get; set; }
        public string SONumber { get; set; }

    }
    public partial class crud_ForecastOrderRevision
    {
        public string RowStatus { get; set; }
        public int RevisionNumber { get; set; }
        public string Description { get; set; }
        public Nullable<DateTime> RevisionDate { get; set; }
        public string RevisionBy { get; set; }

    }

    public class SupplierOrderListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class PostSupplierOrderModel
    {
        public Tbl_TRS_SupplierOrder SupplierOrder { get; set; }
        public List<crud_SupplierOrderDetail> SupplierOrderDetail { get; set; }
        public string ApprovalId { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_SupplierOrderDetail
    {
        public string RowStatus { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> MaxStock { get; set; }
        public Nullable<double> StockKanban { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> OrderUnitQty { get; set; }

    }
    public class ReturnPartListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class PostReturnPartModel
    {
        public Tbl_TRS_ReturnPart ReturnPart { get; set; }
        public List<crud_ReturnPartDetail> ReturnPartDetail { get; set; }
        public string ApprovalId { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_ReturnPartDetail
    {
        public string RowStatus { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> ReturnUnitQty { get; set; }

    }
    public class ReceivingOrderListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class PostReceivingOrderModel
    {
        public Tbl_TRS_ReceivingOrder ReceivingOrder { get; set; }
        public string ApprovalId { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_ReceivingOrderDetail
    {
        public string RowStatus { get; set; }
        public string ReceiveNumber { get; set; }
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public string KanbanKey { get; set; }
        public Nullable<double> ReceiveUnitQty { get; set; }
        public Nullable<System.DateTime> ScanTime { get; set; }


    }
    public class RequestOrderPartsListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class PostRequestOrderPartsModel
    {
        public Tbl_TRS_RequestOrderParts RequestOrderParts { get; set; }
        public List<crud_RequestOrderPartsDetail> RequestOrderPartsDetail { get; set; }
        public string ApprovalId { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_RequestOrderPartsDetail
    {
        public string RowStatus { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> StockKanban { get; set; }
        public Nullable<double> StockQty { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> OrderUnitQty { get; set; }
        public Nullable<double> ReceiveQty { get; set; }

    }
}