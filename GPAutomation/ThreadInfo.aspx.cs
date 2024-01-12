using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using System.Data.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using System.Xml;
using System.Globalization;
//using LMS2.components;

namespace GPAutomation
{
    public partial class ThreadInfo : System.Web.UI.Page
    {
        protected string UserIDDisplay { set { lbUserID.Text = value; } }
        protected bool UserChangeTimer { get { return tmUserChangeTimer.Enabled; } set { tmUserChangeTimer.Enabled = value; } }

       
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void IbRetrieve(object sender, ImageClickEventArgs e)
        {
            if (Regex.IsMatch(tbNumOfRows.Text.Trim(), @"^\d+$") && int.Parse(tbNumOfRows.Text) > 0)
            {
                Session[Session["CurPageName"].ToString() + "NumOfRows"] = tbNumOfRows.Text;
                Session["NumOfRows"] = tbNumOfRows.Text;
                //GridView_Summary.DataBind();
               // GridView_Summary.PageSize = (Session["NumOfRows"] != null) ? int.Parse(Session["NumOfRows"].ToString()) : 15;
            }
            else
            {
                string ErrorMessage = "[" + DateTime.Now.ToString() + "] " + "Rows must be a number greater than zero";
                //ContentHelper.SetMainContent(ErrorMessage);
            }
        }

        protected void ibConfigFilter_Click(object sender, ImageClickEventArgs e)
        {
            //string redirUrl = string.Format("ColumnSettings.aspx?PageID={0}", Server.UrlEncode(_Session("CurPageName")));
            string redirUrl = "";
            Response.Redirect(redirUrl);
        }

        protected void ibExcel_Click(object sender, ImageClickEventArgs e)
        {
            //ExportToExcel(Master.SQLText.Text);
        }

        protected void ibDetails_Click(object sender, ImageClickEventArgs e)
        {
            /*
            string redirUrl = "";
            if (_Session("SpecialDetail").Length > 0)
            {
                string detailsPage = _Session("SpecialDetail");
                redirUrl = string.Format("{0}{1}ThreadID={2}", detailsPage, (detailsPage.IndexOf('?') >= 0) ? "&" : "?", Server.UrlEncode(_Session("SelectedItemKey")));
            }
            else
                redirUrl = string.Format("Detail.aspx?PageID={0}&NodeID={1}&ThreadID={2}", Server.UrlEncode(_Session("CurPageName")), Request.QueryString["NodeID"], Server.UrlEncode(_Session("SelectedItemKey")));
            if (redirUrl.Length > 0)
                Response.Redirect(redirUrl);

            // */
        }

        protected void ibHist_Click(object sender, ImageClickEventArgs e)
        {
            //InvokeHistoryPage();
        }

        protected void ibMarkAsRead_Click(object sender, ImageClickEventArgs e)
        {
            /*
            DataSet dsWork = new DataSet();
            string error = string.Empty;
            if (GridView_Summary.Rows.Count > 0)
            {
                dsWork = SASWrapper.QueryStoredProc_ResultSet("uspLOG_MarkAllAsRead", new string[] { "@Username" }, new string[] { UserID }, DatabaseName, ref error);
                Cache[ObjectDataSource1.CacheKeyDependency] = new object();
                GridView_Summary.DataBind();
            }

            // */
        }
    }

   
}

