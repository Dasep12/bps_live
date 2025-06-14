using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class ImportCustomersListModel
    {
        public List<ImportCustomerModel> ImportCustomer { get; set; }
        public List<ImportCustomerContactModel> ImportCustomerContact { get; set; }
    }
    public class ImportCustomerModel
    {
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string AccountCode { get; set; }
        public string CustomerName { get; set; }
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
    public class ImportCustomerContactModel
    {
        public string CustomerId { get; set; }
        public string ContactName { get; set; }
        public string Organization { get; set; }
        public string Position { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public bool ReceiveOrder { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }

    public class PostCustomerModel
    {
        public Tbl_MST_Customer customer { get; set; }
        public List<crud_CustomerContact> customercontact { get; set; }
        public string formAction { get; set; }
    }

    public partial class crud_CustomerContact
    {
        public string RowStatus { get; set; }
        public string ContactId { get; set; }
        public string CustomerId { get; set; }
        public string ContactName { get; set; }
        public string Organization { get; set; }
        public string Position { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public Nullable<bool> ReceiveOrder { get; set; }

    }
    public class ImportFinishGoodsListModel
    {
        public List<ImportFinishGoodModel> ImportFinishGood { get; set; }
        public List<ImportFinishGoodPriceModel> ImportFinishGoodPrice { get; set; }
        public List<ImportFinishGoodRelationModel> ImportFinishGoodRelation { get; set; }
    }
    public class ImportFinishGoodModel
    {
        public string CustomerId { get; set; }
        public string CustomerUnitModel { get; set; }
        public string PartNumber { get; set; }
        public string PartNumberCustomer { get; set; }
        public string UniqueNumber { get; set; }
        public string PartName { get; set; }
        public string CategoryId { get; set; }
        public string PackingId { get; set; }
        public string AreaId { get; set; }
        public string LocationId { get; set; }
        public string UnitLevel1 { get; set; }
        public string UnitLevel2 { get; set; }
        public double UnitQty { get; set; }
        public bool PassThrough { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportFinishGoodPriceModel
    {
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public double Price { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportFinishGoodRelationModel
    {
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public string SupplierId { get; set; }
        public string PartNumberRawMaterial { get; set; }
        public double QtyUsage { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }

        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class PostFinishGoodModel
    {
        public Tbl_MST_PartFinishGoods FinishGood { get; set; }
        public List<crud_PartFinishGoodsPrice> FinishGoodPrice { get; set; }
        public List<crud_PartFinishGoodsRelation> FinishGoodRelation { get; set; }
        public string formAction { get; set; }
    }
    public partial class crud_PartFinishGoodsPrice
    {
        public string RowStatus { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<double> Price { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
    public partial class crud_PartFinishGoodsRelation
    {
        public string RowStatus { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public string SupplierId { get; set; }
        public string PartNumberRawMaterial { get; set; }
        public Nullable<double> QtyUsage { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
    public class PostKanbanSettingModel
    {
        public Tbl_MST_KanbanSetting kanbansetting { get; set; }
        public List<Tbl_MST_KanbanSettingSequence> kanbansettingsequence { get; set; }
        public string formAction { get; set; }
    }
}