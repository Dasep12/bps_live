using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Core.VSSP.Controllers
{
    public class QRCodeGeneratorController : Controller
    {
        // GET: QRCodeGenerator
        SystemService systemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();


        public ActionResult index()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult Generate(QRCodeModel qrcode)
        //{
        //    try
        //    {
        //        qrcode.QRCodeImagePath = GenerateQRCode(qrcode.QRCodeText);
        //        ViewBag.Message = "QR Code Created successfully";
        //    }
        //    catch (Exception ex)
        //    {
        //        //catch exception if there is any
        //    }
        //    return View("Index", qrcode);
        //}

        public bool GenerateQRCode(string docid, string qrcodeText, string barcodeText, string basePath, string userid, string formAction)
        {

            try
            {
                string folderPath = "";
                string qrcodePath = "";
                string barcodePath = "";
                string hostingqrcode = "";
                string hostingbarcode = "";

                var barcodeWriter = new BarcodeWriter();

                if (systemService.Vf(qrcodeText) != "")
                {

                    folderPath = System.Web.Hosting.HostingEnvironment.MapPath("~/_VSSPAssets/Images/apps/" + basePath + "/Qrcode/");
                    qrcodePath = folderPath + docid.Replace('/', '.') + ".jpg";
                    hostingqrcode = "~/_VSSPAssets/Images/apps/" + basePath + "/Qrcode/" + docid.Replace('/', '.') + ".jpg";

                    // If the directory doesn't exist then create it.
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    barcodeWriter.Format = BarcodeFormat.QR_CODE;
                    barcodeWriter.Options = new EncodingOptions
                    {
                        Width = 200,
                        Height = 200
                    };

                    var result = barcodeWriter.Write(qrcodeText);
                    var barcodeBitmap = new Bitmap(result);
                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(qrcodePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                            byte[] bytes = memory.ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                }

                if (systemService.Vf(barcodeText) != "")
                {

                    folderPath = System.Web.Hosting.HostingEnvironment.MapPath("~/_VSSPAssets/Images/apps/" + basePath + "/Barcode/");
                    barcodePath = folderPath + docid.Replace('/', '.') + ".jpg";
                    hostingbarcode = "~/_VSSPAssets/Images/apps/" + basePath + "/Barcode/" + docid.Replace('/', '.') + ".jpg";

                    // If the directory doesn't exist then create it.
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    barcodeWriter.Format = BarcodeFormat.CODE_128;
                    barcodeWriter.Options = new EncodingOptions
                    {
                        Width = 100,
                        Height = 80
                    };
                    var result = barcodeWriter.Write(barcodeText);
                    var barcodeBitmap = new Bitmap(result);
                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                            byte[] bytes = memory.ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                }


                var ListUpdate = vssp_db.Tbl_TRS_QrCodePath.Where(a => a.DocId == docid).FirstOrDefault();
                if (ListUpdate != null){
                            
                    ListUpdate.QrcodePath = hostingqrcode;
                    ListUpdate.BarcodePath = hostingbarcode;
                    ListUpdate.UserId = userid;
                    ListUpdate.EditDate = DateTime.Now;

                } else
                {
                    Tbl_TRS_QrCodePath ListCreate = new Tbl_TRS_QrCodePath();
                    ListCreate.DocId = docid;
                    ListCreate.QrcodePath = hostingqrcode;
                    ListCreate.BarcodePath = hostingbarcode;
                    ListCreate.UserId = userid;
                    ListCreate.EditDate = DateTime.Now;

                    vssp_db.Tbl_TRS_QrCodePath.Add(ListCreate);

                }

                try
                {
                    vssp_db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return false;
                }
                return true;

            } catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return false;
            }
        }

        public void generateOrderQr(string ordernumber,string orderType)
        {

            var qrcodepath = vssp_db.Tbl_TRS_QrCodePath.Where(a => a.DocId == ordernumber).FirstOrDefault();

            if (qrcodepath == null)
            {
                var DNQrcode = GenerateQRCode(ordernumber, ordernumber, "", orderType, "report", "create");
            }

        }
        public void generateKanbanCardQrcode(string ordernumber)
        {
            var KanbanOrderDetail = (from a in vssp_db.Tbl_TRS_SupplierOrderKanban
                                     join b in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                     from b in part.DefaultIfEmpty()
                                     join c in vssp_db.Tbl_TRS_SupplierOrderDetail on new { a.OrderNumber, a.SupplierId, a.PartNumber } equals new { c.OrderNumber, c.SupplierId, c.PartNumber } into partorder
                                     from c in partorder.DefaultIfEmpty()
                                     join d in vssp_db.Tbl_TRS_QrCodePath on a.KanbanKey equals d.DocId into qrcode
                                     from d in qrcode.DefaultIfEmpty()
                                     where a.OrderNumber == ordernumber
                                     select new
                                     {
                                         a.KanbanKey,
                                         a.KanbanRun,
                                         c.OrderQty,
                                         a.OrderNumber,
                                         a.ReceiveNumber,
                                         a.SupplierId,
                                         b.UniqueNumber,
                                         a.PartNumber,
                                         b.PartName,
                                         b.PartModel,
                                         b.CategoryId,
                                         b.PackingId,
                                         b.AreaId,
                                         b.LocationId,
                                         a.CostId,
                                         b.UnitQty,
                                         b.UnitLevel2,
                                         a.KanbanCycle,
                                         a.IncomingDate,
                                         a.IncomingTime,
                                         a.Received,
                                         d.QrcodePath,
                                         d.BarcodePath
                                     }).ToList();

            foreach (var kanban in KanbanOrderDetail)
            {
                string qrcodetext = kanban.KanbanKey + ";" + kanban.OrderNumber + ";" + kanban.SupplierId + ";" + kanban.UniqueNumber + ";" + kanban.PartNumber + ";" + kanban.PartName + ";" +
                                    systemService.Vn(kanban.UnitQty.ToString()) + ";" + kanban.UnitLevel2 + ";" + kanban.KanbanRun + ";" + kanban.OrderQty + ";" + kanban.KanbanCycle + ";" +
                                    systemService.Vd(kanban.IncomingDate.ToString()) + ";" + systemService.Vd(kanban.IncomingTime.ToString(), "HH:mm");

                var KanbanQrcode = GenerateQRCode(kanban.KanbanKey, qrcodetext, kanban.KanbanKey, "SupplierOrder", "report", "create");
            }
        }

        public void generateKanbanProductionQrcode(string CustomerId, string PartNumber, string KanbanKey)
        {
            var KanbanCard = (from a in vssp_db.Vw_MST_KanbanProductionList
                              join b in vssp_db.Tbl_TRS_QrCodePath on a.KanbanKey equals b.DocId into qrcode
                              from b in qrcode.DefaultIfEmpty()
                              where a.Actived == true && a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.KanbanKey.Contains(KanbanKey)
                              select new { a.KanbanKey, a.KanbanCode, a.LineId, a.LineName, a.CustomerId, a.CustomerName, a.UniqueNumber, a.PartNumber, a.PartName, a.UnitQty, a.UnitLevel2, a.PackingId, a.KanbanRun, a.Actived, b.QrcodePath }).ToList();


            foreach (var kanban in KanbanCard)
            {
                string qrcodetext = kanban.KanbanKey + ";" + kanban.LineId + ";" + kanban.CustomerId + ";" + kanban.UniqueNumber + ";" + kanban.PartNumber + ";" + kanban.PartName + ";" +
                                    systemService.Vn(kanban.UnitQty.ToString()) + ";" + kanban.UnitLevel2 + ";" + kanban.KanbanRun;

                var KanbanQrcode = GenerateQRCode(kanban.KanbanKey, qrcodetext, kanban.KanbanKey, "KanbanProduction", "report", "create");
            }
        }
        public void generateLabelProductionQrcode(string ProductionNumber)
        {
            var KanbanCard = (from a in vssp_db.Vw_TRS_ProductionLineProcessList
                              where a.ProductionNumber == ProductionNumber
                              select a).ToList();


            foreach (var kanban in KanbanCard)
            {
                string qrcodetext = kanban.CustomerId + ";" + kanban.PartNumber + ";" + kanban.UniqueNumber;
                var KanbanQrcode = GenerateQRCode(kanban.ProductionNumber, qrcodetext, "", "LabelProduction", "report", "create");
            }
        }
        public void cleanOrderQr(string ordernumber)
        {

            var qrcodepath = vssp_db.Tbl_TRS_QrCodePath.Where(a => a.DocId == ordernumber).FirstOrDefault();

            if (qrcodepath != null)
            {
                string path = System.Web.Hosting.HostingEnvironment.MapPath(qrcodepath.QrcodePath);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                vssp_db.Tbl_TRS_QrCodePath.Remove(qrcodepath);
            }

            try
            {
                vssp_db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
            }

        }
        public void cleanKanbanCardQrcode(string ordernumber)
        {
            var KanbanOrderDetail = (from a in vssp_db.Tbl_TRS_SupplierOrderKanban
                                     join b in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                     from b in part.DefaultIfEmpty()
                                     join c in vssp_db.Tbl_TRS_SupplierOrderDetail on new { a.OrderNumber, a.SupplierId, a.PartNumber } equals new { c.OrderNumber, c.SupplierId, c.PartNumber } into partorder
                                     from c in partorder.DefaultIfEmpty()
                                     join d in vssp_db.Tbl_TRS_QrCodePath on a.KanbanKey equals d.DocId into qrcode
                                     from d in qrcode.DefaultIfEmpty()
                                     where a.OrderNumber == ordernumber
                                     select new
                                     {
                                         a.KanbanKey,
                                         a.KanbanRun,
                                         c.OrderQty,
                                         a.OrderNumber,
                                         a.ReceiveNumber,
                                         a.SupplierId,
                                         b.UniqueNumber,
                                         a.PartNumber,
                                         b.PartName,
                                         b.PartModel,
                                         b.CategoryId,
                                         b.PackingId,
                                         b.AreaId,
                                         b.LocationId,
                                         a.CostId,
                                         b.UnitQty,
                                         b.UnitLevel2,
                                         a.KanbanCycle,
                                         a.IncomingDate,
                                         a.IncomingTime,
                                         a.Received,
                                         d.QrcodePath,
                                         d.BarcodePath
                                     }).ToList();

            foreach (var kanban in KanbanOrderDetail)
            {
                var qrcodepath = vssp_db.Tbl_TRS_QrCodePath.Where(a => a.DocId == kanban.KanbanKey).FirstOrDefault();
                string path = "";

                if (qrcodepath != null)
                {
                    //delete qrcode
                    path = System.Web.Hosting.HostingEnvironment.MapPath(qrcodepath.QrcodePath);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    //delete barcode
                    path = System.Web.Hosting.HostingEnvironment.MapPath(qrcodepath.BarcodePath);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    vssp_db.Tbl_TRS_QrCodePath.Remove(qrcodepath);

                }
            }

            try
            {
                vssp_db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
            }

        }
        public void cleanKanbanProductionQrcode(string CustomerId, string PartNumber, string KanbanKey)
        {
            var KanbanCard = (from a in vssp_db.Vw_MST_KanbanProductionList
                              join b in vssp_db.Tbl_TRS_QrCodePath on a.KanbanKey equals b.DocId into qrcode
                              from b in qrcode.DefaultIfEmpty()
                              where a.Actived == true && a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.KanbanKey.Contains(KanbanKey)
                              select new { a.KanbanKey, a.KanbanCode, a.LineId, a.LineName, a.CustomerId, a.CustomerName, a.UniqueNumber, a.PartNumber, a.PartName, a.UnitQty, a.UnitLevel2, a.PackingId, a.KanbanRun, a.Actived, b.QrcodePath }).ToList();


            foreach (var kanban in KanbanCard)
            {
                var qrcodepath = vssp_db.Tbl_TRS_QrCodePath.Where(a => a.DocId == kanban.KanbanKey).FirstOrDefault();
                string path = "";

                if (qrcodepath != null)
                {
                    //delete qrcode
                    path = System.Web.Hosting.HostingEnvironment.MapPath(qrcodepath.QrcodePath);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    //delete barcode
                    path = System.Web.Hosting.HostingEnvironment.MapPath(qrcodepath.BarcodePath);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    vssp_db.Tbl_TRS_QrCodePath.Remove(qrcodepath);

                }
            }

            try
            {
                vssp_db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
            }

        }
    }
}