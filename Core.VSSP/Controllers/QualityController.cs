using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ZXing;

namespace Core.VSSP.Controllers
{
    public class QualityController : Controller
    {
        // GET: Quality
        AccountService _AccountService = new AccountService();
        SystemService _SystemService = new SystemService();
        EmailController emailController = new EmailController();
        vssp_entity vssp_db = new vssp_entity();

        public ActionResult DefectCategory()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "DefectCategory");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;


                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult DefectCategoryJson(string id, string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            var DefectCategory = (from a in vssp_db.Tbl_MST_DefectCategory
                                  where a.Actived == isActive && (a.CategoryId.Contains(searchFilter) || a.CategoryName.Contains(searchFilter))
                                  orderby a.CategoryId
                                  select new { a.CategoryId, a.CategoryName, a.Actived, a.UserId, a.EditDate }).ToList();

            return Json(DefectCategory, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudDefectCategory(string Categoryid, string Categoryname, bool actived, string uid, string formAction)
        {

            try
            {

                switch (formAction.ToLower())
                {
                    case "create":

                        Tbl_MST_DefectCategory ListDefectCategory = new Tbl_MST_DefectCategory();
                        ListDefectCategory.CategoryId = Categoryid;
                        ListDefectCategory.CategoryName = Categoryname;
                        ListDefectCategory.Actived = actived;
                        ListDefectCategory.UserId = uid;
                        ListDefectCategory.EditDate = DateTime.Now;

                        vssp_db.Tbl_MST_DefectCategory.Add(ListDefectCategory);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_MST_DefectCategory.First(a => a.CategoryId == Categoryid);

                        ListUpdate.CategoryName = Categoryname;
                        ListUpdate.Actived = actived;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        break;

                    case "delete":

                        var ListDelete = vssp_db.Tbl_MST_DefectCategory.First(a => a.CategoryId == Categoryid);
                        vssp_db.Tbl_MST_DefectCategory.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(Categoryname, JsonRequestBehavior.AllowGet);
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
        public ActionResult DefectList()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "DefectList");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;


                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult DefectListJson(string id, string categoryid, string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            var DefectList = (from a in vssp_db.Tbl_MST_DefectList
                              join b in vssp_db.Tbl_MST_DefectCategory on a.DefectCategory equals b.CategoryId into defectcategory
                              from b in defectcategory.DefaultIfEmpty()
                              join c in vssp_db.Tbl_MST_MeasurementsCategories on a.PartCategory equals c.CategoryId into partcategory
                              from c in partcategory.DefaultIfEmpty()
                              where a.Actived == isActive && (a.DefectId.Contains(searchFilter) || a.DefectName.Contains(searchFilter))
                              orderby a.DefectId
                              select new { a.DefectId, a.DefectName, a.DefectCategory, b.CategoryName, a.PartCategory, PartCategoryName = c.CategoryName, a.Actived, a.UserId, a.EditDate }).ToList();

            if(_SystemService.Vf(categoryid) != "")
            {
                DefectList = DefectList.Where(a => a.PartCategory == categoryid).ToList();
            }

            return Json(DefectList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudDefectList(string defectid, string defectname, string defectcategory, string partcategory, bool actived, string uid, string formAction)
        {

            try
            {

                switch (formAction.ToLower())
                {
                    case "create":

                        Tbl_MST_DefectList ListDefectList = new Tbl_MST_DefectList();
                        ListDefectList.DefectId = defectid;
                        ListDefectList.DefectName = defectname;
                        ListDefectList.DefectCategory = defectcategory;
                        ListDefectList.PartCategory = partcategory;
                        ListDefectList.Actived = actived;
                        ListDefectList.UserId = uid;
                        ListDefectList.EditDate = DateTime.Now;

                        vssp_db.Tbl_MST_DefectList.Add(ListDefectList);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_MST_DefectList.First(a => a.DefectId == defectid);

                        ListUpdate.DefectName = defectname;
                        ListUpdate.DefectCategory = defectcategory;
                        ListUpdate.PartCategory = partcategory;
                        ListUpdate.Actived = actived;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        break;

                    case "delete":

                        var ListDelete = vssp_db.Tbl_MST_DefectList.First(a => a.DefectId == defectid);
                        vssp_db.Tbl_MST_DefectList.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(defectname, JsonRequestBehavior.AllowGet);
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
        public ActionResult InspectionGate()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "InspectionGate");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;


                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult InspectionGateJson(string id, string searchFilter, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);

            var inspectionGate = (from a in vssp_db.Tbl_MST_InspectionGate
                                  where a.Actived==isActive && (a.GateId.Contains(searchFilter) || a.GateName.Contains(searchFilter) || a.InspectionType.Contains(searchFilter))
                                  orderby a.InspectionType, a.GateId
                                  select new { a.GateId, a.GateName, a.InspectionType, a.Actived, a.UserId, a.EditDate }).ToList();

            return Json(inspectionGate, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudInspectionGate(string gateid, string gatename, string inspectiontype, bool actived, string uid, string formAction)
        {

            try
            {

                switch (formAction.ToLower())
                {
                    case "create":

                        Tbl_MST_InspectionGate ListInspectionGate = new Tbl_MST_InspectionGate();
                        ListInspectionGate.GateId = gateid;
                        ListInspectionGate.GateName = gatename;
                        ListInspectionGate.InspectionType = inspectiontype;
                        ListInspectionGate.Actived = actived;
                        ListInspectionGate.UserId = uid;
                        ListInspectionGate.EditDate = DateTime.Now;

                        vssp_db.Tbl_MST_InspectionGate.Add(ListInspectionGate);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_MST_InspectionGate.First(a => a.GateId == gateid);

                        ListUpdate.GateName = gatename;
                        ListUpdate.InspectionType = inspectiontype;
                        ListUpdate.Actived = actived;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        break;

                    case "delete":

                        var ListDelete = vssp_db.Tbl_MST_InspectionGate.First(a => a.GateId == gateid);
                        vssp_db.Tbl_MST_InspectionGate.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(gatename, JsonRequestBehavior.AllowGet);
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
        public ActionResult PartIdentificationSupplier()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "PartIdentificationSupplier");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;


                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult PartIdentificationSupplierJson(string id, string searchFilter, string scanQrCodes, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            scanQrCodes = _SystemService.Vf(scanQrCodes);

            if (scanQrCodes == "")
            {
                var PartIdentificationSupplier = (from a in vssp_db.Tbl_MST_PartIdentificationSupplier
                                          join b in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                          from b in part.DefaultIfEmpty()
                                          where (b.Actived == true ? a.Actived : b.Actived) == isActive && (a.DocumentNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || b.UniqueNumber.Contains(searchFilter) || b.PartName.Contains(searchFilter))
                                          orderby a.DocumentNumber
                                          select new
                                          {
                                              a.SupplierId,
                                              a.PartNumber,
                                              b.PartName,
                                              b.UniqueNumber,
                                              a.SupplierUniqueNumber,
                                              b.PartModel,
                                              b.UnitQty,
                                              a.PartCategory,
                                              a.DocumentNumber,
                                              a.ReleaseDate,
                                              a.Revision,
                                              a.ECINumber,
                                              a.GateId,
                                              a.CycleTime,
                                              a.PI_Images,
                                              a.Drawing_Images,
                                              Actived = (b.Actived == true ? a.Actived : b.Actived),
                                              PartActived = b.Actived,
                                              a.UserId,
                                              a.EditDate
                                          }).Distinct().ToList();

                return Json(PartIdentificationSupplier, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string[] scanresult = scanQrCodes.Split(';');
                string supid = "";
                string partno = "";

                searchFilter = scanresult[0].ToString();

                var ekanban = (from a in vssp_db.Tbl_TRS_ReceivingOrderDetailKanban
                               join b in vssp_db.Tbl_TRS_SupplierOrderKanban on a.KanbanKey equals b.KanbanKey into kanban
                               from b in kanban.DefaultIfEmpty()
                               where a.KanbanKey == searchFilter
                               select new { a.SupplierId, b.PartNumber }).FirstOrDefault();

                if (ekanban != null)
                {
                    supid = ekanban.SupplierId;
                    partno = ekanban.PartNumber;
                }
                else
                {
                    if (scanresult.Count() > 1)
                    {
                        supid = scanresult[0];
                        partno = scanresult[1];
                    }
                    else
                    {
                        supid = scanQrCodes;
                        partno = scanQrCodes;
                    }
                }

                var PartIdentificationSupplier = (from a in vssp_db.Tbl_MST_PartIdentificationSupplier
                                                  join b in vssp_db.Tbl_MST_PartRawMaterials on new { a.SupplierId, a.PartNumber } equals new { b.SupplierId, b.PartNumber } into part
                                                  from b in part.DefaultIfEmpty()
                                                  where a.Actived == isActive && (a.SupplierId.Contains(supid) && a.PartNumber.Contains(partno)) || (a.SupplierUniqueNumber.Contains(searchFilter) || b.UniqueNumber.Contains(searchFilter))
                                                  orderby a.DocumentNumber
                                                  select new { a.SupplierId, a.PartNumber, b.PartName, b.UniqueNumber, a.SupplierUniqueNumber, b.PartModel, b.UnitQty, a.PartCategory, a.DocumentNumber, a.ReleaseDate, a.Revision, a.ECINumber, a.GateId, a.PI_Images, a.Drawing_Images, a.Actived, a.UserId, a.EditDate }).Distinct().ToList();

                return Json(PartIdentificationSupplier, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult crudPartIdentificationSupplier(string jsonData)
        {

            try
            {

                string uid = Session["UserID"].ToString();
                HttpPostedFileBase file = Request.Files["PartIdentificationSupplierImages"];
                HttpPostedFileBase drawing = Request.Files["PartIdentificationSupplierDrawing"];

                postPartIdentificationSupplier postPartIdentificationSupplier = JsonConvert.DeserializeObject<postPartIdentificationSupplier>(jsonData);
                Tbl_MST_PartIdentificationSupplier PartIdentificationSupplier = postPartIdentificationSupplier.PartIdentificationSupplier;

                string formAction = postPartIdentificationSupplier.formAction;
                if (file != null)
                {
                    string path = "";
                    string filename = "";
                    string extention = "";
                    string savefile = "";
                    string imagelocation = "";

                    extention = Path.GetExtension(file.FileName);
                    filename = PartIdentificationSupplier.PartNumber.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("/", "_").Replace(".", "") + extention;
                    path = Server.MapPath("~/_VSSPAssets/Images/apps/PartInspectionSupplier/" + PartIdentificationSupplier.SupplierId);
                    savefile = Path.Combine(path, filename);
                    imagelocation = "../_VSSPAssets/Images/apps/PartInspectionSupplier/" + PartIdentificationSupplier.SupplierId + "/" + filename;

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (System.IO.File.Exists(savefile))
                    {
                        System.IO.File.Delete(savefile);
                    }
                    if (System.IO.File.Exists(PartIdentificationSupplier.PI_Images))
                    {
                        System.IO.File.Delete(PartIdentificationSupplier.PI_Images);
                    }

                    file.SaveAs(savefile);
                    PartIdentificationSupplier.PI_Images = imagelocation;
                }
                if (drawing != null)
                {
                    string path = "";
                    string filename = "";
                    string extention = "";
                    string savefile = "";
                    string imagelocation = "";

                    extention = Path.GetExtension(drawing.FileName);
                    filename = PartIdentificationSupplier.PartNumber.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("/", "_").Replace(".", "") + extention;
                    path = Server.MapPath("~/_VSSPAssets/Images/apps/PartSpecificationSupplier/" + PartIdentificationSupplier.SupplierId);
                    savefile = Path.Combine(path, filename);
                    imagelocation = "../_VSSPAssets/Images/apps/PartSpecificationSupplier/" + PartIdentificationSupplier.SupplierId + "/" + filename;

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (System.IO.File.Exists(savefile))
                    {
                        System.IO.File.Delete(savefile);
                    }
                    if (System.IO.File.Exists(PartIdentificationSupplier.Drawing_Images))
                    {
                        System.IO.File.Delete(PartIdentificationSupplier.Drawing_Images);
                    }

                    drawing.SaveAs(savefile);
                    PartIdentificationSupplier.Drawing_Images = imagelocation;
                }

                switch (formAction.ToLower())
                {
                    case "create":

                        Tbl_MST_PartIdentificationSupplier ListPartIdentificationSupplier = new Tbl_MST_PartIdentificationSupplier();
                        ListPartIdentificationSupplier.SupplierId = PartIdentificationSupplier.SupplierId;
                        ListPartIdentificationSupplier.PartNumber = PartIdentificationSupplier.PartNumber;
                        ListPartIdentificationSupplier.SupplierUniqueNumber = PartIdentificationSupplier.SupplierUniqueNumber;
                        ListPartIdentificationSupplier.PartCategory = PartIdentificationSupplier.PartCategory;
                        ListPartIdentificationSupplier.DocumentNumber = PartIdentificationSupplier.DocumentNumber;
                        ListPartIdentificationSupplier.ReleaseDate = PartIdentificationSupplier.ReleaseDate;
                        ListPartIdentificationSupplier.Revision = PartIdentificationSupplier.Revision;
                        ListPartIdentificationSupplier.ECINumber = PartIdentificationSupplier.ECINumber;
                        ListPartIdentificationSupplier.GateId = PartIdentificationSupplier.GateId;
                        ListPartIdentificationSupplier.CycleTime = PartIdentificationSupplier.CycleTime;
                        ListPartIdentificationSupplier.PI_Images = PartIdentificationSupplier.PI_Images;
                        ListPartIdentificationSupplier.Drawing_Images = PartIdentificationSupplier.Drawing_Images;
                        ListPartIdentificationSupplier.Actived = PartIdentificationSupplier.Actived;
                        ListPartIdentificationSupplier.UserId = uid;
                        ListPartIdentificationSupplier.EditDate = DateTime.Now;

                        vssp_db.Tbl_MST_PartIdentificationSupplier.Add(ListPartIdentificationSupplier);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_MST_PartIdentificationSupplier.First(a => a.SupplierId == PartIdentificationSupplier.SupplierId && a.PartNumber == PartIdentificationSupplier.PartNumber);

                        ListUpdate.SupplierUniqueNumber = PartIdentificationSupplier.SupplierUniqueNumber;
                        ListUpdate.PartCategory = PartIdentificationSupplier.PartCategory;
                        ListUpdate.DocumentNumber = PartIdentificationSupplier.DocumentNumber;
                        ListUpdate.ReleaseDate = PartIdentificationSupplier.ReleaseDate;
                        ListUpdate.Revision = PartIdentificationSupplier.Revision;
                        ListUpdate.ECINumber = PartIdentificationSupplier.ECINumber;
                        ListUpdate.GateId = PartIdentificationSupplier.GateId;
                        ListUpdate.CycleTime = PartIdentificationSupplier.CycleTime;
                        ListUpdate.PI_Images = PartIdentificationSupplier.PI_Images != null ? PartIdentificationSupplier.PI_Images : ListUpdate.PI_Images;
                        ListUpdate.Drawing_Images = PartIdentificationSupplier.Drawing_Images != null ? PartIdentificationSupplier.Drawing_Images : ListUpdate.Drawing_Images;
                        ListUpdate.Actived = PartIdentificationSupplier.Actived;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        break;

                    case "delete":

                        var ListDelete = vssp_db.Tbl_MST_PartIdentificationSupplier.First(a => a.SupplierId == PartIdentificationSupplier.SupplierId && a.PartNumber == PartIdentificationSupplier.PartNumber);
                        vssp_db.Tbl_MST_PartIdentificationSupplier.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(PartIdentificationSupplier.DocumentNumber, JsonRequestBehavior.AllowGet);
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

        public ActionResult PartIdentification()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "PartIdentification");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;


                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult PartIdentificationJson(string id, string searchFilter, string scanProdKanban, string scanQrCodes, string customerId, bool customerKanban = false, bool isActive = true)
        {
            searchFilter = _SystemService.Vf(searchFilter);
            scanProdKanban = _SystemService.Vf(scanProdKanban);
            scanQrCodes = _SystemService.Vf(scanQrCodes);

            if (scanProdKanban == "" && scanQrCodes == "") 
            {
                var PartIdentification = (from a in vssp_db.Tbl_MST_PartIdentification
                                          join b in vssp_db.Tbl_MST_PartFinishGoods on new { a.CustomerId, a.PartNumber } equals new { b.CustomerId, b.PartNumber} into part
                                          from b in part.DefaultIfEmpty()
                                          where (b.Actived == true ? a.Actived : b.Actived) == isActive && (a.DocumentNumber.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || b.UniqueNumber.Contains(searchFilter) || b.PartName.Contains(searchFilter))
                                          orderby a.DocumentNumber
                                          select new { 
                                              a.CustomerId, a.PartNumber, b.PartName, b.UniqueNumber, a.CustomerUniqueNumber, b.CustomerUnitModel, b.UnitQty, a.PartCategory, a.PartIdentity, 
                                              a.DocumentNumber, a.ReleaseDate, a.Revision, a.ECINumber, a.CycleTime, a.PI_Images, a.Drawing_Images,
                                              Actived = (b.Actived == true ? a.Actived : b.Actived),
                                              PartActived = b.Actived, 
                                              a.UserId, a.EditDate }).Distinct().ToList();

                return Json(PartIdentification, JsonRequestBehavior.AllowGet);

            } else
            {
                if (scanProdKanban == "" && scanQrCodes != "")
                {
                    string[] scanresult = scanQrCodes.Split(';');
                    string custid = "";
                    string partno = "";

                    searchFilter = scanresult[0].ToString();

                    var prodkanban = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.KanbanKey == searchFilter).FirstOrDefault();
                    if (prodkanban != null)
                    {
                        custid = prodkanban.CustomerId;
                        partno = prodkanban.PartNumber;
                    }
                    else
                    {
                        if (scanresult.Count() > 1)
                        {
                            custid = scanresult[0];
                            partno = scanresult[1];
                        }
                        else
                        {
                            custid = scanQrCodes;
                            partno = scanQrCodes;
                        }
                    }

                    var PartIdentification = (from a in vssp_db.Tbl_MST_PartIdentification
                                              join b in vssp_db.Tbl_MST_PartFinishGoods on a.PartNumber equals b.PartNumber into part
                                              from b in part.DefaultIfEmpty()
                                              where a.Actived == isActive && (a.CustomerId.Contains(custid) && a.PartNumber.Contains(partno)) || (a.CustomerUniqueNumber.Contains(searchFilter) || b.UniqueNumber.Contains(searchFilter))
                                              orderby a.DocumentNumber
                                              select new { a.CustomerId, a.PartNumber, b.PartName, b.UniqueNumber, a.CustomerUniqueNumber, b.CustomerUnitModel, b.UnitQty, a.PartCategory, a.PartIdentity, a.DocumentNumber, a.ReleaseDate, a.Revision, a.ECINumber, a.CycleTime, a.PI_Images, a.Drawing_Images, a.Actived, a.UserId, a.EditDate }).Distinct().ToList();

                    return Json(PartIdentification, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string[] scanresult = scanQrCodes.Split(';');
                    string custid = "";
                    string partno = "";

                    if (scanresult.Count() > 1)
                    {
                        custid = scanresult[0];
                        partno = scanresult[1];
                    }
                    else
                    {
                        custid = scanQrCodes;
                        partno = scanQrCodes;
                    }

                    if (customerKanban)
                    {
                        var kanbanResult = kanbanCustomerResult(customerId, scanProdKanban);
                        if (custid == kanbanResult.CustomerId && partno == kanbanResult.PartNumber)
                        {
                            custid = kanbanResult.CustomerId;
                            partno = kanbanResult.PartNumber;
                        }
                        else
                        {
                            custid = "*";
                            partno = "*";
                        }
                    }
                    else
                    {
                        string[] kanbankey = scanProdKanban.Split(';');
                        scanProdKanban = _SystemService.Vf(kanbankey[0].ToString());

                        var prodkanban = vssp_db.Tbl_MST_KanbanProduction.Where(a => a.KanbanKey == scanProdKanban).FirstOrDefault();
                        if (prodkanban != null)
                        {
                            if (custid == prodkanban.CustomerId && partno == prodkanban.PartNumber)
                            {
                                custid = prodkanban.CustomerId;
                                partno = prodkanban.PartNumber;
                            }
                            else
                            {
                                custid = "*";
                                partno = "*";
                            }
                        }
                        else
                        {
                            custid = "*";
                            partno = "*";
                        }


                    }
                    var PartIdentification = (from a in vssp_db.Tbl_MST_PartIdentification
                                              join b in vssp_db.Tbl_MST_PartFinishGoods on a.PartNumber equals b.PartNumber into part
                                              from b in part.DefaultIfEmpty()
                                              where a.Actived == isActive && a.CustomerId == custid && a.PartNumber == partno
                                              orderby a.DocumentNumber
                                              select new { a.CustomerId, a.PartNumber, b.PartName, b.UniqueNumber, a.CustomerUniqueNumber, b.CustomerUnitModel, b.UnitQty, a.PartCategory, a.PartIdentity, a.DocumentNumber, a.ReleaseDate, a.Revision, a.ECINumber, a.CycleTime, a.PI_Images, a.Drawing_Images, a.Actived, a.UserId, a.EditDate }).Distinct().ToList();

                    return Json(PartIdentification, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult crudPartIdentification(string jsonData)
        {

            try
            {

                string uid = Session["UserID"].ToString();
                HttpPostedFileBase file = Request.Files["PartIdentificationImages"];
                HttpPostedFileBase drawing = Request.Files["PartIdentificationDrawing"];

                postPartIdentification postPartIdentification = JsonConvert.DeserializeObject<postPartIdentification>(jsonData);
                Tbl_MST_PartIdentification partIdentification = postPartIdentification.PartIdentification;

                string formAction = postPartIdentification.formAction;
                if (file != null)
                {
                    string path = "";
                    string filename = "";
                    string extention = "";
                    string savefile = "";
                    string imagelocation = "";

                    extention = Path.GetExtension(file.FileName);
                    filename = partIdentification.PartNumber.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("/", "_").Replace(".", "") + extention;
                    path = Server.MapPath("~/_VSSPAssets/Images/apps/PartInspection/" + partIdentification.CustomerId);
                    savefile = Path.Combine(path, filename);
                    imagelocation = "../_VSSPAssets/Images/apps/PartInspection/" + partIdentification.CustomerId + "/" + filename;

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (System.IO.File.Exists(savefile))
                    {
                        System.IO.File.Delete(savefile);
                    }
                    if (System.IO.File.Exists(partIdentification.PI_Images))
                    {
                        System.IO.File.Delete(partIdentification.PI_Images);
                    }

                    file.SaveAs(savefile);
                    partIdentification.PI_Images = imagelocation;
                }
                if (drawing != null)
                {
                    string path = "";
                    string filename = "";
                    string extention = "";
                    string savefile = "";
                    string imagelocation = "";

                    extention = Path.GetExtension(drawing.FileName);
                    filename = partIdentification.PartNumber.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("/", "_").Replace(".", "") + extention;
                    path = Server.MapPath("~/_VSSPAssets/Images/apps/PartSpecification/" + partIdentification.CustomerId);
                    savefile = Path.Combine(path, filename);
                    imagelocation = "../_VSSPAssets/Images/apps/PartSpecification/" + partIdentification.CustomerId + "/" + filename;

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (System.IO.File.Exists(savefile))
                    {
                        System.IO.File.Delete(savefile);
                    }
                    if (System.IO.File.Exists(partIdentification.Drawing_Images))
                    {
                        System.IO.File.Delete(partIdentification.Drawing_Images);
                    }

                    drawing.SaveAs(savefile);
                    partIdentification.Drawing_Images = imagelocation;
                }

                switch (formAction.ToLower())
                {
                    case "create":

                        Tbl_MST_PartIdentification ListPartIdentification = new Tbl_MST_PartIdentification();
                        ListPartIdentification.CustomerId = partIdentification.CustomerId;
                        ListPartIdentification.PartNumber = partIdentification.PartNumber;
                        ListPartIdentification.CustomerUniqueNumber = partIdentification.CustomerUniqueNumber;
                        ListPartIdentification.PartCategory = partIdentification.PartCategory;
                        ListPartIdentification.PartIdentity = partIdentification.PartIdentity;
                        ListPartIdentification.DocumentNumber = partIdentification.DocumentNumber;
                        ListPartIdentification.ReleaseDate = partIdentification.ReleaseDate;
                        ListPartIdentification.Revision = partIdentification.Revision;
                        ListPartIdentification.ECINumber = partIdentification.ECINumber;
                        ListPartIdentification.CycleTime = partIdentification.CycleTime;
                        ListPartIdentification.PI_Images = partIdentification.PI_Images;
                        ListPartIdentification.Drawing_Images = partIdentification.Drawing_Images;
                        ListPartIdentification.Actived = partIdentification.Actived;
                        ListPartIdentification.UserId = uid;
                        ListPartIdentification.EditDate = DateTime.Now;

                        vssp_db.Tbl_MST_PartIdentification.Add(ListPartIdentification);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_MST_PartIdentification.First(a => a.CustomerId == partIdentification.CustomerId && a.PartNumber == partIdentification.PartNumber);

                        ListUpdate.CustomerUniqueNumber = partIdentification.CustomerUniqueNumber;
                        ListUpdate.PartCategory = partIdentification.PartCategory;
                        ListUpdate.PartIdentity = partIdentification.PartIdentity;
                        ListUpdate.DocumentNumber = partIdentification.DocumentNumber;
                        ListUpdate.ReleaseDate = partIdentification.ReleaseDate;
                        ListUpdate.Revision = partIdentification.Revision;
                        ListUpdate.ECINumber = partIdentification.ECINumber;
                        ListUpdate.CycleTime = partIdentification.CycleTime;
                        ListUpdate.PI_Images = partIdentification.PI_Images != null ? partIdentification.PI_Images : ListUpdate.PI_Images;
                        ListUpdate.Drawing_Images = partIdentification.Drawing_Images != null ? partIdentification.Drawing_Images : ListUpdate.Drawing_Images;
                        ListUpdate.Actived = partIdentification.Actived;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        break;

                    case "delete":

                        var ListDelete = vssp_db.Tbl_MST_PartIdentification.First(a => a.CustomerId == partIdentification.CustomerId && a.PartNumber == partIdentification.PartNumber);
                        vssp_db.Tbl_MST_PartIdentification.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(partIdentification.DocumentNumber, JsonRequestBehavior.AllowGet);
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

        public ActionResult PartListJson()
        {
            var partlist = (from a in vssp_db.Tbl_MST_PartFinishGoods
                            orderby a.UniqueNumber, a.PartNumber
                            select new { a.PartNumber, a.PartName, a.UniqueNumber, a.CustomerUnitModel }).Distinct().ToList();
            return Json(partlist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IncomingInspection()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string compid = Session["CompID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "IncomingInspection");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Incoming Quality Control";
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.CompId = compid;

                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult IncomingInspectionJson(
            string searchFilter,
            string inspectionnumber,
            string inspectiongate,
            Nullable<DateTime> startdate = null,
            Nullable<DateTime> enddate = null
            )
        {

            inspectionnumber = _SystemService.Vf(inspectionnumber);
            searchFilter = _SystemService.Vf(searchFilter);

            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;

                var IncomingInspection = (from a in vssp_db.Vw_QC_InspectionIncoming
                                          where a.InspectionDate >= startdate && a.InspectionDate <= enddate && a.InspectionNumber.Contains(inspectionnumber) && (a.InspectionGate.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                                          orderby a.InspectionGate, a.InspectionDate descending, a.StartTime descending
                                          select a).ToList();

                if (_SystemService.Vf(inspectiongate) != "")
                {
                    IncomingInspection = IncomingInspection.Where(a => a.InspectionGate == inspectiongate).ToList();
                }

                //return Json(IncomingInspection, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(IncomingInspection, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                var IncomingInspection = (from a in vssp_db.Vw_QC_InspectionIncoming
                                          where a.InspectionNumber.Contains(inspectionnumber) && (a.InspectionNumber.Contains(searchFilter) || a.InspectionGate.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                                          orderby a.InspectionGate, a.InspectionDate descending, a.StartTime descending
                                          select a).ToList();

                if (_SystemService.Vf(inspectiongate) != "")
                {
                    IncomingInspection = IncomingInspection.Where(a => a.InspectionGate == inspectiongate).ToList();
                }

                //return Json(IncomingInspection, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(IncomingInspection, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
        }
        public ActionResult OutgoingInspection(string category = "")
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string uin = Session["UserName"].ToString();
                string compid = Session["CompID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "OutgoingInspection?category=" + category);

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = "Outgoing Quality Control";
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.UserId = uid;
                    ViewBag.UserName = uin;
                    ViewBag.CompId = compid;

                    if(category == "threepointcheck")
                    {
                        ViewBag.Title = "Outgoing Quality Control [3 Point Check]";
                        ViewBag.Category = category;
                    }

                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult OutgoingInspectionJson(
            string searchFilter,
            string inspectionnumber, 
            string inspectiongate, 
            Nullable<DateTime> startdate = null,
            Nullable<DateTime> enddate = null 
            )
        {

            inspectionnumber = _SystemService.Vf(inspectionnumber);
            searchFilter = _SystemService.Vf(searchFilter);

            if (startdate != null)
            {
                if (enddate == null) enddate = startdate;

                var OutgoingInspection = (from a in vssp_db.Vw_QC_Inspection
                                          where a.InspectionDate >= startdate && a.InspectionDate<=enddate && a.InspectionNumber.Contains(inspectionnumber) && (a.InspectionGate.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                                          orderby a.InspectionGate, a.InspectionDate descending, a.StartTime descending
                                          select a).ToList();

                if(_SystemService.Vf(inspectiongate) != "")
                {
                    OutgoingInspection = OutgoingInspection.Where(a => a.InspectionGate == inspectiongate).ToList();
                }

                //return Json(OutgoingInspection, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(OutgoingInspection, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                var OutgoingInspection = (from a in vssp_db.Vw_QC_Inspection
                                          where a.InspectionNumber.Contains(inspectionnumber) && (a.InspectionNumber.Contains(searchFilter) || a.InspectionGate.Contains(searchFilter) || a.PartNumber.Contains(searchFilter) || a.UniqueNumber.Contains(searchFilter) || a.PartName.Contains(searchFilter))
                                          orderby a.InspectionGate, a.InspectionDate descending, a.StartTime descending
                                          select a).ToList();

                if (_SystemService.Vf(inspectiongate) != "")
                {
                    OutgoingInspection = OutgoingInspection.Where(a => a.InspectionGate == inspectiongate).ToList();
                }

                //return Json(OutgoingInspection, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(OutgoingInspection, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
        }
        public ActionResult InspectionDefectListJson(string inspectionnumber, string inspectiontype = "Outgoing")
        {
            inspectionnumber = _SystemService.Vf(inspectionnumber);

            switch (inspectiontype.ToLower())
            {
                case "outgoing":
                    var InspectionDefectList = (from a in vssp_db.Tbl_QC_InspectionDefects
                                                join b in vssp_db.Tbl_MST_DefectList on a.DefectId equals b.DefectId into defect
                                                from b in defect.DefaultIfEmpty()
                                                where a.InspectionNumber.Contains(inspectionnumber)
                                                select new { a.InspectionNumber, a.DefectNumber, a.DefectId, b.DefectName, a.DefectQty, a.Repair, a.Scrap, a.UserId, a.EditDate }).ToList();

                    var jsonResult = Json(InspectionDefectList, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;

                case "incoming":
                    var InspectionIncomingDefectList = (from a in vssp_db.Tbl_QC_InspectionIncomingDefects
                                                join b in vssp_db.Tbl_MST_DefectList on a.DefectId equals b.DefectId into defect
                                                from b in defect.DefaultIfEmpty()
                                                where a.InspectionNumber.Contains(inspectionnumber)
                                                select new { a.InspectionNumber, a.DefectNumber, a.DefectId, b.DefectName, a.DefectQty, a.PartReturn, a.Repair, a.Scrap, a.UserId, a.EditDate }).ToList();
                    //return Json(InspectionDefectList, JsonRequestBehavior.AllowGet);
                    var jsonResultIncoming = Json(InspectionIncomingDefectList, JsonRequestBehavior.AllowGet);
                    jsonResultIncoming.MaxJsonLength = int.MaxValue;
                    return jsonResultIncoming;

                default:
                    return Json("No Defect Data Found.", JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GetDefectNumber()
        {
            Guid guid = Guid.NewGuid();
            return Json(guid.ToString(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudIncomingInspection(string jsonData)
        {

            try
            {
                PurchaseController purchaseController = new PurchaseController();
                postIncomingInspection postIncomingInspection = JsonConvert.DeserializeObject<postIncomingInspection>(jsonData);
                Tbl_QC_InspectionIncoming inspection = postIncomingInspection.InspectionIncoming;
                List<Tbl_QC_InspectionIncomingDefects> inspectionDefects = postIncomingInspection.InspectionIncomingDefects;
                string formAction = postIncomingInspection.formAction.ToLower();
                double lastunitqty = 0;

                switch (formAction)
                {
                    case "create":

                        string years = _SystemService.Vd(DateTime.Now.ToString(), "yyyy");
                        string month = _SystemService.Vd(DateTime.Now.ToString(), "MM");

                        //var inspectionNumber_Result = vssp_db.SP_GET_InspectionNumber(month, years, postIncomingInspection.compid).FirstOrDefault();
                        //inspection.InspectionNumber = inspectionNumber_Result.InspectionNumber;

                        Guid guid = Guid.NewGuid();
                        inspection.InspectionNumber = guid.ToString();

                        Tbl_QC_InspectionIncoming QC_Inspection = new Tbl_QC_InspectionIncoming();
                        QC_Inspection.InspectionNumber = inspection.InspectionNumber;
                        QC_Inspection.InspectionGate = inspection.InspectionGate;
                        QC_Inspection.InspectionDate = inspection.InspectionDate;
                        QC_Inspection.StartTime = inspection.StartTime;
                        QC_Inspection.FinishTime = inspection.FinishTime;
                        QC_Inspection.SupplierId = inspection.SupplierId;
                        QC_Inspection.PartNumber = inspection.PartNumber;
                        QC_Inspection.TotalCheck = inspection.TotalCheck;
                        QC_Inspection.TotalDefectUnit = inspection.TotalDefectUnit;
                        QC_Inspection.TotalDefectQty = inspection.TotalDefectQty;
                        QC_Inspection.Replaced = inspection.Replaced;
                        QC_Inspection.Remains = inspection.Remains;
                        QC_Inspection.UserId = inspection.UserId;
                        QC_Inspection.UserName = inspection.UserName;
                        QC_Inspection.EditDate = DateTime.Now;

                        vssp_db.Tbl_QC_InspectionIncoming.Add(QC_Inspection);

                        /* crud Defect */
                        crudDefectIncoming(inspectionDefects, inspection.InspectionNumber, inspection.UserId);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_QC_InspectionIncoming.First(a => a.InspectionNumber == inspection.InspectionNumber);
                        lastunitqty = _SystemService.Vn(ListUpdate.Replaced.ToString());

                        ListUpdate.InspectionGate = inspection.InspectionGate;
                        ListUpdate.InspectionDate = inspection.InspectionDate;
                        ListUpdate.StartTime = inspection.StartTime;
                        ListUpdate.FinishTime = inspection.FinishTime;
                        ListUpdate.SupplierId = inspection.SupplierId;
                        ListUpdate.PartNumber = inspection.PartNumber;
                        ListUpdate.TotalCheck = inspection.TotalCheck;
                        ListUpdate.TotalDefectUnit = inspection.TotalDefectUnit;
                        ListUpdate.TotalDefectQty = inspection.TotalDefectQty;
                        ListUpdate.Replaced = inspection.Replaced;
                        ListUpdate.Remains = inspection.Remains;
                        ListUpdate.UserId = inspection.UserId;
                        ListUpdate.UserName = inspection.UserName;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Defect */
                        crudDefectIncoming(inspectionDefects, inspection.InspectionNumber, inspection.UserId);

                        break;

                    case "delete":

                        /* remove existing Defect */
                        var deleteDefect = from a in vssp_db.Tbl_QC_InspectionIncomingDefects
                                           where a.InspectionNumber == inspection.InspectionNumber
                                           select a;

                        deleteDefect.ForEach(Defect =>
                        {
                            vssp_db.Tbl_QC_InspectionIncomingDefects.Remove(Defect);
                        });

                        /* remove existing Inspection */
                        var ListDelete = vssp_db.Tbl_QC_InspectionIncoming.First(a => a.InspectionNumber == inspection.InspectionNumber);
                        lastunitqty = _SystemService.Vn(ListDelete.Replaced.ToString());

                        vssp_db.Tbl_QC_InspectionIncoming.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    /* crud stock raw return */
                    purchaseController.crudStockRawReturn(inspection.SupplierId, inspection.PartNumber, _SystemService.Vn(inspection.Replaced.ToString()), lastunitqty, formAction, "", false, true);

                    return Json(inspection, JsonRequestBehavior.AllowGet);
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
        public void crudDefectIncoming(List<Tbl_QC_InspectionIncomingDefects> inspectionDefects, string inspectionNumber, string uid)
        {

            /* clean old defect files */
            var existdefectlist = (from a in vssp_db.Tbl_QC_InspectionIncomingDefects
                                   where a.InspectionNumber == inspectionNumber
                                   select a).ToList();

            existdefectlist.ForEach(Defect =>
            {
                vssp_db.Tbl_QC_InspectionIncomingDefects.Remove(Defect);
            });

            foreach (var defects in inspectionDefects)
            {
                Tbl_QC_InspectionIncomingDefects _InspectionDefects = new Tbl_QC_InspectionIncomingDefects();
                _InspectionDefects.InspectionNumber = inspectionNumber;
                _InspectionDefects.DefectNumber = defects.DefectNumber;
                _InspectionDefects.DefectId = defects.DefectId;
                _InspectionDefects.DefectQty = defects.DefectQty;
                _InspectionDefects.PartReturn = defects.PartReturn;
                _InspectionDefects.Repair = defects.Repair;
                _InspectionDefects.Scrap = defects.Scrap;
                _InspectionDefects.UserId = uid;
                _InspectionDefects.EditDate = DateTime.Now;

                vssp_db.Tbl_QC_InspectionIncomingDefects.Add(_InspectionDefects);
            }
        }
        public KanbanDataModel kanbanCustomerResult(string CustomerId, string KanbanData)
        {

            KanbanDataModel kanbanDataModel = new KanbanDataModel();
            var setting = vssp_db.Tbl_MST_KanbanSetting.Where(a => a.CustomerId == CustomerId).FirstOrDefault();

            if (setting != null)
            {
                string sparator = setting.DataSparator;
                string[] kanbandata = KanbanData.Split(new[] { sparator }, StringSplitOptions.None);

                var jsondata = new Dictionary<string, string> { };
                int seq = 0;
                foreach (var kanban in kanbandata)
                {
                    seq += 1;
                    var kanbanseq = vssp_db.Tbl_MST_KanbanSettingSequence.Where(a => a.Active == true && a.CustomerId == CustomerId && a.SequenceNumber == seq).FirstOrDefault();

                    if (kanbanseq != null)
                    {
                        jsondata.Add(kanbanseq.FieldName, kanban);
                    }

                }
                var jsSerializer = new JavaScriptSerializer();
                var serialized = jsSerializer.Serialize(jsondata);

                kanbanDataModel = JsonConvert.DeserializeObject<KanbanDataModel>(serialized);
                kanbanDataModel.CustomerId = (kanbanDataModel.CustomerId == null ? CustomerId : kanbanDataModel.CustomerId);

            }
            return kanbanDataModel;
        }
        public ActionResult kanbanCustomerReader(string CustomerId, string KanbanData)
        {
            var kanbanDataModel = kanbanCustomerResult(CustomerId, KanbanData);

            if (kanbanDataModel != null)
            {
                

                if (kanbanDataModel.CustomerId == null) kanbanDataModel.CustomerId = CustomerId;

                var customerValidation = vssp_db.Tbl_MST_Customer.Where(a => a.CustomerId == kanbanDataModel.CustomerId || a.CustomerCode == kanbanDataModel.CustomerId).FirstOrDefault();
                if (customerValidation != null)
                {
                    var partValidation = vssp_db.Tbl_MST_PartFinishGoods.Where(a => (a.CustomerId == customerValidation.CustomerId && (a.PartNumber == kanbanDataModel.PartNumber || a.PartNumberCustomer == kanbanDataModel.PartNumber))).FirstOrDefault();
                    if (partValidation != null)
                    {

                        //crudDeliveryOrderKanbanTemp(DONumber, CustomerId, kanbanDataModel.PartNumber, kanbanDataModel.KanbanNumber, kanbanDataModel.RefNumber, kanbanDataModel.OrderQty, KanbanData, "Create");
                        kanbanDataModel.ErrMessages = "<b>" + kanbanDataModel.PartNumber + " :<br /> has been scan successfully.";

                    }
                    else
                    {
                        kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong customer part number!</b><br /> Please check your kanban card.";
                    }
                }
                else
                {
                    kanbanDataModel.ErrMessages = "<b>Error :<br />  Wrong customer kanban!</b><br /> Please check your kanban card.";
                }
                return Json(kanbanDataModel, JsonRequestBehavior.AllowGet);

            }
            else
            {
                kanbanDataModel = new KanbanDataModel();
                kanbanDataModel.ErrMessages = "<b>Error :<br />  Customer kanban setting not found!</b><br /> Please check customer kanban setting.";
                return Json(kanbanDataModel, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult crudOutgoingInspection(string jsonData)
        {

            try
            {
                postOutgoingInspection postOutgoingInspection = JsonConvert.DeserializeObject<postOutgoingInspection>(jsonData);
                Tbl_QC_Inspection inspection = postOutgoingInspection.Inspection;
                List<Tbl_QC_InspectionDefects> inspectionDefects = postOutgoingInspection.InspectionDefects;
                string formAction = postOutgoingInspection.formAction.ToLower();
                
                switch (formAction)
                {
                    case "create":

                        string years = _SystemService.Vd(DateTime.Now.ToString(), "yyyy");
                        string month = _SystemService.Vd(DateTime.Now.ToString(), "MM");

                        //var inspectionNumber_Result = vssp_db.SP_GET_InspectionNumber(month, years, postOutgoingInspection.compid).FirstOrDefault();
                        //inspection.InspectionNumber = inspectionNumber_Result.InspectionNumber;

                        Guid guid = Guid.NewGuid();
                        inspection.InspectionNumber = guid.ToString();

                        Tbl_QC_Inspection QC_Inspection = new Tbl_QC_Inspection();
                        QC_Inspection.InspectionNumber = inspection.InspectionNumber;
                        QC_Inspection.InspectionGate = inspection.InspectionGate;
                        QC_Inspection.InspectionDate = inspection.InspectionDate;
                        QC_Inspection.StartTime = inspection.StartTime;
                        QC_Inspection.FinishTime = inspection.FinishTime;
                        QC_Inspection.CustomerId = inspection.CustomerId;
                        QC_Inspection.PartNumber = inspection.PartNumber;
                        QC_Inspection.TotalCheck = inspection.TotalCheck;
                        QC_Inspection.TotalDefectUnit = inspection.TotalDefectUnit;
                        QC_Inspection.TotalDefectQty = inspection.TotalDefectQty;
                        QC_Inspection.Replaced = inspection.Replaced;
                        QC_Inspection.Remains = inspection.Remains;
                        QC_Inspection.UserId = inspection.UserId;
                        QC_Inspection.UserName = inspection.UserName;
                        QC_Inspection.EditDate = DateTime.Now;

                        vssp_db.Tbl_QC_Inspection.Add(QC_Inspection);

                        /* crud Defect */
                        crudDefect(inspectionDefects, inspection.InspectionNumber, inspection.UserId);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_QC_Inspection.First(a => a.InspectionNumber == inspection.InspectionNumber);

                        ListUpdate.InspectionGate = inspection.InspectionGate;
                        ListUpdate.InspectionDate = inspection.InspectionDate;
                        ListUpdate.StartTime = inspection.StartTime;
                        ListUpdate.FinishTime = inspection.FinishTime;
                        ListUpdate.CustomerId = inspection.CustomerId;
                        ListUpdate.PartNumber = inspection.PartNumber;
                        ListUpdate.TotalCheck = inspection.TotalCheck;
                        ListUpdate.TotalDefectUnit = inspection.TotalDefectUnit;
                        ListUpdate.TotalDefectQty = inspection.TotalDefectQty;
                        ListUpdate.Replaced = inspection.Replaced;
                        ListUpdate.Remains = inspection.Remains;
                        ListUpdate.UserId = inspection.UserId;
                        ListUpdate.UserName = inspection.UserName;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Defect */
                        crudDefect(inspectionDefects, inspection.InspectionNumber, inspection.UserId);

                        break;

                    case "delete":

                        /* remove existing Defect */
                        var deleteDefect = from a in vssp_db.Tbl_QC_InspectionDefects
                                          where a.InspectionNumber == inspection.InspectionNumber
                                          select a;

                        deleteDefect.ForEach(Defect =>
                        {
                            vssp_db.Tbl_QC_InspectionDefects.Remove(Defect);
                        });

                        /* remove existing Inspection */
                        var ListDelete = vssp_db.Tbl_QC_Inspection.First(a => a.InspectionNumber == inspection.InspectionNumber);

                        vssp_db.Tbl_QC_Inspection.Remove(ListDelete);

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    return Json(inspection, JsonRequestBehavior.AllowGet);
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
        public void crudDefect(List<Tbl_QC_InspectionDefects> inspectionDefects, string inspectionNumber, string uid)
        {
            /* clean old defect files */
            var existdefectlist = (from a in vssp_db.Tbl_QC_InspectionDefects
                              where a.InspectionNumber == inspectionNumber
                              select a).ToList();
            
            existdefectlist.ForEach(Defect =>
            {
                vssp_db.Tbl_QC_InspectionDefects.Remove(Defect);
            });

            foreach (var defects in inspectionDefects)
            {               
                Tbl_QC_InspectionDefects _InspectionDefects = new Tbl_QC_InspectionDefects();
                _InspectionDefects.InspectionNumber = inspectionNumber;
                _InspectionDefects.DefectNumber = defects.DefectNumber;
                _InspectionDefects.DefectId = defects.DefectId;
                _InspectionDefects.DefectQty = defects.DefectQty;
                _InspectionDefects.Repair = defects.Repair;
                _InspectionDefects.Scrap = defects.Scrap;
                _InspectionDefects.UserId = uid;
                _InspectionDefects.EditDate = DateTime.Now;

                vssp_db.Tbl_QC_InspectionDefects.Add(_InspectionDefects);
            }
        }
        public ActionResult ProblemInformation()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                string compid = Session["CompID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "Quality", "ProblemInformation");

                if (acccessPreviliege.CanSee == false)
                {
                    return RedirectToAction("UnauthorizedAccess", "System");
                }
                else
                {
                    ViewBag.Title = _SystemService.Vf(acccessPreviliege.MenuName);
                    ViewBag.IconClass = _SystemService.Vf(acccessPreviliege.IconClass);
                    ViewBag.canSee = acccessPreviliege.CanSee.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canCreate = acccessPreviliege.CanCreate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canUpdate = acccessPreviliege.CanUpdate.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.canDelete = acccessPreviliege.CanDelete.ToString().Replace("True", "").Replace("False", "disabled");
                    ViewBag.ApprovalId = acccessPreviliege.MenuID;
                    ViewBag.ApprovalLevel = acccessPreviliege.ApprovalLevel;
                    ViewBag.ApprovalName = acccessPreviliege.ApprovalName;
                    ViewBag.UserId = uid;
                    ViewBag.CompId = compid;

                    Session["Layout"] = "portal";

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult ProblemInformationJson(string problemnumber, string inspectiongate, DateTime startDate, DateTime endDate, string searchFilter)
        {

            problemnumber = _SystemService.Vf(problemnumber);
            inspectiongate = _SystemService.Vf(inspectiongate);
            searchFilter = _SystemService.Vf(searchFilter);

            var ProblemInformation = (from a in vssp_db.Vw_QC_ProblemInformation
                                      where
                                         a.Status != 5 && a.ProblemNumber.Contains(problemnumber) && a.InspectionGate.Contains(inspectiongate) &&
                                        (a.ProblemDate >= startDate && a.ProblemDate <= endDate) &&
                                        (a.ProblemInformation.Contains(searchFilter) || a.FollowUp.Contains(searchFilter))
                                      orderby a.ProblemTime descending, a.ProblemNumber descending
                                      select new
                                      {
                                          a.ProblemNumber,
                                          a.ProblemDate,
                                          a.ProblemTime,
                                          a.ClosingDate,
                                          a.ClosingTime,
                                          a.InspectionType,
                                          a.InspectionGate,
                                          a.GateName,
                                          a.ProblemInformation,
                                          a.FollowUp,
                                          a.NotificationTo,
                                          a.Status,
                                          a.StatusName,
                                          a.UserId,
                                          a.EditDate
                                      }).ToList();

            //if (_SystemService.Vf(inspectiongate) != "*")
            //{
            //    ProblemInformation = ProblemInformation.Where(a => a.InspectionGate == inspectiongate).ToList();
            //}

            var jsonResult = Json(ProblemInformation, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult ProblemInformationApprovalListJson(string ProblemNumber, Nullable<bool> approved)
        {
            try
            {

                var ProblemInformationApproval = (from a in vssp_db.Tbl_QC_ProblemInformationApproval
                                                  where a.ProblemNumber.Contains(ProblemNumber)
                                                  orderby a.ApprovalLevel
                                                  select new { a.ProblemNumber, a.UserId, a.UserName, a.ApprovalLevel, a.ApprovalName, a.ApprovalEmail, a.SentEmail, a.SentEmailDate, a.Approved, a.ApprovedDate }).ToList();

                if (approved != null)
                {
                    ProblemInformationApproval = ProblemInformationApproval.Where(a => a.Approved == approved).ToList();
                }

                return Json(ProblemInformationApproval, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ProblemInformationApprovalUserList(string userid, string menuid)
        {
            List<UserApprovalListModel> userApprovalListsUpdate = _AccountService.UserApprovalType(userid, menuid).Where(a => a.UserID != userid).ToList();
            return Json(userApprovalListsUpdate, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudProblemInformation(string problemNumber, DateTime problemDate, DateTime problemTime, string inspectionGate, string problemInformation, string followUp, string notificationTo, string uid, string compId, string formAction)
        {

            try
            {

                string[] userList = notificationTo.Split(',');
                string[] sentTo = null;

                switch (formAction.ToLower())
                {
                    case "create":

                        string month = _SystemService.Vd(DateTime.Now.ToString(), "MM");
                        string years = _SystemService.Vd(DateTime.Now.ToString(), "yyyy");

                        var pnumber = vssp_db.SP_GET_ProblemNumber(month, years, compId).FirstOrDefault();
                        if(pnumber != null)
                        {
                            problemNumber = pnumber.ProblemNumber;
                        }

                        Tbl_QC_ProblemInformation ListProblemInformation = new Tbl_QC_ProblemInformation();
                        ListProblemInformation.ProblemNumber = problemNumber;
                        ListProblemInformation.ProblemDate = problemDate;
                        ListProblemInformation.ProblemTime = problemTime;
                        ListProblemInformation.InspectionGate = inspectionGate;
                        ListProblemInformation.ProblemInformation = problemInformation;
                        ListProblemInformation.FollowUp = followUp;
                        ListProblemInformation.Status = 0;
                        ListProblemInformation.UserId = uid;
                        ListProblemInformation.EditDate = DateTime.Now;

                        vssp_db.Tbl_QC_ProblemInformation.Add(ListProblemInformation);

                        /* crud Approval */
                        sentTo = crudProblemInformationApproval(userList, problemNumber, uid, formAction);

                        break;

                    case "update":

                        var ListUpdate = vssp_db.Tbl_QC_ProblemInformation.First(a => a.ProblemNumber == problemNumber);

                        ListUpdate.ProblemDate = problemDate;
                        ListUpdate.ProblemTime = problemTime;
                        ListUpdate.InspectionGate = inspectionGate;
                        ListUpdate.ProblemInformation = problemInformation;
                        ListUpdate.FollowUp = followUp;
                        ListUpdate.UserId = uid;
                        ListUpdate.EditDate = DateTime.Now;

                        /* crud Approval */
                        sentTo = crudProblemInformationApproval(userList, problemNumber, uid, formAction);

                        break;

                    case "closing":

                        var ListClosed = vssp_db.Tbl_QC_ProblemInformation.First(a => a.ProblemNumber == problemNumber);

                        ListClosed.ClosingDate = DateTime.UtcNow;
                        ListClosed.ClosingTime = DateTime.Now;
                        ListClosed.Status = 3;

                        break;
                    case "delete":

                        /* crud Approval */
                        sentTo = crudProblemInformationApproval(userList, problemNumber, uid, formAction);

                        var ListDelete = vssp_db.Tbl_QC_ProblemInformation.First(a => a.ProblemNumber == problemNumber);
                        
                        ListDelete.Status = 5;

                        break;
                }

                try
                {
                    vssp_db.SaveChanges();
                    var sender = _AccountService.UserEditList(uid).FirstOrDefault();
                    if (sentTo.Count() != 0 && sender != null)
                    {
                        foreach(var userid in sentTo) {

                            string body = "<b class='text-danger'>" + problemInformation + "</b><br/><br/>Please solve these problem as soon as possible!";
                            bool result = _SystemService.CrudNotification(userid, "Problem Information", sender.UserName, "Problem Information " + problemNumber, body, "");
                            if (result)
                            {
                                var approval = vssp_db.Tbl_QC_ProblemInformationApproval.Where(a=> a.ProblemNumber == problemNumber && a.UserId == userid).FirstOrDefault();
                                if(approval != null)
                                {
                                    approval.SentEmail = true;
                                    approval.SentEmailDate = DateTime.Now;
                                    vssp_db.SaveChanges();
                                }
                            }
                        }
                    }
                    return Json(problemNumber, JsonRequestBehavior.AllowGet);
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
        public string[] crudProblemInformationApproval(string[] ApprovalId, string ProblemNumber, string UserId, string action)
        {
            List<string> sentTo = new List<string>();

            switch (action.ToLower())
            {
                case "create":

                    /* create Details */
                    foreach (var userid in ApprovalId)
                    {
                        var users = _AccountService.UserEditList(userid).FirstOrDefault(); 
                        Tbl_QC_ProblemInformationApproval ListApproval = new Tbl_QC_ProblemInformationApproval();
                        ListApproval.ProblemNumber = ProblemNumber;
                        ListApproval.UserId = users.UserID;
                        ListApproval.UserName = users.UserName;
                        ListApproval.ApprovalLevel = 2;
                        ListApproval.ApprovalName = "Checked";
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
                            sentTo.Add(users.UserID);
                        }

                        vssp_db.Tbl_QC_ProblemInformationApproval.Add(ListApproval);

                    }

                    break;

                case "update":

                    /* remove change approval */
                    List<Tbl_QC_ProblemInformationApproval> UserApproval = vssp_db.Tbl_QC_ProblemInformationApproval.Where(a => a.ProblemNumber == ProblemNumber && !ApprovalId.Contains(a.UserId)).ToList();
                    foreach (var user in UserApproval)
                    {
                        vssp_db.Tbl_QC_ProblemInformationApproval.Remove(user);
                    }

                    /* create approval */
                    foreach (var userid in ApprovalId)
                    {
                        var users = _AccountService.UserEditList(userid).FirstOrDefault();
                        Tbl_QC_ProblemInformationApproval existUser = (from a in vssp_db.Tbl_QC_ProblemInformationApproval
                                                                       where a.ProblemNumber == ProblemNumber && a.UserId == users.UserID
                                                                       select a).FirstOrDefault();
                        if (existUser == null)
                        {
                            Tbl_QC_ProblemInformationApproval ListApproval = new Tbl_QC_ProblemInformationApproval();
                            ListApproval.ProblemNumber = ProblemNumber;
                            ListApproval.UserId = users.UserID;
                            ListApproval.UserName = users.UserName;
                            ListApproval.ApprovalLevel = 2;
                            ListApproval.ApprovalName = "Checked";
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
                                sentTo.Add(users.UserID);
                            }

                            vssp_db.Tbl_QC_ProblemInformationApproval.Add(ListApproval);

                        }
                    }

                    break;

                case "delete":

                    foreach (var userid in ApprovalId)
                    {
                        var ListDelete = vssp_db.Tbl_QC_ProblemInformationApproval.First(a => a.ProblemNumber == ProblemNumber && a.UserId == userid);

                        vssp_db.Tbl_QC_ProblemInformationApproval.Remove(ListDelete);
                    }
                    break;
            }
            return sentTo.ToArray();
        }
    }
}