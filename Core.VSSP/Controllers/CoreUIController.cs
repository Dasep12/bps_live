using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Controllers
{
    [Route("CoreUI")]
    public class CoreUIController : Controller
    {
        [Route("{view=Index}")]
        public ActionResult Index(string view)
        {
            ViewData["Title"] = "CoreUI Free Bootstrap Admin Template";
            Session["Layout"] = "portal";
            return View(view);
        }
    }
}