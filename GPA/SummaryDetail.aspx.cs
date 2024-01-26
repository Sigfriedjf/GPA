﻿using LMS2.components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GPA
{

    public partial class SummaryDetail : BasePage
    {
        protected override string UserIDDisplay { set { lbUserID.Text = value; } }
        protected override bool UserChangeTimer { get { return tmUserChangeTimer.Enabled; } set { tmUserChangeTimer.Enabled = value; } }
        protected Dictionary<string, string> NoDetailColumns = new Dictionary<string, string>() {
          {"wa11f2p8", "wa11f2p8"},
            {"wa71f1p11", "wa71f1p11"},
            {"wa31f1p7", "wa31f1p7"},
            {"wa31f6p5", "wa31f6p5"},
            {"wa25f4p3", "wa25f4p3"},
            {"wa55f1p1", "wa55f1p1"}
        };

        protected new void Page_PreInit(object sender, EventArgs e)
        {
            base.Page_PreInit(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DetailsView1.EditRowStyle.BackColor = System.Drawing.ColorTranslator.FromHtml(ColorManager.ColorGridSelectedRow);
            if (Request.QueryString["HideFilter"] != null)
                ((LMSGridViewFilter)Master).FilteringEnabled = false;

            ibPagePrint.Attributes.Add("onClick", "window.print()");
            GridView_Summary.PageSize = (Session["NumOfRows"] != null) ? int.Parse(Session["NumOfRows"].ToString()) : 15;
            if (!IsPostBack)
            {
                if ((PageID.ToLower() == "wa11f1p1" || PageID.ToLower() == "wa11f1p7b") && !IsHistoryPage)
                {
                    cbActive.Visible = true;
                    cbActive.Checked = (_Session("ShowObsolete") == "1");
                }
                if ((PageID.ToLower() == "wa55f1p1") && !IsHistoryPage)
                {
                    cbActive.Visible = true;
                    cbActive.Checked = (_Session("ShowObsolete") == "1");
                }

                // Don't show the print button for audit trail pages
                ibPagePrint.Visible = !IsHistoryPage;

                // Start by forcing cache stale
                Cache[ObjectDataSource1.CacheKeyDependency] = new object();

                tbNumOfRows.Text = GridView_Summary.PageSize.ToString();
                Session[Session["CurPageName"].ToString() + "NumOfRows"] = tbNumOfRows.Text;
                Session["NumOfRows"] = tbNumOfRows.Text;

                Master.SQLText.Text = _Session("Filter");

                // Enable history button if a history page has been defined.
                ibHist.Visible = HasHistoryPage && !IsHistoryPage;

                // Enable Debug Panel if appropriate
                DebugPanel.Visible = (Session["ShowPageDebugInfo"] != null) ? (Session["ShowPageDebugInfo"].ToString() == "1") : false;

                if (DetailMode.StartsWith("N")) //None
                    DetailsView1.Visible = false;

                // If help file exists, show button and bind to launch it
                ibPageInfo.Visible = false;
                if (_Session("HelpPage").Length > 0)
                {
                    ibPageInfo.Visible = true;
                    OpenPopUp(ibPageInfo, "HelpPage.aspx?PageID=" + _Session("HelpPage"), "_blank", 600, 600);
                }
                else
                {
                    // Might be a history page
                    if (GridView_Summary.QueryColumnIndex < 0)
                    {
                        string HistHelpPath = GetFilePath("HelpFiles", "HistoryInfo", "htm", "NoHelp");
                        if (HistHelpPath.Length > 0)
                        {
                            ibPageInfo.Visible = true;
                            OpenPopUp(ibPageInfo, HistHelpPath, "_blank", 600, 600);
                        }
                    }
                }

                // Enable the view report button on certain pages.
                if (Request.QueryString["RPTPageID"] != null)
                {
                    DataSet ds2 = SASWrapper.ExecuteQuery(DatabaseName, string.Format("Select PageHeading from dbo.AppsPages where PageID='{0}'", Request.QueryString["RPTPageID"]));
                    if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                        btnViewReport.Text = ds2.Tables[0].Rows[0][0].ToString();
                    btnViewReport.Visible = true;
                }
            }

            if (ViewState["SASData"] == null)
            {
                DataTable dtFilter = ((DataTable)Session["ColumnDefinitions"]).Copy();

                // Figure out what columns we want to keep...
                Dictionary<string, int> hideCols = Session["HiddenColumns"] as Dictionary<string, int>;
                Dictionary<string, int> colReq = new Dictionary<string, int>();
                for (int i = 0; i < GridView_Summary.Columns.Count; i++)
                    if (i > 0 || (GridView_Summary.Columns[i] as ImageField) == null)
                        if (!hideCols.ContainsKey(((BoundField)GridView_Summary.Columns[i]).DataField.ToLower()))
                            colReq[((BoundField)GridView_Summary.Columns[i]).DataField] = i;

                // Remove any columns not in the keep list, and translate the ones we're keeping
                for (int i = dtFilter.Columns.Count - 1; i >= 0; i--)
                {
                    string colName = dtFilter.Columns[i].ColumnName;
                    if (!colReq.ContainsKey(colName))
                        dtFilter.Columns.RemoveAt(i);
                    else
                        dtFilter.Columns[i].Caption = GridView_Summary.Columns[colReq[colName]].HeaderText;
                }

                // Order the filter columns to match the order in the grid
                int ordinal = 0;
                foreach (string key in colReq.Keys)
                    dtFilter.Columns[key].SetOrdinal(ordinal++);

                ViewState["SASData"] = dtFilter;
            }
            Master.BindFilterData += new LMSGridViewFilter.BindFilter(BindFilter);
            Master.CreateFilterTable((DataTable)ViewState["SASData"], Master.SQLText.Text);

            if (Regex.IsMatch(Master.SQLText.Text.Trim(), @"ScheduledDate") && PageID.ToLower() == "wa55f1p1")
            {
                cbActive.Checked = false;
                Session["ShowObsolete"] = cbActive.Checked ? "1" : "0";
            }

            if (PageID.ToLower() == "wa55f1p1")
                Session["WhereDate"] = "CONVERT(nvarchar(20), ScheduledDate, 101)=CONVERT(nvarchar(20), GETDATE(), 101)";

            if (!CanInsert || !CanEdit)
                DetailsView1.EmptyDataTemplate = null;

            ibConfigFilter.Visible = !IsHistoryPage;
            // Don't show the detail view icon for these pages
            if (NoDetailColumns.ContainsKey(PageID.ToLower()))
                ibDetails.Visible = false;
            else
                ibDetails.Visible = !IsHistoryPage;
            // Don't show the bulk edit for these pages
            if (_Session("BulkEdit").Length == 0)
                ibBulkEdit.Visible = false;
            else
                ibBulkEdit.Visible = !IsHistoryPage;
            if (!IsHistoryPage)
            {
                Panel1.CssClass = "ListPanel";
                SummaryPanel.CssClass = "SummaryPanel";
                DetailContent.CssClass = "DetailPanel";
            }
            else
            {
                Panel1.CssClass = "ListPanelHist";
                SummaryPanel.CssClass = "SummaryPanelHist";
                DetailContent.CssClass = "DetailPanelHist";
            }
        }

        protected new void Page_PreRender(object sender, EventArgs e)
        {
            base.Page_PreRender(sender, e);

            if (GridView_Summary.SelectedIndex < 0 && GridView_Summary.Rows.Count == 0)
                ProcessSelection();

            if ((Request.QueryString["AutoBind"] != null) & (Session["AutoRefreshTimer"] != null) && (int.Parse(Session["AutoRefreshTimer"].ToString()) > 0))
            {
                string script = @"
				<script language='javascript'>
			    var counterSecs = 0;
					function ajax_countup()
					{
						element = document.getElementById('" + lbAjaxCounter.ClientID + @"');
						if (element)
						{
							if (element.innerHTML == '0' & counterSecs > 1)
							{
								counterSecs = 0;
							}
							element.innerHTML = counterSecs;
							counterSecs++;
						}
					}
					setInterval ( 'ajax_countup()', 1000 );
				</script>";

                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ajax_countup", script);

                // While editing disable timer
                if (DetailsView1.CurrentMode == DetailsViewMode.ReadOnly)
                    Timer1.Enabled = true;
                else
                    Timer1.Enabled = false;
                Timer1.Interval = int.Parse(Session["AutoRefreshTimer"].ToString()) * 1000;
                UpdateCounter.Visible = true;
            }
            else
            {
                Timer1.Enabled = false;
                UpdateCounter.Visible = false;
            }

            lbTotalRecords.Text = _Session("TotalGridRecords");
            if (PageID.ToLower() == "wa11f1p1" || PageID.ToLower() == "wa11f1p7b")
                cbActive.Text = lbActive.Text;
            if (PageID.ToLower() == "wa55f1p1")
                cbActive.Text = lbToday.Text;

            if (DebugPanel.Visible)
            {
                Debug_lPageID.Text = "PageID: " + PageID;
                Debug_lUserID.Text = "UserID: " + UserID;
                Debug_lPermissions.Text = "Permissions: " + PageSecurity.ToString();
                Debug_lDatabaseName.Text = "DatabaseName: " + DatabaseName;
                Debug_lServer.Text = "ServerName: " + ServerName;
                Debug_lCulture.Text = "Browser Culture: " + Page.Culture;
                Debug_lSessionCached.Text = (pageIDChanged) ? " New PageID on this call / postback - Session vars / translations reloaded" : " Postback to the same PageID - no Session vars or translations reloaded";
                Debug_Gridview1.DataSource = (DataTable)Session["PageSessionVars"];
                Debug_Gridview1.DataBind();

                DataTable pageStrings = (DataTable)Session["PageStringTable"];
                pageStrings.Merge((DataTable)Session["ColumnTranslations"]);
                Debug_GridView2.DataSource = pageStrings;
                Debug_GridView2.DataBind();
            }
        }

        protected void DetailDataBound(object sender, EventArgs e)
        {
            if (DetailsView1.CurrentMode == DetailsViewMode.ReadOnly || DetailsView1.CurrentMode == DetailsViewMode.Edit)
            {
                int r = GetDetailViewRow("Hyperlink");
                if (r > 0 && DetailsView1.Rows.Count > 1)
                {
                    DataControlFieldCell cell = (DataControlFieldCell)DetailsView1.Rows[r].Cells[1];
                    Label lbl = cell.Controls[0] as Label;
                    if (lbl != null && IsValidURL(lbl.Text)) lbl.Text = "<a onClick=\"window.open('" + lbl.Text + "');return false;\" href=\"" + lbl.Text + "\"><b>" + lbl.Text + "</b></a>";
                }
            }
        }

        /// <summary>
        /// BindFilter is run from the master after add filter, clear filter, or run SQL filter are clicked
        /// </summary>
        protected void BindFilter()
        {
            Session[Session["CurPageName"].ToString() + "Filter"] = Master.Where.Trim();
            Session["Filter"] = Master.Where.Trim();
            Session["RecalcSelection"] = true;
            if (Regex.IsMatch(Master.Where.Trim(), @"ScheduledDate") && PageID.ToLower() == "wa55f1p1")
            {
                cbActive.Checked = false;
                Session["ShowObsolete"] = cbActive.Checked ? "1" : "0";
            }

            GridView_Summary.DataBind();
            Master.SQLText.Text = Session["Filter"].ToString();
            // Have to set the value in the master this way because I can't get the viewstate data from the master.
            Master.FilterData = (DataTable)ViewState["SASData"];

            if (DetailMode.Length == 0)
                DetailsView1.ChangeMode(DetailsViewMode.ReadOnly);
            ProcessSelection();
            FixDetailUpdateTimingProblem();
        }

        protected int GetDetailViewRow(string column)
        {
            string Columns = _Session("DetailsColumnShown").Trim();
            int r = 0;

            DataTable dt = new DataTable("ColumnShown");

            // Get list of hidden columns so we can prevent them from being added...
            Dictionary<string, int> hideCols = HttpContext.Current.Session["HiddenColumns"] as Dictionary<string, int>;

            string[] columnsStr = Columns.Split(new char[] { ',' });
            int colOrder = 1;
            foreach (string col in columnsStr)
            {
                if (col.Length > 0 && !dt.Columns.Contains(col) && !hideCols.ContainsKey(col.ToLower()))
                {
                    if (col == column)
                    {
                        r = colOrder;
                        break;
                    }
                    colOrder++;
                }
            }
            return r;
        }

        protected void FixDetailUpdateTimingProblem()
        {
            // Force a DataBind of the detail view -- This is to resolve an update timing bug that sometimes leaves the detail with the incorrect data
            if (GridView_Summary.Rows.Count > 0 && GridView_Summary.QueryColumnIndex >= 0)
                if (GridView_Summary.SelectedRow != null)
                    DetailsView1.DataBind();
        }

        public override void ProcessSelection()
        {
            if (GridView_Summary.Rows.Count > 0 && GridView_Summary.QueryColumnIndex >= 0)
            {
                if (GridView_Summary.SelectedIndex == -1 && GridView_Summary.Rows.Count > 0)
                    GridView_Summary.SelectedIndex = 0;

                if (GridView_Summary.Rows.Count <= GridView_Summary.SelectedIndex)
                    GridView_Summary.SelectedIndex = GridView_Summary.Rows.Count - 1;

                if (GridView_Summary.SelectedRow != null)
                {
                    if (_Session("SelectedItemKey") != GridView_Summary.SelectedRow.Cells[GridView_Summary.QueryColumnIndex].Text)
                    {
                        Session["SelectedItemKey"] = GridView_Summary.SelectedRow.Cells[GridView_Summary.QueryColumnIndex].Text;
                        Session[PageID + "SelectedItemKey"] = Session["SelectedItemKey"];
                        MarkSecurityStale();
                        if (DetailsView1.CurrentMode != DetailsViewMode.ReadOnly)
                            DetailsView1.ChangeMode(DetailsViewMode.ReadOnly);
                        if (Request.QueryString["SeparateDetail"] == null)
                            DetailsView1.DataBind();
                        if (NoDetailColumns.ContainsKey(PageID.ToLower()))
                            ibDetails.Visible = false;
                        else
                            ibDetails.Visible = !IsHistoryPage;
                    }
                }
            }
            else
            {
                if (GridView_Summary.QueryColumnIndex == -1)
                {
                    if (NoDetailColumns.ContainsKey(PageID.ToLower()))
                        ibDetails.Visible = false;
                    else
                        ibDetails.Visible = !IsHistoryPage;
                    DetailsView1.Visible = false;
                }
                else // there are no rows
                {
                    // We only want to run this once when we are in insert mode otherwise it keeps wiping out what we enter on a postback from combobox changes.
                    if (DetailsView1.CurrentMode != DetailsViewMode.Insert) ViewState["nullInsertMode"] = false;
                    if (ViewState["nullInsertMode"] != null && !(bool)ViewState["nullInsertMode"])
                    {
                        if (DetailsView1.CurrentMode == DetailsViewMode.Insert) ViewState["nullInsertMode"] = true;
                        ibDetails.Visible = false;
                        ibBulkEdit.Visible = false;
                        Session["SelectedItemKey"] = "";
                        Session[PageID + "SelectedItemKey"] = "";
                        MarkSecurityStale();
                        DetailsView1.DataBind();
                    }
                }
            }

            if (CanEdit && DetailMode.StartsWith("E")) // Edit
            {
                DetailsView1.DefaultMode = DetailsViewMode.Edit;
                DetailsView1.ChangeMode(DetailsViewMode.Edit);
            }
            else if (CanInsert && DetailMode.StartsWith("I")) // Insert
            {
                DetailsView1.DefaultMode = DetailsViewMode.Insert;
                DetailsView1.ChangeMode(DetailsViewMode.Insert);
            }
        }

        public override string NoDataGridMsg
        {
            get
            {
                if (_Session("Filter").Length == 0 && _Session(PageID + "StoredProcForSelect").Length > 0)
                    return PCSTranslation.GetMessage("NoDataNeedFilter");
                return PCSTranslation.GetMessage("NoData");
            }
        }

        protected void ibConfigFilter_Click(object sender, ImageClickEventArgs e)
        {
            string redirUrl = string.Format("ColumnSettings.aspx?PageID={0}", Server.UrlEncode(_Session("CurPageName")));
            Response.Redirect(redirUrl);
        }

        protected void ibExcel_Click(object sender, ImageClickEventArgs e)
        {
            ExportToExcel(Master.SQLText.Text);
        }

        protected void ibHist_Click(object sender, ImageClickEventArgs e)
        {
            InvokeHistoryPage();
        }

        protected void ibDetails_Click(object sender, ImageClickEventArgs e)
        {
            string redirUrl = "";
            if (_Session("SpecialDetail").Length > 0)
            {
                string detailsPage = _Session("SpecialDetail");
                redirUrl = string.Format("{0}{1}Selection={2}", detailsPage, (detailsPage.IndexOf('?') >= 0) ? "&" : "?", Server.UrlEncode(_Session("SelectedItemKey")));
            }
            else
                redirUrl = string.Format("Detail.aspx?PageID={0}&NodeID={1}&Selection={2}", Server.UrlEncode(_Session("CurPageName")), Request.QueryString["NodeID"], Server.UrlEncode(_Session("SelectedItemKey")));
            if (redirUrl.Length > 0)
                Response.Redirect(redirUrl);
        }

        protected void ibBulkEdit_Click(object sender, ImageClickEventArgs e)
        {
            Session["BulkEditQuickFilter"] = Master.QuickFilter.Text;
            string redirUrl = "";
            if (_Session("BulkEdit").Length > 0)
            {
                string editPage = _Session("BulkEdit");
                redirUrl = string.Format("{0}{1}", editPage, (editPage.IndexOf('?') >= 0) ? "&" : "?");
            }
            if (redirUrl.Length > 0)
                Response.Redirect(redirUrl);
        }

        protected void OnInsertButtonClick(object sender, ImageClickEventArgs e)
        {
            DetailsView1.ChangeMode(DetailsViewMode.Insert);
        }

        protected void ibRetrieve_Click(object sender, ImageClickEventArgs e)
        {
            if (DetailMode.Length == 0)
                DetailsView1.ChangeMode(DetailsViewMode.ReadOnly);

            if (Regex.IsMatch(tbNumOfRows.Text.Trim(), @"^\d+$") && int.Parse(tbNumOfRows.Text) > 0)
            {
                Session[Session["CurPageName"].ToString() + "NumOfRows"] = tbNumOfRows.Text;
                Session["NumOfRows"] = tbNumOfRows.Text;
                GridView_Summary.DataBind();
                GridView_Summary.PageSize = (Session["NumOfRows"] != null) ? int.Parse(Session["NumOfRows"].ToString()) : 15;
                DetailsView1.DataBind();
            }
            else
            {
                string ErrorMessage = "[" + DateTime.Now.ToString() + "] " + "Rows must be a number greater than zero";
                ContentHelper.SetMainContent(ErrorMessage);
            }
        }

        protected void MarkCacheStale(object sender, EventArgs e)
        {
            MarkSecurityStale();
            Cache[ObjectDataSource1.CacheKeyDependency] = new object();
        }

        protected void OnViewReport(object sender, EventArgs e)
        {
            if (Request.QueryString["RPTPageID"] != null)
                Response.Redirect("ReportViewer.aspx?PageID=" + Request.QueryString["RPTPageID"] + "&NodeID=" + Request.QueryString["RPTPageID"] + "&ProcNumber=" + _Session("SelectedItemKey"));
        }

        protected void cbActiveChanged(object sender, EventArgs e)
        {
            Session["ShowObsolete"] = cbActive.Checked ? "1" : "0";
            Cache[ObjectDataSource1.CacheKeyDependency] = new object();
            GridView_Summary.DataBind();
        }
    }
}