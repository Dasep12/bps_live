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
    public class SuppliersService
    {
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();

        // GET: SupplierService
        public ImportSuppliersListModel uploadSupplierExcel(HttpPostedFileBase files)
        {
            ImportSuppliersListModel _ImportListModel = new ImportSuppliersListModel();
            _ImportListModel.ImportSupplier = new List<ImportSupplierModel>();
            _ImportListModel.ImportSupplierContact = new List<ImportSupplierContactModel>();
            _ImportListModel.ImportKanbanCycle = new List<ImportKanbanCycleModel>();
            _ImportListModel.ImportSupplierCostCenter = new List<ImportSupplierCostCenterModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //Supplier
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var Supplier = vssp_db.Tbl_MST_Supplier;

                    bool validTemplate = true;

                    //header validator Supplier
                    var SupplierId      = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var SupplierName    = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var Address         = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var City            = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var Provience       = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var Country         = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var PostalCode      = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                    var Websites        = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());
                    var TaxId           = _SystemService.Vf(workSheet?.Cells[1, 10]?.Value?.ToString());

                    if (SupplierId.Replace(" ","").Replace("*","").ToLower() != "supplierid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (SupplierName.Replace(" ", "").Replace("*", "").ToLower() != "suppliername")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (Address.Replace(" ", "").Replace("*", "").ToLower() != "address")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (City.Replace(" ", "").Replace("*", "").ToLower() != "city")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (Provience.Replace(" ", "").Replace("*", "").ToLower() != "provience")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (Country.Replace(" ", "").Replace("*", "").ToLower() != "country")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (PostalCode.Replace(" ", "").Replace("*", "").ToLower() != "postalcode")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (Websites.Replace(" ", "").Replace("*", "").ToLower() != "websites")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (TaxId.Replace(" ", "").Replace("*", "").ToLower() != "npwp")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    stopValidation:

                    //get data excel
                    if (validTemplate == true) {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportSupplierModel ImportList = new ImportSupplierModel();

                            ImportList.SupplierId   = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.SupplierName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.Address      = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.City         = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.Provience    = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.Country      = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.PostalCode   = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                            ImportList.Websites     = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                            ImportList.TaxId        = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());

                            var exist = from a in Supplier
                                        where a.SupplierId == ImportList.SupplierId
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

                            if (ImportList.SupplierId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportListModel.ImportSupplier.Add(ImportList);
                        }

                    }

                    stopUpload:

                    //Supplier Contact
                    workSheet = package.Workbook.Worksheets[1];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var Suppliercontact = vssp_db.Tbl_MST_SupplierContact;

                    //header validator Supplier
                    var SupplierId2     = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var ContactName     = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var Organization    = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var Position        = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var Phone1          = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var Phone2          = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var Fax             = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                    var Email           = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());
                    var ReceiveOrder    = _SystemService.Vf(workSheet?.Cells[1, 10]?.Value?.ToString());
                    var ReceiveInvoice  = _SystemService.Vf(workSheet?.Cells[1, 11]?.Value?.ToString());

                    if (SupplierId2.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (ContactName.Replace(" ", "").Replace("*", "").ToLower() != "contactname")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (Organization.Replace(" ", "").Replace("*", "").ToLower() != "organization")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (Position.Replace(" ", "").Replace("*", "").ToLower() != "position")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (Phone1.Replace(" ", "").Replace("*", "").ToLower() != "phone1")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (Phone2.Replace(" ", "").Replace("*", "").ToLower() != "phone2")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (Fax.Replace(" ", "").Replace("*", "").ToLower() != "fax")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (Email.Replace(" ", "").Replace("*", "").ToLower() != "email")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (ReceiveOrder.Replace(" ", "").Replace("*", "").ToLower() != "receiveorder")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    if (ReceiveInvoice.Replace(" ", "").Replace("*", "").ToLower() != "receiveinvoice")
                    {
                        validTemplate = false;
                        goto stopValidation2;
                    }
                    stopValidation2:

                    //get data excel
                    if (validTemplate == true)
                    {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportSupplierContactModel ImportList = new ImportSupplierContactModel();

                            ImportList.SupplierId       = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.ContactName      = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.Organization     = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.Position         = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.Phone1           = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.Phone2           = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.Fax              = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                            ImportList.Email            = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                            ImportList.ReceiveOrder     = _SystemService.Vb(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                            ImportList.ReceiveInvoice   = _SystemService.Vb(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());

                            var exist = from a in Suppliercontact
                                        where a.SupplierId == ImportList.SupplierId
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

                            if (ImportList.SupplierId == "")
                            {
                                goto stopUpload2;
                            }

                            _ImportListModel.ImportSupplierContact.Add(ImportList);
                        }
                    }

                    stopUpload2:

                    //RawMaterial Kanban
                    workSheet = package.Workbook.Worksheets[2];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var RawMaterialsKanban = vssp_db.Tbl_MST_KanbanCycle;

                    //header validator Kanban
                    var SupplierId3 = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var Cycle1 = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var Cycle2 = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var Cycle3 = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var CycleTime = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var StartDate = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var EndDate = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());

                    if (SupplierId3.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (Cycle1.Replace(" ", "").Replace("*", "").ToLower() != "cycle1")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (Cycle2.Replace(" ", "").Replace("*", "").ToLower() != "cycle2")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (Cycle3.Replace(" ", "").Replace("*", "").ToLower() != "cycle3")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (CycleTime.Replace(" ", "").Replace("*", "").ToLower() != "cycletime")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (StartDate.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (EndDate.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
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

                            ImportKanbanCycleModel ImportList = new ImportKanbanCycleModel();

                            ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.Cycle1 = _SystemService.Vn(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.Cycle2 = _SystemService.Vn(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.Cycle3 = _SystemService.Vn(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.CycleTime = workSheet?.Cells[rowIterator, 6]?.Value?.ToString();
                            if (workSheet?.Cells[rowIterator, 7]?.Value?.ToString() != null)
                            {
                                ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 7]?.Value?.ToString(), "yyyy-MM-dd"));
                            }
                            if (workSheet?.Cells[rowIterator, 8]?.Value?.ToString() != null)
                            {
                                ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 8]?.Value?.ToString(), "yyyy-MM-dd"));
                            }

                            var exist = from a in RawMaterialsKanban
                                        where a.SupplierId == ImportList.SupplierId && a.StartDate == ImportList.StartDate
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

                            if (ImportList.SupplierId == "")
                            {
                                goto stopUpload3;
                            }

                            _ImportListModel.ImportKanbanCycle.Add(ImportList);
                        }
                    }

                    stopUpload3:

                    //Supplier CostCenter
                    workSheet = package.Workbook.Worksheets[3];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var SupplierCostCenter = vssp_db.Tbl_MST_SupplierCostCenter;

                    //header validator Supplier
                    var SupplierId4 = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var CostId = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var CostName = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());

                    if (SupplierId2.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
                    {
                        validTemplate = false;
                        goto stopValidation4;
                    }
                    if (CostId.Replace(" ", "").Replace("*", "").ToLower() != "costid")
                    {
                        validTemplate = false;
                        goto stopValidation4;
                    }
                    if (CostName.Replace(" ", "").Replace("*", "").ToLower() != "costname")
                    {
                        validTemplate = false;
                        goto stopValidation4;
                    }

                    stopValidation4:

                    //get data excel
                    if (validTemplate == true)
                    {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportSupplierCostCenterModel ImportList = new ImportSupplierCostCenterModel();

                            ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.CostId = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.CostName = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());

                            var exist = from a in SupplierCostCenter
                                        where a.SupplierId == ImportList.SupplierId && a.CostId == ImportList.CostId
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

                            if (ImportList.SupplierId == "")
                            {
                                goto stopUpload4;
                            }

                            _ImportListModel.ImportSupplierCostCenter.Add(ImportList);
                        }
                    }

                }
            }

            stopUpload4:

            return _ImportListModel;

        }
        public ImportSuppliersListModel crudImportSupplierExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            ImportSuppliersListModel _ImportListModel = new ImportSuppliersListModel();
            _ImportListModel.ImportSupplier = new List<ImportSupplierModel>();
            _ImportListModel.ImportSupplierContact = new List<ImportSupplierContactModel>();
            _ImportListModel.ImportKanbanCycle = new List<ImportKanbanCycleModel>();
            _ImportListModel.ImportSupplierCostCenter = new List<ImportSupplierCostCenterModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //Supplier
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var Supplier = vssp_db.Tbl_MST_Supplier;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {
                            
                        ImportSupplierModel ImportList = new ImportSupplierModel();

                        ImportList.SupplierId   = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.SupplierName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.Address      = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.City         = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.Provience    = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        ImportList.Country      = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                        ImportList.PostalCode   = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                        ImportList.Websites     = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                        ImportList.TaxId        = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());

                        if (ImportList.SupplierId == "")
                        {
                            goto stopUpload;
                        }

                        var exist = from a in Supplier
                                    where a.SupplierId == ImportList.SupplierId
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_Supplier ListCreate = new Tbl_MST_Supplier();
                            ListCreate.SupplierId   = ImportList.SupplierId;
                            ListCreate.SupplierName = ImportList.SupplierName;
                            ListCreate.Address      = ImportList.Address;
                            ListCreate.City         = ImportList.City;
                            ListCreate.Provience    = ImportList.Provience;
                            ListCreate.Country      = ImportList.Country;
                            ListCreate.PostalCode   = ImportList.PostalCode;
                            ListCreate.Websites     = ImportList.Websites;
                            ListCreate.TaxId        = ImportList.TaxId;
                            ListCreate.Actived      = true;
                            ListCreate.UserID       = UserId;
                            ListCreate.EditDate     = DateTime.Now;
                            vssp_db.Tbl_MST_Supplier.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_Supplier.First(a => a.SupplierId == ImportList.SupplierId);

                                ListUpdate.SupplierName = ImportList.SupplierName;
                                ListUpdate.Address      = ImportList.Address;
                                ListUpdate.City         = ImportList.City;
                                ListUpdate.Provience    = ImportList.Provience;
                                ListUpdate.Country      = ImportList.Country;
                                ListUpdate.PostalCode   = ImportList.PostalCode;
                                ListUpdate.Websites     = ImportList.Websites;
                                ListUpdate.TaxId        = ImportList.TaxId;
                                ListUpdate.UserID       = UserId;
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

                        _ImportListModel.ImportSupplier.Add(ImportList);
                    }

                    stopUpload:

                    //Supplier contact
                    workSheet = package.Workbook.Worksheets[1];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var Suppliercontact = vssp_db.Tbl_MST_SupplierContact;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportSupplierContactModel ImportList = new ImportSupplierContactModel();

                        ImportList.SupplierId       = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.ContactName      = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.Organization     = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.Position         = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.Phone1           = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        ImportList.Phone2           = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                        ImportList.Fax              = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                        ImportList.Email            = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                        ImportList.ReceiveOrder     = _SystemService.Vb(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                        ImportList.ReceiveInvoice   = _SystemService.Vb(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());

                        if (ImportList.SupplierId == "")
                        {
                            goto stopUpload2;
                        }

                        var exist = from a in Suppliercontact
                                    where a.SupplierId == ImportList.SupplierId
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_SupplierContact ListCreate = new Tbl_MST_SupplierContact();

                            ListCreate.SupplierId       = ImportList.SupplierId;
                            ListCreate.ContactName      = ImportList.ContactName;
                            ListCreate.Organization     = ImportList.Organization;
                            ListCreate.Position         = ImportList.Position;
                            ListCreate.Phone1           = ImportList.Phone1;
                            ListCreate.Phone2           = ImportList.Phone2;
                            ListCreate.Fax              = ImportList.Fax;
                            ListCreate.Email            = ImportList.Email;
                            ListCreate.ReceiveOrder     = ImportList.ReceiveOrder;
                            ListCreate.ReceiveInvoice   = ImportList.ReceiveInvoice;

                            vssp_db.Tbl_MST_SupplierContact.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_SupplierContact.First(a => a.SupplierId == ImportList.SupplierId);

                                ListUpdate.ContactName = ImportList.ContactName;
                                ListUpdate.Organization = ImportList.Organization;
                                ListUpdate.Position = ImportList.Position;
                                ListUpdate.Phone1 = ImportList.Phone1;
                                ListUpdate.Phone2 = ImportList.Phone2;
                                ListUpdate.Fax = ImportList.Fax;
                                ListUpdate.Email = ImportList.Email;
                                ListUpdate.ReceiveOrder = ImportList.ReceiveOrder;
                                ListUpdate.ReceiveInvoice = ImportList.ReceiveInvoice;

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

                        _ImportListModel.ImportSupplierContact.Add(ImportList);
                    }

                    stopUpload2:

                    //RawMaterial Kanban
                    workSheet = package.Workbook.Worksheets[2];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var KanbanCycle = vssp_db.Tbl_MST_KanbanCycle;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportKanbanCycleModel ImportList = new ImportKanbanCycleModel();

                        ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.Cycle1 = _SystemService.Vn(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.Cycle2 = _SystemService.Vn(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.Cycle3 = _SystemService.Vn(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.CycleTime = workSheet?.Cells[rowIterator, 6]?.Value?.ToString();
                        if (workSheet?.Cells[rowIterator, 7]?.Value?.ToString() != null)
                        {
                            ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 7]?.Value?.ToString(), "yyyy-MM-dd"));
                        }
                        if (workSheet?.Cells[rowIterator, 8]?.Value?.ToString() != null)
                        {
                            ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 8]?.Value?.ToString(), "yyyy-MM-dd"));
                        }


                        if (ImportList.SupplierId == "")
                        {
                            goto stopUpload3;
                        }

                        var exist = from a in KanbanCycle
                                    where a.SupplierId == ImportList.SupplierId && a.StartDate == ImportList.StartDate
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_KanbanCycle ListCreate = new Tbl_MST_KanbanCycle();

                            ListCreate.SupplierId = ImportList.SupplierId;
                            ListCreate.Cycle1 = ImportList.Cycle1;
                            ListCreate.Cycle2 = ImportList.Cycle2;
                            ListCreate.Cycle3 = ImportList.Cycle3;
                            ListCreate.CycleTime = ImportList.CycleTime;
                            ListCreate.StartDate = ImportList.StartDate;
                            ListCreate.EndDate = ImportList.EndDate;
                            ListCreate.UserId = UserId;
                            ListCreate.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_KanbanCycle.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_KanbanCycle.First(a => a.SupplierId == ImportList.SupplierId && a.StartDate == ImportList.StartDate);

                                ListUpdate.Cycle1 = ImportList.Cycle1;
                                ListUpdate.Cycle2 = ImportList.Cycle2;
                                ListUpdate.Cycle3 = ImportList.Cycle3;
                                ListUpdate.CycleTime = ImportList.CycleTime;
                                ListUpdate.EndDate = ImportList.EndDate;
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

                        _ImportListModel.ImportKanbanCycle.Add(ImportList);
                    }

                    stopUpload3:

                    //Supplier CostCenter
                    workSheet = package.Workbook.Worksheets[3];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var SupplierCostCenter = vssp_db.Tbl_MST_SupplierCostCenter;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportSupplierCostCenterModel ImportList = new ImportSupplierCostCenterModel();

                        ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.CostId = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.CostName = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());

                        if (ImportList.SupplierId == "")
                        {
                            goto stopUpload4;
                        }

                        var exist = from a in SupplierCostCenter
                                    where a.SupplierId == ImportList.SupplierId && a.CostId == ImportList.CostId
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_SupplierCostCenter ListCreate = new Tbl_MST_SupplierCostCenter();

                            ListCreate.SupplierId = ImportList.SupplierId;
                            ListCreate.CostId = ImportList.CostId;
                            ListCreate.CostName = ImportList.CostName;
                            ListCreate.UserId = UserId;
                            ListCreate.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_SupplierCostCenter.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_SupplierCostCenter.First(a => a.SupplierId == ImportList.SupplierId && a.CostId == ImportList.CostId);

                                ListUpdate.CostName = ImportList.CostName;
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

                        _ImportListModel.ImportSupplierCostCenter.Add(ImportList);
                    }

                }
            }

            stopUpload4:

            return _ImportListModel;
        }

        //RAW MATERIALS
        public ImportRawMaterialsListModel uploadRawMaterialExcel(HttpPostedFileBase files, string canConfidential)
        {
            ImportRawMaterialsListModel _ImportListModel = new ImportRawMaterialsListModel();
            _ImportListModel.ImportRawMaterial = new List<ImportRawMaterialModel>();
            _ImportListModel.ImportRawMaterialCostCenter = new List<ImportRawMaterialCostCenterModel>();
            _ImportListModel.ImportRawMaterialPrice = new List<ImportRawMaterialPriceModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //RawMaterial
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var RawMaterial = vssp_db.Tbl_MST_PartRawMaterials;

                    bool validTemplate = true;

                    //header validator RawMaterial
                    var SupplierId          = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var PartNumber          = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var PartNumberSupplier  = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var UniqueNumber        = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var PartName            = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var PartModel           = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var CategoryId          = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                    var PackingId           = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());
                    var AreaId              = _SystemService.Vf(workSheet?.Cells[1, 10]?.Value?.ToString());
                    var LocationId          = _SystemService.Vf(workSheet?.Cells[1, 11]?.Value?.ToString());
                    var UnitLevel1          = _SystemService.Vf(workSheet?.Cells[1, 12]?.Value?.ToString());
                    var UnitLevel2          = _SystemService.Vf(workSheet?.Cells[1, 13]?.Value?.ToString());
                    var UnitQty             = _SystemService.Vf(workSheet?.Cells[1, 14]?.Value?.ToString());
                    var SafetyHours         = _SystemService.Vf(workSheet?.Cells[1, 15]?.Value?.ToString());

                    if (SupplierId.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (PartNumberSupplier.Replace(" ", "").Replace("*", "").ToLower() != "partnumbersupplier")
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
                    stopValidation:

                    //get data excel
                    if (validTemplate == true)
                    {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportRawMaterialModel ImportList = new ImportRawMaterialModel();

                            ImportList.SupplierId          = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.PartNumber          = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.PartNumberSupplier  = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.UniqueNumber        = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.PartName            = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.PartModel           = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.CategoryId          = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                            ImportList.PackingId           = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                            ImportList.AreaId              = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                            ImportList.LocationId          = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                            ImportList.UnitLevel1          = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());
                            ImportList.UnitLevel2          = _SystemService.Vf(workSheet?.Cells[rowIterator, 13]?.Value?.ToString());
                            ImportList.UnitQty             = _SystemService.Vn(workSheet?.Cells[rowIterator, 14]?.Value?.ToString());
                            ImportList.SafetyHours         = _SystemService.Vn(workSheet?.Cells[rowIterator, 15]?.Value?.ToString());

                            var exist = from a in RawMaterial
                                        where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber
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

                            if (ImportList.SupplierId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportListModel.ImportRawMaterial.Add(ImportList);
                        }

                    }

                    stopUpload:
                    if (canConfidential == "")
                    {

                        //RawMaterial CostCenter
                        workSheet = package.Workbook.Worksheets[1];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var RawMaterialCostCenter = vssp_db.Tbl_MST_PartRawMaterialsCostCenter;

                        //header validator RawMaterial
                        var SupplierId2 = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                        var PartNumber2 = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                        var StartDate2 = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                        var EndDate2 = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                        var CostId = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                        var ClassificationId = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                        var PaymentId = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                        var CostCategoryId = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());

                        if (SupplierId2.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (PartNumber2.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (StartDate2.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (EndDate2.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (CostId.Replace(" ", "").Replace("*", "").ToLower() != "costid")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (ClassificationId.Replace(" ", "").Replace("*", "").ToLower() != "classificationid")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (PaymentId.Replace(" ", "").Replace("*", "").ToLower() != "paymentid")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (CostCategoryId.Replace(" ", "").Replace("*", "").ToLower() != "categoryid")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }

                        stopValidation2:

                        //get data excel
                        if (validTemplate == true)
                        {

                            for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                            {

                                ImportRawMaterialCostCenterModel ImportList = new ImportRawMaterialCostCenterModel();

                                ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                                ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                                if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
                                {
                                    ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
                                }
                                if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null)
                                {
                                    ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
                                }
                                ImportList.CostId = workSheet?.Cells[rowIterator, 6]?.Value?.ToString();
                                ImportList.ClassificationId = workSheet?.Cells[rowIterator, 7]?.Value?.ToString();
                                ImportList.PaymentId = workSheet?.Cells[rowIterator, 8]?.Value?.ToString();
                                ImportList.CategoryId = workSheet?.Cells[rowIterator, 9]?.Value?.ToString();

                                var exist = from a in RawMaterialCostCenter
                                            where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
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

                                if (ImportList.SupplierId == "")
                                {
                                    goto stopUpload2;
                                }

                                _ImportListModel.ImportRawMaterialCostCenter.Add(ImportList);
                            }
                        }

                        stopUpload2:

                        //RawMaterial Price
                        workSheet = package.Workbook.Worksheets[2];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var RawMaterialPrice = vssp_db.Tbl_MST_PartRawMaterialsPrice;

                        //header validator RawMaterial
                        var SupplierId3     = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                        var PartNumber3     = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                        var StartDate3      = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                        var EndDate3        = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                        var Price           = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());

                        if (SupplierId3.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
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

                                ImportRawMaterialPriceModel ImportList = new ImportRawMaterialPriceModel();

                                ImportList.SupplierId   = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                                ImportList.PartNumber   = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                                if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
                                {
                                    ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
                                }
                                if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null){ 
                                    ImportList.EndDate      = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
                                }
                                ImportList.Price        = _SystemService.Vn(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());

                                var exist = from a in RawMaterialPrice
                                            where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
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

                                if (ImportList.SupplierId == "")
                                {
                                    goto stopUpload3;
                                }

                                _ImportListModel.ImportRawMaterialPrice.Add(ImportList);
                            }
                        }
                                                
                    }
                }
            }

            stopUpload3:

            return _ImportListModel;

        }

        public ImportRawMaterialsListModel crudImportRawMaterialExcel(Boolean replace, string UserId, HttpPostedFileBase files, string canConfidential)
        {
            ImportRawMaterialsListModel _ImportListModel = new ImportRawMaterialsListModel();
            _ImportListModel.ImportRawMaterial = new List<ImportRawMaterialModel>();
            _ImportListModel.ImportRawMaterialCostCenter = new List<ImportRawMaterialCostCenterModel>();
            _ImportListModel.ImportRawMaterialPrice = new List<ImportRawMaterialPriceModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //RawMaterial
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var RawMaterial = vssp_db.Tbl_MST_PartRawMaterials;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportRawMaterialModel ImportList = new ImportRawMaterialModel();

                        ImportList.SupplierId           = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.PartNumber           = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.PartNumberSupplier   = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.UniqueNumber         = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.PartName             = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        ImportList.PartModel            = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                        ImportList.CategoryId           = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                        ImportList.PackingId            = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                        ImportList.AreaId               = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                        ImportList.LocationId           = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                        ImportList.UnitLevel1           = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());
                        ImportList.UnitLevel2           = _SystemService.Vf(workSheet?.Cells[rowIterator, 13]?.Value?.ToString());
                        ImportList.UnitQty              = _SystemService.Vn(workSheet?.Cells[rowIterator, 14]?.Value?.ToString());
                        ImportList.SafetyHours          = _SystemService.Vn(workSheet?.Cells[rowIterator, 15]?.Value?.ToString());

                        if (ImportList.SupplierId == "")
                        {
                            goto stopUpload;
                        }

                        var exist = from a in RawMaterial
                                    where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_PartRawMaterials ListCreate = new Tbl_MST_PartRawMaterials();

                            ListCreate.SupplierId           = ImportList.SupplierId;
                            ListCreate.PartNumber           = ImportList.PartNumber;
                            ListCreate.PartNumberSupplier   = ImportList.PartNumberSupplier;
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
                            ListCreate.LocationId           = ImportList.LocationId;
                            ListCreate.Actived              = true;
                            ListCreate.UserId               = UserId;
                            ListCreate.EditDate             = DateTime.Now;

                            vssp_db.Tbl_MST_PartRawMaterials.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_PartRawMaterials.First(a => a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber);

                                //ListUpdate.SupplierId           = ImportList.SupplierId;
                                //ListUpdate.PartNumber           = ImportList.PartNumber;
                                ListUpdate.PartNumberSupplier   = ImportList.PartNumberSupplier;
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

                        _ImportListModel.ImportRawMaterial.Add(ImportList);
                    }

                    stopUpload:

                    if (canConfidential == "")
                    {

                        //RawMaterial CostCenter
                        workSheet = package.Workbook.Worksheets[1];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var RawMaterialCostCenter = vssp_db.Tbl_MST_PartRawMaterialsCostCenter;

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportRawMaterialCostCenterModel ImportList = new ImportRawMaterialCostCenterModel();

                            ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
                            {
                                ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
                            }
                            if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null)
                            {
                                ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
                            }
                            ImportList.CostId = workSheet?.Cells[rowIterator, 6]?.Value?.ToString();
                            ImportList.ClassificationId = workSheet?.Cells[rowIterator, 7]?.Value?.ToString();
                            ImportList.PaymentId = workSheet?.Cells[rowIterator, 8]?.Value?.ToString();
                            ImportList.CategoryId = workSheet?.Cells[rowIterator, 9]?.Value?.ToString();

                            if (ImportList.SupplierId == "")
                            {
                                goto stopUpload2;
                            }

                            var exist = from a in RawMaterialCostCenter
                                        where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_PartRawMaterialsCostCenter ListCreate = new Tbl_MST_PartRawMaterialsCostCenter();

                                ListCreate.SupplierId = ImportList.SupplierId;
                                ListCreate.PartNumber = ImportList.PartNumber;
                                ListCreate.StartDate = ImportList.StartDate;
                                ListCreate.EndDate = ImportList.EndDate;
                                ListCreate.CostId = ImportList.CostId;
                                ListCreate.ClassificationId = ImportList.ClassificationId;
                                ListCreate.PaymentId = ImportList.PaymentId;
                                ListCreate.CategoryId = ImportList.CategoryId;
                                ListCreate.UserId = UserId;
                                ListCreate.EditDate = DateTime.Now;

                                vssp_db.Tbl_MST_PartRawMaterialsCostCenter.Add(ListCreate);

                                ImportList.Status = true;
                                ImportList.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var ListUpdate = vssp_db.Tbl_MST_PartRawMaterialsCostCenter.First(a => a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate);

                                    ListUpdate.EndDate = ImportList.EndDate;
                                    ListUpdate.CostId = ImportList.CostId;
                                    ListUpdate.ClassificationId = ImportList.ClassificationId;
                                    ListUpdate.PaymentId = ImportList.PaymentId;
                                    ListUpdate.CategoryId = ImportList.CategoryId;
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

                            _ImportListModel.ImportRawMaterialCostCenter.Add(ImportList);
                        }

                        stopUpload2:

                        //RawMaterial Price
                        workSheet = package.Workbook.Worksheets[2];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var RawMaterialPrice = vssp_db.Tbl_MST_PartRawMaterialsPrice;

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportRawMaterialPriceModel ImportList = new ImportRawMaterialPriceModel();

                            ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
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

                            if (ImportList.SupplierId == "")
                            {
                                goto stopUpload3;
                            }

                            var exist = from a in RawMaterialPrice
                                        where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_PartRawMaterialsPrice ListCreate = new Tbl_MST_PartRawMaterialsPrice();

                                ListCreate.SupplierId = ImportList.SupplierId;
                                ListCreate.PartNumber = ImportList.PartNumber;
                                ListCreate.StartDate = ImportList.StartDate;
                                ListCreate.EndDate = ImportList.EndDate;
                                ListCreate.Price = ImportList.Price;
                                ListCreate.UserId = UserId;
                                ListCreate.EditDate = DateTime.Now;

                                vssp_db.Tbl_MST_PartRawMaterialsPrice.Add(ListCreate);

                                ImportList.Status = true;
                                ImportList.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var ListUpdate = vssp_db.Tbl_MST_PartRawMaterialsPrice.First(a => a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate);

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

                            _ImportListModel.ImportRawMaterialPrice.Add(ImportList);
                        }

                    }
                }
            }

            stopUpload3:

            return _ImportListModel;
        }

        ////RAW MATERIALS
        //public ImportKanbanCycleListModel uploadKanbanCycleExcel(HttpPostedFileBase files, string canConfidential)
        //{
        //    ImportKanbanCycleListModel _ImportListModel = new ImportKanbanCycleListModel();
        //    _ImportListModel.ImportCycleTime1 = new List<ImportCycleTime1Model>();
        //    _ImportListModel.ImportCycleTime2 = new List<ImportCycleTime2Model>();
        //    _ImportListModel.ImportCycleTime3 = new List<ImportCycleTime3Model>();

        //    if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
        //    {

        //        string fileName = files.FileName;
        //        string fileContentType = files.ContentType;
        //        byte[] fileBytes = new byte[files.ContentLength];
        //        var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //        using (var package = new ExcelPackage(files.InputStream))
        //        {
        //            //KanbanCycle
        //            var workSheet = package.Workbook.Worksheets[0];
        //            var noOfCol = workSheet.Dimension.End.Column;
        //            var noOfRow = workSheet.Dimension.End.Row;
        //            var SupplierId = "";
        //            var PartNumber = "";
        //            var StartDate = "";
        //            var EndDate = "";
        //            var CycleTime = "";

        //            bool validTemplate = true;

        //            //header validator KanbanCycle 1
        //            var KanbanCycle1 = vssp_db.Tbl_MST_KanbanCycle1;

        //            SupplierId  = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
        //            PartNumber  = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
        //            StartDate   = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
        //            EndDate     = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
        //            CycleTime   = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());

        //            if (SupplierId.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
        //            {
        //                validTemplate = false;
        //                goto stopValidation;
        //            }

        //            if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
        //            {
        //                validTemplate = false;
        //                goto stopValidation;
        //            }
        //            if (StartDate.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
        //            {
        //                validTemplate = false;
        //                goto stopValidation;
        //            }
        //            if (EndDate.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
        //            {
        //                validTemplate = false;
        //                goto stopValidation;
        //            }
        //            if (CycleTime.Replace(" ", "").Replace("*", "").ToLower() != "cycletime")
        //            {
        //                validTemplate = false;
        //                goto stopValidation;
        //            }

        //            stopValidation:

        //            //get data excel
        //            if (validTemplate == true)
        //            {

        //                for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
        //                {

        //                    ImportCycleTime1Model ImportList = new ImportCycleTime1Model();

        //                    ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
        //                    ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
        //                    if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
        //                    }
        //                    if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
        //                    }
        //                    if (workSheet?.Cells[rowIterator, 6]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.CycleTime = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 6]?.Value?.ToString(), "yyyy-MM-dd"));
        //                    }
        //                    var exist = from a in KanbanCycle1
        //                                where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.EndDate && a.CycleTime == ImportList.CycleTime
        //                                select a;

        //                    if (exist.Count() == 0)
        //                    {
        //                        ImportList.Status = true;
        //                        ImportList.Result = "";
        //                    }
        //                    else
        //                    {
        //                        ImportList.Status = false;
        //                        ImportList.Result = "already exist!";
        //                    }

        //                    if (ImportList.SupplierId == "")
        //                    {
        //                        goto stopUpload;
        //                    }
        //                    _ImportListModel.ImportCycleTime1.Add(ImportList);
        //                }

        //            }

        //            stopUpload:

        //            //KanbanCycle Kanban
        //            workSheet = package.Workbook.Worksheets[1];
        //            noOfCol = workSheet.Dimension.End.Column;
        //            noOfRow = workSheet.Dimension.End.Row;

        //            //header validator KanbanCycle 2
        //            var KanbanCycle2 = vssp_db.Tbl_MST_KanbanCycle2;

        //            SupplierId = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
        //            PartNumber = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
        //            StartDate = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
        //            EndDate = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
        //            CycleTime = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());

        //            if (SupplierId.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
        //            {
        //                validTemplate = false;
        //                goto stopValidation2;
        //            }

        //            if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
        //            {
        //                validTemplate = false;
        //                goto stopValidation2;
        //            }
        //            if (StartDate.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
        //            {
        //                validTemplate = false;
        //                goto stopValidation2;
        //            }
        //            if (EndDate.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
        //            {
        //                validTemplate = false;
        //                goto stopValidation2;
        //            }
        //            if (CycleTime.Replace(" ", "").Replace("*", "").ToLower() != "cycletime")
        //            {
        //                validTemplate = false;
        //                goto stopValidation2;
        //            }

        //            stopValidation2:

        //            //get data excel
        //            if (validTemplate == true)
        //            {

        //                for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
        //                {

        //                    ImportCycleTime2Model ImportList = new ImportCycleTime2Model();

        //                    ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
        //                    ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
        //                    if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
        //                    }
        //                    if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
        //                    }
        //                    if (workSheet?.Cells[rowIterator, 6]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.CycleTime = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 6]?.Value?.ToString(), "HH:mm"));
        //                    }
        //                    var exist = from a in KanbanCycle2
        //                                where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.EndDate && a.CycleTime == ImportList.CycleTime
        //                                select a;

        //                    if (exist.Count() == 0)
        //                    {
        //                        ImportList.Status = true;
        //                        ImportList.Result = "";
        //                    }
        //                    else
        //                    {
        //                        ImportList.Status = false;
        //                        ImportList.Result = "already exist!";
        //                    }

        //                    if (ImportList.SupplierId == "")
        //                    {
        //                        goto stopUpload2;
        //                    }
        //                    _ImportListModel.ImportCycleTime2.Add(ImportList);
        //                }

        //            }

        //            stopUpload2:

        //            //KanbanCycle Kanban
        //            workSheet = package.Workbook.Worksheets[2];
        //            noOfCol = workSheet.Dimension.End.Column;
        //            noOfRow = workSheet.Dimension.End.Row;

        //            //header validator KanbanCycle 3
        //            var KanbanCycle3 = vssp_db.Tbl_MST_KanbanCycle3;

        //            SupplierId = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
        //            PartNumber = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
        //            StartDate = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
        //            EndDate = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
        //            CycleTime = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());

        //            if (SupplierId.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
        //            {
        //                validTemplate = false;
        //                goto stopValidation3;
        //            }

        //            if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
        //            {
        //                validTemplate = false;
        //                goto stopValidation3;
        //            }
        //            if (StartDate.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
        //            {
        //                validTemplate = false;
        //                goto stopValidation3;
        //            }
        //            if (EndDate.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
        //            {
        //                validTemplate = false;
        //                goto stopValidation3;
        //            }
        //            if (CycleTime.Replace(" ", "").Replace("*", "").ToLower() != "cycletime")
        //            {
        //                validTemplate = false;
        //                goto stopValidation3;
        //            }

        //            stopValidation3:

        //            //get data excel
        //            if (validTemplate == true)
        //            {

        //                for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
        //                {

        //                    ImportCycleTime3Model ImportList = new ImportCycleTime3Model();

        //                    ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
        //                    ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
        //                    if (workSheet?.Cells[rowIterator, 4]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 4]?.Value?.ToString(), "yyyy-MM-dd"));
        //                    }
        //                    if (workSheet?.Cells[rowIterator, 5]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 5]?.Value?.ToString(), "yyyy-MM-dd"));
        //                    }
        //                    if (workSheet?.Cells[rowIterator, 6]?.Value?.ToString() != null)
        //                    {
        //                        ImportList.CycleTime = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 6]?.Value?.ToString(), "HH:mm"));
        //                    }
        //                    var exist = from a in KanbanCycle3
        //                                where a.SupplierId == ImportList.SupplierId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.EndDate && a.CycleTime == ImportList.CycleTime
        //                                select a;

        //                    if (exist.Count() == 0)
        //                    {
        //                        ImportList.Status = true;
        //                        ImportList.Result = "";
        //                    }
        //                    else
        //                    {
        //                        ImportList.Status = false;
        //                        ImportList.Result = "already exist!";
        //                    }

        //                    if (ImportList.SupplierId == "")
        //                    {
        //                        goto stopUpload3;
        //                    }
        //                    _ImportListModel.ImportCycleTime3.Add(ImportList);
        //                }

        //            }
        //        }
        //    }

        //    stopUpload3:

        //    return _ImportListModel;

        //}

    }
}