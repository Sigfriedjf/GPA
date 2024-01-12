using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LMS2.components
{
    public class PCSFormView : FormView
    {
        public PCSFormView()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SetupDataFields();
        }

        private void SetupDataFields()
        {
            // For some reason this gets called on each databind and the templates have all been reset to null each time, so rather than recreate
            // the templates each time we click a new summary row, I am squirreling them away into the view state. 

            if (ViewState["DetailLayout"] == null)
            {
                bool is811Page = ((Page as BasePage).PageID.StartsWith("WA4") || (Page as BasePage).PageID.StartsWith("WA51") || (Page as BasePage).PageID == "WA53F1P1");
                //string pageID = (is811Page) ? "811ReportDetail" : (Page as BasePage).PageID;
                string pageID = (is811Page) ? "811ReportDetail" : "811ReportDetail";
                string layoutFile = string.Format("DetailLayouts//{0}.xml", pageID);
                string siteSpecificLayoutFile = string.Format("DetailLayouts//{0}_{1}.xml", pageID, (Page as BasePage).FacilityID);
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + siteSpecificLayoutFile))
                    layoutFile = siteSpecificLayoutFile;

                // Make sure we don't operate in a mode we don't have permissions to use.
                if (DefaultMode == FormViewMode.Edit && !(Page as BasePage).CanEdit)
                    ChangeMode(FormViewMode.ReadOnly);
                else if (DefaultMode == FormViewMode.Insert && !(Page as BasePage).CanInsert)
                    ChangeMode(FormViewMode.ReadOnly);

                ViewState["DetailLayout"] = new DetailLayout(Page as BasePage, layoutFile, DefaultMode);
            }
            DetailLayout layout = (DetailLayout)ViewState["DetailLayout"];

            if (layout != null)
            {
                if (ViewState["ItemTemplate"] == null)
                    ViewState["ItemTemplate"] = layout.CreateItemTemplate();
                ItemTemplate = (ITemplate)ViewState["ItemTemplate"];

                if (ViewState["EditItemTemplate"] == null)
                    ViewState["EditItemTemplate"] = layout.CreateEditTemplate();
                EditItemTemplate = (ITemplate)ViewState["EditItemTemplate"];

                if (ViewState["InsertItemTemplate"] == null)
                    ViewState["InsertItemTemplate"] = layout.CreateInsertTemplate();
                InsertItemTemplate = (ITemplate)ViewState["InsertItemTemplate"];
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            if (CurrentMode == FormViewMode.ReadOnly)
            {
                // Squirrel away current PK value in case we want to delete it...
                // Ultra sneaky back door I built into our SASDetailDataSource since I couldn't find a way in to get at the data.
                // I could tell you more about it, but then I'd have to kill you.
                SASDetailDataSourceView dsView = (DataSourceObject as SASDetailDataSource)._dataView;
                if (dsView != null)
                {
                    DataSet ds = dsView._ds;
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                            ViewState["DetailsPKValue"] = dt.Rows[0][HttpContext.Current.Session["PrimaryKeyColumn"].ToString()];
                    }
                }

            }

            if (CurrentMode == FormViewMode.Edit)
            {
                // Save current values away so we can get the defaults when processing edits later in OnItemUpdating
                // Ultra sneaky back door I built into our SASDetailDataSource since I couldn't find a way in to get at the data.
                // I could tell you more about it, but then I'd have to kill you.
                SASDetailDataSourceView dsView = (DataSourceObject as SASDetailDataSource)._dataView;
                if (dsView != null)
                {
                    DataSet ds = dsView._ds;
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        ViewState["DetailsOldValues"] = dt.Copy();
                    }
                }
            }
        }

        void tb_TextChanged(object sender, EventArgs e)
        {
            TextBox tb1 = sender as TextBox;
            List<Control> controlList = new List<Control>();
            RetrieveRecursively(this, ref controlList, typeof(DetailLayout.LayoutTextBox));
            double totalHours = 0;

            foreach (DetailLayout.LayoutTextBox tb2 in controlList)
            {
                if (((Page as BasePage).PageID == "ShiftHandoffLeaving") && (tb2._rowItem.DBColumnName.StartsWith("RespHours")))
                {
                    double dblValue;
                    if (double.TryParse(tb2.Text, out dblValue) == true)
                        totalHours = totalHours + dblValue;
                }
                else
                {
                    if ((tb2._rowItem.DBColumnName == "StdDensity") || (tb2._rowItem.DBColumnName == "Density"))
                    {
                        if (tb1.Text.Trim().Length == 0) tb1.Text = "0";
                        DataSet ds2 = SASWrapper.ExecuteQuery(HttpContext.Current.Session["DatabaseName"].ToString(), string.Format("Select dbo.uf_PoundPerGallon({0})", tb1.Text));
                        if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                        {
                            double Density = double.Parse(ds2.Tables[0].Rows[0][0].ToString());
                            tb2.Text = string.Format("{0:F4}", Density);
                        }
                    }
                }
            }
            if ((Page as BasePage).PageID == "ShiftHandoffLeaving")
                foreach (DetailLayout.LayoutTextBox tb2 in controlList)
                {
                    if (tb2._rowItem.DBColumnName == "RespTotalHours")
                        tb2.Text = totalHours.ToString();
                }
        }

        protected override void OnItemUpdating(FormViewUpdateEventArgs e)
        {
            base.OnItemUpdating(e);

            // Start by loading array with old values & appropriate types...
            DataTable dt = (DataTable)ViewState["DetailsOldValues"];

            // Process normal text boxes first...
            List<Control> controlList = new List<Control>();
            RetrieveRecursively(this, ref controlList, typeof(DetailLayout.LayoutTextBox));
            foreach (DetailLayout.LayoutTextBox tb in controlList)
                SetValue(dt, tb._rowItem.DBColumnName, tb.Text);

            // Process values from combo boxes...
            controlList.Clear();
            RetrieveRecursively(this, ref controlList, typeof(DetailLayout.LayoutDropDownList));
            foreach (DetailLayout.LayoutDropDownList ddl in controlList)
                SetValue(dt, ddl._rowItem.DBColumnName, ddl.SelectedValue);

            // Load values into new values array for returning to the data source
            for (int i = 0; i < dt.Columns.Count; i++)
                e.NewValues[dt.Columns[i].ColumnName] = dt.Rows[0][i];

            // Check to see if we are creating from a template...
            if (Page.Request.QueryString["SelectColumn"] != null)
                Page.Session["SelectedItemKey"] = e.NewValues[Page.Request.QueryString["SelectColumn"]];
        }

        protected override void OnItemUpdated(FormViewUpdatedEventArgs e)
        {
            base.OnItemUpdated(e);

            // If a GoBack has been set & the default mode was not read-only, then transfer
            if (DefaultMode != FormViewMode.ReadOnly && Page.Request.QueryString["GoBack"] != null)
            {
                string url = string.Format("{0}&Selection={1}", Page.Request.QueryString["GoBack"], Page.Server.UrlEncode(BasePage._Session("SelectedItemKey")));
                Page.Response.Redirect(url, true);
            }
        }

        protected override void OnItemInserting(FormViewInsertEventArgs e)
        {
            base.OnItemInserting(e);

            // We need to get the full column set so we know what to return
            // Use a bogus filter to return no data since I'm only interested in the columns
            DataTable dt = SASWrapper.QueryData(HttpContext.Current.Session["DatabaseName"].ToString(), "", "*", "0=1").Tables[0];
            dt.Rows.Add(dt.NewRow());

            // Process normal text boxes first...
            List<Control> controlList = new List<Control>();
            RetrieveRecursively(this, ref controlList, typeof(DetailLayout.LayoutTextBox));
            foreach (DetailLayout.LayoutTextBox tb in controlList)
                SetValue(dt, tb._rowItem.DBColumnName, tb.Text);

            // Process values from combo boxes...
            controlList.Clear();
            RetrieveRecursively(this, ref controlList, typeof(DetailLayout.LayoutDropDownList));
            foreach (DetailLayout.LayoutDropDownList ddl in controlList)
                SetValue(dt, ddl._rowItem.DBColumnName, ddl.SelectedValue);

            bool scoped = ((BasePage)Page).IsPageFilterScoped;
            // Load values into new values array for returning to the data source
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (scoped && (dt.Columns[i].ColumnName.ToLower() == BasePage._Session("PageScopeColumn").ToLower()))
                    e.Values[dt.Columns[i].ColumnName] = BasePage._Session("PageScopeValue");
                else
                    e.Values[dt.Columns[i].ColumnName] = dt.Rows[0][i];
            }

            ViewState["DetailsInsertedQueryValue"] = e.Values[HttpContext.Current.Session["PrimaryKeyColumn"].ToString()];
        }

        protected override void OnItemDeleting(FormViewDeleteEventArgs e)
        {
            base.OnItemDeleting(e);
            e.Values[HttpContext.Current.Session["PrimaryKeyColumn"].ToString()] = ViewState["DetailsPKValue"];
        }

        protected override void OnItemCreated(EventArgs e)
        {
            // The purpose of this is to do an autopostback and create a textchanged event for APIGravity so that Density can be calculated on the fly..
            base.OnItemCreated(e);

            // Set up focus retention
            HookOnFocus(this.Page as Control);

            List<Control> controlListTB = new List<Control>();
            List<Control> controlListDDL = new List<Control>();
            RetrieveRecursively(this, ref controlListTB, typeof(DetailLayout.LayoutTextBox));
            RetrieveRecursively(this, ref controlListDDL, typeof(DetailLayout.LayoutDropDownList));
            bool APIGravity = false;
            bool Density = false;
            TextBox tbAPIGravity = new TextBox();
            foreach (DetailLayout.LayoutDropDownList ddl in controlListDDL)
            {
                if (ddl.AutoPostBack == false && (Page as BasePage).PageID == "ShiftHandoffEntering")
                {
                    if (ddl._rowItem.DBColumnName == "ShiftType")
                        ddl.SelectedIndexChanged += new EventHandler(ddl_SelectedShiftTypeIndexChanged);
                    if (ddl._rowItem.DBColumnName.StartsWith("OutgoingOper"))
                        ddl.SelectedIndexChanged += new EventHandler(ddl_SelectedOutgoingOperIndexChanged);
                    ddl.AutoPostBack = true;
                }
            }

            foreach (DetailLayout.LayoutTextBox tb in controlListTB)
            {
                if (tb._rowItem.DBColumnName.StartsWith("RespHours") && tb.AutoPostBack == false && (Page as BasePage).PageID == "ShiftHandoffLeaving")
                {
                    tb.AutoPostBack = true;
                    tb.TextChanged += new EventHandler(tb_TextChanged);
                }

                //This was here for Beaumont
                if (tb._rowItem.DBColumnName == "APIGravity" && tb.AutoPostBack == false)
                {
                    APIGravity = true;
                    tbAPIGravity = tb;
                }
                if ((tb._rowItem.DBColumnName == "StdDensity") || (tb._rowItem.DBColumnName == "Density"))
                    Density = true;
            }
            if (APIGravity && Density)
            {
                tbAPIGravity.AutoPostBack = true;
                tbAPIGravity.TextChanged += new EventHandler(tb_TextChanged);
            }
        }

        void ddl_SelectedShiftTypeIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl1 = sender as DropDownList;
            List<Control> controlListTB = new List<Control>();
            RetrieveRecursively(this, ref controlListTB, typeof(DetailLayout.LayoutTextBox));
            DataSet ds2 = new DataSet();
            double totalHours = 0;

            foreach (DetailLayout.LayoutTextBox tb in controlListTB)
            {
                if (tb._rowItem.DBColumnName.StartsWith("Resp"))
                {
                    if (ds2.Tables.Count == 0) ds2 = SASWrapper.ExecuteQuery(HttpContext.Current.Session["DatabaseName"].ToString(), string.Format("SELECT * FROM vShiftType WHERE ShiftType='{0}'", ddl1.SelectedValue));
                    if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 1; i <= 40; i++)
                        {
                            if (tb._rowItem.DBColumnName == "Responsibility" + i) tb.Text = (ds2.Tables[0].Rows[0]["Responsibility" + i].ToString());
                            if (tb._rowItem.DBColumnName == "RespHours" + i)
                            {
                                tb.Text = (ds2.Tables[0].Rows[0]["RespHours" + i].ToString());
                                totalHours = totalHours + double.Parse(tb.Text);
                            }
                        }
                    }
                    else
                        tb.Text = string.Empty;
                }
            }
            foreach (DetailLayout.LayoutTextBox tb in controlListTB)
            {
                if (tb._rowItem.DBColumnName == "RespTotalHours")
                    tb.Text = totalHours.ToString();
            }

            List<Control> controlListDDL = new List<Control>();
            RetrieveRecursively(this, ref controlListDDL, typeof(DetailLayout.LayoutDropDownList));

            foreach (DetailLayout.LayoutDropDownList ddl in controlListDDL)
            {
                if (ddl._rowItem.DBColumnName.StartsWith("Resp"))
                {
                    if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 1; i <= 40; i++)
                            if (ddl._rowItem.DBColumnName == "RespInd" + i) ddl.SelectedValue = (ds2.Tables[0].Rows[0]["RespIndVal" + i].ToString());
                    }
                    else
                        ddl.SelectedIndex = 0;
                }
            }
        }

        void ddl_SelectedOutgoingOperIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl1 = sender as DropDownList;
            DropDownList ddl2 = sender as DropDownList;
            DropDownList ddl3 = sender as DropDownList;
            List<Control> controlListTB = new List<Control>();
            List<Control> controlListDDL = new List<Control>();
            RetrieveRecursively(this, ref controlListTB, typeof(DetailLayout.LayoutTextBox));
            RetrieveRecursively(this, ref controlListDDL, typeof(DetailLayout.LayoutDropDownList));

            foreach (DetailLayout.LayoutTextBox tb in controlListTB)
            {
                if (tb._rowItem.DBColumnName == "OtherOperatorComments")
                {
                    string oper = string.Empty;
                    string separator = string.Empty;
                    foreach (DetailLayout.LayoutDropDownList ddl in controlListDDL)
                    {
                        if (ddl._rowItem.DBColumnName.StartsWith("OutgoingOper"))
                        {
                            oper = oper + separator + "|" + ddl.SelectedValue + "|";
                            separator = ",";
                        }
                    }
                    string error = string.Empty;
                    DataSet ds = SASWrapper.QueryStoredProc_ResultSet("uspLOG_GetOtherOperatorComments", new string[] { "@UserIDs" }, new string[] { oper }, HttpContext.Current.Session["DatabaseName"].ToString(), ref error);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        tb.Text = ds.Tables[0].Rows[0][0].ToString();
                    else
                        tb.Text = string.Empty;
                }
            }
        }

        protected void RetrieveRecursively(Control ctrl, ref List<Control> list, Type type)
        {
            if (ctrl.GetType() == type)
                list.Add(ctrl);
            foreach (Control c in ctrl.Controls)
                RetrieveRecursively(c, ref list, type);
        }

        protected void SetValue(DataTable dt, string colName, string val)
        {
            DataColumn col = dt.Columns[colName];
            object v = DBNull.Value;
            try
            {
                if (val.Length > 0 || col.DataType == typeof(string))
                {
                    if (col.DataType == typeof(string)) v = val;
                    else if (col.DataType == typeof(DateTime)) v = DateTime.Parse(val);
                    else if (col.DataType == typeof(decimal)) v = decimal.Parse(val);
                    else if (col.DataType == typeof(double)) v = double.Parse(val);
                    else if (col.DataType == typeof(float)) v = float.Parse(val);
                    else if (col.DataType == typeof(long)) v = long.Parse(val);
                    else if (col.DataType == typeof(int)) v = int.Parse(val);
                    else if (col.DataType == typeof(short)) v = short.Parse(val);
                    else if (col.DataType == typeof(bool)) v = bool.Parse(val);
                    else throw new ApplicationException("Unhandled type: " + col.DataType.ToString());
                }
                dt.Rows[0][col] = v;
            }
            catch (Exception ex)
            {
                AppError.LogError("CommitParsing", new ApplicationException(string.Format("Failed to parse '{0}' as {1}", val, col.DataType.ToString()), ex));
            }
        }

        /// <summary>
        /// This script sets a focus to the control with a name to which
        /// REQUEST_LASTFOCUS was replaced. Setting focus heppens after the page
        /// (or update panel) was rendered. To delay setting focus the function
        /// window.setTimeout() will be used.
        /// </summary>
        private const string SCRIPT_DOFOCUS = @"window.setTimeout('DoFocus()', 1);
		function DoFocus()
		{
			try {
				document.getElementById('REQUEST_LASTFOCUS').focus();
			} catch (ex) {}
		}";


        private void HookOnFocus(Control CurrentControl)
        {
            //checks if control is one of TextBox, DropDownList, ListBox or Button
            if ((CurrentControl is TextBox) ||
                (CurrentControl is DropDownList) ||
                (CurrentControl is ListBox) ||
                (CurrentControl is Button))
                //adds a script which saves active control on receiving focus 
                //in the hidden field __LASTFOCUS.
                (CurrentControl as WebControl).Attributes.Add(
                   "onfocus",
                   "try{document.getElementById('__LASTFOCUS').value=this.id} catch(e) {}");
            //checks if the control has children
            if (CurrentControl.HasControls())
                //if yes do them all recursively
                foreach (Control CurrentChildControl in CurrentControl.Controls)
                    HookOnFocus(CurrentChildControl);
        }

    }
}