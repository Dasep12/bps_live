using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class StockRawMaterialListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_MST_Supplier> SupplierList { get; set; }
        public List<Tbl_MST_WarehouseArea> AreaList { get; set; }
        public List<Tbl_MST_WarehouseLocation> LocationList { get; set; }
    }
    public class StockRawReturnListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_MST_Supplier> SupplierList { get; set; }
        public List<Tbl_MST_WarehouseArea> AreaList { get; set; }
        public List<Tbl_MST_WarehouseLocation> LocationList { get; set; }
    }
    public class StockFinishGoodsListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_MST_Customer> CustomerList { get; set; }
        public List<Tbl_MST_WarehouseArea> AreaList { get; set; }
        public List<Tbl_MST_WarehouseLocation> LocationList { get; set; }
    }
    public class StockWaitInProcessListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_MST_Supplier> SupplierList { get; set; }
        public List<Tbl_MST_WarehouseArea> AreaList { get; set; }
        public List<Tbl_MST_WarehouseLocation> LocationList { get; set; }
    }
    public class StockTakingListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }

    }
    public class PostStockTakingModel
    {
        public Tbl_TRS_StockTaking StockTaking { get; set; }
        public List<crud_StockTakingDetail> StockTakingDetail { get; set; }
        public string ApprovalId { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_StockTakingDetail
    {
        public string RowStatus { get; set; }
        public string CustomerId { get; set; }
        public string LineId { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> StockKanban { get; set; }
        public Nullable<double> StockQty { get; set; }
        public Nullable<double> ActualQty { get; set; }
        public Nullable<double> BalanceQty { get; set; }

    }
    public class StockAdjustmentListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }

    }
    public class PostStockAdjustmentModel
    {
        public Tbl_TRS_StockAdjustment StockAdjustment { get; set; }
        public List<crud_StockAdjustmentDetail> StockAdjustmentDetail { get; set; }
        public string ApprovalId { get; set; }
        public string formAction { get; set; }

    }
    public partial class crud_StockAdjustmentDetail
    {
        public string RowStatus { get; set; }
        public string CustomerId { get; set; }
        public string LineId { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> StockKanban { get; set; }
        public Nullable<double> StockQty { get; set; }
        public Nullable<double> ActualQty { get; set; }
        public Nullable<double> BalanceQty { get; set; }
        public Nullable<double> AdjustmentQty { get; set; }

    }
}