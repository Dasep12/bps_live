using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Models
{

    public class CMSListModel
    {
        public string No { get; set; }
        public string Tahun { get; set; }
        public string Bulan { get; set; }
        public string NamaBulan { get; set; }
        public string ID { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string IsContent { get; set; }
        public byte[] Image { get; set; }
        public bool Publish { get; set; }
        public string CreateDate { get; set; }
        public string PublishDate { get; set; }
        public string Editor { get; set; }
        public double Readed { get; set; }
        public double Liked { get; set; }
    }

    public class CMSCrudModel
    {
        [Display(Name = "CMS ID")]
        public string ID { get; set; }
        [Display(Name = "Category")]
        public string Category { get; set; }
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Display(Name = "Sub Title")]
        public string SubTitle { get; set; }
        [Display(Name = "Content")]
        public string IsContent { get; set; }
        [Display(Name = "Image")]
        public byte[] Image { get; set; }
        [Display(Name = "Publish")]
        public bool Publish { get; set; }
        [Display(Name = "Create Date")]
        public string CreateDate { get; set; }
        [Display(Name = "Publish Date")]
        public string PublishedDate { get; set; }
        [Display(Name = "Editor")]
        public string Editor { get; set; }
        public IEnumerable<CMSCategoryModel> CMSCategoryList { get; set; }

    }
    public class CMSCategoryModel
    {
        public string ID { get; set; }
        public string Name { get; set; }

    }

    public class NEWSListModel
    {
        public string No { get; set; }
        public string Tahun { get; set; }
        public string Bulan { get; set; }
        public string NamaBulan { get; set; }
        public string ID { get; set; }
        public string Category { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string IsContent { get; set; }
        public byte[] Image { get; set; }
        public bool Publish { get; set; }
        public string CreateDate { get; set; }
        public string PublishDate { get; set; }
        public string Editor { get; set; }
        public double Readed { get; set; }
        public double Liked { get; set; }
    }

    public class NEWSCrudModel
    {
        [Display(Name = "NEWS ID")]
        public string ID { get; set; }
        [Display(Name = "Category")]
        public string Category { get; set; }
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Display(Name = "Content")]
        public string IsContent { get; set; }
        [Display(Name = "Image")]
        public byte[] Image { get; set; }
        [Display(Name = "Publish")]
        public bool Publish { get; set; }
        [Display(Name = "Create Date")]
        public string CreateDate { get; set; }
        [Display(Name = "Publish Date")]
        public string PublishedDate { get; set; }
        [Display(Name = "Editor")]
        public string Editor { get; set; }
        public IEnumerable<NEWSCategoryModel> NEWSCategoryList { get; set; }

    }
    public class NEWSCategoryModel
    {
        public string ID { get; set; }
        public string Name { get; set; }

    }
    public class MessagesModel
    {
        [Display(Name = "No.")]
        public int Nom { get; set; }
        [Display(Name = "ID #")]
        public int ID { get; set; }
        [Display(Name = "Sender")]
        public string UserName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Subject")]
        public string Subject { get; set; }
        [Display(Name = "Messages")]
        public string Messages { get; set; }
        [Display(Name = "Readed")]
        public double Readed { get; set; }
        [Display(Name = "Create Date")]
        public string CreateDate { get; set; }
    }
    public class StockListModel
    {
        public string StockType {get; set;}
        public string CustomerId {get; set;}
        public string CustomerName {get; set;}
        public string LineId {get; set;}
        public string LineName {get; set;}
        public string SupplierId {get; set;}
        public string SupplierName {get; set;}
        public string UniqueNumber {get; set;}
        public string PartNumber {get; set;}
        public string PartName {get; set;}
        public string PartModel {get; set;}
        public string UnitLevel2 {get; set;}
        public double MinStock {get; set;}
        public double MaxStock {get; set;}
        public double StockKanban {get; set;}
        public double StockQty { get; set; }
    }

}