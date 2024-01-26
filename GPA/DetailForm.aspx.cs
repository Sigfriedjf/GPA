using LMS2.components;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GPA
{
    public partial class DetailForm : BasePage
    {
        protected override string UserIDDisplay { set { lbUserID.Text = value; } }
        protected override bool UserChangeTimer { get { return tmUserChangeTimer.Enabled; } set { tmUserChangeTimer.Enabled = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            DebugPanel.Visible = (Session["ShowPageDebugInfo"] != null) ? (Session["ShowPageDebugInfo"].ToString() == "1") : false;

            // The first time into the page, save the referrer so we can get back to it.
            //if (!IsPostBack)
            //{
            //  if (ViewState["HttpReferer"] == null && Request.ServerVariables["HTTP_REFERER"] != null)
            //    ViewState["HttpReferer"] = Path.GetFileName(Request.ServerVariables["HTTP_REFERER"]);
            //  else
            //    BackButton.Visible = false;
            //}

           ((LMSGridViewFilter)Master).FilteringEnabled = false;
            BackButton.Visible = false;

            if (CanEdit && DetailMode.StartsWith("E")) // Edit
            {
                FormView1.DefaultMode = FormViewMode.Edit;
                FormView1.ChangeMode(FormViewMode.Edit);
            }
            else if (CanInsert && DetailMode.StartsWith("I")) // Insert
            {
                FormView1.DefaultMode = FormViewMode.Insert;
                FormView1.ChangeMode(FormViewMode.Insert);
            }

            if (PageID == "WA22F1P1A" && CanEdit)
                LabXferTable.Visible = true;
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

        protected void BackButtonClick(object sender, ImageClickEventArgs e)
        {
            if (ViewState["HttpReferer"] != null)
                Response.Redirect(ViewState["HttpReferer"].ToString());
        }

        protected void FormViewItemInserted(object sender, EventArgs e)
        {
            if (ViewState["HttpReferer"] != null)
                Response.Redirect(ViewState["HttpReferer"].ToString());
        }

        protected void FormViewItemDeleted(object sender, EventArgs e)
        {
            if (ViewState["HttpReferer"] != null)
                Response.Redirect(ViewState["HttpReferer"].ToString());
        }

        protected virtual void FormViewModeChanging(Object sender, FormViewModeEventArgs e)
        {
            // Use the NewMode property to determine the mode to which the 
            // FormView control is transitioning.
            switch (e.NewMode)
            {
                case FormViewMode.Edit:
                    if ((PageID == "ShiftHandoffEntering" || PageID == "ShiftHandoffLeaving") && _Session(PageID + "IncludeEditInd") == "1")
                    {
                        Session[PageID + "SelectedItemKey"] = UserID + "$EDIT$";
                        Session["SelectedItemKey"] = UserID + "$EDIT$";
                    }
                    else if (_Session(PageID + "IncludeEditInd") == "1")
                    {
                        Session[PageID + "SelectedItemKey"] = Session[PageID + "SelectedItemKey"].ToString().Replace("$EDIT$", string.Empty) + "$EDIT$";
                        Session["SelectedItemKey"] = Session["SelectedItemKey"].ToString().Replace("$EDIT$", string.Empty) + "$EDIT$";
                    }
                    break;
                case FormViewMode.ReadOnly:
                    if (PageID != "ShiftHandoffEntering" && PageID != "ShiftHandoffLeaving" && _Session(PageID + "IncludeEditInd") == "1")
                    {
                        Session[PageID + "SelectedItemKey"] = Session[PageID + "SelectedItemKey"].ToString().Replace("$EDIT$", string.Empty);
                        Session["SelectedItemKey"] = Session["SelectedItemKey"].ToString().Replace("$EDIT$", string.Empty);
                    }
                    break;
                case FormViewMode.Insert:
                    break;
                default:
                    break;
            }
        }

        protected void OnLabXfer(object sender, EventArgs e)
        {
            string err = "";
            SASWrapper.QueryStoredProc_ResultSet("uspLMS_CopyLabDataFromTankToTank",
                new string[] { "@FromTank", "@ToTank", "@UpdatedBy" },
                new string[] { _Session("SelectedItemKey"), tbTankName.Text.Trim().ToUpper(), UserID },
                DatabaseName, ref err);

            if (err.Length == 0)
            {
                lbLabXferOk.Visible = true;
                LabXferButton.Visible = false;
                tbTankName.Visible = false;
            }

            FormView1.DataBind();
        }
    }
}