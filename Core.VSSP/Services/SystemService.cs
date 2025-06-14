using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Core.VSSP.Models;
using System.Configuration;
using System.Data.SqlClient;
using Core.VSSP.WorkEntity;
using System.Linq.Expressions;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Drawing;

namespace Core.VSSP.Services
{
    public class SystemService
    {

        //Db Connection string
        string DBCon = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
        string DBMaster = ConfigurationManager.ConnectionStrings["DBMaster"].ConnectionString;
        string MSQL = "";
        int nom = 0;
        int loop = 0;

        vssp_entity workentity = new vssp_entity();

        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            if (image != null)
            {
                byte[] imageBytes = null;
                BinaryReader reader = new BinaryReader(image.InputStream);
                imageBytes = reader.ReadBytes((int)image.ContentLength);
                if (imageBytes.Length == 0)
                {
                    imageBytes = null;
                }

                return imageBytes;

            }
            else
            {
                return null;
            }
        }
        public string ConvertToBase64(Stream stream)
        {
            var bytes = new Byte[(int)stream.Length];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);

            return Convert.ToBase64String(bytes);
        }
        public string EncryptPass(string passText)
        {

            if (passText != null)
            {

                string EncryptionKey = "MAKV2SPBNI99212";

                string sha256 = Sha256Hash(passText, "encrypted");

                byte[] clearBytes = Encoding.Unicode.GetBytes(passText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        passText = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            return passText;

        }

        public string DecryptPass(string passText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            passText = passText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(passText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    passText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return passText;
        }

        public string Sha256Hash(string Password, string Type)
        {

            string hashkey = "P@ssw0rd#17510";

            // Create sha256 hash
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(hashkey));
            byte[] iv = new byte[16] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

            if (Type == "encrypted")
            {
                return this.EncryptString(Password, key, iv);
            }
            else
            {
                return this.DecryptString(Password, key, iv);
            }
        }
        public string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            // Set key and IV
            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

            // Convert the plainText string into a byte array
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);

            // Encrypt the input plaintext string
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            // Complete the encryption process
            cryptoStream.FlushFinalBlock();

            // Convert the encrypted data from a MemoryStream to a byte array
            byte[] cipherBytes = memoryStream.ToArray();

            // Close both the MemoryStream and the CryptoStream
            memoryStream.Close();
            cryptoStream.Close();

            // Convert the encrypted byte array to a base64 encoded string
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

            // Return the encrypted data as a string
            return cipherText;
        }

        public string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            // Set key and IV
            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the decrypted byte array to string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }

            // Return the decrypted data as a string
            return plainText;
        }
        public string Vf(string Value, bool crud = false)
        {
            string StrVal = "";

            if (Value == null)
            {
                StrVal = "";
            }
            else
            {
                StrVal = Value.Trim();
            }
            if (crud == true)
            {
                StrVal = StrVal.Replace("'", "''");
            }
            return StrVal;
        }


        public bool Vb(string Value)
        {
            string boolVal = "";

            if (Value == null || Value == "")
            {
                boolVal = "false";
            }
            else
            {
                if (Value == "0" || Value == "false")
                {
                    boolVal = "false";
                }
                else
                if (Value == "1" || Value == "true")
                {
                    boolVal = "true";
                }
                else
                {
                    boolVal = Value;
                }
            }

            return bool.Parse(boolVal);
        }
        public int Vbn(string Value)
        {
            string boolVal = "";

            if (Value == null || Value == "")
            {
                boolVal = "false";
            }
            else
            {
                if (Value == "0" || Value.ToLower() == "false")
                {
                    boolVal = "0";
                }
                else
                if (Value == "1" || Value.ToLower() == "true")
                {
                    boolVal = "1";
                }
                else
                {
                    boolVal = Value;
                }
            }

            return int.Parse(boolVal);
        }
        public string Vc(string Value)
        {
            string StrVal = "";

            if (Value == null)
            {
                StrVal = "";
            }
            else
            {
                if (Value.Contains(':'))
                {
                    StrVal = Value.Substring(Value.LastIndexOf(':'));
                    StrVal = this.Vf(Value.Replace(StrVal, ""));
                }
                else
                {
                    StrVal = Value;
                }
            }
            return StrVal;
        }
        public double Vn(string Value)
        {
            double StrVal = 0;

            if (Value == null)
            {
                StrVal = 0;
            }
            else
            {
                try
                {
                    StrVal = double.Parse(Value);
                }
                catch
                {
                    StrVal = 0;
                }
            }
            return StrVal;
        }

        public string Vd(string Value, string format = "dd-MMM-yyyy")
        {
            string StrVal = "";

            if (this.Vf(Value) == "")
            {
                StrVal = "";
            }
            else
            {
                try
                {
                    StrVal = DateTime.Parse(Value).ToString(format);
                }
                catch
                {
                    double NumVal = double.Parse(Value);
                    StrVal = DateTime.FromOADate(NumVal).ToString(format);
                }
            }
            return StrVal;
        }
        public string Vt(string Value)
        {
            string StrVal = "";

            if (Value == null)
            {
                StrVal = "";
            }
            else
            {
                try
                {
                    StrVal = DateTime.Parse(Value).ToString("hh:mm:ss");
                }
                catch
                {
                    StrVal = "";
                }
            }
            return StrVal;
        }
        /* --- Sidebar --- */
        public List<SidebarListModel> SidebarList(string UserType, string parrent = "")
        {
            List<SidebarListModel> _SidebarListModel = new List<SidebarListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    if (UserType != "")
                    {
                        MSQL = "Select a.* from Tbl_SYS_Menu a ";
                        MSQL += "inner join Tbl_SYS_MenuPreviliege b on a.MenuID = b.MenuID and b.IsActive = 1 ";
                        MSQL += "and b.UserTypeID = '" + UserType + "' ";
                        MSQL += "Where Active=1 Order By a.MenuID";
                    }
                    else
                    {
                        MSQL = "Select * from Tbl_SYS_Menu Where MenuID is not null";
                        if (parrent != "")
                        {
                            MSQL += " and ParrentID = '" + parrent + "'";
                        }
                        MSQL += " Order By MenuID";
                    }
                    cmd.CommandText = MSQL;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SidebarListModel __SidebarListModel = new SidebarListModel();
                        nom = nom + 1;
                        __SidebarListModel.No = nom;
                        __SidebarListModel.MenuID = reader["MenuID"].ToString();
                        __SidebarListModel.MenuName = reader["MenuName"].ToString();
                        __SidebarListModel.MenuLevel = reader["MenuLevel"].ToString();
                        __SidebarListModel.ParrentID = reader["ParrentID"].ToString();
                        __SidebarListModel.IconClass = reader["IconClass"].ToString();
                        __SidebarListModel.ControllerName = reader["ControllerName"].ToString();
                        __SidebarListModel.ActionName = reader["ActionName"].ToString();
                        __SidebarListModel.NeedApproval = this.Vb(reader["NeedApproval"].ToString());
                        __SidebarListModel.Confidential = this.Vb(reader["Confidential"].ToString());
                        __SidebarListModel.Active = this.Vb(reader["Active"].ToString());

                        _SidebarListModel.Add(__SidebarListModel);
                    }
                }
            }

            return _SidebarListModel;

        }

        //LIST Sidebar
        public List<SidebarEditModel> SidebarEditList(string MenuID)
        {
            List<SidebarEditModel> _SidebarEditModel = new List<SidebarEditModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    cmd.CommandText = "select * from Tbl_SYS_Menu Where MenuID='" + MenuID + "'";

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SidebarEditModel __SidebarEditModel = new SidebarEditModel();

                        __SidebarEditModel.MenuID = reader["MenuID"].ToString();
                        __SidebarEditModel.MenuName = reader["MenuName"].ToString();
                        __SidebarEditModel.MenuLevel = reader["MenuLevel"].ToString();
                        __SidebarEditModel.ParrentID = reader["ParrentID"].ToString();
                        __SidebarEditModel.IconClass = reader["IconClass"].ToString();
                        __SidebarEditModel.ControllerName = reader["ControllerName"].ToString();
                        __SidebarEditModel.ActionName = reader["ActionName"].ToString();
                        __SidebarEditModel.NeedApproval = this.Vb(reader["NeedApproval"].ToString());
                        __SidebarEditModel.Confidential = this.Vb(reader["Confidential"].ToString());
                        __SidebarEditModel.Active = this.Vb(reader["Active"].ToString());

                        _SidebarEditModel.Add(__SidebarEditModel);
                    }
                }
            }

            return _SidebarEditModel;
        }

        //Update Sidebar to the database
        public int SaveSidebar(HttpPostedFileBase file, SidebarEditModel entity, Boolean CreateNew, String username)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                int IsActive = 0;
                int NeedApproval = 0;
                if (entity.NeedApproval == true)
                {
                    NeedApproval = 1;
                }
                if (entity.Active == true)
                {
                    IsActive = 1;
                }

                if (CreateNew == true)
                {
                    MSQL = "INSERT INTO Tbl_SYS_Menu SELECT ";
                    MSQL += "'" + entity.MenuID + "',";
                    MSQL += "'" + entity.MenuName + "',";
                    MSQL += "'" + entity.MenuLevel + "',";
                    MSQL += "'" + entity.ParrentID + "',";
                    MSQL += "'" + entity.IconClass + "',";
                    MSQL += "'" + entity.ControllerName + "',";
                    MSQL += "'" + entity.ActionName + "',";
                    MSQL += "" + NeedApproval + ",";
                    MSQL += "1 ";

                }
                else
                {

                    MSQL = "UPDATE Tbl_SYS_Menu SET ";
                    MSQL += "ParrentID          ='" + entity.ParrentID + "',";
                    MSQL += "MenuName           ='" + entity.MenuName + "',";
                    MSQL += "MenuLevel          ='" + entity.MenuLevel + "',";
                    MSQL += "IconClass          ='" + entity.IconClass + "',";
                    MSQL += "ControllerName     ='" + entity.ControllerName + "',";
                    MSQL += "ActionName         ='" + entity.ActionName + "',";
                    MSQL += "NeedApproval       =" + NeedApproval + ",";
                    MSQL += "Active             =" + IsActive + " ";
                    MSQL += "Where MenuID       ='" + entity.MenuID + "'";
                }

                cmd.CommandText = MSQL;

                int rs = cmd.ExecuteNonQuery();

                return rs;

            }
        }

        //Remove Sidebar
        public int SidebarDelete(SidebarListModel model)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);//call Stored Procedure

                MSQL = "DELETE FROM Tbl_SYS_Menu ";
                MSQL += "Where MenuID       ='" + model.MenuID + "'";

                cmd.CommandText = MSQL;
                int rs = cmd.ExecuteNonQuery();

                return rs;

            }
        }

        /* --- Menu Parent --- */
        public IEnumerable<MenuParentModel> ComboMenuParent(string Level)
        {
            List<MenuParentModel> _MenuParentModel = new List<MenuParentModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select * from Tbl_SYS_Menu ";
                    MSQL += "Where Active=1 and MenuLevel = '" + (this.Vn(Level) - 1) + "' ";
                    MSQL += "order by MenuLevel,MenuID ";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    loop = 0;

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        reloop:
                        loop = loop + 1;

                        MenuParentModel __MenuParentModel = new MenuParentModel();

                        if (loop == 1)
                        {
                            __MenuParentModel.MenuID = "";
                            __MenuParentModel.MenuName = "--PARENT--";
                            __MenuParentModel.MenuLevel = "0";

                            _MenuParentModel.Add(__MenuParentModel);
                            goto reloop;
                        }

                        __MenuParentModel.MenuID = reader["MenuID"].ToString();
                        __MenuParentModel.MenuName = reader["MenuName"].ToString();
                        __MenuParentModel.MenuLevel = reader["MenuLevel"].ToString();

                        _MenuParentModel.Add(__MenuParentModel);
                    }

                    if (_MenuParentModel.Count == 0)
                    {
                        MenuParentModel __MenuParentModel = new MenuParentModel();

                        __MenuParentModel.MenuID = "";
                        __MenuParentModel.MenuName = "--PARENT--";
                        __MenuParentModel.MenuLevel = "0";

                        _MenuParentModel.Add(__MenuParentModel);
                    }
                }
            }

            return _MenuParentModel;
        }

        public IEnumerable<ClassIconModel> ComboClassIcon()
        {
            List<ClassIconModel> _ClassIconModel = new List<ClassIconModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select * from Tbl_SYS_IconClass ";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    loop = 0;

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        reloop:
                        loop = loop + 1;

                        ClassIconModel __ClassIconModel = new ClassIconModel();

                        if (loop == 1)
                        {
                            __ClassIconModel.IconID = "X";
                            __ClassIconModel.IconName = "--ICON--";

                            _ClassIconModel.Add(__ClassIconModel);
                            goto reloop;
                        }

                        __ClassIconModel.IconID = reader["IconName"].ToString();
                        //__ClassIconModel.IconName       = "&#x" + reader["Iconunicode"].ToString() + "; &nbsp; " + reader["IconName"].ToString();
                        __ClassIconModel.IconName = " &nbsp; " + reader["IconName"].ToString();

                        string iname = reader["IconName"].ToString();
                        int lname = reader["IconName"].ToString().Length;

                        __ClassIconModel.IconSortName = iname.Substring(3, lname - 3);

                        _ClassIconModel.Add(__ClassIconModel);
                    }
                }
            }
            return _ClassIconModel;
        }

        public IEnumerable<TahunListModel> ComboTahun()
        {
            List<TahunListModel> _TahunListModel = new List<TahunListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select * from Tbl_SYS_DateYear Order By Tahun Desc ";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    loop = 0;

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        reloop:
                        loop = loop + 1;

                        TahunListModel __TahunListModel = new TahunListModel();

                        if (loop == 1)
                        {
                            __TahunListModel.TahunID = "";
                            __TahunListModel.TahunName = "--YEAR--";

                            _TahunListModel.Add(__TahunListModel);
                            goto reloop;
                        }

                        __TahunListModel.TahunID = reader["Tahun"].ToString();
                        __TahunListModel.TahunName = reader["Tahun"].ToString();

                        _TahunListModel.Add(__TahunListModel);
                    }
                }
            }
            return _TahunListModel;
        }

        public IEnumerable<BulanListModel> ComboBulan()
        {
            List<BulanListModel> _BulanListModel = new List<BulanListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select * from Tbl_SYS_DateMonth Order By MonthID";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    loop = 0;

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        reloop:
                        loop = loop + 1;

                        BulanListModel __BulanListModel = new BulanListModel();

                        if (loop == 1)
                        {
                            __BulanListModel.MonthID = "";
                            __BulanListModel.MonthName = "--MONTH--";

                            _BulanListModel.Add(__BulanListModel);
                            goto reloop;
                        }

                        __BulanListModel.MonthID = reader["MonthID"].ToString();
                        __BulanListModel.MonthName = reader["MonthID"].ToString() + " : " + reader["MonthName"].ToString();

                        _BulanListModel.Add(__BulanListModel);
                    }
                }
            }
            return _BulanListModel;
        }

        /* --- Menu Level --- */
        public IEnumerable<MenuLevelModel> ComboMenuLevel()
        {
            List<MenuLevelModel> _MenuLevelModel = new List<MenuLevelModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select * from Tbl_SYS_MenuLevel order by LevelID ";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    loop = 0;

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        reloop:
                        loop = loop + 1;

                        MenuLevelModel __MenuLevelModel = new MenuLevelModel();

                        if (loop == 1)
                        {
                            __MenuLevelModel.LevelID = "";
                            __MenuLevelModel.LevelName = "--LEVEL--";

                            _MenuLevelModel.Add(__MenuLevelModel);
                            goto reloop;
                        }

                        __MenuLevelModel.LevelID = reader["LevelID"].ToString();
                        __MenuLevelModel.LevelName = reader["LevelName"].ToString();

                        _MenuLevelModel.Add(__MenuLevelModel);
                    }
                }
            }
            return _MenuLevelModel;
        }

        /* --- Email Configuration --- */
        public EmailConfigurationModel GetEmailConfiguration(string AccountName)
        {
            EmailConfigurationModel _EmailConfigurationModel = new EmailConfigurationModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select Top 1 * from Tbl_SYS_EmailConfiguration Where AccountName Is Not NULL";
                    if (string.IsNullOrWhiteSpace(AccountName) == false)
                    {
                        MSQL += " and AccountName='" + AccountName + "'";
                    }
                    cmd.CommandText = MSQL;
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        _EmailConfigurationModel.AccountName = reader["AccountName"].ToString();
                        _EmailConfigurationModel.SenderName = reader["SenderName"].ToString();
                        _EmailConfigurationModel.EmailAddress = reader["EmailAddress"].ToString();
                        _EmailConfigurationModel.EmailUserID = reader["EmailUserID"].ToString();
                        _EmailConfigurationModel.EmailPassword = reader["EmailPassword"].ToString();
                        _EmailConfigurationModel.OutgoingServer = reader["OutgoingServer"].ToString();
                        _EmailConfigurationModel.OutgoingPort = int.Parse(reader["OutgoingPort"].ToString());
                        _EmailConfigurationModel.EnableSSL = Boolean.Parse(reader["EnableSSL"].ToString());

                    }
                }
            }
            return _EmailConfigurationModel;
        }
        public int EmailConfigurationCRUD(EmailConfigurationModel entity, string userid)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_CRUD_EmailConfiguration", conn);//call Stored Procedure

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AccountName", entity.AccountName);
                cmd.Parameters.AddWithValue("@SenderName", entity.SenderName);
                cmd.Parameters.AddWithValue("@EmailAddress", entity.EmailAddress);
                cmd.Parameters.AddWithValue("@EmailUserID", entity.EmailUserID);
                cmd.Parameters.AddWithValue("@EmailPassword", entity.EmailPassword);
                cmd.Parameters.AddWithValue("@OutgoingServer", entity.OutgoingServer);
                cmd.Parameters.AddWithValue("@OutgoingPort", entity.OutgoingPort);
                cmd.Parameters.AddWithValue("@EnableSSL", entity.EnableSSL);
                cmd.Parameters.AddWithValue("@UserID", userid);

                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }

        public CompanyLicenseListModel GetLicenseInfo()
        {
            CompanyLicenseListModel _CompanyLicenseListModel = new CompanyLicenseListModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select Top 1 * from Vw_SYS_CompanyLicense Order By LicenseEnd Desc";

                    cmd.CommandText = MSQL;
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        _CompanyLicenseListModel.FormAction = "";
                        _CompanyLicenseListModel.ID = reader["ID"].ToString();
                        _CompanyLicenseListModel.Name = reader["Name"].ToString();
                        _CompanyLicenseListModel.Title = reader["Title"].ToString();
                        _CompanyLicenseListModel.TaxId = reader["TaxId"].ToString();
                        _CompanyLicenseListModel.Address = reader["Address"].ToString();
                        _CompanyLicenseListModel.City = reader["City"].ToString();
                        _CompanyLicenseListModel.Provience = reader["Provience"].ToString();
                        _CompanyLicenseListModel.Country = reader["Country"].ToString();
                        _CompanyLicenseListModel.ZipCode = reader["ZipCode"].ToString();
                        _CompanyLicenseListModel.Phone1 = reader["Phone1"].ToString();
                        _CompanyLicenseListModel.Phone2 = reader["Phone2"].ToString();
                        _CompanyLicenseListModel.Fax = reader["Fax"].ToString();
                        _CompanyLicenseListModel.Email1 = reader["Email1"].ToString();
                        _CompanyLicenseListModel.Email2 = reader["Email2"].ToString();
                        _CompanyLicenseListModel.Websites = reader["Websites"].ToString();
                        _CompanyLicenseListModel.LicenseNumber = reader["LicenseNumber"].ToString();
                        _CompanyLicenseListModel.LicenseStart = reader["LicenseStart"].ToString();
                        _CompanyLicenseListModel.LicenseEnd = reader["LicenseEnd"].ToString();
                        _CompanyLicenseListModel.LicenseStatus = reader["LicenseStatus"].ToString();
                        _CompanyLicenseListModel.LicenseDay = reader["LicenseDay"].ToString();
                        _CompanyLicenseListModel.RemainDay = reader["RemainDay"].ToString();
                        _CompanyLicenseListModel.LicenseCertificate = reader["Certificate"].ToString();

                        if (reader["Logo"].GetType() == typeof(DBNull))
                        {
                            _CompanyLicenseListModel.Logo = null;
                        }
                        else
                        {
                            _CompanyLicenseListModel.Logo = (byte[])reader["Logo"];
                        }

                        if (reader["LogoSmall"].GetType() == typeof(DBNull))
                        {
                            _CompanyLicenseListModel.LogoSmall = null;
                        }
                        else
                        {
                            _CompanyLicenseListModel.LogoSmall = (byte[])reader["LogoSmall"];
                        }

                    }
                }
            }
            return _CompanyLicenseListModel;
        }

        public int CompanyLicenseCRUD(CompanyLicenseModel entity, string userid)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_CRUD_CompanyLicense", conn);//call Stored Procedure

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", entity.ID);
                cmd.Parameters.AddWithValue("@Name", entity.Name);
                cmd.Parameters.AddWithValue("@Title", entity.Title);
                cmd.Parameters.AddWithValue("@TaxId", entity.TaxId);
                cmd.Parameters.AddWithValue("@Address", entity.Address);
                cmd.Parameters.AddWithValue("@City", entity.City);
                cmd.Parameters.AddWithValue("@Provience", entity.Provience);
                cmd.Parameters.AddWithValue("@Country", entity.Country);
                cmd.Parameters.AddWithValue("@ZipCode", entity.ZipCode);
                cmd.Parameters.AddWithValue("@Phone1", entity.Phone1);
                cmd.Parameters.AddWithValue("@Phone2", entity.Phone2);
                cmd.Parameters.AddWithValue("@Fax", entity.Fax);
                cmd.Parameters.AddWithValue("@Email1", entity.Email1);
                cmd.Parameters.AddWithValue("@Email2", entity.Email2);
                cmd.Parameters.AddWithValue("@Websites", entity.Websites);
                cmd.Parameters.AddWithValue("@Logo", entity.Logo);
                cmd.Parameters.AddWithValue("@LogoSmall", entity.LogoSmall);
                cmd.Parameters.AddWithValue("@UserID", userid);
                cmd.Parameters.AddWithValue("@Action", entity.FormAction);

                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }
        
        public AppVersionModel GetAppVersion()
        {
            AppVersionModel _AppVersionModel = new AppVersionModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select * from Tbl_SYS_AppVersion";

                    cmd.CommandText = MSQL;
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        _AppVersionModel.AppID = reader["AppID"].ToString();
                        _AppVersionModel.AppName = reader["AppName"].ToString();
                        _AppVersionModel.AppVersion = reader["AppVersion"].ToString();
                        _AppVersionModel.AppRevision = reader["AppRevision"].ToString();
                        _AppVersionModel.AppBuild = reader["AppBuild"].ToString();
                        _AppVersionModel.AppCompany = reader["AppCompany"].ToString();
                        _AppVersionModel.AppDescription = reader["AppDescription"].ToString();
                        _AppVersionModel.AppWebsite = reader["AppWebsite"].ToString();
                        if (reader["AppLogo"].GetType() == typeof(DBNull))
                        {
                            _AppVersionModel.AppLogo = null;
                        }
                        else
                        {
                            _AppVersionModel.AppLogo = (byte[])reader["AppLogo"];
                        }
                    }
                }
            }
            return _AppVersionModel;
        }

        public string UpdateAppVersion(AppVersionModel entity)
        {

            string rs = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(DBCon))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SP_CRUD_AppVersion", conn);//call Stored Procedure

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppID", this.Vf(entity.AppID));
                    cmd.Parameters.AddWithValue("@AppName", this.Vf(entity.AppName));
                    cmd.Parameters.AddWithValue("@AppVersion", this.Vf(entity.AppVersion));
                    cmd.Parameters.AddWithValue("@AppRevision", this.Vf(entity.AppRevision));
                    cmd.Parameters.AddWithValue("@AppBuild", this.Vf(entity.AppBuild));
                    cmd.Parameters.AddWithValue("@AppCompany", this.Vf(entity.AppCompany));
                    cmd.Parameters.AddWithValue("@AppWebsite", this.Vf(entity.AppWebsite));
                    cmd.Parameters.AddWithValue("@AppDescription", this.Vf(entity.AppDescription));
                    cmd.Parameters.AddWithValue("@AppLogo", entity.AppLogo);

                    cmd.ExecuteNonQuery();

                    rs = "Success";
                }
            }
            catch (Exception e)
            {
                var errinfo = GetExceptionDetails(e);
                rs = errinfo;
            }
            return rs;
        }

        public int LogActivitiesCRUD(string userid, string ipaddress, string country, string city, string module, string activities, string remark)
        {
            using (SqlConnection conn = new SqlConnection(DBCon))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_CRUD_LogActivities", conn);//call Stored Procedure

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userid);
                cmd.Parameters.AddWithValue("@LogIpAddress", ipaddress);
                cmd.Parameters.AddWithValue("@LogCountry", country);
                cmd.Parameters.AddWithValue("@LogCity", city);
                cmd.Parameters.AddWithValue("@LogModule", module);
                cmd.Parameters.AddWithValue("@LogActivities", activities);
                cmd.Parameters.AddWithValue("@LogRemark", remark);

                int rs = cmd.ExecuteNonQuery();
                return rs;

            }
        }

        /* --- Export Option --- */
        public IEnumerable<ExportListModel> ComboExport(bool WithPrint = false)
        {
            List<ExportListModel> _ExportListModel = new List<ExportListModel>();

            loop = 0;

            while (loop < 2)
            {
                loop = loop + 1;
                ExportListModel __ExportListModel = new ExportListModel();

                if (loop == 1)
                {
                    /* PDF */
                    __ExportListModel.ExportID = "pdf";
                    __ExportListModel.ExportName = "PDF File";

                }
                else
                {
                    /* EXCEL */
                    __ExportListModel.ExportID = "xls";
                    __ExportListModel.ExportName = "Excel File";

                }

                _ExportListModel.Add(__ExportListModel);
            }

            if (WithPrint == true)
            {
                ExportListModel __ExportListModel = new ExportListModel();

                /* PRINTER */
                __ExportListModel.ExportID = "printer";
                __ExportListModel.ExportName = "Default Printer";
                _ExportListModel.Add(__ExportListModel);
            }

            /* RETURN */
            return _ExportListModel;
        }        
        public bool CrudNotification(
            string userid, string category, string sender,
            string subject, string messagehtml, string urllink)
        {

            bool result = false;
            int exec = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(DBCon))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_CRUD_Notification", conn))//call Stored Procedure
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userid);
                        cmd.Parameters.AddWithValue("@Category", category);
                        cmd.Parameters.AddWithValue("@Sender", sender);
                        cmd.Parameters.AddWithValue("@Subject", subject);
                        cmd.Parameters.AddWithValue("@MessageHtml", messagehtml);
                        cmd.Parameters.AddWithValue("@UrlLink", urllink);
                        cmd.Parameters.AddWithValue("@FormAction", "create");

                        conn.Open();
                        exec = cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception e)
            {
                var errinfo = GetExceptionDetails(e);
                Console.WriteLine(errinfo);
            }

            if (exec != 0)
            {
                result = true;
            }
            return result;

        }
        public NotificationTotalModel NotificationTotal(string userid)
        {
            NotificationTotalModel _NotificationTotalModel = new NotificationTotalModel();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("SP_SYS_NotificationSummary", conn))//call Stored Procedure
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userid);
                    cmd.Parameters.AddWithValue("@FormAction", "total");

                    cmd.CommandTimeout = 120;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        _NotificationTotalModel.UserID = reader["UserID"].ToString();
                        _NotificationTotalModel.CountCategory = Vn(reader["CountCategory"].ToString());

                    }
                }
            }

            return _NotificationTotalModel;

        }
        public List<NotificationSubTotalModel> NotificationSubTotal(string userid)
        {
            List<NotificationSubTotalModel> _NotificationSubTotalModel = new List<NotificationSubTotalModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("SP_SYS_NotificationSummary", conn))//call Stored Procedure
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userid);
                    cmd.Parameters.AddWithValue("@FormAction", "subtotal");
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        NotificationSubTotalModel __NotificationSubTotalModel = new NotificationSubTotalModel();
                        __NotificationSubTotalModel.UserID = reader["UserID"].ToString();
                        __NotificationSubTotalModel.Category = reader["Category"].ToString();
                        __NotificationSubTotalModel.CountCategory = Vn(reader["CountCategory"].ToString());
                        __NotificationSubTotalModel.Icon = reader["Icon"].ToString();

                        _NotificationSubTotalModel.Add(__NotificationSubTotalModel);
                    }
                }
            }

            return _NotificationSubTotalModel;

        }
        public List<NotificationModel> NotificationList(string userid, string category, string recordnumber)
        {
            List<NotificationModel> _NotificationModel = new List<NotificationModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("SP_SYS_Notification", conn))//call Stored Procedure
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userid);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@RecordNumber", recordnumber);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        NotificationModel __NotificationModel = new NotificationModel();
                        __NotificationModel.RecordNumber = reader["RecordNumber"].ToString();
                        __NotificationModel.RecordDate = Vd(reader["RecordDate"].ToString(),"dd-MMM-yyyy HH:mm");
                        __NotificationModel.UserID = reader["UserID"].ToString();
                        __NotificationModel.Category = reader["Category"].ToString();
                        __NotificationModel.Sender = reader["Sender"].ToString();
                        __NotificationModel.Subject = reader["Subject"].ToString();
                        __NotificationModel.Message = reader["Message"].ToString();
                        __NotificationModel.MessageHtml = reader["MessageHtml"].ToString();
                        __NotificationModel.UrlLink = reader["UrlLink"].ToString();
                        __NotificationModel.Sent = bool.Parse(reader["Sent"].ToString());
                        __NotificationModel.Readed = bool.Parse(reader["Readed"].ToString());
                        __NotificationModel.Favorites = bool.Parse(reader["Favorites"].ToString());
                        __NotificationModel.Deleted = bool.Parse(reader["Deleted"].ToString());
                        __NotificationModel.Status = reader["Status"].ToString();

                        _NotificationModel.Add(__NotificationModel);
                    }
                }
            }

            return _NotificationModel;

        }
        public string NotificationRead(string ID, string FormAction)
        {
            string result = "";

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);

                switch (FormAction)
                {
                    case "readed":
                        MSQL = "Update Tbl_SYS_Notification With (TabLock)  set Readed = 1 where RecordNumber='" + ID + "'";
                        break;
                    case "favorites":
                        MSQL = "Update Tbl_SYS_Notification With (TabLock)  set Favorites = (case when isnull(Favorites,0)=0 then 1 else 0 end) where RecordNumber='" + ID + "'";
                        break;
                    case "deleted":
                        MSQL = "Update Tbl_SYS_Notification With (TabLock)  set Deleted = (case when isnull(Deleted,0)=0 then 1 else 0 end) where RecordNumber='" + ID + "'";
                        break;
                    default:
                        // code block
                        break;
                }

                cmd.CommandText = MSQL;
                cmd.ExecuteNonQuery();
                conn.Close();

                List<NotificationModel> _NotificationModel = new List<NotificationModel>();

                using (SqlCommand cmdr = new SqlCommand("SP_SYS_Notification", conn))//call Stored Procedure
                {
                    cmdr.CommandType = System.Data.CommandType.StoredProcedure;
                    cmdr.Parameters.AddWithValue("@RecordNumber", ID);
                    conn.Open();
                    SqlDataReader reader = cmdr.ExecuteReader();
                    while (reader.Read())
                    {
                        NotificationModel __NotificationModel = new NotificationModel();
                        __NotificationModel.Status = reader["Status"].ToString();

                        _NotificationModel.Add(__NotificationModel);
                    }
                }

                foreach (var x in _NotificationModel)
                {
                    result = x.Status;
                }

                return result;

            }
        }

        public IEnumerable<LogActivitiesModel> GetLogActivities(string userid)
        {
            List<LogActivitiesModel> _LogActivitiesModel = new List<LogActivitiesModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {
                    MSQL = "select * from Tbl_SYS_LogActivities where UserId is not null";
                    if (this.Vf(userid) != "")
                    {
                        MSQL += " and UserId='" + userid + "'";
                    }
                    MSQL += " Order By LogDate Desc";

                    cmd.CommandText = MSQL;
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        LogActivitiesModel __LogActivitiesModel = new LogActivitiesModel();

                        __LogActivitiesModel.LogDate = this.Vd(reader["LogDate"].ToString(), "dd-MMM-yy HH:mm:ss");
                        __LogActivitiesModel.UserID = reader["UserID"].ToString();
                        __LogActivitiesModel.IpAddress = reader["IpAddress"].ToString();
                        __LogActivitiesModel.Country = reader["Country"].ToString();
                        __LogActivitiesModel.City = reader["City"].ToString();
                        __LogActivitiesModel.Module = reader["Module"].ToString();
                        __LogActivitiesModel.Activity = reader["Activity"].ToString();
                        __LogActivitiesModel.Remarks = reader["Remarks"].ToString();


                        _LogActivitiesModel.Add(__LogActivitiesModel);
                    }
                }
            }
            return _LogActivitiesModel;
        }
        public string GetIP()
        {
            //string userip = System.Web.HttpContext.Current.Request.UserHostAddress;
            //if (System.Web.HttpContext.Current.Request.UserHostAddress != null)
            //{
            //    Int64 macinfo = new Int64();
            //    string macSrc = macinfo.ToString("X");
            //    if (macSrc == "0")
            //    {
            //        userip = "127.0.0.1";
            //    }
            //}

            string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //string ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (ip == "::1")
            {
                ip = "127.0.0.1";
            }
            return ip;

        }
        public IpDataModel GetIpData()
        {
            using (var webClient = new System.Net.WebClient())
            {
                try { 
                string apikey = "c115d0561db9570eddedfbb45b2e2be5f3a0168dc20c4f0239f71c59";
                string url = "https://api.ipdata.co/?api-key=" + apikey;
                var jsonData = webClient.DownloadString(url);
                // Now parse with JSON.Net
                IpDataModel ipData = JsonConvert.DeserializeObject<IpDataModel>(jsonData);

                return ipData;
                } catch(Exception ex)
                {
                    return null;
                }
            }
        }
        public SystemUtilizeModel SystemUtilize()
        {
            SystemUtilizeModel _SystemUtilizeModel = new SystemUtilizeModel();

            try
            {
                var systemutilize = workentity.SP_SYS_UsageProcessorInfo();
                foreach (WorkEntity.SP_SYS_UsageProcessorInfo_Result ProcessorInfo in systemutilize)
                {
                    _SystemUtilizeModel.ProcessorName = ProcessorInfo.ProcessorName;
                    _SystemUtilizeModel.LogicalCpu = Vn(ProcessorInfo.LogicalCpu.ToString());
                    _SystemUtilizeModel.PhysicalMemory = Vn(ProcessorInfo.PhysicalMemory.ToString());
                }
            }
            finally
            {
                //nothing
            }
            return _SystemUtilizeModel;

        }

        public IEnumerable<DatabaseUtilizeModel> DatabaseUtilize()
        {
            List<DatabaseUtilizeModel> _DatabaseUtilizeModel = new List<DatabaseUtilizeModel>();
            try
            {
                var UsageDatabaseInfo = workentity.SP_SYS_UsageDatabaseInfo();
                foreach (WorkEntity.SP_SYS_UsageDatabaseInfo_Result DatabaseInfo in UsageDatabaseInfo)
                {
                    DatabaseUtilizeModel __DatabaseUtilizeModel = new DatabaseUtilizeModel();
                    __DatabaseUtilizeModel.InstanceName = DatabaseInfo.InstanceName;
                    __DatabaseUtilizeModel.Edition = DatabaseInfo.Edition;
                    __DatabaseUtilizeModel.ProductVersion = DatabaseInfo.ProductVersion;
                    __DatabaseUtilizeModel.ProductLevel = DatabaseInfo.ProductLevel;
                    __DatabaseUtilizeModel.PhysicalName = DatabaseInfo.PhysicalName;
                    __DatabaseUtilizeModel.DatabaseSize = Vn(DatabaseInfo.DatabaseSize.ToString());
                    __DatabaseUtilizeModel.Units = DatabaseInfo.Units;

                    _DatabaseUtilizeModel.Add(__DatabaseUtilizeModel);
                }
            }
            finally
            {
                //nothhing
            }
            return _DatabaseUtilizeModel;

        }

        public IEnumerable<DriveUtilizeModel> DriveUtilize()
        {
            List<DriveUtilizeModel> _DriveUtilizeModel = new List<DriveUtilizeModel>();

            try
            {
                var UsageDriveInfo = workentity.SP_SYS_UsageDriveInfo();
                foreach (WorkEntity.SP_SYS_UsageDriveInfo_Result driveInfo in UsageDriveInfo)
                {
                    DriveUtilizeModel __DriveUtilizeModel = new DriveUtilizeModel();
                    __DriveUtilizeModel.DriveLetter = driveInfo.DriveLetter;
                    __DriveUtilizeModel.Label = driveInfo.DriveLabel;
                    __DriveUtilizeModel.FreeSpace = Vn(driveInfo.FreeSpace.ToString());
                    __DriveUtilizeModel.UsedSpace = Vn(driveInfo.UsedSpace.ToString());
                    __DriveUtilizeModel.TotalSpace = Vn(driveInfo.TotalSpace.ToString());
                    __DriveUtilizeModel.PercentageFreeSpace = Vn(driveInfo.PercentageFreeSpace.ToString());
                    __DriveUtilizeModel.PercentageUsedSpace = Vn(driveInfo.PercentageUsageSpace.ToString());

                    switch (__DriveUtilizeModel.PercentageUsedSpace)
                    {
                        case double n when n > 75:
                            __DriveUtilizeModel.PercentColour = "bg-danger";
                            break;
                        case double n when n > 50:
                            __DriveUtilizeModel.PercentColour = "bg-warning";
                            break;
                        case double n when n > 25:
                            __DriveUtilizeModel.PercentColour = "bg-primary";
                            break;
                        case double n when n > 0:
                            __DriveUtilizeModel.PercentColour = "bg-success";
                            break;
                    }

                    _DriveUtilizeModel.Add(__DriveUtilizeModel);
                }
            }
            finally
            {
                //nothing
            }
            return _DriveUtilizeModel;

        }

        public IEnumerable<DatabaseBackupModel> DatabaseBackupList(string backupdir = null, string backupdb = null)
        {
            string databaseName = workentity.Database.Connection.Database;
            backupdb = databaseName;

            List<DatabaseBackupModel> _DatabaseBackupModel = new List<DatabaseBackupModel>();

            var DatabaseBackupList = workentity.SP_SYS_DatabaseBackupList(backupdir, backupdb);
            foreach (SP_SYS_DatabaseBackupList_Result databaseBackupList in DatabaseBackupList)
            {
                DatabaseBackupModel __DatabaseBackupModel = new DatabaseBackupModel();
                __DatabaseBackupModel.BackupFile = databaseBackupList.BackupName;

                _DatabaseBackupModel.Add(__DatabaseBackupModel);
            }

            return _DatabaseBackupModel;

        }

        public string DatabaseBackup(string backupdir = null, string backupdb = null)
        {

            string databaseName = workentity.Database.Connection.Database;
            backupdb = databaseName;

            var DatabaseBackup = workentity.SP_SYS_DatabaseBackup(backupdir, backupdb);
            string result = "";
            foreach (SP_SYS_DatabaseBackup_Result databaseBackup in DatabaseBackup)
            {
                result = databaseBackup.BackupDatabase;
            }

            return result;

        }
        public string DatabaseRestore(string restorefile, string restoredir = null, string restoredb = null)
        {
            using (SqlConnection conn = new SqlConnection(DBMaster))
            {
                string rs = "";
                string databaseName = workentity.Database.Connection.Database;
                restoredb = databaseName;

                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_SYS_DatabaseRestore", conn);//call Stored Procedure
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@restorefile", restorefile);
                cmd.Parameters.AddWithValue("@restoredir", restoredir);
                cmd.Parameters.AddWithValue("@restoredb", restoredb);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rs = reader["restorestatus"].ToString();
                }
                return rs;
            }
        }
        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            return ((MemberExpression)memberAccess.Body).Member.Name;
        }
        public Vw_IDX_StockTakingEvent GetStockTakingEvent()
        {
            vssp_entity vssp = new vssp_entity();
            Vw_IDX_StockTakingEvent _StockTakingEvents = new Vw_IDX_StockTakingEvent();
            vssp.Database.CommandTimeout = 0;
            try
            {
                _StockTakingEvents = vssp.Vw_IDX_StockTakingEvent.FirstOrDefault();
            }
            finally
            {
                //nothing
            }
            return _StockTakingEvents;

        }
        public List<MenuReportListModel> MenuReportList(string menuid)
        {
            List<MenuReportListModel> _MenuReportListModel = new List<MenuReportListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    MSQL = "Select * from Tbl_SYS_MenuReport ";
                    MSQL += "Where MenuID='" + menuid + "' Order By MenuID";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MenuReportListModel __MenuReportListModel = new MenuReportListModel();
                        nom = nom + 1;
                        __MenuReportListModel.MenuID = reader["MenuID"].ToString();
                        __MenuReportListModel.FileName = reader["FileName"].ToString();
                        __MenuReportListModel.SchemaType = reader["SchemaType"].ToString();
                        __MenuReportListModel.SchemaName = reader["SchemaName"].ToString();
                        __MenuReportListModel.CustomFilter = reader["CustomFilter"].ToString();
                        __MenuReportListModel.SortOrder = reader["SortOrder"].ToString();

                        _MenuReportListModel.Add(__MenuReportListModel);
                    }
                }
            }

            return _MenuReportListModel;

        }
        public List<MenuReportFilterListModel> MenuReportFilterList(string menuid)
        {
            List<MenuReportFilterListModel> _MenuReportFilterListModel = new List<MenuReportFilterListModel>();

            using (SqlConnection conn = new SqlConnection(DBCon))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))//call Stored Procedure
                {

                    MSQL = "Select * from Tbl_SYS_MenuReportFilter ";
                    MSQL += "Where MenuID='" + menuid + "' Order By MenuID,Sort";

                    cmd.CommandText = MSQL;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MenuReportFilterListModel __MenuReportFilterListModel = new MenuReportFilterListModel();
                        nom = nom + 1;
                        __MenuReportFilterListModel.MenuID = reader["MenuID"].ToString();
                        __MenuReportFilterListModel.SchemaName = reader["SchemaName"].ToString();
                        __MenuReportFilterListModel.Field = reader["Field"].ToString();
                        __MenuReportFilterListModel.Caption = reader["Caption"].ToString();
                        __MenuReportFilterListModel.FilterName = reader["FilterName"].ToString();
                        __MenuReportFilterListModel.FilterType = reader["FilterType"].ToString();
                        __MenuReportFilterListModel.Active = bool.Parse(reader["Active"].ToString());
                        __MenuReportFilterListModel.Sort = int.Parse(reader["Sort"].ToString());

                        _MenuReportFilterListModel.Add(__MenuReportFilterListModel);
                    }
                }
            }

            return _MenuReportFilterListModel;

        }
        public string GetExceptionDetails(Exception ex)
        {
            var stringBuilder = new StringBuilder();
            int loop = 0;
            string defaultMsg = ex.Message;

            while (ex != null)
            {
                loop += 1;
                switch (ex)
                {
                    case DbEntityValidationException dbEx:
                        var errorMessages = dbEx.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
                        var fullErrorMessage = string.Join("; ", errorMessages);
                        var message = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                        //stringBuilder.Insert(0, dbEx.StackTrace);
                        stringBuilder.Insert(0, message);
                        break;

                    default:
                        //stringBuilder.Insert(0, ex.StackTrace + "<br/>");
                        if (loop > 1) stringBuilder.Insert(0, ex.Message + "<br/>");
                        break;
                }

                ex = ex.InnerException;
                //if (loop > 1) ex = null;

            }
            if (stringBuilder.Length == 0) stringBuilder.Insert(0, defaultMsg);
            //return stringBuilder.ToString().Replace("'", "%27").Replace("\r\n", "_n_").Replace(". ", "._n_").Replace("Tbl_MST_","").Replace("dbo.Tbl_MST_","").Replace("Tbl_TRS_", "").Replace("dbo.Tbl_TRS_", "").Replace("ACC_MST_", "").Replace("dbo.Tbl_ACC_", "");
            return stringBuilder.ToString().Replace("Tbl_MST_", "").Replace("dbo.Tbl_MST_", "").Replace("Tbl_TRS_", "").Replace("dbo.Tbl_TRS_", "").Replace("ACC_MST_", "").Replace("dbo.Tbl_ACC_", "");
        }
        public byte[] ByteToImage(byte[] blob)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(blob, 0, blob.Length);
                mStream.Seek(0, SeekOrigin.Begin);

                Bitmap bm = new Bitmap(mStream);
                ImageConverter converter = new ImageConverter();
                return (byte[])converter.ConvertTo(bm, typeof(byte[]));

            }
        }

    }
}