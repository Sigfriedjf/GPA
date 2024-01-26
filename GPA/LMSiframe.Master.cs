using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GPA
{
    public partial class LMSiframe : System.Web.UI.MasterPage, IBasePageDefault
    {
        /// <summary>
        /// Set Error Message
        /// </summary>
        public string ErrorMessage
        {
            get { return lblErrorMsg.Text; }
            set
            {
                lblErrorMsg.Text = value;
                ErrorPanel.Visible = value.Trim().Length > 0 ? true : false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Helper method
            ContentHelper.BuildMainContent(this);
            ErrorPanel.Visible = lblErrorMsg.Text.Trim().Length > 0 ? true : false;
            string sOnLoad = string.Empty;
            if (!Request.FilePath.EndsWith("ReportViewer.aspx"))
                body1.Attributes.Add("onload", "LoadAfterAjaxHandler()");
            //if (Request.QueryString["src"] != null || Request.QueryString["src"] == "ww")
            //    body1.Attributes.Add("oncontextmenu", "return false;");
        }

    }
}