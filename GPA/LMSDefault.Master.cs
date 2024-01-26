using System;
using System.Collections;
using System.Configuration;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.IO;
using LMS2.components;
using System.Runtime.Remoting.Messaging;

public partial class LMSDefault : System.Web.UI.MasterPage
{
    private string sPageName;

    /// <summary>
    /// Set the help page URL
    /// </summary>
    public string HelpPage
    {
        get { return sPageName; }
        set { sPageName = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sUserID = string.Empty;
        if (Request.QueryString["src"] != null || Request.QueryString["src"] == "ww")
            Session["WonderWare"] = "ww";
        if (Request.QueryString["UID"] != null && Request.QueryString["UID"].Length > 0)
        {
            sUserID = Request.QueryString["UID"];
            Session["UserID"] = sUserID;
        }
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
                    Session["DBInstanceName"] = strInstance;
            }
        }


        DataSet ds = SASWrapper.InitializeWebPage("", "", BasePage._Session("UserID").Length > 0 ? BasePage._Session("UserID") : sUserID);
        if (ds != null && ds.Tables.Count > 0)
        {
            DataTable dt = ds.Tables[0];
            if (dt.Columns.Contains("Result"))
            {
                foreach (DataRow row in dt.Rows)
                    Session[row["Name"].ToString()] = row["Result"];
            }
        }

        Head1.Title = BasePage._Session("WebsiteTitle");
        hlExxonLogo.ImageUrl = (BasePage._Session("FacilityID") == "WAK") ? "~/Images/exxonmobil_WAK.gif" : "~/Images/exxonmobil_BMT.gif";
        divBrandingBar.Visible = (Session["BrandingBar"] != null && Session["BrandingBar"].ToString() == "1");
        ibAlt.Visible = ibEnglish.Visible = (Session["AllowAltLanguage"] != null && bool.Parse(Session["AllowAltLanguage"].ToString().ToLower()));
        ibAlt.ImageUrl = (BasePage._Session("AltFlag").Length > 0) ? "~/Images/" + BasePage._Session("AltFlag") : "~/Images/US_sml.gif";
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        PopulateRootLevel();
    }

    private void PopulateRootLevel()
    {
        string error = string.Empty;
        DataSet dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetTreeNodes", new string[] { "@UserID", "@ParentID" }, new string[] { (Session["UserID"] != null) ? Session["UserID"].ToString() : "", "" }, Session["DatabaseName"].ToString(), ref error);
        if (dsWork != null && dsWork.Tables.Count > 0)
        {
            PopulateNodes(dsWork.Tables[0].Copy(), TreeView1.Nodes);
            TreeView1.CollapseAll();
        }
        if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
        {
            ibAlt.ToolTip = dsWork.Tables[0].Rows[0]["AltFlagToolTip"].ToString();
            ibEnglish.ToolTip = dsWork.Tables[0].Rows[0]["USFlagToolTip"].ToString();
        }
    }

    private void PopulateNodes(DataTable dt, TreeNodeCollection nodes)
    {
        foreach (DataRow dr in dt.Rows)
        {
            TreeNode tn = new TreeNode();
            tn.Text = dr["Alias"].ToString();
            tn.Value = dr["NodeID"].ToString();
            nodes.Add(tn);

            // If node has child nodes, then enable on-demand populating
            if ((int)dr["childnodecount"] > 0)
            {
                PopulateSubLevel(tn.Value, tn);
                tn.SelectAction = TreeNodeSelectAction.Expand;
            }
            else
            {
                // Run stored proc to get the page to redirect to
                string error = string.Empty;
                DataSet dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetNavTreeURL", new string[] { "@TreeValue" }, new string[] { tn.Value }, Session["DatabaseName"].ToString(), ref error);
                if (dsWork != null && dsWork.Tables.Count > 0 && dsWork.Tables[0].Rows.Count > 0)
                {
                    string sURL = (dsWork.Tables[0].Rows[0].ItemArray[3] != null) ? dsWork.Tables[0].Rows[0].ItemArray[3].ToString() : string.Empty;
                    //if (sURL.ToLower().IndexOf("reportviewer.aspx") >= 0 && Session["WonderWare"] != null)
                    //    tn.NavigateUrl = "javascript:OpenWindow('" + sURL + "');void(0);";
                    //else
                    tn.NavigateUrl = (!string.IsNullOrEmpty(sURL)) ? "javascript:UpdateContent('" + sURL + "');" : "javascript:void(0);";
                }
            }
        }
    }

    private void PopulateSubLevel(string parentid, TreeNode parentNode)
    {
        string error = string.Empty;
        DataSet dsWork = SASWrapper.QueryStoredProc_ResultSet("uspGetTreeNodes", new string[] { "@UserID", "@ParentID" }, new string[] { (Session["UserID"] != null) ? Session["UserID"].ToString() : "", parentid }, Session["DatabaseName"].ToString(), ref error);
        if (dsWork != null && dsWork.Tables.Count > 0)
            PopulateNodes(dsWork.Tables[0].Copy(), parentNode.ChildNodes);
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

    protected void ibAlt_Click(object sender, ImageClickEventArgs e)
    {
        DataSet ds = new DataSet();
        DataTable dt = ds.Tables.Add("LanguageIndicator");
        dt.Columns.Add("UserID");
        dt.Columns.Add("AltLanguageIndicator");
        string[] sArr = { (Session["UserID"] != null) ? Session["UserID"].ToString() : "", "1" };
        dt.Rows.Add(sArr);
        string sTextError = SASWrapper.UpdateData("uspLMS_AppsUserPreferenceUpdateLanguage", Session["DatabaseName"].ToString(), ds);
        Session["CurPageName"] = string.Empty;
    }

    protected void ibEnglish_Click(object sender, ImageClickEventArgs e)
    {
        DataSet ds = new DataSet();
        DataTable dt = ds.Tables.Add("LanguageIndicator");
        dt.Columns.Add("UserID");
        dt.Columns.Add("AltLanguageIndicator");
        string[] sArr = { (Session["UserID"] != null) ? Session["UserID"].ToString() : "", "0" };
        dt.Rows.Add(sArr);
        string sTextError = SASWrapper.UpdateData("uspLMS_AppsUserPreferenceUpdateLanguage", Session["DatabaseName"].ToString(), ds);
        Session["CurPageName"] = string.Empty;
    }

}