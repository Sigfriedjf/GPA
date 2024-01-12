using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.ComponentModel;

namespace LMS2.components
{
    public class BasePage : Page
    {
        private string _error = string.Empty;
        protected string PageError { get { return _error; } }
        protected Boolean pageIDChanged = false;
        private enum SessionVarsToCopy { DisplayView, OverrideQueryLimit, AllColumns, ColumnShown, DetailsColumnShown, NumOfRows, Filter, SortString, AdvFilterFlag, QueryKeyColumn, PrimaryKeyColumn, SelectedItemKey, SpecialDetail, ShowObsolete, HelpPage, BulkEdit, ColumnToolbar }

        // Security related definitions and properties...
        [Flags]
        public enum SASPermission { None = 0, Read = 1, Update = 2, Delete = 4, NotYetSet = 8 }
        private SASPermission _pageSecurity = SASPermission.NotYetSet;
        public SASPermission PageSecurity
        {
            get
            {
                if (_pageSecurity == SASPermission.NotYetSet)
                {
                    string secFlag = PCSSecurity.Permission(UserID, PageID).ToString();
                    _pageSecurity = ((SASPermission)(int)Math.Pow(2, int.Parse(secFlag)) - 1);
                }
                return _pageSecurity;
            }
        }
        public bool CanRead { get { return ((PageSecurity & SASPermission.Read) == SASPermission.Read); } }
        public bool CanDelete { get { return ((PageSecurity & SASPermission.Delete) == SASPermission.Delete); } }
        public bool CanEdit { get { return ((PageSecurity & SASPermission.Update) == SASPermission.Update && _Session(PageID + "Insertable") != "2"); } }
        public bool CanInsert { get { return (_Session(PageID + "Insertable") != "0" && PageSecurity > SASPermission.Update); } }
        public void MarkSecurityStale() { _pageSecurity = SASPermission.NotYetSet; }
        public static bool UseCancelConfirmation { get { return _Session("CancelConfirmation") == "True" ? true : false; } }
        public bool NoPasswordExpiration { get { return _Session("NoPasswordExpiration") == "1"; } }
        public DateTime LastPasswordChange { get { DateTime dt; return (DateTime.TryParse(_Session("PasswordChanged"), out dt)) ? dt : DateTime.MinValue; } }
        public int PasswordExpPeriod { get { return (_Session("PasswordExpiration").Length > 0) ? int.Parse(_Session("PasswordExpiration")) : 0; } }

        public static string DateFormat { get { return _Session("DateFormat"); } }
        public static string TimeFormat { get { return _Session("TimeFormat"); } }
        public static string DateTimeFormat { get { return (_Session("DateFormat") + ' ' + _Session("TimeFormat")); } }

        // Public page properties
        public string FacilityID { get { return _Session("FacilityID"); } }
        public string PageID { get { return _Session("CurPageName"); } }
        public string UserID { get { return _Session("UserID"); } }
        public string DatabaseName { get { return _Session("DatabaseName"); } }
        public string ServerName { get { return _Session("ServerName"); } }
        public bool UseAltLanguage { get { return _Session("AltLanguageIndicator") == "1" ? true : false; } }
        public bool WrapAltLanguage { get { return _Session("WrapAltLanguage") == "True" ? true : false; } }
        public string HelpFilePath { get { return _Session("HelpFilePath"); } set { Session["HelpFilePath"] = value; } }
        public bool IsHistoryPage { get { return (Request.QueryString["History"] == "Yes"); } }
        public bool HasHistoryPage { get { return (_Session(PageID + "HistoryView").Length > 0); } }
        public bool LeavingHistory { get { return (Session["LeavingHistory"] != null) ? (bool)Session["LeavingHistory"] : false; } }
        public bool UserChanged { get { return (Session["UserChanged"] != null) ? (bool)Session["UserChanged"] : false; } set { Session["UserChanged"] = value; } }
        public string DetailMode { get { return (Request.QueryString["DetailMode"] != null) ? Request.QueryString["DetailMode"] : ""; } }
        public string FileName { get { return System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath); } }

        // These properties are overidden to provide user switching support
        protected virtual string UserIDDisplay { set { } }
        protected virtual bool UserChangeTimer { get { return false; } set { } }

        // Secret sauce... ;-)  This causes the view state to be persisted in a Session state variable instead of a hidden page variable.  This
        // reduces the size of what gets sent to the server on each postback.  If we find the consumption of server resources to be a problem 
        // we may want to remove this line and let it fall back to the default
        protected override PageStatePersister PageStatePersister { get { return new SessionPageStatePersister(this); } }

        // The PreInit override calls and sets up all of the session variables for the page...
        protected void Page_PreInit(object sender, EventArgs e)
        {
            string foo = Server.UrlEncode("(Resource='QT1' and UploadStatus=20)");

            if (!IsPostBack)
                Session["RowSelect"] = null; // Set by OrderSelection.aspx for special case row selection logic in schedule pages.

            Culture = "auto:en-US"; // Causes pages to auto-detect the browser locale and fall back to English (en-US) if unable to do it.
            AddSessionKeepAlive();  // Causes pages to ping the server and keep Session vars alive
            Response.Cache.SetCacheability(HttpCacheability.NoCache); // Removes client side caching, which was killing us in some cases where
                                                                      // the back button was being pressed.

            if (!Request.FilePath.EndsWith("BadError.aspx"))
                LoadSessionVariables((Request["pageID"] != null) ? Request["pageID"] : "");

            // check for password expiration if we haven't checked yet
            if (!Request.FilePath.EndsWith("PasswordExpired.aspx"))
            {
                if (!IsPostBack)
                    if (CheckPasswordExpiration())
                        RedirectToSecurePasswordChange();
            }
        }

        // Loads session variables for the designated page...
        protected void LoadSessionVariables(string pageID)
        {
            string _error = "";
            pageIDChanged = false;

            // If no PageID, then send to Default.aspx (this should only occur during our debugging)
            if (Request["PageID"] == null)
                Response.Redirect("Default.aspx");

            string WWQueryFilter = string.Empty;
            if ((Request.QueryString["src"] != null && Request.QueryString["src"] == "ww") && !string.IsNullOrEmpty(Request.QueryString["Filter"]))
            {
                WWQueryFilter = Server.UrlDecode(Request.QueryString["Filter"]);
                WWQueryFilter = Regex.Replace(WWQueryFilter, ",", " AND ");
                WWQueryFilter = Regex.Replace(WWQueryFilter, @"~~Today~~", DateTime.Now.ToString("MM/dd/yyyy"));
            }

            string WWQueryTag = string.Empty;
            string WWQueryTagLevel1 = string.Empty;
            string WWQueryTagLevel2 = string.Empty;
            string WWQueryTagLevel3 = string.Empty;

            if (Request.QueryString["src"] != null && Request.QueryString["src"] == "ww")
            {
                Session["WonderWare"] = "ww";
                if (!string.IsNullOrEmpty(Request.QueryString["Tag"]))
                {
                    WWQueryTag = Server.UrlDecode(Request.QueryString["Tag"]);
                    WWQueryTag = Regex.Replace(WWQueryTag, "'", string.Empty);
                }
                if (!string.IsNullOrEmpty(Request.QueryString["TL1"]))
                {
                    WWQueryTagLevel1 = Server.UrlDecode(Request.QueryString["TL1"]);
                    WWQueryTagLevel1 = Regex.Replace(WWQueryTagLevel1, "'", string.Empty);
                }
                if (!string.IsNullOrEmpty(Request.QueryString["TL2"]))
                {
                    WWQueryTagLevel2 = Server.UrlDecode(Request.QueryString["TL2"]);
                    WWQueryTagLevel2 = Regex.Replace(WWQueryTagLevel2, "'", string.Empty);
                }
                if (!string.IsNullOrEmpty(Request.QueryString["TL3"]))
                {
                    WWQueryTagLevel3 = Server.UrlDecode(Request.QueryString["TL3"]);
                    WWQueryTagLevel3 = Regex.Replace(WWQueryTagLevel3, "'", string.Empty);
                }
            }

            // Only load the page if it has not already been loaded
            if (_Session("CurPageName") != pageID || _Session("CurAspxPage") != Request.FilePath || IsHistoryPage || LeavingHistory || UserChanged || (WWQueryFilter.Length > 0 && _Session("Filter") != "()" && WWQueryFilter != _Session("Filter")) || WWQueryTag.Length > 0 && WWQueryTag != _Session("Tag") || WWQueryTagLevel1.Length > 0 && WWQueryTagLevel1 != _Session("TL1") || WWQueryTagLevel2.Length > 0 && WWQueryTagLevel2 != _Session("TL2") || WWQueryTagLevel3.Length > 0 && WWQueryTagLevel3 != _Session("TL3"))
            {
                Session["CurPageName"] = pageID;
                Session["CurAspxPage"] = Request.FilePath;
                Session["AdvFilterFlag"] = "False";
                Session["QuickFilter"] = string.Empty;
                Session["UserPermission"] = "0";
                Session["Editable"] = "0";

                if (LeavingHistory && Session["History_CallingPageWasFilterScoped"] != null && !((bool)Session["History_CallingPageWasFilterScoped"]))
                    Session["PageIsFilterScoped"] = false;

                Session["LeavingHistory"] = IsHistoryPage; // if this is a history page, set the flag so we'll reload the normal page when we come back in

                string sUserID = string.Empty;
                if (Request.QueryString["UID"] != null && Request.QueryString["UID"].Length > 0)
                {
                    sUserID = Request.QueryString["UID"];
                    Session["UserID"] = sUserID;
                }
                DataSet ds = SASWrapper.InitializeWebPage(pageID, DatabaseName, _Session("UserID").Length > 0 ? _Session("UserID") : sUserID);
                pageIDChanged = true;
                if (ds != null && ds.Tables.Count > 0)
                    Session["PageSessionVars"] = ds.Tables[0];

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Columns.Contains("Result"))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string key = (row["Name"] != null) ? row["Name"].ToString() : "";
                            string val = (row["Result"] != null) ? row["Result"].ToString() : "";
                            string type = (row["ParmType"] != null) ? row["ParmType"].ToString() : "";

                            if (key.Length > 0)
                            {
                                if (row["ParmType"].ToString().StartsWith("Page"))
                                    Session[pageID + key] = val;
                                else if (row["ParmType"].ToString().StartsWith("Query"))
                                    Session[pageID + key] = val;
                                else if (row["ParmType"].ToString().StartsWith("Error"))
                                    _error = (_error.Length > 0) ? string.Format("{0}\r\n{1}", _error, val) : val;
                                else if (row["ParmType"].ToString().StartsWith("Global"))
                                    Session[key] = val;
                                else if (row["ParmType"].ToString().StartsWith("User"))
                                    Session[key] = val;
                                else if (row["ParmType"].ToString().StartsWith("Application"))
                                    Session[key] = val;
                            }
                        }
                    }
                    else
                        _error = "Page Init Error: Invalid result set returned from SAS: Missing element [Result]";
                }
                else
                    _error = "Page Init Error: Empty result set returned from SAS";

                // Set up local session vars for data access
                foreach (string sv in Enum.GetNames(typeof(SessionVarsToCopy)))
                    Session[sv] = Session[pageID + sv];

                // Get column names and translations and store them for later
                // Use a bogus filter to return no data since I'm only interested in the columns
                ds = SASWrapper.QueryData(DatabaseName, "", "*", "0=1");
                if (ds != null && ds.Tables.Count > 0)
                {
                    // if the scope column is not present in the current column set (which it may not be since it is from another table) then we need
                    // to add it in so that it can be translated in the scope label
                    string scopeColumn = "";
                    if (_Session("PageScopePageID") == PageID)
                        scopeColumn = _Session("PageScopeColumn");

                    DataTable dt = ds.Tables[0];
                    bool scopeColumnAdded = false;
                    TranslateRequest tr = new TranslateRequest();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        tr.Add("", "column", dt.Columns[i].ColumnName);
                        if (dt.Columns[i].ColumnName.ToLower() == scopeColumn.ToLower())
                            scopeColumnAdded = true;
                    }
                    if (!scopeColumnAdded && scopeColumn.Length > 0)
                        tr.Add("", "column", scopeColumn);

                    DataTable tdt = SASWrapper.TranslateStrings(((BasePage)Page).DatabaseName, ((BasePage)Page).UseAltLanguage, tr.GetTable());
                    Session["ColumnDefinitions"] = dt; // store list of columns and types 
                    Session["ColumnTranslations"] = tdt; // store for later use...
                }

                // If ColumnShown or DetailColumnShown has an "*" then expand it to contain all columns.  
                Dictionary<string, DataColumn> cols = PCSTranslation.ColumnDefinitions;
                if (_Session("AllColumns").Trim() == "*" || _Session("ColumnShown").Trim() == "*" || _Session("DetailsColumnShown").Trim() == "*")
                {
                    // Expand columns...
                    string colList = "";
                    foreach (DataColumn dc in cols.Values)
                        colList += (((colList.Length > 0) ? "," : "") + dc.ColumnName);
                    if (_Session("AllColumns").Trim() == "*")
                        Session["AllColumns"] = colList;
                    if (_Session("ColumnShown").Trim() == "*")
                        Session["ColumnShown"] = colList;
                    if (_Session("DetailsColumnShown").Trim() == "*")
                        Session["DetailsColumnShown"] = colList;
                }

                // Make sure all requested columns actually exist... if not then remove them...
                // In this case for all columns, we are going to maintain a hash table so we can add any columns which accidentally appear in 
                // ColumnShown or DetailColumnShown that are not in AllColumns and then we are going to persist that value back to keep things consistent.
                Dictionary<string, string> allColumns = new Dictionary<string, string>();
                string newColumnList = "";
                foreach (string s in Regex.Split(_Session("AllColumns"), ","))
                {
                    string colName = s.Trim().ToLower();
                    if (colName.Length > 0 && cols.ContainsKey(colName))
                    {
                        allColumns[colName] = cols[colName].ColumnName;
                        newColumnList += (((newColumnList.Length > 0) ? "," : "") + cols[colName].ColumnName);
                    }
                }
                Session["AllColumns"] = newColumnList;
                int allColumnsCount = allColumns.Count;

                // Make sure all requested columns actually exist... if not then remove them...
                newColumnList = "";
                Dictionary<string, int> visibleColumns = new Dictionary<string, int>();
                foreach (string s in Regex.Split(_Session("ColumnShown"), ","))
                {
                    string colName = s.Trim().ToLower();
                    if (colName.Length > 0)
                    {
                        if (!allColumns.ContainsKey(colName) && cols.ContainsKey(colName))
                            allColumns[colName] = cols[colName].ColumnName;
                        if (cols.ContainsKey(colName))
                        {
                            newColumnList += (((newColumnList.Length > 0) ? "," : "") + cols[colName].ColumnName);
                            visibleColumns[colName] = 1;
                        }
                    }
                }
                Session["ColumnShown"] = newColumnList;

                // Make sure all requested columns actually exist... if not then remove them...
                newColumnList = "";
                foreach (string s in Regex.Split(_Session("DetailsColumnShown"), ","))
                {
                    string colName = s.Trim().ToLower();
                    if (colName.Length > 0)
                    {
                        if (!allColumns.ContainsKey(colName) && cols.ContainsKey(colName))
                            allColumns[colName] = cols[colName].ColumnName;
                        if (cols.ContainsKey(s.Trim().ToLower()))
                        {
                            newColumnList += (((newColumnList.Length > 0) ? "," : "") + cols[colName].ColumnName);
                            visibleColumns[colName] = 1;
                        }
                    }
                }
                Session["DetailsColumnShown"] = newColumnList;

                if (System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath.ToLower()) != "bulkedit")
                    Session["VisbleColumnsHash"] = visibleColumns;

                if (!IsHistoryPage)
                {
                    // If any new cols were added to allColumns from either ColumnShown or DetailsColumnShown then persist it back...
                    if (allColumnsCount < allColumns.Count)
                    {
                        newColumnList = "";
                        foreach (string s in allColumns.Values)
                            newColumnList += (((newColumnList.Length > 0) ? "," : "") + s);

                        // call the stored procedure that will set up our initial values in the test page...
                        string error = "";
                        string[] paramNames = new string[] { "@PageID", "@NewAllColumns" };
                        string[] paramValues = new string[] { PageID, newColumnList };
                        SASWrapper.QueryStoredProc_ResultSet("uspLMS_AppsUpdateAllColumns", paramNames, paramValues, DatabaseName, ref error);
                    }
                }
                else
                {
                    // if this is a HistoryPage, override ColumnShown with AllColumns and set permission to read-only
                    Dictionary<string, byte> histColumns = new Dictionary<string, byte>();
                    List<string> histColumnsList = new List<string>() { "UpdatedDate", "UpdatedBy" };
                    histColumns["updateddate"] = 1;
                    histColumns["updatedby"] = 1;

                    // Add columns from detailColumnShown
                    foreach (string s in Regex.Split(_Session("DetailsColumnShown"), ","))
                    {
                        string colName = s.Trim();
                        if (colName.Length > 0 && !histColumns.ContainsKey(s.ToLower()))
                        {
                            histColumns[s.ToLower()] = 1;
                            histColumnsList.Add(s);
                        }
                    }

                    // Add in any columns from AllColumns which are not yet in the list
                    foreach (string colName in allColumns.Values)
                        if (!histColumns.ContainsKey(colName.ToLower()))
                            histColumnsList.Add(colName);

                    // Add comments field if available, even if not in AllColumns
                    if (cols.ContainsKey("comments"))
                        histColumnsList.Add("Comments");

                    string histColumnsStr = "";
                    foreach (string s in histColumnsList)
                        histColumnsStr += (((histColumnsStr.Length > 0) ? "," : "") + s);

                    // Set QueryKeyColumn to the history's QueryKeyColumn so the detailview tracks the summarygrid selection
                    Session[PageID + "ColumnHidden"] = Session[PageID + "ColumnHidden"].ToString().Length > 0 ? Session[PageID + "ColumnHidden"].ToString() + ",IDHist" : "IDHist";
                    Session[PageID + "ColumnHidden"] = Session["QueryKeyColumn"].ToString().Length > 0 ? Session[PageID + "ColumnHidden"].ToString() + "," + Session["QueryKeyColumn"] : Session["QueryKeyColumn"];
                    Session["QueryKeyColumn"] = "IDHist";
                    Session[pageID + "QueryKeyColumn"] = "IDHist";
                    Session["ColumnShown"] = histColumnsStr;
                    Session["DetailsColumnShown"] = histColumnsStr;

                    _pageSecurity = ((int)PageSecurity > 1) ? SASPermission.Read : PageSecurity; // set to 1 (read-only) if more than that, otherwise leave alone
                    Session["SortString"] = "UpdatedDate DESC";
                    Session[PageID + "AggregateColumn"] = "";
                    Session["Filter"] = "";
                    string atSuffix = PCSTranslation.GetMessage("AuditTrail").Trim();
                    if (!Session[PageID + "PageHeading"].ToString().EndsWith(atSuffix))
                        Session[PageID + "PageHeading"] = Session[PageID + "PageHeading"].ToString() + atSuffix;
                }

                // Determine columns that should be hidden in grid
                string skey = PageID + "ColumnHidden";
                Dictionary<string, int> hideCols = new Dictionary<string, int>();
                if (Session[skey] != null && Session[skey].ToString().Length > 0)
                {
                    foreach (string col in Regex.Split(Session[skey].ToString(), ","))
                        hideCols[col.Trim().ToLower()] = -1; // -1 is important beacuse later this becomes a column index if present
                }
                // If the page is scope filtered, add the field to the hidden list
                if (IsPageFilterScoped && _Session("PageScopeFilterColumn").Length > 0 && !hideCols.ContainsKey(_Session("PageScopeFilterColumn").ToLower()))
                    hideCols[_Session("PageScopeFilterColumn").ToLower()] = -1;
                Session["HiddenColumns"] = hideCols;

                // If Filter is specified on the Request, use it instead...
                Session["HiddenFilter"] = string.Empty;
                if (!string.IsNullOrEmpty(Request.QueryString["Filter"]))
                {
                    Session["Filter"] = "";
                    Session["UpdateUserPref"] = false;

                    string Filter = Server.UrlDecode(Request.QueryString["Filter"]);
                    Filter = Regex.Replace(Filter, ",", " AND ");
                    Filter = Regex.Replace(Filter, @"~~Today~~", DateTime.Now.ToString("MM/dd/yyyy"));

                    if (hideCols.Count > 0)
                        foreach (string col in hideCols.Keys)
                            // if the Filter is a hidden column then set HiddenFilter session else set Filter session
                            if (Filter != null)
                            {
                                Regex r = new Regex("(\\w*)[^\\w]");
                                Match m = r.Match(Filter);
                                if ((m.Success && m.Groups.Count > 1) && (col.ToLower() == m.Groups[1].Value.ToLower()))
                                    Session["HiddenFilter"] = Filter;
                            }
                    if (Session["HiddenFilter"].ToString().Length == 0)
                        Session["Filter"] = Filter;
                }
                else
                    Session["UpdateUserPref"] = true;

                // If Tag is specified in the request set the session
                Session["Tag"] = string.Empty;
                if (!string.IsNullOrEmpty(Request.QueryString["Tag"]))
                {
                    string Tag = Server.UrlDecode(Request.QueryString["Tag"]);
                    Session["Tag"] = Regex.Replace(Tag, "'", string.Empty);
                }

                // If TL1 is specified in the request set the session
                Session["TL1"] = string.Empty;
                if (!string.IsNullOrEmpty(Request.QueryString["TL1"]))
                {
                    string Tag = Server.UrlDecode(Request.QueryString["TL1"]);
                    Session["TL1"] = Regex.Replace(Tag, "'", string.Empty);
                }

                // If TL2 is specified in the request set the session
                Session["TL2"] = string.Empty;
                if (!string.IsNullOrEmpty(Request.QueryString["TL2"]))
                {
                    string Tag = Server.UrlDecode(Request.QueryString["TL2"]);
                    Session["TL2"] = Regex.Replace(Tag, "'", string.Empty);
                }

                // If TL3 is specified in the request set the session
                Session["TL3"] = string.Empty;
                if (!string.IsNullOrEmpty(Request.QueryString["TL3"]))
                {
                    string Tag = Server.UrlDecode(Request.QueryString["TL3"]);
                    Session["TL3"] = Regex.Replace(Tag, "'", string.Empty);
                }

                // Set help file path...
                HelpFilePath = GetFilePath("HelpFiles", PageID, "htm", "NoHelp");

                // Determine read-only columns
                DetermineReadOnlyColumns();

                // Determine columns needing a combo drop downs
                Session["DropDownColumns"] = new DropDownColumns(PageID, _Session(PageID + "DropDownColumn"));
                Session[PageID + "DropDownColumns"] = Session["DropDownColumn"];  // this is needed for figuring out tag navigation

                // Determine columns needing tbe excluded during insert
                skey = PageID + "ExcludeColumnOnInsert";
                Dictionary<string, int> ieCols = new Dictionary<string, int>();
                if (Session[skey] != null && Session[skey].ToString().Length > 0)
                {
                    foreach (string col in Regex.Split(Session[skey].ToString(), ","))
                        ieCols[col.Trim().ToLower()] = 1;
                }
                ieCols["updateddate"] = 1;
                ieCols["updatedby"] = 1;
                Session["InsertExcludedColumns"] = ieCols;

                // Determine columns needing to be excluded during quick filter
                skey = PageID + "ExcludeFromQuickFilter";
                Dictionary<string, int> qfCols = new Dictionary<string, int>();
                if (Session[skey] != null && Session[skey].ToString().Length > 0)
                {
                    foreach (string col in Regex.Split(Session[skey].ToString(), ","))
                        qfCols[col.Trim().ToLower()] = 1;
                }
                Session["QuickFilterExcludedColumns"] = qfCols;

                // Determine columns needing styles
                skey = PageID + "ColumnStyle";
                Dictionary<string, ColumnStyle> colStyles = new Dictionary<string, ColumnStyle>();
                Dictionary<string, string> colFormats = new Dictionary<string, string>();
                if (Session[skey] != null && Session[skey].ToString().Length > 0)
                {
                    foreach (string col in Regex.Split(Session[skey].ToString(), ";"))
                    {
                        if (col.Trim().Length > 0)
                        {
                            MatchCollection stuff = Regex.Matches(col.Trim(), @"[^{},=]+");
                            string Name = string.Empty;
                            ColumnStyle cs = new ColumnStyle();
                            for (int i = 0; i < stuff.Count; i++)
                            {
                                if (i == 0) Name = stuff[i].ToString().Trim().ToLower();
                                else
                                {
                                    if (stuff[i].ToString().Trim().ToLower() == "height")
                                        cs.height = int.Parse(stuff[i + 1].ToString().Trim());
                                    if (stuff[i].ToString().Trim().ToLower() == "width")
                                        cs.width = int.Parse(stuff[i + 1].ToString().Trim());
                                    if (stuff[i].ToString().Trim().ToLower() == "multiline")
                                        cs.multiline = bool.Parse(stuff[i + 1].ToString().Trim());
                                    if (stuff[i].ToString().Trim().ToLower() == "format")
                                    {
                                        cs.format = stuff[i + 1].ToString().Trim();
                                        colFormats[Name] = cs.format;
                                    }
                                }
                            }
                            colStyles[Name] = cs;
                        }
                    }
                }
                Session["ColumnStyles"] = colStyles;
                Session["ColumnFormats"] = colFormats;

                // If we are page scoped then don't keep selection between page invokations
                if (Session["PageIsFilterScoped"] != null && ((bool)Session["PageIsFilterScoped"]) && string.IsNullOrEmpty(Request.QueryString["Selection"]))
                {
                    //if (!IsHistoryPage) // don't clear selection if it is a history page
                    {
                        Session[pageID + "SelectedItemKey"] = "";
                        Session["SelectedItemKey"] = "";
                    }
                }
            }

            // Get column aggregate definitions...
            Session["ColumnAggregates"] = new Aggregates(_Session(PageID + "AggregateColumn"));

            // If Selection is specified on the Request, use it instead...
            bool pageStateProcessed = ViewState["PageStateProcessed"] != null ? (bool)ViewState["PageStateProcessed"] : false;
            if (!IsPostBack && !pageStateProcessed && Request.QueryString["Selection"] != null && (Session["QueryKeyColumn"] != null) && !string.IsNullOrEmpty(Session["QueryKeyColumn"].ToString()))
            {
                ViewState["PageSelectionProcessed"] = true;
                Session[pageID + "SelectedItemKey"] = Request.QueryString["Selection"];
                Session["SelectedItemKey"] = Request.QueryString["Selection"];
            }
            else // special processing for the My Profile page
            {
                if (PageID == "MyProfile")
                {
                    Session[pageID + "SelectedItemKey"] = UserID;
                    Session["SelectedItemKey"] = UserID;
                }

                if (PageID == "ShiftHandoffEntering")
                {
                    Session[pageID + "SelectedItemKey"] = UserID;
                    Session["SelectedItemKey"] = UserID;
                }

                if (PageID == "ShiftHandoffLeaving")
                {
                    Session[pageID + "SelectedItemKey"] = UserID;
                    Session["SelectedItemKey"] = UserID;
                }
            }
            if (Session[pageID + "SelectedItemKey"] == null)
            {
                Session[pageID + "SelectedItemKey"] = "";
                Session["SelectedItemKey"] = "";
            }

            // If no permission to access this page then punt away to "no permissions" page
            if (PageSecurity == SASPermission.None && Request.QueryString["PageID"].Length > 0)
                Response.Redirect(string.Format("NoPermission.aspx?PageID=&NodeID={0}", Request.QueryString["NodeID"]));

            // Check server licensing
            LMS2.components.LicenseManager lm = (LMS2.components.LicenseManager)Application["LicenseManager"];
            if (!lm.IsLicenseValid && !Request.FilePath.EndsWith("NoLicense.aspx"))
                Response.Redirect("NoLicense.aspx?PageID=&NodeID=");

            // Check concurrent user limit
            if (!lm.IsCurrentSessionLicensed(Session.SessionID) && !Request.FilePath.EndsWith("NoUserLicense.aspx"))
                Response.Redirect("NoUserLicense.aspx?PageID=&NodeID=");

            if (!string.IsNullOrEmpty(Request.QueryString["PageScopeReset"]))
                Session["PageScopePageID"] = string.Empty;

            Title = _Session(pageID + "PageHeading", "IWAF Untitled");
        }

        protected void DetermineReadOnlyColumns()
        {
            string skey = PageID + "ColumnReadOnly";

            // If an "*" then set equal to DetailColumnShown
            if (_Session(skey).Trim() == "*")
                Session[skey] = Session["DetailsColumnShown"];

            string primaryKey = (Page.Session[PageID + "PrimaryKeyColumn"] != null) ? Page.Session[PageID + "PrimaryKeyColumn"].ToString().ToLower() : "";
            Dictionary<string, int> roCols = new Dictionary<string, int>();
            if (Session[skey] != null && Session[skey].ToString().Length > 0)
            {
                foreach (string col in Regex.Split(Session[skey].ToString(), ","))
                    roCols[col.Trim().ToLower()] = 1;
            }

            // Ensure that primary key, UpdatedDate, and UpdatedBy are read-only
            if (primaryKey.Length > 0)
                roCols[primaryKey] = 1;
            roCols["updateddate"] = 1;
            roCols["updatedby"] = 1;
            Session["ReadOnlyColumns"] = roCols;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Dipslay the userid if the screen has a place for it
            UserIDDisplay = UserID;

            if (UserChanged)
            {
                if (!UserChangeTimer)
                    UserChanged = false;
                UserChangeTimer = false;
            }

            // Resolve Page Scope...
            //if (PageID == _Session("PageScopePageID") && _Session("PageScopeColumn").Length > 0)
            if (IsPageFilterScoped && _Session("PageScopeColumn").Length > 0)
            {
                Dictionary<string, string> colTrans = PCSTranslation.ColumnTranslations;
                string colName = _Session("PageScopeColumn");
                colName = (colTrans.ContainsKey(colName.ToLower())) ? colTrans[colName.ToLower()] : colName;
                Session["PageScope"] = string.Format("( {0} = {1} )", colName, _Session("PageScopeValue"));
            }
            else if (!IsPageFilterScoped && Request.QueryString["Filter"] != null && Request.QueryString["Filter"].Length > 0)
            {
                Session["PageScope"] = string.Format("( {0} )", Request.QueryString["Filter"]);
            }
            else
            {
                Session["PageScope"] = "";
            }

            // Determine all strings which are I8N candidates and call SAS for their appropriate display text.  This is also used to swap out
            // column names with more appropriate display names, even if not internationalized.
            TranslateRequest tr = new TranslateRequest();
            AddControlsToControlList(Controls, ref tr);

            if ((pageIDChanged && !IsPostBack) || Session["PageStringTable"] == null || Request.FilePath.EndsWith("BadError.aspx"))
            {
                DataTable pageStringTable = tr.GetTable();
                pageStringTable = SASWrapper.TranslateStrings(DatabaseName, UseAltLanguage, pageStringTable);
                Session["PageStringTable"] = pageStringTable;
            }

            ApplyTranslatedStrings((DataTable)Session["PageStringTable"]);
        }

        private enum I8NControl { DoNothing, Label, DataControlFieldHeaderCell, DetailsViewRow, ImageButton, Button }
        private void AddControlsToControlList(ControlCollection controls, ref TranslateRequest tr)
        {
            Dictionary<string, I8NControl> controlTypes = new Dictionary<string, I8NControl>();
            foreach (I8NControl val in Enum.GetValues(typeof(I8NControl)))
                controlTypes[val.ToString()] = val;

            foreach (Control c in controls)
            {
                string type = c.GetType().ToString();
                type = type.Substring(type.LastIndexOf('.') + 1);
                I8NControl ctrlType = (controlTypes.ContainsKey(type)) ? controlTypes[type] : I8NControl.DoNothing;

                switch (ctrlType)
                {
                    case I8NControl.Label:
                        {
                            Label lb = (c as Label);

                            // Starting and Ending with $$ says we should use this as the name of a Session var which holds the real text
                            if (lb.Text.StartsWith("$$") && lb.Text.EndsWith("$$"))
                            {
                                string varName = lb.Text.Substring(2, lb.Text.Length - 4);
                                if (Session[PageID + varName] != null)
                                    lb.Text = Session[PageID + varName].ToString();
                                else if (Session[varName] != null)
                                    lb.Text = Session[varName].ToString();
                                // Remove the ((( and ))) from the PageScope to make it look nicer
                                if (varName == "PageScope")
                                    lb.Text = lb.Text.Replace("(((", string.Empty).Replace(")))", string.Empty);
                            }
                            AddControlToList(c.UniqueID, "label", lb.Text, ref tr);
                        }
                        break;

                    case I8NControl.ImageButton:
                        {
                            ImageButton ib = (c as ImageButton);
                            AddControlToList(c.UniqueID, "imagebutton", ib.ToolTip, ref tr);
                        }
                        break;

                    case I8NControl.Button:
                        {
                            Button ib = (c as Button);
                            AddControlToList(c.UniqueID, "button", ib.Text, ref tr);
                        }
                        break;

                    case I8NControl.DoNothing: /* Do nothing... this is where anything falls that isn't specifically specified */
                        {
                            //if (c.UniqueID.Contains("Detail"))
                            //{
                            //  HtmlControl hc = (c as HtmlControl);
                            //  AddControlToList(c.UniqueID, c.GetType().ToString(), (hc != null && hc.Attributes["Text"] != null) ? hc.Attributes["Text"] : "null", ref ctrls);
                            //}
                        }
                        break;
                }

                if (c.HasControls())
                    AddControlsToControlList(c.Controls, ref tr);
            }
        }

        private void AddControlToList(string id, string type, string text, ref TranslateRequest tr)
        {
            if (text.Length > 0)
                tr.Add(id, type, text);
        }

        private void ApplyTranslatedStrings(DataTable dt)
        {
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string altText = (row["AltText"] != DBNull.Value) ? row["AltText"].ToString() : "";

                    if (altText.Length > 0)
                    {
                        string id = (row["ID"] != DBNull.Value) ? row["ID"].ToString() : "";
                        Control c = (id.Length > 0) ? FindControl(id) : null;

                        if (c != null)
                        {
                            string type = (row["Type"] != DBNull.Value) ? row["Type"].ToString() : "";
                            if (type.StartsWith("l"))
                                (c as Label).Text = altText;
                            if (type.StartsWith("i"))
                                (c as ImageButton).ToolTip = altText;
                            if (type.StartsWith("b"))
                                (c as Button).Text = altText;
                        }
                    }
                }
            }
        }

        public static string _Session(string id)
        {
            return _Session(id, "");
        }

        public static string _Session(string id, string def)
        {
            object o = HttpContext.Current.Session[id];
            return (o != null) ? o.ToString() : def;
        }

        public bool IsPageFilterScoped
        {
            get
            {
                bool scoped = (Session["PageIsFilterScoped"] != null) ? (bool)Session["PageIsFilterScoped"] : false;
                return (PageID == _Session("PageScopePageID")) && scoped;
            }
        }

        public string GetFilePath(string directory, string name, string ext, string defFile)
        {
            string filename = "";

            if (UseAltLanguage)
            {
                filename = string.Format("{0}/{1}_{2}_ALT.{3}", directory, name, FacilityID, ext);
                if (!File.Exists(Server.MapPath(filename)))
                {
                    // Try without facilty but still alt language
                    filename = string.Format("{0}/{1}_ALT.{2}", directory, name, ext);
                    if (!File.Exists(Server.MapPath(filename)))
                    {
                        filename = "";
                    }
                }
            }

            if (filename.Length == 0)
            {
                // Try with facility
                filename = string.Format("{0}/{1}_{2}.{3}", directory, name, FacilityID, ext);
                if (!File.Exists(Server.MapPath(filename)))
                {
                    // Try without facilty 
                    filename = string.Format("{0}/{1}.{2}", directory, name, ext);
                    if (!File.Exists(Server.MapPath(filename)))
                    {
                        // apply default if set
                        filename = (defFile.Length > 0) ? string.Format("{0}/{1}.{2}", directory, defFile, ext) : "";
                        if (filename.Length > 0 && !File.Exists(Server.MapPath(filename)))
                        {
                            // Still no good... punt
                            filename = "";
                        }
                    }
                }
            }

            return filename;
        }

        private void AddSessionKeepAlive()
        {
            int int_MilliSecondsTimeOut = (this.Session.Timeout * 60000) - 15000;
            string str_Script = @"
			<script type='text/javascript'>
				//Number of Reconnects
				var count=0;
				//Maximum reconnects setting
				var max = 5;
				function Reconnect()
				{
					count++;
					if (count < max)
					{
						var img = new Image(1,1);
						var decRound = Math.round(Math.random()*1000);
						img.src = 'KeepAliveRequest.aspx?id=' + decRound.toString();
					}
				}
				window.setInterval('Reconnect()', " + int_MilliSecondsTimeOut.ToString() + @"); //Set to length required
			</script>";

            ClientScript.RegisterClientScriptBlock(this.GetType(), "Reconnect", str_Script);
        }


        protected void InvokeHistoryPage()
        {
            if (HasHistoryPage && _Session("SelectedItemKey").Length > 0)
            {
                string filter = "";
                Session["History_CallingPageWasFilterScoped"] = IsPageFilterScoped;
                if (IsPageFilterScoped)
                {
                    filter = string.Format("{0}='{1}'", _Session("PageScopeColumn"), _Session("PageScopeValue"));
                }
                else
                {
                    Session["PageScopeColumn"] = _Session("QueryKeyColumn");
                    Session["PageScopeFilterColumn"] = _Session("QueryKeyColumn");
                    Session["PageScopeValue"] = _Session("SelectedItemKey");
                    filter = string.Format("{0}='{1}'", _Session("QueryKeyColumn"), _Session("SelectedItemKey"));
                }

                Session["PageScopePageID"] = PageID;
                Session["PageIsFilterScoped"] = true;

                Response.Redirect(string.Format("SummaryDetail.aspx?PageID={0}&NodeID={1}&History=Yes&Filter={2}", Server.UrlEncode(PageID), Request.QueryString["NodeID"], Server.UrlEncode(filter)));
            }
        }

        protected void ExportToExcel(string filter)
        {
            string columns = ((Session["AllColumns"] != null) && Session["AllColumns"].ToString() != string.Empty) ? Session["AllColumns"].ToString() : "";
            if (string.IsNullOrEmpty(columns))
                columns = ((Session["ColumnShown"] != null) && Session["ColumnShown"].ToString() != string.Empty) ? Session["ColumnShown"].ToString() : "*";

            // Get list of hidden columns so we can remove them from the columns string
            Dictionary<string, int> hideCols = Session["HiddenColumns"] as Dictionary<string, int>;
            Dictionary<string, int> cols = new Dictionary<string, int>();
            columns = Regex.Replace(columns.ToLower(), @"\s", ""); // trim all spaces
            foreach (string col in Regex.Split(columns, ","))
                if (col.Length > 0 && !hideCols.ContainsKey(col.ToLower()))
                    cols[col] = 1;
            columns = string.Empty;
            foreach (KeyValuePair<string, int> kvp in cols)
            {
                columns = columns + kvp.Key + ",";
            }
            columns = columns.Remove(columns.Length - 1, 1); // remove the last ","

            if (Session["HiddenFilter"] != null && !string.IsNullOrEmpty(Session["HiddenFilter"].ToString()))
                filter = string.IsNullOrEmpty(filter) ? Session["HiddenFilter"].ToString() : Session["HiddenFilter"].ToString() + " AND " + filter;

            DataSet ds = SASWrapper.QueryData(_Session("DatabaseName"), "", columns, filter);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                TranslateRequest tr = new TranslateRequest();
                for (int i = 0; i < dt.Columns.Count; i++)
                    tr.Add("", "column", dt.Columns[i].ColumnName);
                DataTable tdt = SASWrapper.TranslateStrings(DatabaseName, UseAltLanguage, tr.GetTable());
                for (int i = 0; i < tdt.Rows.Count; i++)
                {
                    string fieldName = tdt.Rows[i]["Text"].ToString();
                    string headerText = (tdt.Rows[i]["AltText"].ToString().Length > 0) ? tdt.Rows[i]["AltText"].ToString() : fieldName;
                    dt.Columns[i].Caption = headerText;
                }

                Utilities.ExportToExcel(ds.Tables[0], PageID);
            }
        }

        public virtual void ProcessSelection()
        {
            // do nothing
        }

        public static Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, id);
                if (t != null)
                    return t;
            }

            return null;
        }

        public void SetFilteredSelectionMessage(bool display)
        {
            Label tb = FindControlRecursive(this, "FilteredSelectionMsg") as Label;
            if (tb != null)
            {
                if (display)
                    tb.Text = PCSTranslation.GetMessage("SelectedFilteredOut");
                tb.Visible = display;
            }
        }

        public bool CheckPasswordExpiration()
        {
            int daysToExpire = int.MaxValue; // default to no expiration

            if (((BasePage)Page).DatabaseName.ToLower() == _Session("PrimaryDatabase").ToLower() && PasswordExpPeriod > 0 && !NoPasswordExpiration)
            {
                if (LastPasswordChange != DateTime.MinValue)
                {
                    TimeSpan span = DateTime.Now - LastPasswordChange;
                    daysToExpire = PasswordExpPeriod - (int)span.TotalDays;
                    if (daysToExpire < 0)
                        daysToExpire = 0;
                }
                else
                {
                    daysToExpire = PasswordExpPeriod;
                    string err = "";
                    SASWrapper.QueryStoredProc_ResultSet("uspLMS_UpdatePasswordChanged",
                        new string[] { "@UserID", "@ChangedDate" },
                        new string[] { UserID, DateTime.Now.ToString() },
                        ((BasePage)Page).DatabaseName, ref err);
                }
            }
            Session["PasswordDaysToExpire"] = daysToExpire;

            int passWarnDays;
            if (!int.TryParse(_Session("PasswordExpirationWarning"), out passWarnDays))
                passWarnDays = 10;

            return ((daysToExpire <= 0) || (daysToExpire != int.MaxValue && daysToExpire <= passWarnDays && Request.FilePath.EndsWith("Home.aspx")));
        }

        public virtual string NoDataGridMsg
        {
            get
            {
                // Overridden in subclasses to provide better info...
                return PCSTranslation.GetMessage("NoData");
            }
        }

        public void OpenPopUp(WebControl opener, string PagePath, string windowName, int width, int height)
        {
            string clientScript;
            string windowAttribs;

            // Building Client side window attributes with width and height.
            // Also the the window will be positioned to the middle of the screen
            windowAttribs = "width=" + width + "px," +
                            "height=" + height + "px," +
                            "left='+((screen.width -" + width + ") / 2)+'," +
                            "top='+ (screen.height - " + height + ") / 2+'," +
                            "status=no,toolbar=no,menubar=no,location=no,scrollbars=yes,resizable=yes";

            // Building the client script- window.open, with additional parameters
            clientScript = "window.open('" + PagePath + "','" + windowName + "','" + windowAttribs + "');return false;";

            // regiter the script to the clientside click event of the 'opener' control
            opener.Attributes.Add("onClick", clientScript);
        }

        protected void RedirectToSecurePasswordChange()
        {
            // Only redirect if not already on an error or password related page
            if (!Request.FilePath.EndsWith("PasswordExpired.aspx") &&
                  !Request.FilePath.EndsWith("PasswordChanged.aspx") &&
                  !Request.FilePath.EndsWith("BadError.aspx"))
            {
                String originalUrl = string.Format("~/Secure/PasswordExpired.aspx?UserID={0}&PageID=", Server.UrlEncode(UserID));
                String modifiedUrl = "https://" + Request.Url.Authority + Response.ApplyAppPathModifier(originalUrl);
                Response.Redirect(modifiedUrl);
            }
        }

        protected void ChangeUser(object sender, EventArgs e)
        {
            if (!UserChanged)
            {
                UserChangeTimer = true;
            }
        }

        protected void ChangeUserPostbackTimer_Tick(object sender, EventArgs e)
        {
            if (!UserChanged)
            {
                Session["UserID"] = null;
                UserChanged = true;
                Response.StatusCode = 401;
                Response.StatusDescription = "Unauthorized";
                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        public bool IsValidURL(string url)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}