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
    
    public partial class Vw_MST_KanbanProductionList
    {
        public string KanbanKey { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string KanbanCode { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string CustomerUnitModel { get; set; }
        public Nullable<double> UnitQty { get; set; }
        public string UnitLevel2 { get; set; }
        public string PackingId { get; set; }
        public string LocationId { get; set; }
        public Nullable<int> KanbanRun { get; set; }
        public Nullable<bool> ScanIn { get; set; }
        public Nullable<bool> ScanOut { get; set; }
        public Nullable<bool> Storage { get; set; }
        public Nullable<bool> Actived { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string UnitLevel1 { get; set; }
    }
}
