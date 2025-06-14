using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Core.VSSP.Models;
using Core.VSSP.Services;

namespace Core.VSSP.Controllers
{
    public class DynamicLinqController : Controller
    {
        SystemService systemService = new SystemService();
        public string queryBuilder(PrintOptionModel printOption, List<FilterDataModel> filterData, MenuReportListModel menuReportList)
        {
            string WhereString = "";
            string MSQL = "";
            /* Builder where values */
            switch (menuReportList.SchemaType.ToLower())
            {
                case "procedure":
                    foreach (var filter in filterData)
                    {
                        if (systemService.Vf(filter.FilterValues) != "")
                        {
                            if (WhereString != "") WhereString += ", ";
                            WhereString += "@" + filter.Field;
                            if (filter.FilterType.ToLower() == "varchar" || filter.FilterType.ToLower() == "nvarchar")
                            {
                                WhereString += "='" + filter.FilterValues + "'";
                            }
                            else
                            if (filter.FilterType.ToLower() == "datetime")
                            {
                                WhereString += "='" + systemService.Vd(filter.FilterValues,"yyyy-MM-dd") + "'";
                            }
                            else
                            {
                                WhereString += "=" + filter.FilterValues + "";
                            }
                        }

                        /* Builder sql queries */
                        MSQL = "Exec " + menuReportList.SchemaName;
                        if (WhereString != "")
                        {
                            MSQL += " " + WhereString;
                        }

                    }
                    break;
                default:
                    if (filterData != null || menuReportList != null)
                    {
                        foreach (var filter in filterData)
                        {
                            if (systemService.Vf(filter.FilterValues) != "")
                            {
                                if (WhereString != "") WhereString += " and ";
                                if(filter.FilterName.ToLower() == "datetimerangepicker")
                                {
                                    WhereString += "format(" + filter.Field + ",'yyyy-MM-dd HH:mm')";
                                }
                                else
                                {
                                    WhereString += (filter.FilterType == "datetime" ? "format(" + filter.Field + ",'yyyy-MM-dd')" : filter.Field);
                                }
                                if (filter.FilterType.ToLower() == "varchar" || filter.FilterType.ToLower() == "nvarchar")
                                {
                                    WhereString += "='" + filter.FilterValues + "'";
                                }
                                else
                                if (filter.FilterType.ToLower() == "datetime")
                                {
                                    WhereString += " between '" + filter.FilterValues.Replace(" - ", "' and '") + "'";
                                }
                                else
                                {
                                    WhereString += "=" + filter.FilterValues + "";
                                }
                            }
                        }

                        /* Builder custom where values */
                        if (menuReportList.CustomFilter != "")
                        {
                            if (WhereString != "") WhereString += " and ";
                            WhereString += menuReportList.CustomFilter;
                        }

                        /* Builder sql queries */
                        MSQL = "Select * From " + menuReportList.SchemaName;
                        if (WhereString != "")
                        {
                            MSQL += " Where " + WhereString;
                        }
                        if (menuReportList.SortOrder != "")
                        {
                            MSQL += " Order By " + menuReportList.SortOrder;
                        }
                    }
                    break;
            }

            return MSQL;
                
        }
        
    }
    public static class CommonMethod
    {
        public static List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex) { }
                    }
                }
                return objT;
            }).ToList();
        }
    }
    
}