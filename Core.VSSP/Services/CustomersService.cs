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
    public class CustomersService
    {
        SystemService _SystemService = new SystemService();
        vssp_entity vssp_db = new vssp_entity();

        // GET: CustomerService
        public ImportCustomersListModel uploadCustomerExcel(HttpPostedFileBase files)
        {
            ImportCustomersListModel _ImportListModel = new ImportCustomersListModel();
            _ImportListModel.ImportCustomer = new List<ImportCustomerModel>();
            _ImportListModel.ImportCustomerContact = new List<ImportCustomerContactModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //Customer
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var customer = vssp_db.Tbl_MST_Customer;

                    bool validTemplate = true;

                    //header validator customer
                    var CustomerId = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var CustomerCode = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var AccountCode = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var CustomerName = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var Address = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var City = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var Provience = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                    var Country = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());
                    var PostalCode = _SystemService.Vf(workSheet?.Cells[1, 10]?.Value?.ToString());
                    var Websites = _SystemService.Vf(workSheet?.Cells[1, 11]?.Value?.ToString());
                    var TaxId = _SystemService.Vf(workSheet?.Cells[1, 12]?.Value?.ToString());

                    if (CustomerId.Replace(" ", "").Replace("*", "").ToLower() != "customerid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (CustomerCode.Replace(" ", "").Replace("*", "").ToLower() != "customercode")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (AccountCode.Replace(" ", "").Replace("*", "").ToLower() != "accountcode")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (CustomerName.Replace(" ", "").Replace("*", "").ToLower() != "customername")
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
                    if (validTemplate == true)
                    {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportCustomerModel ImportList = new ImportCustomerModel();

                            ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.CustomerCode = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.AccountCode = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.CustomerName = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.Address = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.City = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.Provience = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                            ImportList.Country = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                            ImportList.PostalCode = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                            ImportList.Websites = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                            ImportList.TaxId = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());

                            var exist = from a in customer
                                        where a.CustomerId == ImportList.CustomerId
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

                            if (ImportList.CustomerId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportListModel.ImportCustomer.Add(ImportList);
                        }

                    }

                    stopUpload:

                    //Customer Contact
                    workSheet = package.Workbook.Worksheets[1];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var customercontact = vssp_db.Tbl_MST_CustomerContact;

                    //header validator customer contact
                    var CustomerId2 = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var ContactName = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var Organization = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var Position = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var Phone1 = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var Phone2 = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var Fax = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                    var Email = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());
                    var ReceiveOrder = _SystemService.Vf(workSheet?.Cells[1, 10]?.Value?.ToString());

                    if (CustomerId2.Replace(" ", "").Replace("*", "").ToLower() != "customerid")
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
                    stopValidation2:

                    //get data excel
                    if (validTemplate == true)
                    {

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportCustomerContactModel ImportList = new ImportCustomerContactModel();

                            ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.ContactName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.Organization = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.Position = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.Phone1 = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.Phone2 = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.Fax = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                            ImportList.Email = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                            ImportList.ReceiveOrder = _SystemService.Vb(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());

                            var exist = from a in customercontact
                                        where a.CustomerId == ImportList.CustomerId
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

                            if (ImportList.CustomerId == "")
                            {
                                goto stopUpload2;
                            }

                            _ImportListModel.ImportCustomerContact.Add(ImportList);
                        }
                    };

                }
            }

            stopUpload2:

            return _ImportListModel;

        }
        public ImportCustomersListModel crudImportCustomerExcel(Boolean replace, string UserId, HttpPostedFileBase files)
        {
            ImportCustomersListModel _ImportListModel = new ImportCustomersListModel();
            _ImportListModel.ImportCustomer = new List<ImportCustomerModel>();
            _ImportListModel.ImportCustomerContact = new List<ImportCustomerContactModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //customer
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var customer = vssp_db.Tbl_MST_Customer;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportCustomerModel ImportList = new ImportCustomerModel();

                        ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.CustomerCode = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.AccountCode = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.CustomerName = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.Address = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        ImportList.City = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                        ImportList.Provience = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                        ImportList.Country = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                        ImportList.PostalCode = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                        ImportList.Websites = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                        ImportList.TaxId = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());

                        if (ImportList.CustomerId == "")
                        {
                            goto stopUpload;
                        }

                        var exist = from a in customer
                                    where a.CustomerId == ImportList.CustomerId
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_Customer ListCreate = new Tbl_MST_Customer();
                            ListCreate.CustomerId = ImportList.CustomerId;
                            ListCreate.CustomerCode = ImportList.CustomerCode;
                            ListCreate.AccountCode = ImportList.AccountCode;
                            ListCreate.CustomerName = ImportList.CustomerName;
                            ListCreate.Address = ImportList.Address;
                            ListCreate.City = ImportList.City;
                            ListCreate.Provience = ImportList.Provience;
                            ListCreate.Country = ImportList.Country;
                            ListCreate.PostalCode = ImportList.PostalCode;
                            ListCreate.Websites = ImportList.Websites;
                            ListCreate.TaxId = ImportList.TaxId;
                            ListCreate.Actived = true;
                            ListCreate.UserID = UserId;
                            ListCreate.EditDate = DateTime.Now;
                            vssp_db.Tbl_MST_Customer.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_Customer.First(a => a.CustomerId == ImportList.CustomerId);

                                ListUpdate.CustomerCode = ImportList.CustomerCode;
                                ListUpdate.AccountCode = ImportList.AccountCode;
                                ListUpdate.CustomerName = ImportList.CustomerName;
                                ListUpdate.Address = ImportList.Address;
                                ListUpdate.City = ImportList.City;
                                ListUpdate.Provience = ImportList.Provience;
                                ListUpdate.Country = ImportList.Country;
                                ListUpdate.PostalCode = ImportList.PostalCode;
                                ListUpdate.Websites = ImportList.Websites;
                                ListUpdate.TaxId = ImportList.TaxId;
                                ListUpdate.UserID = UserId;
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

                        _ImportListModel.ImportCustomer.Add(ImportList);
                    }

                    stopUpload:

                    //customer contact
                    workSheet = package.Workbook.Worksheets[1];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var customercontact = vssp_db.Tbl_MST_CustomerContact;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportCustomerContactModel ImportList = new ImportCustomerContactModel();

                        ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.ContactName = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.Organization = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.Position = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.Phone1 = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        ImportList.Phone2 = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                        ImportList.Fax = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                        ImportList.Email = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                        ImportList.ReceiveOrder = _SystemService.Vb(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());

                        if (ImportList.CustomerId == "")
                        {
                            goto stopUpload2;
                        }

                        var exist = from a in customercontact
                                    where a.CustomerId == ImportList.CustomerId && a.ContactName == ImportList.ContactName
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_CustomerContact ListCreate = new Tbl_MST_CustomerContact();

                            ListCreate.CustomerId = ImportList.CustomerId;
                            ListCreate.ContactName = ImportList.ContactName;
                            ListCreate.Organization = ImportList.Organization;
                            ListCreate.Position = ImportList.Position;
                            ListCreate.Phone1 = ImportList.Phone1;
                            ListCreate.Phone2 = ImportList.Phone2;
                            ListCreate.Fax = ImportList.Fax;
                            ListCreate.Email = ImportList.Email;
                            ListCreate.ReceiveOrder = ImportList.ReceiveOrder;

                            vssp_db.Tbl_MST_CustomerContact.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_CustomerContact.First(a => a.CustomerId == ImportList.CustomerId && a.ContactName == ImportList.ContactName);

                                ListUpdate.ContactName = ImportList.ContactName;
                                ListUpdate.Organization = ImportList.Organization;
                                ListUpdate.Position = ImportList.Position;
                                ListUpdate.Phone1 = ImportList.Phone1;
                                ListUpdate.Phone2 = ImportList.Phone2;
                                ListUpdate.Fax = ImportList.Fax;
                                ListUpdate.Email = ImportList.Email;
                                ListUpdate.ReceiveOrder = ImportList.ReceiveOrder;

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
                            ImportList.Result =errinfo;
                        }

                        _ImportListModel.ImportCustomerContact.Add(ImportList);
                    }
                }
            }

            stopUpload2:

            return _ImportListModel;
        }

        //FINISH GOODS
        public ImportFinishGoodsListModel uploadFinishGoodExcel(HttpPostedFileBase files, string canConfidential)
        {
            ImportFinishGoodsListModel _ImportListModel = new ImportFinishGoodsListModel();
            _ImportListModel.ImportFinishGood = new List<ImportFinishGoodModel>();
            _ImportListModel.ImportFinishGoodPrice = new List<ImportFinishGoodPriceModel>();
            _ImportListModel.ImportFinishGoodRelation = new List<ImportFinishGoodRelationModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //FinishGood
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var FinishGood = vssp_db.Tbl_MST_PartFinishGoods;

                    bool validTemplate = true;

                    //header validator FinishGood
                    var CustomerId = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var CustomerUnitModel = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var PartNumber = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var PartNumberCustomer = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var UniqueNumber = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var PartName = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var CategoryId = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());
                    var PackingId = _SystemService.Vf(workSheet?.Cells[1, 9]?.Value?.ToString());
                    var AreaId = _SystemService.Vf(workSheet?.Cells[1, 10]?.Value?.ToString());
                    var LocationId = _SystemService.Vf(workSheet?.Cells[1, 11]?.Value?.ToString());
                    var UnitLevel1 = _SystemService.Vf(workSheet?.Cells[1, 12]?.Value?.ToString());
                    var UnitLevel2 = _SystemService.Vf(workSheet?.Cells[1, 13]?.Value?.ToString());
                    var UnitQty = _SystemService.Vf(workSheet?.Cells[1, 14]?.Value?.ToString());
                    var PassThrough = _SystemService.Vf(workSheet?.Cells[1, 15]?.Value?.ToString());

                    if (CustomerId.Replace(" ", "").Replace("*", "").ToLower() != "customerid")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (CustomerUnitModel.Replace(" ", "").Replace("*", "").ToLower() != "customerunitmodel")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }

                    if (PartNumber.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                    {
                        validTemplate = false;
                        goto stopValidation;
                    }
                    if (PartNumberCustomer.Replace(" ", "").Replace("*", "").ToLower() != "partnumbercustomer")
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
                    if (PassThrough.Replace(" ", "").Replace("*", "").ToLower() != "passthrough")
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

                            ImportFinishGoodModel ImportList = new ImportFinishGoodModel();

                            ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.CustomerUnitModel = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.PartNumberCustomer = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.UniqueNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            ImportList.PartName = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                            ImportList.CategoryId = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                            ImportList.PackingId = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                            ImportList.AreaId = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                            ImportList.LocationId = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                            ImportList.UnitLevel1 = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());
                            ImportList.UnitLevel2 = _SystemService.Vf(workSheet?.Cells[rowIterator, 13]?.Value?.ToString());
                            ImportList.UnitQty = _SystemService.Vn(workSheet?.Cells[rowIterator, 14]?.Value?.ToString());
                            ImportList.PassThrough = _SystemService.Vb(workSheet?.Cells[rowIterator, 15]?.Value?.ToString());

                            var exist = from a in FinishGood
                                        where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber
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

                            if (ImportList.CustomerId == "")
                            {
                                goto stopUpload;
                            }
                            _ImportListModel.ImportFinishGood.Add(ImportList);
                        }

                    }

                    stopUpload:

                    if (canConfidential == "")
                    {
                        //FinishGood Price
                        workSheet = package.Workbook.Worksheets[1];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var FinishGoodPrice = vssp_db.Tbl_MST_PartFinishGoodsPrice;

                        //header validator FinishGood
                        var CustomerId2 = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                        var PartNumber2 = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                        var StartDate = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                        var EndDate = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                        var Price = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());

                        if (CustomerId2.Replace(" ", "").Replace("*", "").ToLower() != "customerid")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (PartNumber2.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (StartDate.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (EndDate.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
                        {
                            validTemplate = false;
                            goto stopValidation2;
                        }
                        if (Price.Replace(" ", "").Replace("*", "").ToLower() != "price")
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

                                ImportFinishGoodPriceModel ImportList = new ImportFinishGoodPriceModel();

                                ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
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

                                var exist = from a in FinishGoodPrice
                                            where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
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

                                if (ImportList.CustomerId == "")
                                {
                                    goto stopUpload2;
                                }

                                _ImportListModel.ImportFinishGoodPrice.Add(ImportList);
                            }
                        };

                    }

                    stopUpload2:

                    //FinishGood Relation
                    workSheet = package.Workbook.Worksheets[2];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var FinishGoodsRelation = vssp_db.Tbl_MST_PartFinishGoodsRelation;

                    //header validator Relation
                    var CustomerId3 = _SystemService.Vf(workSheet?.Cells[1, 2]?.Value?.ToString());
                    var PartNumber3 = _SystemService.Vf(workSheet?.Cells[1, 3]?.Value?.ToString());
                    var SupplierId = _SystemService.Vf(workSheet?.Cells[1, 4]?.Value?.ToString());
                    var PartNumberRawMaterial = _SystemService.Vf(workSheet?.Cells[1, 5]?.Value?.ToString());
                    var QtyUsage = _SystemService.Vf(workSheet?.Cells[1, 6]?.Value?.ToString());
                    var StartDate2 = _SystemService.Vf(workSheet?.Cells[1, 7]?.Value?.ToString());
                    var EndDate2 = _SystemService.Vf(workSheet?.Cells[1, 8]?.Value?.ToString());

                    if (CustomerId3.Replace(" ", "").Replace("*", "").ToLower() != "customerid")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (PartNumber3.Replace(" ", "").Replace("*", "").ToLower() != "partnumber")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (SupplierId.Replace(" ", "").Replace("*", "").ToLower() != "supplierid")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (PartNumberRawMaterial.Replace(" ", "").Replace("*", "").ToLower() != "partnumberrawmaterial")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (QtyUsage.Replace(" ", "").Replace("*", "").ToLower() != "qtyusage")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (StartDate2.Replace(" ", "").Replace("*", "").ToLower() != "startdate")
                    {
                        validTemplate = false;
                        goto stopValidation3;
                    }
                    if (EndDate2.Replace(" ", "").Replace("*", "").ToLower() != "enddate")
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

                            ImportFinishGoodRelationModel ImportList = new ImportFinishGoodRelationModel();

                            ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                            ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                            ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                            ImportList.PartNumberRawMaterial = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                            ImportList.QtyUsage = _SystemService.Vn(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                            if (workSheet?.Cells[rowIterator, 7]?.Value?.ToString() != null)
                            {
                                ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 7]?.Value?.ToString(), "yyyy-MM-dd"));
                            }
                            if (workSheet?.Cells[rowIterator, 8]?.Value?.ToString() != null)
                            {
                                ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 8]?.Value?.ToString(), "yyyy-MM-dd"));
                            }

                            var exist = from a in FinishGoodsRelation
                                        where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber && a.SupplierId == ImportList.SupplierId && a.PartNumberRawMaterial == ImportList.PartNumberRawMaterial && a.StartDate == ImportList.StartDate
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

                            if (ImportList.CustomerId == "")
                            {
                                goto stopUpload3;
                            }

                            _ImportListModel.ImportFinishGoodRelation.Add(ImportList);
                        }
                    }
                }
            }

            stopUpload3:

            return _ImportListModel;

        }

        public ImportFinishGoodsListModel crudImportFinishGoodExcel(Boolean replace, string UserId, HttpPostedFileBase files, string canConfidential)
        {
            ImportFinishGoodsListModel _ImportListModel = new ImportFinishGoodsListModel();
            _ImportListModel.ImportFinishGood = new List<ImportFinishGoodModel>();
            _ImportListModel.ImportFinishGoodPrice = new List<ImportFinishGoodPriceModel>();
            _ImportListModel.ImportFinishGoodRelation = new List<ImportFinishGoodRelationModel>();

            if ((files != null) && (files.ContentLength > 0) && !string.IsNullOrEmpty(files.FileName))
            {

                string fileName = files.FileName;
                string fileContentType = files.ContentType;
                byte[] fileBytes = new byte[files.ContentLength];
                var data = files.InputStream.Read(fileBytes, 0, Convert.ToInt32(files.ContentLength));

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(files.InputStream))
                {
                    //FinishGood
                    var workSheet = package.Workbook.Worksheets[0];
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    var FinishGood = vssp_db.Tbl_MST_PartFinishGoods;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportFinishGoodModel ImportList = new ImportFinishGoodModel();

                        ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.CustomerUnitModel = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.PartNumberCustomer = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.UniqueNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        ImportList.PartName = _SystemService.Vf(workSheet?.Cells[rowIterator, 7]?.Value?.ToString());
                        ImportList.CategoryId = _SystemService.Vf(workSheet?.Cells[rowIterator, 8]?.Value?.ToString());
                        ImportList.PackingId = _SystemService.Vf(workSheet?.Cells[rowIterator, 9]?.Value?.ToString());
                        ImportList.AreaId = _SystemService.Vf(workSheet?.Cells[rowIterator, 10]?.Value?.ToString());
                        ImportList.LocationId = _SystemService.Vf(workSheet?.Cells[rowIterator, 11]?.Value?.ToString());
                        ImportList.UnitLevel1 = _SystemService.Vf(workSheet?.Cells[rowIterator, 12]?.Value?.ToString());
                        ImportList.UnitLevel2 = _SystemService.Vf(workSheet?.Cells[rowIterator, 13]?.Value?.ToString());
                        ImportList.UnitQty = _SystemService.Vn(workSheet?.Cells[rowIterator, 14]?.Value?.ToString());
                        ImportList.PassThrough = _SystemService.Vb(workSheet?.Cells[rowIterator, 15]?.Value?.ToString());

                        if (ImportList.CustomerId == "")
                        {
                            goto stopUpload;
                        }

                        var exist = from a in FinishGood
                                    where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber
                                    select a;

                        if (exist.Count() == 0)
                        {

                            Tbl_MST_PartFinishGoods ListCreate = new Tbl_MST_PartFinishGoods();

                            ListCreate.CustomerId = ImportList.CustomerId;
                            ListCreate.CustomerUnitModel = ImportList.CustomerUnitModel;
                            ListCreate.PartNumber = ImportList.PartNumber;
                            ListCreate.PartNumberCustomer = ImportList.PartNumberCustomer;
                            ListCreate.UniqueNumber = ImportList.UniqueNumber;
                            ListCreate.PartName = ImportList.PartName;
                            ListCreate.CategoryId = ImportList.CategoryId;
                            ListCreate.PackingId = ImportList.PackingId;
                            ListCreate.AreaId = ImportList.AreaId;
                            ListCreate.LocationId = ImportList.LocationId;
                            ListCreate.UnitLevel1 = ImportList.UnitLevel1;
                            ListCreate.UnitLevel2 = ImportList.UnitLevel2;
                            ListCreate.UnitQty = ImportList.UnitQty;
                            ListCreate.LocationId = ImportList.LocationId;
                            ListCreate.PassThrough = ImportList.PassThrough;
                            ListCreate.Actived = true;
                            ListCreate.UserId = UserId;
                            ListCreate.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_PartFinishGoods.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_PartFinishGoods.First(a => a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber);

                                //ListUpdate.CustomerId           = ImportList.CustomerId;
                                ListUpdate.CustomerUnitModel = ImportList.CustomerUnitModel;
                                //ListUpdate.PartNumber           = ImportList.PartNumber;
                                ListUpdate.PartNumberCustomer = ImportList.PartNumberCustomer;
                                ListUpdate.UniqueNumber = ImportList.UniqueNumber;
                                ListUpdate.PartName = ImportList.PartName;
                                ListUpdate.CategoryId = ImportList.CategoryId;
                                ListUpdate.PackingId = ImportList.PackingId;
                                ListUpdate.AreaId = ImportList.AreaId;
                                ListUpdate.LocationId = ImportList.LocationId;
                                ListUpdate.UnitLevel1 = ImportList.UnitLevel1;
                                ListUpdate.UnitLevel2 = ImportList.UnitLevel2;
                                ListUpdate.UnitQty = ImportList.UnitQty;
                                ListUpdate.PassThrough = ImportList.PassThrough;
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

                        _ImportListModel.ImportFinishGood.Add(ImportList);
                    }

                    stopUpload:

                    if (canConfidential == "")
                    {
                        //FinishGood Price
                        workSheet = package.Workbook.Worksheets[1];
                        noOfCol = workSheet.Dimension.End.Column;
                        noOfRow = workSheet.Dimension.End.Row;
                        var FinishGoodPrice = vssp_db.Tbl_MST_PartFinishGoodsPrice;

                        for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                        {

                            ImportFinishGoodPriceModel ImportList = new ImportFinishGoodPriceModel();

                            ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
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

                            if (ImportList.CustomerId == "")
                            {
                                goto stopUpload2;
                            }

                            var exist = from a in FinishGoodPrice
                                        where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate
                                        select a;

                            if (exist.Count() == 0)
                            {

                                Tbl_MST_PartFinishGoodsPrice ListCreate = new Tbl_MST_PartFinishGoodsPrice();

                                ListCreate.CustomerId = ImportList.CustomerId;
                                ListCreate.PartNumber = ImportList.PartNumber;
                                ListCreate.StartDate = ImportList.StartDate;
                                ListCreate.EndDate = ImportList.EndDate;
                                ListCreate.Price = ImportList.Price;
                                ListCreate.UserId = UserId;
                                ListCreate.EditDate = DateTime.Now;

                                vssp_db.Tbl_MST_PartFinishGoodsPrice.Add(ListCreate);

                                ImportList.Status = true;
                                ImportList.Result = "success imported.";
                            }
                            else
                            {
                                if (replace == true)
                                {

                                    var ListUpdate = vssp_db.Tbl_MST_PartFinishGoodsPrice.First(a => a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber && a.StartDate == ImportList.StartDate);

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

                            _ImportListModel.ImportFinishGoodPrice.Add(ImportList);
                        }

                    }

                    stopUpload2:

                    //FinishGood Relation
                    workSheet = package.Workbook.Worksheets[2];
                    noOfCol = workSheet.Dimension.End.Column;
                    noOfRow = workSheet.Dimension.End.Row;
                    var FinishGoodRelation = vssp_db.Tbl_MST_PartFinishGoodsRelation;

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {

                        ImportFinishGoodRelationModel ImportList = new ImportFinishGoodRelationModel();

                        ImportList.CustomerId = _SystemService.Vf(workSheet?.Cells[rowIterator, 2]?.Value?.ToString());
                        ImportList.PartNumber = _SystemService.Vf(workSheet?.Cells[rowIterator, 3]?.Value?.ToString());
                        ImportList.SupplierId = _SystemService.Vf(workSheet?.Cells[rowIterator, 4]?.Value?.ToString());
                        ImportList.PartNumberRawMaterial = _SystemService.Vf(workSheet?.Cells[rowIterator, 5]?.Value?.ToString());
                        ImportList.QtyUsage = _SystemService.Vn(workSheet?.Cells[rowIterator, 6]?.Value?.ToString());
                        if (workSheet?.Cells[rowIterator, 7]?.Value?.ToString() != null)
                        {
                            ImportList.StartDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 7]?.Value?.ToString(), "yyyy-MM-dd"));
                        }
                        if (workSheet?.Cells[rowIterator, 8]?.Value?.ToString() != null)
                        {
                            ImportList.EndDate = Convert.ToDateTime(_SystemService.Vd(workSheet?.Cells[rowIterator, 8]?.Value?.ToString(), "yyyy-MM-dd"));
                        }

                        if (ImportList.CustomerId == "")
                        {
                            goto stopUpload3;
                        }

                        var exist = from a in FinishGoodRelation
                                    where a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber && a.SupplierId == ImportList.SupplierId && a.PartNumberRawMaterial == ImportList.PartNumberRawMaterial && a.StartDate == ImportList.StartDate
                                    select a;


                        if (exist.Count() == 0)
                        {

                            Tbl_MST_PartFinishGoodsRelation ListCreate = new Tbl_MST_PartFinishGoodsRelation();

                            ListCreate.CustomerId = ImportList.CustomerId;
                            ListCreate.PartNumber = ImportList.PartNumber;
                            ListCreate.SupplierId = ImportList.SupplierId;
                            ListCreate.PartNumberRawMaterial = ImportList.PartNumberRawMaterial;
                            ListCreate.QtyUsage = ImportList.QtyUsage;
                            ListCreate.StartDate = ImportList.StartDate;
                            ListCreate.EndDate = ImportList.EndDate;
                            ListCreate.UserId = UserId;
                            ListCreate.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_PartFinishGoodsRelation.Add(ListCreate);

                            ImportList.Status = true;
                            ImportList.Result = "success imported.";
                        }
                        else
                        {
                            if (replace == true)
                            {

                                var ListUpdate = vssp_db.Tbl_MST_PartFinishGoodsRelation.First(a => a.CustomerId == ImportList.CustomerId && a.PartNumber == ImportList.PartNumber && a.SupplierId == ImportList.SupplierId && a.PartNumberRawMaterial == ImportList.PartNumberRawMaterial && a.StartDate == ImportList.StartDate);

                                //ListUpdate.CustomerId               = ImportList.CustomerId;
                                //ListUpdate.PartNumber               = ImportList.PartNumber;
                                //ListUpdate.PartNumberRawMaterial    = ImportList.PartNumberRawMaterial;
                                ListUpdate.EndDate = ImportList.EndDate;
                                ListUpdate.QtyUsage = ImportList.QtyUsage;
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

                        _ImportListModel.ImportFinishGoodRelation.Add(ImportList);
                    }
                }
            }

            stopUpload3:

            return _ImportListModel;
        }
    }
}