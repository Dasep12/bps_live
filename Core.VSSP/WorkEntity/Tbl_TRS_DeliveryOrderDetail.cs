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
    
    public partial class Tbl_TRS_DeliveryOrderDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_TRS_DeliveryOrderDetail()
        {
            this.Tbl_TRS_DeliveryOrderKanban = new HashSet<Tbl_TRS_DeliveryOrderKanban>();
            this.Tbl_ACC_CustomerInvoiceDetail = new HashSet<Tbl_ACC_CustomerInvoiceDetail>();
        }
    
        public string DONumber { get; set; }
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public Nullable<double> DeliveryQty { get; set; }
        public Nullable<double> DeliveryUnitQty { get; set; }
    
        public virtual Tbl_MST_PartFinishGoods Tbl_MST_PartFinishGoods { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_TRS_DeliveryOrderKanban> Tbl_TRS_DeliveryOrderKanban { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_ACC_CustomerInvoiceDetail> Tbl_ACC_CustomerInvoiceDetail { get; set; }
        public virtual Tbl_TRS_DeliveryOrder Tbl_TRS_DeliveryOrder { get; set; }
    }
}
