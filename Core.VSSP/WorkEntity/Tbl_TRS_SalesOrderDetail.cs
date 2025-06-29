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
    
    public partial class Tbl_TRS_SalesOrderDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_TRS_SalesOrderDetail()
        {
            this.Tbl_TRS_ScheduleDelivery = new HashSet<Tbl_TRS_ScheduleDelivery>();
            this.Tbl_ACC_CustomerInvoiceDetail = new HashSet<Tbl_ACC_CustomerInvoiceDetail>();
        }
    
        public string SONumber { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> OrderN1 { get; set; }
        public Nullable<double> OrderN2 { get; set; }
        public Nullable<double> OrderN3 { get; set; }
        public Nullable<double> DeliveryPerDay { get; set; }
    
        public virtual Tbl_TRS_SalesOrder Tbl_TRS_SalesOrder { get; set; }
        public virtual Tbl_MST_PartFinishGoods Tbl_MST_PartFinishGoods { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_TRS_ScheduleDelivery> Tbl_TRS_ScheduleDelivery { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_ACC_CustomerInvoiceDetail> Tbl_ACC_CustomerInvoiceDetail { get; set; }
    }
}
