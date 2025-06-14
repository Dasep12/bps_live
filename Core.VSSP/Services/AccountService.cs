using Core.VSSP.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.Services;
using System.Data;
using Newtonsoft.Json;
using Core.VSSP.WorkEntity;

namespace Core.VSSP.Services
{
    public class AccountService : Controller
    {

        //Db Connection string
        string DBCon = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
        string DBVSSP = ConfigurationManager.ConnectionStrings["vssp_entity"].ConnectionString;
        string MSQL = "";
        int nom = 0;
        int loop = 0;
        SystemService _SystemService = new SystemService();
        vssp_entity work_entity = new vssp_entity();
        public LoginResultModel UserAuthenticate(LoginModel model)
        {
            LoginResultModel __LoginResultModel = new LoginResultModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UserAuthenticate", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", model.UserID);
                //cmd.Parameters.AddWithValue("@Password", model.Password);
                cmd.Parameters.AddWithValue("@Password", _SystemService.EncryptPass(model.Password));
                cmd.Parameters.AddWithValue("@LogIpAddress", model.LogIpAddress);
                cmd.Parameters.AddWithValue("@LogCountry", model.LogCountry);
                cmd.Parameters.AddWithValue("@LogCity", model.LogCity);
                //int rs = cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    __LoginResultModel.UserID = reader["UserID"].ToString();
                    __LoginResultModel.UserName = reader["UserName"].ToString();
                    __LoginResultModel.UserType = reader["UserTypeID"].ToString();
                    __LoginResultModel.Phone = reader["Phone"].ToString();
                    __LoginResultModel.Organization = reader["OrgName"].ToString();
                    __LoginResultModel.Location = reader["LocationName"].ToString();
                    __LoginResultModel.Email = reader["Email"].ToString();
                    __LoginResultModel.Area = reader["Area"].ToString();
                    __LoginResultModel.DateCreated = reader["DateCreated"].ToString();
                    __LoginResultModel.LogIpAddress = reader["LogIpAddress"].ToString();
                    __LoginResultModel.LogCountry = reader["LogCountry"].ToString();
                    __LoginResultModel.LogCity = reader["LogCity"].ToString();

                    if (reader["Image"].GetType() == typeof(DBNull))
                    {
                        __LoginResultModel.Image = null;
                    }
                    else
                    {
                        __LoginResultModel.Image = (byte[])reader["Image"];
                    }
                    if (reader["Sign"].GetType() == typeof(DBNull))
                    {
                        __LoginResultModel.Sign = null;
                    }
                    else
                    {
                        __LoginResultModel.Sign = (byte[])reader["Sign"];
                    }

                    if (__LoginResultModel.UserType == "DEV")
                    {
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(DBCon);
                        vssp_entity vssp = new vssp_entity();
                        // Retrieve the DataSource property.    
                        //string ServerName = builder.DataSource;
                        //string Database = builder.InitialCatalog;
                        string ServerName = vssp.Database.Connection.DataSource;
                        string Database = vssp.Database.Connection.Database;

                        __LoginResultModel.ServerName = ServerName;
                        __LoginResultModel.DatabaseName = Database;
                    }
                    else
                    {
                        __LoginResultModel.ServerName = "";
                        __LoginResultModel.DatabaseName = "";
                    }

                }
                return __LoginResultModel;
            }
        }

        public IEnumerable<ValidationUserModel> CekUser(string userid, string oldpassword)
        {
            List<ValidationUserModel> _LoginResultModel = new List<ValidationUserModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select CountAccount=Count(UserID) from Tbl_SYS_Users where UserID is not null ";
                    if (userid != null)
                    {
                        MSQL += "and ((UserID='" + userid + "') or (Email='" + userid +"')) ";
                    }
                    if (oldpassword != null)
                    {
                        MSQL += "and Password='" + _SystemService.EncryptPass(oldpassword) + "' ";
                    }
                    cmd.CommandText = MSQL;
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        ValidationUserModel __LoginResultModel = new ValidationUserModel();

                        __LoginResultModel.CountAccount = float.Parse(reader["CountAccount"].ToString());

                        _LoginResultModel.Add(__LoginResultModel);
                    }
                }
            }
            return _LoginResultModel;
        }

        /* --- User --- */
        public List<UserTypeListModel> UserTypeList()
        {
            List<UserTypeListModel> _UserTypeListModel = new List<UserTypeListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("UserTypeList", conn))//call Stored Procedure
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UserTypeListModel __UserTypeListModel = new UserTypeListModel();
                        nom = nom + 1;
                        __UserTypeListModel.No = nom;
                        __UserTypeListModel.ID = reader["ID"].ToString();
                        __UserTypeListModel.UserType = reader["UserType"].ToString();
                        __UserTypeListModel.Remark = reader["Remark"].ToString();
                        __UserTypeListModel.MenuList = reader["MenuList"].ToString();

                        _UserTypeListModel.Add(__UserTypeListModel);
                    }
                }
            }

            return _UserTypeListModel;

        }

        public List<UserTypeEditModel> UserTypeEditList(string id)
        {
            List<UserTypeEditModel> _UserTypeEditListModel = new List<UserTypeEditModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("UserTypeList", conn))//call Stored Procedure
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UserTypeEditModel __UserTypeEditListModel = new UserTypeEditModel();
                        nom = nom + 1;
                        __UserTypeEditListModel.CategoryID = reader["ID"].ToString();
                        __UserTypeEditListModel.UserType = reader["UserType"].ToString();
                        __UserTypeEditListModel.Remark = reader["Remark"].ToString();
                        __UserTypeEditListModel.MenuList = reader["MenuList"].ToString();

                        _UserTypeEditListModel.Add(__UserTypeEditListModel);
                    }
                }
            }

            return _UserTypeEditListModel;

        }

        public int UpdateUserType(UserTypeAddModel entity, Boolean CreateNew, String username)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                string menulist = "";
                if (entity.MenuList != null && entity.MenuList.Length >= 5)
                {
                    menulist = entity.MenuList.Substring(2, (entity.MenuList.Length - 4));
                    menulist = menulist.Replace("\",\"", "','");
                    menulist = "'" + menulist + "'";
                }

                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                MSQL = "Delete From Tbl_SYS_UserType		Where ID	    = '" + entity.CategoryID + "'";
                cmd.CommandText = MSQL;
                cmd.ExecuteNonQuery();

                MSQL = "Delete From Tbl_SYS_MenuPreviliege	Where UserTypeID= '" + entity.CategoryID + "'";
                cmd.CommandText = MSQL;
                cmd.ExecuteNonQuery();

                MSQL = "Insert into Tbl_SYS_UserType Select '" + entity.CategoryID + "','" + entity.UserType + "','" + entity.Remark + "'";
                cmd.CommandText = MSQL;
                cmd.ExecuteNonQuery();

                MSQL = "Insert into Tbl_SYS_MenuPreviliege ";
                MSQL += "Select '" + entity.CategoryID + "',a.MenuID,isnull(b.Active,0),GETDATE() ";
                MSQL += "from Tbl_SYS_Menu a ";
                MSQL += "left join (select MenuID,Active=1 from Tbl_SYS_Menu) b	on a.MenuID=b.MenuID and b.MenuID in ('*'," + menulist + ")";
                cmd.CommandText = MSQL;
                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }

        public int UserTypeDelete(UserTypeListModel model)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UserTypeDelete", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", model.ID);
                int rs = cmd.ExecuteNonQuery();

                return rs;

            }
        }

        /* --- User --- */
        public List<UserListModel> UserList(string utype)
        {
            List<UserListModel> _UserListModel = new List<UserListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("UserGet", conn))//call Stored Procedure
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserType", utype);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    nom = 0;
                    while (reader.Read())
                    {

                        UserListModel __UserListModel = new UserListModel();
                        nom = nom + 1;
                        __UserListModel.No = nom;
                        __UserListModel.UserID = reader["UserID"].ToString();
                        __UserListModel.UserName = reader["UserName"].ToString();
                        __UserListModel.Email = reader["Email"].ToString();
                        __UserListModel.UserTypeID = reader["UserTypeID"].ToString();
                        __UserListModel.Phone = reader["Phone"].ToString();
                        __UserListModel.Area= reader["Area"].ToString();
                        __UserListModel.ConfidentialAccess = _SystemService.Vb(reader["ConfidentialAccess"].ToString());
                        if (reader["Image"] != System.DBNull.Value)
                        {
                            __UserListModel.Image = (byte[])reader["Image"];
                        }
                        else
                        {
                            __UserListModel.Image = null;
                        }
                        if (reader["Sign"] != System.DBNull.Value)
                        {
                            __UserListModel.Sign = (byte[])reader["Sign"];
                        }
                        else
                        {
                            __UserListModel.Sign = null;
                        }
                        __UserListModel.IsActive = bool.Parse(reader["IsActive"].ToString());
                        __UserListModel.IsArchived = bool.Parse(reader["IsArchived"].ToString());
                        __UserListModel.DateCreated = reader["DateCreated"].ToString();

                        _UserListModel.Add(__UserListModel);

                    }
                }
            }

            return _UserListModel;

        }

        //LIST User
        public List<UserEditModel> UserEditList(String id)
        {
            List<UserEditModel> _UserEditModel = new List<UserEditModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("UserGet", conn))//call Stored Procedure
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", id);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UserEditModel __UserEditModel = new UserEditModel();

                        __UserEditModel.UserID = reader["UserID"].ToString();
                        __UserEditModel.UserName = reader["UserName"].ToString();
                        __UserEditModel.Password = _SystemService.DecryptPass(reader["Password"].ToString());
                        __UserEditModel.Email = reader["Email"].ToString();
                        __UserEditModel.UserTypeID = reader["UserTypeID"].ToString();
                        __UserEditModel.Phone = reader["Phone"].ToString();
                        __UserEditModel.Area = reader["Area"].ToString();
                        __UserEditModel.ConfidentialAccess = _SystemService.Vb(reader["ConfidentialAccess"].ToString());
                        if (reader["Image"] != System.DBNull.Value)
                        {
                            __UserEditModel.Image = (byte[])reader["Image"];
                        }
                        else
                        {
                            __UserEditModel.Image = null;
                        }
                        if (reader["Sign"] != System.DBNull.Value)
                        {
                            __UserEditModel.Sign = (byte[])reader["Sign"];
                        }
                        else
                        {
                            __UserEditModel.Sign = null;
                        }
                        _UserEditModel.Add(__UserEditModel);
                    }
                }
            }

            return _UserEditModel;
        }
        public int CreateUser(UserAddModel entity, Boolean CreateNew, string username, string useraccess, string userapproval)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("UserSave", conn);//call Stored Procedure
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", entity.UserID);
                cmd.Parameters.AddWithValue("@Password", _SystemService.EncryptPass(entity.Password));
                cmd.Parameters.AddWithValue("@UserName", entity.UserName);
                cmd.Parameters.AddWithValue("@UserTypeID", entity.UserTypeID);
                cmd.Parameters.AddWithValue("@Email", entity.Email);
                cmd.Parameters.AddWithValue("@Phone", entity.Phone);
                cmd.Parameters.AddWithValue("@Area", entity.Area);
                cmd.Parameters.AddWithValue("@ConfidentialAccess", entity.ConfidentialAccess);
                cmd.Parameters.AddWithValue("@Image", entity.Image);
                cmd.Parameters.AddWithValue("@Sign", entity.Sign);
                cmd.Parameters.AddWithValue("@CreateNew", CreateNew);
                int rs = cmd.ExecuteNonQuery();

                if (useraccess != "")
                {
                    MSQL = "Delete From Tbl_SYS_UserAccess Where UserID='" + entity.UserID + "'";
                    cmd = new SqlCommand(MSQL, conn);
                    cmd.ExecuteNonQuery();

                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(useraccess, (typeof(DataTable)));
                    foreach (DataRow row in dt.Rows)
                    {

                        MSQL = "Insert Into Tbl_SYS_UserAccess Select ";
                        MSQL += "'" + entity.UserID + "',";
                        MSQL += "'" + row["MenuID"].ToString() + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanSee"].ToString()) + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanCreate"].ToString()) + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanUpdate"].ToString()) + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanDelete"].ToString()) + "'";
                        cmd = new SqlCommand(MSQL, conn);
                        cmd.ExecuteNonQuery();

                    }
                }
                if (_SystemService.Vf(userapproval) != "")
                {
                    MSQL = "Delete From Tbl_SYS_UserApproval Where UserID='" + entity.UserID + "'";
                    cmd = new SqlCommand(MSQL, conn);
                    cmd.ExecuteNonQuery();

                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(userapproval, (typeof(DataTable)));
                    foreach (DataRow row in dt.Rows)
                    {

                        MSQL = "Insert Into Tbl_SYS_UserApproval Select ";
                        MSQL += "'" + entity.UserID + "',";
                        MSQL += "'" + row["MenuID"].ToString() + "',";
                        MSQL += "" + row["ApprovalLevel"].ToString() + "";
                        cmd = new SqlCommand(MSQL, conn);
                        cmd.ExecuteNonQuery();

                    }
                }

                return rs;

            }
        }
        //Update User to the database
        public int UpdateUser(UserEditModel entity, Boolean CreateNew, string username, string useraccess, string userapproval)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("UserSave", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", entity.UserID);
                cmd.Parameters.AddWithValue("@Password", _SystemService.EncryptPass(entity.Password));
                cmd.Parameters.AddWithValue("@UserName", entity.UserName);
                cmd.Parameters.AddWithValue("@UserTypeID", entity.UserTypeID);
                cmd.Parameters.AddWithValue("@Email", entity.Email);
                cmd.Parameters.AddWithValue("@Phone", entity.Phone);
                cmd.Parameters.AddWithValue("@Area", entity.Area);
                cmd.Parameters.AddWithValue("@ConfidentialAccess", entity.ConfidentialAccess);
                cmd.Parameters.AddWithValue("@Image", entity.Image);
                cmd.Parameters.AddWithValue("@Sign", entity.Sign);
                cmd.Parameters.AddWithValue("@CreateNew", CreateNew);
                int rs = cmd.ExecuteNonQuery();

                if (_SystemService.Vf(useraccess) != "")
                {
                    MSQL = "Delete From Tbl_SYS_UserAccess Where UserID='" + entity.UserID + "'";
                    cmd = new SqlCommand(MSQL, conn);
                    cmd.ExecuteNonQuery();

                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(useraccess, (typeof(DataTable)));
                    foreach (DataRow row in dt.Rows)
                    {

                        MSQL = "Insert Into Tbl_SYS_UserAccess Select ";
                        MSQL += "'" + entity.UserID + "',";
                        MSQL += "'" + row["MenuID"].ToString() + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanSee"].ToString()) + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanCreate"].ToString()) + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanUpdate"].ToString()) + "',";
                        MSQL += "'" + _SystemService.Vb(row["CanDelete"].ToString()) + "'";
                        cmd = new SqlCommand(MSQL, conn);
                        cmd.ExecuteNonQuery();

                    }
                }

                if (_SystemService.Vf(userapproval) != "")
                {
                    MSQL = "Delete From Tbl_SYS_UserApproval Where UserID='" + entity.UserID + "'";
                    cmd = new SqlCommand(MSQL, conn);
                    cmd.ExecuteNonQuery();

                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(userapproval, (typeof(DataTable)));
                    foreach (DataRow row in dt.Rows)
                    {

                        MSQL = "Insert Into Tbl_SYS_UserApproval Select ";
                        MSQL += "'" + entity.UserID + "',";
                        MSQL += "'" + row["MenuID"].ToString() + "',";
                        MSQL += "" + row["ApprovalLevel"].ToString() + "";
                        cmd = new SqlCommand(MSQL, conn);
                        cmd.ExecuteNonQuery();

                    }
                }

                return rs;

            }
        }
        //Remove User
        public int UserDelete(UserListModel model)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UserDelete", conn);//call Stored Procedure
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", model.UserID);
                cmd.Parameters.AddWithValue("@Action", "delete");
                int rs = cmd.ExecuteNonQuery();

                return rs;

            }
        }

        public int UserArchive(UserListModel model)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                MSQL = "update Tbl_SYS_Users set IsActive=0,IsArchived=1 where UserID='" + model.UserID + "'";
                cmd.CommandText = MSQL;
                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }

        public int UserActivate(UserListModel model)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                MSQL = "update Tbl_SYS_Users set IsActive=1,IsArchived=0 where UserID='" + model.UserID + "'";
                cmd.CommandText = MSQL;
                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }

        /* --- Menu --- */
        public List<MenuModel> UserMenuList(string userCategoryID, string utype, string userid, string menuid = null, Nullable<Boolean> needapproval = false)
        {
            List<MenuModel> _MenuModel = new List<MenuModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("SP_SYS_MenuPreviliege", conn))//call Stored Procedure
                {

                    conn.Open();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserTypeID", userCategoryID);
                    cmd.Parameters.AddWithValue("@UserAccessID", utype);
                    cmd.Parameters.AddWithValue("@UserID", userid);
                    cmd.Parameters.AddWithValue("@MenuID", menuid);
                    cmd.Parameters.AddWithValue("@NeedApproval", needapproval);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MenuModel __MenuModel = new MenuModel();
                        nom = nom + 1;
                        __MenuModel.No              = nom;
                        __MenuModel.MenuID          = reader["MenuID"].ToString();
                        __MenuModel.MenuName        = reader["MenuName"].ToString();
                        __MenuModel.MenuLevel       = reader["MenuLevel"].ToString();
                        __MenuModel.hasChildren     = _SystemService.Vb(reader["hasChildren"].ToString());
                        __MenuModel.NeedApproval    = _SystemService.Vb(reader["NeedApproval"].ToString());
                        __MenuModel.IsActive        = _SystemService.Vb(reader["IsActive"].ToString());
                        __MenuModel.CanSee          = _SystemService.Vb(reader["CanSee"].ToString());
                        __MenuModel.CanCreate       = _SystemService.Vb(reader["CanCreate"].ToString());
                        __MenuModel.CanUpdate       = _SystemService.Vb(reader["CanUpdate"].ToString());
                        __MenuModel.CanDelete       = _SystemService.Vb(reader["CanDelete"].ToString());
                        __MenuModel.ApprovalLevel   = _SystemService.Vn(reader["ApprovalLevel"].ToString());

                        _MenuModel.Add(__MenuModel);
                    }
                }
            }

            return _MenuModel;

        }

        public MenuModel AccessPreviliege(string userid, string controller, string action)
        {
            MenuModel _MenuModel = new MenuModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("SP_SYS_AccessPreviliege", conn))//call Stored Procedure
                {

                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userid);
                    cmd.Parameters.AddWithValue("@Controller", controller);
                    cmd.Parameters.AddWithValue("@Action", action);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MenuModel __MenuModel = new MenuModel();
                        nom = nom + 1;
                        __MenuModel.No                  = nom;
                        __MenuModel.MenuID              = reader["MenuID"].ToString();
                        __MenuModel.MenuName            = reader["MenuName"].ToString();
                        __MenuModel.MenuLevel           = reader["MenuLevel"].ToString();
                        __MenuModel.IconClass           = reader["IconClass"].ToString();
                        __MenuModel.ConfidentialAccess  = _SystemService.Vb(reader["ConfidentialAccess"].ToString());
                        __MenuModel.CanSee              = _SystemService.Vb(reader["CanSee"].ToString());
                        __MenuModel.CanCreate           = _SystemService.Vb(reader["CanCreate"].ToString());
                        __MenuModel.CanUpdate           = _SystemService.Vb(reader["CanUpdate"].ToString());
                        __MenuModel.CanDelete           = _SystemService.Vb(reader["CanDelete"].ToString());
                        __MenuModel.ApprovalLevel       = _SystemService.Vn(reader["ApprovalLevel"].ToString());
                        __MenuModel.ApprovalName        = reader["ApprovalName"].ToString();

                        _MenuModel = __MenuModel;
                    }
                }
            }

            return _MenuModel;

        }
        public List<ApprovalTypeListModel> ApprovalType()
        {

            List<ApprovalTypeListModel> _ApprovalTypeListModel = new List<ApprovalTypeListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                MSQL = "select * From Tbl_SYS_MenuApprovalType Order By ApprovalLevel ";

                cmd.CommandText = MSQL;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ApprovalTypeListModel __ApprovalTypeListModel = new ApprovalTypeListModel();
                    __ApprovalTypeListModel.ApprovalLevel = _SystemService.Vn(reader["ApprovalLevel"].ToString());
                    __ApprovalTypeListModel.ApprovalName = reader["ApprovalName"].ToString();

                    _ApprovalTypeListModel.Add(__ApprovalTypeListModel);
                }
            }

            return _ApprovalTypeListModel;
        }
        public List<UserApprovalListModel> UserApprovalType(string userid, string menuid)
        {

            List<UserApprovalListModel> _UserApprovalListModel = new List<UserApprovalListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                /* UPDATE   : 2021/07/10
                 * ERROR    : ALL USER LEVEL AS CREATED GET IN
                 * MODIFIED : (ApprovalLevel not in (0,1) or UserID='" + userid + "') 
                 */

                MSQL = "select * From Vw_SYS_UserApproval where (ApprovalLevel not in (0,1) or UserID='" + userid + "') ";
                //if (_SystemService.Vf(userid) != "")
                //{
                //    MSQL += "and UserID='" + userid + "' ";
                //}
                if (_SystemService.Vf(menuid) != "")
                {
                    MSQL += "and MenuId='" + menuid + "' ";
                }
                MSQL += "Order By MenuId";
                cmd.CommandText = MSQL;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UserApprovalListModel __UserApprovalListModel = new UserApprovalListModel();
                    __UserApprovalListModel.MenuID = reader["MenuID"].ToString();
                    __UserApprovalListModel.MenuName = reader["MenuName"].ToString();
                    __UserApprovalListModel.UserID = reader["UserID"].ToString();
                    __UserApprovalListModel.UserName = reader["UserName"].ToString();
                    __UserApprovalListModel.Email = reader["Email"].ToString();
                    __UserApprovalListModel.ApprovalLevel = int.Parse(reader["ApprovalLevel"].ToString());
                    __UserApprovalListModel.ApprovalName = reader["ApprovalName"].ToString();
                    if (reader["Sign"].GetType() == typeof(DBNull))
                    {
                        __UserApprovalListModel.Sign = null;
                    }
                    else
                    {
                        __UserApprovalListModel.Sign = (byte[])reader["Sign"];
                    }

                    _UserApprovalListModel.Add(__UserApprovalListModel);
                }
            }
            
            return _UserApprovalListModel;

        }
        public int CreateAccount(
                string UserID,
                string Password,
                string UserName,
                string UserRole,
                string Email,
                Boolean CreateNew)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                int rs = 0;

                if (UserID != "")
                {

                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UserSave", conn);//call Stored Procedure
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    cmd.Parameters.AddWithValue("@Password", _SystemService.EncryptPass(Password));
                    cmd.Parameters.AddWithValue("@UserName", UserName);
                    cmd.Parameters.AddWithValue("@UserTypeID", UserRole);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    //cmd.Parameters.AddWithValue("@Phone", Phone);
                    //cmd.Parameters.AddWithValue("@Area", Area);
                    //cmd.Parameters.AddWithValue("@Image", Image);
                    cmd.Parameters.AddWithValue("@CreateNew", CreateNew);

                    rs = cmd.ExecuteNonQuery();

                }
                else
                {
                    rs = 0;
                }

                return rs;

            }
        }

        public int SaveUserRole(string UserID, string MenuID, string IsShow, string IsAdd, string IsEdit, string IsDelete)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                int rs = 0;

                if ((UserID != "") || (MenuID != ""))
                {

                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UserRoleCreate", conn);//call Stored Procedure
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    cmd.Parameters.AddWithValue("@MenuID", MenuID);
                    cmd.Parameters.AddWithValue("@IsShow", IsShow);
                    cmd.Parameters.AddWithValue("@IsAdd", IsAdd);
                    cmd.Parameters.AddWithValue("@IsEdit", IsEdit);
                    cmd.Parameters.AddWithValue("@IsDelete", IsDelete);
                    rs = cmd.ExecuteNonQuery();

                }
                else
                {
                    rs = 0;
                }

                return rs;

            }
        }

        /* --- UserType --- */
                public IEnumerable<UserTypeListModel> ComboUserType(string utype)
        {
            List<UserTypeListModel> _UserTypeModel = new List<UserTypeListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("UserTypeList", conn))//call Stored Procedure
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserType", utype);
                    conn.Open();
                    loop = 0;

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        reloop:
                        loop = loop + 1;

                        UserTypeListModel __UserTypeModel = new UserTypeListModel();

                        if (loop == 1)
                        {
                            __UserTypeModel.ID = "";
                            __UserTypeModel.UserType = "-- User Type --";

                            _UserTypeModel.Add(__UserTypeModel);
                            goto reloop;

                        }

                        __UserTypeModel.ID = reader["ID"].ToString();
                        __UserTypeModel.UserType = reader["UserType"].ToString();

                        _UserTypeModel.Add(__UserTypeModel);
                    }
                }
            }
            return _UserTypeModel;
        }
        /* --- Area --- */
        public IEnumerable<AreaListModel> ComboArea()
        {
            List<AreaListModel> _AreaModel = new List<AreaListModel>();
            
            var AreaList = from a in work_entity.Vw_MST_AreaLine
                           orderby a.AreaID
                           select new { a.AreaID, a.AreaName };

            loop = 0;
            foreach(var area in AreaList)
            {
                reloop:
                loop = loop + 1;

                AreaListModel __AreaModel = new AreaListModel();

                if (loop == 1)
                {
                    __AreaModel.AreaID = "";
                    __AreaModel.AreaName = "-- Area --";

                    _AreaModel.Add(__AreaModel);
                    goto reloop;

                }


                if (loop == 2)
                {
                    __AreaModel.AreaID = "ALL";
                    __AreaModel.AreaName = "All Area";

                    _AreaModel.Add(__AreaModel);
                    goto reloop;

                }

                __AreaModel.AreaID      = area.AreaID;
                __AreaModel.AreaName    = area.AreaName;

                _AreaModel.Add(__AreaModel);
            }

            return _AreaModel;
        }
        public IEnumerable<UserResetModel> GetUserResetRequest(string token)
        {
            List<UserResetModel> _UserResetModel = new List<UserResetModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("UserResetGet", conn))//call Stored Procedure
                {
                    conn.Open();

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", token);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        UserResetModel __UserResetModel = new UserResetModel();

                        __UserResetModel.Token          = reader["Token"].ToString();
                        __UserResetModel.RequestTime    = _SystemService.Vd(reader["RequestTime"].ToString(),"yyyy-MM-dd HH:mm:ss");
                        __UserResetModel.ExpiredTime    = _SystemService.Vd(reader["ExpiredTime"].ToString(), "yyyy-MM-dd HH:mm:ss");
                        __UserResetModel.EmailAddress   = reader["EmailAddress"].ToString();
                        __UserResetModel.LifeTime       = _SystemService.Vn(reader["LifeTime"].ToString());
                        __UserResetModel.Taken          = _SystemService.Vd(reader["Taken"].ToString(), "yyyy-MM-dd HH:mm:ss");
                        __UserResetModel.Valid          =  bool.Parse(reader["Valid"].ToString());
                        __UserResetModel.Expire         = bool.Parse(reader["Expire"].ToString());

                        _UserResetModel.Add(__UserResetModel);
                    }
                }
            }
            return _UserResetModel;
        }

        public bool crudResetPassword(
                string Token,
                string Email,
                string Password,
                float LifeTime,
                bool Valid,
                string FormAction)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                bool rs = false;

                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UserReset", conn);//call Stored Procedure
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", Token);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Password", _SystemService.EncryptPass(Password));
                    cmd.Parameters.AddWithValue("@LifeTime",LifeTime);
                    cmd.Parameters.AddWithValue("@Valid", Valid);
                    cmd.Parameters.AddWithValue("@FormAction", FormAction);

                    cmd.ExecuteNonQuery();

                    rs = true;
                }
                catch(Exception e){
                    rs = false;
                }

                return rs;

            }
        }
    }
}