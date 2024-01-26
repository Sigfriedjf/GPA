using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;
using LMS2.components;

namespace GPA
{
    public partial class Detail : BasePage
    {
        protected override string UserIDDisplay { set { lbUserID.Text = value; } }
        protected override bool UserChangeTimer { get { return tmUserChangeTimer.Enabled; } set { tmUserChangeTimer.Enabled = value; } }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (CanEdit && DetailMode.StartsWith("E")) // Edit
            {
                DetailsView1.DefaultMode = DetailsViewMode.Edit;
                DetailsView1.ChangeMode(DetailsViewMode.Edit);
            }
            else if (CanEdit && DetailMode.StartsWith("I")) // Edit
            {
                DetailsView1.DefaultMode = DetailsViewMode.Insert;
                DetailsView1.ChangeMode(DetailsViewMode.Insert);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DetailsView1.EditRowStyle.BackColor = System.Drawing.ColorTranslator.FromHtml(ColorManager.ColorGridSelectedRow);
            DebugPanel.Visible = (Session["ShowPageDebugInfo"] != null) ? (Session["ShowPageDebugInfo"].ToString() == "1") : false;
            ((LMSGridViewFilter)(Master)).FilteringEnabled = false;
        }

        protected new void Page_PreRender(object sender, EventArgs e)
        {
            base.Page_PreRender(sender, e);
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

        protected void ItemUpdated(object sender, EventArgs e)
        {
            if (DetailsView1.DefaultMode != DetailsViewMode.ReadOnly && Request.QueryString["GoBack"] != null)
                OrderEntryProcessing();
        }

        protected void ItemInserted(object sender, EventArgs e)
        {
            if (DetailsView1.DefaultMode != DetailsViewMode.ReadOnly && Request.QueryString["GoBack"] != null)
                OrderEntryProcessing();
            else if (ViewState["HttpReferer"] != null)
                Response.Redirect(ViewState["HttpReferer"].ToString());
        }

        protected void ItemDeleted(object sender, EventArgs e)
        {
            if (ViewState["HttpReferer"] != null)
                Response.Redirect(ViewState["HttpReferer"].ToString());
        }

        protected void OrderEntryProcessing()
        {
            // If a GoBack has been set & the default mode was not read-only, then transfer

            // Need to get recordType another way but I'm tired
            //string ID811 = string.Empty;
            //DataSet ds = new DataSet();
            //if ((Page as BasePage).PageID.StartsWith("WA41"))
            //    ds = SASWrapper.ExecuteQuery(DatabaseName, string.Format("select max(id811) from [811Report] where updatedby = '{0}' and RecordType = 'BH'", UserID));
            //if ((Page as BasePage).PageID.StartsWith("WA42"))
            //    ds = SASWrapper.ExecuteQuery(DatabaseName, string.Format("select max(id811) from [811Report] where updatedby = '{0}' and RecordType = 'P'", UserID));
            //if ((Page as BasePage).PageID.StartsWith("WA43"))
            //    ds = SASWrapper.ExecuteQuery(DatabaseName, string.Format("select max(id811) from [811Report] where updatedby = '{0}' and RecordType = 'LH'", UserID));
            //if ((Page as BasePage).PageID.StartsWith("WA46"))
            //    ds = SASWrapper.ExecuteQuery(DatabaseName, string.Format("select max(id811) from [811Report] where updatedby = '{0}' and RecordType = 'U'", UserID));
            //if ((Page as BasePage).PageID.StartsWith("WA49"))
            //    ds = SASWrapper.ExecuteQuery(DatabaseName, string.Format("select max(id811) from [811Report] where updatedby = '{0}' and RecordType = 'T'", UserID));
            //if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            //    ID811 = ds.Tables[0].Rows[0][0].ToString();

            //string url = string.Empty;
            //// special handling for unloading pages...
            //if (PageID.ToLower() == "wa31f12p4" || PageID.ToLower() == "wa31f13p4")
            //{
            //    url = string.Format("{0}&Filter=ProcessNumber%3d'{1}'", Request.QueryString["GoBack"], Server.UrlEncode(BasePage._Session("SelectedItemKey")));
            //    Session["PageScopeColumn"] = "ProcessNumber";
            //    Session["PageScopeFilterColumn"] = "ProcessNumber";
            //    Session["PageScopeValue"] = _Session("SelectedItemKey");
            //    Session["PageScopeRefPageID"] = PageID.Substring(0, PageID.Length - 1) + '3'; // set to P3 of the current pageid
            //    Session["PageScopePageID"] = PageID.Substring(0, PageID.Length - 1) + '1'; // set to P1 of the current pageid
            //    Session["PageIsFilterScoped"] = true;
            //}
            //else
            string url = string.Format("{0}&Selection={1}", Request.QueryString["GoBack"], Page.Server.UrlEncode(_Session("SelectedItemKey")));
            //url = string.Format("{0}&Selection={1}", Request.QueryString["GoBack"], ID811);

            if (((LMSGridViewFilter)(Master)).ErrorMessage.Length == 0)
                Response.Redirect(url, true);
        }
    }
}