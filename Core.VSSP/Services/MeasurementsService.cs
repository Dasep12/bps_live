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
    public class MeasurementsService
    {
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();
        // GET: MeasurementsService
        public IEnumerable<ImportCategoriesModel> uploadCategoriesExcel(HttpPostedFileBase files)
        {
            List<ImportCategoriesModel> _ImportCategoriesModel = new List<ImportCategoriesModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    bool validTemplate = true;
                    var Categories = vssp_db.Tbl_MST_MeasurementsCategories;

                    //header validator
                    var CategoryId      = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var CategoryName    = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var Categoriesremarks = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                        
                    if (CategoryId.Replace(" ","").Replace("*","").ToLower() != "categoryid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (CategoryName.Replace(" ", "").Replace("*", "").ToLower() != "categoryname")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (Categoriesremarks.Replace(" ","").Replace("*", "").ToLower() != "remarks")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    stopValidation:
                    
                    //get data excel
                    if (validTemplate == true) { 
                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportCategoriesModel importCategories = new ImportCategoriesModel();

                            importCategories.CategoryId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importCategories.CategoryName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importCategories.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());

                            var exist = from a in Categories
                                        where a.CategoryId == importCategories.CategoryId
                                        select a;

                            if (exist.Count()== 0)
                            {
                                importCategories.Status = true;
                                importCategories.Result = "";
                            } else
                            {
                                importCategories.Status = false;
                                importCategories.Result = "already exist!";
                            }

                            if (importCategories.CategoryId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportCategoriesModel.Add(importCategories);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportCategoriesModel;
        }
        public IEnumerable<ImportCategoriesModel> crudImportCategoriesExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            List<ImportCategoriesModel> _ImportCategoriesModel = new List<ImportCategoriesModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    bool validTemplate = true;
                    var Categories = vssp_db.Tbl_MST_MeasurementsCategories;

                    //get data excel
                    if (validTemplate == true)
                    {
                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportCategoriesModel importCategories = new ImportCategoriesModel();

                            importCategories.CategoryId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importCategories.CategoryName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importCategories.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());


                            if (importCategories.CategoryId == "")
                            {
                                goto stopUpload;
                            }

                            var exist = from a in Categories
                                        where a.CategoryId == importCategories.CategoryId
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_MeasurementsCategories CategoriesCreate = new Tbl_MST_MeasurementsCategories();
                                CategoriesCreate.CategoryId = importCategories.CategoryId;
                                CategoriesCreate.CategoryName = importCategories.CategoryName;
                                CategoriesCreate.Remarks = importCategories.Remarks;
                                CategoriesCreate.UserID = UserId;
                                CategoriesCreate.EditDate = DateTime.Now;
                                vssp_db.Tbl_MST_MeasurementsCategories.Add(CategoriesCreate);

                                importCategories.Status = true;
                                importCategories.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var CategoriesUpdate = vssp_db.Tbl_MST_MeasurementsCategories.First(a => a.CategoryId == importCategories.CategoryId);

                                    CategoriesUpdate.CategoryName = importCategories.CategoryName;
                                    CategoriesUpdate.Remarks = importCategories.Remarks;
                                    CategoriesUpdate.UserID = UserId;
                                    CategoriesUpdate.EditDate = DateTime.Now;

                                    importCategories.Status = true;
                                    importCategories.Result = "replaced existing!";
                                }
                                else
                                {
                                    importCategories.Status = false;
                                    importCategories.Result = "skipped import, already exist!";
                                }
                            }

                            try
                            {
                                vssp_db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                var errinfo = _SystemService.GetExceptionDetails(e);
                                importCategories.Status = false;
                                importCategories.Result = errinfo;
                            }

                            _ImportCategoriesModel.Add(importCategories);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportCategoriesModel;
        }
        public IEnumerable<ImportUnitsModel> uploadUnitsExcel(HttpPostedFileBase files)
        {
            List<ImportUnitsModel> _ImportUnitsModel = new List<ImportUnitsModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    bool validTemplate = true;
                    var Units = vssp_db.Tbl_MST_MeasurementsUnits;

                    //header validator
                    var UnitId = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var Unitsname = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var ParentId = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var UnitLevel = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var Unitsremarks = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());

                    if (UnitId.Replace(" ", "").Replace("*", "").ToLower() != "unitid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (Unitsname.Replace(" ", "").Replace("*", "").ToLower() != "unitname")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (ParentId.Replace(" ", "").Replace("*", "").ToLower() != "parentid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (UnitLevel.Replace(" ", "").Replace("*", "").ToLower() != "unitlevel")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (Unitsremarks.Replace(" ", "").Replace("*", "").ToLower() != "remarks")
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

                            ImportUnitsModel importUnits = new ImportUnitsModel();

                            importUnits.UnitId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importUnits.UnitName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importUnits.ParentId = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            importUnits.UnitLevel = _SystemService.Vn(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            importUnits.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());

                            var exist = from a in Units
                                        where a.UnitId == importUnits.UnitId
                                        select a;

                            if (exist.Count() == 0)
                            {
                                importUnits.Status = true;
                                importUnits.Result = "";
                            }
                            else
                            {
                                importUnits.Status = false;
                                importUnits.Result = "already exist!";
                            }

                            if (importUnits.UnitId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportUnitsModel.Add(importUnits);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportUnitsModel;
        }
        public IEnumerable<ImportUnitsModel> crudImportUnitsExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            List<ImportUnitsModel> _ImportUnitsModel = new List<ImportUnitsModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    bool validTemplate = true;
                    var Units = vssp_db.Tbl_MST_MeasurementsUnits;

                    //get data excel
                    if (validTemplate == true)
                    {
                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportUnitsModel importUnits = new ImportUnitsModel();

                            importUnits.UnitId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importUnits.UnitName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importUnits.ParentId = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            importUnits.UnitLevel = _SystemService.Vn(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            importUnits.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());

                            if (importUnits.UnitId == "")
                            {
                                goto stopUpload;
                            }

                            var exist = from a in Units
                                        where a.UnitId == importUnits.UnitId
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_MeasurementsUnits UnitsCreate = new Tbl_MST_MeasurementsUnits();
                                UnitsCreate.UnitId = importUnits.UnitId;
                                UnitsCreate.UnitName = importUnits.UnitName;
                                UnitsCreate.ParentId = importUnits.ParentId;
                                UnitsCreate.UnitLevel = int.Parse(importUnits.UnitLevel.ToString());
                                UnitsCreate.Remarks = importUnits.Remarks;
                                UnitsCreate.UserID = UserId;
                                UnitsCreate.EditDate = DateTime.Now;
                                vssp_db.Tbl_MST_MeasurementsUnits.Add(UnitsCreate);

                                importUnits.Status = true;
                                importUnits.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var UnitsUpdate = vssp_db.Tbl_MST_MeasurementsUnits.First(a => a.UnitId == importUnits.UnitId);

                                    UnitsUpdate.UnitName = importUnits.UnitName;
                                    UnitsUpdate.Remarks = importUnits.Remarks;
                                    UnitsUpdate.ParentId = importUnits.ParentId;
                                    UnitsUpdate.UnitLevel = int.Parse(importUnits.UnitLevel.ToString());
                                    UnitsUpdate.UserID = UserId;
                                    UnitsUpdate.EditDate = DateTime.Now;

                                    importUnits.Status = true;
                                    importUnits.Result = "replaced existing!";
                                }
                                else
                                {
                                    importUnits.Status = false;
                                    importUnits.Result = "skipped import, already exist!";
                                }
                            }

                            try
                            {
                                vssp_db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                var errinfo = _SystemService.GetExceptionDetails(e);
                                importUnits.Status = false;
                                importUnits.Result = errinfo;
                            }

                            _ImportUnitsModel.Add(importUnits);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportUnitsModel;
        }
        public IEnumerable<ImportPackingModel> uploadPackingExcel(HttpPostedFileBase files)
        {
            List<ImportPackingModel> _ImportPackingModel = new List<ImportPackingModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    bool validTemplate = true;
                    var Packing = vssp_db.Tbl_MST_MeasurementsPacking;

                    //header validator
                    var PackingId = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var PackingName = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var Packingremarks = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());

                    if (PackingId.Replace(" ", "").Replace("*", "").ToLower() != "packingid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (PackingName.Replace(" ", "").Replace("*", "").ToLower() != "packingname")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (Packingremarks.Replace(" ", "").Replace("*", "").ToLower() != "remarks")
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

                            ImportPackingModel importPacking = new ImportPackingModel();

                            importPacking.PackingId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importPacking.PackingName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importPacking.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());

                            var exist = from a in Packing
                                        where a.PackingId == importPacking.PackingId
                                        select a;

                            if (exist.Count() == 0)
                            {
                                importPacking.Status = true;
                                importPacking.Result = "";
                            }
                            else
                            {
                                importPacking.Status = false;
                                importPacking.Result = "already exist!";
                            }

                            if (importPacking.PackingId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportPackingModel.Add(importPacking);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportPackingModel;
        }
        public IEnumerable<ImportPackingModel> crudImportPackingExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            List<ImportPackingModel> _ImportPackingModel = new List<ImportPackingModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    bool validTemplate = true;
                    var Packing = vssp_db.Tbl_MST_MeasurementsPacking;

                    //get data excel
                    if (validTemplate == true)
                    {
                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportPackingModel importPacking = new ImportPackingModel();

                            importPacking.PackingId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importPacking.PackingName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importPacking.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());


                            if (importPacking.PackingId == "")
                            {
                                goto stopUpload;
                            }

                            var exist = from a in Packing
                                        where a.PackingId == importPacking.PackingId
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_MeasurementsPacking PackingCreate = new Tbl_MST_MeasurementsPacking();
                                PackingCreate.PackingId = importPacking.PackingId;
                                PackingCreate.PackingName = importPacking.PackingName;
                                PackingCreate.Remarks = importPacking.Remarks;
                                PackingCreate.UserID = UserId;
                                PackingCreate.EditDate = DateTime.Now;
                                vssp_db.Tbl_MST_MeasurementsPacking.Add(PackingCreate);

                                importPacking.Status = true;
                                importPacking.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var PackingUpdate = vssp_db.Tbl_MST_MeasurementsPacking.First(a => a.PackingId == importPacking.PackingId);

                                    PackingUpdate.PackingName = importPacking.PackingName;
                                    PackingUpdate.Remarks = importPacking.Remarks;
                                    PackingUpdate.UserID = UserId;
                                    PackingUpdate.EditDate = DateTime.Now;

                                    importPacking.Status = true;
                                    importPacking.Result = "replaced existing!";
                                }
                                else
                                {
                                    importPacking.Status = false;
                                    importPacking.Result = "skipped import, already exist!";
                                }
                            }

                            try
                            {
                                vssp_db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                var errinfo = _SystemService.GetExceptionDetails(e);
                                importPacking.Status = false;
                                importPacking.Result = errinfo;
                            }

                            _ImportPackingModel.Add(importPacking);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportPackingModel;
        }
    }
}