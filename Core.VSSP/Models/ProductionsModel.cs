using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class ImportProductionsListModel
    {
        public List<ImportLineModel> ImportLine { get; set; }
    }
    public class ImportLineModel
    {
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string AreaId { get; set; }
        public string LocationId { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    
    public class PostLineModel
    {
        public Tbl_MST_Line Line { get; set; }
        public List<crud_KanbanCalculation> KanbanCalculation { get; set; }
        public List<crud_LineGate> LineGate { get; set; }
        public string formAction { get; set; }
    }
    public class crud_KanbanCalculation
    {
        public string LineId { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<double> InProcess { get; set; }
        public Nullable<double> Stock { get; set; }
        public Nullable<double> PrepareHeijunka { get; set; }
        public Nullable<double> WIP { get; set; }
        public Nullable<double> PrepareDelivery { get; set; }
        public string RowStatus { get; set; }
    }
    public class crud_LineGate
    {
        public string LineId { get; set; }
        public string GateId { get; set; }
        public string GateName { get; set; }
        public string RowStatus { get; set; }
    }
    public class ImportProductionMaterialsListModel
    {
        public List<ImportProductionMaterialModel> ImportProductionMaterial { get; set; }
        public List<ImportProductionMaterialPriceModel> ImportProductionMaterialPrice { get; set; }

    }
    public class ImportProductionMaterialModel
    {
        public string LineId { get; set; }
        public string PartNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public string CategoryId { get; set; }
        public string PackingId { get; set; }
        public string AreaId { get; set; }
        public string LocationId { get; set; }
        public string UnitLevel1 { get; set; }
        public string UnitLevel2 { get; set; }
        public double UnitQty { get; set; }
        public double SafetyHours { get; set; }
        public bool SubProcess { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportProductionMaterialPriceModel
    {
        public string LineId { get; set; }
        public string PartNumber { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public double Price { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }    
    public class PostProductionMaterialModel
    {
        public Tbl_MST_PartProductionMaterials ProductionMaterial { get; set; }
        public List<crud_PartProductionMaterialsPrice> ProductionMaterialPrice { get; set; }
        public string formAction { get; set; }
    }   
    public partial class crud_PartProductionMaterialsPrice
    {
        public string RowStatus { get; set; }
        public string LineId { get; set; }
        public string PartNumber { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<double> Price { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
    public class PostBillOfMaterialsModel
    {
        public Tbl_MST_PartBillOfMaterials PartBillOfMaterials { get; set; }
        public List<Vw_MST_BillOfMaterialsDetails> BillOfMaterialsDetails { get; set; }
        public string formAction { get; set; }
    }
    public class ControlPlanningListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }
    public class PostControlPlanningModel
    {
        public Tbl_TRS_ControlPlanning ControlPlanning { get; set; }
        public List<crud_ControlPlanningDetail> ControlPlanningDetail { get; set; }
        public List<crud_ScheduleProduction> ScheduleProduction { get; set; }
        public string ApprovalId { get; set; }
        public string formAction { get; set; }
    }
    public class crud_ControlPlanningDetail
    {
        public string OrderNumber { get; set; }
        public string LineId { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> N1 { get; set; }
        public Nullable<double> N2 { get; set; }
        public Nullable<double> N3 { get; set; }
        public Nullable<double> DailyQty { get; set; }
        public Nullable<double> ShiftQty { get; set; }
        public string SONumber { get; set; }
        public string RowStatus { get; set; }
    }
    public class crud_ScheduleProduction
    {
        public string OrderNumber { get; set; }
        public string LineId { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public string ShiftId { get; set; }
        public System.DateTime ProductionDate { get; set; }
        public Nullable<double> ProductionQty { get; set; }
    }
    public class LineProcessListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
        public DateTime ProductionDate { get; set; }
        public List<Tbl_MST_Line> LineList { get; set; }
        public List<Tbl_MST_LineGate> LineGateList { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }

    }
    public class KanbanScanStatusModel
    {
        public string KanbanStatus { get; set; }
        public string ProductionNumber { get; set; }
        public string ErrMessages { get; set; }
    }
    public class PostKanbanControlModel
    {
        public Tbl_MST_KanbanProduction Kanban { get; set; }
    }
    public class ProductionDailyLatestOutputModel
    {
        //a.OrderNumber, a.KanbanKey, a.LineId,
        //a.CustomerId, a.UniqueNumber, a.PartNumber,
        //a.PartName, a.Qty_OK, a.UnitLevel2, a.SumKanban, a.SumQty, a.SumNG
        public string OrderNumber { get; set; }
        public string KanbanKey { get; set; }
        public string LineId { get; set; }
        public string GateId { get; set; }
        public string CustomerId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public Nullable<double> Qty_OK { get; set; }
        public Nullable<double> Qty_NG { get; set; }
        public string UnitLevel2 { get; set; }
        public int SumKanban { get; set; }
        public double SumQty { get; set; }
        public double SumNG { get; set; }
        public string QrcodePath { get; set; }
        public string PI_Images { get; set; }

    }
}