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
    public class WarehouseService
    {
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();
        // GET: WarehouseService
        public IEnumerable<ImportAreaModel> uploadAreaExcel(HttpPostedFileBase files)
        {
            List<ImportAreaModel> _ImportAreaModel = new List<ImportAreaModel>();

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
                    var area = vssp_db.Tbl_MST_WarehouseArea;

                    //header validator
                    var areaid      = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var areaname    = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var arearemarks = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                        
                    if (areaid.Replace(" ","").Replace("*","").ToLower() != "areaid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (areaname.Replace(" ", "").Replace("*", "").ToLower() != "areaname")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (arearemarks.Replace(" ","").Replace("*", "").ToLower() != "remarks")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    stopValidation:
                    
                    //get data excel
                    if (validTemplate == true) { 
                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportAreaModel importArea = new ImportAreaModel();

                            importArea.AreaID = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importArea.AreaName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importArea.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());

                            var exist = from a in area
                                        where a.AreaID == importArea.AreaID
                                        select a;

                            if (exist.Count()== 0)
                            {
                                importArea.Status = true;
                                importArea.Result = "";
                            } else
                            {
                                importArea.Status = false;
                                importArea.Result = "already exist!";
                            }

                            if (importArea.AreaID == "")
                            {
                                goto stopUpload;
                            }
                            _ImportAreaModel.Add(importArea);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportAreaModel;
        }
        public IEnumerable<ImportAreaModel> crudImportAreaExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            List<ImportAreaModel> _ImportAreaModel = new List<ImportAreaModel>();

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
                    var area = vssp_db.Tbl_MST_WarehouseArea;

                    //get data excel
                    if (validTemplate == true)
                    {
                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportAreaModel importArea = new ImportAreaModel();

                            importArea.AreaID = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importArea.AreaName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importArea.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());


                            if (importArea.AreaID == "")
                            {
                                goto stopUpload;
                            }

                            var exist = from a in area
                                        where a.AreaID == importArea.AreaID
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_WarehouseArea areaCreate = new Tbl_MST_WarehouseArea();
                                areaCreate.AreaID = importArea.AreaID;
                                areaCreate.AreaName = importArea.AreaName;
                                areaCreate.Remarks = importArea.Remarks;
                                areaCreate.UserID = UserId;
                                areaCreate.EditDate = DateTime.Now;
                                vssp_db.Tbl_MST_WarehouseArea.Add(areaCreate);

                                importArea.Status = true;
                                importArea.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var areaUpdate = vssp_db.Tbl_MST_WarehouseArea.First(a => a.AreaID == importArea.AreaID);

                                    areaUpdate.AreaName = importArea.AreaName;
                                    areaUpdate.Remarks = importArea.Remarks;
                                    areaUpdate.UserID = UserId;
                                    areaUpdate.EditDate = DateTime.Now;

                                    importArea.Status = true;
                                    importArea.Result = "replaced existing!";
                                }
                                else
                                {
                                    importArea.Status = false;
                                    importArea.Result = "skipped import, already exist!";
                                }
                            }

                            try
                            {
                                vssp_db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                var errinfo = _SystemService.GetExceptionDetails(e);
                                importArea.Status = false;
                                importArea.Result = errinfo;
                            }

                            _ImportAreaModel.Add(importArea);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportAreaModel;
        }
        public IEnumerable<ImportLocationModel> uploadLocationExcel(HttpPostedFileBase files)
        {
            List<ImportLocationModel> _ImportLocationModel = new List<ImportLocationModel>();

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
                    var Location = vssp_db.Tbl_MST_WarehouseLocation;

                    //header validator
                    var AreaId = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var LocationId = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var Locationname = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var Locationremarks = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());

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

                    if (Locationname.Replace(" ", "").Replace("*", "").ToLower() != "locationname")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (Locationremarks.Replace(" ", "").Replace("*", "").ToLower() != "remarks")
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

                            ImportLocationModel importLocation = new ImportLocationModel();

                            importLocation.AreaId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importLocation.LocationId = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importLocation.LocationName = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            importLocation.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());

                            var exist = from a in Location
                                        where a.AreaId == importLocation.AreaId && a.LocationId == importLocation.LocationId
                                        select a;

                            if (exist.Count() == 0)
                            {
                                importLocation.Status = true;
                                importLocation.Result = "";
                            }
                            else
                            {
                                importLocation.Status = false;
                                importLocation.Result = "already exist!";
                            }

                            if (importLocation.LocationId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportLocationModel.Add(importLocation);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportLocationModel;
        }
        public IEnumerable<ImportLocationModel> crudImportLocationExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            List<ImportLocationModel> _ImportLocationModel = new List<ImportLocationModel>();

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
                    var Location = vssp_db.Tbl_MST_WarehouseLocation;

                    //get data excel
                    if (validTemplate == true)
                    {
                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportLocationModel importLocation = new ImportLocationModel();

                            importLocation.AreaId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            importLocation.LocationId = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            importLocation.LocationName = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            importLocation.Remarks = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());


                            if (importLocation.LocationId == "")
                            {
                                goto stopUpload;
                            }

                            var exist = from a in Location
                                        where a.AreaId == importLocation.AreaId && a.LocationId == importLocation.LocationId
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_WarehouseLocation LocationCreate = new Tbl_MST_WarehouseLocation();
                                LocationCreate.AreaId = importLocation.AreaId;
                                LocationCreate.LocationId = importLocation.LocationId;
                                LocationCreate.LocationName = importLocation.LocationName;
                                LocationCreate.Remarks = importLocation.Remarks;
                                LocationCreate.UserId = UserId;
                                LocationCreate.EditDate = DateTime.Now;
                                vssp_db.Tbl_MST_WarehouseLocation.Add(LocationCreate);

                                importLocation.Status = true;
                                importLocation.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var LocationUpdate = vssp_db.Tbl_MST_WarehouseLocation.First(a => a.AreaId==importLocation.AreaId && a.LocationId == importLocation.LocationId);

                                    LocationUpdate.LocationName = importLocation.LocationName;
                                    LocationUpdate.Remarks = importLocation.Remarks;
                                    LocationUpdate.UserId = UserId;
                                    LocationUpdate.EditDate = DateTime.Now;

                                    importLocation.Status = true;
                                    importLocation.Result = "replaced existing!";
                                }
                                else
                                {
                                    importLocation.Status = false;
                                    importLocation.Result = "skipped import, already exist!";
                                }
                            }

                            try
                            {
                                vssp_db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                var errinfo = _SystemService.GetExceptionDetails(e);
                                importLocation.Status = false;
                                importLocation.Result = errinfo;
                            }

                            _ImportLocationModel.Add(importLocation);
                        }
                    }
                }
            }

            stopUpload:

            return _ImportLocationModel;
        }
    }
}