using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.WorkEntity;

namespace Core.VSSP.Models
{
    public class PrintOptionModel
    {
        public string MenuId { get; set; }
        public string GroupReport { get; set; }
        public string CategoryReport { get; set; }
        public string FileFormat { get; set; }
        public bool ToPrinter { get; set; }
    }

    public class FilterDataModel
    {
        public string SchemaName { get; set; }
        public string Field { get; set; }
        public string Caption { get; set; }
        public string FilterName { get; set; }
        public string FilterType { get; set; }
        public string FilterValues { get; set; }
    }

    public class PostPrintModel
    {
        public PrintOptionModel PrintOption { get; set; }
        public List<FilterDataModel> FilterData { get; set; }
    }
    public class ReportCompanyProfile
    {
        public string CompId { get; set; }
        public string CompName { get; set; }
        public byte[] CompLogo { get; set; }
        public string CompAddress { get; set; }
        public string CompCity { get; set; }
        public string CompCountry { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }

    }
    public class ReportDeliveryOrderModel
    {
        public string DONumber { get; set; }
        public DateTime DODate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string DeliveryAddress { get; set; }
        public string PONumber { get; set; }
        public string RefNumber { get; set; }
        public double TotalItem { get; set; }
        public double TotalDelivery { get; set; }
    }
    public class ReportDeliveryOrderAttentionModel
    {
        public string AttentionName { get; set; }
    }
    public class ReportDeliveryOrderDetailsModel
    {
        public string DONumber { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public double UnitsQty { get; set; }
        public string UnitLevel1 { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double PriceTotal { get; set; }
        public string UserId { get; set; }
        public DateTime ImportDate { get; set; }
    }
    public class ReportSalesOrderModel
    {
        public string SONumber { get; set; }
        public DateTime SODate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PONumber { get; set; }
        public DateTime PODate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string DeliveryMonth { get; set; }
        public string DeliveryYear { get; set; }
        public bool PassThrough { get; set; }
        public double Shift { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportSalesOrderDetailsModel
    {
        public string SONumber { get; set; }
        public string CustomerId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double OrderQty { get; set; }
        public double OrderN1 { get; set; }
        public double OrderN2 { get; set; }
        public double OrderN3 { get; set; }
        public double DeliveryPerDay { get; set; }
    }
    public class ReportForecastOrderModel
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderYear { get; set; }
        public string OrderMonth { get; set; }
        public double Shift { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public double TotalPart { get; set; }
        public double TotalOrder { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public string Approval { get; set; }
        public double Revision { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
        public string KanbanCycle { get; set; }
        public string KanbanTime { get; set; }
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
    }
    public class ReportForecastOrderDetailModel
    {
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public string Unit { get; set; }
        public double QtyByKanban { get; set; }
        public double OrderLastQty { get; set; }
        public double OrderQty { get; set; }
        public double DailyLastQty { get; set; }
        public double DailyQty { get; set; }
        public double N1 { get; set; }
        public double N2 { get; set; }
        public double N3 { get; set; }
        public double FluctuationQty { get; set; }
        public double FluctuationPercent { get; set; }
        public string Units { get; set; }
        public string SONumber { get; set; }
        public string PackingId { get; set; }
    }
    public class ReportForecastOrderRevisionModel
    {
        public string OrderNumber { get; set; }
        public double RevisionNumber { get; set; }
        public string RevisionDate { get; set; }
        public string RevisionBy { get; set; }
        public string Description { get; set; }
    }
    public class ReportForecastOrderApprovalModel
    {
        public string OrderNumber { get; set; }
        public string ApprovedDate { get; set; }
        public string UserName1 { get; set; }
        public string UserName2 { get; set; }
        public string UserName3 { get; set; }
        public string UserName4 { get; set; }
        public string UserName5 { get; set; }
        public Byte[] Sign1 { get; set; }
        public Byte[] Sign2 { get; set; }
        public Byte[] Sign3 { get; set; }
        public Byte[] Sign4 { get; set; }
        public Byte[] Sign5 { get; set; }
    }
    public class ReportMasterListKanbanModel
    {
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public string Unit { get; set; }
        public string Units { get; set; }
        public string PackingId { get; set; }
        public string LocationId { get; set; }
        public double QtyByKanban { get; set; }
        public double QtyByDay { get; set; }
        public double KanbanN10 { get; set; }
        public double KanbanN00 { get; set; }
        public double KanbanN01 { get; set; }
        public double KanbanReg { get; set; }
        public double SafetyHour { get; set; }
        public double SafetyKanban { get; set; }
        public double SafetyParts { get; set; }
        public double WorkHour { get; set; }
        public double Cycle1 { get; set; }
        public double Cycle2 { get; set; }
        public double Cycle3 { get; set; }
        public double MinStock { get; set; }
        public double MaxStock { get; set; }
    }
    public class ReportControlPlanningModel
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderYear { get; set; }
        public string OrderMonth { get; set; }
        public double Shift { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public double TotalPart { get; set; }
        public double TotalOrder { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public string Approval { get; set; }
        public double Revision { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
        public string KanbanCycle { get; set; }
        public string KanbanTime { get; set; }
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
    }
    public class ReportMasterListKanbanProductionModel
    {
        public string OrderNumber { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public string Unit { get; set; }
        public string Units { get; set; }
        public string PackingId { get; set; }
        public double OrderQty { get; set; }
        public double N1 { get; set; }
        public double N2 { get; set; }
        public double N3 { get; set; }
        public double QtyByDay { get; set; }
        public double QtyByShift { get; set; }
        public double QtyByKanban { get; set; }
        public double QtyByHour { get; set; }
        public double KanbanCalc1 { get; set; }
        public double KanbanCalc2 { get; set; }
        public double KanbanCalc3 { get; set; }
        public double KanbanCalc4 { get; set; }
        public double KanbanCalc5 { get; set; }
        public double KanbanN10 { get; set; }
        public double KanbanN00 { get; set; }
        public double KanbanN01 { get; set; }
        public double MinStock { get; set; }
        public double MaxStock { get; set; }
    }
    public class ReportSupplierOrderModel
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public string SSP { get; set; }
        public string ProcessName { get; set; }
        public string KanbanCycle { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime IncomingTime { get; set; }
        public double Shift { get; set; }
        public double TotalPart { get; set; }
        public double TotalOrder { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
        public string QrCode { get; set; }
    }
    public class ReportSupplierOrderDetailModel
    {
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public string PackingId { get; set; }
        public string UnitLevel1 { get; set; }
        public string UnitLevel2 { get; set; }
        public double UnitQty { get; set; }
        public double OrderQty { get; set; }
        public double OrderUnitQty { get; set; }
        public double ReceiveQty { get; set; }
        public string QrCode { get; set; }
    }
    public  class ReportSupplierOrderKanbanModel
    {
        public string KanbanKey { get; set; }
        public double KanbanRun { get; set; }
        public double KanbanTotal { get; set; }
        public string OrderNumber { get; set; }
        public string ReceiveNumber { get; set; }
        public string SupplierId { get; set; }
        public string CostId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public string Category { get; set; }
        public string Packing { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        public double UnitQty { get; set; }
        public string Unit { get; set; }
        public string KanbanCycle { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime IncomingTime { get; set; }
        public bool Received { get; set; }
        public string QrCode { get; set; }
        public string Barcode { get; set; }
    }
    public class ReportReturnPartModel
    {
        public string ReturnNumber { get; set; }
        public DateTime ReturnDate { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime IncomingTime { get; set; }
        public double TotalPart { get; set; }
        public double TotalUnitReturn { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
        public string QrCode { get; set; }
    }
    public class ReportReturnPartDetailModel
    {
        public string ReturnNumber { get; set; }
        public string SupplierId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public string PackingId { get; set; }
        public string UnitLevel1 { get; set; }
        public string UnitLevel2 { get; set; }
        public double UnitQty { get; set; }
        public double ReturnUnitQty { get; set; }
        public double ReceiveQty { get; set; }
        public string QrCode { get; set; }
    }
    public class ReportReceivingOrderModel
    {
        public string ReceiveNumber { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string KanbanCycle { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime IncomingTime { get; set; }
        public double TotalPart { get; set; }
        public double TotalReceive { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public bool ReturnPart { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime EditDate { get; set; }
        public string QrCode { get; set; }
    }
    public class ReportReceivingOrderDetailModel
    {
        public string ReceiveNumber { get; set; }
        public string OrderNumber { get; set; }
        public string SupplierId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public double UnitQty { get; set; }
        public string Unit { get; set; }
        public string PackingId { get; set; }
        public string PartModel { get; set; }
        public double OrderQty { get; set; }
        public double OrderUnitQty { get; set; }
        public double ReceiveQty { get; set; }
        public double ReceiveUnitQty { get; set; }
        public double OutstandingQty { get; set; }
        public double OutstandingUnitQty { get; set; }
    }
    public class ReportStockTakingModel
    {
        public string InventoryNumber { get; set; }
        public DateTime InventoryDate { get; set; }
        public DateTime InventoryStartTime { get; set; }
        public DateTime InventoryEndTime { get; set; }
        public double TotalPart { get; set; }
        public double TotalOrder { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public string Approval { get; set; }
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportStockTakingDetailModel
    {
        public string InventoryNumber { get; set; }
        public string StockType { get; set; }
        public string CustomerId { get; set; }
        public string LineId { get; set; }
        public string SupplierId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string AreaId { get; set; }
        public string LocationId { get; set; }
        public string CategoryId { get; set; }
        public string PartModel { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public string PackingId { get; set; }
        public double StockKanban { get; set; }
        public double StockQty { get; set; }
        public double ActualQty { get; set; }
        public double BalanceQty { get; set; }
    }
    public class ReportStockTakingApprovalModel
    {
        public string InventoryNumber { get; set; }
        public string ApprovedDate { get; set; }
        public string UserName1 { get; set; }
        public string UserName2 { get; set; }
        public string UserName3 { get; set; }
        public string UserName4 { get; set; }
        public Byte[] Sign1 { get; set; }
        public Byte[] Sign2 { get; set; }
        public Byte[] Sign3 { get; set; }
        public Byte[] Sign4 { get; set; }
    }
    public class ReportStockAdjustmentModel
    {
        public string AdjustmentNumber { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public double TotalPart { get; set; }
        public string InventoryNumber { get; set; }
        public string AreaId { get; set; }
        public string LocationId { get; set; }
        public double TotalOrder { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public string Approval { get; set; }
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportStockAdjustmentDetailModel
    {
        public string AdjustmentNumber { get; set; }
        public string CustomerId { get; set; }
        public string LineId { get; set; }
        public string SupplierId { get; set; }
        public string StockType { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string AreaId { get; set; }
        public string LocationId { get; set; }
        public string CategoryId { get; set; }
        public string PartModel { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public string PackingId { get; set; }
        public double StockKanban { get; set; }
        public double StockQty { get; set; }
        public double ActualQty { get; set; }
        public double BalanceQty { get; set; }
        public double AdjustmentQty { get; set; }
    }
    public class ReportStockAdjustmentApprovalModel
    {
        public string AdjustmentNumber { get; set; }
        public string ApprovedDate { get; set; }
        public string UserName1 { get; set; }
        public string UserName2 { get; set; }
        public string UserName3 { get; set; }
        public string UserName4 { get; set; }
        public Byte[] Sign1 { get; set; }
        public Byte[] Sign2 { get; set; }
        public Byte[] Sign3 { get; set; }
        public Byte[] Sign4 { get; set; }
    }
    public class ReportInvoiceRecapModel
    {
        public string RecapNumber { get; set; }
        public DateTime RecapDate { get; set; }
        public string RecapYear { get; set; }
        public string RecapMonth { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReceiveNote { get; set; }
        public string TaxInvoice { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public double TotalPrice { get; set; }
        public double PPN { get; set; }
        public double PPH23 { get; set; }
        public double DebitNote { get; set; }
        public double Payment { get; set; }
        public double TotalPart { get; set; }
        public double TotalRecap { get; set; }
        public double Status { get; set; }
        public string StatusName { get; set; }
        public bool Paid { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
        public string Approval { get; set; }
        public double ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
    }
    public class ReportInvoiceRecapDetailsModel
    {
        public string RecapNumber { get; set; }
        public string OrderNumber { get; set; }
        public string ReceiveNumber { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string SupplierId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public string ClassificationId { get; set; }
        public string PaymentId { get; set; }
        public string CategoryId { get; set; }
        public double RecapQty { get; set; }
        public double PriceQty { get; set; }
        public double TotalPrice { get; set; }
        public double PPN { get; set; }
        public double PPH23 { get; set; }
        public double DebitNote { get; set; }
        public double Payment { get; set; }
    }

    public partial class ReportRawMaterialTransactionModel
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public DateTime Date_Process { get; set; }
        public double ReceiveKanban { get; set; }
        public double ReceiveQty { get; set; }
        public double DeliveryKanban { get; set; }
        public double DeliveryQty { get; set; }
    }
    public class reportResultModel
    {
        List<Vw_TRS_DeliveryOrderDetail> orderDetails { get; set; }
    }
    public class ReportSalesOrder
    {
        public string SONumber { get; set; }
        public DateTime SODate { get; set; }
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string MonthNames { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public bool PassThrough { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double OrderQty { get; set; }
        public double PriceTotal { get; set; }
        public int Status { get; set; }
    }
    public class ReportSalesDelivery
    {
        public string DONumber { get; set; }
        public DateTime DODate { get; set; }
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string MonthNames { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool PassThrough { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double DeliveryQty { get; set; }
        public double PriceTotal { get; set; }
        public int Status { get; set; }
    }
    public class ReportSalesOrderSummary
    {
        public string Years { get; set; }
        public string MonthString { get; set; }
        public string MonthNames { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalItem { get; set; }
        public double TotalOrder { get; set; }
        public double TotalPrice { get; set; }
    }
    public class ReportSalesDeliverySummary
    {
        public string Years { get; set; }
        public string MonthString { get; set; }
        public string MonthNames { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalItem { get; set; }
        public double TotalDelivery { get; set; }
        public double TotalPrice { get; set; }
    }
    public class ReportSalesOrderYearly
    {
        public string Years { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double JAN_ITEM { get; set; }
        public double JAN_ORDER { get; set; }
        public double JAN_PRICE { get; set; }
        public double FEB_ITEM { get; set; }
        public double FEB_ORDER { get; set; }
        public double FEB_PRICE { get; set; }
        public double MAR_ITEM { get; set; }
        public double MAR_ORDER { get; set; }
        public double MAR_PRICE { get; set; }
        public double APR_ITEM { get; set; }
        public double APR_ORDER { get; set; }
        public double APR_PRICE { get; set; }
        public double MAY_ITEM { get; set; }
        public double MAY_ORDER { get; set; }
        public double MAY_PRICE { get; set; }
        public double JUN_ITEM { get; set; }
        public double JUN_ORDER { get; set; }
        public double JUN_PRICE { get; set; }
        public double JUL_ITEM { get; set; }
        public double JUL_ORDER { get; set; }
        public double JUL_PRICE { get; set; }
        public double AUG_ITEM { get; set; }
        public double AUG_ORDER { get; set; }
        public double AUG_PRICE { get; set; }
        public double SEP_ITEM { get; set; }
        public double SEP_ORDER { get; set; }
        public double SEP_PRICE { get; set; }
        public double OCT_ITEM { get; set; }
        public double OCT_ORDER { get; set; }
        public double OCT_PRICE { get; set; }
        public double NOV_ITEM { get; set; }
        public double NOV_ORDER { get; set; }
        public double NOV_PRICE { get; set; }
        public double DEC_ITEM { get; set; }
        public double DEC_ORDER { get; set; }
        public double DEC_PRICE { get; set; }
        public double TOT_ITEM { get; set; }
        public double TOT_ORDER { get; set; }
        public double TOT_PRICE { get; set; }
    }
    public class ReportSalesDeliveryYearly
    {
        public string Years { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double JAN_ITEM { get; set; }
        public double JAN_DELIVERY { get; set; }
        public double JAN_PRICE { get; set; }
        public double FEB_ITEM { get; set; }
        public double FEB_DELIVERY { get; set; }
        public double FEB_PRICE { get; set; }
        public double MAR_ITEM { get; set; }
        public double MAR_DELIVERY { get; set; }
        public double MAR_PRICE { get; set; }
        public double APR_ITEM { get; set; }
        public double APR_DELIVERY { get; set; }
        public double APR_PRICE { get; set; }
        public double MAY_ITEM { get; set; }
        public double MAY_DELIVERY { get; set; }
        public double MAY_PRICE { get; set; }
        public double JUN_ITEM { get; set; }
        public double JUN_DELIVERY { get; set; }
        public double JUN_PRICE { get; set; }
        public double JUL_ITEM { get; set; }
        public double JUL_DELIVERY { get; set; }
        public double JUL_PRICE { get; set; }
        public double AUG_ITEM { get; set; }
        public double AUG_DELIVERY { get; set; }
        public double AUG_PRICE { get; set; }
        public double SEP_ITEM { get; set; }
        public double SEP_DELIVERY { get; set; }
        public double SEP_PRICE { get; set; }
        public double OCT_ITEM { get; set; }
        public double OCT_DELIVERY { get; set; }
        public double OCT_PRICE { get; set; }
        public double NOV_ITEM { get; set; }
        public double NOV_DELIVERY { get; set; }
        public double NOV_PRICE { get; set; }
        public double DEC_ITEM { get; set; }
        public double DEC_DELIVERY { get; set; }
        public double DEC_PRICE { get; set; }
        public double TOT_ITEM { get; set; }
        public double TOT_DELIVERY { get; set; }
        public double TOT_PRICE { get; set; }
    }
    public class ReportSalesAchievement
    {
        public string Years { get; set; }
        public string MonthString { get; set; }
        public string MonthNames { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SONumber { get; set; }
        public DateTime SODate { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double SalesQty { get; set; }
        public double DeliveryQty { get; set; }
        public double BalanceQty { get; set; }
        public double SalesAmount { get; set; }
        public double DeliveryAmount { get; set; }
        public double BalanceAmount { get; set; }
        public bool PassThrough { get; set; }
        public int Status { get; set; }
    }
    public class ReportPartFinishGoodsPrice
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string CustomerUnitModel { get; set; }
        public double Price { get; set; }
        public bool Actived { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportPartRawMaterialsPrice
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public double Price { get; set; }
        public bool Actived { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportAchievementDelivery
    {
        public string Years { get; set; }
        public string Months { get; set; }
        public string MonthNames { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public double TotalOrder { get; set; }
        public double _01_Schedule { get; set; }
        public double _01_Delivery { get; set; }
        public double _02_Schedule { get; set; }
        public double _02_Delivery { get; set; }
        public double _03_Schedule { get; set; }
        public double _03_Delivery { get; set; }
        public double _04_Schedule { get; set; }
        public double _04_Delivery { get; set; }
        public double _05_Schedule { get; set; }
        public double _05_Delivery { get; set; }
        public double _06_Schedule { get; set; }
        public double _06_Delivery { get; set; }
        public double _07_Schedule { get; set; }
        public double _07_Delivery { get; set; }
        public double _08_Schedule { get; set; }
        public double _08_Delivery { get; set; }
        public double _09_Schedule { get; set; }
        public double _09_Delivery { get; set; }
        public double _10_Schedule { get; set; }
        public double _10_Delivery { get; set; }
        public double _11_Schedule { get; set; }
        public double _11_Delivery { get; set; }
        public double _12_Schedule { get; set; }
        public double _12_Delivery { get; set; }
        public double _13_Schedule { get; set; }
        public double _13_Delivery { get; set; }
        public double _14_Schedule { get; set; }
        public double _14_Delivery { get; set; }
        public double _15_Schedule { get; set; }
        public double _15_Delivery { get; set; }
        public double _16_Schedule { get; set; }
        public double _16_Delivery { get; set; }
        public double _17_Schedule { get; set; }
        public double _17_Delivery { get; set; }
        public double _18_Schedule { get; set; }
        public double _18_Delivery { get; set; }
        public double _19_Schedule { get; set; }
        public double _19_Delivery { get; set; }
        public double _20_Schedule { get; set; }
        public double _20_Delivery { get; set; }
        public double _21_Schedule { get; set; }
        public double _21_Delivery { get; set; }
        public double _22_Schedule { get; set; }
        public double _22_Delivery { get; set; }
        public double _23_Schedule { get; set; }
        public double _23_Delivery { get; set; }
        public double _24_Schedule { get; set; }
        public double _24_Delivery { get; set; }
        public double _25_Schedule { get; set; }
        public double _25_Delivery { get; set; }
        public double _26_Schedule { get; set; }
        public double _26_Delivery { get; set; }
        public double _27_Schedule { get; set; }
        public double _27_Delivery { get; set; }
        public double _28_Schedule { get; set; }
        public double _28_Delivery { get; set; }
        public double _29_Schedule { get; set; }
        public double _29_Delivery { get; set; }
        public double _30_Schedule { get; set; }
        public double _30_Delivery { get; set; }
        public double _31_Schedule { get; set; }
        public double _31_Delivery { get; set; }
        public double Total_Schedule { get; set; }
        public double Total_Delivery { get; set; }
    }
    public class ReportOutstandingDelivery
    {
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string SONumber { get; set; }
        public DateTime SODate { get; set; }
        public string CustomerId { get; set; }
        public string PONumber { get; set; }
        public DateTime PODate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string PartName { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double OrderKanban { get; set; }
        public double OrderQty { get; set; }
        public double DeliveryKanban { get; set; }
        public double DeliveryQty { get; set; }
        public double OutstandingKanban { get; set; }
        public double OutstandingQty { get; set; }
        public int Status { get; set; }
    }

    public class ReportCustomerInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress1 { get; set; }
        public string CustomerAddress2 { get; set; }
        public double SubTotal { get; set; }
        public double PPN { get; set; }
        public double PPH23 { get; set; }
        public double GrandTotal { get; set; }
        public string Terms { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string ApprovalName { get; set; }
    }
    public class ReportCustomerInvoiceDetailModel
    {
        public string InvoiceNumber { get; set; }
        public string CustomerId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double InvoiceQty { get; set; }
        public double Amount { get; set; }
    }
    public class ReportAccountingBankAccountModel
    {
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
    }
    public class ReportCustomerInvoiceApprovalModel
    {
        public string InvoiceNumber { get; set; }
        public string ApprovedDate { get; set; }
        public string UserName1 { get; set; }
        public string UserName2 { get; set; }
        public string UserName3 { get; set; }
        public Byte[] Sign1 { get; set; }
        public Byte[] Sign2 { get; set; }
        public Byte[] Sign3 { get; set; }
    }
    public class ReportCustomerInvoiceDeliveryRecapModel
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string DONumber { get; set; }
        public DateTime DODate { get; set; }
        public string SONumber { get; set; }
        public string PONumber { get; set; }
        public DateTime PODate { get; set; }
        public string RefNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public Double DeliveryUnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public string InvoiceNumber { get; set; }
        public bool Invoiced { get; set; }
    }
    public class ReportSupplierOrderShortage
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime IncomingDate { get; set; }
        //public string ReceiveNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public double UnitQty { get; set; }
        public DateTime IncomingTime { get; set; }
        public DateTime ReceiveDate { get; set; }
        public double IncomingDelay { get; set; }
        public double OrderQty { get; set; }
        public double OrderUnitQty { get; set; }
        public double ReceiveUnitQty { get; set; }
        public double OutstandingQty { get; set; }
        public int Score { get; set; }
        public string Remarks { get; set; }
    }
    public class ReportSupplierOrderRecap
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderYear { get; set; }
        public string OrderMonth { get; set; }
        public string MonthNames { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberSupplier { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double OrderQty { get; set; }
        public double PriceTotal { get; set; }
        public int Status { get; set; }
    }
    public class ReportSupplierOrderYearly
    {
        public string Years { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public double JAN_ITEM { get; set; }
        public double JAN_PURCHASE { get; set; }
        public double JAN_PRICE { get; set; }
        public double FEB_ITEM { get; set; }
        public double FEB_PURCHASE { get; set; }
        public double FEB_PRICE { get; set; }
        public double MAR_ITEM { get; set; }
        public double MAR_PURCHASE { get; set; }
        public double MAR_PRICE { get; set; }
        public double APR_ITEM { get; set; }
        public double APR_PURCHASE { get; set; }
        public double APR_PRICE { get; set; }
        public double MAY_ITEM { get; set; }
        public double MAY_PURCHASE { get; set; }
        public double MAY_PRICE { get; set; }
        public double JUN_ITEM { get; set; }
        public double JUN_PURCHASE { get; set; }
        public double JUN_PRICE { get; set; }
        public double JUL_ITEM { get; set; }
        public double JUL_PURCHASE { get; set; }
        public double JUL_PRICE { get; set; }
        public double AUG_ITEM { get; set; }
        public double AUG_PURCHASE { get; set; }
        public double AUG_PRICE { get; set; }
        public double SEP_ITEM { get; set; }
        public double SEP_PURCHASE { get; set; }
        public double SEP_PRICE { get; set; }
        public double OCT_ITEM { get; set; }
        public double OCT_PURCHASE { get; set; }
        public double OCT_PRICE { get; set; }
        public double NOV_ITEM { get; set; }
        public double NOV_PURCHASE { get; set; }
        public double NOV_PRICE { get; set; }
        public double DEC_ITEM { get; set; }
        public double DEC_PURCHASE { get; set; }
        public double DEC_PRICE { get; set; }
        public double TOT_ITEM { get; set; }
        public double TOT_PURCHASE { get; set; }
        public double TOT_PRICE { get; set; }
    }
    public class ReportReceiveOrderRecap
    {
        public string ReceiveNumber { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string OrderYear { get; set; }
        public string OrderMonth { get; set; }
        public string MonthNames { get; set; }
        public string OrderNumber { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberSupplier { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double ReceiveQty { get; set; }
        public double PriceTotal { get; set; }
        public int Status { get; set; }
    }
    public class ReportReceiveOrderYearly
    {
        public string Years { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public double JAN_ITEM { get; set; }
        public double JAN_RECEIVE { get; set; }
        public double JAN_PRICE { get; set; }
        public double FEB_ITEM { get; set; }
        public double FEB_RECEIVE { get; set; }
        public double FEB_PRICE { get; set; }
        public double MAR_ITEM { get; set; }
        public double MAR_RECEIVE { get; set; }
        public double MAR_PRICE { get; set; }
        public double APR_ITEM { get; set; }
        public double APR_RECEIVE { get; set; }
        public double APR_PRICE { get; set; }
        public double MAY_ITEM { get; set; }
        public double MAY_RECEIVE { get; set; }
        public double MAY_PRICE { get; set; }
        public double JUN_ITEM { get; set; }
        public double JUN_RECEIVE { get; set; }
        public double JUN_PRICE { get; set; }
        public double JUL_ITEM { get; set; }
        public double JUL_RECEIVE { get; set; }
        public double JUL_PRICE { get; set; }
        public double AUG_ITEM { get; set; }
        public double AUG_RECEIVE { get; set; }
        public double AUG_PRICE { get; set; }
        public double SEP_ITEM { get; set; }
        public double SEP_RECEIVE { get; set; }
        public double SEP_PRICE { get; set; }
        public double OCT_ITEM { get; set; }
        public double OCT_RECEIVE { get; set; }
        public double OCT_PRICE { get; set; }
        public double NOV_ITEM { get; set; }
        public double NOV_RECEIVE { get; set; }
        public double NOV_PRICE { get; set; }
        public double DEC_ITEM { get; set; }
        public double DEC_RECEIVE { get; set; }
        public double DEC_PRICE { get; set; }
        public double TOT_ITEM { get; set; }
        public double TOT_RECEIVE { get; set; }
        public double TOT_PRICE { get; set; }
    }
    public class ReportAchievementForecast
    {
        public string Years { get; set; }
        public string Months { get; set; }
        public string MonthNames { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public double TotalForecast { get; set; }
        public double _01_Purchase { get; set; }
        public double _01_Receive { get; set; }
        public double _02_Purchase { get; set; }
        public double _02_Receive { get; set; }
        public double _03_Purchase { get; set; }
        public double _03_Receive { get; set; }
        public double _04_Purchase { get; set; }
        public double _04_Receive { get; set; }
        public double _05_Purchase { get; set; }
        public double _05_Receive { get; set; }
        public double _06_Purchase { get; set; }
        public double _06_Receive { get; set; }
        public double _07_Purchase { get; set; }
        public double _07_Receive { get; set; }
        public double _08_Purchase { get; set; }
        public double _08_Receive { get; set; }
        public double _09_Purchase { get; set; }
        public double _09_Receive { get; set; }
        public double _10_Purchase { get; set; }
        public double _10_Receive { get; set; }
        public double _11_Purchase { get; set; }
        public double _11_Receive { get; set; }
        public double _12_Purchase { get; set; }
        public double _12_Receive { get; set; }
        public double _13_Purchase { get; set; }
        public double _13_Receive { get; set; }
        public double _14_Purchase { get; set; }
        public double _14_Receive { get; set; }
        public double _15_Purchase { get; set; }
        public double _15_Receive { get; set; }
        public double _16_Purchase { get; set; }
        public double _16_Receive { get; set; }
        public double _17_Purchase { get; set; }
        public double _17_Receive { get; set; }
        public double _18_Purchase { get; set; }
        public double _18_Receive { get; set; }
        public double _19_Purchase { get; set; }
        public double _19_Receive { get; set; }
        public double _20_Purchase { get; set; }
        public double _20_Receive { get; set; }
        public double _21_Purchase { get; set; }
        public double _21_Receive { get; set; }
        public double _22_Purchase { get; set; }
        public double _22_Receive { get; set; }
        public double _23_Purchase { get; set; }
        public double _23_Receive { get; set; }
        public double _24_Purchase { get; set; }
        public double _24_Receive { get; set; }
        public double _25_Purchase { get; set; }
        public double _25_Receive { get; set; }
        public double _26_Purchase { get; set; }
        public double _26_Receive { get; set; }
        public double _27_Purchase { get; set; }
        public double _27_Receive { get; set; }
        public double _28_Purchase { get; set; }
        public double _28_Receive { get; set; }
        public double _29_Purchase { get; set; }
        public double _29_Receive { get; set; }
        public double _30_Purchase { get; set; }
        public double _30_Receive { get; set; }
        public double _31_Purchase { get; set; }
        public double _31_Receive { get; set; }
        public double Total_Purchase { get; set; }
        public double Total_Receive { get; set; }
    }
    public class SupplierOrderPerformance
    {
        public string PerformanceYear { get; set; }
        public string PerformanceMonth { get; set; }
        public string OrderNumber { get; set; }
        public string ReceiveNumber { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public DateTime IncomingTime { get; set; }
        public DateTime ReceiveDate { get; set; }
        public Double DiffTime { get; set; }
        public Double OrderQty { get; set; }
        public Double OrderUnitQty { get; set; }
        public double ReceiveQty { get; set; }
        public double ReceiveUnitQty { get; set; }
        public Double OutstandingQty { get; set; }
        public Double OutstandingUnitQty { get; set; }
        public string PerformanceNumber { get; set; }
        public DateTime PerformanceDate { get; set; }
        public string PerformanceRemarks { get; set; }
        public string Problem { get; set; }
        public string Actions { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public Double DeliveryQty { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportDeliveryOrderRecap
    {
        public string DONumber { get; set; }
        public DateTime DODate { get; set; }
        public string RefNumber { get; set; }
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string MonthNames { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public bool PassThrough { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double DeliveryQty { get; set; }
        public double PriceTotal { get; set; }
        public int Status { get; set; }
    }
    public class ReportSupplierInvoiceReceivingRecap
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ReceiveNumber { get; set; }
        public DateTime ReceivingDate { get; set; }
        public string OrderNumber { get; set; }
        public string PONumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Remarks { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public double ReceiveUnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public string InvoiceNumber { get; set; }
        public bool Invoiced { get; set; }
    }
    public class ReportSupplierInvoiceReceivingSummary
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string MonthNames { get; set; }
        public double TotalReceiving { get; set; }
        public double TotalInvoice { get; set; }
        public double Balance { get; set; }
    }
    public class ReportCustomerInvoiceDeliveryRecap
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string DONumber { get; set; }
        public DateTime DODate { get; set; }
        public string SONumber { get; set; }
        public string PONumber { get; set; }
        public DateTime PODate { get; set; }
        public string RefNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public double DeliveryUnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public string InvoiceNumber { get; set; }
        public bool Invoiced { get; set; }
    }
    public class ReportCustomerInvoiceDeliverySummary
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string MonthNames { get; set; }
        public double TotalDelivery { get; set; }
        public double TotalInvoice { get; set; }
        public double Balance { get; set; }
    }
    public class ReportKanbanProductionList
    {
        public string KanbanKey { get; set; }
        public string KanbanCode { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public string PackingId { get; set; }
        public string LocationId { get; set; }
        public double KanbanRun { get; set; }
        public bool Actived { get; set; }
        public string QrCode { get; set; }
        public string BarCode { get; set; }
    }
    public class ReportLabelProductionList
    {
        public string ProductionNumber { get; set; }
        public DateTime ProductionDate { get; set; }
        public string KanbanKey { get; set; }
        public string KanbanCode { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string GateId { get; set; }
        public string GateName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Model { get; set; }
        public string PartIdentity { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public string PackingId { get; set; }
        public string LocationId { get; set; }
        public double KanbanRun { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectorName { get; set; }
        public bool Actived { get; set; }
        public string QrCode { get; set; }
    }
    public class ReportRequestOrderPartsModel
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string ShiftId { get; set; }
        public int TotalPart { get; set; }
        public double TotalOrder { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string Approval { get; set; }
        public int ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportRequestOrderPartsDetailsModel
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string LineId { get; set; }
        public string SupplierId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public double UnitQty { get; set; }
        public string Unit { get; set; }
        public double OrderQty { get; set; }
        public double OrderUnitQty { get; set; }
        public double ReceiveQty { get; set; }
        public double OutstandingQty { get; set; }
        public double StockKanban { get; set; }
        public double StockQty { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
    }
    public partial class ReportProductionRecap
    {
        public DateTime ProductionDate { get; set; }
        public string ShiftId { get; set; }
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string MonthNames { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public bool PassThrough { get; set; }
        public string UnitLevel2 { get; set; }
        public double PriceUnit { get; set; }
        public double ProcessQty { get; set; }
        public double PriceTotal { get; set; }
    }
    public class ReportProductionYearly
    {
        public string Years { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public double JAN_ITEM { get; set; }
        public double JAN_PRODUCTION { get; set; }
        public double JAN_PRICE { get; set; }
        public double FEB_ITEM { get; set; }
        public double FEB_PRODUCTION { get; set; }
        public double FEB_PRICE { get; set; }
        public double MAR_ITEM { get; set; }
        public double MAR_PRODUCTION { get; set; }
        public double MAR_PRICE { get; set; }
        public double APR_ITEM { get; set; }
        public double APR_PRODUCTION { get; set; }
        public double APR_PRICE { get; set; }
        public double MAY_ITEM { get; set; }
        public double MAY_PRODUCTION { get; set; }
        public double MAY_PRICE { get; set; }
        public double JUN_ITEM { get; set; }
        public double JUN_PRODUCTION { get; set; }
        public double JUN_PRICE { get; set; }
        public double JUL_ITEM { get; set; }
        public double JUL_PRODUCTION { get; set; }
        public double JUL_PRICE { get; set; }
        public double AUG_ITEM { get; set; }
        public double AUG_PRODUCTION { get; set; }
        public double AUG_PRICE { get; set; }
        public double SEP_ITEM { get; set; }
        public double SEP_PRODUCTION { get; set; }
        public double SEP_PRICE { get; set; }
        public double OCT_ITEM { get; set; }
        public double OCT_PRODUCTION { get; set; }
        public double OCT_PRICE { get; set; }
        public double NOV_ITEM { get; set; }
        public double NOV_PRODUCTION { get; set; }
        public double NOV_PRICE { get; set; }
        public double DEC_ITEM { get; set; }
        public double DEC_PRODUCTION { get; set; }
        public double DEC_PRICE { get; set; }
        public double TOT_ITEM { get; set; }
        public double TOT_PRODUCTION { get; set; }
        public double TOT_PRICE { get; set; }
    }
    public class ReportAchievementProduction
    {
        public string Years { get; set; }
        public string Months { get; set; }
        public string MonthNames { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public double TotalOrder { get; set; }
        public double _01_Schedule { get; set; }
        public double _01_Production { get; set; }
        public double _02_Schedule { get; set; }
        public double _02_Production { get; set; }
        public double _03_Schedule { get; set; }
        public double _03_Production { get; set; }
        public double _04_Schedule { get; set; }
        public double _04_Production { get; set; }
        public double _05_Schedule { get; set; }
        public double _05_Production { get; set; }
        public double _06_Schedule { get; set; }
        public double _06_Production { get; set; }
        public double _07_Schedule { get; set; }
        public double _07_Production { get; set; }
        public double _08_Schedule { get; set; }
        public double _08_Production { get; set; }
        public double _09_Schedule { get; set; }
        public double _09_Production { get; set; }
        public double _10_Schedule { get; set; }
        public double _10_Production { get; set; }
        public double _11_Schedule { get; set; }
        public double _11_Production { get; set; }
        public double _12_Schedule { get; set; }
        public double _12_Production { get; set; }
        public double _13_Schedule { get; set; }
        public double _13_Production { get; set; }
        public double _14_Schedule { get; set; }
        public double _14_Production { get; set; }
        public double _15_Schedule { get; set; }
        public double _15_Production { get; set; }
        public double _16_Schedule { get; set; }
        public double _16_Production { get; set; }
        public double _17_Schedule { get; set; }
        public double _17_Production { get; set; }
        public double _18_Schedule { get; set; }
        public double _18_Production { get; set; }
        public double _19_Schedule { get; set; }
        public double _19_Production { get; set; }
        public double _20_Schedule { get; set; }
        public double _20_Production { get; set; }
        public double _21_Schedule { get; set; }
        public double _21_Production { get; set; }
        public double _22_Schedule { get; set; }
        public double _22_Production { get; set; }
        public double _23_Schedule { get; set; }
        public double _23_Production { get; set; }
        public double _24_Schedule { get; set; }
        public double _24_Production { get; set; }
        public double _25_Schedule { get; set; }
        public double _25_Production { get; set; }
        public double _26_Schedule { get; set; }
        public double _26_Production { get; set; }
        public double _27_Schedule { get; set; }
        public double _27_Production { get; set; }
        public double _28_Schedule { get; set; }
        public double _28_Production { get; set; }
        public double _29_Schedule { get; set; }
        public double _29_Production { get; set; }
        public double _30_Schedule { get; set; }
        public double _30_Production { get; set; }
        public double _31_Schedule { get; set; }
        public double _31_Production { get; set; }
        public double Total_Schedule { get; set; }
        public double Total_Production { get; set; }
    }
    public class ReportDefectInGate
    {
        public int Num { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string InspectionGate { get; set; }
        public DateTime InspectionDate { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public double TotalCheck { get; set; }
        public double TotalDefect { get; set; }

    }
    public class ReportParetoDefectPart
    {
        public int Num { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string InspectionGate { get; set; }
        public DateTime InspectionDate { get; set; }
        public string DefectId { get; set; }
        public string DefectName { get; set; }
        public double QtyDefect { get; set; }
    }
    public class ReportEfficiencyInspector
    {
        public string Years { get; set; }
        public string Months { get; set; }
        public string InspectionNumber { get; set; }
        public string InspectionGate { get; set; }
        public DateTime InspectionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public double TotalTime { get; set; }
        public double CycleTime { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string CustomerUnitModel { get; set; }
        public double UnitQty { get; set; }
        public string PartCategory { get; set; }
        public string PI_Images { get; set; }
        public double TotalCheck { get; set; }
        public double TotalDefectQty { get; set; }
        public double Replaced { get; set; }
        public double Remains { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportTopDefect
    {
        public int Num { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string InspectionGate { get; set; }
        public DateTime InspectionDate { get; set; }
        public string DefectId { get; set; }
        public string DefectName { get; set; }
        public double QtyDefect { get; set; }
    }
    public class ReportTopWorstSupplierIncoming
    {
        public int Num { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string InspectionGate { get; set; }
        public DateTime InspectionDate { get; set; }
        public string DefectId { get; set; }
        public string DefectName { get; set; }
        public double DefectQty { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumberSupplier { get; set; }
        public string PartCategory { get; set; }
        public string PartCategoryName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
    }
    public class ReportTopWorstSupplier
    {
        public int Num { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string InspectionGate { get; set; }
        public DateTime InspectionDate { get; set; }
        public string DefectId { get; set; }
        public string DefectName { get; set; }
        public double DefectQty { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string PartCategory { get; set; }
        public string PartCategoryName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
    }
    public class PdfPathModel
    {
        public string pdfUrl { get; set; }
        public string pdfPath { get; set; }
    }
    public class ReportForecastAchievementModel
    {
        public string OrderYear { get; set; }
        public string OrderMonth { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public double UnitQty { get; set; }
        public double OrderQty { get; set; }
        public double ReceivedQty { get; set; }
        public double BalanceQty { get; set; }
        public double BalancePercent { get; set; }
    }
    public class ReportStockControlModel
    {
        public string OrderYear { get; set; }
        public string OrderMonth { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public double UnitQty { get; set; }
        public double OrderQty { get; set; }
        public double DailyQty { get; set; }
        public double EarlyStock { get; set; }
        public double _01_StockIn { get; set; }
        public double _01_StockOut { get; set; }
        public double _02_StockIn { get; set; }
        public double _02_StockOut { get; set; }
        public double _03_StockIn { get; set; }
        public double _03_StockOut { get; set; }
        public double _04_StockIn { get; set; }
        public double _04_StockOut { get; set; }
        public double _05_StockIn { get; set; }
        public double _05_StockOut { get; set; }
        public double _06_StockIn { get; set; }
        public double _06_StockOut { get; set; }
        public double _07_StockIn { get; set; }
        public double _07_StockOut { get; set; }
        public double _08_StockIn { get; set; }
        public double _08_StockOut { get; set; }
        public double _09_StockIn { get; set; }
        public double _09_StockOut { get; set; }
        public double _10_StockIn { get; set; }
        public double _10_StockOut { get; set; }
        public double _11_StockIn { get; set; }
        public double _11_StockOut { get; set; }
        public double _12_StockIn { get; set; }
        public double _12_StockOut { get; set; }
        public double _13_StockIn { get; set; }
        public double _13_StockOut { get; set; }
        public double _14_StockIn { get; set; }
        public double _14_StockOut { get; set; }
        public double _15_StockIn { get; set; }
        public double _15_StockOut { get; set; }
        public double _16_StockIn { get; set; }
        public double _16_StockOut { get; set; }
        public double _17_StockIn { get; set; }
        public double _17_StockOut { get; set; }
        public double _18_StockIn { get; set; }
        public double _18_StockOut { get; set; }
        public double _19_StockIn { get; set; }
        public double _19_StockOut { get; set; }
        public double _20_StockIn { get; set; }
        public double _20_StockOut { get; set; }
        public double _21_StockIn { get; set; }
        public double _21_StockOut { get; set; }
        public double _22_StockIn { get; set; }
        public double _22_StockOut { get; set; }
        public double _23_StockIn { get; set; }
        public double _23_StockOut { get; set; }
        public double _24_StockIn { get; set; }
        public double _24_StockOut { get; set; }
        public double _25_StockIn { get; set; }
        public double _25_StockOut { get; set; }
        public double _26_StockIn { get; set; }
        public double _26_StockOut { get; set; }
        public double _27_StockIn { get; set; }
        public double _27_StockOut { get; set; }
        public double _28_StockIn { get; set; }
        public double _28_StockOut { get; set; }
        public double _29_StockIn { get; set; }
        public double _29_StockOut { get; set; }
        public double _30_StockIn { get; set; }
        public double _30_StockOut { get; set; }
        public double _31_StockIn { get; set; }
        public double _31_StockOut { get; set; }
    }
    public class ReportStockLevelFGModel
    {
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string CustomerUnitModel { get; set; }
        public double UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double ForecastQty { get; set; }
        public int WorkDay { get; set; }
        public double VolumeDay { get; set; }
        public double StockQty { get; set; }
        public double StockDay { get; set; }
        public double SafetyQty { get; set; }
        public int SafetyDay { get; set; }
    }
    public class ReportSalesForecastAchiementModel
    {
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string CustomerUnitModel { get; set; }
        public double OrderQty { get; set; }
        public double DeliveryQty { get; set; }
        public double BalanceQty { get; set; }
        public double BalancePercent { get; set; }
    }
    public class ReportPriceListRawMaterialsModel
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public string Years { get; set; }
        public double JAN { get; set; }
        public double FEB { get; set; }
        public double MAR { get; set; }
        public double APR { get; set; }
        public double MAY { get; set; }
        public double JUN { get; set; }
        public double JUL { get; set; }
        public double AUG { get; set; }
        public double SEP { get; set; }
        public double OCT { get; set; }
        public double NOV { get; set; }
        public double DEC { get; set; }
        public bool Actived { get; set; }
        public string UserId { get; set; }
        public DateTime EditDate { get; set; }
    }
    public class ReportFinanceStockModel {
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public string StockType { get; set; }
        public string UnitLevel2 { get; set; }
        public double UnitQty { get; set; }
        public string PartModel { get; set; }
        public double EarlyStockQty { get; set; }
        public double StockIn { get; set; }
        public double StockOut { get; set; }
        public double StockBalance { get; set; }
        public double InStockQty { get; set; }
        public double AbnormalQty { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
    }
}