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
    
    public partial class Vw_TRS_SupplierOrder
    {
        public string OrderNumber { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SSP { get; set; }
        public string ProcessName { get; set; }
        public Nullable<bool> SSPStock { get; set; }
        public Nullable<bool> DeliveryOrder { get; set; }
        public string KanbanCycle { get; set; }
        public Nullable<System.DateTime> IncomingDate { get; set; }
        public Nullable<System.DateTime> IncomingTime { get; set; }
        public Nullable<int> Shift { get; set; }
        public Nullable<int> TotalPart { get; set; }
        public Nullable<double> TotalOrder { get; set; }
        public Nullable<int> Status { get; set; }
        public string StatusName { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    }
}
