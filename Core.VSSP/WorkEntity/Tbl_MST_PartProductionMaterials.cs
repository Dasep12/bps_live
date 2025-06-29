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
    
    public partial class Tbl_MST_PartProductionMaterials
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_MST_PartProductionMaterials()
        {
            this.Tbl_MST_PartProductionMaterialsPrice = new HashSet<Tbl_MST_PartProductionMaterialsPrice>();
        }
    
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
        public Nullable<double> UnitQty { get; set; }
        public Nullable<double> SafetyHours { get; set; }
        public Nullable<bool> SubProcess { get; set; }
        public Nullable<bool> Actived { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        public virtual Tbl_MST_MeasurementsCategories Tbl_MST_MeasurementsCategories { get; set; }
        public virtual Tbl_MST_MeasurementsPacking Tbl_MST_MeasurementsPacking { get; set; }
        public virtual Tbl_MST_MeasurementsUnits Tbl_MST_MeasurementsUnits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_MST_PartProductionMaterialsPrice> Tbl_MST_PartProductionMaterialsPrice { get; set; }
        public virtual Tbl_MST_Line Tbl_MST_Line { get; set; }
    }
}
