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

namespace GPAutomation
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

            //((LMSGridView)Master).FilteringEnabled = false;
            BackButton.Visible = false;

            if (CanEdit && DetailMode.StartsWith("E")) // Edit
            {
                //FormView1.DefaultMode = FormViewMode.Edit;
                //FormView1.ChangeMode(FormViewMode.Edit);
            }
            else if (CanInsert && DetailMode.StartsWith("I")) // Insert
            {
                //FormView1.DefaultMode = FormViewMode.Insert;
                //FormView1.ChangeMode(FormViewMode.Insert);
            }

            if (PageID == "WA22F1P1A" && CanEdit)
                LabXferTable.Visible = true;
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