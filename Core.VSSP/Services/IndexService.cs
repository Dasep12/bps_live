using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IO;

using Core.VSSP.Models;

namespace Core.VSSP.Services
{

    public class IndexService
    {
        //Db Connection string
        string DBCon = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
        string MSQL = "";
        SystemService _SystemService = new SystemService();
        CryptoLibService cryptoLib = new CryptoLibService();
        public IEnumerable<CMSListModel> CMSList(string FormAction, string Category, string ID, string Title)
        {

            if (Category == null) { Category = ""; }
            if (ID == null) { ID = ""; }
            if (Title == null) { Title = ""; }

            List<CMSListModel> _CMSListModel = new List<CMSListModel>();

            if (FormAction == "NewsRead")
            {
                using (SqlConnection conn = new SqlConnection(DBCon))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("", conn);

                    MSQL = "Update Tbl_CMS_MainIndex With (TabLock)  set Readed = (isnull(Readed,0)+1) where ID='" + ID + "'";
                    cmd.CommandText = MSQL;
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    if (FormAction == "Top4News")
                    {
                        MSQL = "SELECT Top 4 * FROM Vw_CMS_MainIndex Where Publish=1 ";
                        if (Category != "")
                        {
                            MSQL += "and Category='" + Category + "' ";
                        }
                        MSQL += "order by PublishedDate Desc";
                    }
                    else
                    if (FormAction == "Top10")
                    {
                        MSQL = "SELECT Top 10 * FROM Vw_CMS_MainIndex  Where ID Not In ('" + ID + "') ";
                        if (Category != "")
                        {
                            MSQL += "and Category='" + Category + "' ";
                        }
                        MSQL += " Order By Readed Desc";

                    }
                    else
                    if (FormAction == "NewsRead")
                    {
                        if (ID != "")
                        {
                            MSQL = "SELECT * FROM Vw_CMS_MainIndex  Where ID = '" + ID + "' ";
                        }
                        else
                        {
                            MSQL = "SELECT Top 1 * FROM Vw_CMS_MainIndex  Where Publish=1 order by ID desc ";
                        }
                    }
                    else
                    if (FormAction == "Archive")
                    {
                        MSQL = "SELECT Top 15 * FROM Vw_CMS_MainIndex  Where Publish=1 and ID<>'" + ID + "' ";
                        MSQL += "Order By Tahun Desc, Bulan Desc, PublishedDate Desc";

                    }
                    else
                    if (FormAction == "NewsRead")
                    {
                        MSQL = "SELECT * FROM Vw_CMS_MainIndex  Where ID = '" + ID + "' ";
                    }
                    else
                    if (FormAction == "Corporate")
                    {
                        MSQL = "SELECT * FROM Vw_CMS_MainIndex  Where Publish=1 and Category='" + Category + "' ";
                        if (Title != "")
                        {
                            MSQL += "and Title='" + Title + "' ";
                        }
                        MSQL += "Order By ID Asc";

                    }
                    else
                    if (FormAction == "Technology")
                    {
                        MSQL = "SELECT * FROM Vw_CMS_MainIndex  Where Publish=1 and Category='" + Category + "' ";
                        MSQL += "Order By ID Asc";

                    }
                    else
                    {
                        MSQL = "SELECT * FROM Vw_CMS_MainIndex  Where ID is not null ";
                        if (ID != "")
                        {
                            MSQL += "and ID='" + ID + "' ";
                        }
                        if (Category != "")
                        {
                            MSQL += "and Category='" + Category + "' ";
                        }
                        MSQL += "order by Category,ID Desc";
                    }

                    cmd.CommandText = MSQL;
                    conn.Open();
                    int nom = 0;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        CMSListModel __CMSListModel = new CMSListModel();
                        nom += 1;
                        __CMSListModel.No = nom.ToString();
                        __CMSListModel.Tahun = reader["Tahun"].ToString();
                        __CMSListModel.Bulan = reader["Bulan"].ToString();
                        __CMSListModel.NamaBulan = reader["NamaBulan"].ToString();
                        __CMSListModel.ID = reader["ID"].ToString();
                        __CMSListModel.Category = reader["Category"].ToString();
                        __CMSListModel.Title = reader["Title"].ToString();
                        __CMSListModel.SubTitle = reader["SubTitle"].ToString();
                        __CMSListModel.IsContent = reader["IsContent"].ToString();
                        __CMSListModel.Publish = bool.Parse(reader["Publish"].ToString());
                        __CMSListModel.CreateDate = _SystemService.Vd(reader["CreateDate"].ToString());
                        __CMSListModel.PublishDate = _SystemService.Vd(reader["PublishedDate"].ToString());
                        __CMSListModel.Editor = reader["Editor"].ToString();
                        __CMSListModel.Readed = _SystemService.Vn(reader["Readed"].ToString());
                        __CMSListModel.Liked = _SystemService.Vn(reader["Liked"].ToString());

                        if (reader["Image"].GetType() != typeof(DBNull))
                        {
                            __CMSListModel.Image = (byte[])reader["Image"];
                        }


                        _CMSListModel.Add(__CMSListModel);
                    }
                }
            }
            return _CMSListModel;
        }
        public CMSCrudModel CMSShowData(string ID, string filter)
        {
            CMSCrudModel _CMSCrudModel = new CMSCrudModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "SELECT * FROM Tbl_CMS_MainIndex Where ID is not null ";
                    if (ID != null || ID?.ToString() != "")
                    {
                        MSQL += "and ID='" + ID + "' ";
                    }
                    MSQL += "order by Category,CreateDate Desc";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        _CMSCrudModel.ID = reader["ID"].ToString();
                        _CMSCrudModel.Category = reader["Category"].ToString();
                        _CMSCrudModel.Title = reader["Title"].ToString();
                        _CMSCrudModel.SubTitle = reader["SubTitle"].ToString();
                        _CMSCrudModel.IsContent = reader["IsContent"].ToString();
                        _CMSCrudModel.Publish = bool.Parse(reader["Publish"].ToString());
                        _CMSCrudModel.CreateDate = _SystemService.Vd(reader["CreateDate"].ToString());
                        _CMSCrudModel.PublishedDate = _SystemService.Vd(reader["PublishedDate"].ToString());
                        _CMSCrudModel.Editor = reader["Editor"].ToString();

                        if (reader["Image"].GetType() != typeof(DBNull))
                        {
                            _CMSCrudModel.Image = (byte[])reader["Image"];
                        }

                    }
                }
            }
            return _CMSCrudModel;
        }

        public IEnumerable<CMSCategoryModel> ComboCMSCategory()
        {
            List<CMSCategoryModel> _CMSCategoryModel = new List<CMSCategoryModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    MSQL = "SELECT * FROM Tbl_CMS_Category order by ID ";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    int loop = 0;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                    reloop:
                        loop = loop + 1;
                        CMSCategoryModel __CMSCategoryModel = new CMSCategoryModel();

                        if (loop == 1)
                        {
                            __CMSCategoryModel.ID = "";
                            __CMSCategoryModel.Name = "--CMS Category--";

                            _CMSCategoryModel.Add(__CMSCategoryModel);
                            goto reloop;
                        }

                        __CMSCategoryModel.ID = reader["ID"].ToString();
                        __CMSCategoryModel.Name = reader["Name"].ToString();

                        _CMSCategoryModel.Add(__CMSCategoryModel);
                    }
                }
            }
            return _CMSCategoryModel;
        }
        public int CMSCrud(CMSCrudModel model, string FormAction)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_CRUD_CMSMainIndex", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", model.ID);
                cmd.Parameters.AddWithValue("@Category", model.Category);
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@SubTitle", model.SubTitle);
                cmd.Parameters.AddWithValue("@IsContent", model.IsContent);
                cmd.Parameters.AddWithValue("@Image", model.Image);
                cmd.Parameters.AddWithValue("@Publish", model.Publish);
                cmd.Parameters.AddWithValue("@CreateDate", model.CreateDate);
                cmd.Parameters.AddWithValue("@PublishedDate", model.PublishedDate);
                cmd.Parameters.AddWithValue("@Editor", model.Editor);
                cmd.Parameters.AddWithValue("@FormAction", FormAction);

                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }
        public int CMSDelete(string ID, string FormAction)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_CRUD_CMSMainIndex", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@FormAction", FormAction);

                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }
        public IEnumerable<NEWSCategoryModel> NEWSCategories()
        {
            List<NEWSCategoryModel> _NEWSCategoryModel = new List<NEWSCategoryModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    MSQL = "SELECT * FROM Tbl_NEWS_Category order by CategoryID ";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        NEWSCategoryModel __NEWSCategoryModel = new NEWSCategoryModel();

                        __NEWSCategoryModel.ID = reader["CategoryID"].ToString();
                        __NEWSCategoryModel.Name = reader["CategoryName"].ToString();

                        _NEWSCategoryModel.Add(__NEWSCategoryModel);
                    }
                }
            }
            return _NEWSCategoryModel;
        }
        public IEnumerable<NEWSListModel> NEWSList(string FormAction, string Category, string ID, string Title)
        {

            if (Category == null) { Category = ""; }
            if (ID == null) { ID = ""; }
            if (Title == null) { Title = ""; }

            List<NEWSListModel> _NEWSListModel = new List<NEWSListModel>();

            if (FormAction == "NewsRead")
            {
                using (SqlConnection conn = new SqlConnection(DBCon))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("", conn);

                    MSQL = "Update Tbl_NEWS_MainIndex With (TabLock)  set Readed = (isnull(Readed,0)+1) where ID='" + ID + "'";
                    cmd.CommandText = MSQL;
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    if (FormAction == "LatestNews")
                    {
                        MSQL = "SELECT Top 10 * FROM Vw_NEWS_MainIndex Where Publish=1 ";
                        if (Category != "")
                        {
                            MSQL += "and Category='" + Category + "' ";
                        }
                        MSQL += "order by PublishedDate Desc";
                    }
                    else
                    if (FormAction == "Top10")
                    {
                        MSQL = "SELECT Top 10 * FROM Vw_NEWS_MainIndex Where Publish=1 and ID Not In ('" + ID + "') ";
                        if (Category != "")
                        {
                            MSQL += "and Category='" + Category + "' ";
                        }
                        MSQL += " Order By Readed Desc";

                    }
                    else
                    if (FormAction == "NewsRead")
                    {
                        if (ID != "")
                        {
                            MSQL = "SELECT * FROM Vw_NEWS_MainIndex  Where ID = '" + ID + "' ";
                        }
                        else
                        {
                            MSQL = "SELECT Top 1 * FROM Vw_NEWS_MainIndex  Where Publish=1 order by ID desc ";
                        }
                    }
                    else
                    if (FormAction == "Archive")
                    {
                        MSQL = "SELECT Top 10 * FROM Vw_NEWS_MainIndex  Where Publish=1 and ID<>'" + ID + "' ";
                        MSQL += "Order By Tahun Desc, Bulan Desc, PublishedDate Desc";

                    }
                    else
                    if (FormAction == "CategoryList")
                    {
                        MSQL = "SELECT * FROM Vw_NEWS_MainIndex  Where Publish=1 and Category='" + Category + "' ";
                        if (Title != "")
                        {
                            MSQL += "and Title='" + Title + "' ";
                        }
                        MSQL += "Order By PublishedDate Desc";

                    }
                    else
                    if (FormAction == "Search")
                    {
                        MSQL = "SELECT * FROM Vw_NEWS_MainIndex  Where Publish=1 and ((CategoryName Like '%"+Title+ "%') or (Title  Like '%" + Title + "%') or (IsContent Like '%" + Title + "%')) ";
                        MSQL += "Order By PublishedDate Desc";

                    }
                    else
                    {
                        MSQL = "SELECT * FROM Vw_NEWS_MainIndex Where ID is not null ";
                        if (ID != "")
                        {
                            MSQL += "and ID='" + ID + "' ";
                        }
                        if (Category != "")
                        {
                            MSQL += "and Category='" + Category + "' ";
                        }
                        MSQL += "order by Category,ID Desc";
                    }

                    cmd.CommandText = MSQL;
                    conn.Open();
                    int nom = 0;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        NEWSListModel __NEWSListModel = new NEWSListModel();
                        nom += 1;
                        __NEWSListModel.No              = nom.ToString();
                        __NEWSListModel.Tahun           = reader["Tahun"].ToString();
                        __NEWSListModel.Bulan           = reader["Bulan"].ToString();
                        __NEWSListModel.NamaBulan       = reader["NamaBulan"].ToString();
                        __NEWSListModel.ID              = reader["ID"].ToString();
                        __NEWSListModel.Category        = reader["Category"].ToString();
                        __NEWSListModel.CategoryName    = reader["CategoryName"].ToString();
                        __NEWSListModel.Title           = reader["Title"].ToString();
                        __NEWSListModel.IsContent       = reader["IsContent"].ToString();
                        __NEWSListModel.Publish         = bool.Parse(reader["Publish"].ToString());
                        __NEWSListModel.CreateDate      = _SystemService.Vd(reader["CreateDate"].ToString());
                        __NEWSListModel.PublishDate     = _SystemService.Vd(reader["PublishedDate"].ToString());
                        __NEWSListModel.Editor          = reader["Editor"].ToString();
                        __NEWSListModel.Readed          = _SystemService.Vn(reader["Readed"].ToString());
                        __NEWSListModel.Liked           = _SystemService.Vn(reader["Liked"].ToString());

                        if (reader["Image"].GetType() != typeof(DBNull))
                        {
                            __NEWSListModel.Image = (byte[])reader["Image"];
                        }


                        _NEWSListModel.Add(__NEWSListModel);
                    }
                }
            }
            return _NEWSListModel;
        }
        public NEWSCrudModel NEWSShowData(string ID, string filter)
        {
            NEWSCrudModel _NEWSCrudModel = new NEWSCrudModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "SELECT * FROM Tbl_NEWS_MainIndex Where ID is not null ";
                    if (ID != null || ID?.ToString() != "")
                    {
                        MSQL += "and ID='" + ID + "' ";
                    }
                    MSQL += "order by Category,CreateDate Desc";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        _NEWSCrudModel.ID = reader["ID"].ToString();
                        _NEWSCrudModel.Category = reader["Category"].ToString();
                        _NEWSCrudModel.Title = reader["Title"].ToString();
                        _NEWSCrudModel.IsContent = reader["IsContent"].ToString();
                        _NEWSCrudModel.Publish = bool.Parse(reader["Publish"].ToString());
                        _NEWSCrudModel.CreateDate = _SystemService.Vd(reader["CreateDate"].ToString());
                        _NEWSCrudModel.PublishedDate = _SystemService.Vd(reader["PublishedDate"].ToString());
                        _NEWSCrudModel.Editor = reader["Editor"].ToString();

                        if (reader["Image"].GetType() != typeof(DBNull))
                        {
                            _NEWSCrudModel.Image = (byte[])reader["Image"];
                        }

                    }
                }
            }
            return _NEWSCrudModel;
        }

        public IEnumerable<NEWSCategoryModel> ComboNEWSCategory()
        {
            List<NEWSCategoryModel> _NEWSCategoryModel = new List<NEWSCategoryModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    MSQL = "SELECT * FROM Tbl_NEWS_Category order by CategoryID ";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    int loop = 0;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                    reloop:
                        loop = loop + 1;
                        NEWSCategoryModel __NEWSCategoryModel = new NEWSCategoryModel();

                        if (loop == 1)
                        {
                            __NEWSCategoryModel.ID = "";
                            __NEWSCategoryModel.Name = "--NEWS Category--";

                            _NEWSCategoryModel.Add(__NEWSCategoryModel);
                            goto reloop;
                        }

                        __NEWSCategoryModel.ID = reader["CategoryID"].ToString();
                        __NEWSCategoryModel.Name = reader["CategoryName"].ToString();

                        _NEWSCategoryModel.Add(__NEWSCategoryModel);
                    }
                }
            }
            return _NEWSCategoryModel;
        }
        public int NEWSCrud(NEWSCrudModel model, string FormAction)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_CRUD_NEWSMainIndex", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", model.ID);
                cmd.Parameters.AddWithValue("@Category", model.Category);
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@IsContent", model.IsContent);
                cmd.Parameters.AddWithValue("@Image", model.Image);
                cmd.Parameters.AddWithValue("@Publish", model.Publish);
                cmd.Parameters.AddWithValue("@CreateDate", model.CreateDate);
                cmd.Parameters.AddWithValue("@PublishedDate", model.PublishedDate);
                cmd.Parameters.AddWithValue("@Editor", model.Editor);
                cmd.Parameters.AddWithValue("@FormAction", FormAction);

                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }
        public int NEWSDelete(string ID, string FormAction)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_CRUD_NEWSMainIndex", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@FormAction", FormAction);

                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }
        

    }
}