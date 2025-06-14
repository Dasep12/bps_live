using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.Services;
using Core.VSSP.Models;
using Core.VSSP.WorkEntity;

namespace Core.VSSP.Controllers
{
    public class EmailController : Controller
    {
        // GET: Approval
        SystemService       _SystemService      = new SystemService();
        CryptoLibService    _CryptoLibService   = new CryptoLibService();
        AccountService      _AccountService     = new AccountService();
        vssp_entity         vssp_db             = new vssp_entity();

        public ActionResult SentApproval(string FormAction, string[] SentTo, string CopyTo, string Subject, string Content, string OrderNumber)
        {

            List<Tbl_TRS_ForecastOrderApproval> _ForecastOrderApprovals = new List<Tbl_TRS_ForecastOrderApproval>();
            List<Tbl_TRS_StockTakingApproval> _StockTakingApprovals = new List<Tbl_TRS_StockTakingApproval>();
            List<Tbl_TRS_StockAdjustmentApproval> _StockAdjustmentApprovals = new List<Tbl_TRS_StockAdjustmentApproval>();
            List<Tbl_ACC_InvoiceRecapApproval> _InvoiceRecapApprovals = new List<Tbl_ACC_InvoiceRecapApproval>();
            List<Tbl_ACC_CustomerInvoiceApproval> _CustomerInvoiceApprovals = new List<Tbl_ACC_CustomerInvoiceApproval>();
            List<Tbl_TRS_ControlPlanningApproval> _ControlPlanningApprovals = new List<Tbl_TRS_ControlPlanningApproval>();
            List<Tbl_TRS_RequestOrderPartsApproval> _RequestOrderPartsApprovals = new List<Tbl_TRS_RequestOrderPartsApproval>();

            var approvalresult = new object();
            string sender = Session["UserName"].ToString();

            //string[] sentList = SentTo.Split(',');            

            switch (FormAction)
            {
                case "ForecastApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_TRS_ForecastOrderApproval __ForecastOrderApprovals = new Tbl_TRS_ForecastOrderApproval();
                            __ForecastOrderApprovals.OrderNumber = OrderNumber;
                            __ForecastOrderApprovals.UserId = userdetails.UserID;
                            __ForecastOrderApprovals.UserName = userdetails.UserName;
                            __ForecastOrderApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("ForecastApproval", "Purchase", new { ordernumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__ForecastOrderApprovals.UserId,"Part Requirement List",sender, Subject,Content,url);

                            var ListSent = vssp_db.Tbl_TRS_ForecastOrderApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                vssp_db.SaveChanges();

                                __ForecastOrderApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __ForecastOrderApprovals.ApprovalName = ListSent.ApprovalName;
                                __ForecastOrderApprovals.SentEmail = true;
                                __ForecastOrderApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __ForecastOrderApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __ForecastOrderApprovals.ApprovalName = ListSent.ApprovalName;
                                __ForecastOrderApprovals.SentEmail = false;
                                __ForecastOrderApprovals.SentEmailDate = DateTime.Now;

                            }

                            _ForecastOrderApprovals.Add(__ForecastOrderApprovals);
                        }
                    }

                    approvalresult = _ForecastOrderApprovals;
                    break;

                case "ForecastSupplier":

                    foreach (string sent in SentTo)
                    {

                        Tbl_TRS_ForecastOrder order = vssp_db.Tbl_TRS_ForecastOrder.Where(a => a.OrderNumber == OrderNumber).FirstOrDefault();
                        Tbl_MST_SupplierContact _SupplierContact = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == order.SupplierId && a.Email == sent).FirstOrDefault();
                        Tbl_TRS_ForecastOrderApproval __ForecastOrderApprovals = new Tbl_TRS_ForecastOrderApproval();
                        
                        __ForecastOrderApprovals.OrderNumber = OrderNumber;
                        __ForecastOrderApprovals.UserId = _SupplierContact.Email;
                        __ForecastOrderApprovals.UserName = _SupplierContact.ContactName;
                        __ForecastOrderApprovals.ApprovalEmail = _SupplierContact.Email;
                        __ForecastOrderApprovals.ApprovalLevel = 9;
                        __ForecastOrderApprovals.ApprovalName = sent;

                        string huid = sent;
                        string url = Url.Action("ForecastSupplier", "Purchase", new { ordernumber = OrderNumber, uid = huid });
                        var emailresult = SentEmail(sent, CopyTo, Subject, _SupplierContact.ContactName, Content, url);
                        if (emailresult == true)
                        {
                            
                            //Tbl_TRS_ForecastOrder order = vssp_db.Tbl_TRS_ForecastOrder.Where(a => a.OrderNumber == OrderNumber).FirstOrDefault();
                            order.Status = 1;
                            vssp_db.SaveChanges();

                            __ForecastOrderApprovals.SentEmail = true;
                            __ForecastOrderApprovals.SentEmailDate = DateTime.Now;

                        } else
                        {
                            __ForecastOrderApprovals.SentEmail = false;
                            __ForecastOrderApprovals.SentEmailDate = DateTime.Now;

                        }
                        _ForecastOrderApprovals.Add(__ForecastOrderApprovals);

                    }

                    approvalresult = _ForecastOrderApprovals;
                    break;

                case "SupplierOrder":

                    foreach (string sent in SentTo)
                    {

                        Tbl_TRS_SupplierOrder order = vssp_db.Tbl_TRS_SupplierOrder.Where(a => a.OrderNumber == OrderNumber).FirstOrDefault();
                        Tbl_MST_SupplierContact _SupplierContact = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId==order.SupplierId && a.Email==sent).FirstOrDefault();
                        Tbl_TRS_ForecastOrderApproval __ForecastOrderApprovals = new Tbl_TRS_ForecastOrderApproval();
                        
                        __ForecastOrderApprovals.OrderNumber = OrderNumber;
                        __ForecastOrderApprovals.UserId = _SupplierContact.Email;
                        __ForecastOrderApprovals.UserName = _SupplierContact.ContactName;
                        __ForecastOrderApprovals.ApprovalEmail = _SupplierContact.Email;
                        __ForecastOrderApprovals.ApprovalLevel = 9;
                        __ForecastOrderApprovals.ApprovalName = sent;

                        string huid = sent;
                        string url = Url.Action("SupplierOrderMail", "Purchase", new { ordernumber = OrderNumber, uid = huid });
                        var emailresult = SentEmail(sent, CopyTo, Subject, _SupplierContact.ContactName, Content, url);
                        if (emailresult == true)
                        {

                            order.Status = 1;
                            vssp_db.SaveChanges();

                            __ForecastOrderApprovals.SentEmail = true;
                            __ForecastOrderApprovals.SentEmailDate = DateTime.Now;

                        }
                        else
                        {
                            __ForecastOrderApprovals.SentEmail = false;
                            __ForecastOrderApprovals.SentEmailDate = DateTime.Now;

                        }
                        _ForecastOrderApprovals.Add(__ForecastOrderApprovals);

                    }

                    approvalresult = _ForecastOrderApprovals;
                    break;

                case "ReturnPart":

                    foreach (string sent in SentTo)
                    {

                        Tbl_TRS_ReturnPart returnpart = vssp_db.Tbl_TRS_ReturnPart.Where(a => a.ReturnNumber == OrderNumber).FirstOrDefault();
                        Tbl_MST_SupplierContact _SupplierContact = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == returnpart.SupplierId && a.Email == sent).FirstOrDefault();
                        Tbl_TRS_ForecastOrderApproval __ForecastOrderApprovals = new Tbl_TRS_ForecastOrderApproval();

                        __ForecastOrderApprovals.OrderNumber = OrderNumber;
                        __ForecastOrderApprovals.UserId = _SupplierContact.Email;
                        __ForecastOrderApprovals.UserName = _SupplierContact.ContactName;
                        __ForecastOrderApprovals.ApprovalEmail = _SupplierContact.Email;
                        __ForecastOrderApprovals.ApprovalLevel = 9;
                        __ForecastOrderApprovals.ApprovalName = sent;

                        string huid = sent;
                        string url = Url.Action("ReturnPartMail", "Purchase", new { returnnumber = OrderNumber, uid = huid });
                        var emailresult = SentEmail(sent, CopyTo, Subject, _SupplierContact.ContactName, Content, url);
                        if (emailresult == true)
                        {

                            returnpart.Status = 1;
                            vssp_db.SaveChanges();

                            __ForecastOrderApprovals.SentEmail = true;
                            __ForecastOrderApprovals.SentEmailDate = DateTime.Now;

                        }
                        else
                        {
                            __ForecastOrderApprovals.SentEmail = false;
                            __ForecastOrderApprovals.SentEmailDate = DateTime.Now;

                        }
                        _ForecastOrderApprovals.Add(__ForecastOrderApprovals);

                    }

                    approvalresult = _ForecastOrderApprovals;
                    break;

                case "StockTakingApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_TRS_StockTaking stockTaking = vssp_db.Tbl_TRS_StockTaking.Where(a => a.InventoryNumber == OrderNumber).FirstOrDefault();
                            Tbl_TRS_StockTakingApproval __StockTakingApprovals = new Tbl_TRS_StockTakingApproval();
                            __StockTakingApprovals.InventoryNumber = OrderNumber;
                            __StockTakingApprovals.UserId = userdetails.UserID;
                            __StockTakingApprovals.UserName = userdetails.UserName;
                            __StockTakingApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("StockTakingApproval", "Inventory", new { inventorynumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__StockTakingApprovals.UserId, "Stock Taking", sender, Subject, Content, url);
                            var ListSent = vssp_db.Tbl_TRS_StockTakingApproval.First(a => a.InventoryNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                stockTaking.Status = 1;

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                vssp_db.SaveChanges();

                                __StockTakingApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __StockTakingApprovals.ApprovalName = ListSent.ApprovalName;
                                __StockTakingApprovals.SentEmail = true;
                                __StockTakingApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __StockTakingApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __StockTakingApprovals.ApprovalName = ListSent.ApprovalName;
                                __StockTakingApprovals.SentEmail = false;
                                __StockTakingApprovals.SentEmailDate = DateTime.Now;

                            }

                            _StockTakingApprovals.Add(__StockTakingApprovals);
                        }
                    }

                    approvalresult = _StockTakingApprovals;
                    break;

                case "StockAdjustmentApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_TRS_StockAdjustment stockAdjustment = vssp_db.Tbl_TRS_StockAdjustment.Where(a => a.AdjustmentNumber == OrderNumber).FirstOrDefault();
                            Tbl_TRS_StockAdjustmentApproval __StockAdjustmentApprovals = new Tbl_TRS_StockAdjustmentApproval();
                            __StockAdjustmentApprovals.AdjustmentNumber = OrderNumber;
                            __StockAdjustmentApprovals.UserId = userdetails.UserID;
                            __StockAdjustmentApprovals.UserName = userdetails.UserName;
                            __StockAdjustmentApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("StockAdjustmentApproval", "Inventory", new { Adjustmentnumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__StockAdjustmentApprovals.UserId, "Stock Adjustment", sender, Subject, Content, url);
                            var ListSent = vssp_db.Tbl_TRS_StockAdjustmentApproval.First(a => a.AdjustmentNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                stockAdjustment.Status = 1;

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                vssp_db.SaveChanges();

                                __StockAdjustmentApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __StockAdjustmentApprovals.ApprovalName = ListSent.ApprovalName;
                                __StockAdjustmentApprovals.SentEmail = true;
                                __StockAdjustmentApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __StockAdjustmentApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __StockAdjustmentApprovals.ApprovalName = ListSent.ApprovalName;
                                __StockAdjustmentApprovals.SentEmail = false;
                                __StockAdjustmentApprovals.SentEmailDate = DateTime.Now;

                            }

                            _StockAdjustmentApprovals.Add(__StockAdjustmentApprovals);
                        }
                    }

                    approvalresult = _StockAdjustmentApprovals;
                    break;
                case "InvoiceRecapApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_ACC_InvoiceRecapApproval __InvoiceRecapApprovals = new Tbl_ACC_InvoiceRecapApproval();
                            __InvoiceRecapApprovals.RecapNumber = OrderNumber;
                            __InvoiceRecapApprovals.UserId = userdetails.UserID;
                            __InvoiceRecapApprovals.UserName = userdetails.UserName;
                            __InvoiceRecapApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("InvoiceRecapApproval", "FinanceAccounting", new { RecapNumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__InvoiceRecapApprovals.UserId, "Invoice Recap", sender, Subject, Content, url);
                            var ListSent = vssp_db.Tbl_ACC_InvoiceRecapApproval.First(a => a.RecapNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                vssp_db.SaveChanges();

                                __InvoiceRecapApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __InvoiceRecapApprovals.ApprovalName = ListSent.ApprovalName;
                                __InvoiceRecapApprovals.SentEmail = true;
                                __InvoiceRecapApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __InvoiceRecapApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __InvoiceRecapApprovals.ApprovalName = ListSent.ApprovalName;
                                __InvoiceRecapApprovals.SentEmail = false;
                                __InvoiceRecapApprovals.SentEmailDate = DateTime.Now;

                            }

                            _InvoiceRecapApprovals.Add(__InvoiceRecapApprovals);
                        }
                    }

                    approvalresult = _InvoiceRecapApprovals;
                    break;

                case "InvoiceRecapSupplier":

                    foreach (string sent in SentTo)
                    {

                        Tbl_ACC_InvoiceRecap order = vssp_db.Tbl_ACC_InvoiceRecap.Where(a => a.RecapNumber == OrderNumber).FirstOrDefault();
                        Tbl_MST_SupplierContact _SupplierContact = vssp_db.Tbl_MST_SupplierContact.Where(a => a.SupplierId == order.SupplierId && a.Email == sent).FirstOrDefault();
                        Tbl_ACC_InvoiceRecapApproval __InvoiceRecapApprovals = new Tbl_ACC_InvoiceRecapApproval();

                        __InvoiceRecapApprovals.RecapNumber = OrderNumber;
                        __InvoiceRecapApprovals.UserId = _SupplierContact.Email;
                        __InvoiceRecapApprovals.UserName = _SupplierContact.ContactName;
                        __InvoiceRecapApprovals.ApprovalEmail = _SupplierContact.Email;
                        __InvoiceRecapApprovals.ApprovalLevel = 9;
                        __InvoiceRecapApprovals.ApprovalName = sent;

                        string huid = sent;
                        string url = Url.Action("InvoiceRecapSupplier", "FinanceAccounting", new { RecapNumber = OrderNumber, uid = huid });
                        var emailresult = SentEmail(sent, CopyTo, Subject, _SupplierContact.ContactName, Content, url);
                        if (emailresult == true)
                        {

                            Tbl_ACC_InvoiceRecap Recap = vssp_db.Tbl_ACC_InvoiceRecap.Where(a => a.RecapNumber == OrderNumber).FirstOrDefault();
                            Recap.Status = 1;
                            vssp_db.SaveChanges();

                            __InvoiceRecapApprovals.SentEmail = true;
                            __InvoiceRecapApprovals.SentEmailDate = DateTime.Now;

                        }
                        else
                        {
                            __InvoiceRecapApprovals.SentEmail = false;
                            __InvoiceRecapApprovals.SentEmailDate = DateTime.Now;

                        }
                        _InvoiceRecapApprovals.Add(__InvoiceRecapApprovals);

                    }

                    approvalresult = _InvoiceRecapApprovals;
                    break;
                case "CustomerInvoiceApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_ACC_CustomerInvoiceApproval __CustomerInvoiceApprovals = new Tbl_ACC_CustomerInvoiceApproval();
                            __CustomerInvoiceApprovals.InvoiceNumber = OrderNumber;
                            __CustomerInvoiceApprovals.UserId = userdetails.UserID;
                            __CustomerInvoiceApprovals.UserName = userdetails.UserName;
                            __CustomerInvoiceApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("CustomerInvoiceApproval", "FinanceAccounting", new { InvoiceNumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__CustomerInvoiceApprovals.UserId, "Customer Invoice", sender, Subject, Content, url);
                            var ListSent = vssp_db.Tbl_ACC_CustomerInvoiceApproval.First(a => a.InvoiceNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                vssp_db.SaveChanges();

                                __CustomerInvoiceApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __CustomerInvoiceApprovals.ApprovalName = ListSent.ApprovalName;
                                __CustomerInvoiceApprovals.SentEmail = true;
                                __CustomerInvoiceApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __CustomerInvoiceApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __CustomerInvoiceApprovals.ApprovalName = ListSent.ApprovalName;
                                __CustomerInvoiceApprovals.SentEmail = false;
                                __CustomerInvoiceApprovals.SentEmailDate = DateTime.Now;

                            }

                            _CustomerInvoiceApprovals.Add(__CustomerInvoiceApprovals);
                        }
                    }

                    if (_CustomerInvoiceApprovals.Count() > 0)
                    {
                        Tbl_ACC_CustomerInvoice Recap = vssp_db.Tbl_ACC_CustomerInvoice.Where(a => a.InvoiceNumber == OrderNumber).FirstOrDefault();
                        Recap.Status = 1;
                        vssp_db.SaveChanges();

                    }

                    approvalresult = _CustomerInvoiceApprovals;

                    break;
                case "CustomerInvoiceCustomer":

                    foreach (string sent in SentTo)
                    {

                        Tbl_ACC_CustomerInvoice order = vssp_db.Tbl_ACC_CustomerInvoice.Where(a => a.InvoiceNumber == OrderNumber).FirstOrDefault();
                        Tbl_MST_CustomerContact _CustomerContact = vssp_db.Tbl_MST_CustomerContact.Where(a => a.CustomerId == order.CustomerId && a.Email == sent).FirstOrDefault();
                        Tbl_ACC_CustomerInvoiceApproval __CustomerInvoiceApprovals = new Tbl_ACC_CustomerInvoiceApproval();

                        __CustomerInvoiceApprovals.InvoiceNumber = OrderNumber;
                        __CustomerInvoiceApprovals.UserId = _CustomerContact.Email;
                        __CustomerInvoiceApprovals.UserName = _CustomerContact.ContactName;
                        __CustomerInvoiceApprovals.ApprovalEmail = _CustomerContact.Email;
                        __CustomerInvoiceApprovals.ApprovalLevel = 9;
                        __CustomerInvoiceApprovals.ApprovalName = sent;

                        string huid = sent;
                        string url = Url.Action("CustomerInvoiceCustomer", "FinanceAccounting", new { RecapNumber = OrderNumber, uid = huid });
                        var emailresult = SentEmail(sent, CopyTo, Subject, _CustomerContact.ContactName, Content, url);
                        if (emailresult == true)
                        {

                            Tbl_ACC_CustomerInvoice Recap = vssp_db.Tbl_ACC_CustomerInvoice.Where(a => a.InvoiceNumber == OrderNumber).FirstOrDefault();
                            Recap.Status = 1;
                            vssp_db.SaveChanges();

                            __CustomerInvoiceApprovals.SentEmail = true;
                            __CustomerInvoiceApprovals.SentEmailDate = DateTime.Now;

                        }
                        else
                        {
                            __CustomerInvoiceApprovals.SentEmail = false;
                            __CustomerInvoiceApprovals.SentEmailDate = DateTime.Now;

                        }
                        _CustomerInvoiceApprovals.Add(__CustomerInvoiceApprovals);

                    }

                    approvalresult = _CustomerInvoiceApprovals;
                    break;
                case "ControlPlanningApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_TRS_ControlPlanningApproval __ControlPlanningApprovals = new Tbl_TRS_ControlPlanningApproval();
                            __ControlPlanningApprovals.OrderNumber = OrderNumber;
                            __ControlPlanningApprovals.UserId = userdetails.UserID;
                            __ControlPlanningApprovals.UserName = userdetails.UserName;
                            __ControlPlanningApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("ControlPlanningApproval", "Productions", new { ordernumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__ControlPlanningApprovals.UserId, "Production Planning", sender, Subject, Content, url);

                            var ListSent = vssp_db.Tbl_TRS_ControlPlanningApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                vssp_db.SaveChanges();

                                __ControlPlanningApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __ControlPlanningApprovals.ApprovalName = ListSent.ApprovalName;
                                __ControlPlanningApprovals.SentEmail = true;
                                __ControlPlanningApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __ControlPlanningApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __ControlPlanningApprovals.ApprovalName = ListSent.ApprovalName;
                                __ControlPlanningApprovals.SentEmail = false;
                                __ControlPlanningApprovals.SentEmailDate = DateTime.Now;

                            }

                            _ControlPlanningApprovals.Add(__ControlPlanningApprovals);
                        }
                    }

                    approvalresult = _ControlPlanningApprovals;
                    break;

                case "RequestOrderPartsApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_TRS_RequestOrderPartsApproval __RequestOrderPartsApprovals = new Tbl_TRS_RequestOrderPartsApproval();
                            __RequestOrderPartsApprovals.OrderNumber = OrderNumber;
                            __RequestOrderPartsApprovals.UserId = userdetails.UserID;
                            __RequestOrderPartsApprovals.UserName = userdetails.UserName;
                            __RequestOrderPartsApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("RequestOrderPartsApproval", "Purchase", new { ordernumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__RequestOrderPartsApprovals.UserId, "Request Order Parts", sender, Subject, Content, url);

                            var ListSent = vssp_db.Tbl_TRS_RequestOrderPartsApproval.First(a => a.OrderNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                Tbl_TRS_RequestOrderParts ListOrder = vssp_db.Tbl_TRS_RequestOrderParts.First(a => a.OrderNumber == OrderNumber);
                                if (ListOrder.Status < 1)
                                {
                                    ListOrder.Status = 1;
                                }

                                vssp_db.SaveChanges();

                                __RequestOrderPartsApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __RequestOrderPartsApprovals.ApprovalName = ListSent.ApprovalName;
                                __RequestOrderPartsApprovals.SentEmail = true;
                                __RequestOrderPartsApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __RequestOrderPartsApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __RequestOrderPartsApprovals.ApprovalName = ListSent.ApprovalName;
                                __RequestOrderPartsApprovals.SentEmail = false;
                                __RequestOrderPartsApprovals.SentEmailDate = DateTime.Now;

                            }

                            _RequestOrderPartsApprovals.Add(__RequestOrderPartsApprovals);
                        }
                    }

                    approvalresult = _RequestOrderPartsApprovals;
                    break;

                case "ProblemInformationApproval":

                    foreach (string sent in SentTo)
                    {
                        UserEditModel userdetails = _AccountService.UserEditList(sent).Find(a => a.UserID == sent);

                        if (userdetails != null)
                        {
                            Tbl_QC_ProblemInformationApproval __ProblemInformationApprovals = new Tbl_QC_ProblemInformationApproval();
                            __ProblemInformationApprovals.ProblemNumber = OrderNumber;
                            __ProblemInformationApprovals.UserId = userdetails.UserID;
                            __ProblemInformationApprovals.UserName = userdetails.UserName;
                            __ProblemInformationApprovals.ApprovalEmail = userdetails.Email;

                            string huid = _CryptoLibService.Sha256Crypto(userdetails.UserID, "Encrypt");
                            string url = Url.Action("ProblemInformationApproval", "Quality", new { problemnumber = OrderNumber, uid = huid });
                            //var emailresult = SentEmail(userdetails.Email, CopyTo, Subject, userdetails.UserName, Content, url);
                            var emailresult = _SystemService.CrudNotification(__ProblemInformationApprovals.UserId, "Problem Information", sender, Subject, Content, url);

                            var ListSent = vssp_db.Tbl_QC_ProblemInformationApproval.First(a => a.ProblemNumber == OrderNumber && a.UserId == userdetails.UserID);

                            if (emailresult == true)
                            {

                                ListSent.SentEmail = true;
                                ListSent.SentEmailDate = DateTime.Now;

                                vssp_db.SaveChanges();

                                __ProblemInformationApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __ProblemInformationApprovals.ApprovalName = ListSent.ApprovalName;
                                __ProblemInformationApprovals.SentEmail = true;
                                __ProblemInformationApprovals.SentEmailDate = DateTime.Now;


                            }
                            else
                            {

                                __ProblemInformationApprovals.ApprovalLevel = ListSent.ApprovalLevel;
                                __ProblemInformationApprovals.ApprovalName = ListSent.ApprovalName;
                                __ProblemInformationApprovals.SentEmail = false;
                                __ProblemInformationApprovals.SentEmailDate = DateTime.Now;

                            }

                        }
                    }

                    approvalresult = _ForecastOrderApprovals;
                    break;
            }


            return Json(approvalresult, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ConfirmApproval(string FormAction, string ApprovalId, string OrderNumber, bool Approved)
        {
            return View();
        }
        public bool SentEmail(
            string emailto, string emailcc, 
            string emailsubject, string emailusername, 
            string emailcontent, string urlconfirm
            )
        {
            bool iresult = false;
            urlconfirm = this.GetBaseUrl() + urlconfirm;

            try
            {

                string body = string.Empty;
                DateTime now = DateTime.Now;
                string copyrightyear = now.ToString("yyyy");

                emailcontent = HttpUtility.UrlDecode(emailcontent, System.Text.Encoding.Default);
                emailcontent = emailcontent.Replace("{urlapi}", urlconfirm);

                using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Shared/_VSSPLayout/EmailTemplates/Approval/Orders.html")))
                {
                    body = reader.ReadToEnd();
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                      | SecurityProtocolType.Tls11
                      | SecurityProtocolType.Tls12;

                body = body.Replace("{username}", emailusername); //replacing the required things  
                body = body.Replace("{content}", System.Text.RegularExpressions.Regex.Unescape(emailcontent));
                body = body.Replace("{appname}", Session["AppID"].ToString());
                body = body.Replace("{compid}", Session["CompID"].ToString());
                body = body.Replace("{compname}", Session["CompName"].ToString());
                body = body.Replace("{compphone}", Session["CompPhone1"].ToString());
                body = body.Replace("{compaddress}", Session["CompAddress"].ToString());
                body = body.Replace("{compcity}", Session["CompCity"].ToString());
                body = body.Replace("{compcountry}", Session["CompCountry"].ToString());
                body = body.Replace("{compwebsites}", Session["CompWebsites"].ToString());
                body = body.Replace("{copyrightyear}", copyrightyear);
                body = body.Replace("{baseurl}", this.GetBaseUrl());
                var emailconfig = _SystemService.GetEmailConfiguration(null);

                var senderEmail = new MailAddress(emailconfig.EmailAddress.Replace(";",","), emailconfig.SenderName);
                var receiverEmail = new MailAddress(emailto, emailusername);
                var userid = emailconfig.EmailUserID;
                var password = emailconfig.EmailPassword;
                var bodymail = body;
                var smtp = new SmtpClient
                {
                    Host = emailconfig.OutgoingServer,
                    Port = emailconfig.OutgoingPort,
                    EnableSsl = emailconfig.EnableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(userid, password)
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    IsBodyHtml = true,
                    Subject = emailsubject,
                    Body = bodymail
                })
                {
                    mess.CC.Add(emailcc);
                    smtp.Send(mess);
                }

                iresult = true;

            }
            catch (Exception e)
            {
                var errinfo = _SystemService.GetExceptionDetails(e);
                TempData["Error"] = errinfo;
                iresult = false;
            }
            return iresult;
        }

        public string GetBaseUrl()
        {
            try
            {
                var urlschm = HttpContext.Request.Url.Scheme;
                var urlhost = HttpContext.Request.Url.Host;
                var urlport = HttpContext.Request.Url.Port;
                var baseurl = urlschm + "://" + urlhost;
                if (urlport.ToString() != "")
                {
                    baseurl += ":" + urlport;
                }

                return baseurl;

            }
            catch
            {
                string defaulturl = "";
                if (Session != null)
                {
                    defaulturl = Session["CompWebsites"].ToString();
                }
                else
                {
                    defaulturl = "127.0.0.0";
                }
                return defaulturl;
            }
        }
    }
}