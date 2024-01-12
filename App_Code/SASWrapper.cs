using System;
using System.Data;
using System.Configuration;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace LMS2.components
{
    public  class SASWrapper
    {
        public static DataSet InitializeWebPage(string pageID, string databaseName, string userID)
        {
            DataSet ds = null;
            string sUserID = HttpContext.Current.User.Identity.Name;
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            try
            {
                if (userID.Length > 0 && HttpContext.Current.Session["ValidIPAddresses"] == null)
                    SetIPAddresses();

                if (userID.Length > 0 && HttpContext.Current.Session["ValidIPAddresses"] != null && (HttpContext.Current.Session["ValidIPAddresses"].ToString().Contains(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]) || HttpContext.Current.Session["ValidIPAddresses"].ToString().Contains("999.999.999.999")))
                    sUserID = userID;

                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    if (HttpContext.Current.Session["DBInstanceName"] == null)
                        SetInstance();
                    databaseName = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + databaseName : databaseName;
                    ds = sas.InitializeWebPage(pageID, databaseName, sUserID);
                }
                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:InitializeWebPage[" + pageID + "]", ex.ToString());
            }
            return ds;
        }

        public static DataSet QueryStoredProc_ResultSet(string spName, string[] paramIDs, string[] paramValues, string dbName, ref string error)
        {
            DataSet ds = null;
            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    if (HttpContext.Current.Session["DBInstanceName"] == null)
                        SetInstance();
                    dbName = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + dbName : dbName;
                    ds = sas.QueryStoredProc_ResultSet(spName, paramIDs, paramValues, dbName, ref error);
                    error = error.Replace("SAS SQLError executing stored procedure", " ");
                    error = error.Replace("Error Number:  50000:", " ");
                    if (!string.IsNullOrEmpty(error))
                        AppError.LogError("SASWrapper:QueryStoredProc_ResultSet[" + spName + "]", error);
                }
                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:QueryStoredProc_ResultSet[" + spName + "]", ex.ToString());
            }
            return ds;
        }

        public static DataSet QueryData(string databaseName, string table, string columns, string filter)
        {
            return QueryData(_Session("CurPageName"), databaseName, table, false, columns, filter, "");
        }

        public static DataSet QueryData(string databaseName, string table, string columns, string filter, string sort)
        {
            return QueryData(_Session("CurPageName"), databaseName, table, false, columns, filter, sort);
        }

        public static DataSet QueryData(string pageID, string databaseName, string table, bool isStoredProc, string columns, string filter, string sort)
        {
            string error = string.Empty;
            DataSet ds = null;
            string displayView = (IsHistoryPage()) ? ((table.Length > 0 && !isStoredProc) ? table : _Session(pageID + "HistoryView")) : ((table.Length > 0 && !isStoredProc) ? table : _Session(pageID + "DisplayView"));
            string spSelect = (table.Length > 0 && isStoredProc) ? table : _Session(pageID + "StoredProcForSelect");

            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;

                    string user = _Session("UserID");
                    user = (user.IndexOf("\\") >= 0) ? user.Substring(user.IndexOf("\\") + 1) : user;

                    if (displayView.Length > 0)
                    {
                        if (HttpContext.Current.Session["DBInstanceName"] == null)
                            SetInstance();
                        databaseName = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + databaseName : databaseName;
                        ds = sas.QueryData(columns, displayView, databaseName, filter, ref error);
                    }
                    else if (spSelect.Length > 0)
                    {

                        // special code for handling tagname filtering
                        string FileName = System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath).ToLower();
                        string tagfilter = string.Empty;
                        if ((FileName == "tagsummarydetail" || FileName == "threadinfo") && !IsHistoryPage())
                        {
                            if (_Session("TagnameFilter").Length > 0)
                                tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagnameFilter") : _Session("TagnameFilter");

                            if (_Session("TagLevel1Filter").Length > 0)
                                tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagLevel1Filter") : _Session("TagLevel1Filter");

                            if (_Session("TagLevel2Filter").Length > 0)
                                tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagLevel2Filter") : _Session("TagLevel2Filter");

                            if (_Session("TagLevel3Filter").Length > 0)
                                tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagLevel3Filter") : _Session("TagLevel3Filter");

                            if (tagfilter.Length > 0)
                                if (filter.Length > 0)
                                    tagfilter = " AND (" + tagfilter + ")";
                                else
                                    tagfilter = "(" + tagfilter + ")";

                            tagfilter += "$PARM$StartDate=" + _Session("TagStartDate");
                            tagfilter += "$PARM$StartTime=" + _Session("TagStartTime");
                            tagfilter += "$PARM$DurationDay=" + _Session("TagDurationDay");
                            tagfilter += "$PARM$DurationMin=" + _Session("TagDurationMin");
                            tagfilter += "$PARM$Direction=" + _Session("TagDirection");
                            tagfilter += "$PARM$Interval=" + _Session("TagInterval");
                            tagfilter += "$PARM$Notes=" + _Session("TagNoteEntry") + "$PARM$";
                        }
                        filter = filter + tagfilter;
                        if (HttpContext.Current.Session["DBInstanceName"] == null)
                            SetInstance();
                        databaseName = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + databaseName : databaseName;
                        ds = sas.QueryStoredProc_ResultSet_UUQP(pageID, databaseName, spSelect, 0, columns, 0, filter, "", false, user, ref error);
                    }

                    // If a sort has been defined then apply it...
                    if (sort.Length > 0 && ds != null && ds.Tables.Count > 0)
                    {
                        DataView dv = new DataView(ds.Tables[0], "", sort, DataViewRowState.CurrentRows);
                        string tableName = ds.Tables[0].TableName;
                        ds.Tables.RemoveAt(0);
                        ds.Tables.Add(dv.ToTable(tableName));
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        AppError.LogError("SASWrapper:QueryData[" + table + "]", error);

                        // Go to error page because we cannot recover from this...
                        string url = string.Format("BadError.aspx?NodeID={3}&PageID={0}&Method={1}&Error={2}",
                            HttpContext.Current.Server.UrlEncode(_Session("CurPageName")), "QueryData",
                            HttpContext.Current.Server.UrlEncode(error), HttpContext.Current.Request.QueryString["NodeID"]);
                        HttpContext.Current.Response.Redirect(url, true);
                    }
                }

                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:QueryData[" + table + "]", ex.ToString());
            }

            return ds;
        }

        public static DataSet ExecuteQuery(string databaseName, string query)
        {
            string error = string.Empty;
            DataSet ds = null;

            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;

                    if (query.Length > 0 || databaseName.Length > 0)
                    {
                        if (HttpContext.Current.Session["DBInstanceName"] == null)
                            SetInstance();
                        databaseName = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + databaseName : databaseName;
                        ds = sas.ExecuteQuery(databaseName, query, ref error);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        int idx = error.IndexOf("Invalid column name ");
                        if (idx >= 0)
                        {
                            string msg = error.Substring(idx);
                            msg = (msg.IndexOf(".") >= 0) ? msg.Substring(0, msg.IndexOf(".")) : msg;
                            if (error.IndexOf("INSQL") >= 0)
                                error = Regex.Replace(msg + ".  The tag mapping definition may not be correct for this tag.", "column", "tag");
                        }

                        AppError.LogError("SASWrapper:QueryData[" + query + "]", error);
                    }
                }
                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:QueryData[" + query + "]", ex.ToString());
            }

            return ds;
        }

        public static DataSet QueryDataUUQP(string pageID, string dbname, string table, int OQL, string columns, int numRows, string filter, string sortString, string showObsolete, string hiddenFilter)
        {
            string error = string.Empty;
            DataSet ds = null;
            string displayView = (IsHistoryPage()) ? ((table.Length > 0) ? table : _Session(pageID + "HistoryView")) : ((table.Length > 0) ? table : _Session(pageID + "DisplayView"));
            string spSelect = _Session(pageID + "StoredProcForSelect");
            table = (table == null) ? "" : table;
            filter = ((filter == null) || (filter.Trim() == "()")) ? "" : filter;

            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    string user = _Session("UserID");
                    user = (user.IndexOf("\\") >= 0) ? user.Substring(user.IndexOf("\\") + 1) : user;

                    // Save query preferences if not applying a quick filter
                    if (_Session("QuickFilter").Length == 0 && !IsHistoryPage())
                    {
                        //string user = HttpContext.Current.User.Identity.Name;
                        SASWrapper.QueryStoredProc_ResultSet("uspAppsQueryPreferenceUpdateFilter",
                            new string[] { "@UserID", "@PageID", "@Filter", "@NumOfRows", "@ShowObsolete" },
                            new string[] { user, pageID, filter, numRows.ToString(), _Session("ShowObsolete") },
                            dbname, ref error);
                    }

                    // special code for handling "show obsolete"
                    string FileName = System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath).ToLower();
                    if ((pageID.ToLower() == "wa11f1p1" || pageID.ToLower() == "wa11f1p7b") && _Session("ShowObsolete") == "0" && !IsHistoryPage())
                        filter += (filter.Length > 0) ? " AND ActiveShort=1" : "ActiveShort=1";
                    if ((FileName == "summaryorder" || pageID.ToLower() == "wa55f1p1") && _Session("ShowObsolete") == "1" && !IsHistoryPage())
                        filter += (filter.Length > 0) ? " AND " + _Session("WhereDate") : _Session("WhereDate");

                    // special code for handling tagname filtering
                    string tagfilter = string.Empty;
                    if (_Session("TagFilterUpdate") == "true" && ((FileName == "tagsummarydetail" || FileName == "threadinfo" || (FileName == "bulkedit" && bool.Parse(_Session("TagFiltering")))) && !IsHistoryPage()))
                    {
                        if (_Session("TagnameFilter").Length > 0)
                            tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagnameFilter") : _Session("TagnameFilter");

                        if (_Session("TagLevel1Filter").Length > 0)
                            tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagLevel1Filter") : _Session("TagLevel1Filter");

                        if (_Session("TagLevel2Filter").Length > 0)
                            tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagLevel2Filter") : _Session("TagLevel2Filter");

                        if (_Session("TagLevel3Filter").Length > 0)
                            tagfilter += (tagfilter.Length > 0) ? " OR " + _Session("TagLevel3Filter") : _Session("TagLevel3Filter");

                        if (tagfilter.Length > 0)
                            if (filter.Length > 0)
                                tagfilter = " AND (" + tagfilter + ")";
                            else
                                tagfilter = "(" + tagfilter + ")";

                        tagfilter += "$PARM$StartDate=" + _Session("TagStartDate");
                        tagfilter += "$PARM$StartTime=" + _Session("TagStartTime");
                        tagfilter += "$PARM$DurationDay=" + _Session("TagDurationDay");
                        tagfilter += "$PARM$DurationMin=" + _Session("TagDurationMin");
                        tagfilter += "$PARM$Direction=" + _Session("TagDirection");
                        tagfilter += "$PARM$Interval=" + _Session("TagInterval");
                        tagfilter += "$PARM$Notes=" + _Session("TagNoteEntry") + "$PARM$";

                        //if (FileName != "ThreadInfo") HttpContext.Current.Session["TagFilterUpdate"] = string.Empty;
                    }
                    else
                        if ((FileName != "tagsummarydetail" && FileName != "threadinfo") || IsHistoryPage())
                        tagfilter = string.Empty;

                    if (!string.IsNullOrEmpty(_Session("HiddenFilter")))
                        filter = string.IsNullOrEmpty(filter) ? _Session("HiddenFilter") : _Session("HiddenFilter") + " AND " + filter;
                    filter = filter + tagfilter;
                    HttpContext.Current.Session["TagFilterSQL"] = filter;

                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;

                    Log.WriteLine(string.Format("{0}, {1}, {2}, '{3}'", DateTime.Now.ToString("HH:mm:ss.f"), user, pageID, ((filter != null) ? filter : "")));

                    if (HttpContext.Current.Session["DBInstanceName"] == null)
                        SetInstance();
                    dbname = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + dbname : dbname;
                    if (displayView.Length > 0)
                        ds = sas.QueryData_UUQP(columns, pageID, dbname, displayView, OQL, columns, numRows, ((filter != null) ? filter : ""), sortString, "False", false, user, ref error);
                    else if (spSelect.Length > 0)
                        ds = sas.QueryStoredProc_ResultSet_UUQP(pageID, dbname, spSelect, OQL, columns, numRows, filter, "False", false, user, ref error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        AppError.LogError("SASWrapper:QueryDataUUQP[" + pageID + "]", error);

                        // Go to error page because we cannot recover from this...
                        string url = string.Format("BadError.aspx?NodeID={3}&PageID={0}&Method={1}&Error={2}",
                            HttpContext.Current.Server.UrlEncode(_Session("CurPageName")), "QueryData",
                            HttpContext.Current.Server.UrlEncode(error), HttpContext.Current.Request.QueryString["NodeID"]);
                        HttpContext.Current.Response.Redirect(url, true);
                    }

                    if (columns.StartsWith("Thumbnail"))
                        FixThumbnailUrls(ref ds);
                }
                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:QueryDataUUQP[" + pageID + "]", ex.ToString());
            }

            return ds;
        }

        public static string UpdateData(string storedProcName, string dbname, DataSet changes)
        {
            string error = string.Empty;
            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    if (HttpContext.Current.Session["DBInstanceName"] == null)
                        SetInstance();
                    dbname = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + dbname : dbname;
                    error = sas.StoredProcForUpdate(storedProcName, dbname, changes);
                    error = error.Replace("SAS SQLError  50000: ", " ");
                    if (!string.IsNullOrEmpty(error))
                        AppError.LogError("SASWrapper:UpdateData[" + storedProcName + "]", error);
                }
                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:UpdateData[" + storedProcName + "]", ex.ToString());
            }

            return error;
        }

        public static string UpdateData(string storedProcName, string dbname, string columnNames, string columnValues)
        {
            string error = string.Empty;
            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    if (HttpContext.Current.Session["DBInstanceName"] == null)
                        SetInstance();
                    dbname = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + dbname : dbname;
                    error = sas.StoredProcForUpdate2(dbname, storedProcName, columnNames, columnValues);
                    if (!string.IsNullOrEmpty(error))
                        AppError.LogError("SASWrapper:UpdateData[" + storedProcName + "]", error);
                }
                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:UpdateData[" + storedProcName + "]", ex.ToString());
            }

            return error;
        }

        public static string UpdateData_LogError(string storedProcName, string dbname, DataSet changes)
        {
            // This version of the method exists because if an error occurs and LogError is called, then
            // it will in turn call UpdateData which will in turn call LogError, etc... leading to a
            // stack overflow... so this version of UpdateData simple doesn't call LogError or do any 
            // exception hanlding.
            string error = string.Empty;
            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    if (HttpContext.Current.Session["DBInstanceName"] == null)
                        SetInstance();
                    dbname = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + dbname : dbname;
                    error = sas.StoredProcForUpdate(storedProcName, dbname, changes);
                }
                // */
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            return error;
        }

        public static DataTable TranslateStrings(string dbName, bool useAltLang, DataTable dt)
        {
            // start with original so we can return what we passed in if something goes wrong
            DataTable translated = dt.Copy();
            string error = "";

            try
            {
                /*
                using (SAS2.Service1 sas = new SAS2.Service1())
                {
                    sas.PreAuthenticate = true;
                    sas.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    if (HttpContext.Current.Session["DBInstanceName"] == null)
                        SetInstance();
                    dbName = (_Session("DBInstanceName").Length > 0) ? _Session("DBInstanceName") + "|" + dbName : dbName;
                    error = sas.TranslatePageStrings(dbName, useAltLang, ref translated);
                    if (!string.IsNullOrEmpty(error))
                        AppError.LogError("SASWrapper:TranslateStrings", error);
                }
                // */
            }
            catch (Exception ex)
            {
                AppError.LogError("SASWrapper:TranslateStrings", ex.ToString());
            }

            return translated;
        }

        private static string _Session(string id)
        {
            return _Session(id, "");
        }

        private static string _Session(string id, string def)
        {
            return (HttpContext.Current.Session[id] != null) ? HttpContext.Current.Session[id].ToString() : def;
        }

        private static bool IsHistoryPage()
        {
            return (HttpContext.Current.Request.QueryString["History"] == "Yes");
        }

        private static void FixThumbnailUrls(ref DataSet ds)
        {
            // This method determines if thumbnail images exist or not, and if not, then sets them to null so the default img will get displayed
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + row["ThumbnailImageUrl"].ToString()))
                        row["ThumbnailImageUrl"] = DBNull.Value;
            }
        }
        private static void SetInstance()
        {
            string strDBInstanceName = HttpContext.Current.Request.ApplicationPath.Replace("/", string.Empty);
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            for (int i = 0; i < appSettings.Count; i++)
            {
                if (appSettings.GetKey(i).StartsWith("DatabaseInstanceName"))
                {
                    string[] sValue = new string[] { string.Empty, string.Empty };
                    string[] sInstance = new string[] { string.Empty, string.Empty };
                    string strApplicationPath = string.Empty;
                    string strInstance = string.Empty;
                    sValue = appSettings[i].Split(new char[] { ';' });
                    for (int j = 0; j < 2; j++)
                    {
                        sInstance = sValue[j].Split(new char[] { '=' });
                        if (sInstance[0].ToLower() == "applicationpath")
                            strApplicationPath = sInstance[1];
                        if (sInstance[0].ToLower() == "instancename")
                            strInstance = sInstance[1];
                    }
                    if (strDBInstanceName.ToLower() == strApplicationPath.ToLower())
                        HttpContext.Current.Session["DBInstanceName"] = strInstance;
                }
            }
        }

        private static void SetIPAddresses()
        {
            string strIPAddresses = HttpContext.Current.Request.ApplicationPath.Replace("/", string.Empty);
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            for (int i = 0; i < appSettings.Count; i++)
            {
                if (appSettings.GetKey(i).StartsWith("ValidIPAddresses"))
                {
                    string[] sValue = new string[] { string.Empty, string.Empty };
                    string[] sIP = new string[] { string.Empty, string.Empty };
                    string strApplicationPath = string.Empty;
                    string strValidIPAddresses = string.Empty;
                    sValue = appSettings[i].Split(new char[] { ';' });
                    for (int j = 0; j < 2; j++)
                    {
                        sIP = sValue[j].Split(new char[] { '=' });
                        if (sIP[0].ToLower() == "applicationpath")
                            strApplicationPath = sIP[1];
                        if (sIP[0].ToLower() == "validipaddresses")
                            strValidIPAddresses = sIP[1];
                    }
                    HttpContext.Current.Session["ValidIPAddresses"] = strValidIPAddresses;
                    if (strIPAddresses.ToLower() == strApplicationPath.ToLower())
                    {
                        HttpContext.Current.Session["ValidIPAddresses"] = strValidIPAddresses;
                        break;
                    }
                }
            }
        }
    }
}