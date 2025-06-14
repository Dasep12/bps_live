using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace Core.VSSP.Controllers
{
    public class CustomersController : Controller
    {
        // GET: Customer
        CryptoLibService _CryptoLibService = new CryptoLibService();
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        CustomersService _CustomersService = new CustomersService();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult List()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Customers", "List");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Customers " + _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    var stockTakingEvent = _SystemService.GetStockTakingEvent();

                    if (stockTakingEvent != null && stockTakingEvent.InventoryStatus.Contains("in progress"))
                    {
                        ViewBag.Messages = stockTakingEvent.InventoryStatus;
                        return View("../System/SystemLocked");
                    }
                    else
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";

                        return View(exportOption);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult CustomerListJson(string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Customer = (from a in vssp_db.Vw_MST_Customer
                            where (a.CustomerId.Contains(searchFilter) || a.CustomerName.Contains(searchFilter)) && a.Actived == isActive
                            orderby a.CustomerId
                            select new { a.CustomerId, a.CustomerCode, a.AccountCode, a.CustomerName, a.Address, a.City, a.Provience, a.Country, a.PostalCode, a.TaxId, a.Websites, 
                                a.ContactName,a.Phone,a.Email,
                                a.Actived, a.Logo, a.UserID, a.EditDate }).ToList();

            var jsonResult = Json(Customer, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }
        public ActionResult CustomerContactListJson(string customerid)
        {
            customerid = _SystemService.Vf(customerid);
            var contact = from a in vssp_db.Tbl_MST_CustomerContact
                          where a.CustomerId == customerid
                          orderby a.CustomerId
                          select new { ContactId = a.ContactName, a.CustomerId, a.ContactName, a.Organization, a.Position, a.Phone1, a.Phone2, a.Fax, a.Email, a.ReceiveOrder };
            return Json(contact, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FinishGoods()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Customers", "FinishGoods");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    var stockTakingEvent = _SystemService.GetStockTakingEvent();

                    if (stockTakingEvent != null && stockTakingEvent.InventoryStatus.Contains("in progress"))
                    {
                        ViewBag.Messages = stockTakingEvent.InventoryStatus;
                        return View("../System/SystemLocked");
                    }
                    else
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";

                        return View(exportOption);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult FinishGoodsListJson(
                            string searchFilter, string FormAction = "", string CustomerId = "",
                            string PartNumber = "", string UniqueNumber = "", string UniqueNotInclude = null,
                            string PartNotInclude = null, string PassThrough = null, string SONumber = "", bool noprice = false, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            if (searchFilter != "validator")
            {

                var FinishGoods = (from a in vssp_db.Vw_MST_PartFinishGoods
                                  where a.Actived == isActive && (a.CustomerId.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.CustomerUnitModel.Contains(searchFilter))
                                  orderby a.CustomerId, a.PartNumber
                                  select new { a.FinishGoodKey, a.CustomerId, a.CustomerName, a.CustomerUnitModel, a.PartNumber, a.PartNumberCustomer, a.UniqueNumber, a.PartName, a.CategoryId, a.PackingId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.PassThrough, a.Actived, IsActived = a.Actived, a.UserId, a.EditDate }).ToList();

                if (noprice)
                {
                    FinishGoods = FinishGoods.Where(a => a.Price == null).ToList();
                }
                if (_SystemService.Vf(CustomerId) != "")
                {
                    FinishGoods = FinishGoods.Where(a => a.CustomerId == CustomerId).ToList();
                }
                if (_SystemService.Vf(PassThrough) != "")
                {
                    var _passThrough = _SystemService.Vb(PassThrough);
                    FinishGoods = FinishGoods.Where(a => a.PassThrough == _passThrough).ToList();
                }

                if (UniqueNotInclude != null)
                {
                    var exceptionList = new List<string>();
                    JsonTextReader reader = new JsonTextReader(new StringReader(UniqueNotInclude));
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.TokenType.ToString() == "String")
                            {
                                exceptionList.Add(reader.Value.ToString());
                            }
                        }
                    }
                    FinishGoods = FinishGoods.Where(a => !exceptionList.Contains(a.UniqueNumber)).ToList();
                }
                if (PartNotInclude != null)
                {
                    var exceptionList = new List<string>();
                    JsonTextReader reader = new JsonTextReader(new StringReader(PartNotInclude));
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.TokenType.ToString() == "String")
                            {
                                exceptionList.Add(reader.Value.ToString());
                            }
                        }
                    }
                    FinishGoods = FinishGoods.Where(a => !exceptionList.Contains(a.PartNumber)).ToList();
                }
                if (_SystemService.Vf(SONumber) != "")
                {
                    List<Tbl_TRS_SalesOrderDetail> _salesorder = vssp_db.Tbl_TRS_SalesOrderDetail.Where(a => a.SONumber == SONumber).ToList();
                    var acceptionList = new List<string>();
                    foreach(var sales in _salesorder)
                    {
                        acceptionList.Add(sales.PartNumber);
                    }
                    FinishGoods = FinishGoods.Where(a => acceptionList.Contains(a.PartNumber)).ToList();
                }
                return Json(FinishGoods, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var FinishGoods = new object();

                switch (FormAction)
                {
                    case "Create":

                        FinishGoods = (from a in vssp_db.Vw_MST_PartFinishGoods
                                      where a.CustomerId == CustomerId && (a.PartNumber == PartNumber || a.PartNumber == PartNumber)
                                      orderby a.CustomerId, a.PartNumber
                                      select new { a.FinishGoodKey, a.CustomerId, a.CustomerName, a.CustomerUnitModel, a.PartNumber, a.PartNumberCustomer, a.UniqueNumber, a.PartName, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.PassThrough, a.Actived, a.UserId, a.EditDate }).ToList();
                        break;

                    case "Update":

                        FinishGoods = (from a in vssp_db.Vw_MST_PartFinishGoods
                                      where a.PartNumber != PartNumber && a.CustomerId == CustomerId && a.PartNumber == PartNumber
                                       orderby a.CustomerId, a.PartNumber
                                      select new { a.FinishGoodKey, a.CustomerId, a.CustomerName, a.CustomerUnitModel, a.PartNumber, a.PartNumberCustomer, a.UniqueNumber, a.PartName, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.PassThrough, a.Actived, a.UserId, a.EditDate }).ToList();
                        break;

                    default:

                        FinishGoods = (from a in vssp_db.Vw_MST_PartFinishGoods
                                      where a.CustomerId == "*"
                                      orderby a.CustomerId, a.PartNumber
                                      select new { a.FinishGoodKey, a.CustomerId, a.CustomerName, a.CustomerUnitModel, a.PartNumber, a.PartNumberCustomer, a.UniqueNumber, a.PartName, a.CategoryId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.PassThrough, a.Actived, a.UserId, a.EditDate }).ToList();

                        break;
                }

                return Json(FinishGoods, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult FinishGoodsPriceListJson(string customerid, string partnumber)
        {
            var FinishGoodsPrice = (from a in vssp_db.Tbl_MST_PartFinishGoodsPrice
                                   where a.CustomerId == customerid && a.PartNumber == partnumber
                                   orderby a.CustomerId, a.PartNumber, a.StartDate descending
                                   select new { a.CustomerId, a.PartNumber, a.StartDate, a.EndDate, a.Price, a.UserId, a.EditDate }).ToList();
            return Json(FinishGoodsPrice, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FinishGoodsRelationListJson(string customerid, string partnumber)
        {            
            var FinishGoodsRelation = (from a in vssp_db.Tbl_MST_PartFinishGoodsRelation
                                      where a.CustomerId == customerid && a.PartNumber == partnumber
                                      orderby a.CustomerId, a.PartNumber
                                      select new { a.CustomerId, a.PartNumber, a.SupplierId, a.PartNumberRawMaterial, a.StartDate, a.EndDate, a.QtyUsage, a.UserId, a.EditDate }).ToList();
            return Json(FinishGoodsRelation, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ImportJson(string formaction, string canConfidential)
        {
            if (formaction == "Customer")
            {
                ImportCustomersListModel List = new ImportCustomersListModel();
                return Json(List, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "Customer-validation")
            {
                HttpFileCollectionBase files = Request.Files;
                var ListUpload = _CustomersService.uploadCustomerExcel(files[0]);
                return Json(ListUpload, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "FinishGood")
            {
                ImportFinishGoodModel FinishGoods = new ImportFinishGoodModel();
                return Json(FinishGoods, JsonRequestBehavior.AllowGet);
            }
            else
            if (formaction == "FinishGood-validation")
            {
                HttpFileCollectionBase files = Request.Files;
                var FinishGoodsUpload = _CustomersService.uploadFinishGoodExcel(files[0], canConfidential);
                return Json(FinishGoodsUpload, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error! No Valid Action", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult crudImportJson(Boolean replace, string formaction, string canConfidential)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    if (formaction == "Customer")
                    {
                        string userId = Session["UserID"].ToString();
                        HttpFileCollectionBase files = Request.Files;
                        var ListUpload = _CustomersService.crudImportCustomerExcel(replace, userId, files[0]);
                        return Json(ListUpload, JsonRequestBehavior.AllowGet);
                    }
                    else
                    if (formaction == "FinishGood")
                    {
                        string userId = Session["UserID"].ToString();
                        HttpFileCollectionBase files = Request.Files;
                        var ListUpload = _CustomersService.crudImportFinishGoodExcel(replace, userId, files[0], canConfidential);
                        return Json(ListUpload, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error! No Valid Action", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }

        }
        public ActionResult crudCustomerList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();
                    HttpFileCollectionBase files = Request.Files;
                    HttpPostedFileBase file = null;
                    for (int i = 0; i < files.Count; i++)
                    {
                        file = files[i];
                    }

                    PostCustomerModel postcustomer = JsonConvert.DeserializeObject<PostCustomerModel>(jsonData);
                    Tbl_MST_Customer customer = postcustomer.customer;
                    List<crud_CustomerContact> customerContact = postcustomer.customercontact;
                    string formAction = postcustomer.formAction.ToLower();

                    if (file != null)
                    {
                        customer.Logo = _SystemService.ConvertToBytes(file);
                    }

                    Tbl_MST_Customer ListCustomer = new Tbl_MST_Customer();
                    ListCustomer.CustomerId = customer.CustomerId;
                    ListCustomer.CustomerCode = customer.CustomerCode;
                    ListCustomer.AccountCode = customer.AccountCode;
                    ListCustomer.CustomerName = customer.CustomerName;
                    ListCustomer.Address = customer.Address;
                    ListCustomer.City = customer.City;
                    ListCustomer.Provience = customer.Provience;
                    ListCustomer.Country = customer.Country;
                    ListCustomer.PostalCode = customer.PostalCode;
                    ListCustomer.Websites = customer.Websites;
                    ListCustomer.TaxId = customer.TaxId;
                    ListCustomer.Logo = customer.Logo;
                    ListCustomer.Actived = customer.Actived;
                    ListCustomer.UserID = uid;
                    ListCustomer.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_Customer.Add(ListCustomer);

                            /* crud contacts */
                            crudCustomerContact(customerContact, customer.CustomerId);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_Customer.First(a => a.CustomerId == customer.CustomerId);

                            ListUpdate.CustomerCode = customer.CustomerCode;
                            ListUpdate.AccountCode = customer.AccountCode;
                            ListUpdate.CustomerName = customer.CustomerName;
                            ListUpdate.Address = customer.Address;
                            ListUpdate.City = customer.City;
                            ListUpdate.Provience = customer.Provience;
                            ListUpdate.Country = customer.Country;
                            ListUpdate.PostalCode = customer.PostalCode;
                            ListUpdate.Websites = customer.Websites;
                            ListUpdate.TaxId = customer.TaxId;
                            ListUpdate.Actived = customer.Actived;
                            if (customer.Logo != null)
                            {
                                ListUpdate.Logo = customer.Logo;
                            }
                            ListUpdate.UserID = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud contacts */
                            crudCustomerContact(customerContact, customer.CustomerId);

                            break;

                        case "delete":

                            var ContactDelete = from a in vssp_db.Tbl_MST_CustomerContact
                                                where a.CustomerId == customer.CustomerId
                                                select a;

                            foreach (var contact in ContactDelete)
                            {
                                vssp_db.Tbl_MST_CustomerContact.Remove(contact);
                            }

                            var ListDelete = vssp_db.Tbl_MST_Customer.First(a => a.CustomerId == customer.CustomerId);
                            vssp_db.Tbl_MST_Customer.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(customer, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public void crudCustomerContact(List<crud_CustomerContact> customerContacts, string CustomerId)
        {

            foreach (var contacts in customerContacts)
            {
                if (contacts.RowStatus != null)
                {
                    switch (contacts.RowStatus.ToLower())
                    {
                        case "create":

                            /* create contacts */
                            Tbl_MST_CustomerContact ListContact = new Tbl_MST_CustomerContact();
                            ListContact.CustomerId = CustomerId;
                            ListContact.ContactName = contacts.ContactName;
                            ListContact.Organization = contacts.Organization;
                            ListContact.Position = contacts.Position;
                            ListContact.Phone1 = contacts.Phone1;
                            ListContact.Phone2 = contacts.Phone2;
                            ListContact.Fax = contacts.Fax;
                            ListContact.Email = contacts.Email;
                            ListContact.ReceiveOrder = contacts.ReceiveOrder;

                            vssp_db.Tbl_MST_CustomerContact.Add(ListContact);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_CustomerContact.First(a => a.CustomerId == CustomerId && a.ContactName == contacts.ContactId);

                            ListUpdate.ContactName = contacts.ContactName;
                            ListUpdate.Organization = contacts.Organization;
                            ListUpdate.Position = contacts.Position;
                            ListUpdate.Phone1 = contacts.Phone1;
                            ListUpdate.Phone2 = contacts.Phone2;
                            ListUpdate.Fax = contacts.Fax;
                            ListUpdate.Email = contacts.Email;
                            ListUpdate.ReceiveOrder = contacts.ReceiveOrder;

                            break;

                        case "delete":

                            var ListDelete = vssp_db.Tbl_MST_CustomerContact.First(a => a.CustomerId == CustomerId && a.ContactName == contacts.ContactId);

                            vssp_db.Tbl_MST_CustomerContact.Remove(ListDelete);

                            break;
                    }
                }
            }

        }
        public ActionResult crudFinishGoodList(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostFinishGoodModel postFinishGood = JsonConvert.DeserializeObject<PostFinishGoodModel>(jsonData);
                    Tbl_MST_PartFinishGoods FinishGood = postFinishGood.FinishGood;
                    List<crud_PartFinishGoodsPrice> FinishGoodPrice = postFinishGood.FinishGoodPrice;
                    List<crud_PartFinishGoodsRelation> FinishGoodRelation = postFinishGood.FinishGoodRelation;
                    string formAction = postFinishGood.formAction.ToLower();

                    Tbl_MST_PartFinishGoods ListFinishGood = new Tbl_MST_PartFinishGoods();
                    ListFinishGood.CustomerId = FinishGood.CustomerId;
                    ListFinishGood.CustomerUnitModel = FinishGood.CustomerUnitModel;
                    ListFinishGood.PartNumber = FinishGood.PartNumber;
                    ListFinishGood.PartNumberCustomer = FinishGood.PartNumberCustomer;
                    ListFinishGood.UniqueNumber = FinishGood.UniqueNumber;
                    ListFinishGood.PartName = FinishGood.PartName;
                    ListFinishGood.CategoryId = FinishGood.CategoryId;
                    ListFinishGood.PackingId = FinishGood.PackingId;
                    ListFinishGood.AreaId = FinishGood.AreaId;
                    ListFinishGood.LocationId = FinishGood.LocationId;
                    ListFinishGood.UnitLevel1 = FinishGood.UnitLevel1;
                    ListFinishGood.UnitLevel2 = FinishGood.UnitLevel2;
                    ListFinishGood.UnitQty = FinishGood.UnitQty;
                    ListFinishGood.PassThrough = FinishGood.PassThrough;
                    ListFinishGood.Actived = FinishGood.Actived;
                    ListFinishGood.UserId = uid;
                    ListFinishGood.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            FinishGood.PartNumber = _SystemService.Vf(FinishGood.PartNumber);
                            FinishGood.PartNumberCustomer = _SystemService.Vf(FinishGood.PartNumberCustomer);
                            FinishGood.UniqueNumber = _SystemService.Vf(FinishGood.UniqueNumber);

                            ListFinishGood.PartNumber =  FinishGood.PartNumber;
                            ListFinishGood.PartNumberCustomer = FinishGood.PartNumberCustomer;
                            ListFinishGood.UniqueNumber = FinishGood.UniqueNumber;

                            vssp_db.Tbl_MST_PartFinishGoods.Add(ListFinishGood);

                            /* crud Prices */
                            crudFinishGoodPrice(FinishGoodPrice, FinishGood.CustomerId, FinishGood.PartNumber, uid);

                            /* crud relations */
                            crudFinishGoodRelation(FinishGoodRelation, FinishGood.CustomerId, FinishGood.PartNumber, uid);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_PartFinishGoods.First(a => a.CustomerId == FinishGood.CustomerId && a.PartNumber == FinishGood.PartNumber);

                            ListUpdate.CustomerId = FinishGood.CustomerId;
                            ListUpdate.CustomerUnitModel = FinishGood.CustomerUnitModel;
                            ListUpdate.PartNumber = FinishGood.PartNumber;
                            ListUpdate.PartNumberCustomer = FinishGood.PartNumberCustomer;
                            ListUpdate.UniqueNumber = FinishGood.UniqueNumber;
                            ListUpdate.PartName = FinishGood.PartName;
                            ListUpdate.CategoryId = FinishGood.CategoryId;
                            ListUpdate.PackingId = FinishGood.PackingId;
                            ListUpdate.AreaId = FinishGood.AreaId;
                            ListUpdate.LocationId = FinishGood.LocationId;
                            ListUpdate.UnitLevel1 = FinishGood.UnitLevel1;
                            ListUpdate.UnitLevel2 = FinishGood.UnitLevel2;
                            ListUpdate.UnitQty = FinishGood.UnitQty;
                            ListUpdate.PassThrough = FinishGood.PassThrough;
                            ListUpdate.Actived = FinishGood.Actived;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Prices */
                            crudFinishGoodPrice(FinishGoodPrice, FinishGood.CustomerId, FinishGood.PartNumber, uid);

                            /* crud relations */
                            crudFinishGoodRelation(FinishGoodRelation, FinishGood.CustomerId, FinishGood.PartNumber, uid);

                            break;

                        case "delete":


                            /* remove existing Stock */
                            var deleteStock = from a in vssp_db.Tbl_TRS_StockFG
                                              where a.CustomerId == FinishGood.CustomerId && a.PartNumber == FinishGood.PartNumber
                                              select a;

                            deleteStock.ForEach(Stock =>
                            {
                                vssp_db.Tbl_TRS_StockFG.Remove(Stock);
                            });

                            /* remove existing Prices */
                            var deletePrice = from a in vssp_db.Tbl_MST_PartFinishGoodsPrice
                                              where a.CustomerId == FinishGood.CustomerId && a.PartNumber == FinishGood.PartNumber
                                              select a;

                            deletePrice.ForEach(Prices =>
                            {
                                vssp_db.Tbl_MST_PartFinishGoodsPrice.Remove(Prices);
                            });

                            /* remove existing Relations */
                            var deleteRelation = from a in vssp_db.Tbl_MST_PartFinishGoodsRelation
                                                 where a.CustomerId == FinishGood.CustomerId && a.PartNumber == FinishGood.PartNumber
                                                 select a;

                            deleteRelation.ForEach(Relations =>
                            {
                                vssp_db.Tbl_MST_PartFinishGoodsRelation.Remove(Relations);
                            });

                            /* remove existing Part Finish Good */
                            var ListDelete = vssp_db.Tbl_MST_PartFinishGoods.First(a => a.CustomerId == FinishGood.CustomerId && a.PartNumber == FinishGood.PartNumber);

                            vssp_db.Tbl_MST_PartFinishGoods.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        if (formAction.ToLower() != "delete")
                        {
                            var result = vssp_db.Vw_MST_PartFinishGoods.Where(a => a.CustomerId == FinishGood.CustomerId && a.PartNumber == FinishGood.PartNumber).FirstOrDefault();
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(FinishGood, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public void crudFinishGoodPrice(List<crud_PartFinishGoodsPrice> FinishGoodPrices, string CustomerId, string PartNumber, string uid)
        {

            foreach (var Prices in FinishGoodPrices)
            {
                if (Prices.RowStatus != null)
                {
                    switch (Prices.RowStatus.ToLower())
                    {
                        case "create":

                            /* create Prices */
                            Tbl_MST_PartFinishGoodsPrice ListPrice = new Tbl_MST_PartFinishGoodsPrice();
                            ListPrice.CustomerId = CustomerId;
                            ListPrice.PartNumber = PartNumber;
                            ListPrice.StartDate = Prices.StartDate;
                            ListPrice.EndDate = Prices.EndDate;
                            ListPrice.Price = Prices.Price;
                            ListPrice.UserId = uid;
                            ListPrice.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_PartFinishGoodsPrice.Add(ListPrice);

                            break;

                        case "update":

                            /* update Prices */
                            var ListUpdate = vssp_db.Tbl_MST_PartFinishGoodsPrice.First(a => a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.StartDate == Prices.StartDate);

                            ListUpdate.EndDate = Prices.EndDate;
                            ListUpdate.Price = Prices.Price;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* delete Prices */
                            var ListDelete = vssp_db.Tbl_MST_PartFinishGoodsPrice.First(a => a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.StartDate == Prices.StartDate);

                            vssp_db.Tbl_MST_PartFinishGoodsPrice.Remove(ListDelete);

                            break;
                    }
                }
            }
        }
        public void crudFinishGoodRelation(List<crud_PartFinishGoodsRelation> FinishGoodRelations, string CustomerId, string PartNumber, string uid)
        {

            foreach (var Relations in FinishGoodRelations)
            {
                if (Relations.RowStatus != null)
                {
                    switch (Relations.RowStatus.ToLower())
                    {
                        case "create":


                            /* create Relations */
                            Tbl_MST_PartFinishGoodsRelation ListRelation = new Tbl_MST_PartFinishGoodsRelation();
                            ListRelation.CustomerId = CustomerId;
                            ListRelation.PartNumber = PartNumber;
                            ListRelation.SupplierId = Relations.SupplierId;
                            ListRelation.PartNumberRawMaterial = Relations.PartNumberRawMaterial;
                            ListRelation.QtyUsage = Relations.QtyUsage;
                            ListRelation.StartDate = Relations.StartDate;
                            ListRelation.EndDate = Relations.EndDate;
                            ListRelation.UserId = uid;
                            ListRelation.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_PartFinishGoodsRelation.Add(ListRelation);

                            break;

                        case "update":

                            /* update Relations */
                            var ListUpdate = vssp_db.Tbl_MST_PartFinishGoodsRelation.First(a => a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.SupplierId == Relations.SupplierId && a.PartNumberRawMaterial == Relations.PartNumberRawMaterial && a.StartDate == Relations.StartDate);

                            ListUpdate.QtyUsage = Relations.QtyUsage;
                            ListUpdate.EndDate = Relations.EndDate;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* delete Relations */
                            var ListDelete = vssp_db.Tbl_MST_PartFinishGoodsRelation.First(a => a.CustomerId == CustomerId && a.PartNumber == PartNumber && a.SupplierId == Relations.SupplierId && a.PartNumberRawMaterial == Relations.PartNumberRawMaterial && a.StartDate == Relations.StartDate);

                            vssp_db.Tbl_MST_PartFinishGoodsRelation.Remove(ListDelete);

                            break;
                    }
                }
            }
        }
        public ActionResult KanbanSetting()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Customers", "KanbanSetting");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Customers " + _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    var stockTakingEvent = _SystemService.GetStockTakingEvent();

                    if (stockTakingEvent != null && stockTakingEvent.InventoryStatus.Contains("in progress"))
                    {
                        ViewBag.Messages = stockTakingEvent.InventoryStatus;
                        return View("../System/SystemLocked");
                    }
                    else
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";

                        return View(exportOption);
                    }
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult KanbanSettingJson(string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            var Customer = (from a in vssp_db.Tbl_MST_KanbanSetting
                            join b in vssp_db.Tbl_MST_Customer on a.CustomerId equals b.CustomerId into cust
                            from b in cust.DefaultIfEmpty()
                            where (a.CustomerId.Contains(searchFilter) || b.CustomerName.Contains(searchFilter)) && b.Actived == isActive
                            orderby a.CustomerId
                            select new { a.CustomerId, b.CustomerName, a.SalesOrder, a.Production, a.DeliveryOrder, a.DataSparator, b.Actived, a.UserId, a.EditDate }).ToList();

            return Json(Customer, JsonRequestBehavior.AllowGet);
        }
        public ActionResult KanbanSettingSequenceJson(string customerid)
        {
            customerid = _SystemService.Vf(customerid);
            var contact = from a in vssp_db.Tbl_MST_KanbanSettingSequence
                          where a.CustomerId == customerid
                          orderby a.CustomerId
                          select new { a.CustomerId, a.SequenceNumber, a.FieldName, a.Active, a.Remark };
            return Json(contact, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudKanbanSetting(string jsonData)
        {
            if (Session["UserID"] != null)
            {

                try
                {
                    string uid = Session["UserID"].ToString();

                    PostKanbanSettingModel postKanbanSetting = JsonConvert.DeserializeObject<PostKanbanSettingModel>(jsonData);
                    Tbl_MST_KanbanSetting KanbanSetting = postKanbanSetting.kanbansetting;
                    List<Tbl_MST_KanbanSettingSequence> KanbanSettingSequence = postKanbanSetting.kanbansettingsequence;
                    string formAction = postKanbanSetting.formAction.ToLower();

                    Tbl_MST_KanbanSetting ListKanbanSetting = new Tbl_MST_KanbanSetting();
                    ListKanbanSetting.CustomerId = KanbanSetting.CustomerId;
                    ListKanbanSetting.DataSparator = KanbanSetting.DataSparator;
                    ListKanbanSetting.SalesOrder = KanbanSetting.SalesOrder;
                    ListKanbanSetting.Production = KanbanSetting.Production;
                    ListKanbanSetting.DeliveryOrder = KanbanSetting.DeliveryOrder;
                    ListKanbanSetting.UserId = uid;
                    ListKanbanSetting.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "create":

                            vssp_db.Tbl_MST_KanbanSetting.Add(ListKanbanSetting);

                            /* crud Sequences */
                            crudKanbanSettingSequence(KanbanSettingSequence, KanbanSetting.CustomerId, formAction);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_KanbanSetting.First(a => a.CustomerId == KanbanSetting.CustomerId);

                            ListUpdate.DataSparator = KanbanSetting.DataSparator;
                            ListUpdate.SalesOrder = KanbanSetting.SalesOrder;
                            ListUpdate.Production = KanbanSetting.Production;
                            ListUpdate.DeliveryOrder = KanbanSetting.DeliveryOrder;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* crud Sequences */
                            crudKanbanSettingSequence(KanbanSettingSequence, KanbanSetting.CustomerId, formAction);

                            break;

                        case "delete":

                            /* crud Sequences */
                            crudKanbanSettingSequence(KanbanSettingSequence, KanbanSetting.CustomerId, formAction);

                            var ListDelete = vssp_db.Tbl_MST_KanbanSetting.First(a => a.CustomerId == KanbanSetting.CustomerId);
                            vssp_db.Tbl_MST_KanbanSetting.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(KanbanSetting.CustomerId, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public void crudKanbanSettingSequence(List<Tbl_MST_KanbanSettingSequence> KanbanSettingSequences, string CustomerId, string action)
        {
            foreach (var Sequences in KanbanSettingSequences)
            {
                if (action != null)
                {
                    /* create Old Sequences */
                    var SequenceDelete = from a in vssp_db.Tbl_MST_KanbanSettingSequence
                                            where a.CustomerId == CustomerId
                                            select a;

                    foreach (var Sequence in SequenceDelete)
                    {
                        vssp_db.Tbl_MST_KanbanSettingSequence.Remove(Sequence);
                    }

                    switch (action.ToLower())
                    {
                        case "create":

                            /* create Sequences */
                            Tbl_MST_KanbanSettingSequence ListSequence = new Tbl_MST_KanbanSettingSequence();
                            ListSequence.CustomerId = CustomerId;
                            ListSequence.SequenceNumber = Sequences.SequenceNumber;
                            ListSequence.FieldName = Sequences.FieldName;
                            ListSequence.Active = Sequences.Active;
                            ListSequence.Remark = Sequences.Remark;

                            vssp_db.Tbl_MST_KanbanSettingSequence.Add(ListSequence);

                            break;

                        case "update":

                            /* create Sequences */
                            Tbl_MST_KanbanSettingSequence ListUpdate = new Tbl_MST_KanbanSettingSequence();
                            ListUpdate.CustomerId = CustomerId;
                            ListUpdate.SequenceNumber = Sequences.SequenceNumber;
                            ListUpdate.FieldName = Sequences.FieldName;
                            ListUpdate.Active = Sequences.Active;
                            ListUpdate.Remark = Sequences.Remark;

                            vssp_db.Tbl_MST_KanbanSettingSequence.Add(ListUpdate);

                            break;

                    }
                }
            }
        }
    }
}