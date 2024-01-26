using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace LMS2.components
{
    public class PCSGridView : System.Web.UI.WebControls.GridView
    {
        protected bool areRowsSelectable = false;
        protected bool pageJustChanged = false;

        public PCSGridView()
        {
            CssClass = "GridView";
            HeaderStyle.CssClass = "GridViewHeader";
            AlternatingRowStyle.CssClass = "GridViewAlternateItem";
            EmptyDataRowStyle.CssClass = "GridViewLabelInfo";
            PagerStyle.CssClass = "GridViewPager";
            SelectedRowStyle.CssClass = "GridViewSelectedRow";
            FooterStyle.CssClass = "GridViewFooter";
            AutoGenerateColumns = false;
        }

        protected string PageID
        {
            get { return Session("GridViewPageID"); }
            set { Page.Session["GridViewPageID"] = value; }
        }

        public int QueryColumnIndex
        {
            get { return (Page.Session["GridViewQueryColumnIndex"] != null) ? (int)Page.Session["GridViewQueryColumnIndex"] : 0; }
            set { Page.Session["GridViewQueryColumnIndex"] = value; }
        }

        protected override void OnPagePreLoad(object sender, EventArgs e)
        {
            base.OnPagePreLoad(sender, e);

            string str_Script = @"
                <script type='text/javascript'>
                {
                    var iTimeoutId = null;

                    function doClick(poststring)
                    {
                        iTimeoutId = setTimeout(poststring, 240);
                    }

                    function doDblClick(querystring)
                    {
                        if (iTimeoutId)
                            window.clearTimeout(iTimeoutId);
                        if (window.getSelection) window.getSelection().removeAllRanges();
                        else if (document.selection) document.selection.empty();
                        winContent = window.open('TBAJAX.aspx' + querystring, 'mywindow', 'left='+((screen.width - 420) / 2)+',top='+(screen.height - 5) / 2+',status=no,titlebar=no;toolbar=no,menubar=no,directories=no,location=no,scrollbars=yes,resizable=no,width=420,height=5px')
                        winContent.focus()
                    }
                }
                </script>";
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "HidePanel", str_Script);

            EmptyDataText = "";// ((BasePage)Page).NoDataGridMsg;

            areRowsSelectable = (Session("QueryKeyColumn").Length > 0);

            PageID = Session("CurPageName");

            // Get the column list and make sure that it contains the QueryKeyColumn if one was specified -- if not then add it.
            string columnsRequested = Session("ColumnShown", "*");
            string queryKeyColumn = Session("QueryKeyColumn");
            bool queryKeyIncluded = queryKeyColumn.Length == 0;

            List<string> _columns = new List<string>();
            foreach (string s in Regex.Split(columnsRequested, ","))
            {
                string col = s.Trim().ToLower();
                if (col == queryKeyColumn.ToLower())
                    queryKeyIncluded = true;
                _columns.Add(col);
            }
            if (!queryKeyIncluded)
            {
                _columns.Add(queryKeyColumn.ToLower());
                Page.Session["ColumnShown"] = columnsRequested + string.Format("{0}{1}", (columnsRequested.Length > 0) ? "," : "", queryKeyColumn);
            }

            bool hasThumbnail = columnsRequested.ToLower().StartsWith("thumbnail");
            if (hasThumbnail)
            {
                // Remove all but the 1st column which is a data bound thumbnail image
                for (int i = 1; i < Columns.Count; i++)
                    Columns.RemoveAt(1);
            }
            else
                Columns.Clear();

            // Get list of columns that should be hidden in grid
            Dictionary<string, int> hideCols = Page.Session["HiddenColumns"] != null ? Page.Session["HiddenColumns"] as Dictionary<string, int> : new Dictionary<string, int>();

            if (hideCols.ContainsKey("backcolor") || hideCols.ContainsKey("forecolor"))
            {
                AlternatingRowStyle.CssClass = "GridViewColorAlternateItem";
                SelectedRowStyle.CssClass = "GridViewColorSelectedRow";
                CssClass = "GridViewColor";
                SelectedRowStyle.Font.Bold = true;
            }

            DataTable dt = Page.Session["ColumnDefinitions"] as DataTable;
            QueryColumnIndex = -1;
            Dictionary<string, DataColumn> colDef = PCSTranslation.ColumnDefinitions;
            Dictionary<string, string> colTrans = PCSTranslation.ColumnTranslations;
            for (int i = ((hasThumbnail) ? 1 : 0); i < _columns.Count; i++)
            {
                if (colDef.ContainsKey(_columns[i]))
                {
                    DataColumn col = colDef[_columns[i]];

                    BoundField bf = new BoundField();
                    bf.DataField = col.ColumnName;
                    bf.HeaderText = colTrans[col.ColumnName.ToLower()];
                    if (((BasePage)Page).UseAltLanguage && !((BasePage)Page).WrapAltLanguage)
                        bf.HeaderStyle.Wrap = false;
                    bf.SortExpression = bf.DataField;
                    if (((BasePage)Page).IsHistoryPage)
                        bf.ItemStyle.Wrap = false;

                    // If formatting specifically specified then use it, otherwise use type defaults
                    Type colType = col.DataType;
                    Dictionary<string, ColumnStyle> colStyles = Page.Session["ColumnStyles"] as Dictionary<string, ColumnStyle>;
                    //Dictionary<string, string> colFormats = Page.Session["ColumnFormats"] as Dictionary<string, string>;
                    if (colStyles.ContainsKey(bf.DataField.ToLower()))
                        bf.DataFormatString = string.Format("{{0:{0}}}", colStyles[bf.DataField.ToLower()].format);
                    else
                    {
                        // Set any default formatting based upon the column type... 
                        if (colType == typeof(DateTime) && col.ColumnName.ToLower() != "updateddate")
                            bf.DataFormatString = string.Format("{{0:{0}}}", BasePage.DateFormat); // show only the short date format
                        else if (colType == typeof(DateTime))
                            bf.DataFormatString = string.Format("{{0:{0}}}", BasePage.DateTimeFormat);
                    }

                    // Right align numeric values and their column headers
                    if (colType == typeof(double) || colType == typeof(float) || colType == typeof(int))
                    {
                        bf.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                        bf.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
                    }

                    Columns.Add(bf);

                    if (hideCols.ContainsKey(bf.DataField.ToLower()))
                        hideCols[bf.DataField.ToLower()] = i;

                    if (bf.DataField.ToLower() == queryKeyColumn.ToLower())
                        QueryColumnIndex = i;
                }
            }

            Aggregates agg = (Aggregates)Page.Session["ColumnAggregates"];
            if (agg!=null &&agg.hasAggregateColumns)
                ShowFooter = true; // turn on footer row for result display

        }

        protected override void OnRowDataBound(GridViewRowEventArgs e)
        {
            base.OnRowDataBound(e);

            if (e.Row.RowType == DataControlRowType.DataRow && Session("ColumnToolbar").Length > 0)
            {
                string columns = ((Session("ColumnShown") != null) && Session("ColumnShown").ToString() != string.Empty) ? Session("ColumnShown").ToString() : "*";

                columns = Regex.Replace(columns.ToLower(), @"\s", ""); // trim all spaces
                Dictionary<string, int> hideCols = Page.Session["HiddenColumns"] as Dictionary<string, int>;
                Dictionary<int, string> cols = new Dictionary<int, string>();
                Dictionary<int, string> order = new Dictionary<int, string>();
                int cnt = 0;
                foreach (string col in Regex.Split(columns, ","))
                {
                    if (col.Length > 0 && !hideCols.ContainsKey(col.ToLower()))
                        cols[cnt] = col;
                    cnt++;
                }
                cnt = 0;
                foreach (string col in Regex.Split(Session("ColumnToolbar").ToLower(), ","))
                {
                    order[cnt] = col;
                    cnt++;
                }
                string queryString = string.Empty;

                for (int y = 0; y < order.Count; y++)
                {
                    queryString = getQueryString(y, order, cols, queryString, e);
                    if (queryString.Length > 0) break;
                }


                if (queryString.Length > 0)
                {
                    //e.Row.Attributes["ondblclick"] = string.Format("popUpToolbar('{0}')", queryString);
                    e.Row.Attributes["ondblclick"] = string.Format("doDblClick('{0}')", queryString);
                    e.Row.Style["cursor"] = "hand";
                }
            }

            // Make row clickable to select it
            if (areRowsSelectable && QueryColumnIndex >= 0 && e.Row.RowType == DataControlRowType.DataRow)
            {
                //e.Row.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference(this, "Select$" + e.Row.RowIndex);
                e.Row.Attributes["onclick"] = string.Format("doClick(\"{0}\")", this.Page.ClientScript.GetPostBackEventReference(this, "Select$" + e.Row.RowIndex));
                e.Row.Style["cursor"] = "hand";
            }

            // Get list of columns that should be hidden in grid and hide the cells.  We must do it this way because hiding the columns in
            // OnPagePreLoad simply eliminates the bound field from the list and it never gets rendered... and hence cannot be used.
            // Only apply the Visible=false if this is not the pager control, otherwise you lose the pager control when the PK is hidden
            if (e.Row.RowType != DataControlRowType.Pager && e.Row.RowType != DataControlRowType.EmptyDataRow)
            {
                Dictionary<string, int> hideCols = Page.Session["HiddenColumns"] as Dictionary<string, int>;
                int backPos = -1;
                int forePos = -1;
                if (hideCols.ContainsKey("backcolor")) backPos = hideCols["backcolor"];
                if (hideCols.ContainsKey("forecolor")) forePos = hideCols["forecolor"];
                foreach (int colIndex in hideCols.Values)
                    if (colIndex >= 0 && colIndex < e.Row.Cells.Count)
                    {
                        e.Row.Cells[colIndex].Visible = false;
                        if ((colIndex == backPos) && e.Row.Cells[colIndex].Text.Length > 0 && e.Row.Cells[colIndex].Text != "&nbsp;")
                            e.Row.BackColor = System.Drawing.ColorTranslator.FromHtml("#" + e.Row.Cells[colIndex].Text);
                        //for (int i = 0; i < e.Row.Cells.Count; i++)
                        //    e.Row.Cells[i].BackColor = System.Drawing.ColorTranslator.FromHtml("#" + e.Row.Cells[colIndex].Text);
                        if ((colIndex == forePos) && e.Row.Cells[colIndex].Text.Length > 0 && e.Row.Cells[colIndex].Text != "&nbsp;")
                            e.Row.ForeColor = System.Drawing.ColorTranslator.FromHtml("#" + e.Row.Cells[colIndex].Text);
                    }
            }

            if (((BasePage)Page).IsHistoryPage && e.Row.RowType == DataControlRowType.DataRow)
            {
                for (int i = 0; i < e.Row.Cells.Count; i++)
                {
                    string value = e.Row.Cells[i].Text;
                    if (!string.IsNullOrEmpty(value))
                    {
                        e.Row.Cells[i].Text = value.Length > 30 ? value.Substring(0, 30) + "..." : value;
                        e.Row.Cells[i].ToolTip = HttpUtility.HtmlDecode(value);
                    }
                }
            }
        }

        private string getQueryString(int y, Dictionary<int, string> order, Dictionary<int, string> cols, string queryString, GridViewRowEventArgs e)
        {
            for (int x = 0; x < cols.Count; x++)
                if (cols.ContainsKey(x) && cols[x].ToString().ToLower() == order[y].ToString().ToLower() && HttpContext.Current.Server.HtmlDecode(e.Row.Cells[x].Text).Trim().Length > 0)
                {
                    queryString = string.Format("?{0}={1}", cols[x].ToString().ToLower(), HttpContext.Current.Server.UrlEncode(e.Row.Cells[x].Text));
                    break;
                }
            return queryString;
        }

        protected override void OnPageIndexChanged(EventArgs e)
        {
            base.OnPageIndexChanged(e);
            // Set the selected row to the first
            SelectedIndex = -1;
            pageJustChanged = true;
        }

        protected override void OnSorting(GridViewSortEventArgs e)
        {
            base.OnSorting(e);
            HttpContext.Current.Session["RecalcSelection"] = true;
        }

        protected override void OnDataBinding(EventArgs e)
        {
            bool itemInserted = (HttpContext.Current.Session["RecalcSelection"] != null) ? (bool)HttpContext.Current.Session["RecalcSelection"] : false;
            if (itemInserted)
                SelectedIndex = -1; // if a new insert, then undo selection to try and pick newly added entry

            if ((!Page.IsPostBack || itemInserted) && areRowsSelectable && !pageJustChanged)
            {
                HttpContext.Current.Session["RecalcSelection"] = false;

                if (Session("SelectedItemKey").Length > 0)
                {
                    DataView dv = (DataView)(DataSourceObject as ObjectDataSource).Select();

                    //if (dv.Sort.Length > 0)
                    if (SortExpression.Length > 0)
                        dv.Sort += string.Format("{0}{1}", SortExpression, (SortDirection == SortDirection.Descending) ? " DESC" : "");

                    string val = Session("SelectedItemKey").ToLower();
                    DataTable dt = dv.ToTable();

                    string queryKeyColumn = Session("QueryKeyColumn").ToLower();
                    int queryIndex = -1;
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (dt.Columns[i].ColumnName.ToLower() == queryKeyColumn)
                        {
                            queryIndex = i;
                            break;
                        }
                    }
                    if (queryIndex >= 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i][queryIndex].ToString().ToLower() == val)
                            {
                                PageIndex = (int)(i / PageSize);
                                SelectedIndex = (i % PageSize);
                                break;
                            }
                        }
                    }

                    // In the event that we are matching the id to a view based column, then try and match against the beginning since we couldn't
                    // match against the whole.
                    if (SelectedIndex == -1)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string matchVal = dt.Rows[i][queryIndex].ToString().ToLower();
                            int j = matchVal.IndexOf(" ");
                            if (j >= 0)
                            {
                                // Take the value up to the first embedded space... we only get in here if there is an embedded space
                                matchVal = matchVal.Substring(0, j);
                                if (matchVal == val)
                                {
                                    PageIndex = (int)(i / PageSize);
                                    SelectedIndex = (i % PageSize);
                                    break;
                                }
                            }
                        }
                    }

                    // If we still don't have a selection, then the item we are trying to select must have beenb filtered out...
                    if (SelectedIndex == -1)
                        ((BasePage)Page).SetFilteredSelectionMessage(true);
                }
            }

            if (SelectedIndex != -1)
                ((BasePage)Page).SetFilteredSelectionMessage(false);

            base.OnDataBinding(e);
        }

        protected override void OnDataBound(EventArgs e)
        {
            base.OnDataBound(e);

            Aggregates agg = (Aggregates)Page.Session["ColumnAggregates"];
            if (agg != null)
            {
                agg.Reset();

                // We have to get the underlying data differently depending on how the grid was bound (data source or directly bound to an object)
                DataTable dt = (DataSource) as DataTable;
                if (dt == null)
                {
                    ObjectDataSource src = DataSourceObject as ObjectDataSource;
                    if (src != null)
                    {
                        DataView dv = src.Select() as DataView;
                        if (dv != null)
                            dt = dv.Table;
                    }
                }

                if (dt != null)
                {
                    Page.Session["GridRecordCount"] = dt.Rows.Count;
                    Page.Session["TotalGridRecords"] = string.Format(PCSTranslation.GetMessage("TotalRecords"), dt.Rows.Count.ToString());

                    string[] aggCols = agg.ColumnList;
                    if (aggCols.Length > 0 && FooterRow != null)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            foreach (string col in aggCols)
                            {
                                double val = double.NaN;
                                try
                                {
                                    val = double.Parse(row[col].ToString());
                                }
                                catch { /* shouldn't happen, but eat the exception if it does */ }

                                agg.AddValue(col, val);
                            }
                        }

                        // Write results into grid footer
                        Dictionary<string, string> colTrans = PCSTranslation.ColumnTranslations;
                        foreach (string col in aggCols)
                        {
                            if (colTrans.ContainsKey(col.ToLower()))
                            {
                                string coltxt = colTrans[col.ToLower()];
                                for (int i = 0; i < Columns.Count; i++)
                                {
                                    if (coltxt == Columns[i].HeaderText)
                                    {
                                        FooterRow.Cells[i].Text = agg.GetFinalAnswer(col);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

            }

            // We need to do this to make sure the details view is tracking the selected item in the details view when the user changes pages,
            // sorting, deleting items, etc in the summary grid or details view.
            if (areRowsSelectable && Rows.Count > 0)
                ((BasePage)Page).ProcessSelection();
            else
                SelectedIndex = -1;

            pageJustChanged = false;
        }

        protected string Session(string id)
        {
            return Session(id, "");
        }

        protected string Session(string id, string def)
        {
            object o = Page.Session[id];
            return (o != null) ? o.ToString() : def;
        }

    }

}