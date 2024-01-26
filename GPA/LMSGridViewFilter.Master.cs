using AjaxControlToolkit;
using LMS2.components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GPA
{
    public partial class LMSGridViewFilter : System.Web.UI.MasterPage
    {
        string sWhere;
        private DataTable dtFilterData = new DataTable();
        private DataTable dtFilterHeading = new DataTable();
        public bool AdvFilterFlag { get { return GetSessionVariable("AdvFilterFlag") == "True" ? true : false; } }
        public bool UseAltLanguage { get { return GetSessionVariable("AltLanguageIndicator") == "1" ? true : false; } }
        public string Filter { get { return GetSessionVariable("Filter") != null ? GetSessionVariable("Filter") : ""; } }
        public string DatabaseName { get { return GetSessionVariable("DatabaseName") != null ? GetSessionVariable("DatabaseName") : ""; } }
        public string UserID { get { return GetSessionVariable("UserID") != null ? GetSessionVariable("UserID") : ""; } }
        public string CurPageName { get { return GetSessionVariable("CurPageName") != null ? GetSessionVariable("CurPageName") : ""; } }
        public bool FilteringEnabled { get { return FilteringPanel.Visible; } set { FilteringPanel.Visible = value; } }

        // Declare delegates
        public delegate void BindFilter();

        // Define an Event based on the above Delegates
        public event BindFilter BindFilterData;

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

        /// <summary>
        /// where clause
        /// </summary>
        public string Where
        {
            get { return sWhere; }
            set { sWhere = value; }
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
            set { QuickFilterBox = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            OpenPopUp(ibFilterInfo, "HelpPage.aspx?FunctionID=FilterCriteria", "_blank", 600, 600);
            // If a filter is in effect and the QuickFilter is not in use, then force the filter panel to be expanded regardless of it's state
            if (QuickFilterBox.Text.Length == 0 && !string.IsNullOrEmpty(Filter))
                CollapsiblePanelExtender1.Collapsed = false;
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
        }

        /// <summary>
        /// Creates a where clause and then runs binddata.  Runs when the user clicks "Retrieve from database"
        /// </summary>
        protected void ibAddFilter_Click(object sender, ImageClickEventArgs e)
        {
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
            BindFilterData();
        }

        /// <summary>
        /// Runs when the user clicks "Clear Filter"
        /// </summary>
        protected void ibClearFilter_Click(object sender, ImageClickEventArgs e)
        {
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
            ApplyQuickFilter();
            BindFilterData();

            // If the filter panel is open, then close it as part of running a quick filter
            CollapsiblePanelExtender1.Collapsed = true;
            CollapsiblePanelExtender1.ClientState = "true";
        }

        public void ApplyQuickFilter()
        {
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
    }
}