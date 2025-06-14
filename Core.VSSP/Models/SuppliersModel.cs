using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class ImportSuppliersListModel
    {
        public List<ImportSupplierModel> ImportSupplier { get; set; }
        public List<ImportSupplierContactModel> ImportSupplierContact { get; set; }
        public List<ImportKanbanCycleModel> ImportKanbanCycle { get; set; }
        public List<ImportSupplierCostCenterModel> ImportSupplierCostCenter { get; set; }
    }
    public class ImportSupplierModel
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Provience { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Websites { get; set; }
        public string TaxId { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportSupplierContactModel
    {
        public string SupplierId { get; set; }
        public string ContactName { get; set; }
        public string Organization { get; set; }
        public string Position { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public bool ReceiveOrder { get; set; }
        public bool ReceiveInvoice { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportSupplierCostCenterModel
    {
        public string SupplierId { get; set; }
        public string CostId { get; set; }
        public string CostName { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }

    }
    public class PostSupplierModel
    {
        public Tbl_MST_Supplier Supplier { get; set; }
        public List<crud_SupplierContact> Suppliercontact { get; set; }
        public List<crud_KanbanCycle> KanbanCycle { get; set; }
        public List<crud_SupplierCostCenter> SupplierCostCenter { get; set; }
        public List<crud_SupplierBankAccount> SupplierBankAccount { get; set; }
        public string formAction { get; set; }
    }

    public partial class crud_SupplierContact
    {
        public string RowStatus { get; set; }
        public string ContactId { get; set; }
        public string SupplierId { get; set; }
        public string ContactName { get; set; }
        public string Organization { get; set; }
        public string Position { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public Nullable<bool> ReceiveOrder { get; set; }
        public Nullable<bool> ReceiveInvoice { get; set; }

    }
    public partial class crud_SupplierCostCenter
    {
        public string RowStatus { get; set; }
        public string SupplierId { get; set; }
        public string CostId { get; set; }
        public string CostName { get; set; }

    }
    public partial class crud_SupplierBankAccount
    {
        public string RowStatus { get; set; }
        public string SupplierId { get; set; }
        public string BankId { get; set; }
        public string BankAddress { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string TransactionType { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
    }
    public class ImportRawMaterialsListModel
    {
        public List<ImportRawMaterialModel> ImportRawMaterial { get; set; }
        public List<ImportRawMaterialPriceModel> ImportRawMaterialPrice { get; set; }
        public List<ImportRawMaterialCostCenterModel> ImportRawMaterialCostCenter { get; set; }

    }
    public class ImportRawMaterialModel
    {
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberSupplier { get; set; }
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
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportRawMaterialPriceModel
    {
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public double Price { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportRawMaterialCostCenterModel
    {
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public string CostId { get; set; }
        public string ClassificationId { get; set; }
        public string PaymentId { get; set; }
        public string CategoryId { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportKanbanCycleModel
    {
        public string SupplierId { get; set; }
        public double Cycle1 { get; set; }
        public double Cycle2 { get; set; }
        public double Cycle3 { get; set; }
        public string CycleTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class PostRawMaterialModel
    {
        public Tbl_MST_PartRawMaterials RawMaterial { get; set; }
        public List<crud_PartRawMaterialsCostCenter> RawMaterialCostCenter { get; set; }
        public List<crud_PartRawMaterialsPrice> RawMaterialPrice { get; set; }
        public string formAction { get; set; }
    }
    public partial class crud_PartRawMaterialsCostCenter
    {
        public string RowStatus { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string CostId { get; set; }
        public string ClassificationId { get; set; }
        public string PaymentId { get; set; }
        public string CategoryId { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
    public partial class crud_PartRawMaterialsPrice
    {
        public string RowStatus { get; set; }
        public string SupplierId { get; set; }
        public string PartNumber { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<double> Price { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
    public partial class crud_KanbanCycle
    {
        public string RowStatus { get; set; }
        public string SupplierId { get; set; }
        public Nullable<double> Cycle1 { get; set; }
        public Nullable<double> Cycle2 { get; set; }
        public Nullable<double> Cycle3 { get; set; }
        public string CycleTime { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }

}