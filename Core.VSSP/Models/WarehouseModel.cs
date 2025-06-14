using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class ImportAreaModel
    {
        public string AreaID { get; set; }
        public string AreaName { get; set; }
        public string Remarks { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportLocationModel
    {
        public string AreaId { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string Remarks { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
}