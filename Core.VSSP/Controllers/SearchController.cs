using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Core.VSSP.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult SearchPartRawMaterial()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }
            return View();
        }
        public ActionResult SearchPartRawLineMaterial()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }
            return View();
        }
        public ActionResult SearchPartFinishGoods()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }
            return View();
        }
        public ActionResult SearchPartFinishGoodsLine()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }
            return View();
        }
        public ActionResult SearchPartStockTaking()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }
            return View();
        }
        public ActionResult SearchPartStockRawReturn()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }
            return View();
        }
        public ActionResult _FilterPartial()
        {

            return PartialView();

        }

    }
}