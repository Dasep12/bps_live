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
    public class ProductionsService
    {
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();

        // GET: Productionservice
        public ImportProductionsListModel uploadLineExcel(HttpPostedFileBase files)
        {
            ImportProductionsListModel _ImportListModel = new ImportProductionsListModel();
            _ImportListModel.ImportLine = new List<ImportLineModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //Line
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var Line = vssp_db.Tbl_MST_Line;

                    bool validTemplate = true;

                    //header validator Line
                    var LineId      = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var LineName    = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var AreaId      = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var LocationId  = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());

                    if (LineId.Replace(" ","").Replace("*","").ToLower() != "lineid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (LineName.Replace(" ", "").Replace("*", "").ToLower() != "linename")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (AreaId.Replace(" ", "").Replace("*", "").ToLower() != "areaid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (LocationId.Replace(" ", "").Replace("*", "").ToLower() != "locationid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    stopValidation:

                    //get data excel
                    if (validTemplate == true) {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportLineModel ImportList = new ImportLineModel();

                            ImportList.LineId       = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.LineName     = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.AreaId       = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.LocationId   = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());

                            var exist = from a in Line
                                        where a.LineId == ImportList.LineId
                                        select a;

                            if (exist.Count()== 0)
                            {
                                ImportList.Status = true;
                                ImportList.Result = "";
                            } else
                            {
                                ImportList.Status = false;
                                ImportList.Result = "already exist!";
                            }

                            if (ImportList.LineId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportListModel.ImportLine.Add(ImportList);
                        }

                    }

                    
                }
            }

            stopUpload:

            return _ImportListModel;

        }
        public ImportProductionsListModel crudImportLineExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            ImportProductionsListModel _ImportListModel = new ImportProductionsListModel();
            _ImportListModel.ImportLine = new List<ImportLineModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //Line
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var Line = vssp_db.Tbl_MST_Line;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {
                            
                        ImportLineModel ImportList = new ImportLineModel();

                        ImportList.LineId       = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.LineName     = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.AreaId       = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.LocationId   = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());

                        if (ImportList.LineId == "")
                        {
                            goto stopUpload;
                        }

                        var exist = from a in Line
                                    where a.LineId == ImportList.LineId
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_Line ListCreate = new Tbl_MST_Line();
                            ListCreate.LineId       = ImportList.LineId;
                            ListCreate.LineName     = ImportList.LineName;
                            ListCreate.AreaId       = ImportList.AreaId;
                            ListCreate.LocationId   = ImportList.LocationId;
                            ListCreate.Actived      = true;
                            ListCreate.UserId       = UserId;
                            ListCreate.EditDate     = DateTime.Now;
                            vssp_db.Tbl_MST_Line.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_Line.First(a => a.LineId == ImportList.LineId);

                                ListUpdate.LineName     = ImportList.LineName;
                                ListUpdate.AreaId       = ImportList.AreaId;
                                ListUpdate.LocationId   = ImportList.LocationId;
                                ListUpdate.UserId       = UserId;
                                ListUpdate.EditDate     = DateTime.Now;

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
                        }
                        catch (DbEntityValidationException e)
                        {
                            var errinfo = _SystemService.GetExceptionDetails(e);
                            ImportList.Status = false;
                            ImportList.Result = errinfo;
                        }

                        _ImportListModel.ImportLine.Add(ImportList);
                    }
                }
            }

            stopUpload:

            return _ImportListModel;
        }

        //Production MATERIALS
        public ImportProductionMaterialsListModel uploadProductionMaterialExcel(HttpPostedFileBase files, string canConfidential)
        {
            ImportProductionMaterialsListModel _ImportListModel = new ImportProductionMaterialsListModel();
            _ImportListModel.ImportProductionMaterial = new List<ImportProductionMaterialModel>();
            _ImportListModel.ImportProductionMaterialPrice = new List<ImportProductionMaterialPriceModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //ProductionMaterial
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var ProductionMaterial = vssp_db.Tbl_MST_PartProductionMaterials;

                    bool validTemplate = true;

                    //header validator ProductionMaterial
                    var LineId              = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var PartNumber          = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var UniqueNumber        = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var PartName            = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var PartModel           = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var CategoryId          = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var PackingId           = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                    var AreaId              = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());
                    var LocationId          = _SystemService.Vf(workSheet?.Cells[1, 10]?.Value?.ToString());
                    var UnitLevel1          = _SystemService.Vf(workSheet?.Cells[1, 11]?.Value?.ToString());
                    var UnitLevel2          = _SystemService.Vf(workSheet?.Cells[1, 12]?.Value?.ToString());
                    var UnitQty             = _SystemService.Vf(workSheet?.Cells[1, 13]?.Value?.ToString());
                    var SafetyHours         = _SystemService.Vf(workSheet?.Cells[1, 14]?.Value?.ToString());
                    var SubProcess          = _SystemService.Vf(workSheet?.Cells[1, 15]?.Value?.ToString());

                    if (LineId.Replace(" ", "").Replace("*", "").ToLower() != "lineid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (UniqueNumber.Replace(" ", "").Replace("*", "").ToLower() != "uniquenumber")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (PartName.Replace(" ", "").Replace("*", "").ToLower() != "partname")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (PartModel.Replace(" ", "").Replace("*", "").ToLower() != "partmodel")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (CategoryId.Replace(" ", "").Replace("*", "").ToLower() != "categoryid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (PackingId.Replace(" ", "").Replace("*", "").ToLower() != "packingid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (AreaId.Replace(" ", "").Replace("*", "").ToLower() != "areaid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (LocationId.Replace(" ", "").Replace("*", "").ToLower() != "locationid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (UnitLevel1.Replace(" ", "").Replace("*", "").ToLower() != "unitlevel1")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (UnitLevel2.Replace(" ", "").Replace("*", "").ToLower() != "unitlevel2")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (UnitQty.Replace(" ", "").Replace("*", "").ToLower() != "unitqty")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (SafetyHours.Replace(" ", "").Replace("*", "").ToLower() != "safetyhours")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (SubProcess.Replace(" ", "").Replace("*", "").ToLower() != "subprocess")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    stopValidation:

                    //get data excel
                    if (validTemplate == true)
                    {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportProductionMaterialModel ImportList = new ImportProductionMaterialModel();

                            ImportList.LineId              = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.PartNumber          = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.UniqueNumber        = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.PartName            = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.PartModel           = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.CategoryId          = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.PackingId           = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                            ImportList.AreaId              = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                            ImportList.LocationId          = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                            ImportList.UnitLevel1          = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                            ImportList.UnitLevel2          = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());
                            ImportList.UnitQty             = _SystemService.Vn(workSheet?.Cells[rowIterator, 13]?.Value?.ToString());
                            ImportList.SafetyHours         = _SystemService.Vn(workSheet?.Cells[rowIterator, 14]?.Value?.ToString());
                            ImportList.SubProcess          = _SystemService.Vb(workSheet?.Cells[rowIterator, 15]?.Value?.ToString());

                            var exist = from a in ProductionMaterial
                                        where a.LineId == ImportList.LineId && a.PartNumber == ImportList.PartNumber
                                        select a;

                            if (exist.Count() == 0)
                            {
                                ImportList.Status = true;
                                ImportList.Result = "";
                            }
                            else
                            {
                                ImportList.Status = false;
                                ImportList.Result = "already exist!";
                            }

                            if (ImportList.LineId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportListModel.ImportProductionMaterial.Add(ImportList);
                        }

                    }

                    stopUpload:
                    if (canConfidential == "")
                    {

                        //ProductionMaterial Price
                        workSheet = package.Workbook.Worksheets[1];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var ProductionMaterialPrice = vssp_db.Tbl_MST_PartProductionMaterialsPrice;

                        //header validator ProductionMaterial
                        var LineId3         = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                        var PartNumber3     = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                        var StartDate3      = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                        var EndDate3        = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                        var Price           = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());

                        if (LineId3.Replace(" ", "").Replace("*", "").ToLower() != "lineid")
                        {
                            validTemplate = false;
                            goto stopValidation3;
                        }
                        if (PartNumber3.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                        {
                            validTemplate = false;
                            goto stopValidation3;
                        }
                        if (StartDate3.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
                        {
                            validTemplate = false;
                            goto stopValidation3;
                        }
                        if (EndDate3.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
                        {
                            validTemplate = false;
                            goto stopValidation3;
                        }
                        if (Price.Replace(" ", "").Replace("*", "").ToLower() != "price")
                        {
                            validTemplate = false;
                            goto stopValidation3;
                        }

                        stopValidation3:

                        //get data excel
                        if (validTemplate == true)
                        {

                            for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                            {

                                ImportProductionMaterialPriceModel ImportList = new ImportProductionMaterialPriceModel();

                                ImportList.LineId   = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                                ImportList.PartNumber   = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                                if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
                                {
                                    ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
                                }
                                if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null){ 
                                    ImportList.EndDate      = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
                                }
                                ImportList.Price        = _SystemService.Vn(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());

                                var exist = from a in ProductionMaterialPrice
                                            where a.LineId == ImportList.LineId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
                                            select a;

                                if (exist.Count() == 0)
                                {
                                    ImportList.Status = true;
                                    ImportList.Result = "";
                                }
                                else
                                {
                                    ImportList.Status = false;
                                    ImportList.Result = "already exist!";
                                }

                                if (ImportList.LineId == "")
                                {
                                    goto stopUpload3;
                                }

                                _ImportListModel.ImportProductionMaterialPrice.Add(ImportList);
                            }
                        }
                                                
                    }
                }
            }

            stopUpload3:

            return _ImportListModel;

        }

        public ImportProductionMaterialsListModel crudImportProductionMaterialExcel(Boolean replace, string UserId, HttpPostedFileBase files, string canConfidential)
        {
            ImportProductionMaterialsListModel _ImportListModel = new ImportProductionMaterialsListModel();
            _ImportListModel.ImportProductionMaterial = new List<ImportProductionMaterialModel>();
            _ImportListModel.ImportProductionMaterialPrice = new List<ImportProductionMaterialPriceModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //ProductionMaterial
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var ProductionMaterial = vssp_db.Tbl_MST_PartProductionMaterials;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportProductionMaterialModel ImportList = new ImportProductionMaterialModel();

                        ImportList.LineId               = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.PartNumber           = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.UniqueNumber         = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.PartName             = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.PartModel            = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        ImportList.CategoryId           = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                        ImportList.PackingId            = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                        ImportList.AreaId               = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                        ImportList.LocationId           = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                        ImportList.UnitLevel1           = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                        ImportList.UnitLevel2           = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());
                        ImportList.UnitQty              = _SystemService.Vn(workSheet?.Cells[rowIterator, 13]?.Value?.ToString());
                        ImportList.SafetyHours          = _SystemService.Vn(workSheet?.Cells[rowIterator, 14]?.Value?.ToString());
                        ImportList.SubProcess           = _SystemService.Vb(workSheet?.Cells[rowIterator, 15]?.Value?.ToString());

                        if (ImportList.LineId == "")
                        {
                            goto stopUpload;
                        }

                        var exist = from a in ProductionMaterial
                                    where a.LineId == ImportList.LineId && a.PartNumber == ImportList.PartNumber
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_PartProductionMaterials ListCreate = new Tbl_MST_PartProductionMaterials();

                            ListCreate.LineId               = ImportList.LineId;
                            ListCreate.PartNumber           = ImportList.PartNumber;
                            ListCreate.UniqueNumber         = ImportList.UniqueNumber;
                            ListCreate.PartName             = ImportList.PartName;
                            ListCreate.PartModel            = ImportList.PartModel;
                            ListCreate.CategoryId           = ImportList.CategoryId;
                            ListCreate.PackingId            = ImportList.PackingId;
                            ListCreate.AreaId               = ImportList.AreaId;
                            ListCreate.LocationId           = ImportList.LocationId;
                            ListCreate.UnitLevel1           = ImportList.UnitLevel1;
                            ListCreate.UnitLevel2           = ImportList.UnitLevel2;
                            ListCreate.UnitQty              = ImportList.UnitQty;
                            ListCreate.SafetyHours          = ImportList.SafetyHours;
                            ListCreate.SubProcess           = ImportList.SubProcess;
                            ListCreate.Actived              = true;
                            ListCreate.UserId               = UserId;
                            ListCreate.EditDate             = DateTime.Now;

                            vssp_db.Tbl_MST_PartProductionMaterials.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_PartProductionMaterials.First(a => a.LineId == ImportList.LineId && a.PartNumber == ImportList.PartNumber);

                                ListUpdate.UniqueNumber         = ImportList.UniqueNumber;
                                ListUpdate.PartName             = ImportList.PartName;
                                ListUpdate.PartModel            = ImportList.PartModel;
                                ListUpdate.CategoryId           = ImportList.CategoryId;
                                ListUpdate.PackingId            = ImportList.PackingId;
                                ListUpdate.AreaId               = ImportList.AreaId;
                                ListUpdate.LocationId           = ImportList.LocationId;
                                ListUpdate.UnitLevel1           = ImportList.UnitLevel1;
                                ListUpdate.UnitLevel2           = ImportList.UnitLevel2;
                                ListUpdate.UnitQty              = ImportList.UnitQty;
                                ListUpdate.SafetyHours          = ImportList.SafetyHours;
                                ListUpdate.SubProcess           = ImportList.SubProcess;
                                ListUpdate.LocationId           = ImportList.LocationId;
                                ListUpdate.UserId               = UserId;
                                ListUpdate.EditDate             = DateTime.Now;

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
                        }
                        catch (DbEntityValidationException e)
                        {
                            var errinfo = _SystemService.GetExceptionDetails(e);
                            ImportList.Status = false;
                            ImportList.Result = errinfo;
                        }

                        _ImportListModel.ImportProductionMaterial.Add(ImportList);
                    }

                    stopUpload:

                    if (canConfidential == "")
                    {

                        //ProductionMaterial Price
                        workSheet = package.Workbook.Worksheets[1];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var ProductionMaterialPrice = vssp_db.Tbl_MST_PartProductionMaterialsPrice;

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportProductionMaterialPriceModel ImportList = new ImportProductionMaterialPriceModel();

                            ImportList.LineId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
                            {
                                ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
                            }
                            if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null)
                            {
                                ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
                            }
                            ImportList.Price = _SystemService.Vn(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());

                            if (ImportList.LineId == "")
                            {
                                goto stopUpload3;
                            }

                            var exist = from a in ProductionMaterialPrice
                                        where a.LineId == ImportList.LineId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_PartProductionMaterialsPrice ListCreate = new Tbl_MST_PartProductionMaterialsPrice();

                                ListCreate.LineId = ImportList.LineId;
                                ListCreate.PartNumber = ImportList.PartNumber;
                                ListCreate.StartDate = ImportList.StartDate;
                                ListCreate.EndDate = ImportList.EndDate;
                                ListCreate.Price = ImportList.Price;
                                ListCreate.UserId = UserId;
                                ListCreate.EditDate = DateTime.Now;

                                vssp_db.Tbl_MST_PartProductionMaterialsPrice.Add(ListCreate);

                                ImportList.Status = true;
                                ImportList.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var ListUpdate = vssp_db.Tbl_MST_PartProductionMaterialsPrice.First(a => a.LineId == ImportList.LineId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate);

                                    ListUpdate.EndDate = ImportList.EndDate;
                                    ListUpdate.Price = ImportList.Price;
                                    ListUpdate.UserId = UserId;
                                    ListUpdate.EditDate = DateTime.Now;

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
                            }
                            catch (DbEntityValidationException e)
                            {
                                var errinfo = _SystemService.GetExceptionDetails(e);
                                ImportList.Status = false;
                                ImportList.Result = errinfo;
                            }

                            _ImportListModel.ImportProductionMaterialPrice.Add(ImportList);
                        }

                    }
                }
            }

            stopUpload3:

            return _ImportListModel;
        }

        

    }
}