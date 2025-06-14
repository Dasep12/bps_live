using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.WorkEntity;

namespace Core.VSSP.Models
{
    public class PostBankModel
    {
        public Tbl_MST_Bank Bank { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }
    }
    public class PostClassificationModel
    {
        public Tbl_MST_AccountingClassification Classification { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }
    }
    public class PostPaymentModel
    {
        public Tbl_MST_AccountingPayment Payment { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }
    }
    public class PostCategoryModel
    {
        public Tbl_MST_AccountingCategory Category { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }
    }
    public class PostBankAccountModel
    {
        public Tbl_MST_AccountingBankAccount BankAccount { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }
    }
    public class PostInvoiceRecapModel
    {
        public Tbl_ACC_InvoiceRecap InvoiceRecap { get; set; }
        public string ApprovalId { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public class PostRecapDeliveryModel
    {
        public List<Vw_ACC_CustomerInvoiceDeliveryRecap> RecapDelivery { get; set; }
        public string InvoiceNumber { get; set; }

    }
    public class PostCustomerInvoiceModel
    {
        public Tbl_ACC_CustomerInvoice CustomerInvoice { get; set; }
        public string ApprovalId { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }

    }
    public class PostCustomerInvoiceReceivableModel
    {
        public Tbl_ACC_CustomerInvoiceReceivable CustomerInvoiceReceivable { get; set; }
        public string uid { get; set; }
        public string formAction { get; set; }
    }
    public class CustomerInvoiceListModel
    {
        public List<ExportListModel> ExportList { get; set; }
        public List<Tbl_TRS_Status> StatusList { get; set; }
    }


    public class GetInvoiceModel
    {
        public string CustomerId { get; set; }
        public string DONumber { get; set; }
        public Nullable<System.DateTime> DODate { get; set; }
        public string SONumber { get; set; }
        public string PONumber { get; set; }
        public Nullable<System.DateTime> PODate { get; set; }
        public string RefNumber { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public Nullable<double> DeliveryUnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
    }
}