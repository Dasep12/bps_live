using Core.VSSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using System.Data.Entity.Validation;
using Core.VSSP.WorkEntity;

namespace Core.VSSP.Services
{
    public class DeliveryService
    {
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();

        // GET: Deliveryervice
        public ImportDeliveryListModel uploadOrderExcel(HttpPostedFileBase files)
        {
            ImportDeliveryListModel _ImportListModel = new ImportDeliveryListModel();
            _ImportListModel.ImportDeliveryOrder = new List<ImportDeliveryOrderModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //Order
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var Order = vssp_db.Tbl_TRS_DeliveryOrderImport;

                    bool validTemplate = true;

                    //header validator Order
                    var DONumber        = _SystemService.Vf(workSheet?.Cells[5, 2]?.Value?.ToString());
                    var DODate          = _SystemService.Vf(workSheet?.Cells[5, 3]?.Value?.ToString());
                    var CustomerId      = _SystemService.Vf(workSheet?.Cells[5, 4]?.Value?.ToString());
                    var RefNumber       = _SystemService.Vf(workSheet?.Cells[5, 5]?.Value?.ToString());
                    var PartNumber      = _SystemService.Vf(workSheet?.Cells[5, 6]?.Value?.ToString());
                    var UniqueNumber    = _SystemService.Vf(workSheet?.Cells[5, 7]?.Value?.ToString());
                    var Qty             = _SystemService.Vf(workSheet?.Cells[5, 8]?.Value?.ToString());

                    if (DONumber.Replace(" ","").Replace("*","").ToLower() != "deliverynumber")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (DODate.Replace(" ", "").Replace("*", "").ToLower() != "date")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (CustomerId.Replace(" ", "").Replace("*", "").ToLower() != "customerid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (RefNumber.Replace(" ", "").Replace("*", "").ToLower() != "refnumber")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (UniqueNumber.Replace(" ", "").Replace("*", "").ToLower() != "unique")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (Qty.Replace(" ", "").Replace("*", "").ToLower() != "qty")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    stopValidation:

                    //get data excel
                    if (validTemplate == true) {

                        for (int rowIterator = 7; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportDeliveryOrderModel ImportList = new ImportDeliveryOrderModel();

                            ImportList.DONumber     = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            if (workSheet?.Cells[rowIterator, 3]?.Value?.ToString() != null)
                            {
                                ImportList.DODate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 3]?.Value?.ToString(), "yyyy-MM-dd"));
                            }
                            ImportList.CustomerId   = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.RefNumber    = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.PartNumber   = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.UniqueNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.Qty          = _SystemService.Vn(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());

                            var exist = (from a in Order
                                         where a.DONumber == ImportList.DONumber && a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber
                                         select a).ToList();

                            var valid = (from a in vssp_db.Tbl_MST_PartFinishGoods
                                         where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber
                                         select a).ToList();

                            if (exist.Count() == 0 && valid.Count() > 0)
                            {
                                ImportList.Status = true;
                                ImportList.Result = "";
                            }
                            else
                            {
                                if (valid.Count() == 0)
                                {
                                    ImportList.Status = false;
                                    ImportList.Result = "Part Not Register!";
                                }
                                else
                                {
                                    ImportList.Status = false;
                                    ImportList.Result = "Already Import!";
                                }
                            }

                            if (ImportList.DONumber == "")
                            {
                                goto stopUpload;
                            }
                            _ImportListModel.ImportDeliveryOrder.Add(ImportList);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportListModel;

        }
        //public ImportDeliveryListModel crudImportOrderExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        //{
        //    ImportDeliveryListModel _ImportListModel = new ImportDeliveryListModel();
        //    _ImportListModel.ImportDeliveryOrder = new List<ImportDeliveryOrderModel>();

        //    if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
        //    {

        //        string fileName = files.FileName;
        //        string fileContentType = files.ContentType;
        //        byte[] fileBytes = new byte[files.ContentLength];
        //        var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //        using (var package = new ExcelPackage(files.InputStream))
        //        {
        //            //Order
        //            var workSheet   = package.Workbook.Worksheets[0];
        //            var noOfCol     = workSheet.Dimension.End.Column;
        //            var noOfRow     = workSheet.Dimension.End.Row;
        //            var Order       = vssp_db.Tbl_TRS_DeliveryOrderImport;

        //            for (int rowIterator = 7; rowIterator <= noOfRow; rowIterator++)
        //            {

        //                ImportDeliveryOrderModel ImportList = new ImportDeliveryOrderModel();


        //                ImportList.DONumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
        //                ImportList.DODate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 3]?.Value?.ToString(), "yyyy-MM-dd"));
        //                ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
        //                ImportList.RefNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
        //                ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
        //                ImportList.UniqueNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
        //                ImportList.Qty = _SystemService.Vn(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());

        //                if (ImportList.DONumber == "")
        //                {
        //                    goto stopUpload;
        //                }

        //                var exist = from a in Order
        //                            where a.DONumber == ImportList.DONumber && a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber
        //                            select a;

        //                if (exist.Count() == 0)
        //                {

        //                    Tbl_TRS_DeliveryOrderImport ListCreate = new Tbl_TRS_DeliveryOrderImport();
        //                    ListCreate.DONumber     = ImportList.DONumber;
        //                    ListCreate.DODate       = ImportList.DODate;
        //                    ListCreate.CustomerId   = ImportList.CustomerId;
        //                    ListCreate.RefNumber    = ImportList.RefNumber;
        //                    ListCreate.PartNumber   = ImportList.PartNumber;
        //                    ListCreate.UniqueNumber = ImportList.UniqueNumber;
        //                    ListCreate.Qty          = ImportList.Qty;
        //                    ListCreate.UserId       = UserId;
        //                    ListCreate.ImportDate   = DateTime.Now;
        //                    vssp_db.Tbl_TRS_DeliveryOrderImport.Add(ListCreate);

        //                    ImportList.Status = true;
        //                    ImportList.Result = "success imported.";
        //                }
        //                else
        //                {
        //                    if (replace == true)
        //                    {

        //                        var ListUpdate = vssp_db.Tbl_TRS_DeliveryOrderImport.First(a => a.DONumber == ImportList.DONumber && a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber);

        //                        ListUpdate.DONumber     = ImportList.DONumber;
        //                        ListUpdate.DODate       = ImportList.DODate;
        //                        ListUpdate.CustomerId   = ImportList.CustomerId;
        //                        ListUpdate.RefNumber    = ImportList.RefNumber;
        //                        ListUpdate.PartNumber   = ImportList.PartNumber;
        //                        ListUpdate.UniqueNumber = ImportList.UniqueNumber;
        //                        ListUpdate.Qty          = ImportList.Qty;
        //                        ListUpdate.UserId       = UserId;
        //                        ListUpdate.ImportDate   = DateTime.Now;

        //                        ImportList.Status = true;
        //                        ImportList.Result = "replaced existing!";
        //                    }
        //                    else
        //                    {
        //                        ImportList.Status = false;
        //                        ImportList.Result = "skipped import, already exist!";
        //                    }
        //                }

        //                try
        //                {
        //                    vssp_db.SaveChanges();
        //                    vssp_db.SP_CRUD_StockFromDelivery(ImportList.DONumber,ImportList.CustomerId,ImportList.PartNumber);
        //                }
        //                catch (DbEntityValidationException e)
        //                {
        //                    string entityErr = "";
        //                    foreach (var eve in e.EntityValidationErrors)
        //                    {
        //                        foreach (var ve in eve.ValidationErrors)
        //                        {
        //                            entityErr += " <br/>" + ve.ErrorMessage;
        //                        }
        //                    };

        //                    ImportList.Status = false;
        //                    ImportList.Result = e.Message + entityErr;
        //                }

        //                _ImportListModel.ImportDeliveryOrder.Add(ImportList);
        //            }
        //        }
        //    }

        //    stopUpload:

        //    return _ImportListModel;
        //}
        public ImportDeliveryListModel crudImportOrderExcel(List<ImportDeliveryOrderModel> ImportListModel, Boolean replace, string UserId)
        {

            ImportDeliveryListModel _ImportListModel = new ImportDeliveryListModel();
            _ImportListModel.ImportDeliveryOrder = new List<ImportDeliveryOrderModel>();
            DateTime date = DateTime.Now;

            if (ImportListModel.Count != 0)
            {
                var Order = vssp_db.Tbl_TRS_DeliveryOrderImport;

                foreach(var importDelivery in ImportListModel)
                {

                    ImportDeliveryOrderModel ImportList = importDelivery;

                    var exist = from a in Order
                                where a.DONumber == ImportList.DONumber && a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber
                                select a;

                    if (exist.Count() == 0)
                    {

                        Tbl_TRS_DeliveryOrderImport ListCreate = new Tbl_TRS_DeliveryOrderImport();
                        ListCreate.DONumber = ImportList.DONumber;
                        ListCreate.DODate = ImportList.DODate;
                        ListCreate.CustomerId = ImportList.CustomerId;
                        ListCreate.RefNumber = ImportList.RefNumber;
                        ListCreate.PartNumber = ImportList.PartNumber;
                        ListCreate.UniqueNumber = ImportList.UniqueNumber;
                        ListCreate.Qty = ImportList.Qty;
                        ListCreate.UserId = UserId;
                        ListCreate.ImportDate = date;
                        vssp_db.Tbl_TRS_DeliveryOrderImport.Add(ListCreate);

                        ImportList.Status = true;
                        ImportList.Result = "success imported.";
                    }
                    else
                    {
                        if (replace == true)
                        {

                            var ListUpdate = vssp_db.Tbl_TRS_DeliveryOrderImport.First(a => a.DONumber == ImportList.DONumber && a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber);

                            ListUpdate.DONumber = ImportList.DONumber;
                            ListUpdate.DODate = ImportList.DODate;
                            ListUpdate.CustomerId = ImportList.CustomerId;
                            ListUpdate.RefNumber = ImportList.RefNumber;
                            ListUpdate.PartNumber = ImportList.PartNumber;
                            ListUpdate.UniqueNumber = ImportList.UniqueNumber;
                            ListUpdate.Qty = ImportList.Qty;
                            ListUpdate.UserId = UserId;
                            ListUpdate.ImportDate = date;

                            ImportList.Status = true;
                            ImportList.Result = "replaced existing!";
                        }
                        else
                        {
                            ImportList.Status = false;
                            ImportList.Result = "skipped import, already exist!";
                        }
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        vssp_db.SP_CRUD_StockFromDelivery(ImportList.DONumber, ImportList.CustomerId, ImportList.PartNumber);
                    }
                    catch (DbEntityValidationException e)
                    {
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        ImportList.Status = false;
                        ImportList.Result = errinfo;
                    }

                    _ImportListModel.ImportDeliveryOrder.Add(ImportList);

                }

            }

            return _ImportListModel;
        }
    }
}