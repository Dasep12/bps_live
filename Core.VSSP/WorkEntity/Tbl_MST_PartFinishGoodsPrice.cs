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
    
    public partial class Tbl_MST_PartFinishGoodsPrice
    {
        public string CustomerId { get; set; }
        public string PartNumber { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<double> Price { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        public virtual Tbl_MST_PartFinishGoods Tbl_MST_PartFinishGoods { get; set; }
    }
}
