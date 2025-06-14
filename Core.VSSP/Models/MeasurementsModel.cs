using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class ImportCategoriesModel
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Remarks { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportUnitsModel
    {
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string ParentId { get; set; }
        public double UnitLevel { get; set; }
        public string Remarks { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }
    public class ImportPackingModel
    {
        public string PackingId { get; set; }
        public string PackingName { get; set; }
        public string Remarks { get; set; }
        public bool Status { get; set; }
        public string Result { get; set; }
    }

}