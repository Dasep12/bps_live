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
    
    public partial class Vw_TRS_SalesOrderAchievements
    {
        public string DeliveryYear { get; set; }
        public string DeliveryMonth { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UniqueNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string CustomerUnitModel { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public double DeliveryQty { get; set; }
        public Nullable<double> BalanceQty { get; set; }
        public Nullable<double> BalancePercent { get; set; }
    }
}
