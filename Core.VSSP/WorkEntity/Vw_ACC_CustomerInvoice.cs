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
    
    public partial class Vw_ACC_CustomerInvoice
    {
        public string InvoiceNumber { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string InvoiceYear { get; set; }
        public string InvoiceMonth { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Nullable<System.DateTime> DOStart { get; set; }
        public Nullable<System.DateTime> DOEnd { get; set; }
        public Nullable<double> SubTotal { get; set; }
        public Nullable<double> PPN { get; set; }
        public Nullable<double> PPH23 { get; set; }
        public Nullable<double> GrandTotal { get; set; }
        public Nullable<double> PPNPercent { get; set; }
        public Nullable<int> Status { get; set; }
        public string StatusName { get; set; }
        public string Approval { get; set; }
        public Nullable<int> ApprovalLevel { get; set; }
        public string ApprovalName { get; set; }
        public string Remarks { get; set; }
        public string Terms { get; set; }
        public Nullable<bool> IncludePPH23 { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
}
