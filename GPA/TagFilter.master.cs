using LMS2.components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GPA
{
    public partial class TagGridView : System.Web.UI.MasterPage
    {
        string sWhere;
        string sTagName;
        string sTL1Name;
        string sTL2Name;
        string sTL3Name;
        string sTagnames;
        string sTagLevel1;
        string sTagLevel2;
        string sTagLevel3;
        string sDevice;
        string sMissingTagnames;
        string sMissingTagLevel1;
        string sMissingTagLevel2;
        string sMissingTagLevel3;
        string sFileName;
        private DataTable dtFilterData = new DataTable();
        private DataTable dtFilterHeading = new DataTable();
        private ArrayList alTagnameArray = new ArrayList();
        private ArrayList alTagLevel1Array = new ArrayList();
        private ArrayList alTagLevel2Array = new ArrayList();
        private ArrayList alTagLevel3Array = new ArrayList();
        public bool AdvFilterFlag { get { return GetSessionVariable("AdvFilterFlag") == "True" ? true : false; } }
        public bool UseAltLanguage { get { return GetSessionVariable("AltLanguageIndicator") == "1" ? true : false; } }
        public string Filter { get { return GetSessionVariable("Filter") != null ? GetSessionVariable("Filter") : ""; } }
        public string DatabaseName { get { return GetSessionVariable("DatabaseName") != null ? GetSessionVariable("DatabaseName") : ""; } }
        public string UserID { get { return GetSessionVariable("UserID") != null ? GetSessionVariable("UserID") : ""; } }
        public string CurPageName { get { return GetSessionVariable("CurPageName") != null ? GetSessionVariable("CurPageName") : ""; } }
        public bool FilteringEnabled { get { return FilteringPanel.Visible; } set { FilteringPanel.Visible = value; } }
        public string Tag { get { return GetSessionVariable("Tag") != null ? GetSessionVariable("Tag") : ""; } }
        public string TL1 { get { return GetSessionVariable("TL1") != null ? GetSessionVariable("TL1") : ""; } }
        public string TL2 { get { return GetSessionVariable("TL2") != null ? GetSessionVariable("TL2") : ""; } }
        public string TL3 { get { return GetSessionVariable("TL3") != null ? GetSessionVariable("TL3") : ""; } }

        // Declare delegates
        public delegate void BindFilter();
        public delegate void AddTag();

        // Define an Event based on the above Delegates
        public event BindFilter BindFilterData;
        public event AddTag AddTagName;

        public HtmlGenericControl GridFilter
        {
            get { return divFilter; }
            set { divFilter = value; }
        }

        public string ErrorMessage
        {
            get { return ((LMSiframe)Master).ErrorMessage; }
        }

        public TextBox SQLText
        {
            get { return tbxSQL; }
            set { tbxSQL = value; }
        }

        public string Where
        {
            get { return sWhere; }
            set { sWhere = value; }
        }

        public string TagName
        {
            get { return sTagName; }
            set { sTagName = value; }
        }

        public string TL1Name
        {
            get { return sTL1Name; }
            set { sTL1Name = value; }
        }

        public string TL2Name
        {
            get { return sTL2Name; }
            set { sTL2Name = value; }
        }

        public string TL3Name
        {
            get { return sTL3Name; }
            set { sTL3Name = value; }
        }

        protected string GetSessionVariable(string name)
        {
            return (Session[name] != null) ? Session[name].ToString() : "";
        }

        public DataTable FilterData
        {
            get { return dtFilterData; }
            set { dtFilterData = value; }
        }

        public TextBox QuickFilter
        {
            get { return QuickFilterBox; }
        }

        public TextBox StartDate
        {
            get { return tbxStartDate; }
            set { tbxStartDate = value; }
        }

        public TextBox DurationDay
        {
            get { return tbxDurationDay; }
            set { tbxDurationDay = value; }
        }

        public TextBox DurationMin
        {
            get { return tbxDurationMin; }
            set { tbxDurationMin = value; }
        }

        public TextBox NoteEntry
        {
            get { return tbxNoteEntry; }
            set { tbxNoteEntry = value; }
        }

        public DropDownList Direction
        {
            get { return ddlDirection; }
            set { ddlDirection = value; }
        }

        public TextBox StartTime
        {
            get { return tbxStartTime; }
            set { tbxStartTime = value; }
        }

        public TableCell StartDateCell
        {
            get { return tcStartDate; }
            set { tcStartDate = value; }
        }

        public TableCell StartTimeCell
        {
            get { return tcStartTime; }
            set { tcStartTime = value; }
        }

        public TableCell DurationDayCell
        {
            get { return tcDurationDay; }
            set { tcDurationDay = value; }
        }

        public TableCell DurationMinCell
        {
            get { return tcDurationMin; }
            set { tcDurationMin = value; }
        }

        public TableCell NoteEntryCell
        {
            get { return tcNoteEntry; }
            set { tcNoteEntry = value; }
        }

        public TableCell DirectionCell
        {
            get { return tcDirection; }
            set { tcDirection = value; }
        }

        public TableCell IntervalCell
        {
            get { return tcInterval; }
            set { tcInterval = value; }
        }

        public TextBox Interval
        {
            get { return tbxInterval; }
            set { tbxInterval = value; }
        }

        public string Tagnames
        {
            get { return GetTagnames(); }
            set { SetTagnames(value); }
        }

        public string TagLevel1
        {
            get { return GetTagLevel1(); }
            set { SetTagLevel1(value); }
        }

        public string TagLevel2
        {
            get { return GetTagLevel2(); }
            set { SetTagLevel2(value); }
        }

        public string TagLevel3
        {
            get { return GetTagLevel3(); }
            set { SetTagLevel3(value); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            tblTagGridFilter.BorderColor = System.Drawing.ColorTranslator.FromHtml(ColorManager.ColorGridHeader);
            sFileName = System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath);
            OpenPopUp(ibFilterInfo, "HelpPage.aspx?FunctionID=FilterCriteria", "_blank", 600, 600);
            OpenPopUp(ibTagFilterInfo, "HelpPage.aspx?FunctionID=TagFilterCriteria", "_blank", 600, 600);
            // If a filter is in effect and the QuickFilter is not in use, then force the filter panel to be expanded regardless of it's state
            if (QuickFilterBox.Text.Length == 0 && !string.IsNullOrEmpty(Filter))
                CollapsiblePanelExtender1.Collapsed = false;

            // If a Tagfilter is in effect force the tagname panel to be expanded
            if (!string.IsNullOrEmpty(Tag) || !string.IsNullOrEmpty(TL1) || !string.IsNullOrEmpty(TL2) || !string.IsNullOrEmpty(TL3) || alTagLevel1Array.Count > 0 || alTagLevel2Array.Count > 0 || alTagLevel3Array.Count > 0 || alTagnameArray.Count > 0)
                CollapsiblePanelExtender2.Collapsed = false;

            if (Request.QueryString["DurationSpan"] != null && Request.QueryString["DurationSpan"] == "True")
            {
                pnlDateTime.Visible = true;
                if (sFileName != "ThreadInfo")
                    CollapsiblePanelExtender2.Collapsed = false;
            }

            if (!IsPostBack)
            {
                if (((BasePage)Page).CanInsert && sFileName != "ThreadInfo")
                    thNew.Visible = true;
                int RowNumber = 0;
                if (!string.IsNullOrEmpty(TL1))
                {
                    InitializeTagTable(null);
                    string[] sTL1 = TL1.Split(new char[] { ',' });
                    RowNumber = 0;
                    foreach (string sTL in sTL1)
                    {
                        if (sTL.Length > 0)
                        {
                            RowNumber = RowNumber + 1;
                            if (RowNumber <= 10)
                                FilterTagLevel2TagLevel3Tagname(sTL, RowNumber);
                        }
                    }
                    AddFilter();
                }
                else if (!string.IsNullOrEmpty(TL2))
                {
                    InitializeTagTable(null);
                    string[] sTL2 = TL2.Split(new char[] { ',' });
                    RowNumber = 0;
                    foreach (string sTL in sTL2)
                    {
                        if (sTL.Length > 0)
                        {
                            RowNumber = RowNumber + 1;
                            if (RowNumber <= 10)
                                FilterTagLevel3Tagname(sTL, RowNumber);
                        }
                    }
                    AddFilter();
                }
                else if (!string.IsNullOrEmpty(TL3))
                {
                    InitializeTagTable(null);
                    string[] sTL3 = TL3.Split(new char[] { ',' });
                    RowNumber = 0;
                    foreach (string sTL in sTL3)
                    {
                        if (sTL.Length > 0)
                        {
                            RowNumber = RowNumber + 1;
                            if (RowNumber <= 10)
                                FilterTagname(sTL, RowNumber);
                        }
                    }
                    AddFilter();
                }
                else if (!string.IsNullOrEmpty(Tag))
                {
                    InitializeTagTable(Tag);
                    string[] sTags = Tag.Split(new char[] { ',' });
                    RowNumber = 0;
                    foreach (string sTag in sTags)
                    {
                        if (sTag.Length > 0)
                        {
                            RowNumber = RowNumber + 1;
                            if (RowNumber <= 10)
                                PopulateTagLabels(sTag, RowNumber);
                        }
                    }
                    AddFilter();
                }
                else
                    InitializeTagTable(null);

                RowNumber = 0;
                for (int j = 0; j < alTagLevel1Array.Count; j++)
                {
                    RowNumber = RowNumber + 1;
                    string sValue = ((string[,])alTagLevel1Array[j])[0, 4]; // Get value from array
                    DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (RowNumber).ToString());
                    ddl.SelectedValue = sValue;
                    FilterTagLevel2TagLevel3Tagname(ddl.SelectedValue, RowNumber);
                    AddFilter();
                }
                for (int j = 0; j < alTagLevel2Array.Count; j++)
                {
                    RowNumber = RowNumber + 1;
                    string sValue = ((string[,])alTagLevel2Array[j])[0, 4]; // Get value from array
                    DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (RowNumber).ToString());
                    ddl.SelectedValue = sValue;
                    FilterTagLevel3Tagname(ddl.SelectedValue, RowNumber);
                    AddFilter();
                }
                for (int j = 0; j < alTagLevel3Array.Count; j++)
                {
                    RowNumber = RowNumber + 1;
                    string sValue = ((string[,])alTagLevel3Array[j])[0, 4]; // Get value from array
                    DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (RowNumber).ToString());
                    ddl.SelectedValue = sValue;
                    FilterTagname(ddl.SelectedValue, RowNumber);
                    AddFilter();
                }
                for (int j = 0; j < alTagnameArray.Count; j++)
                {
                    RowNumber = RowNumber + 1;
                    string sValue = ((string[,])alTagnameArray[j])[0, 4]; // Get value from array
                    DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (RowNumber).ToString());
                    ddl.SelectedValue = sValue;
                    PopulateTagLabels(ddl.SelectedValue, RowNumber);
                    AddFilter();
                }
                alTagLevel1Array.Clear();
                alTagLevel2Array.Clear();
                alTagLevel3Array.Clear();
                alTagnameArray.Clear();
            }
        }

        protected void Page_PreRender(Object sender, EventArgs e)
        {
            DataTable dt = (DataTable)Session["PageStringTable"];
            if (dt != null && tblGridFilter.Rows.Count > 0)
            {
                for (int i = 1; i < tblGridFilter.Rows[0].Cells.Count; i++) // Skip the first column
                {
                    foreach (Control cControl in tblGridFilter.Rows[0].Cells[i].Controls)
                    {
                        if (cControl is LiteralControl)
                        {
                            LiteralControl lc = (LiteralControl)cControl;
                            string headText = lc.Text;
                            foreach (DataRow row in dt.Rows)
                            {
                                if (!string.IsNullOrEmpty(row["AltText"].ToString()))
                                {
                                    string rowText = (row["Text"] != null) ? row["Text"].ToString() : "";
                                    if (headText == rowText && row["Type"].ToString().StartsWith("c")) lc.Text = row["AltText"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            if (AdvFilterFlag)
            {
                SetAdvFilterValues();
            }
            else
            {
                SetBasicFilterValues();
                ibSQLFilter.Attributes.Clear();
            }
            //ibClearFilter.Attributes.Add("onclick", "return confirm('" + lblClearFilter.Text + "')");
            ibSQLFilter.Attributes.Add("onclick", "return confirm('" + lblSwitchFilter.Text + "')");

            if (!string.IsNullOrEmpty(sMissingTagnames) && sMissingTagnames.Length > 0)
            {
                sMissingTagnames = sMissingTagnames.Remove(sMissingTagnames.Length - 1, 1); // remove the last ","
                                                                                            //ContentHelper.SetMainContent("[" + DateTime.Now.ToString() + "] " + string.Format(PCSTranslation.GetMessage("ErrAPIGravMissing"), sMissingTagnames));
                AppError.LogError("TagFilter.master:Page_PreRender", string.Format(PCSTranslation.GetMessage("ErrTagnameMissing"), sMissingTagnames));
            }
            if (!string.IsNullOrEmpty(sMissingTagLevel1) && sMissingTagLevel1.Length > 0)
            {
                sMissingTagLevel1 = sMissingTagLevel1.Remove(sMissingTagLevel1.Length - 1, 1); // remove the last ","
                AppError.LogError("TagFilter.master:Page_PreRender", string.Format(PCSTranslation.GetMessage("ErrTagLevel1Missing"), sMissingTagLevel1));
            }
            if (!string.IsNullOrEmpty(sMissingTagLevel2) && sMissingTagLevel2.Length > 0)
            {
                sMissingTagLevel2 = sMissingTagLevel2.Remove(sMissingTagLevel2.Length - 1, 1); // remove the last ","
                AppError.LogError("TagFilter.master:Page_PreRender", string.Format(PCSTranslation.GetMessage("ErrTagLevel2Missing"), sMissingTagLevel2));
            }
            if (!string.IsNullOrEmpty(sMissingTagLevel3) && sMissingTagLevel3.Length > 0)
            {
                sMissingTagLevel3 = sMissingTagLevel3.Remove(sMissingTagLevel3.Length - 1, 1); // remove the last ","
                AppError.LogError("TagFilter.master:Page_PreRender", string.Format(PCSTranslation.GetMessage("ErrTagLevel3Missing"), sMissingTagLevel3));
            }
        }

        /// <summary>
        /// Creates a where clause and then runs binddata.  Runs when the user clicks "Retrieve from database"
        /// </summary>
        protected void ibAddFilter_Click(object sender, ImageClickEventArgs e)
        {
            if (sFileName == "ThreadInfo") ThreadOrderClearUser();
            Session["TagFilterUpdate"] = "true";
            Session["QuickFilter"] = "";
            AddFilter();
        }

        private void AddFilter()
        {
            int iCols = tblGridFilter.Rows[0].Cells.Count;
            string[] arrFilterID = { "" };
            string[] strArray = new string[iCols];
            ArrayList alArray = new ArrayList();
            sWhere = string.Empty;
            sTagnames = string.Empty;
            sTagLevel1 = string.Empty;
            sTagLevel2 = string.Empty;
            sTagLevel3 = string.Empty;
            string sAndOr = string.Empty;
            //string sAnd = string.Empty;

            for (int i = 1; i < tblGridFilter.Rows.Count; i++) // Skip the header row
            {
                string sRow = string.Empty;
                string sAnd = string.Empty;

                for (int j = 1; j < iCols; j++) // Skip the first column
                {
                    string sOperator = string.Empty;
                    string sValue = string.Empty;
                    string sColumn = string.Empty;
                    bool bDateTime = false;

                    DropDownList ddlControl = new DropDownList();
                    foreach (Control cControl in tblGridFilter.Rows[i].Cells[j].Controls)
                    {
                        bDateTime = false;
                        // DropDownList first
                        if (cControl is DropDownList)
                        {
                            ddlControl = (DropDownList)cControl;
                            arrFilterID = ddlControl.ID.Split("@".ToCharArray());
                            sOperator = ddlControl.SelectedValue;
                            sColumn = arrFilterID[1];
                        }
                        // TextBox second
                        else if (cControl is TextBox)
                        {
                            string[] sParmID = { "@TableName", "@ColumnName" };
                            string[] sParmValue = { Session["DisplayView"].ToString(), sColumn };
                            string error = string.Empty;
                            TextBox tbControl = (TextBox)cControl;
                            sValue = tbControl.Text.Trim();
                            sValue = sValue.Replace("'", "''");

                            if ((sValue.Length > 0) && string.IsNullOrEmpty(sOperator))
                            {
                                // If text has a value and there is no operator default to the first operator
                                ddlControl.SelectedIndex = 1;
                                sOperator = ddlControl.SelectedValue;
                            }

                            if (!string.IsNullOrEmpty(sOperator))
                            {
                                // Check the column datatype
                                Dictionary<string, DataColumn> columns = PCSTranslation.ColumnDefinitions;
                                if (columns.ContainsKey(sColumn.ToLower()) && columns[sColumn.ToLower()].DataType == typeof(DateTime))
                                    bDateTime = true;

                                DateTime dt = new DateTime();
                                if (bDateTime)
                                {
                                    try
                                    {
                                        dt = Convert.ToDateTime(sValue);
                                        if (dt.TimeOfDay.Ticks == 0) sValue = dt.ToString("MM/dd/yyyy");
                                        else sValue = dt.ToString("MM/dd/yyyy HH:mm");
                                    }
                                    catch
                                    {
                                        if (sValue != string.Empty)
                                        {
                                            LiteralControl lc = (LiteralControl)tblGridFilter.Rows[0].Cells[j].Controls[0];
                                            string[] sArr = { sValue, lc.Text };
                                            AppError.LogError("LMSGridViewFilter:ibAddFilter_Click", string.Format(PCSTranslation.GetMessage("DATE1"), sArr));
                                            return;
                                        }
                                    }
                                }

                                if (columns.ContainsKey(sColumn.ToLower()) && columns[sColumn.ToLower()].DataType != typeof(DateTime) && columns[sColumn.ToLower()].DataType != typeof(string))
                                {
                                    double dblValue;
                                    if ((double.TryParse(sValue, out dblValue) == false) && (sValue != string.Empty))
                                    {
                                        LiteralControl lc = (LiteralControl)tblGridFilter.Rows[0].Cells[j].Controls[0];
                                        string[] sArr = { sValue, lc.Text };
                                        AppError.LogError("LMSGridViewFilter:ibAddFilter_Click", string.Format(PCSTranslation.GetMessage("NUMBER1"), sArr));
                                        return;
                                    }
                                }

                                switch (sOperator)
                                {
                                    case "like":
                                        sValue = "'%" + sValue + "%'";
                                        break;
                                    case "not like":
                                        sValue = "'%" + sValue + "%'";
                                        break;
                                    case "=":
                                        {
                                            // Check the column datatype
                                            if (columns.ContainsKey(sColumn.ToLower()) && columns[sColumn.ToLower()].DataType == typeof(DateTime)) bDateTime = true;

                                            if (sValue.Length == 0)
                                                if (string.IsNullOrEmpty(sValue) && bDateTime) sOperator = "IS NULL";
                                                else
                                                    if (columns[sColumn.ToLower()].DataType != typeof(DateTime) && columns[sColumn.ToLower()].DataType != typeof(string) && string.IsNullOrEmpty(sValue))
                                                    // Its a number
                                                    sOperator = "IS NULL";
                                                else
                                                    sValue = "'" + sValue + "'";
                                            else if (bDateTime)
                                            {
                                                if (sValue != string.Empty)
                                                    // If the datetime value is not empty change the operator to a CAST
                                                    sOperator = "cast(convert(varchar(10), [" + sColumn + "], 101) as datetime) = '" + sValue + "'";
                                                else
                                                    // If the datetime value is empty change the operator value to IS NULL
                                                    sOperator = "[" + sColumn + "] IS NULL";
                                                sValue = string.Empty;
                                                sColumn = string.Empty;
                                            }
                                            else sValue = "'" + sValue + "'";
                                        }
                                        break;
                                    default:
                                        if (sValue.Length == 0)
                                        {
                                            if (columns.ContainsKey(sColumn.ToLower()) && columns[sColumn.ToLower()].DataType == typeof(DateTime)) bDateTime = true;

                                            if (string.IsNullOrEmpty(sValue) && bDateTime) sOperator = "IS NOT NULL";
                                            else sValue = "'" + sValue + "'";
                                        }
                                        else
                                            if (!string.IsNullOrEmpty(sOperator)) sValue = "'" + sValue + "'";
                                        break;
                                }// switch
                            }
                            else
                            {
                                // sOperator is null or empty so set the textbox to blank
                                tbControl = (TextBox)cControl;
                                tbControl.Text = string.Empty;
                            }
                        }// TextBox
                    }// foreach

                    // Finished with the controls in the cell.  Now combine them
                    if (!string.IsNullOrEmpty(sOperator))
                    {
                        if (!string.IsNullOrEmpty(sColumn)) sColumn = "[" + sColumn + "]";
                        sRow = sRow + sAnd + "(" + sColumn + " " + sOperator + " " + sValue + ")";
                        sAnd = " and ";
                    }
                } // End of column loop

                // See if the first column is a dropdown
                if (!string.IsNullOrEmpty(sRow))
                {
                    if (sWhere.Length > 0)
                    {
                        foreach (Control cControl in tblGridFilter.Rows[i].Cells[0].Controls)
                        {
                            if (cControl is DropDownList)
                            {
                                DropDownList ddlControl = new DropDownList();
                                ddlControl = (DropDownList)cControl;
                                sAndOr = " " + ddlControl.SelectedValue + " ";
                            }
                        }
                    }
                    sWhere = sWhere + sAndOr + "(" + sRow + ")";
                }
            } // End of row loop

            if (sWhere.Length > 0) sWhere = "(" + sWhere + ") ";
            sTagnames = GetTagnames();
            if (string.IsNullOrEmpty(sTagnames))
            {
                sTagLevel3 = GetTagLevel3();
                if (string.IsNullOrEmpty(sTagLevel3))
                {
                    sTagLevel2 = GetTagLevel2();
                    if (string.IsNullOrEmpty(sTagLevel2))
                        sTagLevel1 = GetTagLevel1();
                }
            }
            BindFilterData();
        }

        /// <summary>
        /// Runs when the user clicks "Clear Filter"
        /// </summary>
        protected void ibClearFilter_Click(object sender, ImageClickEventArgs e)
        {
            if (sFileName == "ThreadInfo") ThreadOrderClearUser();
            int iCols = tblGridFilter.Rows[0].Cells.Count;
            for (int i = 1; i < tblGridFilter.Rows.Count; i++) // Skip the header row
            {
                for (int j = 1; j < iCols; j++) // Skip the first column
                {
                    foreach (Control cControl in tblGridFilter.Rows[i].Cells[j].Controls)
                    {
                        if (cControl is TextBox)
                        {
                            TextBox tbControl = (TextBox)cControl;
                            tbControl.Text = string.Empty;
                        }
                        else if (cControl is DropDownList)
                        {
                            DropDownList ddlControl = (DropDownList)cControl;
                            ddlControl.SelectedIndex = 0;
                        }
                    }
                }
            }

            // Clear quick filter and awitch back to standard filter view
            if (AdvFilterFlag)
                ibSQLFilter_Click(sender, e);

            sWhere = string.Empty;
            BindFilterData();
        }

        protected void ApplyQuickFilter(object sender, EventArgs e)
        {
            if (sFileName == "ThreadInfo") ThreadOrderClearUser();
            string sql = string.Empty;

            if (QuickFilterBox.Text.Trim().Length > 0)
            {
                string condition = string.Format(" like '%{0}%'", QuickFilterBox.Text.Trim());
                DateTime dt;
                if (!DateTime.TryParse(QuickFilterBox.Text.Trim(), out dt))
                    dt = DateTime.MinValue; // not parseable as a datetime
                double dbl;
                if (!double.TryParse(QuickFilterBox.Text.Trim(), out dbl))
                    dbl = double.NaN; // not parseable as a double or int

                Dictionary<string, int> visibleColumns = Session["VisbleColumnsHash"] as Dictionary<string, int>;
                Dictionary<string, DataColumn> columns = PCSTranslation.ColumnDefinitions;
                Dictionary<string, int> QuickFilterExcludedColumns = (Page.Session["QuickFilterExcludedColumns"] != null) ? (Dictionary<string, int>)Page.Session["QuickFilterExcludedColumns"] : new Dictionary<string, int>();
                foreach (DataColumn col in columns.Values)
                {
                    if (visibleColumns.ContainsKey(col.ColumnName.ToLower()) && !QuickFilterExcludedColumns.ContainsKey(col.ColumnName.ToLower()))
                    {
                        if (col.DataType == typeof(string))
                            sql += string.Format("{0}([{1}]{2})", (sql.Length > 0) ? " OR " : "", col.ColumnName, condition);
                        //else if (dt != DateTime.MinValue && col.DataType == typeof(DateTime))
                        //    sql += string.Format("{0}([{1}]>='{2}' AND [{1}]<'{3}')", (sql.Length > 0) ? " OR " : "", col.ColumnName, dt.ToString("MM/dd/yyyy"), dt.AddDays(1.0).ToString("MM/dd/yyyy"));
                        else if (!double.IsNaN(dbl) && (col.DataType == typeof(double) || col.DataType == typeof(float)))
                            sql += string.Format("{0}([{1}]={2})", (sql.Length > 0) ? " OR " : "", col.ColumnName, dbl.ToString());
                        else if (!double.IsNaN(dbl) && (col.DataType == typeof(Int16) || col.DataType == typeof(int) || col.DataType == typeof(Int64)))
                            sql += string.Format("{0}([{1}]={2})", (sql.Length > 0) ? " OR " : "", col.ColumnName, ((int)dbl).ToString());
                    }
                }
                sql = "(" + sql + ")";
            }

            if (sql.Length > 0)
            {
                if (BasePage._Session("QuickFilter").Length == 0)
                    Session["SavedFilter"] = Session["Filter"];
                Session["QuickFilter"] = "True";
            }

            sWhere = sql;
            UpdatePanel_Filter.Update();
            BindFilterData();

            // If the filter panel is open, then close it as part of running a quick filter
            CollapsiblePanelExtender1.Collapsed = true;
            CollapsiblePanelExtender1.ClientState = "true";
        }

        /// <summary>
        /// Runs when the user clicks "SQL Filter"
        /// </summary>
        protected void ibSQLFilter_Click(object sender, ImageClickEventArgs e)
        {
            // Clear the quick filter box when switching filter modes
            QuickFilterBox.Text = "";
            Session["QuickFilter"] = "";

            if (AdvFilterFlag)
            {
                // if moving from advanced to basic then clear everything out
                int iCols = tblGridFilter.Rows[0].Cells.Count;
                for (int i = 1; i < tblGridFilter.Rows.Count; i++) // Skip the header row
                {
                    for (int j = 1; j < iCols; j++) // Skip the first column
                    {
                        foreach (Control cControl in tblGridFilter.Rows[i].Cells[j].Controls)
                        {
                            if (cControl is TextBox)
                            {
                                TextBox tbControl = (TextBox)cControl;
                                tbControl.Text = string.Empty;
                            }
                            else if (cControl is DropDownList)
                            {
                                DropDownList ddlControl = (DropDownList)cControl;
                                ddlControl.SelectedIndex = 0;
                            }
                        }
                    }
                }
                tbxSQL.Text = string.Empty;
                Session["Filter"] = string.Empty;
                Session[Session["CurPageName"].ToString() + "Filter"] = string.Empty;
                Session["AdvFilterFlag"] = "False";
                Session[Session["CurPageName"].ToString() + "AdvFilterFlag"] = "False";
                SetBasicFilterValues();
            }
            else
            {
                Session["AdvFilterFlag"] = "True";
                Session[Session["CurPageName"].ToString() + "AdvFilterFlag"] = "True";
                SetAdvFilterValues();
            }
        }

        protected void OpenPopUp(WebControl opener, string PagePath, string windowName, int width, int height)
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

        /// <summary>
        /// Creates the initial filter table from the dataset
        /// </summary>
        /// <param name="dtContent">Data table content of the filter</param>
        /// <param name="Clause">Where clause</param>
        public void CreateFilterTable(DataTable dtContent, string Clause)
        {
            //if (Request.QueryString["src"] != null || Request.QueryString["src"] == "iwaf")
            //{
            //    Session["Filter"] = string.Empty;
            //    Clause = string.Empty;
            //    SASWrapper.ExecuteQuery(DatabaseName, "UPDATE AppsQueryPreference SET Filter = '' WHERE PageID = '" + CurPageName + "' AND UserID = '" + UserID + "'");
            //}

            TableRow tableRow;
            TableCell column;
            Table TableHeader;
            string sColumn;

            int iNumRows = 0;
            int iMaxRows = 0;

            if (tblGridFilter.Rows.Count <= 0)
            {
                ArrayList alArray = new ArrayList();
                // Parse the where clause
                alArray = GetWhereClause(BasePage._Session("QuickFilter").Length > 0 ? BasePage._Session("SavedFilter") : Clause);

                // check to see if the where clause will fit nicely into the table
                int iColumnCount = 0;
                for (int j = 0; j < alArray.Count; j++)
                {
                    string[,] strWhereArray = (string[,])alArray[j];
                    iNumRows = int.Parse(strWhereArray[0, 0]) + 1;
                    iMaxRows = Math.Max(iNumRows, iMaxRows);
                    foreach (DataColumn dataCol in dtContent.Columns)
                    {
                        string[,] strArray = (string[,])alArray[j];
                        if (strArray[0, 2] == "[" + dataCol.ToString() + "]" || strArray[0, 2] == dataCol.ToString())
                        {
                            iColumnCount = iColumnCount + 1;
                            break;
                        }
                    }
                }

                if (ViewState["numRows"] == null)
                    ViewState["numRows"] = Math.Max(1, iMaxRows) + 1;

                //// if the values are equal then there was a matching table column found for each column in the where clause and if not
                //// set the advanced filter flag on so the where clause is shown in the text box so the user can see what is in the filter
                if (iColumnCount != alArray.Count && Request.QueryString["Filter"] == null)
                {
                    Session["Filter"] = string.Empty;
                    Clause = string.Empty;
                    alArray = GetWhereClause(BasePage._Session("QuickFilter").Length > 0 ? BasePage._Session("SavedFilter") : Clause);
                    SASWrapper.ExecuteQuery(DatabaseName, "UPDATE AppsQueryPreference SET Filter = '' WHERE PageID = '" + CurPageName + "' AND UserID = '" + UserID + "'");
                }
                //{
                //  Session["AdvFilterFlag"] = "True";
                //  Session[Session["CurPageName"].ToString() + "AdvFilterFlag"] = "True";
                //  SetAdvFilterValues();
                //}

                // Create a header table
                TableHeader = new Table() { EnableViewState = true };
                TableRow headerRow = new TableRow();

                // Add the column headers to a new row.
                headerRow = new TableRow();

                //Add the first column to the table
                TableHeaderCell header = new TableHeaderCell();

                header.Controls.Add(new LiteralControl(""));
                headerRow.Cells.Add(header);

                foreach (DataColumn dataCol in dtContent.Columns)
                {
                    sColumn = dataCol.Caption;
                    header = new TableHeaderCell();
                    if (((BasePage)Page).UseAltLanguage && !((BasePage)Page).WrapAltLanguage) header.Wrap = false;
                    header.Controls.Add(new LiteralControl(sColumn));
                    // add scope="col" attribute to each header cell
                    header.Attributes.Add("scope", "col");
                    headerRow.Cells.Add(header);
                }

                // Add the column headers to the first row of the header table.
                tblGridFilter.Rows.AddAt(0, headerRow);

                // Default the first column to the linkbuttons
                sColumn = string.Empty;
                ImageButton ib = new ImageButton();
                ib.ImageUrl = "~/Images/add.gif";
                ib.ID = "ib@Add";
                ib.Click += new ImageClickEventHandler(AddRow);
                ib.BorderWidth = 3;
                ib.BorderColor = System.Drawing.ColorTranslator.FromHtml(ColorManager.ColorGridHeader);
                Control cControl = new Control();
                cControl = ib;
                sColumn = string.Empty;
                ib = new ImageButton();
                ib.ImageUrl = "~/Images/remove.gif";
                ib.ID = "ib@Remove";
                ib.Click += new ImageClickEventHandler(RemoveRow);
                ib.BorderWidth = 3;
                ib.BorderColor = System.Drawing.ColorTranslator.FromHtml(ColorManager.ColorGridHeader);
                Control cControl2 = new Control();
                cControl2 = ib;
                Table tblAddRemoveButtons = new Table();
                tblAddRemoveButtons.BorderWidth = Unit.Pixel(0);
                tblAddRemoveButtons.CellPadding = 0;
                tblAddRemoveButtons.CellSpacing = 6;
                TableHeaderRow tr1 = new TableHeaderRow();
                TableHeaderCell tc1 = new TableHeaderCell();
                TableHeaderCell tc2 = new TableHeaderCell();
                tc1.Controls.Add(cControl);
                tc2.Controls.Add(cControl2);
                tr1.Controls.Add(tc1);
                tr1.Controls.Add(tc2);
                tblAddRemoveButtons.Controls.Add(tr1);
                cControl = tblAddRemoveButtons;

                for (int i = 1; i < int.Parse(ViewState["numRows"].ToString()); i++)
                {
                    // Add a row of controls after the header
                    tableRow = new TableRow();

                    // Add the first column to the table
                    column = new TableHeaderCell();
                    column.BorderWidth = 1;
                    column.BorderColor = System.Drawing.ColorTranslator.FromHtml(ColorManager.ColorGridHeader);
                    column.Controls.Add(cControl);
                    column.Attributes.Add("scope", "row");
                    tableRow.Cells.Add(column);

                    // Now add the rest of the columns
                    foreach (DataColumn dataCol in dtContent.Columns)
                    {
                        sColumn = dataCol.ToString();
                        string sOperator = string.Empty;
                        string sValue = string.Empty;
                        int j = 0;
                        for (j = 0; j < alArray.Count; j++)
                        {
                            string[,] strArray = (string[,])alArray[j];
                            //if (strArray[0, 1].ToLower() == "or" || strArray[0, 1] == "AND")
                            //    sAndOr = strArray[0, 1];
                            // The row number of the table and column name need to match the array row and column name
                            if (Convert.ToInt32(strArray[0, 0]) == i - 1 && strArray[0, 2] == "[" + sColumn + "]")
                            {
                                sOperator = strArray[0, 3]; // Get operator from array
                                sValue = strArray[0, 4]; // Get value from array
                                break;
                            }
                        }
                        DropDownList ddlOperators = new DropDownList();
                        ddlOperators.ID = "ddl" + i + "@" + sColumn;
                        ddlOperators.Items.Add(string.Empty);
                        // Show "like" and "not like" operator when the datatype is string
                        if (dataCol.DataType == typeof(string)) ddlOperators.Items.Add("like");
                        if (dataCol.DataType == typeof(string)) ddlOperators.Items.Add("not like");
                        ddlOperators.Items.Add("=");
                        ddlOperators.Items.Add("<>");
                        ddlOperators.Items.Add("<");
                        ddlOperators.Items.Add("<=");
                        ddlOperators.Items.Add(">");
                        ddlOperators.Items.Add(">=");
                        TextBox tbFilter = new TextBox();
                        tbFilter.ID = "tb" + i + "@" + sColumn;
                        tbFilter.Columns = 8;
                        if (!string.IsNullOrEmpty(sOperator)) ddlOperators.SelectedValue = sOperator;
                        column = new TableCell();
                        column.BorderWidth = 1;
                        column.BorderColor = System.Drawing.ColorTranslator.FromHtml(ColorManager.ColorGridHeader);
                        column.Wrap = false;
                        column.Controls.Add(ddlOperators);
                        tableRow.Cells.Add(column);
                        if (!string.IsNullOrEmpty(sValue)) tbFilter.Text = sValue;
                        column.Controls.Add(tbFilter);

                        if ((dataCol.DataType != typeof(string)) && (dataCol.DataType != typeof(DateTime)) && (dataCol.DataType != typeof(Guid)))
                            column.Controls.Add(new AjaxControlToolkit.FilteredTextBoxExtender { ID = "ftbe" + i + "@" + sColumn, TargetControlID = tbFilter.ID, FilterType = AjaxControlToolkit.FilterTypes.Custom | AjaxControlToolkit.FilterTypes.Numbers, ValidChars = "-." });

                        if (dataCol.DataType == typeof(DateTime) && !(dataCol.ToString().ToLower().Contains("time")))
                        {
                            column.Controls.Add(new AjaxControlToolkit.FilteredTextBoxExtender { ID = "ftbe" + i + "@" + sColumn, TargetControlID = tbFilter.ID, FilterType = AjaxControlToolkit.FilterTypes.Custom | AjaxControlToolkit.FilterTypes.Numbers, ValidChars = "/: " });
                            ImageButton ibCalendar = new ImageButton();
                            ibCalendar.ID = "ib" + i + "@" + sColumn;
                            ibCalendar.ImageUrl = "~/Images/calendar.png";
                            column.Controls.Add(ibCalendar);
                            column.Controls.Add(new AjaxControlToolkit.CalendarExtender { ID = "cal" + i + "@" + sColumn, PopupButtonID = ibCalendar.ID, TargetControlID = tbFilter.ID });
                        }
                        tableRow.Cells.Add(column);
                        if (alArray.Count > 0 && !string.IsNullOrEmpty(sOperator)) alArray.RemoveAt(j);
                    }
                    // Finished with all the columns now add the row to the table.
                    tblGridFilter.Rows.AddAt(i, tableRow);

                    DropDownList ddlAndOr = new DropDownList();
                    ddlAndOr.ID = "ddl" + i + "@AndOr";
                    ddlAndOr.Items.Add("OR");
                    ddlAndOr.Items.Add("AND");
                    if (alArray.Count > 0)
                    {
                        string[,] strTmpArray = (string[,])alArray[0];
                        string sAndOr = strTmpArray[0, 1];
                        if (!string.IsNullOrEmpty(sAndOr)) ddlAndOr.SelectedValue = sAndOr;
                    }
                    cControl = ddlAndOr;
                }
            }
        }

        /// <summary>
        /// Parses the where clause string into an array
        /// </summary>
        /// <param name="Clause">Where clause</param>
        /// <returns>ArrayList</returns>
        private ArrayList GetWhereClause(string Clause)
        {
            if (!string.IsNullOrEmpty(Clause))
            {
                ArrayList alArray = new ArrayList();
                string values = Clause;
                string sColumn = string.Empty;
                string sColumnTemp = string.Empty;
                string sOperator = string.Empty;
                string sValue = string.Empty;
                string sConjuction = string.Empty;
                string[] sArr = new string[] { "", "", "", "", "", "", "", "", "", "" };
                int iColPos = 0;
                int iRow = 0;
                Clause = Clause.Replace("] IS NULL )", "] IS 'NULL' )");

                MatchCollection matches = Regex.Matches(Clause, @"[^\x27]*\x27((?:[^\x27]|\x27\x27)*)\x27");
                foreach (Match m in matches)
                {
                    // Get the value
                    sValue = m.Groups[1].Value;
                    string sClause = m.Value;

                    // strip all of the unwanted characters from the beginning of the clause
                    sClause = sClause.TrimStart(new Char[] { ' ', '(', ')' });

                    string[] strArray = sClause.Split(" ".ToCharArray());

                    // Is there an "AND" or "OR".  Then strip it out of the beginning of the clause
                    if ((strArray[0].ToLower() == "and" || strArray[0].ToLower() == "or"))
                    {
                        sConjuction = strArray[0];
                        string strToTrim = "ORor ANDand";
                        sClause = sClause.TrimStart(strToTrim.ToCharArray());

                        // An "or" or an uppercase "AND" always indicate a new row
                        if (strArray[0].ToLower() == "or" || strArray[0] == "AND") iRow = iRow + 1;
                    }

                    // Strip all of the unwanted characters from the beginning of the clause
                    sClause = sClause.TrimStart(new Char[] { '(', ' ' });

                    // Test the clause to see if it is a cast convert datetime.  If it is then use this method to get the column name
                    string sCastConvert = sClause.Replace(" ", string.Empty);
                    if (!string.IsNullOrEmpty(sCastConvert) && sCastConvert.ToLower().IndexOf("cast(convert", 0) >= 0)
                    {
                        // Split at the comma and get the column name
                        string[] strCastConvertArray = sCastConvert.Split(",".ToCharArray());
                        sColumn = strCastConvertArray[1];

                        // Find the indexes
                        int iBeginningIndex = sCastConvert.LastIndexOf(@")");
                        int iEndingindex = sCastConvert.IndexOf(@"'");

                        // Get the operator
                        if (string.IsNullOrEmpty(sOperator)) sOperator = sCastConvert.Substring(iBeginningIndex + 1, iEndingindex - iBeginningIndex - 1);
                    }
                    else
                    {
                        // It is not a cast convert so now get the column name and the operator from the clause
                        for (int i = 0; i < sClause.Length; i++)
                        {
                            char cChar = Convert.ToChar(sClause[i].ToString());

                            // The column name
                            if (string.IsNullOrEmpty(sOperator) && string.IsNullOrEmpty(sColumn) && ((cChar != '=' && cChar != '>' && cChar != '<' && cChar != ']')))
                            {
                                // Build the temporary column name and track the column position
                                sColumnTemp = sColumnTemp + cChar;
                                iColPos = i;
                            }
                            else if (!string.IsNullOrEmpty(sColumnTemp)) // End of column name reached
                            {
                                sColumn = sColumnTemp;
                                if (cChar == ']')
                                {
                                    iColPos = i;
                                    sColumn = sColumn + cChar;
                                }
                                sColumnTemp = string.Empty;
                            }

                            // Start of the value position
                            if (!string.IsNullOrEmpty(sColumn) && (cChar == '\''))
                            {
                                // Get the operator
                                if (string.IsNullOrEmpty(sOperator)) sOperator = sClause.Substring(iColPos + 1, i - iColPos - 1).Trim();
                                // Handle the datetime field
                                if (sOperator.ToLower() == "is")
                                {
                                    sOperator = "=";
                                    sValue = string.Empty;
                                }
                            }
                        }
                    }
                    // Do final adjustments before moving them into the array
                    if (!string.IsNullOrEmpty(sOperator) && sOperator.ToLower().IndexOf("like", 0) >= 0)
                    {
                        char[] chArr = { '%' };
                        sValue = sValue.TrimStart(chArr);
                        sValue = sValue.TrimEnd(chArr);
                    }
                    sValue = sValue.Replace("''", "'");

                    string[,] strArr = new string[1, 5];
                    strArr[0, 0] = iRow.ToString();
                    strArr[0, 1] = sConjuction;
                    strArr[0, 2] = sColumn;
                    strArr[0, 3] = sOperator;
                    strArr[0, 4] = sValue;
                    alArray.Add(strArr);
                    sColumn = string.Empty;
                    sOperator = string.Empty;
                    sValue = string.Empty;
                    if (iRow == 9) iRow = 0;
                } //foreach
                return alArray;
            }
            else return new ArrayList();
        }

        protected void ibRunSQLFilter_Click(object sender, ImageClickEventArgs e)
        {
            if (sFileName == "ThreadInfo") ThreadOrderClearUser();
            sWhere = tbxSQL.Text;
            BindFilterData();
            tblGridFilter.Rows.Clear();
            CreateFilterTable(dtFilterData, sWhere);
        }

        private void SetAdvFilterValues()
        {
            SetBasicFilterValues();

            //divSQLFilter.Visible = true;
            //divFilterTable.Visible = false;
            //ibAddFilter.Visible = false;
            //ibRunSQLFilter.Visible = true;
            //ibSQLFilter.ImageUrl = "~/Images/filter_sml.gif";
            //ibSQLFilter.ToolTip = "View Filter Table";
        }

        private void SetBasicFilterValues()
        {
            divSQLFilter.Visible = false;
            divFilterTable.Visible = true;
            ibAddFilter.Visible = true;
            ibRunSQLFilter.Visible = false;
            ibSQLFilter.ImageUrl = "~/Images/filter_SQL.gif";
            ibSQLFilter.ToolTip = "Enter SQL Clause";

            ibSQLFilter.Visible = false; // hide adv filter button
        }

        protected void OnFilterPanelStateChange(Object sender, EventArgs e)
        {
            if (CollapsiblePanelExtender1.ClientState == "false") // panel is about to open
            {
                // If there is a quick filter in effect, then clear the filter
                if (QuickFilterBox.Text.Length > 0)
                    ibClearFilter_Click(sender, null);
            }
        }

        protected void AddRow(object sender, EventArgs e)
        {
            //AddFilter();
            if (int.Parse(ViewState["numRows"].ToString()) < 11)
                ViewState["numRows"] = int.Parse(ViewState["numRows"].ToString()) + 1;
            sWhere = tbxSQL.Text;
            BindFilterData();
            tblGridFilter.Rows.Clear();
            CreateFilterTable(dtFilterData, sWhere);
        }

        protected void RemoveRow(object sender, EventArgs e)
        {
            if (int.Parse(ViewState["numRows"].ToString()) > 2)
                ViewState["numRows"] = int.Parse(ViewState["numRows"].ToString()) - 1;
            sWhere = tbxSQL.Text;
            BindFilterData();
            tblGridFilter.Rows.Clear();
            CreateFilterTable(dtFilterData, sWhere);
            //AddFilter();
        }

        protected void ibAdd_Click(object sender, ImageClickEventArgs e)
        {
            for (int i = 0; i < tblTagGridFilter.Rows.Count - 1; i++)
            {
                TableRow tbRow = (TableRow)tblTagGridFilter.FindControl("tr" + (i + 1).ToString());
                if (tbRow.Visible == false)
                {
                    tbRow.Visible = true;
                    return;
                }
            }
        }

        protected void ibRemove_Click(object sender, ImageClickEventArgs e)
        {
            for (int i = tblTagGridFilter.Rows.Count - 1; i > 1; i--)
            {
                TableRow tbRow = (TableRow)tblTagGridFilter.FindControl("tr" + (i).ToString());
                if (tbRow.Visible == true)
                {
                    DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (i).ToString());
                    ddl.Items.Clear();
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (i).ToString());
                    ddl.Items.Clear();
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (i).ToString());
                    ddl.Items.Clear();
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                    ddl.Items.Clear();
                    Label lblDevice = (Label)tblTagGridFilter.FindControl("lblDevice" + (i).ToString());
                    lblDevice.Text = string.Empty;
                    Label lblDescription = (Label)tblTagGridFilter.FindControl("lblDescription" + (i).ToString());
                    lblDescription.Text = string.Empty;
                    Label lblType = (Label)tblTagGridFilter.FindControl("lblType" + (i).ToString());
                    lblType.Text = string.Empty;
                    Label lblUOM = (Label)tblTagGridFilter.FindControl("lblUOM" + (i).ToString());
                    lblUOM.Text = string.Empty;

                    string sql = string.Format("SELECT * FROM v_TagLevel1_lb");
                    DataSet ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {

                        DataTable dttag = ds.Tables[0];
                        foreach (DataRow r in dttag.Rows)
                        {
                            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (i).ToString());
                            ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                        }
                    }

                    sql = string.Format("SELECT * FROM v_TagLevel2_lb");
                    ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {

                        DataTable dttag = ds.Tables[0];
                        foreach (DataRow r in dttag.Rows)
                        {
                            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (i).ToString());
                            ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                        }
                    }

                    sql = string.Format("SELECT * FROM v_TagLevel3_lb");
                    ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {

                        DataTable dttag = ds.Tables[0];
                        foreach (DataRow r in dttag.Rows)
                        {
                            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (i).ToString());
                            ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                        }
                    }

                    sql = string.Format("SELECT * FROM v_Tagname_lb");
                    ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {

                        DataTable dttag = ds.Tables[0];
                        foreach (DataRow r in dttag.Rows)
                        {
                            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                            ddl.Items.Add(new ListItem { Value = r["DisplayText"].ToString(), Text = r["DisplayText"].ToString() });
                        }
                    }

                    tbRow.Visible = false;
                    return;
                }
            }
        }

        protected void ddlTagLevel1_1Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_1.SelectedValue, 1);
        }

        protected void ddlTagLevel1_2Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_2.SelectedValue, 2);
        }

        protected void ddlTagLevel1_3Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_3.SelectedValue, 3);
        }

        protected void ddlTagLevel1_4Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_4.SelectedValue, 4);
        }

        protected void ddlTagLevel1_5Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_5.SelectedValue, 5);
        }

        protected void ddlTagLevel1_6Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_6.SelectedValue, 6);
        }

        protected void ddlTagLevel1_7Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_7.SelectedValue, 7);
        }

        protected void ddlTagLevel1_8Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_8.SelectedValue, 8);
        }

        protected void ddlTagLevel1_9Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_9.SelectedValue, 9);
        }

        protected void ddlTagLevel1_10Changed(object sender, EventArgs e)
        {
            FilterTagLevel2TagLevel3Tagname(ddlTagLevel1_10.SelectedValue, 10);
        }

        private void FilterTagLevel2TagLevel3Tagname(string value, int index)
        {
            // Ensure this row is visible and the row is selected
            TableRow tbRow = (TableRow)tblTagGridFilter.FindControl("tr" + (index).ToString());
            tbRow.Visible = true;
            if (((BasePage)Page).CanInsert && sFileName != "ThreadInfo")
            {
                TableCell tbCell = (TableCell)tblTagGridFilter.FindControl("tcNew" + (index).ToString());
                tbCell.Visible = true;
            }
            DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (index).ToString());
            foreach (ListItem lsItem in ddl.Items)
            {
                if (lsItem.Value.ToLower() == value.ToLower())
                    ddl.SelectedValue = lsItem.Text;
            }

            // Default button to invisible
            ImageButton ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
            ib.Visible = false;

            string error = string.Empty;

            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (index).ToString());
            ddl.Items.Clear();
            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (index).ToString());
            ddl.Items.Clear();
            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (index).ToString());
            ddl.Items.Clear();
            Label lblDevice = (Label)tblTagGridFilter.FindControl("lblDevice" + (index).ToString());
            lblDevice.Text = string.Empty;
            Label lblDescription = (Label)tblTagGridFilter.FindControl("lblDescription" + (index).ToString());
            lblDescription.Text = string.Empty;
            Label lblType = (Label)tblTagGridFilter.FindControl("lblType" + (index).ToString());
            lblType.Text = string.Empty;
            Label lblUOM = (Label)tblTagGridFilter.FindControl("lblUOM" + (index).ToString());
            lblUOM.Text = string.Empty;

            DataSet dsWork = SASWrapper.QueryStoredProc_ResultSet("uspTAG_GetFilteredTagLevel2", new string[] { "@TagLevel1" }, new string[] { value }, DatabaseName, ref error);
            if (dsWork != null && dsWork.Tables.Count > 0)
            {
                if (dsWork.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = dsWork.Tables[0];
                    foreach (DataRow r in dt.Rows)
                    {
                        ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (index).ToString());
                        ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                    }

                    // Show create button
                    if (((BasePage)Page).CanInsert)
                    {
                        ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
                        ib.Visible = true;
                    }
                }
                else
                {
                    //If you made it here its because an area is in the querystring that does not exist in the table
                    sMissingTagLevel1 = sMissingTagLevel1 + value + ",";
                }
            }

            dsWork = SASWrapper.QueryStoredProc_ResultSet("uspTAG_GetFilteredTagLevel3", new string[] { "@TagLevel1" }, new string[] { value }, DatabaseName, ref error);
            if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
            {
                DataTable dt = dsWork.Tables[0];
                foreach (DataRow r in dt.Rows)
                {
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (index).ToString());
                    ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                }
            }

            dsWork = SASWrapper.QueryStoredProc_ResultSet("uspTAG_GetFilteredTagname", new string[] { "@TagLevel1" }, new string[] { value }, DatabaseName, ref error);
            if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
            {
                DataTable dt = dsWork.Tables[0];
                foreach (DataRow r in dt.Rows)
                {
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (index).ToString());
                    ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                }
            }
        }

        protected void ddlTagLevel2_1Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_1.SelectedValue, 1);
        }

        protected void ddlTagLevel2_2Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_2.SelectedValue, 2);
        }

        protected void ddlTagLevel2_3Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_3.SelectedValue, 3);
        }

        protected void ddlTagLevel2_4Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_4.SelectedValue, 4);
        }

        protected void ddlTagLevel2_5Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_5.SelectedValue, 5);
        }

        protected void ddlTagLevel2_6Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_6.SelectedValue, 6);
        }

        protected void ddlTagLevel2_7Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_7.SelectedValue, 7);
        }

        protected void ddlTagLevel2_8Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_8.SelectedValue, 8);
        }

        protected void ddlTagLevel2_9Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_9.SelectedValue, 9);
        }

        protected void ddlTagLevel2_10Changed(object sender, EventArgs e)
        {
            FilterTagLevel3Tagname(ddlTagLevel2_10.SelectedValue, 10);
        }

        private void FilterTagLevel3Tagname(string value, int index)
        {
            // Ensure this row is visible and the row is selected
            TableRow tbRow = (TableRow)tblTagGridFilter.FindControl("tr" + (index).ToString());
            tbRow.Visible = true;
            if (((BasePage)Page).CanInsert && sFileName != "ThreadInfo")
            {
                TableCell tbCell = (TableCell)tblTagGridFilter.FindControl("tcNew" + (index).ToString());
                tbCell.Visible = true;
            }
            DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (index).ToString());
            foreach (ListItem lsItem in ddl.Items)
            {
                if (lsItem.Value.ToLower() == value.ToLower())
                    ddl.SelectedValue = lsItem.Text;
            }

            // Default button to invisible
            ImageButton ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
            ib.Visible = false;

            string error = string.Empty;

            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (index).ToString());
            ddl.Items.Clear();
            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (index).ToString());
            ddl.Items.Clear();
            Label lblDevice = (Label)tblTagGridFilter.FindControl("lblDevice" + (index).ToString());
            lblDevice.Text = string.Empty;
            Label lblDescription = (Label)tblTagGridFilter.FindControl("lblDescription" + (index).ToString());
            lblDescription.Text = string.Empty;
            Label lblType = (Label)tblTagGridFilter.FindControl("lblType" + (index).ToString());
            lblType.Text = string.Empty;
            Label lblUOM = (Label)tblTagGridFilter.FindControl("lblUOM" + (index).ToString());
            lblUOM.Text = string.Empty;

            DataSet dsWork = SASWrapper.QueryStoredProc_ResultSet("uspTAG_GetFilteredTagLevel3", new string[] { "@TagLevel2" }, new string[] { value }, DatabaseName, ref error);
            if (dsWork != null && dsWork.Tables.Count > 0)
            {
                if (dsWork.Tables[0].Rows.Count > 1)
                {
                    string TagLevel1 = string.Empty;
                    DataTable dt = dsWork.Tables[0];
                    foreach (DataRow r in dt.Rows)
                    {
                        ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (index).ToString());
                        ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                        TagLevel1 = r["TagLevel1"].ToString();

                        // Show create button
                        if (((BasePage)Page).CanInsert)
                        {
                            ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
                            ib.Visible = true;
                        }

                    }

                    // Need to set the TagLevel1 selection (TagLevel1 better be unique!)
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (index).ToString());
                    ddl.SelectedValue = TagLevel1;
                }
                else
                {
                    //If you made it here its because a unit is in the querystring that does not exist in the table
                    sMissingTagLevel2 = sMissingTagLevel2 + value + ",";
                }
            }
            //if (dsWork.Tables.Count == 0)
            //{
            //    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (index).ToString());
            //    ddl.SelectedIndex = 0;
            //}

            dsWork = SASWrapper.QueryStoredProc_ResultSet("uspTAG_GetFilteredTagname", new string[] { "@TagLevel2" }, new string[] { value }, DatabaseName, ref error);
            if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
            {
                DataTable dt = dsWork.Tables[0];
                foreach (DataRow r in dt.Rows)
                {
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (index).ToString());
                    ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                }
            }
        }

        protected void ddlTagLevel3_1Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_1.SelectedValue, 1);
        }

        protected void ddlTagLevel3_2Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_2.SelectedValue, 2);
        }

        protected void ddlTagLevel3_3Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_3.SelectedValue, 3);
        }

        protected void ddlTagLevel3_4Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_4.SelectedValue, 4);
        }

        protected void ddlTagLevel3_5Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_5.SelectedValue, 5);
        }

        protected void ddlTagLevel3_6Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_6.SelectedValue, 6);
        }

        protected void ddlTagLevel3_7Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_7.SelectedValue, 7);
        }

        protected void ddlTagLevel3_8Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_8.SelectedValue, 8);
        }

        protected void ddlTagLevel3_9Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_9.SelectedValue, 9);
        }

        protected void ddlTagLevel3_10Changed(object sender, EventArgs e)
        {
            FilterTagname(ddlTagLevel3_10.SelectedValue, 10);
        }

        private void FilterTagname(string value, int index)
        {
            // Ensure this row is visible and the row is selected
            TableRow tbRow = (TableRow)tblTagGridFilter.FindControl("tr" + (index).ToString());
            tbRow.Visible = true;
            if (((BasePage)Page).CanInsert && sFileName != "ThreadInfo")
            {
                TableCell tbCell = (TableCell)tblTagGridFilter.FindControl("tcNew" + (index).ToString());
                tbCell.Visible = true;
            }
            DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (index).ToString());
            foreach (ListItem lsItem in ddl.Items)
            {
                if (lsItem.Value.ToLower() == value.ToLower())
                    ddl.SelectedValue = lsItem.Text;
            }

            // Default button to invisible
            ImageButton ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
            ib.Visible = false;

            string error = string.Empty;

            ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (index).ToString());
            ddl.Items.Clear();
            Label lblDevice = (Label)tblTagGridFilter.FindControl("lblDevice" + (index).ToString());
            lblDevice.Text = string.Empty;
            Label lblDescription = (Label)tblTagGridFilter.FindControl("lblDescription" + (index).ToString());
            lblDescription.Text = string.Empty;
            Label lblType = (Label)tblTagGridFilter.FindControl("lblType" + (index).ToString());
            lblType.Text = string.Empty;
            Label lblUOM = (Label)tblTagGridFilter.FindControl("lblUOM" + (index).ToString());
            lblUOM.Text = string.Empty;

            DataSet dsWork = SASWrapper.QueryStoredProc_ResultSet("uspTAG_GetFilteredTagname", new string[] { "@TagLevel3" }, new string[] { value }, DatabaseName, ref error);
            if (dsWork != null && dsWork.Tables.Count > 0)
            {
                if (dsWork.Tables[0].Rows.Count > 0)
                {
                    string TagLevel1 = string.Empty;
                    string TagLevel2 = string.Empty;
                    string Device = string.Empty;

                    DataTable dt = dsWork.Tables[0];
                    Label lbl = new Label();
                    foreach (DataRow r in dt.Rows)
                    {
                        ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (index).ToString());
                        ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                        TagLevel1 = r["TagLevel1"].ToString();
                        TagLevel2 = r["TagLevel2"].ToString();
                        Device = r["LocationID"].ToString();

                        // Need to set the TagLevel1 selection (TagLevel1 better be unique!)
                        ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (index).ToString());
                        ddl.SelectedValue = TagLevel1;
                        // Need to set the TagLevel2 selection (TagLevel2 better be unique!)
                        ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (index).ToString());
                        ddl.SelectedValue = TagLevel2;
                        // Need to set the Device label
                        lbl = (Label)tblTagGridFilter.FindControl("lblDevice" + (index).ToString());
                        lbl.Text = Device;

                        // Show create button
                        if (((BasePage)Page).CanInsert)
                        {
                            ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
                            ib.Visible = true;
                        }
                    }
                }
                else
                {
                    //If you made it here its because a loop is in the querystring that does not exist in the table
                    sMissingTagLevel3 = sMissingTagLevel3 + value + ",";
                }
            }
        }

        protected void ddlTagname_1Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname1.SelectedValue, 1);
        }

        protected void ddlTagname_2Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname2.SelectedValue, 2);
        }

        protected void ddlTagname_3Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname3.SelectedValue, 3);
        }

        protected void ddlTagname_4Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname4.SelectedValue, 4);
        }

        protected void ddlTagname_5Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname5.SelectedValue, 5);
        }

        protected void ddlTagname_6Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname6.SelectedValue, 6);
        }

        protected void ddlTagname_7Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname7.SelectedValue, 7);
        }

        protected void ddlTagname_8Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname8.SelectedValue, 8);
        }

        protected void ddlTagname_9Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname9.SelectedValue, 9);
        }

        protected void ddlTagname_10Changed(object sender, EventArgs e)
        {
            PopulateTagLabels(ddlTagname10.SelectedValue, 10);
        }

        private void PopulateTagLabels(string value, int index)
        {
            // Ensure this row is visible and the row is selected
            TableRow tbRow = (TableRow)tblTagGridFilter.FindControl("tr" + (index).ToString());
            tbRow.Visible = true;
            if (((BasePage)Page).CanInsert && sFileName != "ThreadInfo")
            {
                TableCell tbCell = (TableCell)tblTagGridFilter.FindControl("tcNew" + (index).ToString());
                tbCell.Visible = true;
            }
            DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (index).ToString());
            foreach (ListItem lsItem in ddl.Items)
            {
                if (lsItem.Value.ToLower() == value.ToLower())
                    ddl.SelectedValue = lsItem.Text;
            }

            // Default button to invisible
            ImageButton ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
            ib.Visible = false;

            Label lblDescription = (Label)tblTagGridFilter.FindControl("lblDescription" + (index).ToString());
            lblDescription.Text = string.Empty;
            Label lblType = (Label)tblTagGridFilter.FindControl("lblType" + (index).ToString());
            lblType.Text = string.Empty;
            Label lblUOM = (Label)tblTagGridFilter.FindControl("lblUOM" + (index).ToString());
            lblUOM.Text = string.Empty;

            DataSet dsWork = new DataSet();
            string error = string.Empty;
            dsWork = SASWrapper.QueryStoredProc_ResultSet("uspTAG_MainTagRefSelect", new string[] { "@Tagname" }, new string[] { value }, DatabaseName, ref error);

            if (dsWork != null && dsWork.Tables.Count > 0)
            {
                if (dsWork.Tables[0].Rows.Count > 0)
                {
                    string TagLevel1 = string.Empty;
                    string TagLevel2 = string.Empty;
                    string TagLevel3 = string.Empty;
                    string Device = string.Empty;

                    DataRow dr = dsWork.Tables[0].Rows[0];
                    lblDescription.Text = dr["Description"].ToString();
                    lblType.Text = dr["Type"].ToString();
                    lblUOM.Text = dr["UOM"].ToString();

                    TagLevel1 = dr["TagLevel1"].ToString();
                    TagLevel2 = dr["TagLevel2"].ToString();
                    TagLevel3 = dr["TagLevel3"].ToString();
                    Device = dr["LocationID"].ToString();

                    // Need to set the TagLevel1 selection (TagLevel1 better be unique!)
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (index).ToString());
                    ddl.SelectedValue = TagLevel1;
                    // Need to set the TagLevel2 selection (TagLevel2 better be unique!)
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (index).ToString());
                    ddl.SelectedValue = TagLevel2;
                    // Need to set the TagLevel3 selection (TagLevel3 better be unique!)
                    ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (index).ToString());
                    ddl.SelectedValue = TagLevel3;
                    Label lbl = (Label)tblTagGridFilter.FindControl("lblDevice" + (index).ToString());
                    lbl.Text = Device;

                    // Show create button
                    if (((BasePage)Page).CanInsert)
                    {
                        ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (index).ToString());
                        ib.Visible = true;
                    }
                }
                else
                {
                    //If you made it here its because a tagname is in the querystring that does not exist in MainTagRef
                    sMissingTagnames = sMissingTagnames + value + ",";
                }
            }
        }

        protected void ibNewRecord1_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_1.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_1.SelectedValue;
                sTL2Name = ddlTagLevel2_1.SelectedValue;
                sTL3Name = ddlTagLevel3_1.SelectedValue;
                sTagName = ddlTagname1.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord2_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_2.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_2.SelectedValue;
                sTL2Name = ddlTagLevel2_2.SelectedValue;
                sTL3Name = ddlTagLevel3_2.SelectedValue;
                sTagName = ddlTagname2.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord3_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_3.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_3.SelectedValue;
                sTL2Name = ddlTagLevel2_3.SelectedValue;
                sTL3Name = ddlTagLevel3_3.SelectedValue;
                sTagName = ddlTagname3.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord4_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_4.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_4.SelectedValue;
                sTL2Name = ddlTagLevel2_4.SelectedValue;
                sTL3Name = ddlTagLevel3_4.SelectedValue;
                sTagName = ddlTagname4.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord5_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_5.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_5.SelectedValue;
                sTL2Name = ddlTagLevel2_5.SelectedValue;
                sTL3Name = ddlTagLevel3_5.SelectedValue;
                sTagName = ddlTagname5.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord6_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_6.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_6.SelectedValue;
                sTL2Name = ddlTagLevel2_6.SelectedValue;
                sTL3Name = ddlTagLevel3_6.SelectedValue;
                sTagName = ddlTagname6.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord7_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_7.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_7.SelectedValue;
                sTL2Name = ddlTagLevel2_7.SelectedValue;
                sTL3Name = ddlTagLevel3_7.SelectedValue;
                sTagName = ddlTagname7.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord8_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_8.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_8.SelectedValue;
                sTL2Name = ddlTagLevel2_8.SelectedValue;
                sTL3Name = ddlTagLevel3_8.SelectedValue;
                sTagName = ddlTagname8.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord9_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_9.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_9.SelectedValue;
                sTL2Name = ddlTagLevel2_9.SelectedValue;
                sTL3Name = ddlTagLevel3_9.SelectedValue;
                sTagName = ddlTagname9.SelectedValue;
                AddTagName();
            }
        }

        protected void ibNewRecord10_Click(object sender, ImageClickEventArgs e)
        {
            if (ddlTagLevel1_10.SelectedValue.Length > 0)
            {
                sTL1Name = ddlTagLevel1_10.SelectedValue;
                sTL2Name = ddlTagLevel2_10.SelectedValue;
                sTL3Name = ddlTagLevel3_10.SelectedValue;
                sTagName = ddlTagname10.SelectedValue;
                AddTagName();
            }
        }

        private void InitializeTagTable(string Tag)
        {
            for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
            {
                DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (i).ToString());
                ddl.Items.Clear();
                ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (i).ToString());
                ddl.Items.Clear();
                ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (i).ToString());
                ddl.Items.Clear();
                ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                ddl.Items.Clear();
                Label lblDevice = (Label)tblTagGridFilter.FindControl("lblDevice" + (i).ToString());
                lblDevice.Text = string.Empty;
                Label lblDescription = (Label)tblTagGridFilter.FindControl("lblDescription" + (i).ToString());
                lblDescription.Text = string.Empty;
                Label lblType = (Label)tblTagGridFilter.FindControl("lblType" + (i).ToString());
                lblType.Text = string.Empty;
                Label lblUOM = (Label)tblTagGridFilter.FindControl("lblUOM" + (i).ToString());
                lblUOM.Text = string.Empty;
                ImageButton ib = (ImageButton)tblTagGridFilter.FindControl("ibNewRecord" + (i).ToString());
                ib.Visible = false;
                if (i > 1)
                {
                    TableRow tr = (TableRow)tblTagGridFilter.FindControl("tr" + (i).ToString());
                    tr.Visible = false;
                }
            }

            string sql = string.Format("SELECT * FROM v_TagLevel1_lb");
            DataSet ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {

                DataTable dttag = ds.Tables[0];
                foreach (DataRow r in dttag.Rows)
                {
                    for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
                    {
                        DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (i).ToString());
                        ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                    }
                }
            }

            sql = string.Format("SELECT * FROM v_TagLevel2_lb");
            ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {

                DataTable dttag = ds.Tables[0];
                foreach (DataRow r in dttag.Rows)
                {
                    for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
                    {
                        DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (i).ToString());
                        ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                    }
                }
            }

            sql = string.Format("SELECT * FROM v_TagLevel3_lb");
            ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {

                DataTable dttag = ds.Tables[0];
                foreach (DataRow r in dttag.Rows)
                {
                    for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
                    {
                        DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (i).ToString());
                        ddl.Items.Add(new ListItem { Value = r["Value"].ToString(), Text = r["Value"].ToString() });
                    }
                }
            }

            //sql = string.Format("SELECT * FROM v_Tagname_lb");
            //ds = SASWrapper.ExecuteQuery(DatabaseName, sql);
            string error = string.Empty;
            ds = SASWrapper.QueryStoredProc_ResultSet("uspTAG_GetTagnames", new string[] { "@Tagname" }, new string[] { Tag }, DatabaseName, ref error);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {

                DataTable dttag = ds.Tables[0];
                foreach (DataRow r in dttag.Rows)
                {
                    for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
                    {
                        DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                        ddl.Items.Add(new ListItem { Value = r["DisplayText"].ToString(), Text = r["DisplayText"].ToString() });
                    }
                }
            }
        }

        /// <summary>
        /// Runs when the user clicks "Clear Tag Filter"
        /// </summary>
        protected void ibClearTagFilter_Click(object sender, ImageClickEventArgs e)
        {
            if (sFileName == "ThreadInfo") ThreadOrderClearUser();
            //Clear Duration
            tbxNoteEntry.Text = string.Empty;
            tbxStartDate.Text = DateTime.Now.ToString("d", DateTimeFormatInfo.InvariantInfo);
            //tbxStartTime.Text = DateTime.Now.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo);
            tbxStartTime.Text = "23:59";
            tbxDurationMin.Text = Session["AlarmDBNoMins"].ToString();
            if (ddlDirection.Items.Count > 0) ddlDirection.SelectedIndex = 0;
            tbxInterval.Text = "00:01:00";
            if (Path.GetFileNameWithoutExtension(HttpContext.Current.Request.Url.AbsolutePath) == "ThreadInfo")
                tbxDurationDay.Text = Session["ThreadNoDays"].ToString();
            else
                tbxDurationDay.Text = Session["AlarmDBNoDays"].ToString();
            Session["TagStartDate"] = tbxStartDate.Text;
            Session["TagStartTime"] = tbxStartTime.Text;
            Session["TagDurationDay"] = tbxDurationDay.Text;
            Session["TagDurationMin"] = tbxDurationMin.Text;
            Session["TagNoteEntry"] = tbxNoteEntry.Text;
            Session["TagDirection"] = ddlDirection.Text;
            Session["TagInterval"] = tbxInterval.Text;

            Session["TagFilterUpdate"] = "true";
            InitializeTagTable(null);
            Session["TagnameFilter"] = GetTagnames();
            Session["TagLevel1Filter"] = GetTagLevel1();
            Session["TagLevel2Filter"] = GetTagLevel2();
            Session["TagLevel3Filter"] = GetTagLevel3();
            Session["QuickFilter"] = string.Empty;
            AddFilter();
        }

        private string GetTagnames()
        {
            sTagnames = string.Empty;
            for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
            {
                DropDownList ddl = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                if (ddl.SelectedValue.Length > 0)
                    sTagnames = sTagnames + "'" + ddl.SelectedValue + "',";
            }
            if (sTagnames.Length > 0)
            {
                sTagnames = sTagnames.Remove(sTagnames.Length - 1, 1);
                sTagnames = "([Tagname] IN (" + sTagnames + "))";
            }
            return sTagnames;
        }

        private string GetTagLevel1()
        {
            sTagLevel1 = string.Empty;
            for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
            {
                DropDownList ddl1 = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel1_" + (i).ToString());
                DropDownList ddl2 = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (i).ToString());
                DropDownList ddl3 = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (i).ToString());
                DropDownList ddl4 = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                if (ddl1.SelectedValue.Length > 0 && ddl2.SelectedValue.Length == 0 && ddl3.SelectedValue.Length == 0 && ddl4.SelectedValue.Length == 0)
                    sTagLevel1 = sTagLevel1 + "'" + ddl1.SelectedValue + "',";
            }
            if (sTagLevel1.Length > 0)
            {
                sTagLevel1 = sTagLevel1.Remove(sTagLevel1.Length - 1, 1);
                sTagLevel1 = "([TL1Name] IN (" + sTagLevel1 + "))";
            }
            return sTagLevel1;
        }

        private string GetTagLevel2()
        {
            sTagLevel2 = string.Empty;
            for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
            {
                DropDownList ddl2 = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel2_" + (i).ToString());
                DropDownList ddl3 = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (i).ToString());
                DropDownList ddl4 = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                if (ddl2.SelectedValue.Length > 0 && ddl3.SelectedValue.Length == 0 && ddl4.SelectedValue.Length == 0)
                    sTagLevel2 = sTagLevel2 + "'" + ddl2.SelectedValue + "',";
            }
            if (sTagLevel2.Length > 0)
            {
                sTagLevel2 = sTagLevel2.Remove(sTagLevel2.Length - 1, 1);
                sTagLevel2 = "([TL2Name] IN (" + sTagLevel2 + "))";
            }
            return sTagLevel2;
        }

        private string GetTagLevel3()
        {
            sTagLevel3 = string.Empty;
            sDevice = string.Empty;
            for (int i = 1; i < tblTagGridFilter.Rows.Count; i++)
            {
                DropDownList ddl3 = (DropDownList)tblTagGridFilter.FindControl("ddlTagLevel3_" + (i).ToString());
                //Label lbl = (Label)tblTagGridFilter.FindControl("lblDevice" + (i).ToString());
                DropDownList ddl4 = (DropDownList)tblTagGridFilter.FindControl("ddlTagname" + (i).ToString());
                if (ddl3.SelectedValue.Length > 0 && ddl4.SelectedValue.Length == 0)
                {
                    sTagLevel3 = sTagLevel3 + "'" + ddl3.SelectedValue + "',";
                    //I can't get the text value of the device label at this point of the process so I have to get it from a function call
                    //sDevice = sDevice + "'" + lbl.Text + "',";
                    DataSet ds = SASWrapper.ExecuteQuery(Session["DatabaseName"].ToString(), string.Format("Select dbo.uf_LogGetLocationID('{0}')", ddl3.SelectedValue));
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        sDevice = sDevice + "'" + ds.Tables[0].Rows[0][0].ToString() + "',";
                }
            }
            if (sTagLevel3.Length > 0)
            {
                sTagLevel3 = sTagLevel3.Remove(sTagLevel3.Length - 1, 1);
                sTagLevel3 = "([TL3Name] IN (" + sTagLevel3 + "))";
                sDevice = sDevice.Remove(sDevice.Length - 1, 1);
                sDevice = "([LocationID] IN (" + sDevice + "))";
                sTagLevel3 = "(" + sTagLevel3 + " OR " + sDevice + ")";
            }
            return sTagLevel3;
        }

        private void SetTagnames(string filter)
        {
            //This method is used by ThreadInfo to reset the tag filters with their previous values since the silverlight control has post to 
            //ThreadInfo whenever it is changed which causes the tag filter to reset.
            alTagnameArray = GetWhereClause(filter);
        }

        private void SetTagLevel1(string filter)
        {
            //This method is used by ThreadInfo to reset the tag filters with their previous values since the silverlight control has post to 
            //ThreadInfo whenever it is changed which causes the tag filter to reset.
            alTagLevel1Array = GetWhereClause(filter);
        }

        private void SetTagLevel2(string filter)
        {
            //This method is used by ThreadInfo to reset the tag filters with their previous values since the silverlight control has post to 
            //ThreadInfo whenever it is changed which causes the tag filter to reset.
            alTagLevel2Array = GetWhereClause(filter);
        }

        private void SetTagLevel3(string filter)
        {
            //This method is used by ThreadInfo to reset the tag filters with their previous values since the silverlight control has post to 
            //ThreadInfo whenever it is changed which causes the tag filter to reset.

            //Strip out everything past OR since we don't want device to be included in the filter
            if ((filter.IndexOf("[LocationID]") >= 0))
            {
                int i = filter.IndexOf(") OR (");
                filter = filter.Remove(i) + ")";
            }
            alTagLevel3Array = GetWhereClause(filter);
        }

        private void ThreadOrderClearUser()
        {
            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add("LogThreadOrder");
            dt.Columns.Add("UserID");
            string[] sArr = { ((BasePage)Page).UserID };
            dt.Rows.Add(sArr);
            string sTextError = SASWrapper.UpdateData("uspLOG_LogThreadOrderClearUser", Session["DatabaseName"].ToString(), ds);
        }
    }
}