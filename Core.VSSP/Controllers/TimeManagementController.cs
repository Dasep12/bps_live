using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Core.VSSP.Models;
using Core.VSSP.Services;
using Core.VSSP.WorkEntity;
using Dapper;
using Newtonsoft.Json;

namespace Core.VSSP.Controllers
{
    public class TimeManagementController : Controller
    {
        // GET: TimeManagement
        AccountService  _AccountService = new AccountService();
        SystemService   _SystemService  = new SystemService();
        vssp_entity     vssp_db         = new vssp_entity();
        public ActionResult WorkingCalendar()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "TimeManagement", "WorkingCalendar");

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

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult WorkingCalendarListJson(string year = null, string month = null, Nullable<DateTime> calendardate = null, Nullable<DateTime> startdate = null, Nullable<DateTime> enddate = null)
        {
            if (_SystemService.Vf(year) == "")
            {
                year = DateTime.Now.ToString("yyyy");
            }
            if (_SystemService.Vf(month) == "")
            {
                month = DateTime.Now.ToString("MM");
            } else
            {
                int val = DateTime.ParseExact(month, "MMMM", CultureInfo.CurrentCulture).Month;
                if (val < 10)
                {
                    month = "0" + val.ToString();
                }else
                {
                    month = val.ToString();
                }
            }

            var CalendarView = new object();
            
            if (calendardate != null)
            {
                CalendarView = from a in vssp_db.Tbl_TMS_WorkingCalendar
                               join b in vssp_db.Tbl_TMS_WorkingCalendarDay on a.DayId equals b.DayId
                               where a.CalendarDate == calendardate
                               select new { a.DayId, b.DayName, b.Holidays, a.CalendarDate, a.Remarks, a.UserId, a.EditDate };
            }
            else
            {
                if (startdate == null)
                {
                    CalendarView = vssp_db.SP_TMS_CalendarView(year, month);
                }
                else
                {
                    CalendarView = from a in vssp_db.Tbl_TMS_WorkingCalendar
                                   join b in vssp_db.Tbl_TMS_WorkingCalendarDay on a.DayId equals b.DayId
                                   where a.CalendarDate >= startdate && a.CalendarDate <= enddate
                                   select new { a.DayId, b.DayName, b.Holidays, a.CalendarDate, a.Remarks, a.UserId, a.EditDate };

                }
            }

            return Json(CalendarView, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WorkingCalendarDayJson(string dayid)
        {

            dayid = _SystemService.Vf(dayid);

            var CalendarDay = from a in vssp_db.Tbl_TMS_WorkingCalendarDay
                              where a.DayId.Contains(dayid)
                              orderby a.DayId
                              select new { a.DayId, a.DayName, a.Holidays, a.UserId, a.EditDate };

            return Json(CalendarDay, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudWorkingCalendar(string DayId, DateTime CalendarDate, string Remarks, string formAction)
        {
            if (Session["UserID"] != null)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    Tbl_TMS_WorkingCalendar WorkingCalendar = new Tbl_TMS_WorkingCalendar();
                    WorkingCalendar.DayId = DayId;
                    WorkingCalendar.CalendarDate = CalendarDate;
                    WorkingCalendar.Remarks = Remarks;
                    WorkingCalendar.UserId = uid;
                    WorkingCalendar.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "Create":

                            vssp_db.Tbl_TMS_WorkingCalendar.Add(WorkingCalendar);

                            break;
                        case "Update":

                            var WorkingCalendarUpdate = vssp_db.Tbl_TMS_WorkingCalendar.First(a => a.CalendarDate == WorkingCalendar.CalendarDate);

                            WorkingCalendarUpdate.DayId = WorkingCalendar.DayId;
                            WorkingCalendarUpdate.Remarks = WorkingCalendar.Remarks;
                            WorkingCalendarUpdate.UserId = uid;
                            WorkingCalendarUpdate.EditDate = DateTime.Now;

                            break;
                        case "Delete":

                            var WorkingCalendarDelete = vssp_db.Tbl_TMS_WorkingCalendar.First(a => a.CalendarDate == WorkingCalendar.CalendarDate);

                            vssp_db.Tbl_TMS_WorkingCalendar.Remove(WorkingCalendarDelete);

                            break;
                    }
                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(WorkingCalendar, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;

                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult WorkingShift()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "TimeManagement", "WorkingShift");

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

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult WorkingShiftGroupJson(string groupid)
        {

            groupid = _SystemService.Vf(groupid);

            var ShiftGroup = from a in vssp_db.Tbl_TMS_WorkingShiftGroup
                              where a.GroupId.Contains(groupid)
                              orderby a.Sort
                              select new { a.GroupId, a.GroupName, a.WorkHour, a.Sort, a.UserId, a.EditDate };

            return Json(ShiftGroup, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WorkingShiftMasterJson(string groupid, string shiftid, Nullable<bool> workday)
        {

            groupid = _SystemService.Vf(groupid);
            shiftid = _SystemService.Vf(shiftid);
            var ShiftMaster = new object();

            if (groupid != "")
            {

                var Shift = (from a in vssp_db.Tbl_TMS_WorkingShiftMaster
                             where a.GroupId==groupid && a.ShiftId.Contains(shiftid)
                             join b in vssp_db.Tbl_TMS_WorkingDayType on a.DayType equals b.DayType
                             orderby a.GroupId, a.Sort
                             select new { a.GroupId, a.ShiftId, a.ShiftName, a.DayType, b.DayTypeName, a.Sort, a.UserId, a.EditDate }).ToList();

                if (workday != null)
                {
                    Shift = Shift.Where(a => a.DayType == workday).ToList();
                }

                ShiftMaster = Shift;

            } else
            {
                var Shift = (from a in vssp_db.Tbl_TMS_WorkingShiftMaster
                             where a.ShiftId.Contains(shiftid)
                             join b in vssp_db.Tbl_TMS_WorkingDayType on a.DayType equals b.DayType
                             orderby a.GroupId, a.Sort
                             select new { a.GroupId, a.ShiftId, a.ShiftName, a.DayType, b.DayTypeName, a.Sort, a.UserId, a.EditDate }).ToList();

                if (workday != null)
                {
                    Shift = Shift.Where(a => a.DayType == workday).ToList();
                }

                ShiftMaster = Shift;
            }

            return Json(ShiftMaster, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WorkingShiftHourJson(string groupid, string shiftid, string dayid)
        {

            groupid = _SystemService.Vf(groupid);
            shiftid = _SystemService.Vf(shiftid);
            dayid   = _SystemService.Vf(dayid);

            var ShiftHour = from a in vssp_db.Tbl_TMS_WorkingShiftHour
                            where a.GroupId == groupid && a.ShiftId == shiftid && a.DayId.Contains(dayid)
                            join b in vssp_db.Tbl_TMS_WorkingShiftMaster on a.ShiftId equals b.ShiftId
                            join c in vssp_db.Tbl_TMS_WorkingDay on a.DayId equals c.DayId
                            orderby a.GroupId, a.ShiftId, a.DayId
                            select new { a.GroupId, a.ShiftId, b.ShiftName, a.DayId, c.DayName, a.StartTime, a.EndTime, a.UserId, a.EditDate };

            return Json(ShiftHour, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WorkingDayTypeJson()
        {

            var DayType = from a in vssp_db.Tbl_TMS_WorkingDayType
                          orderby a.DayTypeName descending
                          select new { a.DayType, a.DayTypeName };

            return Json(DayType, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WorkingDayJson()
        {

            var Day = from a in vssp_db.Tbl_TMS_WorkingDay
                          orderby a.DayId
                          select new { a.DayId, a.DayName };

            return Json(Day, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WorkingShiftPatternGroupJson(string groupid)
        {

            groupid = _SystemService.Vf(groupid);

            var ShiftPatternGroup = from a in vssp_db.Tbl_TMS_WorkingShiftPatternGroup
                             where a.GroupId.Contains(groupid)
                             orderby a.Sort
                             select new { a.GroupId, a.GroupName, a.StartDate, a.Actived, a.Sort, a.UserId, a.EditDate };

            return Json(ShiftPatternGroup, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WorkingShiftPatternMasterJson(string groupid)
        {

            groupid = _SystemService.Vf(groupid);

            var ShiftPatternMaster = from a in vssp_db.Tbl_TMS_WorkingShiftPatternMaster
                              where a.GroupId == groupid
                              join b in vssp_db.Tbl_TMS_WorkingShiftMaster on a.ShiftId equals b.ShiftId
                              orderby a.GroupId,a.PatternNumber
                              select new { a.GroupId, a.PatternNumber, a.ShiftId, b.ShiftName, a.TotalDay, a.UserId, a.EditDate };

            return Json(ShiftPatternMaster, JsonRequestBehavior.AllowGet);
        }
        public ActionResult crudWorkingShiftGroup(string GroupId, string GroupName, int WorkHour, int Sort, string formAction)
        {
            if (Session["UserID"] != null)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    Tbl_TMS_WorkingShiftGroup WorkingShiftGroup = new Tbl_TMS_WorkingShiftGroup();
                    WorkingShiftGroup.GroupId = GroupId;
                    WorkingShiftGroup.GroupName = GroupName;
                    WorkingShiftGroup.WorkHour = WorkHour;
                    WorkingShiftGroup.Sort = Sort;
                    WorkingShiftGroup.UserId = uid;
                    WorkingShiftGroup.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "Create":

                            vssp_db.Tbl_TMS_WorkingShiftGroup.Add(WorkingShiftGroup);

                            break;
                        case "Update":

                            var WorkingShiftGroupUpdate = vssp_db.Tbl_TMS_WorkingShiftGroup.First(a => a.GroupId == WorkingShiftGroup.GroupId);

                            WorkingShiftGroupUpdate.GroupName = WorkingShiftGroup.GroupName;
                            WorkingShiftGroupUpdate.WorkHour = WorkingShiftGroup.WorkHour;
                            WorkingShiftGroupUpdate.Sort = WorkingShiftGroup.Sort;
                            WorkingShiftGroupUpdate.UserId = uid;
                            WorkingShiftGroupUpdate.EditDate = DateTime.Now;

                            break;
                        case "Delete":

                            var WorkingShiftGroupDelete = vssp_db.Tbl_TMS_WorkingShiftGroup.First(a => a.GroupId == WorkingShiftGroup.GroupId);

                            vssp_db.Tbl_TMS_WorkingShiftGroup.Remove(WorkingShiftGroupDelete);

                            break;
                    }
                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(WorkingShiftGroup, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;

                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult crudWorkingShiftMaster(string GroupId, string ShiftId, string ShiftName, bool DayType, int Sort, string formAction)
        {
            if (Session["UserID"] != null)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    Tbl_TMS_WorkingShiftMaster WorkingShiftMaster = new Tbl_TMS_WorkingShiftMaster();
                    WorkingShiftMaster.GroupId = GroupId;
                    WorkingShiftMaster.ShiftId = ShiftId;
                    WorkingShiftMaster.ShiftName = ShiftName;
                    WorkingShiftMaster.DayType = DayType;
                    WorkingShiftMaster.Sort = Sort;
                    WorkingShiftMaster.UserId = uid;
                    WorkingShiftMaster.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "Create":

                            vssp_db.Tbl_TMS_WorkingShiftMaster.Add(WorkingShiftMaster);

                            break;
                        case "Update":

                            var WorkingShiftMasterUpdate = vssp_db.Tbl_TMS_WorkingShiftMaster.First(a => a.GroupId == WorkingShiftMaster.GroupId && a.ShiftId == WorkingShiftMaster.ShiftId);

                            WorkingShiftMasterUpdate.ShiftName = WorkingShiftMaster.ShiftName;
                            WorkingShiftMasterUpdate.DayType = WorkingShiftMaster.DayType;
                            WorkingShiftMasterUpdate.Sort = WorkingShiftMaster.Sort;
                            WorkingShiftMasterUpdate.UserId = uid;
                            WorkingShiftMasterUpdate.EditDate = DateTime.Now;

                            break;
                        case "Delete":

                            var WorkingShiftMasterDelete = vssp_db.Tbl_TMS_WorkingShiftMaster.First(a => a.GroupId == WorkingShiftMaster.GroupId && a.ShiftId == WorkingShiftMaster.ShiftId);

                            vssp_db.Tbl_TMS_WorkingShiftMaster.Remove(WorkingShiftMasterDelete);

                            break;
                    }
                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(WorkingShiftMaster, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;

                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult crudWorkingShiftHour(string GroupId, string ShiftId, string DayId, DateTime StartTime, DateTime EndTime, string formAction)
        {
            if (Session["UserID"] != null)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    Tbl_TMS_WorkingShiftHour WorkingShiftHour = new Tbl_TMS_WorkingShiftHour();
                    WorkingShiftHour.GroupId = GroupId;
                    WorkingShiftHour.ShiftId = ShiftId;
                    WorkingShiftHour.DayId = DayId;
                    WorkingShiftHour.StartTime = StartTime;
                    WorkingShiftHour.EndTime = EndTime;
                    WorkingShiftHour.UserId = uid;
                    WorkingShiftHour.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "Create":

                            vssp_db.Tbl_TMS_WorkingShiftHour.Add(WorkingShiftHour);

                            break;
                        case "Update":

                            var WorkingShiftHourUpdate = vssp_db.Tbl_TMS_WorkingShiftHour.First(a => a.GroupId == WorkingShiftHour.GroupId && a.ShiftId == WorkingShiftHour.ShiftId && a.DayId == WorkingShiftHour.DayId);

                            WorkingShiftHourUpdate.StartTime = WorkingShiftHour.StartTime;
                            WorkingShiftHourUpdate.EndTime = WorkingShiftHour.EndTime;
                            WorkingShiftHourUpdate.UserId = uid;
                            WorkingShiftHourUpdate.EditDate = DateTime.Now;

                            break;
                        case "Delete":

                            var WorkingShiftHourDelete = vssp_db.Tbl_TMS_WorkingShiftHour.First(a => a.GroupId == WorkingShiftHour.GroupId && a.ShiftId == WorkingShiftHour.ShiftId && a.DayId == WorkingShiftHour.DayId);

                            vssp_db.Tbl_TMS_WorkingShiftHour.Remove(WorkingShiftHourDelete);

                            break;
                    }
                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(WorkingShiftHour, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;

                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult crudWorkingShiftPatternGroup(string GroupId, string GroupName, DateTime StartDate, int Sort, bool Actived, string formAction)
        {
            if (Session["UserID"] != null)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    Tbl_TMS_WorkingShiftPatternGroup WorkingShiftPatternGroup = new Tbl_TMS_WorkingShiftPatternGroup();
                    WorkingShiftPatternGroup.GroupId = GroupId;
                    WorkingShiftPatternGroup.GroupName = GroupName;
                    WorkingShiftPatternGroup.StartDate = StartDate;
                    WorkingShiftPatternGroup.Sort = Sort;
                    WorkingShiftPatternGroup.Actived = Actived;
                    WorkingShiftPatternGroup.UserId = uid;
                    WorkingShiftPatternGroup.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "Create":

                            vssp_db.Tbl_TMS_WorkingShiftPatternGroup.Add(WorkingShiftPatternGroup);

                            break;
                        case "Update":

                            var WorkingShiftPatternGroupUpdate = vssp_db.Tbl_TMS_WorkingShiftPatternGroup.First(a => a.GroupId == WorkingShiftPatternGroup.GroupId);

                            WorkingShiftPatternGroupUpdate.GroupName = WorkingShiftPatternGroup.GroupName;
                            WorkingShiftPatternGroupUpdate.StartDate = WorkingShiftPatternGroup.StartDate;
                            WorkingShiftPatternGroupUpdate.Sort = WorkingShiftPatternGroup.Sort;
                            WorkingShiftPatternGroupUpdate.Actived = WorkingShiftPatternGroup.Actived;
                            WorkingShiftPatternGroupUpdate.UserId = uid;
                            WorkingShiftPatternGroupUpdate.EditDate = DateTime.Now;

                            break;
                        case "Delete":

                            var WorkingShiftPatternGroupDelete = vssp_db.Tbl_TMS_WorkingShiftPatternGroup.First(a => a.GroupId == WorkingShiftPatternGroup.GroupId);

                            vssp_db.Tbl_TMS_WorkingShiftPatternGroup.Remove(WorkingShiftPatternGroupDelete);

                            break;
                    }
                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(WorkingShiftPatternGroup, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;

                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult crudWorkingShiftPatternMaster(string GroupId, int PatternNumber, string ShiftId, int TotalDay, string formAction)
        {
            if (Session["UserID"] != null)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    Tbl_TMS_WorkingShiftPatternMaster WorkingShiftPatternMaster = new Tbl_TMS_WorkingShiftPatternMaster();
                    WorkingShiftPatternMaster.GroupId = GroupId;
                    WorkingShiftPatternMaster.PatternNumber = PatternNumber;
                    WorkingShiftPatternMaster.ShiftId = ShiftId;
                    WorkingShiftPatternMaster.TotalDay = TotalDay;
                    WorkingShiftPatternMaster.UserId = uid;
                    WorkingShiftPatternMaster.EditDate = DateTime.Now;

                    switch (formAction)
                    {
                        case "Create":

                            vssp_db.Tbl_TMS_WorkingShiftPatternMaster.Add(WorkingShiftPatternMaster);

                            break;
                        case "Update":

                            var WorkingShiftPatternMasterUpdate = vssp_db.Tbl_TMS_WorkingShiftPatternMaster.First(a => a.GroupId == WorkingShiftPatternMaster.GroupId && a.PatternNumber == WorkingShiftPatternMaster.PatternNumber);

                            WorkingShiftPatternMasterUpdate.ShiftId = WorkingShiftPatternMaster.ShiftId;
                            WorkingShiftPatternMasterUpdate.TotalDay = WorkingShiftPatternMaster.TotalDay;
                            WorkingShiftPatternMasterUpdate.UserId = uid;
                            WorkingShiftPatternMasterUpdate.EditDate = DateTime.Now;

                            break;
                        case "Delete":

                            var WorkingShiftPatternMasterDelete = vssp_db.Tbl_TMS_WorkingShiftPatternMaster.First(a => a.GroupId == WorkingShiftPatternMaster.GroupId && a.PatternNumber == WorkingShiftPatternMaster.PatternNumber);

                            vssp_db.Tbl_TMS_WorkingShiftPatternMaster.Remove(WorkingShiftPatternMasterDelete);

                            break;
                    }
                    try
                    {
                        vssp_db.SaveChanges();
                        return Json(WorkingShiftPatternMaster, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;

                }

            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult ShiftSchedule()
        {
            if (Session["UserID"] != null)
            {

                string uid = Session["UserID"].ToString();
                var acccessPreviliege = _AccountService.AccessPreviliege(uid, "TimeManagement", "ShiftSchedule");

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

                    ExportOptionModel exportOption = new ExportOptionModel();
                    exportOption.ExportList = _SystemService.ComboExport().ToList();

                    Session["Layout"] = "portal";
                    return View(exportOption);
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ShiftScheduleJson(string groupid, Nullable<DateTime> startdate=null,Nullable<DateTime> enddate = null)
        {
            if (startdate == null)
            {
                startdate = DateTime.Now.AddDays(-15);
            }
            if (enddate == null)
            {
                enddate = DateTime.Now.AddDays(15);
            }
            //var ShiftSchedule = vssp_db.SP_TMS_WorkingShiftSchedule(startdate,enddate,groupid);
            
            string DBMaster = ConfigurationManager.ConnectionStrings["DBMaster"].ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(DBMaster);

            string databaseName = vssp_db.Database.Connection.Database;

            var connetionString = "Data Source=" + builder.DataSource + ";Initial Catalog="+ databaseName + ";User ID=" + builder.UserID + ";Password=" + builder.Password + "";

            IEnumerable<IDictionary<string, object>> ShiftSchedule;

            using (var cnn = new SqlConnection(connetionString))
            {
                cnn.Open();

                var p = new DynamicParameters();
                p.Add("@StartDate", startdate, DbType.DateTime);
                p.Add("@EndDate", enddate, DbType.DateTime);
                p.Add("@GroupId", groupid, DbType.String);

                ShiftSchedule = (IEnumerable<IDictionary<string, object>>)
                            cnn.Query(sql: "SP_TMS_WorkingShiftSchedule",
                                      param: p,
                                      commandType: CommandType.StoredProcedure);
            }

            return Json(ShiftSchedule, JsonRequestBehavior.AllowGet);
        }
        public ActionResult createShiftSchedule(string GroupId, DateTime StartDate, DateTime EndDate, bool Replace)
        {

            if (Session["UserID"] != null)
            {
                try
                {
                    if (GroupId == "all")
                    {
                        GroupId = null;
                    }
                    string uid = Session["UserID"].ToString();
                    var ShiftSchedule = vssp_db.SP_CRUD_WorkingShiftSchedule(GroupId, StartDate, EndDate, Replace, uid);
                    return Json(ShiftSchedule, JsonRequestBehavior.AllowGet);

                } catch(Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult updateShiftSchedule(string GroupId, DateTime DateShift, string CurrentShift, string NewShift)
        {

            if (Session["UserID"] != null)
            {
                try
                {

                    string uid = Session["UserID"].ToString();

                    /* DELETE OLD SCHEDULE */
                    var currentShiftSchedule = vssp_db.Tbl_TMS_WorkingShiftSchedule.First(a => a.GroupId == GroupId && a.DateShift == DateShift && a.ShiftId == CurrentShift);
                    vssp_db.Tbl_TMS_WorkingShiftSchedule.Remove(currentShiftSchedule);

                    /* CREATE NEW SCHEDULE */
                    var ShiftPatternMaster = vssp_db.Tbl_TMS_WorkingShiftPatternMaster.First(a => a.GroupId == GroupId && a.ShiftId == NewShift);
                    Tbl_TMS_WorkingShiftSchedule ShiftSchedule = new Tbl_TMS_WorkingShiftSchedule();
                    ShiftSchedule.GroupId = currentShiftSchedule.GroupId;
                    ShiftSchedule.PatternNumber = ShiftPatternMaster.PatternNumber;
                    ShiftSchedule.ShiftId = NewShift;
                    ShiftSchedule.DateShift = currentShiftSchedule.DateShift;
                    ShiftSchedule.UserId = uid;
                    ShiftSchedule.EditDate = DateTime.Now;

                    vssp_db.Tbl_TMS_WorkingShiftSchedule.Add(ShiftSchedule);

                    try
                    {
                        vssp_db.SaveChanges();
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var errinfo = _SystemService.GetExceptionDetails(e);
                        return Json(errinfo, JsonRequestBehavior.AllowGet);
                        throw;
                    }

                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errinfo = _SystemService.GetExceptionDetails(e);
                    return Json(errinfo, JsonRequestBehavior.AllowGet);
                    throw;
                }
            }
            else
            {
                Session["History"] = HttpContext.Request.Url.AbsolutePath;
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult GetWorkDayJson(string month)
        {
            try
            {
                string[] arrs = _SystemService.Vf(month).Split('/');
                string ordermonth = "";
                string orderyears = "";
                DateTime now = DateTime.Now;
                DateTime startdate = now;
                DateTime enddate = now;

                if (month != null)
                {
                    ordermonth = arrs[0];
                    orderyears = arrs[1];
                    startdate = new DateTime(int.Parse(orderyears), int.Parse(ordermonth), 1);
                    enddate = startdate.AddMonths(1).AddDays(-1);
                } else
                {
                    startdate = new DateTime(now.Year, now.Month, 1);
                    enddate = startdate.AddMonths(1).AddDays(-1);
                }

                var Workday = vssp_db.SP_GET_WorkDay(startdate, enddate, "N", false);

                return Json(Workday, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errinfo = _SystemService.GetExceptionDetails(e);
                return Json(errinfo, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ShiftGroupJson(string groupid)
        {

            groupid = _SystemService.Vf(groupid);

            var ShiftGroup = from a in vssp_db.Vw_TMS_ShiftGroup
                             where a.GroupId.Contains(groupid)
                             orderby a.Sort
                             select new { a.GroupId, a.GroupName, a.TotalShift, a.Sort};

            return Json(ShiftGroup, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetShiftListJson(int shift = 0)
        {

            var ShiftList = (from a in vssp_db.Vw_TMS_ShiftGroup
                            join b in vssp_db.Tbl_TMS_WorkingShiftMaster on a.GroupId equals b.GroupId
                            where a.TotalShift == shift
                            orderby a.Sort, b.Sort
                            select new { a.GroupId, a.GroupName, a.TotalShift, b.ShiftId, b.ShiftName }).ToList();

            return Json(ShiftList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTotalWorkdayJson(DateTime start, DateTime end, string groupid)
        {

            var TotalWorkday = vssp_db.SP_TMS_WorkingDay(start, end, groupid).ToList();

            return Json(TotalWorkday, JsonRequestBehavior.AllowGet);
        }
    }
}