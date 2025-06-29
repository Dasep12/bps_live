//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Core.VSSP.WorkEntity
{
    using System;
    using System.Collections.Generic;
    
    public partial class Vw_ACC_InvoiceRecapDetailTemp
    {
        public string RecapId { get; set; }
        public string RecapNumber { get; set; }
        public string ReceiveNumber { get; set; }
        public string OrderNumber { get; set; }
        public Nullable<System.DateTime> ReceiveDate { get; set; }
        public string SupplierId { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartModel { get; set; }
        public string ClassificationId { get; set; }
        public string PaymentId { get; set; }
        public string CategoryId { get; set; }
        public Nullable<double> RecapQty { get; set; }
        public Nullable<double> PriceQty { get; set; }
        public Nullable<double> TotalPrice { get; set; }
        public Nullable<double> PPN { get; set; }
        public Nullable<double> PPH23 { get; set; }
        public Nullable<double> DebitNote { get; set; }
        public Nullable<double> Payment { get; set; }
        public Nullable<bool> Selected { get; set; }
    }
}
