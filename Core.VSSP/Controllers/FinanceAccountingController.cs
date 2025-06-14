using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Newtonsoft.Json;

namespace Core.VSSP.Controllers
{
    public class FinanceAccountingController : Controller
    {
        // GET: Accounting
        SystemService systemService = new SystemService();
        AccountService accountService = new AccountService();
        CryptoLibService cryptoLibService = new CryptoLibService();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult MasterData()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = accountService.AccessPreviliege(uid, "FinanceAccounting", "MasterData");
                var menu = systemService.SidebarEditList(acccessPreviliege.MenuID == null ? "" : acccessPreviliege.MenuID).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Account Payable " + systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");

                    Session["Layout"] = "portal";
                    var stockTakingEvent = systemService.GetStockTakingEvent();

                    if (stockTakingEvent == null) { 
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";
                    }
                
                    return View();
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult BankListJson(string BankId, string filterData)
        {

            BankId = systemService.Vf(BankId);
            var bank = (from a in vssp_db.Tbl_MST_Bank
                        select new { a.BankId, a.BankName, a.TransferFee, a.Remarks, a.UserId, a.EditDate }).ToList();

            if (systemService.Vf(BankId) != "")
            {
                bank = bank.Where(a => a.BankId == BankId).ToList();
            }
            if (systemService.Vf(filterData) != "")
            {
                bank = bank.Where(a => a.BankId.Contains(filterData) || a.BankName.Contains(filterData) || a.Remarks.Contains(filterData)).ToList();
            }
            bank = bank.OrderBy(a => a.BankId).ToList();

            return Json(bank, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ClassificationListJson(string ClassificationId, string filterData)
        {

            ClassificationId = systemService.Vf(ClassificationId);
            var Classification = from a in vssp_db.Tbl_MST_AccountingClassification
                          select new { a.ClassificationId, a.ClassificationName, a.PPN, a.PPH23, a.Remarks, a.UserId, a.EditDate };

            if (systemService.Vf(ClassificationId) != "")
            {
                Classification = Classification.Where(a => a.ClassificationId == ClassificationId);
            }
            if (systemService.Vf(filterData) != "")
            {
                Classification = Classification.Where(a => a.ClassificationId.Contains(filterData) || a.ClassificationName.Contains(filterData) || a.Remarks.Contains(filterData));
            }
            Classification = Classification.OrderBy(a => a.ClassificationId);

            return Json(Classification, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PaymentListJson(string PaymentId, string filterData)
        {

            PaymentId = systemService.Vf(PaymentId);
            var Payment = from a in vssp_db.Tbl_MST_AccountingPayment
                          select new { a.PaymentId, a.PaymentName, a.Remarks, a.UserId, a.EditDate };

            if (systemService.Vf(PaymentId) != "")
            {
                Payment = Payment.Where(a => a.PaymentId == PaymentId);
            }
            if (systemService.Vf(filterData) != "")
            {
                Payment = Payment.Where(a => a.PaymentId.Contains(filterData) || a.PaymentName.Contains(filterData) || a.Remarks.Contains(filterData));
            }
            Payment = Payment.OrderBy(a => a.PaymentId);

            return Json(Payment, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CategoryListJson(string CategoryId, string filterData)
        {

            CategoryId = systemService.Vf(CategoryId);
            var Category = from a in vssp_db.Tbl_MST_AccountingCategory
                       select new { a.CategoryId, a.CategoryName, a.Remarks, a.UserId, a.EditDate };

            if (systemService.Vf(CategoryId) != "")
            {
                Category = Category.Where(a => a.CategoryId == CategoryId);
            }
            if (systemService.Vf(filterData) != "")
            {
                Category = Category.Where(a => a.CategoryId.Contains(filterData) || a.CategoryName.Contains(filterData) || a.Remarks.Contains(filterData));
            }
            Category = Category.OrderBy(a => a.CategoryId);

            return Json(Category, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BankAccountListJson(string BankId, string filterData)
        {

            BankId = systemService.Vf(BankId);
            var Bank = from a in vssp_db.Tbl_MST_AccountingBankAccount
                       join b in vssp_db.Tbl_MST_Bank on a.BankId equals b.BankId into bank
                       from b in bank.DefaultIfEmpty()
                       select new { a.BankId, b.BankName, a.Branch, a.AccountName, a.AccountNumber, a.StartDate,a.EndDate, a.UserId, a.EditDate };

            if (systemService.Vf(BankId) != "")
            {
                Bank = Bank.Where(a => a.BankId == BankId);
            }
            if (systemService.Vf(filterData) != "")
            {
                Bank = Bank.Where(a => a.BankId.Contains(filterData) || a.BankName.Contains(filterData) || a.Branch.Contains(filterData));
            }
            Bank = Bank.OrderByDescending(a => a.StartDate);

            return Json(Bank, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudBankList(string jsonData)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    string uid = Session["UserID"].ToString();

                    PostBankModel postBank = JsonConvert.DeserializeObject<PostBankModel>(jsonData);
                    Tbl_MST_Bank bank = postBank.Bank;
                    string formAction = postBank.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":

                            Tbl_MST_Bank ListBank = new Tbl_MST_Bank();
                            ListBank.BankId = bank.BankId;
                            ListBank.BankName = bank.BankName;
                            ListBank.TransferFee = bank.TransferFee;
                            ListBank.Remarks = bank.Remarks;
                            ListBank.UserId = uid;
                            ListBank.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_Bank.Add(ListBank);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_Bank.First(a => a.BankId == bank.BankId);

                            ListUpdate.BankName = bank.BankName;
                            ListUpdate.TransferFee = bank.TransferFee;
                            ListUpdate.Remarks = bank.Remarks;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* remove existing Bank */
                            var ListDelete = vssp_db.Tbl_MST_Bank.First(a => a.BankId == bank.BankId);

                            vssp_db.Tbl_MST_Bank.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(bank, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = systemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult crudClassificationList(string jsonData)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    string uid = Session["UserID"].ToString();

                    PostClassificationModel postClassification = JsonConvert.DeserializeObject<PostClassificationModel>(jsonData);
                    Tbl_MST_AccountingClassification Classification = postClassification.Classification;
                    string formAction = postClassification.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":

                            Tbl_MST_AccountingClassification ListClassification = new Tbl_MST_AccountingClassification();
                            ListClassification.ClassificationId = Classification.ClassificationId;
                            ListClassification.ClassificationName = Classification.ClassificationName;
                            ListClassification.PPN = Classification.PPN;
                            ListClassification.PPH23 = Classification.PPH23;
                            ListClassification.Remarks = Classification.Remarks;
                            ListClassification.UserId = uid;
                            ListClassification.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_AccountingClassification.Add(ListClassification);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_AccountingClassification.First(a => a.ClassificationId == Classification.ClassificationId);

                            ListUpdate.ClassificationName = Classification.ClassificationName;
                            ListUpdate.PPN = Classification.PPN;
                            ListUpdate.PPH23 = Classification.PPH23;
                            ListUpdate.Remarks = Classification.Remarks;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* remove existing Classification */
                            var ListDelete = vssp_db.Tbl_MST_AccountingClassification.First(a => a.ClassificationId == Classification.ClassificationId);

                            vssp_db.Tbl_MST_AccountingClassification.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(Classification, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = systemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult crudPaymentList(string jsonData)
        {
            try
            {

                PostPaymentModel postPayment = JsonConvert.DeserializeObject<PostPaymentModel>(jsonData);
                Tbl_MST_AccountingPayment Payment = postPayment.Payment;
                string uid = postPayment.uid;
                string formAction = postPayment.formAction.ToLower();

                switch (formAction)
                {
                    case "create":

                        Tbl_MST_AccountingPayment ListPayment = new Tbl_MST_AccountingPayment();
                        ListPayment.PaymentId = Payment.PaymentId;
                        ListPayment.PaymentName = Payment.PaymentName;
                        ListPayment.Remarks = Payment.Remarks;
                        ListPayment.UserId = uid;
                        ListPayment.EditDate = DateTime.Now;

                        vssp_db.Tbl_MST_AccountingPayment.Add(ListPayment);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_MST_AccountingPayment.First(a => a.PaymentId == Payment.PaymentId);

                        ListUpdate.PaymentName = Payment.PaymentName;
                        ListUpdate.Remarks = Payment.Remarks;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        break;

                    case "delete":

                        /* remove existing Payment */
                        var ListDelete = vssp_db.Tbl_MST_AccountingPayment.First(a => a.PaymentId == Payment.PaymentId);

                        vssp_db.Tbl_MST_AccountingPayment.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(Payment, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }          
        }
        public ActionResult crudCategoryList(string jsonData)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    string uid = Session["UserID"].ToString();

                    PostCategoryModel postCategory = JsonConvert.DeserializeObject<PostCategoryModel>(jsonData);
                    Tbl_MST_AccountingCategory Category = postCategory.Category;
                    string formAction = postCategory.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":

                            Tbl_MST_AccountingCategory ListCategory = new Tbl_MST_AccountingCategory();
                            ListCategory.CategoryId = Category.CategoryId;
                            ListCategory.CategoryName = Category.CategoryName;
                            ListCategory.Remarks = Category.Remarks;
                            ListCategory.UserId = uid;
                            ListCategory.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_AccountingCategory.Add(ListCategory);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_AccountingCategory.First(a => a.CategoryId == Category.CategoryId);

                            ListUpdate.CategoryName = Category.CategoryName;
                            ListUpdate.Remarks = Category.Remarks;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            break;

                        case "delete":

                            /* remove existing Category */
                            var ListDelete = vssp_db.Tbl_MST_AccountingCategory.First(a => a.CategoryId == Category.CategoryId);

                            vssp_db.Tbl_MST_AccountingCategory.Remove(ListDelete);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(Category, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = systemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult crudBankAccountList(string jsonData)
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    string uid = Session["UserID"].ToString();

                    PostBankAccountModel postBankAccount = JsonConvert.DeserializeObject<PostBankAccountModel>(jsonData);
                    Tbl_MST_AccountingBankAccount BankAccount = postBankAccount.BankAccount;
                    string formAction = postBankAccount.formAction.ToLower();

                    switch (formAction)
                    {
                        case "create":

                            Tbl_MST_AccountingBankAccount ListBankAccount = new Tbl_MST_AccountingBankAccount();
                            ListBankAccount.BankId = BankAccount.BankId;
                            ListBankAccount.Branch = BankAccount.Branch;
                            ListBankAccount.AccountName = BankAccount.AccountName;
                            ListBankAccount.AccountNumber = BankAccount.AccountNumber;
                            ListBankAccount.StartDate = BankAccount.StartDate;
                            ListBankAccount.EndDate = BankAccount.EndDate;
                            ListBankAccount.UserId = uid;
                            ListBankAccount.EditDate = DateTime.Now;

                            vssp_db.Tbl_MST_AccountingBankAccount.Add(ListBankAccount);

                            /* set end date */
                            crudBankAccountEndDate(ListBankAccount.StartDate, formAction);

                            break;

                        case "update":

                            var ListUpdate = vssp_db.Tbl_MST_AccountingBankAccount.First(a => a.BankId == BankAccount.BankId);

                            ListUpdate.Branch = BankAccount.Branch;
                            ListUpdate.AccountName = BankAccount.AccountName;
                            ListUpdate.AccountNumber = BankAccount.AccountNumber;
                            ListUpdate.StartDate = BankAccount.StartDate;
                            ListUpdate.EndDate = BankAccount.EndDate;
                            ListUpdate.UserId = uid;
                            ListUpdate.EditDate = DateTime.Now;

                            /* set end date */
                            crudBankAccountEndDate(ListUpdate.StartDate, formAction);

                            break;

                        case "delete":

                            /* remove existing BankAccount */
                            var ListDelete = vssp_db.Tbl_MST_AccountingBankAccount.First(a => a.BankId == BankAccount.BankId);

                            vssp_db.Tbl_MST_AccountingBankAccount.Remove(ListDelete);

                            /* set end date */
                            crudBankAccountEndDate(ListDelete.StartDate, formAction);

                            break;
                    }

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(BankAccount, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = systemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public void crudBankAccountEndDate(DateTime startdate, string action)
        {
            var bankaccount = vssp_db.Tbl_MST_AccountingBankAccount.Where(a => a.StartDate < startdate).OrderByDescending(a => a.StartDate).FirstOrDefault();
            if (bankaccount != null)
            {
                switch (action.ToLower())
                {
                    case "create":

                        bankaccount.EndDate = startdate.AddDays(-1);

                        break;
                    case "update":

                        bankaccount.EndDate = startdate.AddDays(-1);

                        break;
                    case "delete":

                        var oldbankaccount = vssp_db.Tbl_MST_AccountingBankAccount.Where(a => a.StartDate > startdate).OrderBy(a => a.StartDate).FirstOrDefault();
                        if (oldbankaccount != null)
                        {
                            startdate = oldbankaccount.StartDate;
                            bankaccount.EndDate = startdate.AddDays(-1);
                        } else
                        {
                            bankaccount.EndDate = null;
                        }

                        break;
                }
            }
        }
        public ActionResult InvoiceRecap()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();

                var acccessPreviliege = accountService.AccessPreviliege(uid, "FinanceAccounting", "InvoiceRecap");
                var menu = systemService.SidebarEditList(acccessPreviliege.MenuID == null ? "" : acccessPreviliege.MenuID).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                if (menu.Confidential == true && acccessPreviliege.ConfidentialAccess == false)
                {
                    return RedirectToAction("ConfidentialAccess", "System");
                }
                else
                {
                    ViewBag.Title = systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;
                    ViewBag.DateTime = DateTime.Now;

                    Session["Layout"] = "portal";
                    var stockTakingEvent = systemService.GetStockTakingEvent();

                    if (stockTakingEvent == null)
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";
                    }

                    return View();
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult InvoiceRecapListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99)
        {
            searchFilter = systemService.Vf(searchFilter);
            List<Vw_ACC_InvoiceRecap> InvoiceRecap = (from a in vssp_db.Vw_ACC_InvoiceRecap
                                                      where a.RecapNumber.Contains(searchFilter) || a.SupplierId.Contains(searchFilter)
                                                      orderby a.RecapDate descending, a.EditDate descending, a.RecapNumber
                                                      select a).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                InvoiceRecap = InvoiceRecap.Where(a => a.RecapDate >= startdate && a.RecapDate <= enddate).ToList();
            }
            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string RecapMonth = arrs[0];
                string RecapYears = arrs[1];
                InvoiceRecap = InvoiceRecap.Where(a => Convert.ToDateTime(a.RecapDate).ToString("MM") == RecapMonth && Convert.ToDateTime(a.RecapDate).ToString("yyyy") == RecapYears).ToList();
            }
            if (status != 99)
            {
                InvoiceRecap = InvoiceRecap.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in InvoiceRecap
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                InvoiceRecap = InvoiceRecap.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            //return Json(InvoiceRecap, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(InvoiceRecap, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public ActionResult InvoiceRecapDetailListJson(string RecapNumber, string SupplierId)
        {
            try
            {

                var InvoiceRecapDetail = from a in vssp_db.Tbl_ACC_InvoiceRecapDetail
                                           join b in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                           from b in part.DefaultIfEmpty()
                                           where a.RecapNumber == RecapNumber
                                           select new
                                           {
                                               a.RecapNumber,
                                               a.SupplierId,
                                               b.UniqueNumber,
                                               a.PartNumber,
                                               b.PartName,
                                               b.UnitQty,
                                               b.UnitLevel1,
                                               b.UnitLevel2,
                                               b.PackingId,
                                               b.PartModel,
                                               a.RecapQty,
                                               a.PriceQty,
                                               a.TotalPrice
                                           };

                if (systemService.Vf(SupplierId) != "")
                {
                    InvoiceRecapDetail = InvoiceRecapDetail.Where(a => a.SupplierId == SupplierId);
                }

                //return Json(InvoiceRecapDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(InvoiceRecapDetail, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult InvoiceRecapApprovalListJson(string recapnumber, Nullable<bool> approved)
        {
            try
            {

                var InvoiceRecapApproval = from a in vssp_db.Tbl_ACC_InvoiceRecapApproval
                                            where a.RecapNumber.Contains(recapnumber)
                                            orderby a.ApprovalLevel
                                            select new { a.RecapNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

                if (approved != null)
                {
                    InvoiceRecapApproval = InvoiceRecapApproval.Where(a => a.Approved == approved);
                }

                return Json(InvoiceRecapApproval, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetInvoiceRecapDetail(string recapnumber, string supplierid, string month, string formAction)
        {
            if (systemService.Vf(formAction) == "")
            {
                List<SP_GET_InvoiceRecapDetail_Result> _InvoiceRecapDetail = new List<SP_GET_InvoiceRecapDetail_Result>();
                //return Json(_InvoiceRecapDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(_InvoiceRecapDetail, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            else
            {
                //string[] arrs = month.Split('/');
                //string recapmonth = arrs[0];
                //string recapyears = arrs[1];
                string[] arrs = month.Split('-');
                DateTime startdate = DateTime.Parse(systemService.Vf(arrs[0]));
                DateTime enddate = DateTime.Parse(systemService.Vf(arrs[1]));

                List<SP_GET_InvoiceRecapDetail_Result> GetInvoiceRecapDetail = vssp_db.SP_GET_InvoiceRecapDetail(recapnumber, supplierid, startdate, enddate, formAction).ToList();

                //return Json(GetInvoiceRecapDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(GetInvoiceRecapDetail, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
        }
        public ActionResult crudSelectedRecap(string RecapNumber, string ReceiveNumber, string SupplierId, string PartNumber, bool Selected)
        {

            var recapTemp = vssp_db.Tbl_ACC_InvoiceRecapDetailTemp.Where(a => a.RecapNumber == RecapNumber && a.ReceiveNumber == ReceiveNumber && a.SupplierId == SupplierId && a.PartNumber == PartNumber).FirstOrDefault();
            recapTemp.Selected = Selected;

            try
            {
                vssp_db.SaveChanges();

                //return Json(PartNumber, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(PartNumber, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (DbEntityValidationException e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudInvoiceRecapList(string jsonData)
        {

            try
            {

                PostInvoiceRecapModel postInvoiceRecap = JsonConvert.DeserializeObject<PostInvoiceRecapModel>(jsonData);
                Tbl_ACC_InvoiceRecap InvoiceRecap = postInvoiceRecap.InvoiceRecap;

                string uid = postInvoiceRecap.uid;
                string formAction = postInvoiceRecap.formAction.ToLower();
                string compid = Session["CompID"].ToString();
                string approvalid = postInvoiceRecap.ApprovalId;

                switch (formAction)
                {
                    case "create":

                        var createInvoiceRecap = vssp_db.SP_CRUD_InvoiceRecap(InvoiceRecap.RecapNumber, 
                                                                                InvoiceRecap.RecapDate, 
                                                                                InvoiceRecap.RecapYear, 
                                                                                InvoiceRecap.RecapMonth, 
                                                                                InvoiceRecap.ReceiveStartDate, 
                                                                                InvoiceRecap.ReceiveEndDate, 
                                                                                InvoiceRecap.InvoiceNumber, 
                                                                                InvoiceRecap.TaxInvoice, 
                                                                                InvoiceRecap.SupplierId, 
                                                                                InvoiceRecap.Remarks, 
                                                                                uid, 
                                                                                compid, 
                                                                                formAction);
                        foreach (SP_CRUD_InvoiceRecap_Result number in createInvoiceRecap)
                        {
                            InvoiceRecap.RecapNumber = number.RecapNumber;
                        }
                        /* crud approval */
                        crudInvoiceRecapApproval(approvalid, InvoiceRecap.RecapNumber, uid, formAction);

                        break;

                    case "update":

                        var updateInvoiceRecap = vssp_db.SP_CRUD_InvoiceRecap(InvoiceRecap.RecapNumber, 
                                                                                InvoiceRecap.RecapDate, 
                                                                                InvoiceRecap.RecapYear, 
                                                                                InvoiceRecap.RecapMonth, 
                                                                                InvoiceRecap.ReceiveStartDate, 
                                                                                InvoiceRecap.ReceiveEndDate, 
                                                                                InvoiceRecap.InvoiceNumber, 
                                                                                InvoiceRecap.TaxInvoice, 
                                                                                InvoiceRecap.SupplierId, 
                                                                                InvoiceRecap.Remarks, 
                                                                                uid, 
                                                                                compid, 
                                                                                formAction);
                        /* crud approval */
                        //crudInvoiceRecapApproval(approvalid, InvoiceRecap.RecapNumber, uid, formAction);

                        break;

                    case "closed":

                        var ListClosed = vssp_db.Tbl_ACC_InvoiceRecap.First(a => a.RecapNumber == InvoiceRecap.RecapNumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_ACC_InvoiceRecap.First(a => a.RecapNumber == InvoiceRecap.RecapNumber);

                        ListCanceled.Status = 4;

                        var cancelreceiveNote = vssp_db.Tbl_TRS_ReceivingOrderDetail.Where(a => a.RecapNumber == InvoiceRecap.RecapNumber).ToList();
                        foreach (var rn in cancelreceiveNote)
                        {
                            rn.RecapNumber = "";
                        }

                        break;

                    case "delete":

                        /* remove existing InvoiceRecap */
                        var ListDelete = vssp_db.Tbl_ACC_InvoiceRecap.First(a => a.RecapNumber == InvoiceRecap.RecapNumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        var deletereceiveNote = vssp_db.Tbl_TRS_ReceivingOrderDetail.Where(a => a.RecapNumber == InvoiceRecap.RecapNumber).ToList();
                        foreach (var rn in deletereceiveNote)
                        {
                            rn.RecapNumber = "";
                        }
                        //vssp_db.Tbl_ACC_InvoiceRecap.Remove(ListDelete); //Update Status To Delete Only Not Remove From DB

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();

                    //return Json(InvoiceRecap, JsonRequestBehavior.AllowGet);
                    var jsonResult = Json(InvoiceRecap, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;

                }
                catch (DbEntityValidationException e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }

        }

        public void crudInvoiceRecapApproval(string ApprovalId, string RecapNumber, string UserId, string action)
        {
            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    List<UserApprovalListModel> userApprovalLists = accountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalLists)
                    {
                        Tbl_ACC_InvoiceRecapApproval ListApproval = new Tbl_ACC_InvoiceRecapApproval();
                        ListApproval.RecapNumber = RecapNumber;
                        ListApproval.UserId = users.UserID;
                        ListApproval.UserName = users.UserName;
                        ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                        ListApproval.ApprovalName = users.ApprovalName;
                        ListApproval.ApprovalEmail = users.Email;
                        ListApproval.SentEmail = false;
                        ListApproval.SentEmailDate = null;
                        if (users.UserID == UserId)
                        {
                            ListApproval.Approved = true;
                            ListApproval.ApprovedDate = DateTime.Now;
                        }
                        else
                        {
                            ListApproval.Approved = false;
                            ListApproval.ApprovedDate = null;
                        }

                        vssp_db.Tbl_ACC_InvoiceRecapApproval.Add(ListApproval);
                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_ACC_InvoiceRecapApproval> UserApproval = vssp_db.Tbl_ACC_InvoiceRecapApproval.Where(a => a.RecapNumber == RecapNumber).ToList();
                    foreach (var user in UserApproval)
                    {
                        UserApprovalListModel ApprovalLists = accountService.UserApprovalType(user.UserId, ApprovalId).First(a => a.UserID == user.UserId);
                    }

                    /* create approval */
                    List<UserApprovalListModel> userApprovalListsUpdate = accountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalListsUpdate)
                    {
                        var existUser = (from a in vssp_db.Tbl_ACC_InvoiceRecapApproval
                                         where a.RecapNumber == RecapNumber && a.UserId == users.UserID
                                         select new { a.RecapNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate }).FirstOrDefault();
                        
                        if (existUser == null)
                        {
                            Tbl_ACC_InvoiceRecapApproval ListApproval = new Tbl_ACC_InvoiceRecapApproval();
                            ListApproval.RecapNumber = RecapNumber;
                            ListApproval.UserId = users.UserID;
                            ListApproval.UserName = users.UserName;
                            ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                            ListApproval.ApprovalName = users.ApprovalName;
                            ListApproval.ApprovalEmail = users.Email;
                            ListApproval.SentEmail = false;
                            ListApproval.SentEmailDate = null;
                            if (users.UserID == UserId)
                            {
                                ListApproval.Approved = true;
                                ListApproval.ApprovedDate = DateTime.Now;
                            }
                            else
                            {
                                ListApproval.Approved = false;
                                ListApproval.ApprovedDate = null;
                            }

                            vssp_db.Tbl_ACC_InvoiceRecapApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "sent":

                    var ListSent = vssp_db.Tbl_ACC_InvoiceRecapApproval.First(a => a.RecapNumber == RecapNumber && a.UserId == ApprovalId);

                    ListSent.SentEmail = true;
                    ListSent.SentEmailDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "approved":

                    var ListUpdate = vssp_db.Tbl_ACC_InvoiceRecapApproval.First(a => a.RecapNumber == RecapNumber && a.UserId == ApprovalId);

                    ListUpdate.Approved = true;
                    ListUpdate.ApprovedDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_ACC_InvoiceRecapApproval.First(a => a.RecapNumber == RecapNumber && a.UserId == ApprovalId);

                    vssp_db.Tbl_ACC_InvoiceRecapApproval.Remove(ListDelete);

                    break;
            }

        }
        public ActionResult InvoiceRecapMail(string RecapNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Delivery Note & Kanban Order";

            try
            {

                if (RecapNumber == null || uid == null)
                {
                    RecapNumber = Session["RecapNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["RecapNumber"] = RecapNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_InvoiceRecap InvoiceRecap = vssp_db.Vw_ACC_InvoiceRecap.Where(a => a.RecapNumber == RecapNumber).FirstOrDefault();
                    //UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_MST_SupplierContact approval = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == InvoiceRecap.SupplierId && a.Email == uid).FirstOrDefault();

                    if (InvoiceRecap != null && approval != null)
                    {

                        string RecapDate = systemService.Vd(InvoiceRecap.RecapDate.ToString(), "MMMM dd, yyyy");

                        ViewBag.OrderTitle = "Delivery Note & Kanban Order";
                        ViewBag.RecapNumber = InvoiceRecap.RecapNumber;
                        ViewBag.RecapDate = RecapDate;
                        ViewBag.SupplierName = InvoiceRecap.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = approval.ContactName;

                        return View();

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Delivery Note & Kanban Order";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }
        public ActionResult InvoiceRecapApproval(string RecapNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Invoice Recap Approval";

            try
            {

                if (RecapNumber == null || uid == null)
                {
                    RecapNumber = Session["RecapNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["RecapNumber"] = RecapNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_InvoiceRecap InvoiceRecap = vssp_db.Vw_ACC_InvoiceRecap.Where(a => a.RecapNumber == RecapNumber).FirstOrDefault();
                    UserEditModel user = accountService.UserEditList(cryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_ACC_InvoiceRecapApproval approval = vssp_db.Tbl_ACC_InvoiceRecapApproval.Where(a => a.RecapNumber == RecapNumber && a.UserId == user.UserID).FirstOrDefault();

                    if (InvoiceRecap != null && user != null && approval != null)
                    {

                        string orderdate = new DateTime(int.Parse(InvoiceRecap.RecapYear), int.Parse(InvoiceRecap.RecapMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.RecapTitle = "Invoice Recap List";
                        ViewBag.RecapNumber = InvoiceRecap.RecapNumber;
                        ViewBag.RecapDate = orderdate;
                        ViewBag.SupplierName = InvoiceRecap.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        if (approval.Approved == false)
                        {
                            return View();
                        }
                        else
                        {
                            ViewBag.ApprovedDate = systemService.Vd(approval.ApprovedDate.ToString(), "dd MMMM, yyyy");
                            return View("InvoiceRecapApproved");
                        }

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Invoice Recap List";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }

        public ActionResult InvoiceRecapSupplier(string RecapNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Invoice Recap List Approval";

            try
            {

                if (RecapNumber == null || uid == null)
                {
                    RecapNumber = Session["RecapNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["RecapNumber"] = RecapNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_InvoiceRecap InvoiceRecap = vssp_db.Vw_ACC_InvoiceRecap.Where(a => a.RecapNumber == RecapNumber).FirstOrDefault();
                    //UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_MST_SupplierContact approval = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == InvoiceRecap.SupplierId && a.Email == uid).FirstOrDefault();

                    if (InvoiceRecap != null && approval != null)
                    {

                        string orderdate = new DateTime(int.Parse(InvoiceRecap.RecapYear), int.Parse(InvoiceRecap.RecapMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.RecapTitle = "Invoice Recap List";
                        ViewBag.RecapNumber = InvoiceRecap.RecapNumber;
                        ViewBag.RecapDate = orderdate;
                        ViewBag.SupplierName = InvoiceRecap.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = approval.ContactName;

                        return View();

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Invoice Recap List";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }

        public ActionResult InvoiceRecapApproved(string RecapNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Invoice Recap List Approval";

            try
            {

                if (RecapNumber == null || uid == null)
                {
                    RecapNumber = Session["RecapNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["RecapNumber"] = RecapNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_InvoiceRecap InvoiceRecap = vssp_db.Vw_ACC_InvoiceRecap.Where(a => a.RecapNumber == RecapNumber).FirstOrDefault();
                    UserEditModel user = accountService.UserEditList(cryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();

                    if (InvoiceRecap != null && user != null)
                    {

                        string orderdate = new DateTime(int.Parse(InvoiceRecap.RecapYear), int.Parse(InvoiceRecap.RecapMonth), 1).ToString("MMMM, yyyy");

                        ViewBag.RecapTitle = "Invoice Recap List";
                        ViewBag.RecapNumber = InvoiceRecap.RecapNumber;
                        ViewBag.RecapDate = orderdate;
                        ViewBag.SupplierName = InvoiceRecap.SupplierName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        crudInvoiceRecapApproval(user.UserID, InvoiceRecap.RecapNumber, user.UserID, "Approved");
                        return RedirectToAction("ContinuePage", "System", new { cmessage = "Successfuly Approved " + ViewBag.RecapTitle + " \n " + RecapNumber, caction = "Dashboard", ccontroller = "Home", capps = "Home" });

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Invoice Recap List";
                        ViewBag.UserName = null;

                        return RedirectToAction("ErrorPage", "System", new { errnumber = "500", errmessage = "Order or User not valid.", backaction = "MainIndex", backcontroller = "Index" });

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }

        public ActionResult GeneralLedgerPayable()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();

                var acccessPreviliege = accountService.AccessPreviliege(uid, "FinanceAccounting", "GeneralLedgerPayable");
                var menu = systemService.SidebarEditList(acccessPreviliege.MenuID == null ? "" : acccessPreviliege.MenuID).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                if (menu.Confidential == true && acccessPreviliege.ConfidentialAccess == false)
                {
                    return RedirectToAction("ConfidentialAccess", "System");
                }
                else
                {
                    ViewBag.Title = systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    Session["Layout"] = "portal";
                    var stockTakingEvent = systemService.GetStockTakingEvent();

                    if (stockTakingEvent == null)
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";
                    }

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = systemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult GeneralLedgerPayableListJson(string month, string supplier)
        {
            string RecapMonth = "";
            string RecapYears = "";

            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                RecapMonth = arrs[0];
                RecapYears = arrs[1];
            }
            else
            {
                RecapMonth = DateTime.Now.ToString("MM");
                RecapYears = DateTime.Now.ToString("yyyy");
            }

            var Ledger = vssp_db.SP_GET_Ledger(RecapYears, RecapMonth, systemService.Vf(supplier));

            //return Json(Ledger, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(Ledger, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult PaymentRecapListJson(string month, string supplier)
        {
            string RecapMonth = "";
            string RecapYears = "";

            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                RecapMonth = arrs[0];
                RecapYears = arrs[1];
            }
            else
            {
                RecapMonth = DateTime.Now.ToString("MM");
                RecapYears = DateTime.Now.ToString("yyyy");
            }

            var PaymentRecap = vssp_db.SP_GET_PaymentRecap(RecapYears, RecapMonth, systemService.Vf(supplier));

            //return Json(PaymentRecap, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(PaymentRecap, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult TransferListListJson(string month, string supplier)
        {
            string RecapMonth = "";
            string RecapYears = "";

            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                RecapMonth = arrs[0];
                RecapYears = arrs[1];
            }
            else
            {
                RecapMonth = DateTime.Now.ToString("MM");
                RecapYears = DateTime.Now.ToString("yyyy");
            }

            var TransferList = vssp_db.SP_GET_TransferList(RecapYears, RecapMonth, systemService.Vf(supplier));

            //return Json(TransferList, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(TransferList, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult CustomerInvoice()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();

                var acccessPreviliege = accountService.AccessPreviliege(uid, "FinanceAccounting", "CustomerInvoice");
                var menu = systemService.SidebarEditList(acccessPreviliege.MenuID == null ? "" : acccessPreviliege.MenuID).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                if (menu.Confidential == true && acccessPreviliege.ConfidentialAccess == false)
                {
                    return RedirectToAction("ConfidentialAccess", "System");
                }
                else
                {
                    ViewBag.Title = systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;
                    ViewBag.DateTime = DateTime.Now;

                    Session["Layout"] = "portal";

                    CustomerInvoiceListModel CustomerInvoice = new CustomerInvoiceListModel();
                    CustomerInvoice.ExportList = systemService.ComboExport().ToList();
                    CustomerInvoice.StatusList = (from a in vssp_db.Tbl_TRS_Status
                                             orderby a.Id
                                             select a).ToList();


                    return View(CustomerInvoice);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult CustomerInvoiceListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    int status = 99)
        {
            searchFilter = systemService.Vf(searchFilter);
            List<Vw_ACC_CustomerInvoice> CustomerInvoice = (from a in vssp_db.Vw_ACC_CustomerInvoice
                                                            where a.InvoiceNumber.Contains(searchFilter) || a.CustomerId.Contains(searchFilter)
                                                            orderby a.InvoiceDate descending, a.EditDate descending, a.InvoiceNumber
                                                            select a).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                CustomerInvoice = CustomerInvoice.Where(a => a.InvoiceDate >= startdate && a.InvoiceDate <= enddate).ToList();
            }
            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string InvoiceMonth = arrs[0];
                string InvoiceYears = arrs[1];
                CustomerInvoice = CustomerInvoice.Where(a => Convert.ToDateTime(a.InvoiceDate).ToString("MM") == InvoiceMonth && Convert.ToDateTime(a.InvoiceDate).ToString("yyyy") == InvoiceYears).ToList();
            }
            if (status != 99)
            {
                CustomerInvoice = CustomerInvoice.Where(a => a.Status.ToString() == status.ToString()).ToList();
            }
            else
            {
                var notinStatus = from a in CustomerInvoice
                                  where a.Status.ToString().Contains("4") || a.Status.ToString().Contains("5")
                                  select a.Status;
                CustomerInvoice = CustomerInvoice.Where(a => !notinStatus.Contains(a.Status)).ToList();
            }

            //return Json(CustomerInvoice, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(CustomerInvoice, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult CustomerInvoiceDetailListJson(string InvoiceNumber, string CustomerId)
        {
            try
            {

                var CustomerInvoiceDetail = (from a in vssp_db.Vw_ACC_CustomerInvoiceDetail
                                            where a.InvoiceNumber == InvoiceNumber
                                            orderby a.InvoiceNumber,a.CustomerId,a.UniqueNumber
                                            select a).ToList();

                if (systemService.Vf(CustomerId) != "")
                {
                    CustomerInvoiceDetail = CustomerInvoiceDetail.Where(a => a.CustomerId == CustomerId).ToList();
                }

                //return Json(CustomerInvoiceDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(CustomerInvoiceDetail, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCustomerInvoiceDetail(string InvoiceNumber, string FormAction)
        {
            try
            {
                List<SP_GET_CustomerInvoiceDetail_Result> getCustomerInvoiceDetail = vssp_db.SP_GET_CustomerInvoiceDetail(InvoiceNumber, FormAction).ToList();
                //return Json(getCustomerInvoiceDetail, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(getCustomerInvoiceDetail, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCustomerInvoiceDeliveryRecap(string InvoiceNumber, string CustomerId, DateTime StartDate, DateTime EndDate, string searchFilter, string FormAction, bool IncludeImport = false)
        {
            try
            {

                searchFilter = systemService.Vf(searchFilter);

                //var invoiceDeliveryRecap = (from a in vssp_db.Vw_ACC_CustomerInvoiceDeliveryRecap
                //                            where a.CustomerId==CustomerId && a.DODate>= StartDate && a.DODate<=EndDate &&
                //                            (a.DONumber.Contains(searchFilter) || a.PONumber.Contains(searchFilter) || a.RefNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                //                            select a
                //                            ).ToList();
                //switch (FormAction.ToLower())
                //{
                //    case "create":
                //        invoiceDeliveryRecap = invoiceDeliveryRecap.Where(a => a.Invoiced == false).ToList();
                //        break;
                //    default:
                //        invoiceDeliveryRecap = invoiceDeliveryRecap.Where(a => (a.Invoiced == false || a.InvoiceNumber == InvoiceNumber)).ToList();
                //        break;
                //}

                //if (IncludeImport == false)
                //{
                //    invoiceDeliveryRecap = invoiceDeliveryRecap.Where(a => a.Status != 6).ToList();
                //}
                //invoiceDeliveryRecap = invoiceDeliveryRecap.OrderBy(a => a.DODate).ThenBy(a => a.DONumber).ThenBy(a => a.PartNumber).ToList();
                //var jsonResult = Json(invoiceDeliveryRecap, JsonRequestBehavior.AllowGet);
                //jsonResult.MaxJsonLength = int.MaxValue;
                //return jsonResult;
                vssp_db.Database.CommandTimeout = 0;
                var invoiceDeliveryRecap = vssp_db.Database.SqlQuery<GetInvoiceModel>(
                    "EXEC SP_GetInvoiceDeliveryRecapDetails @CustomerId, @StartDate, @EndDate, @SearchFilter, @FormAction, @InvoiceNumber, @IncludeImport",
                    new SqlParameter("@CustomerId", CustomerId),
                    new SqlParameter("@StartDate", StartDate),
                    new SqlParameter("@EndDate", EndDate),
                    new SqlParameter("@SearchFilter", searchFilter),
                    new SqlParameter("@FormAction", FormAction),
                    new SqlParameter("@InvoiceNumber", InvoiceNumber),
                    new SqlParameter("@IncludeImport", IncludeImport)
                ).ToList();
                var jsonResult = Json(invoiceDeliveryRecap, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }

        //NEW IMPROVEMENT DASEP 
        public ActionResult GetCustomerInvoiceDeliveryRecapNew(string InvoiceNumber, string CustomerId, DateTime StartDate, DateTime EndDate, string searchFilter, string FormAction, bool IncludeImport, int page = 1, int rows = 50)
        {
            try
            {
                searchFilter = systemService.Vf(searchFilter);

                var query = vssp_db.Vw_ACC_CustomerInvoiceDeliveryRecap
                    .Where(a => a.CustomerId == CustomerId && a.DODate >= StartDate && a.DODate <= EndDate &&
                                (a.DONumber.Contains(searchFilter) || a.PONumber.Contains(searchFilter) || a.RefNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter)));

                switch (FormAction.ToLower())
                {
                    case "create":
                        query = query.Where(a => a.Invoiced == false);
                        break;
                    default:
                        query = query.Where(a => (a.Invoiced == false || a.InvoiceNumber == InvoiceNumber));
                        break;
                }

                if (!IncludeImport)
                {
                    query = query.Where(a => a.Status != 6);
                }

                int totalRecords = query.Count();
                var invoiceDeliveryRecap = query
                    .OrderBy(a => a.DODate)
                    .ThenBy(a => a.DONumber)
                    .ThenBy(a => a.PartNumber)
                    .Skip((page - 1) * rows)
                    .Take(rows)
                    .ToList();

                var jsonResult = Json(new
                {
                    total = (int)Math.Ceiling((double)totalRecords / rows),
                    page = page,
                    records = totalRecords,
                    rows = invoiceDeliveryRecap
                }, JsonRequestBehavior.AllowGet);

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        //



        public ActionResult CustomerInvoiceApprovalListJson(string invoicenumber, Nullable<bool> approved)
        {
            try
            {

                var CustomerInvoiceApproval = from a in vssp_db.Tbl_ACC_CustomerInvoiceApproval
                                            where a.InvoiceNumber.Contains(invoicenumber)
                                            orderby a.ApprovalLevel
                                            select new { a.InvoiceNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate };

                if (approved != null)
                {
                    CustomerInvoiceApproval = CustomerInvoiceApproval.Where(a => a.Approved == approved);
                }

                return Json(CustomerInvoiceApproval, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudRecapDelivery(string jsonData)
        {
            try
            {

                PostRecapDeliveryModel postRecapDelivery = JsonConvert.DeserializeObject<PostRecapDeliveryModel>(jsonData);
                List<Vw_ACC_CustomerInvoiceDeliveryRecap> recapDelivery = postRecapDelivery.RecapDelivery;
                string InvoiceNumber = postRecapDelivery.InvoiceNumber;

                var ListDelete = vssp_db.Tbl_ACC_CustomerInvoiceDetailTemp.Where(a => a.InvoiceNumber == InvoiceNumber).ToList();
                vssp_db.Tbl_ACC_CustomerInvoiceDetailTemp.RemoveRange(ListDelete);

                //foreach(var delete in ListDelete)
                //{
                //    vssp_db.Tbl_ACC_CustomerInvoiceDetailTemp.Remove(delete);
                //}

                foreach(var recap in recapDelivery)
                {
                    Tbl_ACC_CustomerInvoiceDetailTemp ListAdd = new Tbl_ACC_CustomerInvoiceDetailTemp();
                    ListAdd.InvoiceNumber = InvoiceNumber;
                    ListAdd.DONumber = recap.DONumber;
                    ListAdd.SONumber = recap.SONumber;
                    ListAdd.RefNumber = recap.RefNumber;
                    ListAdd.CustomerId = recap.CustomerId;
                    ListAdd.PartNumber = recap.PartNumber;
                    ListAdd.Price = recap.Price;
                    ListAdd.Qty = recap.DeliveryUnitQty;
                    ListAdd.Amount = recap.Amount;

                    vssp_db.Tbl_ACC_CustomerInvoiceDetailTemp.Add(ListAdd);

                }
                
                try
                {
                    vssp_db.SaveChanges();
                    //return Json(recapDelivery, JsonRequestBehavior.AllowGet);
                    var jsonResult = Json(recapDelivery, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                catch (DbEntityValidationException e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult crudCustomerInvoiceList(string jsonData)
        {


            try
            {

                PostCustomerInvoiceModel postCustomerInvoice = JsonConvert.DeserializeObject<PostCustomerInvoiceModel>(jsonData);
                Tbl_ACC_CustomerInvoice CustomerInvoice = postCustomerInvoice.CustomerInvoice;

                string uid = postCustomerInvoice.uid;
                string formAction = postCustomerInvoice.formAction.ToLower();
                string compid = Session["CompID"].ToString();
                string approvalid = postCustomerInvoice.ApprovalId;

                switch (formAction)
                {
                    case "create":

                        if (CustomerInvoice.InvoiceNumber == "")
                        {
                            SP_GET_CustomerInvoiceNumber_Result invNumber = vssp_db.SP_GET_CustomerInvoiceNumber(CustomerInvoice.CustomerId, CustomerInvoice.InvoiceDate, compid).FirstOrDefault();
                            CustomerInvoice.InvoiceNumber = invNumber.InvoiceNumber;
                        }
                        Tbl_ACC_CustomerInvoice ListAdd = new Tbl_ACC_CustomerInvoice();
                        ListAdd.InvoiceNumber = CustomerInvoice.InvoiceNumber;
                        ListAdd.InvoiceDate = CustomerInvoice.InvoiceDate;
                        ListAdd.CustomerId = CustomerInvoice.CustomerId;
                        ListAdd.DOStart = CustomerInvoice.DOStart;
                        ListAdd.DOEnd = CustomerInvoice.DOEnd;
                        ListAdd.SubTotal = CustomerInvoice.SubTotal;
                        ListAdd.PPN = CustomerInvoice.PPN;
                        ListAdd.PPH23 = CustomerInvoice.PPH23;
                        ListAdd.GrandTotal = CustomerInvoice.GrandTotal;
                        ListAdd.Terms = CustomerInvoice.Terms;
                        ListAdd.Remarks = CustomerInvoice.Remarks;
                        ListAdd.IncludePPH23 = CustomerInvoice.IncludePPH23;
                        ListAdd.Paid = false;
                        ListAdd.Status = 0;
                        ListAdd.UserId = uid;
                        ListAdd.EditDate = DateTime.Now;

                        vssp_db.Tbl_ACC_CustomerInvoice.Add(ListAdd);

                        /* crud details */
                        crudCustomerInvoiceDetail(CustomerInvoice.InvoiceNumber, uid, formAction);

                        /* crud approval */
                        crudCustomerInvoiceApproval(approvalid, CustomerInvoice.InvoiceNumber, uid, formAction);

                        break;

                    case "update":

                        Tbl_ACC_CustomerInvoice ListUpdate = vssp_db.Tbl_ACC_CustomerInvoice.First(a => a.InvoiceNumber == CustomerInvoice.InvoiceNumber);
                        ListUpdate.DOStart = CustomerInvoice.DOStart;
                        ListUpdate.DOEnd = CustomerInvoice.DOEnd;
                        ListUpdate.SubTotal = CustomerInvoice.SubTotal;
                        ListUpdate.PPN = CustomerInvoice.PPN;
                        ListUpdate.PPH23 = CustomerInvoice.PPH23;
                        ListUpdate.GrandTotal = CustomerInvoice.GrandTotal;
                        ListUpdate.Terms = CustomerInvoice.Terms;
                        ListUpdate.Remarks = CustomerInvoice.Remarks;
                        ListUpdate.IncludePPH23 = CustomerInvoice.IncludePPH23;
                        ListUpdate.Paid = false;
                        ListUpdate.UserId = CustomerInvoice.UserId;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud details */
                        crudCustomerInvoiceDetail(CustomerInvoice.InvoiceNumber, uid, formAction);

                        /* crud approval */
                        crudCustomerInvoiceApproval(approvalid, CustomerInvoice.InvoiceNumber, uid, formAction);

                        break;

                    case "closed":

                        var ListClosed = vssp_db.Tbl_ACC_CustomerInvoice.First(a => a.InvoiceNumber == CustomerInvoice.InvoiceNumber);

                        ListClosed.Status = 3;

                        break;

                    case "canceled":

                        var ListCanceled = vssp_db.Tbl_ACC_CustomerInvoice.First(a => a.InvoiceNumber == CustomerInvoice.InvoiceNumber);

                        ListCanceled.Status = 4;

                        /* crud details */
                        crudCustomerInvoiceDetail(CustomerInvoice.InvoiceNumber, uid, formAction);

                        break;

                    case "delete":

                        /* remove existing CustomerInvoice */
                        var ListDelete = vssp_db.Tbl_ACC_CustomerInvoice.First(a => a.InvoiceNumber == CustomerInvoice.InvoiceNumber);

                        ListDelete.Status = 5; //Update Status To Delete Only Not Remove From DB

                        /* crud details */
                        crudCustomerInvoiceDetail(CustomerInvoice.InvoiceNumber, uid, formAction);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();

                    //return Json(CustomerInvoice, JsonRequestBehavior.AllowGet);
                    var jsonResult = Json(CustomerInvoice, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;

                }
                catch (DbEntityValidationException e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }

        }
        public void crudCustomerInvoiceDetail(string InvoiceNumber, string UserId, string action)
        {
            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    List<Tbl_ACC_CustomerInvoiceDetailTemp> invoiceDetailTemps = vssp_db.Tbl_ACC_CustomerInvoiceDetailTemp.Where(a => a.InvoiceNumber == UserId).ToList();
                    foreach (var temp in invoiceDetailTemps)
                    {
                        Tbl_ACC_CustomerInvoiceDetail ListAdd = new Tbl_ACC_CustomerInvoiceDetail();
                        ListAdd.InvoiceNumber = InvoiceNumber;
                        ListAdd.DONumber = temp.DONumber;
                        ListAdd.SONumber = temp.SONumber;
                        ListAdd.RefNumber = temp.RefNumber;
                        ListAdd.CustomerId = temp.CustomerId;
                        ListAdd.PartNumber = temp.PartNumber;
                        ListAdd.Price = temp.Price;
                        ListAdd.Qty = temp.Qty;
                        ListAdd.Amount = temp.Amount;

                        vssp_db.Tbl_ACC_CustomerInvoiceDetail.Add(ListAdd);

                        /* update status delivery order */
                        updateStatusCustomerDelivery(temp.DONumber, action);

                    }

                    break;

                case "update":

                    /* clean old data */
                    List<Tbl_ACC_CustomerInvoiceDetail> invoiceDetailExist = vssp_db.Tbl_ACC_CustomerInvoiceDetail.Where(a => a.InvoiceNumber == InvoiceNumber).ToList();
                    foreach(var exist in invoiceDetailExist)
                    {
                        vssp_db.Tbl_ACC_CustomerInvoiceDetail.Remove(exist);

                        /* update status delivery order */
                        updateStatusCustomerDelivery(exist.DONumber, "delete");

                    }
                    /* re-insert new data */
                    List<Tbl_ACC_CustomerInvoiceDetailTemp> invoiceDetailTempsUpdate = vssp_db.Tbl_ACC_CustomerInvoiceDetailTemp.Where(a => a.InvoiceNumber == InvoiceNumber).ToList();
                    foreach (var temp in invoiceDetailTempsUpdate)
                    {
                        Tbl_ACC_CustomerInvoiceDetail ListAdd = new Tbl_ACC_CustomerInvoiceDetail();
                        ListAdd.InvoiceNumber = InvoiceNumber;
                        ListAdd.DONumber = temp.DONumber;
                        ListAdd.SONumber = temp.SONumber;
                        ListAdd.RefNumber = temp.RefNumber;
                        ListAdd.CustomerId = temp.CustomerId;
                        ListAdd.PartNumber = temp.PartNumber;
                        ListAdd.Price = temp.Price;
                        ListAdd.Qty = temp.Qty;
                        ListAdd.Amount = temp.Amount;

                        vssp_db.Tbl_ACC_CustomerInvoiceDetail.Add(ListAdd);

                        /* update status delivery order */
                        updateStatusCustomerDelivery(temp.DONumber, action);

                    }

                    break;

                case "canceled":

                    var ListCancel = vssp_db.Tbl_ACC_CustomerInvoiceDetail.Where(a => a.InvoiceNumber == InvoiceNumber).ToList();
                    foreach (var cancel in ListCancel)
                    {

                        /* update status delivery order */
                        updateStatusCustomerDelivery(cancel.DONumber, action);
                        vssp_db.Tbl_ACC_CustomerInvoiceDetail.Remove(cancel);
                    }

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_ACC_CustomerInvoiceDetail.Where(a => a.InvoiceNumber == InvoiceNumber).ToList();
                    foreach (var delete in ListDelete)
                    {

                        /* update status delivery order */
                        updateStatusCustomerDelivery(delete.DONumber, action);
                        vssp_db.Tbl_ACC_CustomerInvoiceDetail.Remove(delete);

                    }

                    break;
            }

        }
        public void crudCustomerInvoiceApproval(string ApprovalId, string InvoiceNumber, string UserId, string action)
        {
            switch (action.ToLower())
            {
                case "create":

                    /* create approval */
                    List<UserApprovalListModel> userApprovalLists = accountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalLists)
                    {
                        Tbl_ACC_CustomerInvoiceApproval ListApproval = new Tbl_ACC_CustomerInvoiceApproval();
                        ListApproval.InvoiceNumber = InvoiceNumber;
                        ListApproval.UserId = users.UserID;
                        ListApproval.UserName = users.UserName;
                        ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                        ListApproval.ApprovalName = users.ApprovalName;
                        ListApproval.ApprovalEmail = users.Email;
                        ListApproval.SentEmail = false;
                        ListApproval.SentEmailDate = null;
                        if (users.UserID == UserId)
                        {
                            ListApproval.Approved = true;
                            ListApproval.ApprovedDate = DateTime.Now;
                        }
                        else
                        {
                            ListApproval.Approved = false;
                            ListApproval.ApprovedDate = null;
                        }

                        vssp_db.Tbl_ACC_CustomerInvoiceApproval.Add(ListApproval);
                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_ACC_CustomerInvoiceApproval> UserApproval = vssp_db.Tbl_ACC_CustomerInvoiceApproval.Where(a => a.InvoiceNumber == InvoiceNumber).ToList();
                    foreach(var user in UserApproval)
                    {
                        UserApprovalListModel ApprovalLists = accountService.UserApprovalType(user.UserId, ApprovalId).First(a=> a.UserID == user.UserId);
                    }

                    /* create approval */
                    List<UserApprovalListModel> userApprovalListsUpdate = accountService.UserApprovalType(UserId, ApprovalId);
                    foreach (var users in userApprovalListsUpdate)
                    {
                        Tbl_ACC_CustomerInvoiceApproval existUser = (from a in vssp_db.Tbl_ACC_CustomerInvoiceApproval
                                                                     where a.InvoiceNumber == InvoiceNumber && a.UserId == users.UserID
                                                                     select a).FirstOrDefault();
                        if (existUser == null)
                        {
                            Tbl_ACC_CustomerInvoiceApproval ListApproval = new Tbl_ACC_CustomerInvoiceApproval();
                            ListApproval.InvoiceNumber = InvoiceNumber;
                            ListApproval.UserId = users.UserID;
                            ListApproval.UserName = users.UserName;
                            ListApproval.ApprovalLevel = int.Parse(users.ApprovalLevel.ToString());
                            ListApproval.ApprovalName = users.ApprovalName;
                            ListApproval.ApprovalEmail = users.Email;
                            ListApproval.SentEmail = false;
                            ListApproval.SentEmailDate = null;
                            if (users.UserID == UserId)
                            {
                                ListApproval.Approved = true;
                                ListApproval.ApprovedDate = DateTime.Now;
                            }
                            else
                            {
                                ListApproval.Approved = false;
                                ListApproval.ApprovedDate = null;
                            }

                            vssp_db.Tbl_ACC_CustomerInvoiceApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "sent":

                    var ListSent = vssp_db.Tbl_ACC_CustomerInvoiceApproval.First(a => a.InvoiceNumber == InvoiceNumber && a.UserId == ApprovalId);

                    ListSent.SentEmail = true;
                    ListSent.SentEmailDate = DateTime.Now;

                    vssp_db.SaveChanges();

                    break;

                case "approved":

                    var ListUpdate = vssp_db.Tbl_ACC_CustomerInvoiceApproval.First(a => a.InvoiceNumber == InvoiceNumber && a.UserId == ApprovalId);

                    ListUpdate.Approved = true;
                    ListUpdate.ApprovedDate = DateTime.Now;

                    var customerinvoice = vssp_db.Tbl_ACC_CustomerInvoice.First(a => a.InvoiceNumber == InvoiceNumber);
                    
                    if (customerinvoice.Status < 2)
                    {
                        customerinvoice.Status = 2;
                    }

                    vssp_db.SaveChanges();

                    break;

                case "delete":

                    var ListDelete = vssp_db.Tbl_ACC_CustomerInvoiceApproval.First(a => a.InvoiceNumber == InvoiceNumber && a.UserId == ApprovalId);

                    vssp_db.Tbl_ACC_CustomerInvoiceApproval.Remove(ListDelete);

                    break;
            }

        }
        public ActionResult CustomerInvoiceMail(string InvoiceNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Customer Invoice";

            try
            {

                if (InvoiceNumber == null || uid == null)
                {
                    InvoiceNumber = Session["InvoiceNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["InvoiceNumber"] = InvoiceNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_CustomerInvoice CustomerInvoice = vssp_db.Vw_ACC_CustomerInvoice.Where(a => a.InvoiceNumber == InvoiceNumber).FirstOrDefault();
                    Tbl_MST_CustomerContact approval = vssp_db.Tbl_MST_CustomerContact.Where(a => a.CustomerId == CustomerInvoice.CustomerId && a.Email == uid).FirstOrDefault();

                    if (CustomerInvoice != null && approval != null)
                    {

                        string InvoiceDate = systemService.Vd(CustomerInvoice.InvoiceDate.ToString(), "MMMM dd, yyyy");

                        ViewBag.OrderTitle = "Customer Invoice";
                        ViewBag.InvoiceNumber = CustomerInvoice.InvoiceNumber;
                        ViewBag.RecapDate = InvoiceDate;
                        ViewBag.CustomerName = CustomerInvoice.CustomerName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = approval.ContactName;

                        return View();

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Delivery Note & Kanban Order";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }
        public ActionResult CustomerInvoiceApproval(string InvoiceNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Customer Invoice Approval";

            try
            {

                if (InvoiceNumber == null || uid == null)
                {
                    InvoiceNumber = Session["InvoiceNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["InvoiceNumber"] = InvoiceNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_CustomerInvoice CustomerInvoice = vssp_db.Vw_ACC_CustomerInvoice.Where(a => a.InvoiceNumber == InvoiceNumber).FirstOrDefault();
                    UserEditModel user = accountService.UserEditList(cryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_ACC_CustomerInvoiceApproval approval = vssp_db.Tbl_ACC_CustomerInvoiceApproval.Where(a => a.InvoiceNumber == InvoiceNumber && a.UserId == user.UserID).FirstOrDefault();

                    if (CustomerInvoice != null && user != null && approval != null)
                    {

                        string InvoiceDate = systemService.Vd(CustomerInvoice.InvoiceDate.ToString(), "MMMM dd, yyyy");

                        ViewBag.InvoiceTitle = "Customer Invoice Approval";
                        ViewBag.InvoiceNumber = CustomerInvoice.InvoiceNumber;
                        ViewBag.InvoiceDate = InvoiceDate;
                        ViewBag.CustomerName = CustomerInvoice.CustomerName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        if (approval.Approved == false)
                        {
                            return View();
                        }
                        else
                        {
                            ViewBag.ApprovedDate = systemService.Vd(approval.ApprovedDate.ToString(), "dd MMMM, yyyy");
                            return View("CustomerInvoiceApproved");
                        }

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Customer Invoice Approval";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage =errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }

        public ActionResult CustomerInvoiceCustomer(string InvoiceNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Customer Invoice List";

            try
            {

                if (InvoiceNumber == null || uid == null)
                {
                    InvoiceNumber = Session["InvoiceNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["InvoiceNumber"] = InvoiceNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_CustomerInvoice CustomerInvoice = vssp_db.Vw_ACC_CustomerInvoice.Where(a => a.InvoiceNumber == InvoiceNumber).FirstOrDefault();
                    //UserEditModel user = _AccountService.UserEditList(_CryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();
                    Tbl_MST_CustomerContact approval = vssp_db.Tbl_MST_CustomerContact.Where(a => a.CustomerId == CustomerInvoice.CustomerId && a.Email == uid).FirstOrDefault();

                    if (CustomerInvoice != null && approval != null)
                    {

                        string InvoiceDate = systemService.Vd(CustomerInvoice.InvoiceDate.ToString(), "MMMM dd, yyyy");

                        ViewBag.InvoiceTitle = "Customer Invoice List";
                        ViewBag.InvoiceNumber = CustomerInvoice.InvoiceNumber;
                        ViewBag.RecapDate = InvoiceDate;
                        ViewBag.CustomerName = CustomerInvoice.CustomerName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = approval.ContactName;

                        return View();

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Customer Invoice List";
                        ViewBag.UserName = null;

                        return View();

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }

        public ActionResult CustomerInvoiceApproved(string InvoiceNumber, string uid)
        {
            Session["Layout"] = "mainindex";
            ViewBag.Title = "Customer Invoice List Approval";

            try
            {

                if (InvoiceNumber == null || uid == null)
                {
                    InvoiceNumber = Session["InvoiceNumber"].ToString();
                    uid = Session["uid"].ToString();
                }
                else
                {
                    Session["InvoiceNumber"] = InvoiceNumber;
                    Session["uid"] = uid;
                }

                if (Session["CompID"] == null)
                {
                    return RedirectToAction("GetSessionInfo", "System", new { urladdress = Request.RawUrl });
                }
                else
                {
                    Vw_ACC_CustomerInvoice CustomerInvoice = vssp_db.Vw_ACC_CustomerInvoice.Where(a => a.InvoiceNumber == InvoiceNumber).FirstOrDefault();
                    UserEditModel user = accountService.UserEditList(cryptoLibService.Sha256Crypto(uid, "Decrypt")).FirstOrDefault();

                    if (CustomerInvoice != null && user != null)
                    {

                        string InvoiceDate = systemService.Vd(CustomerInvoice.InvoiceDate.ToString(), "MMMM dd, yyyy");

                        ViewBag.RecapTitle = "Customer Invoice List";
                        ViewBag.InvoiceNumber = CustomerInvoice.InvoiceNumber;
                        ViewBag.InvoiceDate = InvoiceDate;
                        ViewBag.CustomerName = CustomerInvoice.CustomerName;
                        ViewBag.UserID = uid;
                        ViewBag.UserName = user.UserName;

                        crudCustomerInvoiceApproval(user.UserID, CustomerInvoice.InvoiceNumber, user.UserID, "Approved");
                        return RedirectToAction("ContinuePage", "System", new { cmessage = "Successfuly Approved " + ViewBag.RecapTitle + " \n " + InvoiceNumber, caction = "Dashboard", ccontroller = "Home", capps = "Home" });

                    }
                    else
                    {
                        ViewBag.OrderTitle = "Customer Invoice List";
                        ViewBag.UserName = null;

                        return RedirectToAction("ErrorPage", "System", new { errnumber = "500", errmessage = "Order or User not valid.", backaction = "MainIndex", backcontroller = "Index" });

                    }

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                ModelState.AddModelError("", errinfo);
                return RedirectToAction("ErrorPage", "System", new { errnumber = Response.StatusCode, errmessage = errinfo, backaction = "MainIndex", backcontroller = "Index" });
            }

        }
        public ActionResult GetInvoiceNumber(string CustomerId, DateTime InvoiceDate)
        {
            string compid = Session["CompID"].ToString();

            SP_GET_CustomerInvoiceNumber_Result invNumber = vssp_db.SP_GET_CustomerInvoiceNumber(CustomerId, InvoiceDate, compid).FirstOrDefault();
            return Json(invNumber, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GeneralLedgerReceivable()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();

                var acccessPreviliege = accountService.AccessPreviliege(uid, "FinanceAccounting", "GeneralLedgerReceivable");
                var menu = systemService.SidebarEditList(acccessPreviliege.MenuID == null ? "" : acccessPreviliege.MenuID).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                if (menu.Confidential == true && acccessPreviliege.ConfidentialAccess == false)
                {
                    return RedirectToAction("ConfidentialAccess", "System");
                }
                else
                {
                    ViewBag.Title = systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.DateTime = DateTime.Now;

                    Session["Layout"] = "portal";
                    var stockTakingEvent = systemService.GetStockTakingEvent();

                    if (stockTakingEvent == null)
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";
                    }

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = systemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult GeneralLedgerReceivableListJson(string month, string customer)
        {
            string InvoiceMonth = "";
            string InvoiceYears = "";

            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                InvoiceMonth = arrs[0];
                InvoiceYears = arrs[1];
            }
            else
            {
                InvoiceMonth = DateTime.Now.ToString("MM");
                InvoiceYears = DateTime.Now.ToString("yyyy");
            }

            var Ledger = (from a in vssp_db.Vw_ACC_CustomerInvoiceReceivable
                          where a.InvoiceYear.Contains(InvoiceYears) && a.InvoiceMonth.Contains(InvoiceMonth) && a.CustomerId.Contains(customer)
                          orderby a.TaxNumber
                          select a).ToList();

            //return Json(Ledger, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(Ledger, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;


        }
        public ActionResult PaymentCustomerInvoiceYearlyJson(string month, string customer)
        {
            string InvoiceMonth = "";
            string InvoiceYears = "";

            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                InvoiceMonth = arrs[0];
                InvoiceYears = arrs[1];
            }
            else
            {
                InvoiceMonth = DateTime.Now.ToString("MM");
                InvoiceYears = DateTime.Now.ToString("yyyy");
            }

            var Recap = (from a in vssp_db.Vw_ACC_CustomerInvoiceReceivableYearly
                          where a.Years.ToString().Contains(InvoiceYears) && a.CustomerId.Contains(customer)
                          orderby a.AccountCode == null ? a.CustomerId : a.AccountCode
                          select a).ToList();

            //return Json(Recap, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(Recap, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public ActionResult crudGeneralLedgerReceivable(string jsonData)
        {
            try
            {

                PostCustomerInvoiceReceivableModel postCustomerInvoiceReceivable = JsonConvert.DeserializeObject<PostCustomerInvoiceReceivableModel>(jsonData);
                Tbl_ACC_CustomerInvoiceReceivable CustomerInvoiceReceivable = postCustomerInvoiceReceivable.CustomerInvoiceReceivable;
                
                string uid = postCustomerInvoiceReceivable.uid;
                string formAction = postCustomerInvoiceReceivable.formAction.ToLower();

                switch (formAction)
                {
                    case "create":

                        Tbl_ACC_CustomerInvoiceReceivable ListCustomerInvoiceReceivable = new Tbl_ACC_CustomerInvoiceReceivable();
                        ListCustomerInvoiceReceivable.InvoiceNumber = CustomerInvoiceReceivable.InvoiceNumber;
                        ListCustomerInvoiceReceivable.TaxNumber = CustomerInvoiceReceivable.TaxNumber;
                        ListCustomerInvoiceReceivable.TransferDate = CustomerInvoiceReceivable.TransferDate;
                        ListCustomerInvoiceReceivable.PaymentSchedule = CustomerInvoiceReceivable.PaymentSchedule;
                        ListCustomerInvoiceReceivable.Paid = CustomerInvoiceReceivable.Paid;
                        ListCustomerInvoiceReceivable.PaymentAmount = CustomerInvoiceReceivable.PaymentAmount;
                        ListCustomerInvoiceReceivable.PaymentDate = CustomerInvoiceReceivable.PaymentDate;
                        ListCustomerInvoiceReceivable.Status = 0;
                        ListCustomerInvoiceReceivable.UserId = uid;
                        ListCustomerInvoiceReceivable.EditDate = DateTime.Now;

                        vssp_db.Tbl_ACC_CustomerInvoiceReceivable.Add(ListCustomerInvoiceReceivable);

                        /* update status customer invoice */
                        updateStatusCustomerInvoice(CustomerInvoiceReceivable.InvoiceNumber, formAction);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_ACC_CustomerInvoiceReceivable.First(a => a.InvoiceNumber == CustomerInvoiceReceivable.InvoiceNumber);

                        ListUpdate.TaxNumber = CustomerInvoiceReceivable.TaxNumber;
                        ListUpdate.TransferDate = CustomerInvoiceReceivable.TransferDate;
                        ListUpdate.PaymentSchedule = CustomerInvoiceReceivable.PaymentSchedule;
                        ListUpdate.Paid = CustomerInvoiceReceivable.Paid;
                        ListUpdate.PaymentAmount = CustomerInvoiceReceivable.PaymentAmount;
                        ListUpdate.PaymentDate = CustomerInvoiceReceivable.PaymentDate;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* update status customer invoice */
                        updateStatusCustomerInvoice(CustomerInvoiceReceivable.InvoiceNumber, formAction);

                        break;

                    case "delete":

                        /* remove existing CustomerInvoiceReceivable */
                        var ListDelete = vssp_db.Tbl_ACC_CustomerInvoiceReceivable.First(a => a.InvoiceNumber == CustomerInvoiceReceivable.InvoiceNumber);

                        vssp_db.Tbl_ACC_CustomerInvoiceReceivable.Remove(ListDelete);

                        /* update status customer invoice */
                        updateStatusCustomerInvoice(CustomerInvoiceReceivable.InvoiceNumber, formAction);

                        break;

                    case "closed":

                        var ListClosed = vssp_db.Tbl_ACC_CustomerInvoiceReceivable.First(a => a.TaxNumber == CustomerInvoiceReceivable.TaxNumber);
                        ListClosed.Status = 3;

                        /* update status customer invoice */
                        updateStatusCustomerInvoice(CustomerInvoiceReceivable.InvoiceNumber, formAction);

                        break;

                }

                try
                {
                    vssp_db.SaveChanges();
                    //return Json(CustomerInvoiceReceivable, JsonRequestBehavior.AllowGet);
                    var jsonResult = Json(CustomerInvoiceReceivable, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;

                }
                catch (DbEntityValidationException e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = systemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = systemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public void updateStatusCustomerDelivery(string donumber, string formaction)
        {
            var invoice = vssp_db.Tbl_TRS_DeliveryOrder.FirstOrDefault(a => a.DONumber == donumber);
            if (invoice != null)
            {
                switch (formaction.ToLower())
                {
                    case "create":
                        invoice.Status = 3;
                        break;
                    case "update":
                        invoice.Status = 3;
                        break;
                    case "canceled":
                        invoice.Status = 0;
                        break;
                    case "delete":
                        invoice.Status = 0;
                        break;
                }
            }
        }
        public void updateStatusCustomerInvoice(string invoicenumber,string formaction)
        {
            var invoice = vssp_db.Tbl_ACC_CustomerInvoice.First(a => a.InvoiceNumber == invoicenumber);
            switch (formaction.ToLower())
            {
                case "create":
                    invoice.Status = 3;
                    break;
                case "update":
                    invoice.Status = 3;
                    break;
                case "delete":
                    invoice.Status = 2;
                    break;
            }
        }
        public ActionResult CustomerInvoiceControl()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string ecc = Session["Email"].ToString();

                var acccessPreviliege = accountService.AccessPreviliege(uid, "FinanceAccounting", "CustomerInvoiceControl");
                var menu = systemService.SidebarEditList(acccessPreviliege.MenuID == null ? "" : acccessPreviliege.MenuID).FirstOrDefault();

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                if (menu.Confidential == true && acccessPreviliege.ConfidentialAccess == false)
                {
                    return RedirectToAction("ConfidentialAccess", "System");
                }
                else
                {
                    ViewBag.Title = systemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = systemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canConfidential = acccessPreviliege.ConfidentialAccess.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.EmailCC = ecc;
                    ViewBag.DateTime = DateTime.Now;

                    Session["Layout"] = "portal";
                    var stockTakingEvent = systemService.GetStockTakingEvent();

                    if (stockTakingEvent == null)
                    {
                        Session["InventoryStatus"] = "";
                        Session["InventoryCountTime"] = "";
                    }

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = systemService.ComboExport().ToList();

                    return View(exportOption);

                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult CustomerInvoiceControlListJson(
                                    string searchFilter,
                                    Nullable<DateTime> startdate = null,
                                    Nullable<DateTime> enddate = null,
                                    string month = null,
                                    Nullable<Boolean> invoiced = null)
        {
            searchFilter = systemService.Vf(searchFilter);
            var CustomerInvoiceControl = (from a in vssp_db.Vw_ACC_CustomerInvoiceDeliveryRecap
                                          join b in vssp_db.Tbl_ACC_CustomerInvoice on a.InvoiceNumber equals b.InvoiceNumber into invoice
                                          from b in invoice.DefaultIfEmpty()
                                          join c in vssp_db.Tbl_ACC_CustomerInvoiceReceivable on a.InvoiceNumber equals c.InvoiceNumber into receivable
                                          from c in receivable.DefaultIfEmpty()
                                          where a.InvoiceNumber.Contains(searchFilter) || a.DONumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.CustomerId.Contains(searchFilter)
                                          orderby a.DODate descending, a.DONumber
                                          select new
                                          {
                                              a.CustomerId,
                                              a.CustomerName,
                                              a.DONumber,
                                              a.DODate,
                                              a.UniqueNumber,
                                              a.PartNumber,
                                              a.PartName,
                                              a.DeliveryUnitQty,
                                              a.UnitLevel2,
                                              a.Price,
                                              a.Amount,
                                              a.InvoiceNumber,
                                              b.InvoiceDate,
                                              a.Invoiced,
                                              c.TaxNumber,
                                              c.Paid
                                          }).ToList();
            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;
                CustomerInvoiceControl = CustomerInvoiceControl.Where(a => a.DODate >= startdate && a.DODate <= enddate).ToList();
            }
            if (systemService.Vf(month) != "")
            {
                string[] arrs = month.Split('/');
                string InvoiceMonth = arrs[0];
                string InvoiceYears = arrs[1];
                CustomerInvoiceControl = CustomerInvoiceControl.Where(a => Convert.ToDateTime(a.DODate).ToString("MM") == InvoiceMonth && Convert.ToDateTime(a.DODate).ToString("yyyy") == InvoiceYears).ToList();
            }
            if (invoiced != null)
            {
                CustomerInvoiceControl = CustomerInvoiceControl.Where(a => a.Invoiced == invoiced).ToList();
            }

            var jsonResult = Json(CustomerInvoiceControl, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
    }  
}