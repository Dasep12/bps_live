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
    
    public partial class Tbl_MST_PartIdentification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_MST_PartIdentification()
        {
            this.Tbl_QC_Inspection = new HashSet<Tbl_QC_Inspection>();
        }
    
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public string CustomerUniqueNumber { get; set; }
        public string PartCategory { get; set; }
        public string PartIdentity { get; set; }
        public string DocumentNumber { get; set; }
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public string Revision { get; set; }
        public string ECINumber { get; set; }
        public Nullable<double> CycleTime { get; set; }
        public string PI_Images { get; set; }
        public string Drawing_Images { get; set; }
        public Nullable<bool> Actived { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_QC_Inspection> Tbl_QC_Inspection { get; set; }
    }
}
