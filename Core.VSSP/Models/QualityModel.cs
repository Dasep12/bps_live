using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{
    public class postPartIdentification
    {
        public Tbl_MST_PartIdentification PartIdentification { get; set; }
        public string formAction { get; set; }
    }
    public class postOutgoingInspection
    {
        public Tbl_QC_Inspection Inspection { get; set; }
        public List<Tbl_QC_InspectionDefects> InspectionDefects { get; set; }
        public string formAction { get; set; }
        public string compid { get; set; }
    }
    public class postPartIdentificationSupplier
    {
        public Tbl_MST_PartIdentificationSupplier PartIdentificationSupplier { get; set; }
        public string formAction { get; set; }
    }
    public class postIncomingInspection
    {
        public Tbl_QC_InspectionIncoming InspectionIncoming { get; set; }
        public List<Tbl_QC_InspectionIncomingDefects> InspectionIncomingDefects { get; set; }
        public string formAction { get; set; }
        public string compid { get; set; }
    }
}