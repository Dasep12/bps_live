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
    
    public partial class Tbl_TRS_SupplierOrder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_TRS_SupplierOrder()
        {
            this.Tbl_TRS_SupplierOrderDetail = new HashSet<Tbl_TRS_SupplierOrderDetail>();
            this.Tbl_TRS_ReceivingOrder = new HashSet<Tbl_TRS_ReceivingOrder>();
        }
    
        public string OrderNumber { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public string SupplierId { get; set; }
        public Nullable<System.DateTime> IncomingDate { get; set; }
        public Nullable<System.DateTime> IncomingTime { get; set; }
        public Nullable<int> Shift { get; set; }
        public string SSP { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> Status { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_TRS_SupplierOrderDetail> Tbl_TRS_SupplierOrderDetail { get; set; }
        public virtual Tbl_MST_SpecialSupplyPart Tbl_MST_SpecialSupplyPart { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_TRS_ReceivingOrder> Tbl_TRS_ReceivingOrder { get; set; }
    }
}
