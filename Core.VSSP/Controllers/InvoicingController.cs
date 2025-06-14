using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Controllers
{
    public class InvoicingController : Controller
    {
        // GET: Invoicing
        SystemService systemService = new SystemService();
        AccountService accountService = new AccountService();
        vssp_entity vssp_db = new vssp_entity();
        
    }
}